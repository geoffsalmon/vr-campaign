using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class TestPlayerLookHistory : MonoBehaviour
{
	private Text debugText;
	void Start(){
		GameObject prefab = Resources.Load ("test/Debug Canvas") as GameObject;
		GameObject canvas = Instantiate (prefab) as GameObject;
		debugText = canvas.transform.GetChild (0).gameObject.GetComponent<Text> ();
		
		if (debugText == null)
			Debug.LogError ("could not find vision debug text.");
	}
	
	// Update is called once per frame
	void Update ()
	{
		if (debugText == null)
			return;
		
		string text = "";
		foreach (PlayerLookHistory plh in FindObjectsOfType<PlayerLookHistory>()) {
			string p = plh.GetAverageLookPosition (10).ToString("F2");
			string d = plh.GetAverageLookDirection ().ToString("F2");
			text += "PlayerLookHistory position=" + p + " direction=" + d;
		}
		debugText.text = text;
	}
}
