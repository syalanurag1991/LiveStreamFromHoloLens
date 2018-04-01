using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DisplayInfo : MonoBehaviour {

	int externalCount = 0;

	TextMesh displayBoard;

	// Use this for initialization
	void Start () {
		displayBoard = gameObject.GetComponent<TextMesh> ();
		StartCoroutine (tester());
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	public string GetCurrentText () {
		string currentMessage = displayBoard.text;
		return currentMessage;
	}

	public void SetDisplayText (string newMessage) {
		string currentMessage = displayBoard.text;
		displayBoard.text = currentMessage + "\n" + newMessage;
	}

	public void ClearAndSetDisplayText (string newMessage) {
		displayBoard.text = newMessage;
	}

	IEnumerator tester(){
		int internalCount = 0;
		externalCount++;
		internalCount++;
		Debug.Log ("Tester: " + internalCount.ToString() + " " + externalCount.ToString());
		yield return null;
	}
}
