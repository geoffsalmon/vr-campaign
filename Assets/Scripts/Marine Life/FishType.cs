using UnityEngine;
using System.Collections;

[System.Serializable]
public class FishType{
	public GameObject prefab;
	public int probability=1;
	public float speed=4;
	
	public FishType(GameObject prefab){
		this.prefab = prefab;
		
		if (speed == 0)
			speed = 4;
		if (probability == 0)
			probability = 1;
	}
}