using UnityEngine;
using System.Collections;

public class FishSchool : MonoBehaviour
{


	public GameObject prefab;
	public int fishCount = 200;
	public float interval = 0.5f;
	public float baseSpeed = 4;

	public float radiusOfAttraction = 150;
	public float radiusOfOrientation = 60;
	public float radiusOfRepulsion = 4;

	public float weightOfRepulsion = 1.5f;
	public float weightOfOrientation = 1f;
	public float weightOfAttraction = 1.5f;
	public float weightOfSelf = 5;

	private float fishSize = 1;
	private float schoolWidth = 50;
	private Fish[] fishes;
	private float catchUpSpeed;
	private GameObject fishContainer;
	private Bounds fishBounds;
	private Bounds boundsOfAttraction, boundsOfOrientation, boundsOfRepulsion;
	private BoundsOctree<Fish> octree;

	void Start ()
	{
		if (prefab == null) {
			string d = "test fish";
			Debug.Log ("Swarm was given no fish prefab. Using default '" + d + "'.");
			prefab = Resources.Load (d) as GameObject;
		}
		Setup ();
		MakeFishes ();

		if (interval > 1)
			Debug.LogError ("interval greater than one: might cause weirdness in ApplyZones.");
	}

	private void Setup ()
	{
		catchUpSpeed = baseSpeed * 1.5f;
		fishes = new Fish[fishCount];
		fishContainer = new GameObject ("fishes - " + gameObject.name);
		fishBounds = new Bounds (Vector3.zero, new Vector3 (fishSize, fishSize, fishSize));
		octree = new BoundsOctree<Fish> (schoolWidth, transform.position, 0.1f, 1);

		boundsOfAttraction = new Bounds (Vector3.zero, new Vector3 (radiusOfAttraction, radiusOfAttraction, radiusOfAttraction));
		boundsOfOrientation = new Bounds (Vector3.zero, new Vector3 (radiusOfOrientation, radiusOfOrientation, radiusOfOrientation));
		boundsOfRepulsion = new Bounds (Vector3.zero, new Vector3 (radiusOfRepulsion, radiusOfRepulsion, radiusOfRepulsion));
	}

	private void MakeFishes ()
	{		
		float halfWidth = schoolWidth / 2;
		for (int i=0; i<fishCount; i++) {
			Vector3 startPosition = gameObject.transform.position + new Vector3 (Random.Range (-halfWidth, halfWidth),
			                                                                     Random.Range (-halfWidth, halfWidth),
			                                                                     Random.Range (-halfWidth, halfWidth));
			GameObject go = Instantiate (prefab, startPosition, Random.rotation) as GameObject;
			go.transform.parent = fishContainer.transform;
			fishes [i] = new Fish (go);
			octree.Add (fishes [i], fishBounds);
			StartCoroutine (Cycle (fishes [i]));
		}
	}
	
	// Update is called once per frame
	void Update ()
	{
		foreach (Fish fish in fishes)
			fish.go.transform.position = fish.go.transform.position + fish.go.transform.forward* fish.speed * Time.deltaTime;
	}

	private IEnumerator Cycle (Fish fish)
	{
		yield return new WaitForSeconds (Random.Range (0, interval));
		while (true) {
			UpdateOctree (fish);
			ApplyZones (fish);
			yield return new WaitForSeconds (interval);
			
		}
	}

	private void ApplyZones (Fish fish)
	{
		Vector3 selfDirection = fish.go.transform.forward * weightOfSelf;
		Vector3 repulsion = GetNearbyFishVector (fish, boundsOfRepulsion) * weightOfRepulsion;
		Vector3 orientation = GetNearbyFishDirection (fish, boundsOfOrientation) * weightOfOrientation;
		Vector3 unitAttraction = GetNearbyFishVector (fish, boundsOfAttraction);
		Vector3 attraction = unitAttraction * weightOfAttraction;

		Vector3 idealDirection = selfDirection - repulsion + orientation + attraction;
		Vector3 newDirection = Vector3.Lerp (fish.go.transform.forward, idealDirection, interval);
		//Vector3 newDirection = idealDirection;
		fish.go.transform.forward = newDirection;
		fish.speed = baseSpeed;
		
	}

	private Vector3 GetNearbyFishDirection (Fish fish, Bounds bounds)
	{
		//looks at all fish surrounding fish within bounds bounds, and returns the average direction they're facing
		Vector3 direction = Vector3.zero;		
		bounds.center = fish.go.transform.position;
		foreach (Fish otherFish in octree.GetColliding(bounds))
			direction += otherFish.go.transform.forward;
		return direction.normalized;
	}

	private Vector3 GetNearbyFishVector (Fish fish, Bounds bounds)
	{
		//looks at all fish surrounding fish within bounds bounds, and returns a unit vector averaging their positions relative to fish
		Vector3 nearby = Vector3.zero;
		bounds.center = fish.go.transform.position;
		foreach (Fish otherFish in octree.GetColliding(bounds))
			nearby += otherFish.go.transform.position - fish.go.transform.position;
		return nearby.normalized;
	}

	private void UpdateOctree (Fish fish)
	{
		octree.Remove (fish);
		fishBounds.center = fish.go.transform.position;
		octree.Add (fish, fishBounds);
	}
	
	void OnDrawGizmos ()
	{
		if (octree != null) {
			octree.DrawAllBounds (); // Draw node boundaries
			octree.DrawAllObjects (); // Mark object positions
		}
	}
}















