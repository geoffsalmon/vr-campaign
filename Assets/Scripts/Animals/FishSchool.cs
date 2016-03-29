using UnityEngine;
using System.Collections;

public class FishSchool : MonoBehaviour
{
	public FishType[] fishTypes;
	public int fishCount = 200;
	public float interval = 0.5f; //this is how often each fish will re-orient itself
	public int lureCode = 0;
	
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
	public MovementBoundary floorBoundary, ceilingBoundary;
	private float schoolWidth = 50; //octree initialization size, small optimization consideration
	private Fish[] fishies;
	private FishLure[] fishLures;
	private GameObject fishContainer;
	private Bounds fishBounds;
	private Bounds boundsOfOrientation, boundsOfRepulsion;
	private BoundsOctree<Fish> octree;
	private int nextFishWeightCounter=0;
	private int nextFishIndex=0;
	
	void Start ()
	{
		if (fishTypes.Length==0) {
			string defaultPrefab = "test fish";
			Debug.LogWarning ("fish school was given no fish prefab. Using default '" + defaultPrefab + "'.");
			GameObject fishPrefab = Resources.Load (defaultPrefab) as GameObject;
			fishTypes=new FishType[1]{new FishType(fishPrefab)};
		}

		GetFishLures ();
		SetupBoundaries ();
		SetupFishies ();
		MakeFishies ();
		StartCoroutine (CalculateAverageFishPosition ());

		if (interval > 1)
			Debug.LogWarning ("Interval greater than one. Might cause weirdness in ApplyZones.");
	}

	private FishLure[] GetFishLures(){
		//fish school only considers lures with a matching lureCode, or if fishSchool.lureCode==0, all lures.
		if (fishLures == null) {
			ArrayList lureArrayList=new ArrayList();
			foreach(FishLure lure in FindObjectsOfType<FishLure>())
				if(lureCode==0 || (lure.lureCode==lureCode))
					lureArrayList.Add (lure);

			fishLures = new FishLure[lureArrayList.Count];
			lureArrayList.CopyTo(fishLures);
		}
		return fishLures;
	}

	private void SetupBoundaries ()
	{			
		float lureWeight = 0;
		foreach (FishLure fl in GetFishLures())
			lureWeight += fl.GetWeight ();
		weightOfOutOfBounds = (weightOfSelf + weightOfRepulsion + weightOfOrientation + weightOfAttraction+lureWeight)/2;

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
		fishies = new Fish[fishCount];
		fishContainer = new GameObject ("fishes - " + gameObject.name);
		fishBounds = new Bounds (Vector3.zero, new Vector3 (0.2f, 0.2f, 0.2f)); //octree needs some fish size, values don't matter much
		octree = new BoundsOctree<Fish> (schoolWidth, transform.position, 0.1f, 1);

		boundsOfOrientation = new Bounds (Vector3.zero, new Vector3 (radiusOfOrientation, radiusOfOrientation, radiusOfOrientation));
		boundsOfRepulsion = new Bounds (Vector3.zero, new Vector3 (radiusOfRepulsion, radiusOfRepulsion, radiusOfRepulsion));
	}

	private FishType GetNextFishType(){
		FishType fishType = fishTypes [nextFishIndex];
		nextFishWeightCounter += 1;
		if (nextFishWeightCounter > fishType.count) {
			nextFishWeightCounter=0;
			nextFishIndex=(nextFishIndex+1)%fishTypes.Length;
		}

		return fishTypes [nextFishIndex];
	}
	
	private void MakeFishies ()
	{		
		float halfWidth = schoolWidth / 2;
		for (int i=0; i<fishCount; i++) {
			Vector3 startPosition = gameObject.transform.position + new Vector3 (Random.Range (-halfWidth, halfWidth),
			                                                                     Random.Range (-halfWidth, halfWidth),
			                                                                     Random.Range (-halfWidth, halfWidth));

			FishType fishType=GetNextFishType();

			GameObject fishGameObject = Instantiate (fishType.prefab, startPosition, Random.rotation) as GameObject;
			fishGameObject.transform.parent = fishContainer.transform;
			Fish fish = fishGameObject.GetComponent<Fish> ();
			if (fish == null)
				fish = fishGameObject.AddComponent<Fish> ();
			fish.Setup (this, fishType);
			fishies [i] = fish;
//			Color newColor = new Color( Random.value, Random.value, Random.value, 1.0f ); // RANDOM COLOR ADDED
//			fishies[i].GetComponent<MeshRenderer>().material.color = newColor; // RANDOM COLOR APPLIED
			octree.Add (fishies [i], fishBounds);
		}
	}
	
	private IEnumerator CalculateAverageFishPosition ()
	{
		while (true) {
			averageFishPosition = Vector3.zero;
			foreach (Fish fish in fishies)
				averageFishPosition += fish.gameObject.transform.position;
			averageFishPosition /= fishies.Length;
			yield return new WaitForSeconds (interval);
		}
	}

	public Vector3 GetLureVector (Fish fish)
	{
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
		//looks at all fish surrounding fish within bounds bounds, and returns the average direction they're facing
		Vector3 direction = Vector3.zero;		
		boundsOfOrientation.center = fish.gameObject.transform.position;
		foreach (Fish otherFish in octree.GetColliding(boundsOfOrientation))
			direction += otherFish.gameObject.transform.forward;
		return direction.normalized;
	}

	public bool IsFishTooLow (Fish fish)
	{
		//where returning false typically means do no extra action
		return floorBoundary!=null && floorBoundary.IsBreakingRule(fish.gameObject.transform.position);
	}

	public bool IsFishTooHigh (Fish fish)
	{
		//where returning false typically means do no extra action
		return ceilingBoundary != null && ceilingBoundary.IsBreakingRule(fish.gameObject.transform.position);
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















