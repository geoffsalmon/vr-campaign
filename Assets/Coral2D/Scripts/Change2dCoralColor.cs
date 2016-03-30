using UnityEngine;
using System.Collections;

public class Change2dCoralColor : MonoBehaviour {
	
	// Use this for initialization
	void Start () {
		Color newColor = new Color( Random.value, Random.value, Random.value, 1.0f );
		// = newColor;
		//renderer.material.color = newColor;
		Debug.Log(newColor);
		GetComponent<MeshRenderer>().material.color = newColor;
//		Color myColor;
//bool mode = Random.value > 0.5f;
//float rand = Random.value;
//int index = Random.Range( 0, 3 );
//myColor[index] = rand;
//myColor[( index + ( mode ? 1 : 2 ) ) % 3] = 1;
//myColor[( index + ( mode ? 2 : 1 ) ) % 3] = 0;
//renderer.material.color = myColor;
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
