using UnityEngine;
using System.Collections;

public class Coral : MonoBehaviour
{
	public Vector3 minScale, maxScale;
	public Color[] possibleColors;
	private bool hasGivenColliderWarning=false;

	void Start ()
	{
		transform.localScale = Vector3.Lerp (minScale, maxScale, Random.Range (0f, 1f));
		SetColor (GetRandomColor ());

		if (!hasGivenColliderWarning && GetComponent<Collider> () != null) {
			hasGivenColliderWarning=true;
			Debug.LogWarning("Corals should not have a collider. This wrecks performance, and possibly coral generation code. "+gameObject.name);
		}
	}

	private Color GetRandomColor ()
	{
		if (possibleColors.Length == 0) {
			Debug.LogWarning ("Coral has no possible colors... using default green. " + gameObject);
			return Color.green;
		}
		return possibleColors [Random.Range (0, possibleColors.Length)];
	}

	private void SetColor (Color newColor)
	{
		foreach (GameObject child in ParentChildFunctions.GetAllChildren(gameObject,true)) {
			MeshRenderer mr = child.GetComponent<MeshRenderer> ();
			if (mr != null) {
				foreach (Material m in mr.materials)
					m.color = newColor;
			}
		}
	}
}
