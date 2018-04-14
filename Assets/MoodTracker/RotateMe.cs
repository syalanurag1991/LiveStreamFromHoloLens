using UnityEngine;

public class RotateMe : MonoBehaviour
{
	public float TurnRate = 90f;

	void Update()
	{
		transform.Rotate(
			Vector3.up *
			Time.deltaTime *
			TurnRate);
	}
}
