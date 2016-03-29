using UnityEngine;
using System.Collections;

public class BubbleRespawn : MonoBehaviour {
//	public Animation anim;
	// Use this for initialization
	void Start () {
//		anim = GetComponent<Animation>();
//		anim.wrapMode = WrapMode.Once;
	}
	
	// Update is called once per frame
	void Update () {
		//anim.Play();
	}
	public void popping(){
		GetComponent<Animation>().Play();
	}
}
