using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIManager : MonoBehaviour {

	// Import Webcam input object
	public DisplayInfo displayExtraInfo;
	public TCPNetworking tcpNetworkingScript;
	public VideoPanel videoPanelScript;

	// For checking if camera has started
	[Space(10)]
	private bool camReady;

	// Use this for initialization
	void Start () {

	}
	
	// Update is called once per frame
	void Update () {

		if (!camReady) {
			if (videoPanelScript.GetCameraStatus()) {
				Debug.Log ("Camera is Working");
				camReady = true;
			} else {
				Debug.Log ("Camera not started");
				camReady = false;
			}
		}

		if (tcpNetworkingScript.GetNetworkIsNotActive()) {
			if(videoPanelScript.startSending){
				tcpNetworkingScript.SetNetworkIsNotActive();
				tcpNetworkingScript.InitializeCommunicationOverTCP ();
			}
		}

		//ProcessReceivedTextData();
		ProcessReceivedMoodTrackerData();
	}
		
	/////////////////////////////////////////// SET MOOD TRACKER ATTRIBUTES  START //////////////////////////////////////////////////////
	public void PrepareToSend(byte[] newData){
		tcpNetworkingScript.UpdateDataToSend(newData);
	}

	void ProcessReceivedTextData()
	{
		byte[] dataToBeProcessed;
		if (!(tcpNetworkingScript.queueOfReceivedDataPackets.Count > 0)) {
			return;
		}

		dataToBeProcessed = tcpNetworkingScript.queueOfReceivedDataPackets.Dequeue();
		string dataText = System.Text.Encoding.UTF8.GetString(dataToBeProcessed);
		Debug.Log(dataText);
	}

	private Vector3 moodTrackerCoordinates;
	void ProcessReceivedMoodTrackerData()
	{
		byte[] dataToBeProcessed = tcpNetworkingScript.latestByte;

		if (dataToBeProcessed.Length == 0)
			return;

		int PacketDataSize = dataToBeProcessed.Length / 4;
		byte[] xPositionByte = new byte[PacketDataSize];
		byte[] yPositionByte = new byte[PacketDataSize];
		byte[] zPositionByte = new byte[PacketDataSize];
		byte[] emotionIndexByte = new byte[PacketDataSize];

		//Array.Reverse(dataToBeProcessed);

		Buffer.BlockCopy(dataToBeProcessed, 0, xPositionByte, 0, PacketDataSize);
		Buffer.BlockCopy(dataToBeProcessed, PacketDataSize, yPositionByte, 0, PacketDataSize);
		Buffer.BlockCopy(dataToBeProcessed, 2*PacketDataSize, zPositionByte, 0, PacketDataSize);
		Buffer.BlockCopy(dataToBeProcessed, 3*PacketDataSize, emotionIndexByte, 0, PacketDataSize);

		int xPosition = BitConverter.ToInt32(xPositionByte, 0);
		int yPosition = BitConverter.ToInt32(yPositionByte, 0);
		int zPosition = BitConverter.ToInt32(zPositionByte, 0);
		int emotionIndex = BitConverter.ToInt32(emotionIndexByte, 0);

		moodTrackerCoordinates.x = xPosition / 100.0f;
		moodTrackerCoordinates.y = yPosition / 100.0f;
		moodTrackerCoordinates.z = zPosition / 100.0f;

		//SetMoodTrackerGeometry(moodTrackerCoordinates);
		SetMoodTrackerColor(emotionIndex);

		string receivedMessage = "x = " + xPosition + " y = " + yPosition + " z = " + zPosition + " emotion = " + emotionIndex;
		Debug.Log(receivedMessage);
	}

	[HideInInspector] public Vector3 normalizedMoodTrackerCoordinates;
	[HideInInspector] public Vector3 moodTrackerSize;
	private Color moodTrackerColor;
	private int indexOfEmotion;
	public void SetMoodTrackerGeometry(Vector3 moodTrackerCoordinates){
		normalizedMoodTrackerCoordinates.x = moodTrackerCoordinates.x;
		normalizedMoodTrackerCoordinates.y = moodTrackerCoordinates.y;
		normalizedMoodTrackerCoordinates.z = moodTrackerCoordinates.z;

		int xPosition = (int) normalizedMoodTrackerCoordinates.x;
		int yPosition = (int) normalizedMoodTrackerCoordinates.y;
		int zPosition = (int) normalizedMoodTrackerCoordinates.z;

		string receivedMessage = "x = " + xPosition + " y = " + yPosition + "\nz = " + zPosition;
		displayExtraInfo.ClearAndSetDisplayText(receivedMessage);
	}

	public Color SetMoodTrackerColor(int emotionIndex){

		if (emotionIndex == 1)
			moodTrackerColor = Color.green; //joy
		else if (emotionIndex == 2)
			moodTrackerColor = Color.magenta; //fear
		else if (emotionIndex == 3)
			moodTrackerColor = Color.yellow; //disgust //excluded
		else if (emotionIndex == 4)
			moodTrackerColor = Color.blue; //sadness
		else if (emotionIndex == 5)
			moodTrackerColor = Color.red; //anger
		else if (emotionIndex == 6)
			moodTrackerColor = Color.white; //surprise //excluded
		else
			moodTrackerColor = Color.black; //neutral

		return moodTrackerColor;
	}

	public Color GetMoodTrackerColor(){
		return moodTrackerColor;
	}

	/////////////////////////////////////////// SET MOOD TRACKER ATTRIBUTES  END ////////////////////////////////////////////////////////
}
