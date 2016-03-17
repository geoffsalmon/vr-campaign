using UnityEngine;
using System.Collections;

public class FishSchool : MonoBehaviour
{
	public GameObject prefab;
	public int fishCount = 200;
	public float interval = 0.5f; //this is how often each fish will re-orient itself
	public float baseSpeed = 4; //fish swim speed

	//for more info on radii and weights see this academic paper:
	//"Simulating The Collective Behavior of Schooling Fish With A Discrete Stochastic Model" 2006
	public float radiusOfAttraction = 150;
	public float radiusOfOrientation = 60;
	public float radiusOfRepulsion = 4;
	public float weightOfRepulsion = 1.5f;
	public float weightOfOrientation = 1f;
	public float weightOfAttraction = 1.5f;
	public float weightOfSelf = 5;
	private float weightOfOutOfBounds; //this is used when the fish goes below the floor, or above the water

	public GameObject maxHeightGameObject;
	public Terrain minHeightTerrain;
	private float schoolWidth = 50; //octree initialization size, small optimization consideration
	private Fish[] fishies;
	private GameObject fishContainer;
	private Bounds fishBounds;
	private Bounds boundsOfAttraction, boundsOfOrientation, boundsOfRepulsion;
	private BoundsOctree<Fish> octree;

	void Start ()
	{
		if (prefab == null) {
			string defaultPrefab = "test fish";
			Debug.Log ("Swarm was given no fish prefab. Using default '" + defaultPrefab + "'.");
			prefab = Resources.Load (defaultPrefab) as GameObject;
		}
		
		if (maxHeightGameObject == null)
			Debug.LogError ("maxHeightGameObject is null. no max height!");
		if (minHeightTerrain == null)
			Debug.LogError ("minHeightTerrain is null. no min height!");

		weightOfOutOfBounds = (weightOfSelf + weightOfRepulsion + weightOfOrientation + weightOfAttraction)/5;
		SetupFishies ();
		MakeFishies ();

		if (interval > 1)
			Debug.LogError ("Warning: interval greater than one. Might cause weirdness in ApplyZones.");
	}

	private void SetupFishies ()
	{
		fishies = new Fish[fishCount];
		fishContainer = new GameObject ("fishes - " + gameObject.name);
		fishBounds = new Bounds (Vector3.zero, new Vector3 (1, 1, 1)); //fish are 1x1x1 meters, only needed for octree, values don't matter much
		octree = new BoundsOctree<Fish> (schoolWidth, transform.position, 0.1f, 1);

		boundsOfAttraction = new Bounds (Vector3.zero, new Vector3 (radiusOfAttraction, radiusOfAttraction, radiusOfAttraction));
		boundsOfOrientation = new Bounds (Vector3.zero, new Vector3 (radiusOfOrientation, radiusOfOrientation, radiusOfOrientation));
		boundsOfRepulsion = new Bounds (Vector3.zero, new Vector3 (radiusOfRepulsion, radiusOfRepulsion, radiusOfRepulsion));
	}

	private void MakeFishies ()
	{		
		float halfWidth = schoolWidth / 2;
		for (int i=0; i<fishCount; i++) {
			Vector3 startPosition = gameObject.transform.position + new Vector3 (Random.Range (-halfWidth, halfWidth),
			                                                                     Random.Range (-halfWidth, halfWidth),
			                                                                     Random.Range (-halfWidth, halfWidth));
			GameObject go = Instantiate (prefab, startPosition, Random.rotation) as GameObject;
			go.transform.parent = fishContainer.transform;
			fishies [i] = new Fish (go);
			octree.Add (fishies [i], fishBounds);
			StartCoroutine (Cycle (fishies [i]));
		}
	}

	void Update ()
	{
		foreach (Fish fish in fishies)
			fish.Swim ();
	}

	private IEnumerator Cycle (Fish fish)
	{
		yield return new WaitForSeconds (Random.Range (0, interval)); //this initial wait is to spread out fish computation
		while (true) {
			UpdateOctree (fish);
			ApplyZones (fish);
			yield return new WaitForSeconds (interval);
			
		}
	}

	private void ApplyZones (Fish fish)
	{
		//calculate and apply the new orientation this fish should have
		Vector3 selfDirection = fish.gameObject.transform.forward * weightOfSelf;
		Vector3 repulsion = GetNearbyFishVector (fish, boundsOfRepulsion) * weightOfRepulsion; //get the vector that best faces away from very nearby fish
		Vector3 orientation = GetNearbyFishDirection (fish, boundsOfOrientation) * weightOfOrientation; //get the direction that nearby fish are generally facing
		Vector3 attraction = GetNearbyFishVector (fish, boundsOfAttraction) * weightOfAttraction; //get the unit vector that best faces towards all fish except those very far away
		
		Vector3 idealDirection = selfDirection - repulsion + orientation + attraction;

		//if fish is above water or below ground, turn it up/down
		if (maxHeightGameObject != null && fish.gameObject.transform.position.y > maxHeightGameObject.transform.position.y) {
			idealDirection += Vector3.down * weightOfOutOfBounds;
		} else if (minHeightTerrain != null) {
			float minHeight = minHeightTerrain.SampleHeight (fish.gameObject.transform.position)+minHeightTerrain.transform.position.y;
			if (fish.gameObject.transform.position.y < minHeight)
				idealDirection += Vector3.up * weightOfOutOfBounds;
		}
		
		Vector3 newDirection = Vector3.Lerp (fish.gameObject.transform.forward, idealDirection, interval); //go more towards this new direction if interval is high and the fish check less often
		//Vector3 newDirection = idealDirection;
		fish.gameObject.transform.forward = newDirection;
		fish.speed = baseSpeed;
	}

	private Vector3 GetNearbyFishDirection (Fish fish, Bounds bounds)
	{
		//looks at all fish surrounding fish within bounds bounds, and returns the average direction they're facing
		Vector3 direction = Vector3.zero;		
		bounds.center = fish.gameObject.transform.position;
		foreach (Fish otherFish in octree.GetColliding(bounds))
			direction += otherFish.gameObject.transform.forward;
		return direction.normalized;
	}

	private Vector3 GetNearbyFishVector (Fish fish, Bounds bounds)
	{
		//looks at all fish surrounding fish within bounds bounds, and returns a unit vector averaging their positions relative to fish
		Vector3 nearby = Vector3.zero;
		bounds.center = fish.gameObject.transform.position;
		foreach (Fish otherFish in octree.GetColliding(bounds))
			nearby += otherFish.gameObject.transform.position - fish.gameObject.transform.position;
		return nearby.normalized;
	}

	private void UpdateOctree (Fish fish)
	{
		//octree is a data structure to efficiently keep track of which fishies are near other fishies. But it must be updated regularly.
		octree.Remove (fish);
		fishBounds.center = fish.gameObject.transform.position;
		octree.Add (fish, fishBounds);
	}
	
	void OnDrawGizmos ()
	{
		//this lets you see the octree after running the game, but viewing it in scene view
		if (octree != null) {
			octree.DrawAllBounds (); // Draw node boundaries
			octree.DrawAllObjects (); // Mark object positions
		}
	}
}















