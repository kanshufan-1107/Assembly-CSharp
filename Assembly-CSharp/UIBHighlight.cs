using UnityEngine;

[CustomEditClass]
[RequireComponent(typeof(PegUIElement))]
public class UIBHighlight : MonoBehaviour
{
	[CustomEditField(Sections = "Highlight Objects")]
	public GameObject m_MouseOverHighlight;

	[CustomEditField(Sections = "Highlight Objects")]
	public GameObject m_MouseDownHighlight;

	[CustomEditField(Sections = "Highlight Objects")]
	public GameObject m_MouseUpHighlight;

	[CustomEditField(Sections = "Highlight Sounds", T = EditType.SOUND_PREFAB)]
	public string m_MouseOverSound = "Small_Mouseover.prefab:692610296028713458ea58bc34adb4c9";

	[CustomEditField(Sections = "Highlight Sounds", T = EditType.SOUND_PREFAB)]
	public string m_MouseOutSound;

	[CustomEditField(Sections = "Highlight Sounds", T = EditType.SOUND_PREFAB)]
	public string m_MouseDownSound = "Small_Click.prefab:2a1c5335bf08dc84eb6e04fc58160681";

	[CustomEditField(Sections = "Highlight Sounds", T = EditType.SOUND_PREFAB)]
	public string m_MouseUpSound;

	[CustomEditField(Sections = "Behavior Settings")]
	public bool m_SelectOnRelease;

	[CustomEditField(Sections = "Behavior Settings")]
	public bool m_HideMouseOverOnPress;

	[SerializeField]
	private bool m_AlwaysOver;

	[SerializeField]
	private bool m_EnableResponse = true;

	[Tooltip("Note: Allowing selection and allowing dragging are mutually exclusive.")]
	[CustomEditField(Sections = "Allow Selection", Label = "Enable")]
	public bool m_AllowSelection;

	[CustomEditField(Parent = "m_AllowSelection")]
	public GameObject m_SelectedHighlight;

	[CustomEditField(Parent = "m_AllowSelection")]
	public GameObject m_MouseOverSelectedHighlight;

	[Tooltip("Note: Allowing selection and allowing dragging are mutually exclusive.")]
	[CustomEditField(Sections = "Allow Dragging", Label = "Enable")]
	public bool m_AllowDragging;

	[CustomEditField(Parent = "m_AllowDragging")]
	public GameObject m_DraggingHighlight;

	[CustomEditField(Parent = "m_AllowDragging")]
	public bool m_SoundOnReleaseDrag;

	[CustomEditField(Parent = "m_AllowDragging", T = EditType.SOUND_PREFAB)]
	public string m_ReleaseDragSound;

	private bool m_Dragging;

	[CustomEditField(Sections = "Hold")]
	public bool m_HighlightOnHold;

	[CustomEditField(Parent = "m_HighlightOnHold")]
	public GameObject m_HoldHighlight;

	private bool m_Holding;

	[CustomEditField(Sections = "Behavior Settings")]
	public bool AlwaysOver
	{
		get
		{
			return m_AlwaysOver;
		}
		set
		{
			m_AlwaysOver = value;
			ResetState();
		}
	}

	[CustomEditField(Sections = "Behavior Settings")]
	public bool EnableResponse
	{
		get
		{
			return m_EnableResponse;
		}
		set
		{
			m_EnableResponse = value;
			ResetState();
		}
	}

	private void Awake()
	{
		PegUIElement m_PegUIElement = base.gameObject.GetComponent<PegUIElement>();
		if (m_PegUIElement != null)
		{
			m_PegUIElement.AddEventListener(UIEventType.ROLLOVER, delegate
			{
				OnRollOver();
			});
			m_PegUIElement.AddEventListener(UIEventType.PRESS, delegate
			{
				OnPress(playSound: true);
			});
			m_PegUIElement.AddEventListener(UIEventType.RELEASE, delegate
			{
				OnRelease();
			});
			m_PegUIElement.AddEventListener(UIEventType.RELEASEALL, delegate
			{
				OnReleaseAll();
			});
			m_PegUIElement.AddEventListener(UIEventType.ROLLOUT, delegate
			{
				OnRollOut();
			});
			m_PegUIElement.AddEventListener(UIEventType.DRAG, delegate
			{
				OnDrag();
			});
			m_PegUIElement.AddEventListener(UIEventType.HOLD, delegate
			{
				OnHold();
			});
			m_PegUIElement.AddEventListener(UIEventType.DISABLE, delegate
			{
				OnDisable();
			});
			m_PegUIElement.AddEventListener(UIEventType.ENABLE, delegate
			{
				OnEnable();
			});
			ResetState();
		}
	}

