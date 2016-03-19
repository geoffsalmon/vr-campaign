using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class FPSInfo : MonoBehaviour {
	private GameObject canvas;
	private Text text;

	private float interval=1;
	private float timer=0;
	private int frameCount;

	void Start () {
		GameObject prefab = Resources.Load ("Debug Canvas") as GameObject;
		canvas = Instantiate (prefab) as GameObject;
		text = canvas.transform.GetChild (0).gameObject.GetComponent<Text> ();
	}
	
	// Update is called once per frame
	void Update () {
		frameCount++;
		timer += Time.deltaTime;
		if (timer > interval) {
			text.text=Mathf.Round((float)frameCount/interval)+" FPS";
			timer=0;
			frameCount=0;
		}
	}
}
