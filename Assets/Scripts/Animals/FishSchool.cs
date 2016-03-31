using UnityEngine;
using System.Collections;
using System.Collections.Generic;

// This class can be attached to an empty game object to create a school of fish.

// See FishType for more information on how to describe the fish in a FishSchool.

public class FishSchool : MonoBehaviour
{
	public FishType[] fishTypes;

	//How often fish will recalculate the direction they are orienting themselves to point.
	public float interval = 0.5f;

	//This school of fish will only respond to lures with a matching lureCode. If this lureCode is zero, it matches all lures.
	public int lureCode = 0;
	
	//For more info on radii and weights see comments in Fish.ApplyZones, or Google this academic paper:
	//"Simulating The Collective Behavior of Schooling Fish With A Discrete Stochastic Model" 2006
	public float radiusOfOrientation = 60;
	public float radiusOfRepulsion = 4;
	public float weightOfRepulsion = 1.5f;
	public float weightOfOrientation = 1f;
	public float weightOfAttraction = 1.5f;
	public float weightOfSelf = 5;

	//These objects help define the floor and ceiling boundaries.
	public MovementBoundary floorBoundary, ceilingBoundary;
	
	//This is the weight that a fish will swim up or down, if it is above water or below ground.
	private float weightOfOutOfBounds;
	
	//This is recalculated periodically.
	private Vector3 averageFishPosition;

	//The size of the box where fish will randomly appear to start. Also the octree initialization size.
	private float schoolWidth = 20;
	private List<Fish> fishies;
	private FishLure[] fishLures;
	private GameObject fishContainer;
	private Bounds fishBounds;
	private Bounds boundsOfOrientation, boundsOfRepulsion;
	private BoundsOctree<Fish> octree;
	private bool hasShownDebug = false;
	
	void Start ()
	{
		Fish.debugTimes = new System.Collections.Generic.List<float> ();

		if (fishTypes.Length == 0) {
			string defaultPrefab = "test/test fish";
			Debug.LogWarning ("fish school was given no fish prefab. Using default '" + defaultPrefab + "'.");
			GameObject fishPrefab = Resources.Load (defaultPrefab) as GameObject;
			fishTypes = new FishType[1]{new FishType (fishPrefab)};
		}

		GetFishLures ();
		SetupBoundaries ();
		SetupFishies ();
		MakeFishies ();
		StartCoroutine (CalculateAverageFishPosition ());

		if (interval > 1)
			Debug.LogWarning ("Interval greater than one. Might cause weirdness in ApplyZones.");
	}

	void Update ()
	{
		if (!hasShownDebug && Time.realtimeSinceStartup > 5) {
			//debug logging for hunting down performance kinks
			hasShownDebug = true;
			string text = "";
			foreach (float a in Fish.debugTimes)
				text += a + "\n";
			FileWriter.Write (text, "data.txt");
		}
	}

	private FishLure[] GetFishLures ()
	{
		//fish school only considers lures with a matching lureCode, or if fishSchool.lureCode==0, all lures.
		if (fishLures == null) {
			ArrayList lureArrayList = new ArrayList ();
			foreach (FishLure lure in FindObjectsOfType<FishLure>())
				if (lureCode == 0 || (lure.lureCode == lureCode))
					lureArrayList.Add (lure);

			fishLures = new FishLure[lureArrayList.Count];
			lureArrayList.CopyTo (fishLures);
		}
		return fishLures;
	}

	private void SetupBoundaries ()
	{			
		float lureWeight = 0;
		foreach (FishLure fl in GetFishLures())
			lureWeight += fl.GetWeight ();
		weightOfOutOfBounds = (weightOfSelf + weightOfRepulsion + weightOfOrientation + weightOfAttraction + lureWeight) / 2;

		if (ceilingBoundary == null)
			Debug.LogWarning ("A fish school has no ceiling, so no maximum height. Maybe this is okay. " + gameObject.name);
		else
			ceilingBoundary.Setup (false);
		if (floorBoundary == null)
			Debug.LogWarning ("A fish school has no floor, so no minimum height. Maybe this is okay. " + gameObject.name);
		else
			floorBoundary.Setup (true);
	}

	public float GetOutOfBoundsWeight ()
	{
		return weightOfOutOfBounds;
	}

	public Vector3 GetAverageFishPosition ()
	{
		return averageFishPosition;
	}
	
