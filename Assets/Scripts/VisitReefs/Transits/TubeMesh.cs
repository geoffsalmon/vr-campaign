using UnityEngine;
using System.Collections;
using System.Collections.Generic;

// Generates a tube mesh programatically using code from http://wiki.unity3d.com/index.php/ProceduralPrimitives#C.23_-_Tube
// This only generates the inside triangles of the tube.

public struct TubePoint
{
	public Vector3 position;
	public Vector3 derivative;
	public float radius;

	public TubePoint(Vector3 position, Vector3 derivative, float radius) {
		this.position = position;
		this.derivative = derivative;
		this.radius = radius;
	}
}

public class TubeMesh {

	// Fills mesh with a single tube with 2 ends, one at y=0 and one at y=1
	public static void generateTube(Mesh mesh) {
		mesh.Clear();

		float height = 1f;
		int nbSides = 24;

		// Outter shell is at radius1 + radius2 / 2, inner shell at radius1 - radius2 / 2
		float bottomRadius1 = 1f;
		float bottomRadius2 = .15f; 
		float topRadius1 = 1f;
		float topRadius2 = .15f;

		int nbVerticesSides = nbSides * 2 + 2;
		#region Vertices

		Vector3[] vertices = new Vector3[nbVerticesSides];
		int vert = 0;
		float _2pi = Mathf.PI * 2f;

		// Bottom cap
		int sideCounter = 0;
		// Sides (in)
		sideCounter = 0;
		while (vert < vertices.Length )
		{
			sideCounter = sideCounter == nbSides ? 0 : sideCounter;

			float r1 = (float)(sideCounter++) / nbSides * _2pi;
			float cos = Mathf.Cos(r1);
			float sin = Mathf.Sin(r1);

			vertices[vert] = new Vector3(cos * (topRadius1 - topRadius2 * .5f), height, sin * (topRadius1 - topRadius2 * .5f));
			vertices[vert + 1] = new Vector3(cos * (bottomRadius1 - bottomRadius2 * .5f), 0, sin * (bottomRadius1 - bottomRadius2 * .5f));
			vert += 2;
		}
		#endregion

		#region Normales

		Vector3[] normales = new Vector3[vertices.Length];
		vert = 0;

		// Sides (in)
		sideCounter = 0;
		while (vert < vertices.Length )
		{
			sideCounter = sideCounter == nbSides ? 0 : sideCounter;

			float r1 = (float)(sideCounter++) / nbSides * _2pi;

			normales[vert] = -(new Vector3(Mathf.Cos(r1), 0f, Mathf.Sin(r1)));
			normales[vert+1] = normales[vert];
			vert+=2;
		}
		#endregion

		#region UVs
		Vector2[] uvs = new Vector2[vertices.Length];

		vert = 0;
		// Sides (in)
		sideCounter = 0;
		while (vert < vertices.Length )
		{
			float t = (float)(sideCounter++) / nbSides;
			uvs[ vert++ ] = new Vector2( t, 0f );
			uvs[ vert++ ] = new Vector2( t, 1f );
		}
		#endregion

		#region Triangles
		int nbFace = nbSides * 4;
		int nbTriangles = nbFace * 2;
		int nbIndexes = nbTriangles * 3;
		int[] triangles = new int[nbIndexes];

		// Bottom cap
		int i = 0;
		sideCounter = 0;
		// Sides (in)
		while( sideCounter < nbSides )
		{
			int current = sideCounter * 2;
			int next = sideCounter * 2 + 2;

			triangles[ i++ ] = next + 1;
			triangles[ i++ ] = next;
			triangles[ i++ ] = current;

			triangles[ i++ ] = current + 1;
			triangles[ i++ ] = next + 1;
			triangles[ i++ ] = current;

			sideCounter++;
		}
		#endregion

		mesh.vertices = vertices;
		mesh.normals = normales;
		mesh.uv = uvs;
		mesh.triangles = triangles;

		mesh.RecalculateBounds();
		mesh.Optimize();
	}


	public static void generateTubes(Mesh mesh, List<TubePoint> points, int numSides = 24) {
		mesh.Clear();
		int numVertices = numSides * points.Count;

		Vector3[] vertices = new Vector3[numVertices];
		Vector3[] normals = new Vector3[numVertices];
		Vector2[] uvs = new Vector2[numVertices];

		int numFace = numSides * (points.Count - 1);
		int numTriangles = numFace * 2;
		int numIndexes = numTriangles * 3;
		int[] triangles = new int[numIndexes];

		int vert = 0;

		int i;
		int j;
		int n;
		for (i = 0, n = points.Count; i < n; i++) {
			// make a ring of vertices around each point oriented perpendicular to the derivative
			TubePoint point = points[i];

			float u = ((float) i) / (n - 1);
			// sweep a vector around the derivative vector
			Vector3 firstRadiusVector = Vector3.Cross(point.derivative, Vector3.up).normalized;
			if (firstRadiusVector.magnitude < 0.00001f) {
				// what to do if tube is straight up?
				firstRadiusVector = Vector3.Cross(point.derivative, Vector3.right).normalized;
			}
			//Debug.Log("New Ring");
			for (j = 0; j < numSides; j++) {
				float angle = 360f * j / numSides;
				Vector3 radVector = Quaternion.AngleAxis(angle, point.derivative) * firstRadiusVector * point.radius;

				vertices[vert] = radVector + point.position;
				normals[vert] = -radVector;
				//Debug.Log("radVector=" + radVector + " vert=" + vertices[vert]);
				uvs[vert] = new Vector2(u, ((float)j) / numSides);
				vert++;
			}
		}
		//Debug.Log("Created " + vert + " vertices of " + numVertices);

		int tri = 0;
		for (i = 0, n = points.Count-1; i < n; i++) {
			for (j = 0; j < numSides; j++) {
				int curr = i*numSides + j;
				int next = i*numSides + ((j+1) % numSides);

				triangles[tri++] = curr;
				triangles[tri++] = curr + numSides;
				triangles[tri++] = next + numSides;

				triangles[tri++] = curr;
				triangles[tri++] = next + numSides;
				triangles[tri++] = next;
			}
		}
		//Debug.Log("Created " + (tri/3.0f) + " triangles of expected " + numTriangles);

		mesh.vertices = vertices;
		mesh.normals = normals;
		mesh.uv = uvs;
		mesh.triangles = triangles;

		mesh.RecalculateBounds();
		mesh.Optimize();
	}

	public static void test(GameObject gameobject) {
		MeshFilter filter = gameobject.AddComponent<MeshFilter>();
		List<TubePoint> points = new List<TubePoint>();
		points.Add(new TubePoint(Vector3.zero, Vector3.right, 1));
		points.Add(new TubePoint(new Vector3(1f, 0.25f, 0f), new Vector3(1f, 0, 0), 1));
		points.Add(new TubePoint(new Vector3(2f, -0.25f, 0f), new Vector3(1f, 0, 0), 1));
		//points.Add(new TubePoint(new Vector3(3f, 0f, 0f), new Vector3(1f, 0, 0), 1));

		TubeMesh.generateTubes(filter.mesh, points, 8);

		MeshRenderer renderer = gameobject.AddComponent<MeshRenderer>();
		renderer.receiveShadows = false;
		renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
		renderer.material.color = Color.red;
	}
}
