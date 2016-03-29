using UnityEngine;
using System.Collections;

public class StartScreen : MonoBehaviour {
	public GameObject spawnObject;
	// Use this for initialization
	void Start () {
		GameObject clone;
    	clone = Instantiate(spawnObject.transform, transform.position, Quaternion.Euler(-90,0,0)) as GameObject;
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
