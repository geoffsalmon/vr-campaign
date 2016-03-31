using UnityEngine;
using System.Collections;

// Attach this component to any gameobject, even one with nested meshes inside it, to decorate its exterior with corals.

// Make sure that the center of the parent game object is not floating in empty space. Otherwise many corals may be generated on that same point.

public class CoralDecorator : MonoBehaviour
{
	//How many corals to generate.
	public int coralCount = 50;
	//Information on corals to generate, see CoralType for more information.
	public CoralType[] coralTypes;

	private int coralLikelihoodTotal = 0;
	private ArrayList addedMeshColliders;
	private int failCount = 0;
	private Vector3 lastNormal;
	private const int INFINITY = 10000;
	private static GameObject coralContainer;
	private static int coralCloneCount = 0;

	void Start ()
	{
		if (coralContainer == null)
			coralContainer = new GameObject ("Corals");

		if (coralTypes.Length > 0) {
			CountCoralLikelihood ();
			AddMeshColliders ();
			MakeCorals ();
			DisableAddedMeshColliders ();
		} else {
			Debug.LogWarning ("CoralDecorator aborted, empty CoralTypes.");
		}
	}

	private void CountCoralLikelihood ()
	{
		foreach (CoralType coralType in coralTypes) {
			if (coralType.prefab == null) {
				Debug.LogWarning ("A CoralType has a null prefab. You probably didn't set up CoralDecorator properly.");
				continue;
			}
			coralLikelihoodTotal += coralType.likelihood;
		}
	}

	private void DisableAddedMeshColliders ()
	{
		foreach (MeshCollider mc in addedMeshColliders)
			mc.enabled = false;
	}

	private void AddMeshColliders ()
	{
		addedMeshColliders = new ArrayList ();

		foreach (GameObject go in ParentChildFunctions.GetAllChildren(gameObject,true)) {
			MeshCollider mc = go.GetComponent<MeshCollider> ();
			if (mc == null) {
				mc = go.AddComponent<MeshCollider> ();
				addedMeshColliders.Add (mc);
			}
		}
	}

	private bool IsPartOfThisCoral (GameObject child)
	{
		//Returns true if the game object is a child of this CoralDecorate script.
		Transform parent = child.transform;
		while (parent!=null) {
			if (parent.gameObject.GetComponent<CoralDecorator> () == this)
				return true;
			parent = parent.parent;
		}
		return false;
	}

	private void MakeCorals ()
	{
		for (int i=0; i<coralCount; i++) {
			CoralType coralType = GetRandomCoralType ();

			GameObject coral = Instantiate (coralType.prefab) as GameObject;

			coralCloneCount++;			
			coral.name = "coral " + coralCloneCount;
			SetCoralTransform (coral, coralType);
		}
	}

	private Vector3 GetRandomCoralPosition ()
	{
		Vector3 direction = Random.onUnitSphere;
		Vector3 infiniteOutside = transform.position + direction * INFINITY;
		Vector3 inwards = direction * -1;

		//find the point farthest points from the center of the coral decorator that lies on part of this coral decorator
		//necessary because Physics.RaycastAll does not return the hits in order
		Vector3 farthestPosition = gameObject.transform.position;
		float biggestDistance = 0;
		foreach (RaycastHit hit in Physics.RaycastAll(infiniteOutside,inwards)) {
			if (IsPartOfThisCoral (hit.collider.gameObject)) {
				Vector3 radius = hit.point - gameObject.transform.position;
				if (radius.sqrMagnitude > biggestDistance) {
					biggestDistance = radius.sqrMagnitude;
					farthestPosition = hit.point;
					lastNormal = hit.normal;
				}
			}
		}
		return farthestPosition;
	}

	private void SetCoralTransform (GameObject coral, CoralType coralType)
	{
		coral.transform.position = GetRandomCoralPosition ();

		//note: in some cases this uses lastNormal which is the normal from the last time GetRandomCoralPosition was called
		Vector3 lookDirection;
		if (coralType.coralDirection == CoralDirections.normal) {
			lookDirection = lastNormal;
		} else if (coralType.coralDirection == CoralDirections.up) {
			lookDirection = Vector3.up;
		} else {
			lookDirection = Random.onUnitSphere;
		}

		Vector3 lookAtPoint = coral.transform.position - gameObject.transform.position + lookDirection * INFINITY;
		coral.transform.LookAt (lookAtPoint);
		if (coralType.coralDirection == CoralDirections.up)
			coral.transform.RotateAround (coral.transform.position, Vector3.up, Random.Range (0, 360));

		coral.transform.parent = coralContainer.transform;
	}

	private CoralType GetRandomCoralType ()
	{
		int a = Random.Range (0, coralLikelihoodTotal);
		CoralType coralType = coralTypes [0];
		foreach (CoralType ct in coralTypes) {
			if (a < ct.likelihood) {
				coralType = ct;
				break;
			}
			a -= ct.likelihood;
		}
		return coralType;
	}
}













