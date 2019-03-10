using UnityEngine;
using System.Collections;

//It is common to create a class to contain all of your
//extension methods. This class must be static.
public static class ExtensionMethods
{
    /// <summary>
    /// Creates new Vector3 from Vector3Ints values. Not free, use sparingly.
    /// </summary>
    /// <returns>The vector3.</returns>
    /// <param name="inVec">In vec.</param>
    public static Vector3 ToVector3(this Vector3Int inVec)
    {
        Vector3 vector3 = new Vector3(inVec.x, inVec.y, inVec.z);
        return vector3;
    }
}