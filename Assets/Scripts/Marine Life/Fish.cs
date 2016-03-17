using UnityEngine;
using System.Collections;

public class Fish
{
	public GameObject gameObject;
	public float speed;
	
	private Vector3 previousDirection,newDirection;
	private float directionTimer = 0;

	public Fish (GameObject fishGameObject, float speed)
	{
		this.gameObject = fishGameObject;
		this.speed = speed;
		ChangeColor (new Color(0.3f,0.3f,0.5f));
	}
		
	public void ChangeColor (Color newColor)
	{
		foreach (GameObject child in ParentChildFunctions.GetAllChildren(gameObject,true)) {
			MeshRenderer meshRenderer = child.GetComponent<MeshRenderer> ();
			if (meshRenderer != null)
				meshRenderer.material.color = newColor;
		}
			
	}

	public void Swim (float interval)
	{
		directionTimer += Time.deltaTime;
		gameObject.transform.forward = Vector3.Lerp (previousDirection, newDirection, directionTimer/interval);

		gameObject.transform.position = gameObject.transform.position + gameObject.transform.forward * speed * Time.deltaTime;
	}

	public void SetNewFacingDirection(Vector3 newFacingDirection){
		previousDirection = gameObject.transform.forward;
		newDirection = newFacingDirection;
		directionTimer = 0;
	}

}
