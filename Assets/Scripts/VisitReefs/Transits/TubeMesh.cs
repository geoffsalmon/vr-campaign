using UnityEngine;
using System.Collections;

// Generates a tube mesh programatically using code from http://wiki.unity3d.com/index.php/ProceduralPrimitives#C.23_-_Tube
// This only generates the inside triangles of the tube.
public class TubeMesh {

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
}
