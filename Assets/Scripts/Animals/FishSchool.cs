using UnityEngine;
using System.Collections;
using System.Collections.Generic;

// This class can be attached to an empty game object to create a school of fish.

// See FishType for more information on how to describe the fish in a FishSchool.

public class FishInfo {
	public GameObject gameObject;
	public Transform transform;
	public float speed;
	public Quaternion previousDirection;
	public Quaternion newDirection;
	public float intervalStart;

	public FishInfo(GameObject fish, FishType fishType) {
		gameObject = fish;
		transform = fish.transform;
		speed = fishType.speed;
		previousDirection = newDirection = transform.localRotation;
		intervalStart = Time.time;
	}
}

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
	private List<FishInfo> fishies;
	private FishLure[] fishLures;
	private GameObject fishContainer;
	private Bounds fishBounds;
	private Bounds collideBounds;
	private BoundsOctree<FishInfo> octree;
	private List<FishInfo> scratchList;

	//State for updating the newDirection of each fish once per interval, multiple fish per frame
	private float intervalStartTime;
	private int updateChunkSize;
	private int chunkSizeMax;
	private int updateStartIndex;
	private bool intervalUpdatesFinished = false;
	
	void Start ()
	{
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

		intervalStartTime = Time.time;
		// roughly guess a chunk size assuming 30fps
		updateChunkSize = Mathf.CeilToInt(fishies.Count / (30.0f * interval));
		// set a max chunk size so that a low frame rate doesn't lead to larger updates leading to lower framerate, etc.
		chunkSizeMax = Mathf.CeilToInt(fishies.Count / (15.0f * interval));

		updateStartIndex = 0;
	}

	void Update ()
	{
		int n = fishies.Count;
		if (n == 0)
			return;
		float time = Time.time;
		float delta = Time.deltaTime;

		// Update fish rotations and positions
		for (int i = 0; i < n; i++) {
			FishInfo info = fishies[i];

			float t = (time - info.intervalStart) / interval;
			Quaternion dir = Quaternion.Slerp(info.previousDirection, info.newDirection, t);
			info.transform.localRotation = dir;
			Vector3 localForward = dir * Vector3.forward;
			info.transform.localPosition += localForward * info.speed * delta;
		}

		// The direction each fish is turning towards is recalulated once in every interval. We
		// update the direction for only a subset of the fish in each call to Update.

		if (updateStartIndex >= n) {
			// Have updated all fish directions
			float actualInterval = time - intervalStartTime;
			if (!intervalUpdatesFinished) {
				// When we first finish ApplyZones on all fish in the interval,
				// recalculate the number to call ApplyZones on each frame such
				// so that fish updates are spread throughout the interval.
				intervalUpdatesFinished = true;

				// Recalculate the number to update such that each fish is
				// updated roughly once per interval
				int oldChunkSize = updateChunkSize;
				updateChunkSize = Mathf.CeilToInt(actualInterval / interval * updateChunkSize);
				if (updateChunkSize > chunkSizeMax)
					updateChunkSize = chunkSizeMax;
			}

			// Even though we've ApplyZoned all the fish, wait until interval has actually
			// expired before starting next one. This avoids updating directions too quickly.
			if (actualInterval < interval) {
				return;
			}
			// Start new interval
			intervalStartTime = time;
			updateStartIndex = 0;
		}

		// Update the newDirection for at most updateChunkSize fish.
		int updateEndIndex = updateStartIndex + updateChunkSize;
		if (updateEndIndex > n)
			updateEndIndex = n;
		for (int i = updateStartIndex; i < updateEndIndex; i++) {
			FishInfo info = fishies[i];
			UpdateOctree(info);
			ApplyZones(info);
			info.intervalStart = time;
		}
		updateStartIndex = updateEndIndex; // record index to start direction update from next time
	}

	private void ApplyZones (FishInfo info)
	{
		//Calculate the new orientation the fish should face.
		//The fish sums up various influences to re-orient itself.
		// 1) Self. The fish keeps its own orientation somewhat.
		// 2) Repulsion. The fish faces away from fish that are very close to it.
		// 3) Orientation. The fish faces in the same direction as fish that are a medium distance from it.
		// 4) Attraction. The fish faces towards the average position of all fish.
		// 5) Lures. The fish faces towards or away from lures, depending on the lure weight.

		//vector that best faces towards very nearby fish.
		Vector3 repulsion;
		//The direction that nearby fish are generally facing.
		Vector3 orientation;
		GetRepulsionAndOrientation(info, out repulsion, out orientation);
		repulsion = repulsion * weightOfRepulsion;
		orientation = orientation * weightOfOrientation;

		Vector3 attractionDirection = GetAverageFishPosition () - info.transform.localPosition;
		//Get the unit vector that best faces towards all fish.
		Vector3 attraction = attractionDirection.normalized * weightOfAttraction;
		//Get the vector representing the influence of all lures in the scene on this fish.
		Vector3 lure = GetLureVector (info);

		//Get the vector with a strong influence up or down, if the fish is above water or below ground.
		Vector3 boundary = Vector3.zero;
		if (IsFishTooLow (info)) {
			boundary = Vector3.up * weightOfOutOfBounds;
		} else if (IsFishTooHigh (info)) {
			boundary = Vector3.down * weightOfOutOfBounds;
		}

		//Calculate the direction this fish will gradually face until the next fish calculation.
		Quaternion dir = info.transform.localRotation;
		Vector3 selfDirection = (dir * Vector3.forward)* weightOfSelf;
		info.newDirection = Quaternion.LookRotation(selfDirection - repulsion + orientation + attraction + lure + boundary);
		info.previousDirection = dir;
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
		fishies = new List<FishInfo> (fishCount);
		scratchList = new List<FishInfo> (fishCount);
		fishContainer = new GameObject ("fishes - " + gameObject.name);
		fishBounds = new Bounds (Vector3.zero, new Vector3 (0.2f, 0.2f, 0.2f)); //octree needs some fish size, values don't matter much
		octree = new BoundsOctree<FishInfo> (schoolWidth, Vector3.zero, 0.1f, 1);

		float r = Mathf.Max(radiusOfOrientation, radiusOfRepulsion);
		collideBounds = new Bounds (Vector3.zero, new Vector3 (r, r, r));
	}
	
	private void MakeFishies ()
	{		
		float halfWidth = schoolWidth / 2;
		foreach (FishType fishType in fishTypes) {
			fishType.Verify();
			for (int i=0; i<fishType.count; i++) {
				if (fishType.prefab==null){
					Debug.LogWarning("One of the FishTypes in FishSchool has a null prefab. Oops! " +gameObject);
					continue;
				}
				Vector3 startPosition = new Vector3 (Random.Range (-halfWidth, halfWidth),
				                                     Random.Range (-halfWidth, halfWidth),
				                                     Random.Range (-halfWidth, halfWidth));

				GameObject fish = Instantiate (fishType.prefab, startPosition, Random.rotation) as GameObject;
				fish.transform.parent = fishContainer.transform;

				FishInfo info = new FishInfo(fish, fishType);
				fishies.Add(info);
//			Color newColor = new Color( Random.value, Random.value, Random.value, 1.0f ); // RANDOM COLOR ADDED
//			fishies[i].GetComponent<MeshRenderer>().material.color = newColor; // RANDOM COLOR APPLIED
				fishBounds.center = fish.transform.localPosition;
				octree.Add (info, fishBounds);
			}
		}
	}
	
	private IEnumerator CalculateAverageFishPosition ()
	{
		while (true) {
			averageFishPosition = Vector3.zero;
			foreach (FishInfo fish in fishies) {
				averageFishPosition += fish.transform.localPosition;
			}
			averageFishPosition /= fishies.Count;
			yield return new WaitForSeconds (interval);
		}
	}

	private Vector3 GetLureVector (FishInfo fish)
	{
		//This sums up all the lures with a lureCode that matches this FishSchool, returning a vector with lure weights applied.
		Vector3 lure = Vector3.zero;
		Vector3 toLure;
		foreach (FishLure fishLure in fishLures) {
			if (fishLure.enabled && fishLure.IsFishInRange (fish.transform.position, out toLure)) {
				lure += toLure.normalized * fishLure.GetWeight ();
			}
		}
		return lure;
	}
	
	private void GetRepulsionAndOrientation (FishInfo fish, out Vector3 repulsion, out Vector3 orientation)
	{
		Vector3 fishPos = fish.transform.localPosition;

		// sum of vectors from this fish to fish within the repulsion radius
		Vector3 repulsionSum = Vector3.zero;
		// sum of direction vectors of fish within the orientation radius
		Vector3 orientationSum = Vector3.zero;

		float repulsionSq = radiusOfRepulsion * radiusOfRepulsion;
		float orientationSq = radiusOfOrientation * radiusOfOrientation;

		// Get all fish withing orientation zone. This assumes the radius of repulsion
		// is smaller than the radius of orientation.
		collideBounds.center = fishPos;
		scratchList.Clear();
		octree.GetColliding(ref collideBounds, scratchList);

		foreach (FishInfo otherFish in scratchList) {
			Vector3 fishDir = otherFish.transform.localPosition - fishPos;
			float distSq = fishDir.sqrMagnitude;
			if (distSq < repulsionSq) {
				repulsionSum += fishDir;
			}
			if (distSq < orientationSq) {
				orientationSum += otherFish.transform.localRotation * Vector3.forward;
			}
		}

		repulsion = repulsionSum.normalized;
		orientation = orientationSum.normalized;
	}

	private bool IsFishTooLow (FishInfo fish)
	{
		//This returns true when the fish is below the floor MovementBoundary.
		return floorBoundary != null && floorBoundary.IsBreakingRule (fish.transform.position);
	}

	private bool IsFishTooHigh (FishInfo fish)
	{
		//This returns true when the fish is above the water MovementBoundary.
		return ceilingBoundary != null && ceilingBoundary.IsBreakingRule (fish.transform.position);
	}

	public void CollectFish(GameObject fish){
		// find matching matching FishInfo
		FishInfo info = null;
		int i = 0;
		int n = fishies.Count;
		for (i = 0; i < n; i++) {
			if (fishies[i].gameObject == fish) {
				info = fishies[i];
				break;
			}
		}
		if (info != null) {
			octree.Remove(info);
			// Move last fish to fill space instead of shifting all fish. This might cause
			// a fish to skip an ApplyZones in an interval.
			fishies[i] = fishies[n - 1];
			fishies.RemoveAt(n - 1);
		}
		Destroy(fish);
	}
	
	private void UpdateOctree (FishInfo fish)
	{
		//Octree is a data structure to efficiently keep track of which fishies are near other fishies. This updates it regularly.
		octree.Remove (fish);
		fishBounds.center = fish.transform.localPosition;
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















