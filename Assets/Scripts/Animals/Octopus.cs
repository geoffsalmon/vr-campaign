using UnityEngine;
using System.Collections;

// Currently, seems to be a placeholder for an Octopus script. I'm not even sure Update is a good place to play animations.

public class Octopus : MonoBehaviour {
	public Animation animation;
	void Start () {
		animation = GetComponent<Animation>();
	}
	void Update () {
		animation.Play();
	}
}
