using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using CustomStreaming;

public class DisplayWebCamOutput : MonoBehaviour {

	private CameraInput camInputScript;
	private WebCamTexture defaultCamTexture;

	private Renderer planeRenderer;
	public bool flipVertical;

	// Use this for initialization
	void Start () {
		camInputScript = FindObjectOfType<CameraInput> ();
		planeRenderer = gameObject.GetComponent<Renderer>();
		
		// Set the cameraDisplayPlane to have the same aspect ratio as the video feed
		float aspectRatio = camInputScript.targetWidth / (float) camInputScript.targetHeight;
		Debug.Log (aspectRatio);
		float zScale = 0.5625f;

		float flip = flipVertical ? 1f : -1f;
		gameObject.transform.localScale = new Vector3 (flip*aspectRatio*zScale, 1.0f, -1.0f*zScale);
		defaultCamTexture = camInputScript.Texture as WebCamTexture;
	}
	
	// Update is called once per frame
	void Update () {
		planeRenderer.material.mainTexture = defaultCamTexture;
	}
}