	public void HighlightOnce()
	{
		OnRollOver(force: true);
	}

	public void Select()
	{
		if (m_SelectOnRelease)
		{
			OnRelease(playSound: true);
		}
		else
		{
			OnPress(playSound: true);
		}
	}

	public void SelectNoSound()
	{
		if (m_SelectOnRelease)
		{
			OnRelease(playSound: false);
		}
		else
		{
			OnPress(playSound: false);
		}
	}

	public void Reset()
	{
		ResetState();
		ShowHighlightObject(m_SelectedHighlight, show: false);
		ShowHighlightObject(m_MouseOverSelectedHighlight, show: false);
		ShowHighlightObject(m_DraggingHighlight, show: false);
		ShowHighlightObject(m_MouseOverHighlight, show: false);
	}

	private void ResetState()
	{
		if (m_AlwaysOver)
		{
			OnRollOver(force: true);
		}
		else
		{
			OnRollOut(force: true);
		}
	}

	private void OnRollOver(bool force = false)
	{
		if ((m_EnableResponse || force) && !m_Dragging && !m_Holding)
		{
			if (!m_AlwaysOver)
			{
				PlaySound(m_MouseOverSound);
			}
			if (m_AllowSelection && (m_SelectedHighlight == null || m_SelectedHighlight.activeSelf))
			{
				ShowHighlightObject(m_SelectedHighlight, show: false);
				ShowHighlightObject(m_MouseOverHighlight, show: false);
				ShowHighlightObject(m_MouseUpHighlight, show: false);
				ShowHighlightObject(m_MouseDownHighlight, show: false);
				ShowHighlightObject(m_MouseOverSelectedHighlight, show: true);
			}
			else
			{
				ShowHighlightObject(m_MouseDownHighlight, show: false);
				ShowHighlightObject(m_MouseUpHighlight, show: false);
				ShowHighlightObject(m_MouseOverHighlight, show: true);
			}
		}
	}

	private void OnRollOut(bool force = false)
	{
		if ((m_EnableResponse || force) && !m_Dragging && !m_Holding)
		{
			PlaySound(m_MouseOutSound);
			if (m_AllowSelection && (m_MouseOverSelectedHighlight == null || m_MouseOverSelectedHighlight.activeSelf))
			{
				ShowHighlightObject(m_MouseOverSelectedHighlight, show: false);
				ShowHighlightObject(m_MouseOverHighlight, show: false);
				ShowHighlightObject(m_MouseUpHighlight, show: false);
				ShowHighlightObject(m_MouseDownHighlight, show: false);
				ShowHighlightObject(m_SelectedHighlight, show: true);
			}
			else
			{
				ShowHighlightObject(m_MouseDownHighlight, show: false);
				ShowHighlightObject(m_MouseOverHighlight, m_AlwaysOver);
				ShowHighlightObject(m_MouseUpHighlight, !m_AlwaysOver);
			}
		}
	}

	private void OnPress()
	{
		OnPress(playSound: true);
	}

	private void OnPress(bool playSound)
	{
		if (m_EnableResponse)
		{
			if (playSound)
			{
				PlaySound(m_MouseDownSound);
			}
			if (m_AllowSelection && !m_SelectOnRelease)
			{
				ShowHighlightObject(m_MouseOverSelectedHighlight, show: false);
				ShowHighlightObject(m_MouseOverHighlight, show: false);
				ShowHighlightObject(m_MouseUpHighlight, show: false);
				ShowHighlightObject(m_MouseDownHighlight, show: false);
				ShowHighlightObject(m_SelectedHighlight, show: true);
			}
			else
			{
				ShowHighlightObject(m_MouseOverHighlight, m_AlwaysOver || !m_HideMouseOverOnPress);
				ShowHighlightObject(m_MouseUpHighlight, !m_AlwaysOver);
				ShowHighlightObject(m_MouseDownHighlight, show: true);
			}
		}
	}

