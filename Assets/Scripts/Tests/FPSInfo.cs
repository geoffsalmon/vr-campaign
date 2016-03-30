using UnityEngine;
using System.Collections;
using UnityEngine.UI;

//attach this script, once, to any gameobject and there will be a debug canvas with an FPS counter, updated every second
public class FPSInfo : MonoBehaviour {
	private GameObject canvas;
	private Text text;

	private float interval=1;
	private float timer=0;
	private int frameCount;

	void Start () {
		GameObject prefab = Resources.Load ("test/Debug Canvas") as GameObject;
		canvas = Instantiate (prefab) as GameObject;
		text = canvas.transform.GetChild (0).gameObject.GetComponent<Text> ();
	}
	
	// Update is called once per frame
	void Update () {
		frameCount++;
		timer += Time.deltaTime;
		if (timer > interval) {
			float fps=Mathf.Round((float)frameCount/interval);
			text.text=fps+" FPS";
			timer=0;
			frameCount=0;
			Debug.Log (fps+" FPS");
		}
	}
}
