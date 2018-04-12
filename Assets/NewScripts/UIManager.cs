using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using CustomStreaming;

public class UIManager : MonoBehaviour {

	public bool displayCamOnDevice = false;

	// Import Webcam input object
	public GameObject inputDeviceCamera;
	public GameObject webcamRenderQuad;
	private CameraInput camInputScript;
	//private CameraInput2 camInputScript;

	// Use this for initialization
	void Start () {
		camInputScript = inputDeviceCamera.GetComponent<CameraInput>();
		quadRenderer = webcamRenderQuad.GetComponent<Renderer> ();

		quadRenderer.enabled = displayCamOnDevice;

		// Camera feed parameters
		if (camInputScript.returnTexture == null) {
			Debug.Log ("Camera not started");
			feedWidth = camInputScript.targetWidth;
			feedHeight = camInputScript.targetHeight;
			camReady = false;
		}
	}
	
	// Update is called once per frame
	void Update () {

		if (!camReady) {
			if (camInputScript.returnTexture == null) {
				Debug.Log ("Camera not started");
				feedWidth = camInputScript.targetWidth;
				feedHeight = camInputScript.targetHeight;
				camReady = false;
			} else {
				Debug.Log ("Camera is Working");
				feedWidth = camInputScript.returnTexture.width;
				feedHeight = camInputScript.returnTexture.height;
				camReady = true;
				SetFeed ();
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
}
