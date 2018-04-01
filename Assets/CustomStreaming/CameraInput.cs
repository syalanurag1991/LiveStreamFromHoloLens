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


using CustomStreaming;

public class CameraInput : MonoBehaviour  {

	public bool enableLog;
	public bool enableLogData = false;

	/// Number of frames per second to sample.  Use 0 and call ProcessFrame() manually to run manually.
	/// Enable/Disable to start/stop the sampling
	public float sampleRate = 20;

	/// Should the selected camera be front facing?
	public bool isFrontFacing = true;

	/// Desired resolution width for capture
	public int targetWidth = 640;
	public int targetHeight = 480;

	/// List of WebCams accessible to Unity
	protected WebCamDevice[] devices;

	/// WebCam chosen to gather metrics from
	protected WebCamDevice device;

	/// Web Cam texture
	private WebCamTexture webCamTexture;

	// Get camera texture
	public Texture Texture
	{
		get
		{
			return webCamTexture;
		}
	}

	/// The rotation of the camera
	public float videoRotationAngle
	{
		get
		{
			return webCamTexture.videoRotationAngle;
		}
	}

	//Soure1: https://stackoverflow.com/questions/42717713/unity-live-video-streaming
	//For storing current texture
	//public RawImage myImage;

	////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	void Start()
	{
		//from Soure1
		Application.runInBackground = true;

#if !UNITY_EDITOR && (UNITY_WSA || NETFX_CORE)
		receiverSocket = new StreamSocket();
		//receiverClient = new StreamSocket();
#else
		receiverClient = new TcpClient();
#endif

		StartCoroutine(initAndWaitForWebCamTexture());
	}

	private void Update()
	{

	}
	////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
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
	//from Source1
	bool camReady = false;
	Texture2D currentTexture;
	IEnumerator initAndWaitForWebCamTexture()
	{
		if (!camReady) {
			// Get all devices , front and back camera
			devices = WebCamTexture.devices;

			if (devices.Length > 0) {
				SelectCamera (isFrontFacing);

				if (device.name != "Null") {
					webCamTexture = new WebCamTexture (device.name, targetWidth, targetHeight, (int)sampleRate);
					webCamTexture.Play ();
				}
			}

			//This will prevent coroutine to go ahead before camera is intialized
			while (webCamTexture.width < 100) {
				yield return null;
			}

			if (webCamTexture.width > 100) {
				camReady = true;
				currentTexture = new Texture2D (webCamTexture.width, webCamTexture.height);
			}
		}

		//yield return null;
		StartCoroutine(SendData());
	}

	/// Set the target device (by name or orientation)
	/// <param name="isFrontFacing">Should the device be forward facing?</param>
	/// <param name="name">The name of the webcam to select.</param>
	public void SelectCamera(bool isFrontFacing, string name = "")
	{
		foreach (WebCamDevice d in devices)
		{
			if (d.name.Length > 1 && d.name == name)
			{
				webCamTexture.Stop();
				device = d;

				webCamTexture = new WebCamTexture(device.name, targetWidth, targetHeight, (int)sampleRate);
				webCamTexture.Play();
			}
			else if (d.isFrontFacing == isFrontFacing)
			{
				device = d;
			}
		}
	}

	////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	//For setting up sending request
	public string receiverIPAddress;
	public int receiverPort = 8010;

#if !UNITY_EDITOR && (UNITY_WSA || NETFX_CORE)
	StreamSocket receiverClient;
	StreamSocket receiverSocket;
#else
	TcpClient receiverClient;
	NetworkStream receiverClientStream = null;
#endif
	private bool connectionReady = false;
	private bool stop = false;
	//This must be the-same with SEND_COUNT on the server
	const int SEND_RECEIVE_COUNT = 15;
	private BinaryWriter binWriter;
	private BinaryReader binReader;

