using System.Collections;
using UnityEngine;

public class ArrowModeButton : PegUIElement
{
	public HighlightState m_highlight;

	private int m_numFlips;

	private bool m_isHighlighted;

	protected override void Awake()
	{
		base.Awake();
		SoundManager.Get().Load("Small_Mouseover.prefab:692610296028713458ea58bc34adb4c9");
		SoundManager.Get().Load("deck_select_button_press.prefab:46d62875e039445439070cc9f7480d48");
	}

	public void Activate(bool activate)
	{
		if (activate != IsEnabled())
		{
			SetEnabled(activate);
			if (!activate && m_highlight != null)
			{
				m_highlight.ChangeState(ActorStateType.HIGHLIGHT_OFF);
			}
			m_numFlips++;
			if (iTweenManager.Get().GetTweenForObject(base.gameObject) == null)
			{
				Flip();
			}
		}
	}

	public void ActivateHighlight(bool highlightOn)
	{
		if (!(m_highlight == null))
		{
			m_isHighlighted = highlightOn;
			ActorStateType highlightState = (m_isHighlighted ? ActorStateType.HIGHLIGHT_PRIMARY_ACTIVE : ActorStateType.HIGHLIGHT_OFF);
			m_highlight.ChangeState(highlightState);
		}
	}

	protected override void OnRelease()
	{
		if (!(m_highlight == null))
		{
			SoundManager.Get().LoadAndPlay("deck_select_button_press.prefab:46d62875e039445439070cc9f7480d48");
			m_isHighlighted = false;
			m_highlight.ChangeState(ActorStateType.HIGHLIGHT_OFF);
		}
	}

	protected override void OnOver(InteractionState oldState)
	{
		SoundManager.Get().LoadAndPlay("Small_Mouseover.prefab:692610296028713458ea58bc34adb4c9");
		if (!(m_highlight == null))
		{
			ActorStateType highlightState = (m_isHighlighted ? ActorStateType.HIGHLIGHT_PRIMARY_MOUSE_OVER : ActorStateType.HIGHLIGHT_MOUSE_OVER);
			m_highlight.ChangeState(highlightState);
		}
	}

	protected override void OnOut(InteractionState oldState)
	{
		if (!(m_highlight == null))
		{
			ActorStateType highlightState = (m_isHighlighted ? ActorStateType.HIGHLIGHT_PRIMARY_ACTIVE : ActorStateType.HIGHLIGHT_OFF);
			m_highlight.ChangeState(highlightState);
		}
	}

	private void Flip()
	{
		Hashtable args = iTweenManager.Get().GetTweenHashTable();
		args.Add("amount", new Vector3(180f, 0f, 0f));
		args.Add("time", 0.5f);
		args.Add("easetype", iTween.EaseType.easeOutElastic);
		args.Add("space", Space.Self);
		args.Add("oncomplete", "OnFlipComplete");
		args.Add("oncompletetarget", base.gameObject);
		iTween.RotateAdd(base.gameObject, args);
	}

	private void OnFlipComplete()
	{
		m_numFlips--;
		if (m_numFlips > 0)
		{
			Flip();
		}
	}
}
