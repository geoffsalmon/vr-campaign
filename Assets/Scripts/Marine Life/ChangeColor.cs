using UnityEngine;
using System.Collections;

public class ChangeColor : MonoBehaviour {
	public Color newColor = new Color( Random.value, Random.value, Random.value, 1.0f );

	// Use this for initialization
	void Start () {
		// = newColor;
		//renderer.material.color = newColor;
		Debug.Log(newColor);
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
