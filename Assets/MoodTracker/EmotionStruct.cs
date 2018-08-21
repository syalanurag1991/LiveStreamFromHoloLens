using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EmotionStruct {

	public float joy;
	public float anger;
	public float sadness;
	public float surprise;
	public float fear;
	public float disgust;

	public float emotionThreshold = 10.0f;

	public EmotionStruct()
	{
		joy = 0.0f;
		anger = 0.0f;
		sadness = 0.0f;
		surprise = 0.0f;
		fear = 0.0f;
		disgust = 0.0f;
	}

	// Note that this does NOT consider surprise or disgust when finding the highest value
	public string getSingleHighestEmotionString()
	{
		if (joy > fear &&
			joy > sadness && 
			joy > anger && 
			joy > emotionThreshold)
		{
			return "joy";
		}
		else if (fear > sadness &&
			fear > anger &&
			fear > emotionThreshold)
		{
			return "fear";
		}
		else if (sadness > anger &&
			sadness > emotionThreshold)
		{
			return "sadness";
		}
		else if (anger > emotionThreshold)
		{
			return "anger";
		}
		else
		{
			return "neutral";
		}
	}
}
