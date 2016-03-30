using UnityEngine;
using System.Collections;

[System.Serializable]
public class LureSetting{
	public float weight=1;
	public float range=0;
	public float duration=1;
	private float sqrRange=0;
	private Color gizmoColor;

	public float GetSqrRange(){
		if (sqrRange == 0)
			sqrRange = range * range;
		return sqrRange;
	}
}

public class FishLure : MonoBehaviour {
	public LureSetting[] lureSettings;
	public int lureCode=0;
	private int index=0;
	private float timer = 0;

	private float minWeight,maxWeight;

	public void Start(){
		minWeight = lureSettings [0].weight;
		maxWeight = minWeight;
		foreach (LureSetting ls in lureSettings) {
			if (ls.weight<minWeight)
				minWeight=ls.weight;
			if(ls.weight>maxWeight)
				maxWeight=ls.weight;
		}
	}

	public float GetWeight(){
		return lureSettings [index].weight;
	}

	public float GetRange(){
		return lureSettings [index].range;
	}

	public float GetSqrRange(){
		return lureSettings [index].GetSqrRange();
	}

	public bool IsFishInRange(Fish fish){
		if (GetRange () == 0)
			return true;
		Vector3 diff = fish.gameObject.transform.position - transform.position;
		return diff.sqrMagnitude < GetSqrRange ();
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
