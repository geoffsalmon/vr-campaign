using UnityEngine;
using System.Collections;

public class FishSchool : MonoBehaviour
{
	public GameObject fishPrefab;
	public int fishCount = 200;
	public float interval = 0.5f; //this is how often each fish will re-orient itself
	public float baseSpeed = 4; //fish swim speed
	
	//for more info on radii and weights see this academic paper:
	//"Simulating The Collective Behavior of Schooling Fish With A Discrete Stochastic Model" 2006
	public float radiusOfOrientation = 60;
	public float radiusOfRepulsion = 4;
	public float weightOfRepulsion = 1.5f;
	public float weightOfOrientation = 1f;
	public float weightOfAttraction = 1.5f;
	public float weightOfSelf = 5;
	private float weightOfOutOfBounds; //this is used when the fish goes below the floor, or above the water
	private Vector3 averageFishPosition;
	
	public GameObject maxHeightGameObject;
	public Terrain minHeightTerrain;
	private float schoolWidth = 50; //octree initialization size, small optimization consideration
	private Fish[] fishies;
	private FishLure[] fishLures;
	private GameObject fishContainer;
	private Bounds fishBounds;
	private Bounds boundsOfOrientation, boundsOfRepulsion;
	private BoundsOctree<Fish> octree;
	
	void Start ()
	{
		if (fishPrefab == null) {
			string defaultPrefab = "test fish";
			Debug.Log ("Swarm was given no fish prefab. Using default '" + defaultPrefab + "'.");
			fishPrefab = Resources.Load (defaultPrefab) as GameObject;
		}
		
		if (maxHeightGameObject == null)
			Debug.Log ("maxHeightGameObject is null. no max height!");
		if (minHeightTerrain == null)
			Debug.Log ("minHeightTerrain is null. no min height!");

		weightOfOutOfBounds = (weightOfSelf + weightOfRepulsion + weightOfOrientation + weightOfAttraction)/5;
		fishLures = FindObjectsOfType<FishLure> ();
		SetupFishies ();
		MakeFishies ();
		StartCoroutine (CalculateAverageFishPosition ());

		if (interval > 1)
			Debug.LogError ("Warning: interval greater than one. Might cause weirdness in ApplyZones.");
	}

	public float GetOutOfBoundsWeight(){
		return weightOfOutOfBounds;
	}

	public Vector3 GetAverageFishPosition(){
		return averageFishPosition;
	}
	
	private void SetupFishies ()
	{
		fishies = new Fish[fishCount];
		fishContainer = new GameObject ("fishes - " + gameObject.name);
		fishBounds = new Bounds (Vector3.zero, new Vector3 (1, 1, 1)); //fish are 1x1x1 meters, only needed for octree, values don't matter much
		octree = new BoundsOctree<Fish> (schoolWidth, transform.position, 0.1f, 1);

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
			GameObject fishGameObject = Instantiate (fishPrefab, startPosition, Random.rotation) as GameObject;
			fishGameObject.transform.parent = fishContainer.transform;
			Fish fish=fishGameObject.GetComponent<Fish>();
			if(fish==null)
				fish=fishGameObject.AddComponent<Fish>();
			fish.Setup(this, baseSpeed);
			fishies [i] = fish;
			octree.Add (fishies [i], fishBounds);
		}
	}
	
	private IEnumerator CalculateAverageFishPosition(){
		while (true) {
			averageFishPosition=Vector3.zero;
			foreach(Fish fish in fishies)
				averageFishPosition+=fish.gameObject.transform.position;
			averageFishPosition/=fishies.Length;
			yield return new WaitForSeconds(interval);
		}
	}

	public Vector3 GetLureVector(Fish fish){
		Vector3 lure = Vector3.zero;
		foreach (FishLure fishLure in fishLures) {
			if(fishLure.IsFishInRange(fish)){
				Vector3 diff=fishLure.gameObject.transform.position-fish.gameObject.transform.position;
				lure+= diff* fishLure.GetWeight();
			}
		}
		return lure;
	}

	private float GetMaxHeight(){
		return maxHeightGameObject.transform.position.y-5;
	}
	
	private float GetMinHeight(Fish fish){
		return minHeightTerrain.SampleHeight (fish.gameObject.transform.position) + minHeightTerrain.transform.position.y + 5;
	}
	
	public Vector3 GetOrientationAverageDirection (Fish fish)
	{
		//looks at all fish surrounding fish within bounds bounds, and returns the average direction they're facing
		Vector3 direction = Vector3.zero;		
		boundsOfOrientation.center = fish.gameObject.transform.position;
		foreach (Fish otherFish in octree.GetColliding(boundsOfOrientation))
			direction += otherFish.gameObject.transform.forward;
		return direction.normalized;
	}

	public bool IsFishBelowGround(Fish fish){
		return minHeightTerrain != null && fish.gameObject.transform.position.y < GetMinHeight (fish);
	}

	public bool IsFishAboveWater(Fish fish){
		return maxHeightGameObject != null && transform.position.y > GetMaxHeight ();
	}
	
	public Vector3 GetRepulsionAveragePosition (Fish fish)
	{
		//looks at all fish surrounding fish within boundsOfRepulsion, and returns a unit vector averaging their positions relative to fish
		Vector3 nearby = Vector3.zero;
		boundsOfRepulsion.center = fish.gameObject.transform.position;
		foreach (Fish otherFish in octree.GetColliding(boundsOfRepulsion))
			nearby += otherFish.gameObject.transform.position - fish.gameObject.transform.position;
		return nearby.normalized;
	}
	
	public void UpdateOctree (Fish fish)
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















