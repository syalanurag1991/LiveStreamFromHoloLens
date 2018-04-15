using UnityEngine;
using UnityEngine.UI;

using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Collections;
using System.Collections.Generic;

#if !UNITY_EDITOR && (UNITY_WSA || NETFX_CORE)
using Windows.Foundation;
using Windows.Networking;
using Windows.Networking.Sockets;
#endif

public class SendDataOverTCP : MonoBehaviour  {

	VideoPanel videoPanelScript;

	//Soure1: https://stackoverflow.com/questions/42717713/unity-live-video-streaming
	//For storing current texture
	//public RawImage myImage;

	////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	void Start()
	{
		Application.runInBackground = true;
		stop = true;

		videoPanelScript = FindObjectOfType<VideoPanel>();

		#if !UNITY_EDITOR && (UNITY_WSA || NETFX_CORE)
		receiverSocket = new StreamSocket();
		#endif
	}

	private void Update()
	{
		if (stop) {
			stop = !videoPanelScript.startSending;
			if(!stop)
				//StartCoroutine(StartConnection());
				InitializeCommunicationOverTCP();
		}

		//ProcessReceivedTextData();
	}

	//Converts integer to byte array
	void StoreIntegerValueToByteArray(int byteLength, byte[] fullBytes)
	{
		//Clear old data
		Array.Clear(fullBytes, 0, fullBytes.Length);
		//Convert int to bytes
		byte[] bytesToSendCount = BitConverter.GetBytes(byteLength);
		//Copy result to fullBytes
		bytesToSendCount.CopyTo(fullBytes, 0);
	}

	public bool enableLog;
	public bool enableLogData = false;
	void LOG(string messsage)
	{
		if (enableLog)
			Debug.Log(messsage);
	}

	void LOGDATA(string messsage)
	{
		if (enableLogData)
			Debug.Log(messsage);
	}

	void LOGWARNING(string messsage)
	{
		if (enableLog)
			Debug.LogWarning(messsage);
	}

