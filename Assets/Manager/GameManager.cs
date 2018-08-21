using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour {

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
				tcpNetworkingScript.SetNetworkToActive();
				tcpNetworkingScript.InitializeCommunicationOverTCP();
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

	int PacketDataSize = 4;
	private Vector3 moodTrackerCoordinates;
	void ProcessReceivedMoodTrackerData()
	{
		byte[] dataToBeProcessed = tcpNetworkingScript.latestByte;

		if (dataToBeProcessed.Length == 0)
			return;

		byte[] xPositionByte = new byte[PacketDataSize];
		byte[] yPositionByte = new byte[PacketDataSize];
		byte[] zPositionByte = new byte[PacketDataSize];

		byte[] emotionIndexByte = new byte[PacketDataSize];
		byte[] joyByte = new byte[PacketDataSize];
		byte[] fearByte = new byte[PacketDataSize];
		byte[] disgustByte = new byte[PacketDataSize];
		byte[] sadnessByte = new byte[PacketDataSize];
		byte[] angerByte = new byte[PacketDataSize];
		byte[] surpriseByte = new byte[PacketDataSize];

		//Array.Reverse(dataToBeProcessed);

		Buffer.BlockCopy(dataToBeProcessed, 0, xPositionByte, 0, PacketDataSize);
		Buffer.BlockCopy(dataToBeProcessed, PacketDataSize, yPositionByte, 0, PacketDataSize);
		Buffer.BlockCopy(dataToBeProcessed, 2*PacketDataSize, zPositionByte, 0, PacketDataSize);

		Buffer.BlockCopy(dataToBeProcessed, 3*PacketDataSize, emotionIndexByte, 0, PacketDataSize);
		Buffer.BlockCopy(dataToBeProcessed, 4*PacketDataSize, joyByte, 0, PacketDataSize);
		Buffer.BlockCopy(dataToBeProcessed, 5*PacketDataSize, fearByte, 0, PacketDataSize);
		Buffer.BlockCopy(dataToBeProcessed, 6*PacketDataSize, disgustByte, 0, PacketDataSize);
		Buffer.BlockCopy(dataToBeProcessed, 7*PacketDataSize, sadnessByte, 0, PacketDataSize);
		Buffer.BlockCopy(dataToBeProcessed, 8*PacketDataSize, angerByte, 0, PacketDataSize);
		Buffer.BlockCopy(dataToBeProcessed, 9*PacketDataSize, surpriseByte, 0, PacketDataSize);

		int xPosition = BitConverter.ToInt32(xPositionByte, 0);
		int yPosition = BitConverter.ToInt32(yPositionByte, 0);
		int zPosition = BitConverter.ToInt32(zPositionByte, 0);
		int emotionIndex = BitConverter.ToInt32(emotionIndexByte, 0);

		float[] emotionData = new float[6];
		emotionData [0] = BitConverter.ToSingle(joyByte, 0);
		emotionData [1] = BitConverter.ToSingle(fearByte, 0);
		emotionData [2] = BitConverter.ToSingle(disgustByte, 0);
		emotionData [3] = BitConverter.ToSingle(sadnessByte, 0);
		emotionData [4] = BitConverter.ToSingle(angerByte, 0);
		emotionData [5] = BitConverter.ToSingle(surpriseByte, 0);
		SetEmotionStructFromRecievedData (emotionData);

		//SetMoodTrackerGeometry(moodTrackerCoordinates);
		SetMoodTrackerColor(emotionIndex);

		string receivedMessage = "x = " + xPosition + " y = " + yPosition + " z = " + zPosition + " emotion = " + emotionIndex;
		//Debug.Log(receivedMessage);
	}

	public EmotionStruct currentFacialEmotion = new EmotionStruct();
	private void SetEmotionStructFromRecievedData(float[] emotionData){
		currentFacialEmotion.joy = emotionData[0];
		currentFacialEmotion.fear = emotionData[1];
		currentFacialEmotion.disgust = emotionData[2];
		currentFacialEmotion.sadness = emotionData[3];
		currentFacialEmotion.anger = emotionData[4];
		currentFacialEmotion.surprise = emotionData[5];
		//Debug.Log ("Current Joy: " + currentFacialEmotion.joy + " | anger: " + currentFacialEmotion.anger + " | sadness: " + currentFacialEmotion.sadness + " | fear: " + currentFacialEmotion.fear);
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
