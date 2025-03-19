using UnityEngine;

public static class VectorUtils
{
	public static Vector3 Abs(Vector3 vector)
	{
		return new Vector3(Mathf.Abs(vector.x), Mathf.Abs(vector.y), Mathf.Abs(vector.z));
	}
}
