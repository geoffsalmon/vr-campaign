using UnityEngine;
using System.Collections;
using UnityEngine.Assertions;

public class VisionTracker : MonoBehaviour
{
	//this is how often, in seconds, the vision tracker will check what is looking at what.
	public static float interval = 1;

	//the directLookAtScore will only increment when the lookAtScore is at least this much.
	public float lookAtThreshold=0.8f;

	//how many seconds into the past where lookAt scores will be recorded
	public float lookAtHistorySeconds=5;

	private float lookAtTimer = 0;
	private float directLookAtScore = 0;
	private GameObject player;
	private float[] lookAtHistory;
	private int historyIndex=0;
	private WaitForSeconds waitForSeconds;

	void Start ()
	{
		waitForSeconds = new WaitForSeconds (interval);
		player = Camera.main.gameObject;
		lookAtHistory= new float[Mathf.CeilToInt(lookAtHistorySeconds/interval)];
		StartCoroutine (Cycle());
	}

	public float GetHistoryScore(){
		//returns a number between [-1,1] where 1 means the player 
		//has been looking at this object for lookAtHistorySeconds seconds
		float total = 0;
		foreach (float i in lookAtHistory)
			total += i;
		return total/lookAtHistorySeconds;
	}

	public float GetLookAtScore(){
		//returns a score between [-1,1] where 1 means the player is looking directly at this object
		Vector3 playerToObject = gameObject.transform.position - player.transform.position;
		playerToObject.Normalize ();
		Vector3 lookDirection = Camera.main.transform.forward;

		return Vector3.Dot (playerToObject, lookDirection);
	}

	public float GetDirectLookAtScore(){
		//the total number of seconds that the player has been directly looking at this object.
		//"directly" is defined as having a greater lookAtScore than the threshold value.
		return directLookAtScore;
	}

	private IEnumerator Cycle ()
	{
		yield return new WaitForSeconds (Random.Range (0, interval));
		while (true) {
			float score=GetLookAtScore();
			if (score>lookAtThreshold)
				directLookAtScore+=interval;

			lookAtHistory[historyIndex]=score;
			historyIndex++;
			historyIndex=historyIndex%lookAtHistory.Length;

			yield return waitForSeconds;

		}
	}
}
