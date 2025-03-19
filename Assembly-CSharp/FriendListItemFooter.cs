using UnityEngine;

public class FriendListItemFooter : PegUIElement, ITouchListItem
{
	public UberText m_Text;

	public Bounds LocalBounds
	{
		get
		{
			Bounds worldSpaceBounds = TransformUtil.ComputeSetPointBounds(GetComponent<Collider>());
			Vector3 localCenter = base.transform.InverseTransformPoint(worldSpaceBounds.center);
			return new Bounds(localCenter, worldSpaceBounds.size);
		}
	}

	public string Text
	{
		get
		{
			return m_Text.Text;
		}
		set
		{
			m_Text.Text = value;
		}
	}

	public bool IsHeader => false;

	public bool Visible
	{
		get
		{
			return base.gameObject.activeSelf;
		}
		set
		{
			base.gameObject.SetActive(value);
		}
	}

	public new T GetComponent<T>() where T : Component
	{
		return ((Component)this).GetComponent<T>();
	}

	protected override void Awake()
	{
		base.Awake();
	}

	public void OnScrollOutOfView()
	{
	}

	public void OnPositionUpdate()
	{
	}

	GameObject ITouchListItem.get_gameObject()
	{
		return base.gameObject;
	}

	Transform ITouchListItem.get_transform()
	{
		return base.transform;
	}
}
