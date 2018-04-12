//  
// Copyright (c) 2017 Vulcan, Inc. All rights reserved.  
// Licensed under the Apache 2.0 license. See LICENSE file in the project root for full license information.
//

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class LocatableCameraUtils
{
    public static Vector3 PixelCoordToWorldCoord(Matrix4x4 viewTransform, Matrix4x4 projectionTransform, HoloLensCameraStream.Resolution resolution, Vector2 pixelCoordinates, float depthOffset = 0, Vector3? referenceDepthPoint = null)
    {
        //TODO: This whole function is in progress and needs to be understood and fixed. It doesn't work properly.

        pixelCoordinates = ConvertPixelCoordsToScaledCoords(pixelCoordinates, resolution);

        float focalLengthX = projectionTransform.GetColumn(0).x;
        float focalLengthY = projectionTransform.GetColumn(1).y;
        float centerOffsetX = projectionTransform.m20;
        float centerOffsetY = projectionTransform.m21;
        var dirRay = new Vector3(pixelCoordinates.x / focalLengthX, pixelCoordinates.y / focalLengthY, 1.0f).normalized; //Direction is in camera space
        var cameraPositionOffset = new Vector3(centerOffsetX / 2f, centerOffsetY / 2f);
        Vector3 centerPosition = viewTransform.MultiplyPoint(cameraPositionOffset);
        //centerPosition += frameSample.worldPosition;
        var direction = new Vector3(Vector3.Dot(dirRay, viewTransform.GetRow(0)), Vector3.Dot(dirRay, viewTransform.GetRow(1)), Vector3.Dot(dirRay, viewTransform.GetRow(2)));

        //Quaternion rotation = Quaternion.LookRotation(-cameraToWorldMatrix.GetColumn(2), cameraToWorldMatrix.GetColumn(1));
        var ray = new Ray(centerPosition, direction);

        var depth = 1f;
        if (referenceDepthPoint.HasValue == false)
        {
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit))
            {
                depth = Vector3.Magnitude(hit.point - centerPosition);
            }
        }
        else
        {
            depth = Vector3.Magnitude(referenceDepthPoint.Value - centerPosition);
        }
        depth -= depthOffset;

        return centerPosition + direction * depth;
    }

    public static Quaternion GetRotationFacingView(Matrix4x4 viewTransform)
    {
        return Quaternion.LookRotation(-viewTransform.GetColumn(2), viewTransform.GetColumn(1));
    }

    public static Matrix4x4 BytesToMatrix(byte[] inMatrix)
    {
        //Then convert the floats to a matrix.
        Matrix4x4 outMatrix = new Matrix4x4
        {
            m00 = inMatrix[0],
            m01 = inMatrix[1],
            m02 = inMatrix[2],
            m03 = inMatrix[3],
            m10 = inMatrix[4],
            m11 = inMatrix[5],
            m12 = inMatrix[6],
            m13 = inMatrix[7],
            m20 = inMatrix[8],
            m21 = inMatrix[9],
            m22 = inMatrix[10],
            m23 = inMatrix[11],
            m30 = inMatrix[12],
            m31 = inMatrix[13],
            m32 = inMatrix[14],
            m33 = inMatrix[15]
        };
        return outMatrix;

        //Not sure yet...but the given array might require that we transpose the Unity matrix to make it correct.
        //It seems that C# Matrices are "row major" and Unity Matrices are "column major".
        //return result.transpose; 
    }

    public static Matrix4x4 ProcessRawViewTransform(byte[] inMatrix)
    {
        //TODO: Understand and verify that this is the proper way to change the view transform matrix.

        Matrix4x4 outMatrix = BytesToMatrix(inMatrix);

        Matrix4x4 zflip = Matrix4x4.identity;
        zflip.SetColumn(2, -1 * zflip.GetColumn(2));

        return zflip * outMatrix * zflip;
    }

    public static Matrix4x4 ProcessRawProjectionTransform(byte[] inMatrix)
    {
        Matrix4x4 outMatrix = BytesToMatrix(inMatrix);

        //TODO: Some stuff might have to be done here.

        return outMatrix;
    }

    /// <summary>
    /// Converts pixel coordinates to screen-space coordinates that span from -1 to 1 on both axes.
    /// This is the format that is required to determine the z-depth of a given pixel taken by the HoloLens camera.
    /// </summary>
    /// <param name="pixelCoords">The coordinate of the pixel that should be converted to screen-space.</param>
    /// <param name="res">The resolution of the image that the pixel came from.</param>
    /// <returns>A 2D vector with values between -1 and 1, representing the left-to-right scale within the image dimensions.</returns>
    static Vector2 ConvertPixelCoordsToScaledCoords(Vector2 pixelCoords, HoloLensCameraStream.Resolution resolution)
    {
        float halfWidth = (float)resolution.width / 2f;
        float halfHeight = (float)resolution.height / 2f;

        //Translate registration to image center;
        pixelCoords.x -= halfWidth;
        pixelCoords.y -= halfHeight;

        //Scale pixel coords to percentage coords (-1 to 1)
        pixelCoords = new Vector2(pixelCoords.x / halfWidth, pixelCoords.y / halfHeight * -1f);

        return pixelCoords;
    }
}
