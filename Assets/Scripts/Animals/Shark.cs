using UnityEngine;
using System.Collections;

// Placeholder for a Shark movement script. Currently just a good way to demonstrate and test repelling lures.

public class Shark : MonoBehaviour {
	public float speed=1;

	void Update () {
		transform.position = transform.position += new Vector3 (0,0,Time.deltaTime * speed);
	}
}
