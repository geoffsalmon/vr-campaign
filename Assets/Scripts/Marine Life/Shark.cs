using UnityEngine;
using System.Collections;

public class Shark : MonoBehaviour {

	public float speed=1;

	void Update () {
		transform.position = transform.position += new Vector3 (0,0,Time.deltaTime * speed);
	}
}
