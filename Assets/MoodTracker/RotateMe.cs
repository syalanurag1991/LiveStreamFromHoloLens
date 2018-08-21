using UnityEngine;

public class RotateMe : MonoBehaviour
{
	public float TurnRate = 90f;
	public float speed = 1f;
	Vector3 topLeft = new Vector3(-1, 1, 10);
	Vector3 bottomLeft = new Vector3(-1, -1, 10);
	Vector3 bottomRight = new Vector3(1, -1, 10);
	Vector3 topRight = new Vector3(1, 1, 10);
	int currentTarget;

	Vector3[] targetPositions = new Vector3[4];

	float elapsedTime = 0;

	void Start()
	{
		transform.position = topRight;
		targetPositions [0] = topRight;
		targetPositions [1] = topLeft;
		targetPositions [2] = bottomLeft;
		targetPositions [3] = bottomRight;
		currentTarget = 0;
	}

	void Update()
	{
		transform.Rotate(
			Vector3.up *
			Time.deltaTime *
			TurnRate);

		elapsedTime += Time.deltaTime;

		Vector3 currentPosition = transform.position;
		Vector3 currentTargetPosition = targetPositions[currentTarget];
		if (currentPosition != currentTargetPosition) {
			//transform.position = Vector3.Lerp (currentPosition, currentTargetPosition, speed * elapsedTime);
			transform.position = Vector3.MoveTowards (currentPosition, currentTargetPosition, speed * Time.deltaTime);
		} else {
			currentTarget = (currentTarget+1)% targetPositions.Length;
		}


//		transform.position = Vector3.Lerp(topLeft, bottomLeft, speed * elapsedTime);
//		transform.position = Vector3.Lerp(bottomLeft, bottomRight, speed * elapsedTime);
//		transform.position = Vector3.Lerp(bottomRight, topRight, speed * elapsedTime);

	}
}

