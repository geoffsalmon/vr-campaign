using UnityEngine;
using System.Collections;

public class Billboard2dCoral : MonoBehaviour {
	private Camera mainCamera;
	private GameObject billboardObject;

	void Awake(){
		mainCamera = Camera.main;
		billboardObject = new GameObject();
		billboardObject.transform.position = transform.position;
		transform.parent = billboardObject.transform;
	}

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	 	billboardObject.transform.LookAt(transform.position + mainCamera.transform.rotation * Vector3.forward, mainCamera.transform.rotation * Vector3.up);
	}
}
