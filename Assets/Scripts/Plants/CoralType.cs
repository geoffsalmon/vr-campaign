using UnityEngine;
using System.Collections;

public enum CoralDirections{
	random,normal,up
}

[System.Serializable]
public class CoralType {
	public GameObject prefab;
	public CoralDirections coralDirection;
	public int likelihood=1;
}
