using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Fish : MonoBehaviour
{
	private float speed;
	private FishSchool fishSchool;
	private Vector3 previousDirection, newDirection;
	private float timeSinceApplyZones = 0;

	public static List<float> debugTimes;

	public void Setup (FishSchool fishSchool, FishType fishType)
	{
		timeSinceApplyZones = Random.Range (0f, fishSchool.interval);
		this.fishSchool = fishSchool;
		speed = fishType.speed;
		ChangeColor (new Color (0.3f, 0.3f, 0.5f));
		fishType.Verify ();
	}
		
	public void ChangeColor (Color newColor)
	{
		foreach (GameObject child in ParentChildFunctions.GetAllChildren(gameObject,true)) {
			MeshRenderer meshRenderer = child.GetComponent<MeshRenderer> ();
			if (meshRenderer != null)
				meshRenderer.material.color = newColor;
		}
			
	}

	public void Update ()
	{
		if (fishSchool != null) {
			timeSinceApplyZones += Time.deltaTime;
			gameObject.transform.forward = Vector3.Lerp (previousDirection, newDirection, timeSinceApplyZones / fishSchool.interval);
			gameObject.transform.position = gameObject.transform.position + gameObject.transform.forward * speed * Time.deltaTime;

			if(timeSinceApplyZones>fishSchool.interval){
				fishSchool.UpdateOctree (this);
				ApplyZones ();
				timeSinceApplyZones = 0; //this must come after ApplyZones
			}
		}
	}
	
	private void ApplyZones ()
	{
		debugTimes.Add (Time.realtimeSinceStartup);
		//calculate and apply the new orientation this fish should have
		Vector3 selfDirection = transform.forward * fishSchool.weightOfSelf;
		Vector3 repulsion = fishSchool.GetRepulsionAveragePosition (this) * fishSchool.weightOfRepulsion; //get the vector that best faces away from very nearby fish
		Vector3 orientation = fishSchool.GetOrientationAverageDirection (this) * fishSchool.weightOfOrientation; //get the direction that nearby fish are generally facing
		
		Vector3 attractionDirection = fishSchool.GetAverageFishPosition () - transform.position;
		Vector3 attraction = attractionDirection.normalized * fishSchool.weightOfAttraction; //get the unit vector that best faces towards all fish except those very far away
		Vector3 lure = fishSchool.GetLureVector (this);

		Vector3 boundary = Vector3.zero;
		//if fish is above water or below ground, turn it up/down
		if (fishSchool.IsFishTooLow (this)) {
			boundary = Vector3.up * fishSchool.GetOutOfBoundsWeight ();
		} else if (fishSchool.IsFishTooHigh (this)) {
			boundary = Vector3.down * fishSchool.GetOutOfBoundsWeight ();
		}

		Vector3 idealDirection = selfDirection - repulsion + orientation + attraction + lure + boundary;

		newDirection = idealDirection;
		previousDirection = transform.forward;
	}

}
