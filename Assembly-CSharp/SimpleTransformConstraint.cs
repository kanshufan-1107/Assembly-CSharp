using UnityEngine;

[ExecuteInEditMode]
public class SimpleTransformConstraint : MonoBehaviour
{
	public bool useLateUpdate;

	public int currentParent;

	public Transform[] parents;

	public bool position = true;

	public bool rotation = true;

	public bool scale = true;

	private void Update()
	{
		if (!useLateUpdate && AreParentsValid())
		{
			if (position)
			{
				base.transform.position = parents[currentParent].position;
			}
			if (rotation)
			{
				base.transform.rotation = parents[currentParent].rotation;
			}
			if (scale)
			{
				base.transform.localScale = parents[currentParent].localScale;
			}
		}
	}

	private void LateUpdate()
	{
		if (useLateUpdate && AreParentsValid())
		{
			if (position)
			{
				base.transform.position = parents[currentParent].position;
			}
			if (rotation)
			{
				base.transform.rotation = parents[currentParent].rotation;
			}
			if (scale)
			{
				base.transform.localScale = parents[currentParent].localScale;
			}
		}
	}

	private bool AreParentsValid()
	{
		if (parents != null && parents.Length != 0 && currentParent < parents.Length)
		{
			return parents[currentParent] != null;
		}
		return false;
	}
}
