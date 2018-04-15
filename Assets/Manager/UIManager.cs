using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIManager : MonoBehaviour {

	public bool displayCamOnDevice = false;

	// Import Webcam input object
	public GameObject webcamRenderQuad;
	public DisplayInfo displayExtraInfo;
	public SendDataOverTCP sendDataScript;
	public VideoPanel videoPanelScript;

	// Use this for initialization
	void Start () {
		quadRenderer = webcamRenderQuad.GetComponent<Renderer>();
		quadRenderer.enabled = displayCamOnDevice;

		// Camera feed parameters
		if (videoPanelScript.returnTexture == null) {
			Debug.Log ("Camera not started");
			camReady = false;
		}
	}
	
	// Update is called once per frame
	void Update () {

		if (!camReady) {
			if (videoPanelScript.returnTexture == null) {
				Debug.Log ("Camera not started");
				camReady = false;
			} else {
				Debug.Log ("Camera is Working");
				feedWidth = videoPanelScript.returnTexture.width;
				feedHeight = videoPanelScript.returnTexture.height;
				camReady = true;

				if (displayCamOnDevice) {
					SetFeed();
				}
			}
		}

		//ProcessReceivedTextData();
		ProcessReceivedMoodTrackerData();
	}
		
//////////////////////////////////////////////// SET CAMERA FEED  START /////////////////////////////////////////////////////////////
	// Configure Webcam output object
	[Space(10)]
	public float displayHeight = 0.54f;
	public bool flipHorizontal = false;
	public bool flipVertical = true;

	private Renderer quadRenderer;
	private float feedWidth;
	private float feedHeight;
	private bool camReady;

	public void SetFeed (){

		float flipDisplayX = flipHorizontal ? 1f : -1f;
		float flipDisplayY = flipVertical ? 1f : -1f;

		// Set the webcam-Render-Quad to have the same aspect ratio as the video feed
		float aspectRatio = feedWidth / feedHeight;

		if (camReady && displayCamOnDevice)
		{
			
			webcamRenderQuad.transform.localScale = new Vector3 (-10 * flipDisplayX * aspectRatio * displayHeight, -10 * flipDisplayY * displayHeight, 1.0f);
			Debug.Log ("Feed Width: " + feedWidth + " Feed Height: " + feedHeight + " Aspect Ratio: " + aspectRatio);

			//New code
			//For setting up Cam Quad Display
			Texture2D targetTexture = new Texture2D ((int)feedWidth, (int)feedHeight, TextureFormat.BGRA32, false);
			quadRenderer.material.mainTexture = targetTexture;
			quadRenderer.material.mainTexture = videoPanelScript.returnTexture;
		}
	}

///////////////////////////////////////////////// SET CAMERA FEED  END //////////////////////////////////////////////////////////////

	void ProcessReceivedTextData()
	{
		byte[] dataToBeProcessed;
		if (!(sendDataScript.queueOfReceivedDataPackets.Count > 0)) {
			return;
		}

		dataToBeProcessed = sendDataScript.queueOfReceivedDataPackets.Dequeue();
		string dataText = System.Text.Encoding.UTF8.GetString(dataToBeProcessed);
		Debug.Log(dataText);
	}

	private Vector3 moodTrackerCoordinates;
	void ProcessReceivedMoodTrackerData()
	{
//		byte[] dataToBeProcessed;
//		if (!(sendDataScript.queueOfReceivedDataPackets.Count > 0)) {
//			return;
//		}
//
//		dataToBeProcessed = sendDataScript.queueOfReceivedDataPackets.Dequeue();

		byte[] dataToBeProcessed = sendDataScript.latestByte;

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

	/////////////////////////////////////////// SET MOOD TRACKER ATTRIBUTES  START //////////////////////////////////////////////////////
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
			moodTrackerColor = Color.yellow; //disgust
		else if (emotionIndex == 4)
			moodTrackerColor = Color.blue; //sadness
		else if (emotionIndex == 5)
			moodTrackerColor = Color.red; //anger
		else if (emotionIndex == 6)
			moodTrackerColor = Color.white; //surprise
		else
			moodTrackerColor = Color.black; //neutral

		return moodTrackerColor;
	}

	public Color GetMoodTrackerColor(){
		return moodTrackerColor;
	}

	/////////////////////////////////////////// SET MOOD TRACKER ATTRIBUTES  END ////////////////////////////////////////////////////////
}
