//  
// Copyright (c) 2017 Vulcan, Inc. All rights reserved.  
// Licensed under the Apache 2.0 license. See LICENSE file in the project root for full license information.
//

using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;
using System.Collections.Generic;

using HoloLensCameraStream;

/// <summary>
/// This example gets the video frames at 30 fps and displays them on a Unity texture,
/// which is locked the User's gaze.
/// </summary>
public class VideoPanelApp : MonoBehaviour
{
    byte[] _latestImageBytes;
    HoloLensCameraStream.Resolution _resolution;

    //"Injected" objects.
    VideoPanel _videoPanelUI;
    VideoCapture _videoCapture;

	void Start()
    {
		_videoPanelUI = GameObject.FindObjectOfType<VideoPanel>();

		#if !UNITY_EDITOR && (UNITY_WSA || NETFX_CORE)
		//Call this in Start() to ensure that the CameraStreamHelper is already "Awake".
		CameraStreamHelper.Instance.GetVideoCaptureAsync(OnVideoCaptureCreated);
		//You could also do this "shortcut":
		//CameraStreamManager.Instance.GetVideoCaptureAsync(v => videoCapture = v);
		#else
		StartCoroutine(initAndWaitForWebCamTexture());
		#endif
    }

    #if !UNITY_EDITOR && (UNITY_WSA || NETFX_CORE)
	private void OnDestroy()
	{
		if (_videoCapture != null)
		{
			_videoCapture.FrameSampleAcquired -= OnFrameSampleAcquired;
			_videoCapture.Dispose();
		}
	}

	void OnVideoCaptureCreated(VideoCapture videoCapture)
	{
		if (videoCapture == null)
		{
			Debug.LogError("Did not find a video capture object. You may not be using the HoloLens.");
			return;
		}

		this._videoCapture = videoCapture;

		_resolution = CameraStreamHelper.Instance.GetLowestResolution();
		//float frameRate = CameraStreamHelper.Instance.GetHighestFrameRate(_resolution);
		float frameRate = CameraStreamHelper.Instance.GetLowestFrameRate(_resolution);
		videoCapture.FrameSampleAcquired += OnFrameSampleAcquired;

		//You don't need to set all of these params.
		//I'm just adding them to show you that they exist.
		CameraParameters cameraParams = new CameraParameters();
		cameraParams.cameraResolutionHeight = _resolution.height;
		cameraParams.cameraResolutionWidth = _resolution.width;

		//new
		frameRate = _videoPanelUI.requestedFrameRate;
		cameraParams.frameRate = Mathf.RoundToInt(frameRate);
		int finalFrameRate = Mathf.RoundToInt(frameRate);

		cameraParams.pixelFormat = CapturePixelFormat.BGRA32;
		cameraParams.rotateImage180Degrees = true; //If your image is upside down, remove this line.
		cameraParams.enableHolograms = false;

		_videoPanelUI.SetResolution(_resolution.width, _resolution.height, finalFrameRate);

		videoCapture.StartVideoModeAsync(cameraParams, OnVideoModeStarted);
	}

	void OnVideoModeStarted(VideoCaptureResult result)
	{
		if (result.success == false)
		{
			Debug.LogWarning("Could not start video mode.");
			return;
		}

		Debug.Log("Video capture started.");
	}

	void OnFrameSampleAcquired(VideoCaptureSample sample)
	{
		//When copying the bytes out of the buffer, you must supply a byte[] that is appropriately sized.
		//You can reuse this byte[] until you need to resize it (for whatever reason).
		if (_latestImageBytes == null || _latestImageBytes.Length < sample.dataLength)
		{
		_latestImageBytes = new byte[sample.dataLength];
		}
		sample.CopyRawImageDataIntoBuffer(_latestImageBytes);
		sample.Dispose();

		_videoPanelUI.SetBytes(_latestImageBytes);
	}
	#else
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

	/// texture buffer to hold raw data
	private Texture2D currentTexture;


	bool camReady = false;
	int requestedFrameRate = 15;
	IEnumerator initAndWaitForWebCamTexture()
	{
		if (!camReady) {
			// Get all devices , front and back camera
			devices = WebCamTexture.devices;

			if (devices.Length > 0) {
				SelectCamera (isFrontFacing);

				if (device.name != "Null") {
					webCamTexture = new WebCamTexture (device.name, targetWidth, targetHeight, requestedFrameRate);
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

		_videoPanelUI.SetResolution(webCamTexture.width, webCamTexture.height, requestedFrameRate);
	}

	public void SelectCamera(bool isFrontFacing, string name = "")
	{
		foreach (WebCamDevice d in devices)
		{
			if (d.name.Length > 1 && d.name == name)
			{
				webCamTexture.Stop();
				device = d;

				webCamTexture = new WebCamTexture(device.name, targetWidth, targetHeight, requestedFrameRate);
				webCamTexture.Play();
			}
			else if (d.isFrontFacing == isFrontFacing)
			{
				device = d;
			}
		}
	}

	void Update(){
		currentTexture.SetPixels(webCamTexture.GetPixels());
		_latestImageBytes = currentTexture.GetRawTextureData();
		_videoPanelUI.SetBytes(_latestImageBytes);
	}

	#endif
}
