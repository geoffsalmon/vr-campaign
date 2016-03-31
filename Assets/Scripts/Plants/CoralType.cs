using UnityEngine;
using System.Collections;

public enum CoralDirections{
	random,normal,up
}

//CoralType is used by CoralDecorator to describe the corals to generate.

[System.Serializable]
public class CoralType {
	//The prefab is simply the coral to generate. Usually it will have a Coral component attached to it already for appearance settings.
	public GameObject prefab;

	//There are three options here:
	// random: the coral rotation will be totally random.
	// normal: the coral rotation will align with the normal of the mesh surface it is generated on.
	// up: the coral rotation will face up, and randomly rotate on the Z axis.
	public CoralDirections coralDirection;

	// How likely each coral will be chosen. For example:
	// CoralType[0].likelihood=1
	// CoralType[1].likelihood=5
	// this means the second CoralType is five times more likely to be chosen.
	public int likelihood=1;
}
