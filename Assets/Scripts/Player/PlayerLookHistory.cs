using UnityEngine;
using System.Collections;
using UnityEngine.Assertions;

public class PlayerLookHistory : MonoBehaviour
{
	private class LogEvent
	{
		public Vector3 lookDirection;
		public Vector3 playerPosition;

		public LogEvent(){
			lookDirection=Vector3.zero;
			playerPosition=Vector3.zero;
		}
	}

	//this is how often, in seconds, the PlayerLookHistory will record information
	public static float interval = 0.2f;
	
	//how many seconds into the past where values will be recorded
	public float lookAtHistorySeconds=5;

	private GameObject player;
	private LogEvent[] logEvents;
	private int historyIndex=0;
	private WaitForSeconds waitForSeconds;
	
	void Start ()
	{
		waitForSeconds = new WaitForSeconds (interval);
		player = Camera.main.gameObject;
		logEvents= new LogEvent[Mathf.CeilToInt(lookAtHistorySeconds/interval)];
		for (int i=0; i<logEvents.Length; i++)
			logEvents [i] = new LogEvent ();

		if (FindObjectsOfType<PlayerLookHistory> ().Length > 1)
			Debug.LogWarning ("PlayerLookHistory is meant to be a singleton. But there exist more than one. This is probably bad, and slows the game down.");

		StartCoroutine (Cycle());
	}

	public Vector3 GetAverageLookDirection(){
		Vector3 total=Vector3.zero;
		foreach (LogEvent log in logEvents) {
			total+=log.lookDirection;
		}
		total /= logEvents.Length;
		return total;
	}

	public Vector3 GetAverageLookPosition(float range){
		Vector3 total=Vector3.zero;
		foreach (LogEvent log in logEvents) {
			total+=log.playerPosition+log.lookDirection*range;
		}
		total /= logEvents.Length;
		return total;
	}
	
	private IEnumerator Cycle ()
	{
		yield return new WaitForSeconds (Random.Range (0, interval));
		while (true) {
			logEvents[historyIndex].lookDirection=player.transform.forward;
			logEvents[historyIndex].playerPosition=player.transform.position;
			historyIndex++;
			historyIndex=historyIndex%logEvents.Length;
			
			yield return waitForSeconds;
			
		}
	}
}
