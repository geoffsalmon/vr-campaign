using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CurvedTunnel : MonoBehaviour {
	public Material TubeMaterial;

	private GameObject tube;
	private bool tubeAdded = false;

	void OnEnable() {
		Transit.OnTransit += OnTransit;	
	}

	void OnDisable() {
		Transit.OnTransit -= OnTransit;
	}

	private float calcRadius(float t) {
		return 8 * (t - 0.5f) * (t - 0.5f) + 1;
	}

	private void OnTransit(Transit transit, Transit.TransitState state, float progress) {
		if (state == Transit.TransitState.Entering && !tubeAdded) {
			tubeAdded = true;

			GameObject tube = new GameObject("tube");
			MeshFilter filter = tube.AddComponent<MeshFilter>();

			List<TubePoint> points = new List<TubePoint>();
			CubicBezier curve = transit.Curve;

			for (float t = 0f; t < 1f; t += 0.2f) {
				points.Add(new TubePoint(curve.position(t), curve.derivative(t), calcRadius(t)));
			}
			points.Add(new TubePoint(curve.position(1), curve.derivative(1), calcRadius(1)));

			/*foreach (TubePoint p in points) {
				Debug.Log(p.position + " " + p.derivative + " " + p.radius);
			}*/

			TubeMesh.generateTubes(filter.mesh, points, 8);

			MeshRenderer renderer = tube.AddComponent<MeshRenderer>();
			renderer.receiveShadows = false;
			renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;

			if (TubeMaterial == null) {
				renderer.material.color = Color.red;
			} else {
				renderer.material = TubeMaterial;
			}
			tube.transform.parent = gameObject.transform;
		} else if (state == Transit.TransitState.Finished) {
			// will be destroyed automatically at end
		}
	}
}
