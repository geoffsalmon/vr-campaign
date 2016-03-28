using UnityEngine;
using System.Collections;

// Manages moving from one reef to another and instantiates the reefs and tunnels inbetween as needed.
public class VisitReefs : MonoBehaviour {
	// Body is the object moved during transits
	public GameObject Body;
	// The forward direction of Head is used to place transit tunnels. TODO: Need a better way of setting this
	public GameObject Head;

	public GameObject TunnelPrefab;
	public GameObject ReefPrefab;

	public float TimeAtReef;
	public float TimeInTransit;

	public enum VisitReefsState {
		Start,
		InTransit,
		AtReef
	};

	public VisitReefsState State { get; private set; }

	private Transform bodyTransform;

	// The reef we are visiting or visited last if in transit
	private GameObject currentReef;
	// The reef we are visiting next if in transit, null otherwise
	private GameObject nextReef;

	// The current tunnel object. Non-null while in transit.
	private GameObject tunnel;

	// Holds all the instantiate reefs
	private GameObject reefs;

	// Time of next state change. Can be delayed if waiting is false.
	private float nextTime;
	private bool waiting = false;

	void OnDisable() {
		Transit.OnTransit -= OnTransit;
	}

	void Start () {
		reefs = new GameObject("Reefs");
		reefs.transform.parent = gameObject.transform;

		bodyTransform = Body.transform;
		State = VisitReefsState.Start;
		nextTime = Time.time;

		if (ReefPrefab == null) {
			ReefPrefab = GameObject.CreatePrimitive (PrimitiveType.Cube);
		}
		if (TunnelPrefab == null) {
			TunnelPrefab = new GameObject("Default Transit");
			TunnelPrefab.AddComponent<Transit>();
			//TunnelPrefab.SetActiveRecursively(false);
			TunnelPrefab.SetActive(false);
		} else {
			// ensure it has a Transit component
			if (TunnelPrefab.GetComponent<Transit>() == null) {
				TunnelPrefab.AddComponent<Transit>();
			}
		}
	}

	private void OnTransit(Transit transit, Transit.TransitState state, float progress) {
		if (state == Transit.TransitState.Finished) {
			// trigger at next update
			nextTime = Time.time;
			waiting = false;
			Destroy(tunnel);
			tunnel = null;
			if (currentReef != null) {
				Destroy(currentReef);
			}
			currentReef = nextReef;
			nextReef = null;
		} else {
			// TODO: When should the next reef be instantiated and old one destroyed? Immediately? Part way through the tunnel?
			if (nextReef == null) {
				// put reef in wrapper object positioned relative to transit destination
				nextReef = new GameObject("Reef Wrapper");
				nextReef.transform.parent = reefs.transform;
				GameObject reef = Instantiate(ReefPrefab);
				reef.transform.parent = nextReef.transform;
				MeshRenderer renderer = reef.GetComponent<MeshRenderer>();
				if (renderer != null) {
					renderer.material.color = new Color(Random.value, Random.value, Random.value, 1.0f);
				}

				// Place reef a bit past where transit will end
				Vector3 dir = transit.FinalPosition - transit.ExitPosition;
				dir.Normalize();
				Vector3 pos = transit.FinalPosition + 4 * dir;
				pos.y = 0;
				nextReef.transform.position = pos;
				// TODO: Rotate reef so it's facing the player

				Debug.Log("New reef at " + pos);
			}
		}
	}

	private void startTransit() {
		if (tunnel != null) {
			Destroy(tunnel);
		}
		tunnel = GameObject.Instantiate(TunnelPrefab);
		tunnel.SetActive(true);
		Transit transit = tunnel.GetComponent<Transit>();
		Transit.OnTransit += OnTransit;
		transit.Body = Body;

		// setup transit, from where to where?
		Vector3 forward = Head.transform.forward;
		forward.y = 0;
		forward.Normalize();

		// setup a straight transit
		transit.EnterPosition = bodyTransform.position + 3 * forward;
		transit.ExitPosition = transit.EnterPosition + 10 * forward;
		transit.FinalPosition = transit.ExitPosition + 3 * forward;

		State = VisitReefsState.InTransit;
		waiting = true;
	}

	private void visitReef() {
		nextTime = Time.time + TimeAtReef;
		State = VisitReefsState.AtReef;
	}
	
	void Update () {
		if (!waiting && Time.time >= nextTime) {
			// time to update state

			switch (State) {
			case VisitReefsState.Start:
				startTransit();
				break;
			case VisitReefsState.InTransit:
				visitReef();
				break;
			case VisitReefsState.AtReef:
				startTransit();
				break;
			}
		}
	}
}
