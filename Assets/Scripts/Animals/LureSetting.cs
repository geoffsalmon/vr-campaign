using UnityEngine;
using System.Collections;

//This is used by FishLure to describe its behaviour.
//A FishLure will cycle through numerous LureSettings, changing its values to equal the LureSetting it is currently on.
//See FishLure for more information.

[System.Serializable]
public class LureSetting{
	//The weight of attraction for this lure. A value of 1 is fairly strong. Negatives repel fish.
	public float weight=1;

	//The range at which fish will be attracted to or repelled by this lure. Set to zero means infinite range.
	public float range=0;

	//How long this LureSetting will last, before FishLure moves on to the next LureSetting.
	public float duration=1;

	private float sqrRange=0;
	
	public float GetSqrRange(){
		if (sqrRange == 0)
			sqrRange = range * range;
		return sqrRange;
	}
}
