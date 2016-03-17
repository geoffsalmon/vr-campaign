using UnityEngine;
using System.Collections;

public class Fish
{

	public Fish (GameObject go)
	{
		this.go = go;
	}
		
	public void ChangeColor (Color newColor)
	{
		foreach (GameObject child in ParentChildFunctions.GetAllChildren(go,true)) {
			MeshRenderer mr = child.GetComponent<MeshRenderer> ();
			if (mr != null)
				mr.material.color = newColor;
		}
			
	}
		
	public GameObject go;
	public float speed;

}
