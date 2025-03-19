using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CustomEditClass]
public class FlippingPanel : MonoBehaviour
{
	public delegate void PanelContentChanged(int newContentOffset);

	[CustomEditField(Sections = "Panels")]
	public List<GameObject> m_panelContent = new List<GameObject>();

	[CustomEditField(Sections = "Panels")]
	public List<Transform> m_faceBones = new List<Transform>();

	[CustomEditField(Sections = "Rotation")]
	public GameObject m_rotatingObject;

	[CustomEditField(Sections = "Rotation")]
	public float m_contentFlipAnimationTime = 0.5f;

	[CustomEditField(Sections = "Rotation")]
	public iTween.EaseType m_contentFlipEaseType = iTween.EaseType.easeOutBounce;

	[CustomEditField(Sections = "Rotation")]
	public float m_rotationDegrees = 120f;

	[CustomEditField(Sections = "Rotation")]
	public bool m_allowLoopingToStartOfContent = true;

	[CustomEditField(Sections = "Sounds", T = EditType.SOUND_PREFAB)]
	public string m_contentFlipSound;

	private List<PanelContentChanged> m_panelContentChangedListeners = new List<PanelContentChanged>();

	private int m_currentContentOffset;

	private int m_currentFaceBone;

	private GameObject m_previousContent;

	private Quaternion m_desiredOrientation = Quaternion.identity;

	public int CurrentContentOffset => m_currentContentOffset;

	private void Awake()
	{
		for (int i = 0; i < m_panelContent.Count; i++)
		{
			if (i == m_currentContentOffset)
			{
				GameObject obj = m_panelContent[i];
				obj.transform.parent = m_faceBones[m_currentFaceBone];
				GameUtils.ResetTransform(obj);
				obj.gameObject.SetActive(value: true);
			}
			else
			{
				m_panelContent[i].SetActive(value: false);
			}
		}
	}

	private void Start()
	{
		m_desiredOrientation = m_rotatingObject.transform.localRotation;
	}

	public bool FlipPanel(bool forward)
	{
		if (iTween.CountByName(m_rotatingObject, "PANEL_ROTATION") > 0)
		{
			iTween.StopByName(m_rotatingObject, "PANEL_ROTATION");
			FinishFlip();
		}
		int nextContentOffset = m_currentContentOffset + (forward ? 1 : (-1));
		if (nextContentOffset >= m_panelContent.Count)
		{
			if (!m_allowLoopingToStartOfContent)
			{
				return false;
			}
			nextContentOffset = 0;
		}
		else if (nextContentOffset < 0)
		{
			if (!m_allowLoopingToStartOfContent)
			{
				return false;
			}
			nextContentOffset = m_panelContent.Count - 1;
		}
		m_previousContent = m_panelContent[m_currentContentOffset];
		m_currentContentOffset = nextContentOffset;
		GameObject nextContent = m_panelContent[m_currentContentOffset];
		int nextFaceBone = m_currentFaceBone + (forward ? 1 : (-1));
		if (nextFaceBone >= m_faceBones.Count)
		{
			nextFaceBone = 0;
		}
		else if (nextFaceBone < 0)
		{
			nextFaceBone = m_faceBones.Count - 1;
		}
		m_currentFaceBone = nextFaceBone;
		if (nextContent != null)
		{
			nextContent.transform.parent = m_faceBones[m_currentFaceBone];
			GameUtils.ResetTransform(nextContent);
			nextContent.gameObject.SetActive(value: true);
		}
		if (!string.IsNullOrEmpty(m_contentFlipSound))
		{
			SoundManager.Get().LoadAndPlay(m_contentFlipSound);
		}
		Quaternion flipRotation = (forward ? Quaternion.AngleAxis(m_rotationDegrees, Vector3.right) : Quaternion.AngleAxis(m_rotationDegrees, Vector3.left));
		Quaternion targetOrientation = m_rotatingObject.transform.localRotation * flipRotation;
		m_desiredOrientation = targetOrientation;
		if (m_contentFlipAnimationTime > 0f)
		{
			Hashtable args = iTweenManager.Get().GetTweenHashTable();
			args.Add("amount", m_rotationDegrees * (forward ? Vector3.right : Vector3.left));
			args.Add("time", m_contentFlipAnimationTime);
			args.Add("easetype", m_contentFlipEaseType);
			args.Add("islocal", true);
			args.Add("name", "PANEL_ROTATION");
			args.Add("oncomplete", (Action<object>)delegate
			{
				FinishFlip();
			});
			iTween.RotateAdd(m_rotatingObject, args);
		}
		FirePanelContentChangedEvent(m_currentContentOffset);
		return true;
	}

	public void AddPanelContentChangedListener(PanelContentChanged listener)
	{
		m_panelContentChangedListeners.Add(listener);
	}

	public void RemovePanelContentChangedListener(PanelContentChanged listener)
	{
		m_panelContentChangedListeners.Remove(listener);
	}

	private void FirePanelContentChangedEvent(int newContentOffset)
	{
		PanelContentChanged[] array = m_panelContentChangedListeners.ToArray();
		for (int i = 0; i < array.Length; i++)
		{
			array[i](newContentOffset);
		}
	}

	private void FinishFlip()
	{
		m_rotatingObject.transform.localRotation = m_desiredOrientation;
		if (m_previousContent != null)
		{
			m_previousContent.gameObject.SetActive(value: false);
		}
	}
}
