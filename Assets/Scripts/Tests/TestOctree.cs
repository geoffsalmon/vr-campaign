using UnityEngine;
using System.Collections;

public class TestOctree : MonoBehaviour {

	public BoundsOctree<GameObject> octree;
	public Ray ray;
	public GameObject cube;
	public Bounds bounds;

	void Start () {
		gameObject.transform.position = Vector3.zero;
		octree = new BoundsOctree<GameObject> (10,transform.position,1,1);
		for(int i=0;i<transform.childCount;i++){
			GameObject child=transform.GetChild(i).gameObject;
			octree.Add (child,child.GetComponent<MeshRenderer>().bounds);
		}
		cube = GameObject.Find ("Cube");

		int size = 10;
		bounds = new Bounds (cube.transform.position,new Vector3(size,size,size));
	}
	
	void OnDrawGizmos() {
		if (octree != null) {
			octree.DrawAllBounds (); // Draw node boundaries
			octree.DrawAllObjects (); // Mark object positions
		
			Gizmos.color = Color.green;
			foreach (GameObject go in octree.GetColliding(ref bounds))
				Gizmos.DrawCube (go.transform.position, Vector3.one);
		}
	}
}
