using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Utilities
{
    public static Vector3 FlattenVectorY(Vector3 vector)
	{
		return new Vector3(vector.x, 0, vector.z);
	}
	public static Vector3 FlattenVectorX(Vector3 vector) 
	{ 
		return new Vector3(0, vector.y, vector.z); 
	}
	public static Vector3 FlattenVectorZ(Vector3 vector)
	{
		return new Vector3(vector.x, vector.y, 0);
	}
	public static Vector3 ComponentVectorY(Vector3 vector)
	{
		return new Vector3(0, vector.y, 0);
	}
	public static Vector3 ComponentVectorX(Vector3 vector)
	{
		return new Vector3(vector.x, 0, 0);
	}
	public static Vector3 ComponentVectorZ(Vector3 vector)
	{
		return new Vector3(0, 0, vector.z);
	}
}
