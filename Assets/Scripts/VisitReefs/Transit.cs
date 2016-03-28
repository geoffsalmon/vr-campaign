using UnityEngine;
using System.Collections;

// Describes the path of the movement and calculates progress of the movement over time.
// Sends an event with progress so listeners can react. The flight is split into 3 sections:
//   1) Entering which moves the Body from it's initial position, stored as StartPosition, to the set EnterPosition
//   2) Flying through tunnel from EnterPosition to ExitPosition
//   3) Leaving out of the tunnel from ExitPosition to FinalPosition
//
// A separate float (EnterProgress, Progress, ExitProgress) is incremented from 0 to 1 for each stage.
// Other scripts listen to this progress and animate the tunnel and movement.
public class Transit : MonoBehaviour {

	public GameObject Body;

	// TODO: Use Ray objectss here instead of vectors to capture direction as well to smoothly transition?
	// Or should rotation be entirely up to the VR rotation?
	public Vector3 StartPosition { get; private set; } 
	public Vector3 EnterPosition;
	public Vector3 ExitPosition;
	public Vector3 FinalPosition;

	public float EnterTransitTime = 1;
	public float InTransitTime = 5;
	public float ExitTransitTime = 1;

	public enum TransitState { Entering, InProgress, Exiting, Finished };
	public TransitState State { get; private set; }

	public delegate void UpdateTransit(Transit transit, TransitState state, float progress);
	public static event UpdateTransit OnTransit;

	private Transform bodyTransform;

	private float startTime;
	private float enterTime;
	private float exitTime;
	private float finishTime;

	public float Progress { get; private set; }
	public float EnterProgress { get; private set; }
	public float ExitProgress { get; private set; }

	void Start () {
		startTime = Time.time;
		enterTime = startTime + EnterTransitTime;
		exitTime = enterTime + InTransitTime;
		finishTime = exitTime + ExitTransitTime;
		State = TransitState.Entering;

		bodyTransform = Body.transform;
		StartPosition = bodyTransform.position;

		Progress = 0;
		EnterProgress = 0;
		ExitProgress = 0;
	}

	void Update () {
		float time = Time.time;
		if (time >= finishTime) {
			State = TransitState.Finished;
			ExitProgress = 1;
		} else if (time >= exitTime) {
			State = TransitState.Exiting;
			ExitProgress = (time - exitTime) / ExitTransitTime;
			Progress = 1;
		} else if (time >= enterTime) {
			State = TransitState.InProgress;
			Progress = (time - enterTime) / InTransitTime;
			EnterProgress = 1;
		} else {
			State = TransitState.Entering;
			EnterProgress = (time - startTime) / EnterTransitTime;
		}

		//Debug.Log(State.ToString() + " " + EnterProgress + " " + Progress + " " + ExitProgress);
		if (OnTransit != null) {
			OnTransit(this, State, Progress);
			if (State == TransitState.Finished) {
				OnTransit = null;
			}
		}

		Debug.DrawLine(StartPosition, EnterPosition, Color.red);
		Debug.DrawLine(EnterPosition, ExitPosition, Color.blue);
		Debug.DrawLine(ExitPosition, FinalPosition, Color.green);
	}

	void OnDrawGizmosSelected() {
		Gizmos.color = Color.red;
		Gizmos.DrawLine(StartPosition, EnterPosition);
		Gizmos.color = Color.blue;
		Gizmos.DrawLine(EnterPosition, ExitPosition);
		Gizmos.color = Color.green;
		Gizmos.DrawLine(ExitPosition, FinalPosition);
	}
}