	IEnumerator SendData()
	{
		bool isConnected = false;
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
					//reader = new StreamReader(streamIn);
					//writer = new StreamWriter(streamOut) { AutoFlush = true };
					binReader = new BinaryReader(streamIn);
					binWriter = new BinaryWriter(streamOut);
#else
					receiverClient.Connect(IPAddress.Parse(receiverIPAddress), receiverPort);
					receiverClientStream = receiverClient.GetStream();
					//reader = new StreamReader(receiverClientStream);
					//writer = new StreamWriter(receiverClientStream);
					binReader = new BinaryReader(receiverClientStream);
					binWriter = new BinaryWriter(receiverClientStream);
#endif
					LOGWARNING("Connected with receiver");
					connectionReady = true;
					isConnected = true;
				}
			});

		while (!isConnected) {
			yield return null;
		}

		bool readyToSendNewFrame = true;
		byte[] frameBytesLength = new byte[SEND_RECEIVE_COUNT];

		while (!stop)
		{
			yield return endOfFrame;

			currentTexture.SetPixels (webCamTexture.GetPixels ());
			//			byte[] textureBytes = currentTexture.EncodeToPNG ();
			//			byte[] textureBytes = currentTexture.GetRawTextureData();
			byte[] textureBytes = currentTexture.EncodeToJPG();
			//Fill total byte length to send. Result is stored in frameBytesLength
			byteLengthToFrameByteArray (textureBytes.Length, frameBytesLength);

			//Set readyToSendNewFrame false
			readyToSendNewFrame = false;

			Loom.RunAsync(() =>
				{
					//Send total byte count first
					//receiverClientStream.Write(frameBytesLength, 0, frameBytesLength.Length);
					binWriter.Write(frameBytesLength, 0, frameBytesLength.Length);
					LOGDATA("Sent Image byte Length: " + frameBytesLength.Length);

					//Send the image bytes
					//receiverClientStream.Write(textureBytes, 0, textureBytes.Length);
					binWriter.Write(textureBytes, 0, textureBytes.Length);
					LOGDATA("Sending Image byte array data : " + textureBytes.Length);

					//Sent. Set readyToSendNewFrame true
					readyToSendNewFrame = true;
				});

			//Wait until we are ready to get new frame(Until we are done sending data)
			while (!readyToSendNewFrame)
			{
				LOG("Get ready to send new frame");
				yield return null;
			}
		}
	}

	////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	//Receiving server must know amount of data to receive
	//Read a byte array to find it's size
	int frameByteArrayToByteLength(byte[] frameBytesLength)
	{
		int byteLength = BitConverter.ToInt32(frameBytesLength, 0);
		return byteLength;
	}

	//Converts integer to byte array
	void byteLengthToFrameByteArray(int byteLength, byte[] fullBytes)
	{
		//Clear old data
		Array.Clear(fullBytes, 0, fullBytes.Length);
		//Convert int to bytes
		byte[] bytesToSendCount = BitConverter.GetBytes(byteLength);
		//Copy result to fullBytes
		bytesToSendCount.CopyTo(fullBytes, 0);
	}

	////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	// Stop everything on GameObject destroy
	void OnDestroy()
	{
		if (webCamTexture != null && webCamTexture.isPlaying)
			webCamTexture.Stop();

		#if !UNITY_EDITOR && (UNITY_WSA || NETFX_CORE)
		receiverSocket.Dispose();
		receiverClient.Dispose();
		binWriter.Dispose();
		binReader.Dispose();

		receiverSocket = null;
		receiverClient = null;
		#else
		if (receiverClient != null)
			receiverClient.Close();

		binWriter.Close();
		binReader.Close();
		receiverClientStream = null;
		#endif

		stop = true;
		connectionReady = false;
		camReady = false;
		LOGWARNING("OnDestroy");
	}

	// Stop everything on Application quit
	private void OnApplicationQuit()
	{
		if (webCamTexture != null && webCamTexture.isPlaying)
			webCamTexture.Stop();

#if !UNITY_EDITOR && (UNITY_WSA || NETFX_CORE)
		receiverSocket.Dispose();
		receiverClient.Dispose();
		binWriter.Dispose();
		binReader.Dispose();

		receiverSocket = null;
		receiverClient = null;
#else
		if (receiverClient != null)
			receiverClient.Close();

		binWriter.Close();
		binReader.Close();
		receiverClientStream = null;
#endif

		stop = true;
		connectionReady = false;
		camReady = false;
		LOGWARNING("OnApplicationQuit");
	}

	WaitForEndOfFrame endOfFrame = new WaitForEndOfFrame();
}
