using UnityEngine;
using System.Collections;
using System.Collections.Generic;

// If a player looks at Collectable GameObjects long enough, they disappear and increment a score.

// The "code" allows you to increment different scores. There's also optional particle systems before and after collection.

[RequireComponent (typeof(VisionTracker))]
public class Collectable : MonoBehaviour
{
	// The "code" corresponds to the type of score incremented. Anywhere in the project you can type:
	//       Collectable.GetScore(5);
	// to access the number of times a Collectable with code=5 has been collected so far.
	public int code = 0;

	//These threshold correspond to VisionTracker.GetTotalLookAtScore, VisionTracker.GetHistoryScore, and VisionTracker.GetLookAtScore.
	//When any three scores are greater than their threshold, the item is declared collected. Zero means ignore this threshold.
	public float totalThreshold = 0;
	public float historyThreshold = 0;
	public float lookAtThreshold = 0;

	//Optionally, you can set particle systems. Aura is used before collection (always), collected is used after collection.
	public GameObject auraParticlesPrefab, collectedParticlesPrefab;

	private VisionTracker visionTracker;
	private GameObject auraParticles, powParticles;
	private static GameObject particlesContainer;
	private bool isCollected = false;
	private static Dictionary<int,int> collectionScores;

	void Start ()
	{
		if (collectionScores == null)
			collectionScores = new Dictionary<int, int> ();
		if (!collectionScores.ContainsKey (code))
			collectionScores [code] = 0;
		if (particlesContainer == null)
			particlesContainer = new GameObject ("Particles for Collectables");
		visionTracker = GetComponent<VisionTracker> ();

		SetupAura ();
		Verify ();
	}

	public static int GetScore(int collectionCode){
		// Anywhere in the project you can type:
		//       Collectable.GetScore(5);
		// to access the number of times a Collectable with code=5 has been collected so far.
		if (collectionScores == null || !collectionScores.ContainsKey (collectionCode))
			return 0;
		return collectionScores [collectionCode];
	}

	private void SetupAura ()
	{
		if (auraParticlesPrefab != null) {
			auraParticles = Instantiate (auraParticlesPrefab, gameObject.transform.position, Quaternion.identity) as GameObject;
			auraParticles.transform.parent = gameObject.transform;
		}
	}

	private void Verify ()
	{
		if (totalThreshold == 0 && historyThreshold == 0 && lookAtThreshold == 0)
			Debug.LogWarning ("All thresholds on Collectable are set to zero. Impossible to collect! " + gameObject.name);
		if (totalThreshold < 0 || historyThreshold < 0 || lookAtThreshold < 0)
			Debug.LogWarning ("A threshold for Collectable is less than zero. This is weird and probably wrong. " + gameObject.name);
	}

	private bool HasHitThreshold ()
	{
		if (totalThreshold > 0) {
			if (totalThreshold < visionTracker.GetTotalLookAtScore ())
				return true;
		}
		if (historyThreshold > 0) {
			if (historyThreshold < visionTracker.GetHistoryScore ())
				return true;
		}
		if (lookAtThreshold > 0) {
			if (lookAtThreshold < visionTracker.GetLookAtScore ())
				return true;
		}
		return false;
	}

	private void SetCollected ()
	{
		isCollected = true;
		collectionScores [code]++;

		if (auraParticles != null)
			Destroy (auraParticles);
		if (collectedParticlesPrefab != null) {
			powParticles = Instantiate (collectedParticlesPrefab, gameObject.transform.position, Quaternion.identity) as GameObject;
			powParticles.transform.parent = particlesContainer.transform;
			Destroy (powParticles, 10);
		}

		Fish fish = GetComponent<Fish> ();
		if (fish != null)
			fish.fishSchool.CollectFish (fish);

	}

	void Update ()
	{
		if (!isCollected && HasHitThreshold ())
			SetCollected ();
	}
}
















