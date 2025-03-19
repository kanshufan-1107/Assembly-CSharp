using System;
using UnityEngine;

public class MobileFriendListItem : MonoBehaviour, ISelectableTouchListItem, ITouchListItem
{
	[Flags]
	public enum TypeFlags
	{
		Request = 0x100,
		RecentPlayer = 0x10,
		NearbyPlayer = 8,
		CurrentGame = 4,
		Friend = 2,
		Header = 1
	}

	private Bounds m_localBounds;

	private ITouchListItem m_parent;

	private GameObject m_showObject;

	public TypeFlags Type { get; set; }

	public Bounds LocalBounds => m_localBounds;

	public bool Selectable
	{
		get
		{
			if (Type != TypeFlags.Friend)
			{
				return Type == TypeFlags.NearbyPlayer;
			}
			return true;
		}
	}

	public bool IsHeader => ItemIsHeader(Type);

	public bool Visible
	{
		get
		{
			if (m_parent != null)
			{
				return m_parent.Visible;
			}
			return true;
		}
		set
		{
			if (!(m_showObject == null) && value != m_showObject.activeSelf)
			{
				m_showObject.SetActive(value);
			}
		}
	}

	public event Action OnScrollOutOfViewEvent;

	public void SetParent(ITouchListItem parent)
	{
		m_parent = parent;
	}

	public void SetShowObject(GameObject showobj)
	{
		m_showObject = showobj;
	}

	public static bool ItemIsHeader(TypeFlags typeFlags)
	{
		return (typeFlags & TypeFlags.Header) != 0;
	}

	private void Awake()
	{
		Transform parent = base.transform.parent;
		TransformProps worldTransform = TransformUtil.GetWorldTransformProps(base.transform);
		base.transform.parent = null;
		TransformUtil.Identity(base.transform);
		m_localBounds = ComputeWorldBounds();
		base.transform.parent = parent;
		TransformUtil.CopyWorld(base.transform, worldTransform);
	}

	public bool IsSelected()
	{
		FriendListUIElement uiElement = GetComponent<FriendListUIElement>();
		if (uiElement != null)
		{
			return uiElement.IsSelected();
		}
		return false;
	}

	public void Selected()
	{
		FriendListUIElement uiElement = GetComponent<FriendListUIElement>();
		if (uiElement != null)
		{
			uiElement.SetSelected(enable: true);
		}
	}

	public void Unselected()
	{
		FriendListUIElement uiElement = GetComponent<FriendListUIElement>();
		if (uiElement != null)
		{
			uiElement.SetSelected(enable: false);
		}
	}

	public Bounds ComputeWorldBounds()
	{
		return TransformUtil.ComputeSetPointBounds(base.gameObject);
	}

	public new T GetComponent<T>() where T : Component
	{
		return ((Component)this).GetComponent<T>();
	}

	public void OnScrollOutOfView()
	{
		if (this.OnScrollOutOfViewEvent != null)
		{
			this.OnScrollOutOfViewEvent();
		}
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