	private void OnRelease()
	{
		OnRelease(playSound: true);
	}

	private void OnRelease(bool playSound)
	{
		if (!m_EnableResponse)
		{
			return;
		}
		if (m_AllowDragging && m_Dragging)
		{
			ReleaseDrag();
			return;
		}
		if (m_HighlightOnHold && m_Holding)
		{
			ReleaseHold();
			return;
		}
		if (m_AllowSelection && m_SelectOnRelease)
		{
			ShowHighlightObject(m_MouseOverSelectedHighlight, show: false);
			ShowHighlightObject(m_MouseOverHighlight, show: false);
			ShowHighlightObject(m_MouseUpHighlight, show: false);
			ShowHighlightObject(m_MouseDownHighlight, show: false);
			ShowHighlightObject(m_HoldHighlight, show: false);
			ShowHighlightObject(m_SelectedHighlight, show: true);
		}
		else
		{
			ShowHighlightObject(m_MouseDownHighlight, show: false);
			ShowHighlightObject(m_MouseUpHighlight, show: false);
			ShowHighlightObject(m_HoldHighlight, show: false);
			ShowHighlightObject(m_MouseOverHighlight, show: true);
		}
		if (playSound)
		{
			PlaySound(m_MouseUpSound);
		}
	}

	private void OnReleaseAll()
	{
		if (m_AllowDragging && m_Dragging)
		{
			ReleaseDrag();
		}
		else if (m_HighlightOnHold && m_Holding)
		{
			ReleaseHold();
		}
	}

	private void OnDrag()
	{
		if (m_EnableResponse && m_AllowDragging)
		{
			if (m_Holding)
			{
				ReleaseHold();
			}
			m_Dragging = true;
			if (!m_AlwaysOver)
			{
				ShowHighlightObject(m_MouseDownHighlight, show: false);
				ShowHighlightObject(m_MouseOverHighlight, show: false);
				ShowHighlightObject(m_HoldHighlight, show: false);
				ShowHighlightObject(m_DraggingHighlight, show: true);
			}
		}
	}

	private void ReleaseDrag()
	{
		m_Dragging = false;
		if (m_SoundOnReleaseDrag)
		{
			PlaySound(m_ReleaseDragSound);
		}
		if (!m_AlwaysOver)
		{
			ShowHighlightObject(m_MouseOverHighlight, show: false);
			ShowHighlightObject(m_MouseUpHighlight, show: false);
			ShowHighlightObject(m_MouseDownHighlight, show: false);
			ShowHighlightObject(m_DraggingHighlight, show: false);
			ShowHighlightObject(m_HoldHighlight, show: false);
		}
	}

	private void OnHold()
	{
		if (m_EnableResponse && !m_AlwaysOver && m_HighlightOnHold)
		{
			m_Holding = true;
			ShowHighlightObject(m_MouseDownHighlight, show: false);
			ShowHighlightObject(m_MouseOverHighlight, show: false);
			ShowHighlightObject(m_DraggingHighlight, show: false);
			ShowHighlightObject(m_HoldHighlight, show: true);
		}
	}

	private void ReleaseHold()
	{
		m_Holding = false;
		if (!m_AlwaysOver)
		{
			ShowHighlightObject(m_MouseUpHighlight, show: false);
			ShowHighlightObject(m_MouseDownHighlight, show: false);
			ShowHighlightObject(m_DraggingHighlight, show: false);
			ShowHighlightObject(m_HoldHighlight, show: false);
			ShowHighlightObject(m_MouseOverHighlight, show: true);
		}
	}

	private void OnDisable()
	{
		ResetState();
	}

	private void OnEnable()
	{
		ResetState();
	}

	private void ShowHighlightObject(GameObject obj, bool show)
	{
		if (obj != null && obj.activeSelf != show)
		{
			obj.SetActive(show);
		}
	}

	private void PlaySound(string soundFilePath)
	{
		if (SoundManager.Get() != null && !string.IsNullOrEmpty(soundFilePath))
		{
			SoundManager.Get().LoadAndPlay(soundFilePath);
		}
	}
}
