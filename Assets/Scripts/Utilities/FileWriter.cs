using UnityEngine;
using System.Collections;

using System;
using System.IO;

//Sometimes debug output is too huge (time codes of thousands of events).
//This is a simple tool to write it to a text file.

public static class FileWriter 
{
	public static void Write(string data, string filename) 
	{
		using (StreamWriter sw = new StreamWriter(filename)) 
			sw.Write(data);
		Debug.Log ("Wrote data to file: '" + filename + "'.");
	}
}