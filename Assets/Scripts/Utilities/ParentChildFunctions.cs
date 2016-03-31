using UnityEngine;
using System.Collections;
using System.Collections.Generic;

// This generic script has useful functions for iterating recursively over parents and children of unity GameObjects.

public static class ParentChildFunctions
{
	//Returns a list of all children, recursively, under a GameObject.
    public static ArrayList GetAllChildren(GameObject parentGameObject, bool includeParent = false)
    {
        string[] excludeSubstrings = new string[0];
        return GetAllChildren(parentGameObject, excludeSubstrings, includeParent);
    }

	//Returns a list of all parents, recursively, above a GameObject.
    public static List<GameObject> GetAllParents(GameObject childGameObject)
    {
        List<GameObject> parents = new List<GameObject>();
        while (childGameObject.transform.parent != null)
        {
            GameObject parent = childGameObject.transform.parent.gameObject;
            parents.Add(parent);
            childGameObject = parent;
        }
        return parents;
    }

	//Turn on or off all the colliders of the children of some GameObject.
	//You'd think just turning off Collider would work, but I seem to recall having some trouble there.
    public static void SetCollidersOfChildren(GameObject parentGameObject, bool isColliderEnabled, bool includeParent = false)
    {
        foreach (GameObject child in GetAllChildren(parentGameObject, includeParent))
        {
            if (child.GetComponent<MeshCollider>() != null)
                child.GetComponent<MeshCollider>().enabled = isColliderEnabled;
            if (child.GetComponent<Collider>() != null)
                child.GetComponent<Collider>().enabled = isColliderEnabled;
            if (child.GetComponent<SphereCollider>() != null)
                child.GetComponent<SphereCollider>().enabled = isColliderEnabled;
            if (child.GetComponent<BoxCollider>() != null)
                child.GetComponent<BoxCollider>().enabled = isColliderEnabled;
        }
    }

	//Returns a list of all children, recursively, under a GameObject.
	//excludes all objects and their children if their name contains any string in excludeSubstrings
    public static ArrayList GetAllChildren(GameObject parentGameObject, string[] excludeSubstrings, bool includeParent = false)
    {
        ArrayList children = new ArrayList();

        if (includeParent)
            children.Add(parentGameObject);

        for (int i = 0; i < parentGameObject.transform.childCount; i++)
        {
            GameObject child = parentGameObject.transform.GetChild(i).gameObject;
            bool excludeChild = false;
            foreach (string substring in excludeSubstrings)
            {
                if (child.name.Contains(substring))
                {
                    excludeChild = true;
                    break;
                }
            }
            if (excludeChild)
                continue;

            children.Add(child);
            if (child.transform.childCount > 0)
                children.AddRange(GetAllChildren(child, excludeSubstrings, false));
        }
        return children;
    }
}
