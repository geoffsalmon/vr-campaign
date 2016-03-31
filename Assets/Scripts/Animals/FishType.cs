using UnityEngine;
using System.Collections;

//This is used by FishSchool to describe the types of fish it has.

// prefab can be any GameObject you want to be a fish.
// speed (typically around 4) is how fast these fish move.
// count is how many of this fishType will be created in FishSchool.

[System.Serializable]
public class FishType{
	public GameObject prefab;
	public int count;
	public float speed;
	
	public FishType(GameObject prefab){
		this.prefab = prefab;
	}

	public void Verify(){
		if (count < 1) {
			Debug.LogWarning ("A fish type in this scene has a count less than 1. This is probably bad! Setting to default. " + prefab);
			count=50;
		}
		if (speed <= 0) {
			Debug.LogWarning ("A fish type in this scene has a speed less than or equal to zero. This is probably bad! Setting to default. " + prefab);
			speed=4;
		}
	}
}