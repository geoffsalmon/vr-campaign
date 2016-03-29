using UnityEngine;
using System.Collections;

// Describes the start and end locations for the path between reefs. Calculates progress of the movement over time.
// Sends an event with progress so listeners can react. The movement is split into 3 stages:
//   1) Entering the tunnel moving from the Body's initial position, StartPosition, to EnterPosition
//   2) Travelling through tunnel from EnterPosition to ExitPosition
//   3) Exiting out of the tunnel moving from ExitPosition to FinalPosition
//
// A separate progress float (EnterProgress, Progress, ExitProgress) is incremented
// from 0 to 1 for each stage based on the amount of time configured for each stage.
// Other scripts listen to this progress and can draw the tunnel and move the player and fish.
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

	// A delegate and static event to notify other scripts of the transit progress. This
	// assumes there is a single active Transit script at any one time.
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

	public CubicBezier Curve { get; private set; }

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

		// Create a curve that can be accessed by anything listening to transit progress
		// TODO: Does this belong in the Transit object?
		// TODO: Should this curve, or a separate set of curves include the tunnel entering and exiting part of the transit
		Curve = new CubicBezier(EnterPosition, 2*EnterPosition - StartPosition + new Vector3(Random.value, Random.value, Random.value) * 4,
		                        2*ExitPosition - FinalPosition + new Vector3(Random.value, Random.value, Random.value) * 4, ExitPosition);
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

		/*
		Debug.DrawLine(StartPosition, EnterPosition, Color.red);
		Debug.DrawLine(EnterPosition, ExitPosition, Color.blue);
		Debug.DrawLine(ExitPosition, FinalPosition, Color.green);
		*/

		/*
		Vector3 pos0, pos1;
		pos1 = Curve.position(0);
		for (float t = 0; t <= 1.0f; t += 0.1f) {
			pos0 = pos1;
			pos1 = Curve.position(t);
			Debug.DrawLine(pos0, pos1, Color.yellow);
		}
		pos0 = pos1;
		pos1 = Curve.position(1);
		Debug.DrawLine(pos0, pos1, Color.yellow);*/
	}
}
