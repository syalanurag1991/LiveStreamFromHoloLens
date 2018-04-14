using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealthCube : MonoBehaviour {

	public UIManager uiManager;

	// Mood Tracker Attributes
	Renderer moodTrackerRenderer;

	// Use this for initialization
	void Start () {
		uiManager = FindObjectOfType<UIManager>();
		moodTrackerRenderer = gameObject.GetComponent<Renderer>();
	}

	// Update is called once per frame
	void Update () {

		//Update uiManager-cube position
		//Vector3 moodTrackerPosition = new Vector3 (uiManager.normalizedMoodTrackerCoordinates.x, uiManager.normalizedMoodTrackerCoordinates.y, uiManager.normalizedMoodTrackerCoordinates.z - 7);

		//if(!(float.IsNaN(moodTrackerPosition.x) || float.IsNaN(moodTrackerPosition.y) || float.IsNaN(moodTrackerPosition.z)))
		//	gameObject.transform.localPosition = moodTrackerPosition;

		//Update mood-cube color
		Color moodColor = uiManager.GetMoodTrackerColor();
		moodTrackerRenderer.material.color = moodColor;
	}
}
