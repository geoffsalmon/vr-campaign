using UnityEngine;
using System.Collections;

// A MovingLure is a FishLure that also smoothly moves towards where the player have mostly been looking.

// MovingLure uses the PlayerLookHistory component to get the information about where the player has been looking.
// If the PlayerLookHistory component doesn't exist, MovingLure will make it using default values.
// See PlayerLookHistory for more information on this.

[RequireComponent (typeof(FishLure))]
public class MovingLure : MonoBehaviour {
	//This is how often (in seconds) the MovingLure will recalculate the position it wants to drift to.
	//Note how it doesn't have to be super smooth, as lures are not visible in game.
	public float refreshInterval=1f;

	//This is how far in front of the camera should be considered the "point of focus" for the player.
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
