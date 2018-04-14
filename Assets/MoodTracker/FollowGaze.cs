using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowGaze : MonoBehaviour {

	private Camera mainCamera;

	// Use this for initialization
	void Start () {
		mainCamera = FindObjectOfType<Camera> ();
		gameObject.transform.SetParent (mainCamera.transform);

		StartCoroutine (StabilizeView());
	}

	IEnumerator StabilizeView(){
		
		yield return new WaitForSeconds (0.2f);
		gameObject.transform.localPosition = new Vector3 (0, 0, 0);
		gameObject.transform.localRotation = Quaternion.Euler (0, 0, 0);

	}


}
