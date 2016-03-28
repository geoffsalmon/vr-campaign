using UnityEngine;
using System.Collections;

public class StraightTunnel : MonoBehaviour {
	//[Tooltip("Assumed to have same orientation and length as cylinder primitve.")]
	//public GameObject TubePrefab;

	public Material TubeMaterial;

	//[Tooltip("Used when TubePrefab is not set.")]
	//public float DefaultDiameter = 1;

	private GameObject tube;
	private bool tubeAdded = false;

	void OnEnable() {
		Transit.OnTransit += OnTransit;	
	}

	void OnDisable() {
		Transit.OnTransit -= OnTransit;
	}

	private void OnTransit(Transit transit, Transit.TransitState state, float progress) {
		if (state == Transit.TransitState.Entering && !tubeAdded) {
			tubeAdded = true;

			GameObject tube = new GameObject("tube");
			MeshFilter filter = tube.AddComponent<MeshFilter>();
			TubeMesh.generateTube(filter.mesh);

			MeshRenderer renderer = tube.AddComponent<MeshRenderer>();
			renderer.receiveShadows = false;
			renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;

			if (TubeMaterial == null) {
				renderer.material.color = Color.red;
			} else {
				renderer.material = TubeMaterial;
			}
			/*
			tube = GameObject.CreatePrimitive(PrimitiveType.Mes);
			tube.GetComponent<MeshRenderer>().material.color = Color.red;
			scale.x = DefaultDiameter;
			scale.z = DefaultDiameter;*/

			// position tube between transit's enter and exit positions
			// Assumes tube is oriented along y dimension with ends at y=0 and y=1.
			Vector3 offset = transit.ExitPosition - transit.EnterPosition;
			Vector3 scale = new Vector3(1f, offset.magnitude, 1f);
			Transform t = tube.transform;
			t.position = transit.EnterPosition;
			t.up = offset;
			t.localScale = scale;
			t.parent = gameObject.transform;
		} else if (state == Transit.TransitState.Finished) {
			// will be destroyed automatically at end
		}
	}
}
