using UnityEngine;
using System.Collections;

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
			count=1;
		}
		if (speed <= 0) {
			Debug.LogWarning ("A fish type in this scene has a speed less than or equal to zero. This is probably bad! Setting to default. " + prefab);
			speed=4;
		}
	}
}