using UnityEngine;
using System.Collections;
using System.Collections.Generic;

//Fish controls the movement and appearance of fish. It's attached to every fish in a FishSchool. This class is used internally by FishSchool.

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
		fishType.Verify ();

		previousDirection = transform.forward;
		newDirection = transform.forward;
	}

	public void Update ()
	{
		//Apply the small changes in location and rotation.
		//Also check if a fish recalculation is necessary.
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
		//Calculate the new orientation the fish should face.
		//The fish sums up various influences to re-orient itself.
		// 1) Self. The fish keeps its own orientation somewhat.
		// 2) Repulsion. The fish faces away from fish that are very close to it.
		// 3) Orientation. The fish faces in the same direction as fish that are a medium distance from it.
		// 4) Attraction. The fish faces towards the average position of all fish.
		// 5) Lures. The fish faces towards or away from lures, depending on the lure weight.

		Vector3 selfDirection = transform.forward * fishSchool.weightOfSelf;
		//Get the vector that best faces away from very nearby fish.
		Vector3 repulsion = fishSchool.GetRepulsionAveragePosition (this) * fishSchool.weightOfRepulsion; 
		//Get the direction that nearby fish are generally facing.
		Vector3 orientation = fishSchool.GetOrientationAverageDirection (this) * fishSchool.weightOfOrientation; 
		
		Vector3 attractionDirection = fishSchool.GetAverageFishPosition () - transform.position;
		//Get the unit vector that best faces towards all fish.
		Vector3 attraction = attractionDirection.normalized * fishSchool.weightOfAttraction;
		//Get the vector representing the influence of all lures in the scene on this fish.
		Vector3 lure = fishSchool.GetLureVector (this);

		//Get the vector with a strong influence up or down, if the fish is above water or below ground.
		Vector3 boundary = Vector3.zero;
		if (fishSchool.IsFishTooLow (this)) {
			boundary = Vector3.up * fishSchool.GetOutOfBoundsWeight ();
		} else if (fishSchool.IsFishTooHigh (this)) {
			boundary = Vector3.down * fishSchool.GetOutOfBoundsWeight ();
		}

		//Calculate the direction this fish will gradually face until the next fish calculation.
		newDirection = selfDirection - repulsion + orientation + attraction + lure + boundary;
		previousDirection = transform.forward;
	}

}
