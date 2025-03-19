using System.Collections.Generic;
using UnityEngine;

public class FriendListItemHeader : PegUIElement, ITouchListItem
{
	public delegate void ToggleContentsFunc(bool show, object userdata);

	private class ToggleContentsListener : EventListener<ToggleContentsFunc>
	{
		public void Fire(bool show)
		{
			m_callback(show, m_userData);
		}
	}

	public UberText m_Text;

	public GameObject m_Arrow;

	public Transform m_FoldinBone;

	public Transform m_FoldoutBone;

	public float m_AnimRotateTime = 0.25f;

	public bool m_toggleEnabled = true;

	public float m_textXOffsetWhenToggleDisabled = -0.2f;

	private List<ToggleContentsListener> m_ToggleEventListeners = new List<ToggleContentsListener>();

	private bool m_ShowContents = true;

	private MultiSliceElement m_multiSlice;

	public GameObject Background { get; set; }

	public Bounds LocalBounds { get; private set; }

	public bool IsHeader => true;

	public bool Visible
	{
		get
		{
			return IsShowingContents;
		}
		set
		{
		}
	}

	public bool IsShowingContents => m_ShowContents;

	public MobileFriendListItem.TypeFlags SubType { get; set; }

	public Option Option { get; set; }

	public void SetText(string text)
	{
		m_Text.Text = text;
	}

	public void SetInitialShowContents(bool show)
	{
		m_ShowContents = show;
		if (m_Arrow != null)
		{
			m_Arrow.transform.rotation = GetCurrentBoneTransform().rotation;
		}
	}

	public void AddToggleListener(ToggleContentsFunc func, object userdata)
	{
		ToggleContentsListener l = new ToggleContentsListener();
		l.SetCallback(func);
		l.SetUserData(userdata);
		m_ToggleEventListeners.Add(l);
	}

	public void ClearToggleListeners()
	{
		m_ToggleEventListeners.Clear();
	}

	public new T GetComponent<T>() where T : Component
	{
		return ((Component)this).GetComponent<T>();
	}

	public void SetToggleEnabled(bool enabled)
	{
		m_toggleEnabled = enabled;
		if (!enabled)
		{
			m_ShowContents = true;
			if (m_Arrow != null)
			{
				TransformUtil.SetLocalPosX(m_Text, m_textXOffsetWhenToggleDisabled);
			}
		}
		if (m_Arrow != null)
		{
			m_Arrow.gameObject.SetActive(enabled);
		}
	}

	protected override void Awake()
	{
		base.Awake();
		AddEventListener(UIEventType.RELEASE, OnHeaderButtonReleased);
		if (m_multiSlice == null)
		{
			m_multiSlice = GetComponentInChildren<MultiSliceElement>();
			if ((bool)m_multiSlice)
			{
				m_multiSlice.UpdateSlices();
			}
		}
	}

	protected virtual void OnHeaderButtonReleased(UIEvent e)
	{
		if (m_toggleEnabled)
		{
			m_ShowContents = !m_ShowContents;
			ToggleContentsListener[] funcs = m_ToggleEventListeners.ToArray();
			for (int i = 0; i < funcs.Length; i++)
			{
				funcs[i].Fire(m_ShowContents);
			}
			UpdateFoldoutArrow();
		}
	}

	private void UpdateFoldoutArrow()
	{
		if (!(m_Arrow == null) && !(m_FoldinBone == null) && !(m_FoldoutBone == null))
		{
			iTween.RotateTo(m_Arrow, GetCurrentBoneTransform().rotation.eulerAngles, m_AnimRotateTime);
		}
	}

	private Transform GetCurrentBoneTransform()
	{
		if (!m_ShowContents)
		{
			return m_FoldinBone;
		}
		return m_FoldoutBone;
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
