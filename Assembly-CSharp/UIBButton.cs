using System;
using System.Collections;
using Hearthstone.UI;
using Hearthstone.UI.Core;
using UnityEngine;

[CustomEditClass]
[RequireComponent(typeof(Collider))]
public class UIBButton : PegUIElement
{
	[CustomEditField(Sections = "Button Objects")]
	public GameObject m_RootObject;

	[CustomEditField(Sections = "Text Object")]
	public UberText m_ButtonText;

	[CustomEditField(Sections = "Click Depress Behavior")]
	public Vector3 m_ClickDownOffset = new Vector3(0f, -0.05f, 0f);

	[CustomEditField(Sections = "Click Depress Behavior")]
	public float m_RaiseTime = 0.1f;

	[CustomEditField(Sections = "Click Depress Behavior")]
	public float m_DepressTime = 0.1f;

	[CustomEditField(Sections = "Click Depress Behavior")]
	public iTween.EaseType m_DepressEaseType = iTween.EaseType.linear;

	[CustomEditField(Sections = "Click Depress Behavior")]
	public bool m_HoldDepressionOnRelease;

	[CustomEditField(Sections = "Click Depress Behavior")]
	public bool m_DepressOnPhone;

	[CustomEditField(Sections = "Roll Over Depress Behavior")]
	public bool m_DepressOnOver;

	[CustomEditField(Sections = "Wiggle Behavior")]
	public Vector3 m_WiggleAmount = new Vector3(90f, 0f, 0f);

	[CustomEditField(Sections = "Wiggle Behavior")]
	public float m_WiggleTime = 0.5f;

	[CustomEditField(Sections = "Flip Enable Behavior")]
	public Vector3 m_DisabledRotation = Vector3.zero;

	[CustomEditField(Sections = "Flip Enable Behavior")]
	public bool m_AnimateFlip;

	[CustomEditField(Sections = "Flip Enable Behavior")]
	public float m_AnimateFlipTime = 0.25f;

	[CustomEditField(Sections = "Flip Enable Behavior")]
	public bool m_WigglePostFlip;

	[CustomEditField(Sections = "Flip Enable Behavior")]
	public Vector3 m_PostFlipWiggleAmount = new Vector3(90f, 0f, 0f);

	[CustomEditField(Sections = "Flip Enable Behavior")]
	public float m_PostFlipWiggleTime = 0.5f;

	[SerializeField]
	[CustomEditField(Sections = "Events")]
	private string m_bubbleUpEvent;

	[CustomEditField(Sections = "Events")]
	public bool m_UseCustomDragTolerance;

	[CustomEditField(Sections = "Events")]
	public float m_CustomDragTolerance = 40f;

	private Vector3? m_RootObjectOriginalPosition;

	private Vector3? m_RootObjectOriginalRotation;

	private bool m_Depressed;

	private bool m_HoldingDepression;

	private Vector3 m_targetRotation;

	[CustomEditField(Sections = "Events")]
	[Overridable]
	public string BubbleUpEvent
	{
		get
		{
			return m_bubbleUpEvent;
		}
		set
		{
			m_bubbleUpEvent = value;
		}
	}

	protected override void OnPress()
	{
		if (!m_DepressOnOver)
		{
			Depress();
		}
	}

	protected override void OnRelease()
	{
		if ((!m_DepressOnOver && !m_HoldDepressionOnRelease) || (m_HoldingDepression && m_HoldDepressionOnRelease))
		{
			Raise();
		}
		else if (m_HoldDepressionOnRelease)
		{
			m_HoldingDepression = true;
		}
	}

	protected override void OnOut(InteractionState oldState)
	{
		if (m_Depressed && !m_HoldingDepression)
		{
			Raise();
		}
	}

	protected override void OnOver(InteractionState oldState)
	{
		if (m_DepressOnOver)
		{
			Depress();
		}
		if (!UniversalInputManager.UsePhoneUI)
		{
			Wiggle();
		}
	}

	public void Select()
	{
		Depress();
	}

	public void Deselect()
	{
		Raise();
	}

	public void Flip(bool faceUp, bool forceImmediate = false)
	{
		if (m_RootObject == null)
		{
			return;
		}
		InitOriginalRotation();
		m_targetRotation = (faceUp ? m_RootObjectOriginalRotation.Value : (m_RootObjectOriginalRotation.Value + m_DisabledRotation));
		iTween.StopByName(m_RootObject, "flip");
		if (m_AnimateFlip && !forceImmediate)
		{
			Vector3 rotationAmount = (faceUp ? (-m_DisabledRotation) : m_DisabledRotation);
			Hashtable args = iTweenManager.Get().GetTweenHashTable();
			args.Add("amount", rotationAmount);
			args.Add("time", m_AnimateFlipTime);
			args.Add("easetype", iTween.EaseType.linear);
			args.Add("islocal", true);
			args.Add("name", "flip");
			args.Add("oncomplete", (Action<object>)delegate
			{
				m_RootObject.transform.localEulerAngles = m_targetRotation;
			});
			iTween.RotateAdd(m_RootObject, args);
			if (m_WigglePostFlip)
			{
				Wiggle(m_PostFlipWiggleAmount, m_targetRotation, m_PostFlipWiggleTime, m_AnimateFlipTime);
			}
		}
		else
		{
			m_RootObject.transform.localEulerAngles = m_targetRotation;
		}
	}

