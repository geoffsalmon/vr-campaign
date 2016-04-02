using UnityEngine;
using System.Collections;

// Attaching FishLure to a GameObject makes fish attracted to it or repelled by it.
// Attach negative weighted lures to a shark to repel fish, or positive weighted lures to points in space, or breadcrumbs, to attract fish.
// It's also possible to make the FishLure change its strength/weights in cycle, and to make only certain schools respond to certain lures.

// A FishSchool will only use a lure if FishSchool.lureCode == FishLure.lureCode

// lureSettings is how you set the lure to change over time. For example:
// lureSettings[0].weight=1   lureSettings[0].duration=10
// lureSettings[1].weight=0   lureSettings[1].duration=5
// lureSettings[2].weight=0.1   lureSettings[2].duration=50
// In this case, the lure starts with weight 1 for 10 seconds. Then it has weight 0 for 5 seconds, and finally weight 0.1 for 50 seconds. Then it loops back to the start.

// Having a single LureSetting means it will always be on that setting.

public class FishLure : MonoBehaviour {
	public LureSetting[] lureSettings;
	public int lureCode=0;
	private int index=0;
	private float timer = 0;

	public float GetWeight(){
		return lureSettings [index].weight;
	}

	public float GetRange(){
		return lureSettings [index].range;
	}

	public float GetSqrRange(){
		return lureSettings [index].GetSqrRange();
	}

	public bool IsFishInRange(Vector3 fishPos, out Vector3 toLure){
		toLure = transform.position - fishPos;
		if (GetRange () == 0)
			return true;
		return toLure.sqrMagnitude < GetSqrRange ();
	}

	public bool IsFishInRange(Vector3 fishPos){
		if (GetRange () == 0)
			return true;
		Vector3 toLure = transform.position - fishPos;
		return toLure.sqrMagnitude < GetSqrRange ();
	}

	public void Update(){
		timer += Time.deltaTime;
		if (lureSettings.Length>1 && timer > lureSettings [index].duration) {
			index=(index+1)%lureSettings.Length;
			timer=0;
		}
	}

	void OnDrawGizmos(){
			Gizmos.color = Color.green;
			Gizmos.DrawSphere (transform.position, 1);		
	}
}
