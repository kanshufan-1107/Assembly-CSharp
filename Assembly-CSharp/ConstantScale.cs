using UnityEngine;

public class ConstantScale : MonoBehaviour
{
	public Vector3 scale = Vector3.one;

	public bool everyFrame;

	private bool isItFirstIteration = true;

	private void LateUpdate()
	{
		if (!everyFrame)
		{
			if (!isItFirstIteration)
			{
				return;
			}
			isItFirstIteration = false;
		}
		Vector3 globalScale = Vector3.one;
		if (base.transform.parent != null)
		{
			globalScale = base.transform.parent.transform.lossyScale;
		}
		if (globalScale.x + globalScale.y + globalScale.z == 0f)
		{
			globalScale = new Vector3(1E-05f, 1E-05f, 1E-05f);
		}
		base.transform.localScale = Vector3.Scale(new Vector3(1f / globalScale.x, 1f / globalScale.y, 1f / globalScale.z), scale);
	}
}
