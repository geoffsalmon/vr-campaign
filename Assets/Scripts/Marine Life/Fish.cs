using UnityEngine;
using System.Collections;

public class Fish
{

	public Fish (GameObject fishGameObject)
	{
		this.gameObject = fishGameObject;
	}
		
	public void ChangeColor (Color newColor)
	{
		foreach (GameObject child in ParentChildFunctions.GetAllChildren(gameObject,true)) {
			MeshRenderer meshRenderer = child.GetComponent<MeshRenderer> ();
			if (meshRenderer != null)
				meshRenderer.material.color = newColor;
		}
			
	}
		
	public GameObject gameObject;
	public float speed;

	public void Swim ()
	{
		gameObject.transform.position = gameObject.transform.position + gameObject.transform.forward * speed * Time.deltaTime;
	}

}
