using System.Collections;
using UnityEngine;


// Script to distribute the bubble particles instances around a sphere
public class BubbleSphereDistribution : MonoBehaviour {

	public int NumberOfPoints = 4; // Number of particle emitters
	public float SphereDiameter = 2.0f; // Diameter of the sphere
    public Camera PlayerCamera;
 
    void Start () {
		
        // Create a sphere primitive and assign its component values
		GameObject innerSphere = GameObject.CreatePrimitive(PrimitiveType.Sphere); 
		innerSphere.transform.localScale = innerSphere.transform.localScale * (SphereDiameter * 2); 
		innerSphere.transform.name = "Inner Sphere";
	    //innerSphere.GetComponent<MeshRenderer>().enabled = false;
		
        // Call the function to distribute the number of points evenly across the whole sphere
		Vector3[] myPoints = DistributePointsOnSphere(NumberOfPoints);
		
        // For each point -- attach a cube (this is used with vision tracker) as well as the bubble particle emitter
		foreach (Vector3 point in myPoints)
		{
			GameObject outerSphere = GameObject.CreatePrimitive(PrimitiveType.Cube);
			outerSphere.transform.position = point*SphereDiameter;
		    outerSphere.transform.localScale = outerSphere.transform.localScale*0.25f;
            //outerSphere.GetComponent<MeshRenderer>().enabled = false;

            // Attach the bubble particle emitter to each cube and make sure that it looks at the camera
            // ---------- For now I have kept the camera at origin, however this needs to be changed -------
            GameObject bubbles = Instantiate(Resources.Load("BubbleParticles", typeof(GameObject))) as GameObject;
		    if (bubbles != null)
		    {
                bubbles.transform.position = point * SphereDiameter;
                bubbles.transform.parent = outerSphere.transform;
                bubbles.transform.LookAt(PlayerCamera.transform);
            }
		}
	}
 

    // Distribute points evenly on a sphere
	Vector3[] DistributePointsOnSphere(int nPoints)
    {
        int divisions = (int)(Mathf.Sqrt(nPoints / 5.0f) + 0.5f);

        ArrayList points = new ArrayList();

        points.Add(new Vector3(0, -1, 0));

        float angle = Mathf.PI / 2 / divisions;

        for (int iLat = -divisions + 1; iLat < divisions; ++iLat)
        {
            float lat = iLat * angle;

            float circumference = 2 * Mathf.PI * Mathf.Cos(lat);
            int steps = (int)(circumference / angle);

            float baseAngle = lat / 2;

            for (int iLon = 0; iLon < steps; ++iLon)
            {
                float lon = baseAngle + iLon * 2 * Mathf.PI / steps;
                points.Add(new Vector3(Mathf.Sin(lon) * Mathf.Cos(lat), Mathf.Sin(lat), Mathf.Cos(lon) * Mathf.Cos(lat)));
            }
        }

        points.Add(new Vector3(0, 1, 0));

        return points.ToArray(typeof(Vector3)) as Vector3[];
    }
}