	private void SetupFishies ()
	{
		int fishCount = 0;
		foreach (FishType ft in fishTypes)
			fishCount += ft.count;
		fishies = new List<Fish> ();
		fishContainer = new GameObject ("fishes - " + gameObject.name);
		fishBounds = new Bounds (Vector3.zero, new Vector3 (0.2f, 0.2f, 0.2f)); //octree needs some fish size, values don't matter much
		octree = new BoundsOctree<Fish> (schoolWidth, transform.position, 0.1f, 1);

		boundsOfOrientation = new Bounds (Vector3.zero, new Vector3 (radiusOfOrientation, radiusOfOrientation, radiusOfOrientation));
		boundsOfRepulsion = new Bounds (Vector3.zero, new Vector3 (radiusOfRepulsion, radiusOfRepulsion, radiusOfRepulsion));
	}
	
	private void MakeFishies ()
	{		
		float halfWidth = schoolWidth / 2;
		foreach (FishType fishType in fishTypes) {
			for (int i=0; i<fishType.count; i++) {
				if (fishType.prefab==null){
					Debug.LogWarning("One of the FishTypes in FishSchool has a null prefab. Oops! " +gameObject);
					continue;
				}
				Vector3 startPosition = gameObject.transform.position + new Vector3 (Random.Range (-halfWidth, halfWidth),
			                                                                     Random.Range (-halfWidth, halfWidth),
			                                                                     Random.Range (-halfWidth, halfWidth));

				GameObject fishGameObject = Instantiate (fishType.prefab, startPosition, Random.rotation) as GameObject;
				fishGameObject.transform.parent = fishContainer.transform;
				Fish fish = fishGameObject.GetComponent<Fish> ();
				if (fish == null)
					fish = fishGameObject.AddComponent<Fish> ();
				fish.Setup (this, fishType);
				fishies.Add (fish);
//			Color newColor = new Color( Random.value, Random.value, Random.value, 1.0f ); // RANDOM COLOR ADDED
//			fishies[i].GetComponent<MeshRenderer>().material.color = newColor; // RANDOM COLOR APPLIED
				octree.Add (fishies [i], fishBounds);
			}
		}
	}
	
	private IEnumerator CalculateAverageFishPosition ()
	{
		while (true) {
			averageFishPosition = Vector3.zero;
			foreach (Fish fish in fishies) {
				averageFishPosition += fish.gameObject.transform.position;
			}
			averageFishPosition /= fishies.Count;
			yield return new WaitForSeconds (interval);
		}
	}

	public Vector3 GetLureVector (Fish fish)
	{
		//This sums up all the lures with a lureCode that matches this FishSchool, returning a vector with lure weights applied.
		Vector3 lure = Vector3.zero;
		foreach (FishLure fishLure in fishLures) {
			if (fishLure.IsFishInRange (fish)) {
				Vector3 diff = fishLure.gameObject.transform.position - fish.gameObject.transform.position;
				lure += diff.normalized * fishLure.GetWeight ();
			}
		}
		return lure;
	}
	
	public Vector3 GetOrientationAverageDirection (Fish fish)
	{
		//Calculates the average direction of all fish within a medium distance to this fish.
		Vector3 direction = Vector3.zero;		
		boundsOfOrientation.center = fish.gameObject.transform.position;
		foreach (Fish otherFish in octree.GetColliding(boundsOfOrientation))
			direction += otherFish.gameObject.transform.forward;
		return direction.normalized;
	}

	public bool IsFishTooLow (Fish fish)
	{
		//This returns true when the fish is below the floor MovementBoundary.
		return floorBoundary != null && floorBoundary.IsBreakingRule (fish.gameObject.transform.position);
	}

	public bool IsFishTooHigh (Fish fish)
	{
		//This returns true when the fish is above the water MovementBoundary.
		return ceilingBoundary != null && ceilingBoundary.IsBreakingRule (fish.gameObject.transform.position);
	}
	
	public Vector3 GetRepulsionAveragePosition (Fish fish)
	{
		//Finds the average position of all fish very near the fish, returns a unit vector in that direction.
		Vector3 nearby = Vector3.zero;
		boundsOfRepulsion.center = fish.gameObject.transform.position;
		foreach (Fish otherFish in octree.GetColliding(boundsOfRepulsion))
			nearby += otherFish.gameObject.transform.position - fish.gameObject.transform.position;
		return nearby.normalized;
	}

	public void CollectFish(Fish fish){
		octree.Remove (fish);
		fish.SetStatus (FishStatuses.collected);
		fishies.Remove (fish);
	}
	
	public void UpdateOctree (Fish fish)
	{
		//Octree is a data structure to efficiently keep track of which fishies are near other fishies. This updates it regularly.
		octree.Remove (fish);
		fishBounds.center = fish.gameObject.transform.position;
		octree.Add (fish, fishBounds);
	}
	
	void OnDrawGizmos ()
	{
		//See the octree in scene view while running the game.
		if (octree != null) {
			octree.DrawAllBounds (); // Draw node boundaries
			octree.DrawAllObjects (); // Mark object positions
		}
	}
}















