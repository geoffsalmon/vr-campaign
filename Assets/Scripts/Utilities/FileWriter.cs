using UnityEngine;
using System.Collections;

using System;
using System.IO;

public static class FileWriter 
{
	public static void Write(string data, string filename) 
	{
		using (StreamWriter sw = new StreamWriter(filename)) 
			sw.Write(data);
		Debug.Log ("Wrote data to file: '" + filename + "'.");
	}
}