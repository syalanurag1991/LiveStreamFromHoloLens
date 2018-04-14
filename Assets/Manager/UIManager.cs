using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIManager : MonoBehaviour {

	public bool displayCamOnDevice = false;

	// Import Webcam input object
	public GameObject inputDeviceCamera;
	public GameObject webcamRenderQuad;
	private SendDataOverTCP camInputScript;

	public DisplayInfo displayExtraInfo;

	// Use this for initialization
	void Start () {
		camInputScript = inputDeviceCamera.GetComponent<SendDataOverTCP>();
		quadRenderer = webcamRenderQuad.GetComponent<Renderer> ();

		quadRenderer.enabled = displayCamOnDevice;

		// Camera feed parameters
		if (camInputScript.returnTexture == null) {
			Debug.Log ("Camera not started");
			camReady = false;
		}
	}
	
	// Update is called once per frame
	void Update () {

		if (!camReady) {
			if (camInputScript.returnTexture == null) {
				Debug.Log ("Camera not started");
				camReady = false;
			} else {
				Debug.Log ("Camera is Working");
				feedWidth = camInputScript.returnTexture.width;
				feedHeight = camInputScript.returnTexture.height;
				camReady = true;

				if (displayCamOnDevice) {
					SetFeed();
				}
			}
		}
			
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
			quadRenderer.material.mainTexture = camInputScript.returnTexture;
		}
	}

///////////////////////////////////////////////// SET CAMERA FEED  END //////////////////////////////////////////////////////////////

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
