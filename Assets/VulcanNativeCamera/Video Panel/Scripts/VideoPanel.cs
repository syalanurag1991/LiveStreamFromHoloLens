// Copyright (c) 2017 Vulcan, Inc. All rights reserved.  
// Licensed under the Apache 2.0 license. See LICENSE file in the project root for full license information.

using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;
using System.Collections.Generic;

using VacuumShaders.TextureExtensions;

public class VideoPanel : MonoBehaviour
{
    //public RawImage rawImage;
	int countOfFramesApp = 0;
	//public Renderer camQuadRenderer;

	public int resizeToWidth;
	public int resizeToHeight;
	public int requestedFrameRate;
	public int bufferSize;

	int finalWidth;
	int finalHeight;
	int finalFrameRate;

	string status;
	int countOfFramesActual = 0;
	int countOfFramesSent = 0;
	byte[] stackedImage;
	Texture2D stackedFrameTexture;

	[HideInInspector]
	public byte[] compressedImage;
	[HideInInspector]
	public int compressedImageSize = 0;
	[HideInInspector]
	public Texture2D resizedFrameTexture;

	Queue<byte[]> queueOfFrames = new Queue<byte[]>();

	public void SetResolution(int width, int height, int framerate)
    {
		//For setting up Cam Quad Display resolution
//		Texture2D targetTexture = new Texture2D(resizeToWidth, resizeToHeight, TextureFormat.RGB24, false);
//		camQuadRenderer.material.mainTexture = targetTexture;
//		Mesh targetMesh = camQuadRenderer.GetComponentInParent<MeshFilter> ().mesh;
//		var newUV = targetMesh.uv;
//
//		for (int i=0; i<newUV.Length; i++) {
//			if (Mathf.Approximately (newUV [i].x, 1))
//				newUV[i].x = 0;
//			else
//				newUV[i].x = 1;
//		}
//
//		targetMesh.uv = newUV;

		finalWidth = width;
		finalHeight = height;
		finalFrameRate = framerate;
    }
		
	public void SetBytes(byte[] image)
    { 
		countOfFramesActual++;
		if(queueOfFrames.Count < bufferSize)
			queueOfFrames.Enqueue (image);
    }

	public bool startSending = false;
	public void PreProcessFrame(){

		status = "Starting pre-processing";

		if (queueOfFrames.Count < 1) {
			return;
		}

		countOfFramesSent++;
		byte[] stackedImage = queueOfFrames.Dequeue();
		stackedFrameTexture.LoadRawTextureData(stackedImage);
		stackedFrameTexture.Apply();
		status = "Texture Applied";

		//Resize the texture for better frame-rate, it's faster to encode image of smaller size
		TextureResizePro.ResizePro(stackedFrameTexture, resizeToWidth, resizeToHeight, out resizedFrameTexture, false);
		status = "Texture Resized";

		//Encode to JPG for smallest size, Encode to PNG for better quality
		compressedImage = resizedFrameTexture.EncodeToJPG();
		compressedImageSize = compressedImage.Length;
		status = "Compression done";

		if (!startSending)
			startSending = true;
	}

	void DisplayFrame(){
		//camQuadRenderer.material.mainTexture = resizedFrameTexture;
		status = "Texture Loaded";
	}

	void Initialize(){
		startSending = false;
		stackedFrameTexture = new Texture2D (896, 504, TextureFormat.RGBA32, false);
		resizedFrameTexture = new Texture2D (resizeToWidth, resizeToHeight, TextureFormat.RGB24, false);
	}

	public bool displayLogData = false;
	public DisplayInfo displayInfo;
	IEnumerator DisplayStatus(){
		while (true) {
			yield return endOfFrame;	
			countOfFramesApp++;
			if (displayLogData) {
				string message1 = "Frames: " + countOfFramesApp + "\nWidth: " + finalWidth + "\nHeight: " + finalHeight;
				string message2 = "Status: " + status + "\nCompressed: " + compressedImageSize;
				string message3 = "Camera FPS: " + finalFrameRate + "\nCamera frames: " + countOfFramesActual + "\nFrames sent: " + countOfFramesSent + "\nBuffered Frames: " + queueOfFrames.Count;
				displayInfo.ClearAndSetDisplayText (message1 + "\n" + message2 + "\n" + message3);
			}
		}
	}

	void Start()
	{
		Initialize();
		StartCoroutine (DisplayStatus());
	}

	void Update(){
		PreProcessFrame();
		DisplayFrame();
	}

	WaitForEndOfFrame endOfFrame = new WaitForEndOfFrame();
}
