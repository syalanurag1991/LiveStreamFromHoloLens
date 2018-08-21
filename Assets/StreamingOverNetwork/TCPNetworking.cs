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
#else
using System.Net;
using System.Threading;
using System.Net.Sockets;
#endif

public class TCPNetworking : MonoBehaviour  {

	//VideoPanel videoPanelScript;

	// Soure1: https://stackoverflow.com/questions/42717713/unity-live-video-streaming
	void Start()
	{
		Application.runInBackground = true;
		stopCommunication = true;
		//startConnection = StartConnection();

		#if !UNITY_EDITOR && (UNITY_WSA || NETFX_CORE)
		receiverSocket = new StreamSocket();
		#else
		receiverClient = new TcpClient();
		#endif
	}

	float timeCounter = 0.0f;
	int currentNumberOfPacketsReceived = 0;
	int previousNumberOfPacketsReceived = -1;
	float refreshTime = 5.0f;
	void Update()
	{
		//Debug.Log("Number of packets recieved: " + currentNumberOfPacketsReceived + " " + previousNumberOfPacketsReceived);

		if (timeCounter < refreshTime) {
			timeCounter += Time.deltaTime;
		} else {
			
			if (isConnected) {
				previousNumberOfPacketsReceived = currentNumberOfPacketsReceived;
			} 

//			else {
//				Debug.Log("Killing connection");
//				KillConnection();
//
//				//startConnection = StartConnection();
//
//				Debug.Log("Starting Connection");
//				InitializeCommunicationOverTCP();
//			}

			currentNumberOfPacketsReceived = 0;
			timeCounter = 0;

		}

		// Bring to initial state when connectivity is lost
		// So that client can connect again
//		if (previousNumberOfPacketsReceived == 0 && currentNumberOfPacketsReceived == 0) {
//			Debug.Log("No connection");
//			if (isConnected) {
//				Debug.Log("Killing connection");
//				KillConnection();
//			}
//		}
	}

	// Converts integer to byte array
	void StoreIntegerValueToByteArray(int byteLength, byte[] fullBytes)
	{
		// Clear old data
		Array.Clear(fullBytes, 0, fullBytes.Length);

		// Convert int to bytes
		byte[] bytesToSendCount = BitConverter.GetBytes(byteLength);

		// Copy result to fullBytes
		bytesToSendCount.CopyTo(fullBytes, 0);
	}

	////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	// Start connection over TCP
	private IEnumerator startConnection = null;
	int instanceNumber = 0;
	public void InitializeCommunicationOverTCP(){

		instanceNumber++;
		LOGWARNING("Attempting to connect");

		if (startConnection != null)
			StopCoroutine(startConnection);

		startConnection = StartConnection();
		StartCoroutine(startConnection);
	}

	public bool GetNetworkIsNotActive(){
		return stopCommunication;
	}

	public void SetNetworkToActive(){
		stopCommunication = false;
	}

	// For setting up sending request
	public string receiverIPAddress;
	public int receiverPort = 8010;

	// This must be the-same with SEND_COUNT on the server
	const int SEND_RECEIVE_COUNT = 15;

	private bool stopCommunication;
	private BinaryWriter binWriter = null;
	private BinaryReader binReader = null;

	#if !UNITY_EDITOR && (UNITY_WSA || NETFX_CORE)
	StreamSocket receiverSocket;
	#else
	TcpClient receiverClient;
	NetworkStream receiverClientStream = null;
	#endif

