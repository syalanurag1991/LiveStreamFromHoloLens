using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Collections;
using System.Collections.Generic;
#if UNITY_EDITOR || UNITY_STANDALONE_WIN
using System.Net;
using System.Net.Sockets;
#endif
using UnityEngine;
using UnityEngine.UI;

//Source: https://github.com/Microsoft/MixedRealityToolkit-Unity/blob/26e3cc3aa176d1cb06e15dac2214caaa27543607/Assets/HoloToolkit/SpatialMapping/Scripts/SpatialMappingSource.cs

/// <summary>
/// RemoteTextureTarget will listen for Textures being sent from a remote system (HoloLens).
/// It is intended to run in the Unity Editor with exactly one
/// HoloLens device sending data.
/// </summary>
public class RemoteTextureTarget : MonoBehaviour {

	////////////////////////////Anurag Added this////////////////
	//WebCam receiver objects
	public RawImage image;
	Texture2D receivedTexture;

	//Debugging helper
	public bool enableLog = false;
	////////////////////////////Anurag Added this////////////////
	
	[Tooltip("The IPv4 Address of the machine running the Unity editor.")]
	public string ServerIP;

	[Tooltip("The connection port on the machine to use.")]
	public int ConnectionPort = 11000;

	#if UNITY_EDITOR || UNITY_STANDALONE_WIN
	/// <summary>
	/// Listens for network connections over TCP.
	/// </summary> 
	private TcpListener networkListener;

	/// <summary>
	/// Keeps client information when a connection happens.
	/// </summary>
	private TcpClient unityReceiverClient;

	/// <summary>
	/// Tracks if a client is connected.
	/// </summary>
	private bool clientConnected;

	/// <summary>
	/// Reads an int from the next 4 bytes of the supplied stream.
	/// </summary>
	/// <param name="stream">The stream to read the bytes from.</param>
	/// <returns>An integer representing the bytes.</returns>
	private int FindTextureDataSize(Stream stream)
	{
		// The bytes arrive in the wrong order, so swap them.
		byte[] bytes = new byte[4];
		stream.Read(bytes, 0, 4);
		byte t = bytes[0];
		bytes[0] = bytes[3];
		bytes[3] = t;

		t = bytes[1];
		bytes[1] = bytes[2];
		bytes[2] = t;

		// Then bitconverter can read the int32.
		return BitConverter.ToInt32(bytes, 0);
	}

	/// <summary>
	/// Called when a client connects. **This is an override function provided in Unity.Networking
	/// </summary>
	/// <param name="result">The result of the connection.</param>
	private void OnClientConnect(IAsyncResult result)
	{
		if (result.IsCompleted)
		{
			unityReceiverClient = networkListener.EndAcceptTcpClient(result);
			if (unityReceiverClient != null)
			{
				Debug.Log("Connected");
				clientConnected = true;
			}
		}
	}

	private static int HeaderSize = sizeof(int) * 2;

	private void ReadTextureData()
	{
		// Get the clients stream.
		NetworkStream stream = unityReceiverClient.GetStream();

		// The first 4 bytes will be the size of the data containing the image.
		int textureDataSize = FindTextureDataSize(stream);

		// Allocate a buffer to hold the data.  
		byte[] imageBuffer = new byte[textureDataSize];

		// Make sure there is data in the stream.
		if (stream.DataAvailable) {
			// Read the data.
			// The data can come in chunks. 
			int readsize = 0;

			while (readsize != textureDataSize) {
				readsize += stream.Read (imageBuffer, readsize, textureDataSize - readsize);
			}

			if (readsize != textureDataSize) {
				Debug.Log ("reading image failed: " + readsize + " != " + textureDataSize);
			}

			// Finally disconnect.
			clientConnected = false;
			unityReceiverClient.Close ();

			// And wait for the next connection.
			AsyncCallback callback = OnClientConnect;
			networkListener.BeginAcceptTcpClient (callback, this);
		}

		bool readyToReadAgain = false;

		//Display Image on the main Thread
		Loom.QueueOnMainThread(() =>
			{
				displayReceivedImage(imageBuffer);
				readyToReadAgain = true;
			});

		//Wait until old Image is displayed
		while (!readyToReadAgain)
		{
			System.Threading.Thread.Sleep(1);
		}
	}

	void displayReceivedImage(byte[] receivedImageBytes)
	{
		//Debug.Log ("I am here 6");
		receivedTexture.LoadImage(receivedImageBytes);
		image.texture = receivedTexture;
	}

	// Use this for initialization.
	private void Start()
	{
		//////////////////////////// Anurag added this ////////////////
		Application.runInBackground = true;
		//unityReceiverClient = new TcpClient();
		//////////////////////////// Anurag added this ////////////////

		// Setup the network listener.
		IPAddress localAddr = IPAddress.Parse(ServerIP.Trim());
		networkListener = new TcpListener(localAddr, ConnectionPort);
		networkListener.Start();

		// Request the network listener to wait for connections asynchronously.
		AsyncCallback callback = OnClientConnect;
		networkListener.BeginAcceptTcpClient(callback, this);
	}

	/// <summary>
	// Update is called once per frame.
	/// <summary>
	private void Update()
	{
		// If we have a connected client, presumably the client wants to send some Textures.
		if (clientConnected)
		{
			ReadTextureData ();	
		}
	}
		
	#endif
}