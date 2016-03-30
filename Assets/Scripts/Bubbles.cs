using UnityEngine;
using System.Collections;

public class Bubbles : MonoBehaviour {
	public GameObject spawnObject;
	// Use this for initialization
	void Start () {
		for(int i=0;i<8;i++){
			GameObject clone;
			// Testing load + new Vector3(Random.Range(-100.0F,100.0F),0,0)
    		clone = Instantiate(spawnObject.transform, transform.position, Quaternion.Euler(0,45*i,0)) as GameObject;
		}
		for(int i=0;i<6;i++){
			GameObject clone;
			clone = Instantiate(spawnObject.transform, transform.position, Quaternion.Euler(-45,60*i,0)) as GameObject;
		}
		for(int i=0;i<6;i++){
			GameObject clone;
			clone = Instantiate(spawnObject.transform, transform.position, Quaternion.Euler(45,60*i,0)) as GameObject;
		}
		GameObject cloneTop;
    		cloneTop = Instantiate(spawnObject.transform, transform.position, Quaternion.Euler(-90,0,0)) as GameObject;
		GameObject cloneBottom;
    		cloneBottom = Instantiate(spawnObject.transform, transform.position, Quaternion.Euler(90,0,0)) as GameObject;
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
