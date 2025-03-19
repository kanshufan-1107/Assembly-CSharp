using UnityEngine;

public class OrientedBounds
{
	public Vector3[] Extents;

	public Vector3 Origin;

	public Vector3 CenterOffset;

	public Vector3 GetTrueCenterPosition()
	{
		return Origin + CenterOffset;
	}

	public Vector3 GetSize()
	{
		if (Extents.Length < 3)
		{
			return Vector3.zero;
		}
		return new Vector3(Extents[0].magnitude * 2f, Extents[1].magnitude * 2f, Extents[2].magnitude * 2f);
	}
}
