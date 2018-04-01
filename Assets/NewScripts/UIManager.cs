using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using CustomStreaming;

public class UIManager : MonoBehaviour {

	// Import Webcam input object
	public Camera mainCamera;
	public GameObject inputDeviceCamera;
	public GameObject webcamRenderQuad;
	private CameraInput camInputScript;

	// Use this for initialization
	void Start () {
		camInputScript = inputDeviceCamera.GetComponent<CameraInput>();
		quadRenderer = webcamRenderQuad.GetComponent<Renderer> ();

		// Camera feed parameters
		if (camInputScript.Texture == null) {
			Debug.Log ("Camera not started");
			feedWidth = camInputScript.targetWidth;
			feedHeight = camInputScript.targetHeight;
			camReady = false;
		}
			
		SetFeed ();

		//Apply webcam texture to quad gameobject
		quadRenderer.material.mainTexture = camInputScript.Texture;
	}
	
	// Update is called once per frame
	void Update () {
		
		// Display the webcam input
		quadRenderer.material.mainTexture = camInputScript.Texture;
			
		if (!camReady) {
			if (camInputScript.Texture == null) {
				Debug.Log ("Camera not started");
				feedWidth = camInputScript.targetWidth;
				feedHeight = camInputScript.targetHeight;
				camReady = false;
			} else {
				Debug.Log ("Camera is Working");
				feedWidth = camInputScript.Texture.width;
				feedHeight = camInputScript.Texture.height;
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

		if (camReady)
		{

			webcamRenderQuad.transform.localScale = new Vector3 (-10 * flipDisplayX * aspectRatio * displayHeight, -10 * flipDisplayY * displayHeight, 1.0f);
			Debug.Log ("Feed Width: " + feedWidth + " Feed Height: " + feedHeight + " Aspect Ratio: " + aspectRatio);

			//New code
			//For setting up Cam Quad Display
			Texture2D targetTexture = new Texture2D ((int)feedWidth, (int)feedHeight, TextureFormat.BGRA32, false);
			quadRenderer.material.mainTexture = targetTexture;

		}
	}

///////////////////////////////////////////////// SET CAMERA FEED  END //////////////////////////////////////////////////////////////
}