	////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	public void InitializeCommunicationOverTCP(){
		Debug.Log("Attempting to connect");
		StartCoroutine(StartConnection());
	}

	//For setting up sending request
	public string receiverIPAddress;
	public int receiverPort = 8010;
	private bool stop = false;
	//This must be the-same with SEND_COUNT on the server
	const int SEND_RECEIVE_COUNT = 15;
	private BinaryWriter binWriter;
	private BinaryReader binReader;

	#if !UNITY_EDITOR && (UNITY_WSA || NETFX_CORE)
	StreamSocket receiverSocket;
	#endif

	bool isConnected = false;
	IEnumerator StartConnection(){
		#if !UNITY_EDITOR && (UNITY_WSA || NETFX_CORE)
		Loom.RunAsync( async () =>
		#else
		Loom.RunAsync(() =>
		#endif
			{
				while(!stop){
					LOGWARNING("Connecting to receiver ...");

					#if !UNITY_EDITOR && (UNITY_WSA || NETFX_CORE)
					HostName receiverHostName = new Windows.Networking.HostName(receiverIPAddress);
					await receiverSocket.ConnectAsync(receiverHostName, receiverPort.ToString());
					Stream streamIn = receiverSocket.InputStream.AsStreamForRead();
					Stream streamOut = receiverSocket.OutputStream.AsStreamForWrite();
					binReader = new BinaryReader(streamIn);
					binWriter = new BinaryWriter(streamOut);
					#endif

					LOGWARNING("Connected with receiver");
					isConnected = true;
				}
			});

		while (!isConnected) {
			yield return null;
		}

		StartCoroutine(SendData());

		//Start receiving data as well
		DataReceiver();
	}

	bool readyToSendNewFrame = true;
	byte[] frameBytesLength = new byte[SEND_RECEIVE_COUNT];
	IEnumerator SendData()
	{
		int totalPacketsSent = 0;
		while (!stop)
		{
			yield return endOfFrame;

			byte[] textureBytes = videoPanelScript.compressedImage;

			//Fill total byte length to send. Result is stored in frameBytesLength
			StoreIntegerValueToByteArray (textureBytes.Length, frameBytesLength);

			//Set readyToSendNewFrame false
			readyToSendNewFrame = false;

			Loom.RunAsync(() =>
				{
					#if !UNITY_EDITOR && (UNITY_WSA || NETFX_CORE)
					//Send total byte count first
					binWriter.Write(frameBytesLength, 0, frameBytesLength.Length);
					LOGDATA("Sent Image byte Length: " + frameBytesLength.Length);

					//Send the image bytes
					binWriter.Write(textureBytes, 0, textureBytes.Length);
					LOGDATA("Sending Image byte array data : " + textureBytes.Length);
					#endif

					//Sent. Set readyToSendNewFrame true
					readyToSendNewFrame = true;
					totalPacketsSent++;
				});

			//Wait until we are ready to get new frame(Until we are done sending data)
			while (!readyToSendNewFrame)
			{
				LOG("Get ready to send new frame");
				yield return null;
			}
		}
	}

	// Stop everything on GameObject destroy
	void OnDestroy()
	{
		#if !UNITY_EDITOR && (UNITY_WSA || NETFX_CORE)
		if (receiverSocket != null)
		receiverSocket.Dispose();

		if(binWriter != null)
		binWriter.Dispose();

		if(binReader != null)
		binReader.Dispose();

		receiverSocket = null;
		#endif
		stop = true;
		LOGWARNING("OnDestroy");
	}

	// Stop everything on Application quit
	private void OnApplicationQuit()
	{
		#if !UNITY_EDITOR && (UNITY_WSA || NETFX_CORE)
		if (receiverSocket != null)
		receiverSocket.Dispose();

		if(binWriter != null)
		binWriter.Dispose();

		if(binReader != null)
		binReader.Dispose();

		receiverSocket = null;
		#endif
		stop = true;
		LOGWARNING("OnApplicationQuit");
	}

	WaitForEndOfFrame endOfFrame = new WaitForEndOfFrame();

	/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	//Receiver
	void DataReceiver()
	{
		Debug.Log ("I am here 14");
		//While loop in another Thread is fine so we don't block main Unity Thread

		#if !UNITY_EDITOR && (UNITY_WSA || NETFX_CORE)
		Loom.RunAsync( async () =>
		#else
		Loom.RunAsync(() =>
		#endif
			{
				Debug.Log ("I am here 15 " + stop.ToString());
				while (!stop)
				{
					//Read Data Size
					int dataSize = ReadDataPacketSize(SEND_RECEIVE_COUNT);
					LOGWARNING("Received Image byte Length: " + dataSize);

					//Read Data Packet
					ReadDataPacket(dataSize);
				}
			});
	}

	//Read incoming header bytes to get size of incoming image
	private int ReadDataPacketSize(int size)
	{
		Debug.Log ("I am here 16");
		bool disconnected = false;

		//NetworkStream serverStream = client.GetStream();
		byte[] dataBytesCount = new byte[size];
		var total = 0;
		do
		{
			Debug.Log("I am here 17");
			var read = binReader.Read(dataBytesCount, total, size - total);
			//Debug.Log("Client recieved " + total.ToString() + " bytes");
			if (read == 0)
			{
				disconnected = true;
				break;
			}
			total += read;
		} while (total != size);

		Debug.Log("I am here 18");

		int byteLength;
		if (disconnected)
		{
			byteLength = -1;
		}
		else
		{
			//byteLength = frameByteArrayToByteLength(imageBytesCount);
			byteLength = BitConverter.ToInt32(dataBytesCount, 0);
		}

		Debug.Log("I am here 19: " + byteLength);

		return byteLength;
	}

	//Read incoming bytes from stream to prepare image for display
	//IEnumerator readDataPacket(int size)
	void ReadDataPacket(int size)
	{
		bool disconnected = false;

		//NetworkStream serverStream = client.GetStream();
		byte[] dataPacket = new byte[size];
		var total = 0;
		do
		{
			var read = binReader.Read(dataPacket, total, size - total);
			//Debug.LogFormat("Client recieved {0} bytes", total);
			if (read == 0)
			{
				disconnected = true;
				break;
			}
			total += read;
		} while (total != size);

		bool readyToReadAgain = false;

		//Display Image
		if (!disconnected)
		{
			StoreReceivedTextDataPackets(dataPacket);
			//ProcessReceivedTextData(dataPacket);
			//ProcessReceivedData(dataPacket);
			readyToReadAgain = true;
		}

		//Wait until old Image is displayed
		while (!readyToReadAgain)
		{
			#if !UNITY_EDITOR && (UNITY_WSA || NETFX_CORE)
			WaitForSomeTime(0.001);
			#else
			System.Threading.Thread.Sleep(1);
			#endif
		}

		//yield return null;
	}

	#if !UNITY_EDITOR && (UNITY_WSA || NETFX_CORE)
	private async void WaitForSomeTime(double timeInSeconds)
	{
		await System.Threading.Tasks.Task.Delay(TimeSpan.FromSeconds(timeInSeconds));
	}
	#endif

	public int recevingBufferSize;
	[HideInInspector]
	public Queue<byte[]> queueOfReceivedDataPackets = new Queue<byte[]>();
	public byte[] latestByte;
	void StoreReceivedTextDataPackets(byte[] receivedDataPacket) {

		latestByte = receivedDataPacket;

		//if (queueOfReceivedDataPackets.Count < recevingBufferSize) {
		//	queueOfReceivedDataPackets.Enqueue (receivedDataPacket);
		//} 

		//else {
		//	queueOfReceivedDataPackets.Clear();
		//}
			
	}

}