using UnityEngine;
using System.Collections;

public class CoralDecorator : MonoBehaviour
{
	public int coralCount = 50;
	public CoralType[] coralTypes;
	private int coralLikelihoodTotal = 0;
	private ArrayList addedMeshColliders;
	private int failCount = 0;
	private Vector3 lastNormal;
	private const int INFINITY = 10000;
	private static GameObject coralContainer;
	private static int coralCloneCount=0;

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

	private bool IsPartOfThisCoral (GameObject go)
	{
		Transform parent = go.transform;
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
			coral.name="coral "+coralCloneCount;
			coral.transform.parent=coralContainer.transform;
			SetCoralTransform (coral, coralType);
		}
	}

	private Vector3 GetRandomCoralPosition ()
	{
		Vector3 direction = Random.onUnitSphere;
		Vector3 infiniteOutside = transform.position + direction * INFINITY;
		Vector3 inwards = direction * -1;
		
		Vector3 position = Vector3.zero;
		Vector3 iterativePosition = infiniteOutside;
		RaycastHit hit;
		int counter = 0;
		while (true) {
			if(!Physics.Raycast(iterativePosition,inwards,out hit))
				break;
			if(counter>100){
				Debug.Log ("super counter oops");
				break;
			}
			iterativePosition=hit.point;
			Debug.Log (iterativePosition+ " new coral="+coralCloneCount+" hit coral="+hit.collider.gameObject.name);
			counter++;

			if(IsPartOfThisCoral(hit.collider.gameObject)){
				position = hit.point;
				lastNormal = hit.normal;
				break;
			}
		}

		return position;
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













