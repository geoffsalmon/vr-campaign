using UnityEngine;
using System.Collections;

[RequireComponent (typeof(FishLure))]
public class MovingLure : MonoBehaviour {
	public float refreshInterval=1f; //recalculate the new position to drift to after this many seconds
	public float range=10f;

	private PlayerLookHistory playerLookHistory;
	private Vector3 previousPosition,targetPosition;
	private float timer=0;

	void Start () {
		targetPosition = gameObject.transform.position;
		playerLookHistory = FindObjectOfType<PlayerLookHistory> ();
		if (playerLookHistory == null)
			playerLookHistory=Camera.main.gameObject.AddComponent<PlayerLookHistory>();

	}

	void Update () {
		timer += Time.deltaTime;
		if (timer > refreshInterval) {
			previousPosition = targetPosition;
			targetPosition = playerLookHistory.GetAverageLookPosition (range);
			timer = 0;
		}
		float ratio = timer / refreshInterval;
		transform.position = Vector3.Lerp (previousPosition, targetPosition, ratio);
	}
}
