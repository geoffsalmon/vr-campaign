using UnityEngine;
using System.Collections;

public enum Inequality
{
	greaterThan,
	lessThan
}

//this class describes a boundary for marine life to swim away from if they cross it.
//it can be a min or max height, above or below a y value, or above or below a gameobject
[System.Serializable]
public class MovementBoundary
{
	private Inequality isInsideZoneInequality;
	public float height = 0;
	public GameObject heightGameObject;
	public Terrain heightTerrain;
	public float heightPadding = 0; //if the terrain/gameobject height is 4 and padding is 3, the boundary height will be 4+3
	private int settingCount=0;

	public void Setup(bool isFloor){
		settingCount = 0;
		if (height != 0)
			settingCount++;
		if (heightGameObject != null)
			settingCount++;
		if (heightTerrain != null)
			settingCount++;

		isInsideZoneInequality = isFloor ? Inequality.greaterThan : Inequality.lessThan;

		VerifyHeightRule ();
	}

	public bool IsBreakingRule (Vector3 position)
	{
		//returns true if, for example, isInsideZoneInequality=greaterThan, and position.y is greater than the height rule/gameObject/terrain
		if (!IsGoodHeightRule ())
			return false;

		float boundaryHeight = height;
		if (heightTerrain != null) {
			boundaryHeight = heightTerrain.SampleHeight (position) + heightTerrain.transform.position.y + heightPadding;
		} else if (heightGameObject != null) {
			boundaryHeight = heightGameObject.transform.position.y;
		}

		bool isBreakingRule = position.y < boundaryHeight;
		if (isInsideZoneInequality == Inequality.lessThan)
			isBreakingRule = !isBreakingRule;
		return isBreakingRule;
	}

	private static bool hasThrownWarning = false;

	private void VerifyHeightRule ()
	{
		if (!Debug.isDebugBuild || hasThrownWarning)
			return;

		if (!IsGoodHeightRule ()) {
			hasThrownWarning = true;
			Debug.LogWarning ("Someone set up a MovementHeightRule with weird settings. You must choose one of the three: height, heightTerrain, heightGameObject. Two of those three should be null.");
		}
	}

	public bool IsGoodHeightRule ()
	{
		return settingCount == 1;
	}
}