	bool isConnected = false;
	IEnumerator StartConnection(){
		LOGWARNING("Really trying to connect");
		#if !UNITY_EDITOR && (UNITY_WSA || NETFX_CORE)
		Loom.RunAsync( async () =>
		#else
		Loom.RunAsync(() =>
		#endif
			{
				while(!stopCommunication){
					LOGWARNING("Connecting to receiver ...");

					#if !UNITY_EDITOR && (UNITY_WSA || NETFX_CORE)
					HostName receiverHostName = new Windows.Networking.HostName(receiverIPAddress);
					await receiverSocket.ConnectAsync(receiverHostName, receiverPort.ToString());
					Stream streamIn = receiverSocket.InputStream.AsStreamForRead();
					Stream streamOut = receiverSocket.OutputStream.AsStreamForWrite();
					binReader = new BinaryReader(streamIn);
					binWriter = new BinaryWriter(streamOut);
					#else
					receiverClient.Connect(IPAddress.Parse(receiverIPAddress), receiverPort);
					receiverClientStream = receiverClient.GetStream();
					binReader = new BinaryReader(receiverClientStream);
					binWriter = new BinaryWriter(receiverClientStream);
					#endif

					LOGWARNING("Connected with receiver");
					isConnected = true;
				}
			});

		while (!isConnected) {
			Debug.Log("I am here " + instanceNumber);
			yield return null;
		}

		// Start receiving data as well
		ReceiveData();
	}

	////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	// Send Data over TCP connection
	byte[] dataPacketSize = new byte[SEND_RECEIVE_COUNT];
	byte[] dataPacketBytes;
	public void UpdateDataToSend(byte[] newData){
		dataPacketBytes = newData;
		SendData();
	}

	int totalPacketsSent = 0;
	void SendData()
	{
		// Fill total byte length to send. Result is stored in dataPacketSize
		StoreIntegerValueToByteArray (dataPacketBytes.Length, dataPacketSize);

		Loom.RunAsync(() =>
			{
				// Send total byte count first
				binWriter.Write(dataPacketSize, 0, dataPacketSize.Length);
				LOGDATA("Sent Image byte Length: " + dataPacketSize.Length);

				// Send the image bytes
				binWriter.Write(dataPacketBytes, 0, dataPacketBytes.Length);
				LOGDATA("Sending Image byte array data : " + dataPacketBytes.Length);

				totalPacketsSent++;
			});
	}

	// To stop sending/receiving on purpose
	private void KillConnection(){

		#if !UNITY_EDITOR && (UNITY_WSA || NETFX_CORE)
		if (receiverSocket != null)
		receiverSocket.Dispose();

		if(binWriter != null)
		binWriter.Dispose();

		if(binReader != null)
		binReader.Dispose();

		receiverSocket = null;
		#else
		if (receiverClient != null)
			receiverClient.Close();

		if(binWriter != null)
			binWriter.Close();

		if(binReader != null)
			binReader.Close();

		if(receiverClientStream != null)
			receiverClientStream.Close();

		receiverClient = null;
		receiverClientStream = null;
		#endif

		binWriter = null;
		binReader = null;
		isConnected = false;
		//stopCommunication = true;
		previousNumberOfPacketsReceived = -1;
		LOGWARNING("Connection killed");
	}

	// Stop everything on GameObject destroy
	void OnDestroy()
	{
		KillConnection();
		LOGWARNING("OnDestroy");
	}

	// Stop everything on Application quit
	private void OnApplicationQuit()
	{
		KillConnection();
		LOGWARNING("OnApplicationQuit");
	}

	////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	// Receive data over TCP connection
	void ReceiveData()
	{
		LOGDATA("All set to receive data");
		// While loop in another Thread is fine so we don't block main Unity Thread

		#if !UNITY_EDITOR && (UNITY_WSA || NETFX_CORE)
		Loom.RunAsync( async () =>
		#else
		Loom.RunAsync(() =>
		#endif
			{
				LOGDATA("Enabled to receive: " + (!stopCommunication).ToString());
				while (!stopCommunication)
				{
					// Read Data Size
					int dataSize = ReadDataPacketSize(SEND_RECEIVE_COUNT);
					LOGWARNING("Received datat-packet size: " + dataSize);

					// Read Data Packet
					ReadDataPacket(dataSize);

					currentNumberOfPacketsReceived++;
				}
			});
	}

	// Read incoming header bytes to get size of incoming image
	private int ReadDataPacketSize(int size)
	{
		LOGDATA("All set to read data-packet size");
		bool disconnected = false;

		byte[] dataBytesCount = new byte[size];
		var total = 0;
		do
		{
			LOGDATA("Reading incoming data-packet size");
			var read = binReader.Read(dataBytesCount, total, size - total);
			if (read == 0)
			{
				disconnected = true;
				break;
			}
			total += read;
		} while (total != size);

		LOGDATA("Finished reading incoming data-packet size");

		int byteLength;
		if (disconnected)
		{
			byteLength = -1;
		}
		else
		{
			byteLength = BitConverter.ToInt32(dataBytesCount, 0);
		}

		LOGDATA("Data-packet size: " + byteLength);
		return byteLength;
	}

	// Read incoming bytes from stream to prepare image for display
	void ReadDataPacket(int size)
	{
		LOGDATA("All set to read data-packet");

		bool disconnected = false;
		byte[] dataPacket = new byte[size];
		var total = 0;

		do
		{
			LOGDATA("Reading incoming data-packet");
			var read = binReader.Read(dataPacket, total, size - total);
			if (read == 0)
			{
				disconnected = true;
				break;
			}
			total += read;
		} while (total != size);

		bool readyToReadAgain = false;

		// Display Image
		if (!disconnected)
		{
			StoreReceivedTextDataPackets(dataPacket);
			readyToReadAgain = true;
		}

		// Wait until old Image is displayed
		while (!readyToReadAgain)
		{
			#if !UNITY_EDITOR && (UNITY_WSA || NETFX_CORE)
			WaitForSomeTime(0.001);
			#else
			System.Threading.Thread.Sleep(1);
			#endif
		}

		LOGDATA("Finished reading incoming data-packet");
	}

	#if !UNITY_EDITOR && (UNITY_WSA || NETFX_CORE)
	private async void WaitForSomeTime(double timeInSeconds)
	{
		await System.Threading.Tasks.Task.Delay(TimeSpan.FromSeconds(timeInSeconds));
	}
	#endif

	// Store received data in a queue or a byte
	public int recevingBufferSize;
	[HideInInspector]
	public Queue<byte[]> queueOfReceivedDataPackets = new Queue<byte[]>();
	[HideInInspector]
	public byte[] latestByte;
	void StoreReceivedTextDataPackets(byte[] receivedDataPacket) {
		latestByte = receivedDataPacket;
	}
		
	////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	// Enable logging on console
	public bool enableLog = false;
	public bool enableLogData = false;
	public bool enableLogWarning = false;
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
		if (enableLogWarning)
			Debug.LogWarning(messsage);
	}
}