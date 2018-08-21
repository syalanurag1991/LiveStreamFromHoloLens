using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DisplayFPS : MonoBehaviour {

	TextMesh fpsText;
	public float deltaTime;

	// Use this for initialization
	void Start () {
		fpsText = gameObject.GetComponent<TextMesh> ();
	}

//	void Update () {
//		deltaTime += (Time.deltaTime - deltaTime) * 0.1f;
//		float fps = 1.0f / deltaTime;
//		fpsText.text = Mathf.Ceil (fps).ToString ();
//	}

	int m_frameCounter = 0;
	float m_timeCounter = 0.0f;
	float m_lastFramerate = 0.0f;
	public float m_refreshTime = 0.1f;

	void Update()
	{
		if( m_timeCounter < m_refreshTime )
		{
			m_timeCounter += Time.deltaTime;
			m_frameCounter++;
		}
		else
		{
			//This code will break if you set your m_refreshTime to 0, which makes no sense.
			m_lastFramerate = (float)m_frameCounter/m_timeCounter;
			m_frameCounter = 0;
			m_timeCounter = 0.0f;
		}

		fpsText.text = Mathf.Ceil (m_lastFramerate).ToString ();
	}

	public void SetDisplayMode(bool display){
		gameObject.SetActive(display);
	}

}
