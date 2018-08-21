using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoodTracker : MonoBehaviour {

	public GameManager gameManagerScript;

	// Mood Tracker Attributes
	Renderer moodTrackerRenderer;

	// Use this for initialization
	void Start () {
		gameManagerScript = FindObjectOfType<GameManager>();
		moodTrackerRenderer = gameObject.GetComponent<Renderer>();
	}

	// Update is called once per frame
	void Update () {

		//Update gameManagerScript-cube position
		//Vector3 moodTrackerPosition = new Vector3 (gameManagerScript.normalizedMoodTrackerCoordinates.x, gameManagerScript.normalizedMoodTrackerCoordinates.y, gameManagerScript.normalizedMoodTrackerCoordinates.z - 7);

		//if(!(float.IsNaN(moodTrackerPosition.x) || float.IsNaN(moodTrackerPosition.y) || float.IsNaN(moodTrackerPosition.z)))
		//	gameObject.transform.localPosition = moodTrackerPosition;

		//Update mood-cube color
		Color moodColor = gameManagerScript.GetMoodTrackerColor();
		moodTrackerRenderer.material.color = moodColor;
	}
}
