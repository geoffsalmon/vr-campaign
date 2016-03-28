using UnityEngine;
using System.Collections;

public class LerpMoveTransit : MonoBehaviour {
	private Transform bodyTransform;

	void OnEnable() {
		Transit.OnTransit += OnTransit;	
	}

	void OnDisable() {
		Transit.OnTransit -= OnTransit;
	}

	// Simply LERP the position through the transit. This won't be smooth.
	private void OnTransit(Transit transit, Transit.TransitState state, float progress) {
		if (bodyTransform == null) {
			bodyTransform = transit.Body.transform;
		}

		switch (state) {
		case Transit.TransitState.Entering:
			bodyTransform.position = Vector3.Lerp(transit.StartPosition, transit.EnterPosition, transit.EnterProgress);
			break;
		case Transit.TransitState.InProgress:
			bodyTransform.position = Vector3.Lerp(transit.EnterPosition, transit.ExitPosition, progress);
			break;
		case Transit.TransitState.Exiting:
			bodyTransform.position = Vector3.Lerp(transit.ExitPosition, transit.FinalPosition, transit.ExitProgress);
			break;
		}
	}
}