	public void SetText(string text)
	{
		if (m_ButtonText != null)
		{
			m_ButtonText.Text = text;
		}
	}

	public string GetText()
	{
		if (!m_ButtonText.GameStringLookup)
		{
			return m_ButtonText.Text;
		}
		return GameStrings.Get(m_ButtonText.Text);
	}

	public bool IsHoldingDepression()
	{
		return m_HoldingDepression;
	}

	private void Raise()
	{
		if (!(m_RootObject == null) && m_Depressed)
		{
			m_Depressed = false;
			m_HoldingDepression = false;
			iTween.StopByName(m_RootObject, "depress");
			if (m_RaiseTime > 0f)
			{
				Hashtable args = iTweenManager.Get().GetTweenHashTable();
				args.Add("position", m_RootObjectOriginalPosition);
				args.Add("time", m_RaiseTime);
				args.Add("easetype", m_DepressEaseType);
				args.Add("islocal", true);
				args.Add("name", "depress");
				iTween.MoveTo(m_RootObject, args);
			}
			else
			{
				m_RootObject.transform.localPosition = m_RootObjectOriginalPosition.Value;
			}
		}
	}

	private void Depress()
	{
		if (!(m_RootObject == null) && !m_Depressed && (!UniversalInputManager.UsePhoneUI || m_DepressOnPhone))
		{
			InitOriginalPosition();
			m_Depressed = true;
			iTween.StopByName(m_RootObject, "depress");
			Vector3 depressPos = m_RootObjectOriginalPosition.Value + m_ClickDownOffset;
			if (m_DepressTime > 0f)
			{
				Hashtable args = iTweenManager.Get().GetTweenHashTable();
				args.Add("position", depressPos);
				args.Add("time", m_DepressTime);
				args.Add("easetype", m_DepressEaseType);
				args.Add("islocal", true);
				args.Add("name", "depress");
				iTween.MoveTo(m_RootObject, args);
			}
			else
			{
				m_RootObject.transform.localPosition = depressPos;
			}
		}
	}

	public void Wiggle()
	{
		if (!(m_RootObject == null))
		{
			InitOriginalRotation();
			Wiggle(m_WiggleAmount, m_RootObjectOriginalRotation.Value, m_WiggleTime, 0f);
		}
	}

	private void Wiggle(Vector3 amount, Vector3 originalRotation, float time, float delay)
	{
		if (!(m_RootObject == null) && amount.sqrMagnitude != 0f && !(time <= 0f))
		{
			InitOriginalRotation();
			if (iTween.CountByName(m_RootObject, "wiggle") > 0)
			{
				iTween.StopByName(m_RootObject, "wiggle");
				m_RootObject.transform.localEulerAngles = m_targetRotation;
			}
			Hashtable args = iTweenManager.Get().GetTweenHashTable();
			args.Add("amount", amount);
			args.Add("time", time);
			args.Add("delay", delay);
			args.Add("name", "wiggle");
			args.Add("onstart", (Action<object>)delegate
			{
				m_RootObject.transform.localEulerAngles = m_targetRotation;
			});
			args.Add("oncomplete", (Action<object>)delegate
			{
				m_RootObject.transform.localEulerAngles = m_targetRotation;
			});
			iTween.PunchRotation(m_RootObject, args);
		}
	}

	private void InitOriginalRotation()
	{
		if (!(m_RootObject == null) && !m_RootObjectOriginalRotation.HasValue)
		{
			m_RootObjectOriginalRotation = m_RootObject.transform.localEulerAngles;
		}
	}

	private void InitOriginalPosition()
	{
		if (!(m_RootObject == null) && !m_RootObjectOriginalPosition.HasValue)
		{
			m_RootObjectOriginalPosition = m_RootObject.transform.localPosition;
		}
	}

	private void OnDisable()
	{
		if (!(m_RootObject == null))
		{
			iTween.StopByName(m_RootObject, "wiggle");
			m_RootObject.transform.localEulerAngles = m_targetRotation;
		}
	}

	protected override void Awake()
	{
		base.Awake();
		if (!(m_RootObject == null))
		{
			if (m_UseCustomDragTolerance)
			{
				SetDragTolerance(m_CustomDragTolerance);
			}
			m_targetRotation = m_RootObject.transform.localEulerAngles;
		}
	}

	protected override void OnTap()
	{
		base.OnTap();
		if (!string.IsNullOrEmpty(m_bubbleUpEvent))
		{
			SendEventUpwardStateAction.SendEventUpward(base.gameObject, m_bubbleUpEvent);
		}
	}
}
