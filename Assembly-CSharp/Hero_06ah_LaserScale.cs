using UnityEngine;

[ExecuteInEditMode]
public class Hero_06ah_LaserScale : MonoBehaviour
{
	[SerializeField]
	private GameObject BlowObj;

	[SerializeField]
	[Range(0f, 1f)]
	private float ScaleMult = 1f;

	private float m_parentScale = 1f;

	private void Awake()
	{
		float totalScale = 1f;
		Transform parent = base.transform.parent;
		while (parent != null)
		{
			totalScale *= parent.localScale.z;
			parent = parent.parent;
		}
		if (totalScale == 0f)
		{
			m_parentScale = 1f;
		}
		else
		{
			m_parentScale = 1f / totalScale;
		}
	}

	private void Update()
	{
		UpdateObj();
	}

	private void UpdateObj()
	{
		if (!(BlowObj == null))
		{
			float blowLength = (base.transform.position - BlowObj.transform.position).magnitude;
			base.transform.localScale = new Vector3(base.transform.localScale.x, base.transform.localScale.y, blowLength * ScaleMult * m_parentScale);
		}
	}
}
