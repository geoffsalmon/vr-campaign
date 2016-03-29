using UnityEngine;
using System.Collections;

[System.Serializable]
public class FishType{
	public GameObject prefab;
	public int count=1;
	public float speed=4;
	
	public FishType(GameObject prefab){
		this.prefab = prefab;
		
		if (speed == 0)
			speed = 4;
		if (count == 0)
			count = 1;
	}
}