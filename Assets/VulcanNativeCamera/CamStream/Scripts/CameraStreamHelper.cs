//  
// Copyright (c) 2017 Vulcan, Inc. All rights reserved.  
// Licensed under the Apache 2.0 license. See LICENSE file in the project root for full license information.
//

using HoloLensCameraStream;
using System;
using System.Linq;
using UnityEngine;

public class CameraStreamHelper : MonoBehaviour
{
    event OnVideoCaptureResourceCreatedCallback VideoCaptureCreated;

    static VideoCapture videoCapture;

    static CameraStreamHelper instance;
    public static CameraStreamHelper Instance
    {
        get
        {
            return instance;
        }
    }

    public void GetVideoCaptureAsync(OnVideoCaptureResourceCreatedCallback onVideoCaptureAvailable)
    {
        if (onVideoCaptureAvailable == null)
        {
            Debug.LogError("You must supply the onVideoCaptureAvailable delegate.");
        }

        if (videoCapture == null)
        {
            VideoCaptureCreated += onVideoCaptureAvailable;
        }
        else
        {
            onVideoCaptureAvailable(videoCapture);
        }
    }

    public HoloLensCameraStream.Resolution GetHighestResolution()
    {
        if (videoCapture == null)
        {
            throw new Exception("Please call this method after a VideoCapture instance has been created.");
        }
        return videoCapture.GetSupportedResolutions().OrderByDescending((r) => r.width * r.height).FirstOrDefault();
    }

    public HoloLensCameraStream.Resolution GetLowestResolution()
    {
        if (videoCapture == null)
        {
            throw new Exception("Please call this method after a VideoCapture instance has been created.");
        }
        return videoCapture.GetSupportedResolutions().OrderBy((r) => r.width * r.height).FirstOrDefault();
    }

    public float GetHighestFrameRate(HoloLensCameraStream.Resolution forResolution)
    {
        if (videoCapture == null)
        {
            throw new Exception("Please call this method after a VideoCapture instance has been created.");
        }
        return videoCapture.GetSupportedFrameRatesForResolution(forResolution).OrderByDescending(r => r).FirstOrDefault();
    }

    public float GetLowestFrameRate(HoloLensCameraStream.Resolution forResolution)
    {
        if (videoCapture == null)
        {
            throw new Exception("Please call this method after a VideoCapture instance has been created.");
        }
        return videoCapture.GetSupportedFrameRatesForResolution(forResolution).OrderBy(r => r).FirstOrDefault();
    }

    private void Awake()
    {
        if (instance != null)
        {
            Debug.LogError("Cannot create two instances of CamStreamManager.");
            return;
        }

        instance = this;
        VideoCapture.CreateAync(OnVideoCaptureInstanceCreated);
    }

    private void OnDestroy()
    {
        if (instance == this)
        {
            instance = null;
        }
    }

    private void OnVideoCaptureInstanceCreated(VideoCapture videoCapture)
    {
        if (videoCapture == null)
        {
            Debug.LogError("Creating the VideoCapture object failed.");
            return;
        }

        CameraStreamHelper.videoCapture = videoCapture;
        if (VideoCaptureCreated != null)
        {
            VideoCaptureCreated(videoCapture);
        }
    }
}
