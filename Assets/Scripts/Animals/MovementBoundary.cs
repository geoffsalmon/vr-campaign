using UnityEngine;
using System.Collections;

public enum Inequality
{
	greaterThan,
	lessThan
}

//This class is used by FishSchool to describe a boundary for marine life to swim away from if they cross it.
//It can be a min or max height, above or below a gameobject, above or below Unity terrain, or above or below a MeshCollider.
[System.Serializable]
public class MovementBoundary
{
	public float height = 0;
	public GameObject heightGameObject;
	public Terrain heightTerrain;
	public MeshCollider meshCollider;

	//if the padding is X, the boundary height will be X units stricter.
	public float heightPadding = 0; 
	private int settingCount=0;
	private Inequality isInsideZoneInequality;
	private static bool hasThrownWarning = false;
	private static bool hasThrownHeightWarning = false;
	private const int INFINITY = 10000;

	public void Setup(bool isFloor){
		settingCount = 0;
		if (height != 0)
			settingCount++;
		if (heightGameObject != null)
			settingCount++;
		if (heightTerrain != null)
			settingCount++;
		if (meshCollider != null)
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
		} else if (meshCollider != null) {
			boundaryHeight = GetHeightFromMeshCollider(position);
		}

		bool isBreakingRule = position.y < boundaryHeight;
		if (isInsideZoneInequality == Inequality.lessThan)
			isBreakingRule = !isBreakingRule;
		return isBreakingRule;
	}

	private float GetHeightFromMeshCollider(Vector3 position){
		foreach (RaycastHit hit in Physics.RaycastAll(position+Vector3.up*INFINITY,Vector3.down))
			if (hit.collider == meshCollider)
				return hit.point.y;

		//infiniteHeight is very high if this is a water barrier, or very low if this is a ground barrier.
		float infiniteHeight = position.y + INFINITY * (isInsideZoneInequality == Inequality.lessThan ? 1 : -1);
		if (Debug.isDebugBuild && hasThrownHeightWarning) {
			hasThrownHeightWarning = true;
			Debug.LogWarning ("A fish failed to get its boundary from the mesh collider. It probably swam far away?");
		}
		return infiniteHeight;
	}

	private void VerifyHeightRule ()
	{
		if (!Debug.isDebugBuild || hasThrownWarning)
			return;

		if (!IsGoodHeightRule ()) {
			hasThrownWarning = true;
			Debug.LogWarning ("Someone set up a MovementHeightRule with weird settings. You must choose one of the four: height, heightTerrain, heightGameObject, meshCollider. The others should be null.");
		}
	}

	public bool IsGoodHeightRule ()
	{
		return settingCount == 1;
	}
}
