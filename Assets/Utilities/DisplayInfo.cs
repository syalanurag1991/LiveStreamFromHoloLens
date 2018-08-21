using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DisplayInfo : MonoBehaviour {

	TextMesh displayBoard;

	// Use this for initialization
	void Start () {
		displayBoard = gameObject.GetComponent<TextMesh> ();
	}
	
	public void SetDisplayMode(bool display){
		gameObject.SetActive(display);
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
}
