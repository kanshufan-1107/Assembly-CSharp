using System;
using System.Collections;
using UnityEngine;

public class CollectionDeckTrayButton : PegUIElement
{
	public delegate void DelOnAnimationFinished(object callbackData);

	private class OnPopAnimationFinishedCallbackData
	{
		public string m_animationName;

		public DelOnAnimationFinished m_callback;

		public object m_callbackData;
	}

	public HighlightState m_highlightState;

	public UberText m_buttonText;

	private const float BUTTON_POP_SPEED = 2.5f;

	private readonly string DECKBOX_POPUP_ANIM_NAME = "NewDeck_PopUp";

	private readonly string DECKBOX_POPDOWN_ANIM_NAME = "NewDeck_PopDown";

	private bool m_isPoppedUp;

	private bool m_isUsable;

	protected override void Awake()
	{
		base.Awake();
		SetEnabled(enabled: false);
		if (SceneMgr.Get().IsInTavernBrawlMode())
		{
			m_buttonText.Text = string.Empty;
		}
		else if (SceneMgr.Get().IsInLettuceMode())
		{
			m_buttonText.Text = GameStrings.Get("GLUE_COLLECTION_NEW_TEAM");
		}
		else
		{
			m_buttonText.Text = GameStrings.Get("GLUE_COLLECTION_NEW_DECK");
		}
		UIBScrollableItem scrollable = GetComponent<UIBScrollableItem>();
		if (scrollable != null)
		{
			scrollable.SetCustomActiveState(IsUsable);
		}
	}

	public void SetIsUsable(bool isUsable)
	{
		m_isUsable = isUsable;
	}

	public bool IsUsable()
	{
		return m_isUsable;
	}

	public void PlayPopUpAnimation()
	{
		PlayPopUpAnimation(null);
	}

	public void PlayPopUpAnimation(DelOnAnimationFinished callback)
	{
		PlayPopUpAnimation(callback, null, null);
	}

	public void PlayPopUpAnimation(DelOnAnimationFinished callback, object callbackData, float? speed = null)
	{
		base.gameObject.SetActive(value: true);
		if (m_isPoppedUp)
		{
			callback?.Invoke(callbackData);
			return;
		}
		m_isPoppedUp = true;
		Animation component = GetComponent<Animation>();
		component[DECKBOX_POPUP_ANIM_NAME].time = 0f;
		component[DECKBOX_POPUP_ANIM_NAME].speed = (speed.HasValue ? speed.Value : 2.5f);
		PlayAnimation(DECKBOX_POPUP_ANIM_NAME, callback, callbackData);
	}

	public void PlayPopDownAnimation()
	{
		PlayPopDownAnimation(null);
	}

	public void PlayPopDownAnimation(DelOnAnimationFinished callback)
	{
		PlayPopDownAnimation(callback, null, null);
	}

	public void PlayPopDownAnimation(DelOnAnimationFinished callback, object callbackData, float? speed = null)
	{
		base.gameObject.SetActive(value: true);
		if (!m_isPoppedUp)
		{
			callback?.Invoke(callbackData);
			return;
		}
		m_isPoppedUp = false;
		Animation component = GetComponent<Animation>();
		component[DECKBOX_POPDOWN_ANIM_NAME].time = 0f;
		component[DECKBOX_POPDOWN_ANIM_NAME].speed = (speed.HasValue ? speed.Value : 2.5f);
		PlayAnimation(DECKBOX_POPDOWN_ANIM_NAME, callback, callbackData);
	}

	public void FlipHalfOverAndHide(float animTime, DelOnAnimationFinished finished = null)
	{
		if (!m_isPoppedUp)
		{
			Debug.LogWarning("Can't flip over and hide button. It is currently not popped up.");
			return;
		}
		iTween.StopByName(base.gameObject, "rotation");
		iTween.RotateTo(base.gameObject, iTween.Hash("rotation", new Vector3(270f, 0f, 0f), "islocal", true, "time", animTime, "easetype", iTween.EaseType.easeInCubic, "oncomplete", (Action<object>)delegate
		{
			if (finished != null)
			{
				finished(this);
			}
			base.gameObject.SetActive(value: false);
			base.transform.localEulerAngles = Vector3.zero;
		}, "name", "rotation"));
		m_isPoppedUp = false;
	}

	public bool IsPoppedUp()
	{
		return m_isPoppedUp;
	}

	private void PlayAnimation(string animationName)
	{
		PlayAnimation(animationName, null, null);
	}

	private void PlayAnimation(string animationName, DelOnAnimationFinished callback, object callbackData)
	{
		GetComponent<Animation>().Play(animationName);
		OnPopAnimationFinishedCallbackData onAnimationFinishedCallbackData = new OnPopAnimationFinishedCallbackData
		{
			m_callback = callback,
			m_callbackData = callbackData,
			m_animationName = animationName
		};
		StopCoroutine("WaitThenCallAnimationCallback");
		StartCoroutine("WaitThenCallAnimationCallback", onAnimationFinishedCallbackData);
	}

	private IEnumerator WaitThenCallAnimationCallback(OnPopAnimationFinishedCallbackData callbackData)
	{
		Animation anim = GetComponent<Animation>();
		yield return new WaitForSeconds(anim[callbackData.m_animationName].length / anim[callbackData.m_animationName].speed);
		bool enableInput = callbackData.m_animationName.Equals(DECKBOX_POPUP_ANIM_NAME);
		SetEnabled(enableInput);
		if (callbackData.m_callback != null)
		{
			callbackData.m_callback(callbackData.m_callbackData);
		}
	}

	protected override void OnOver(InteractionState oldState)
	{
		SoundManager.Get().LoadAndPlay("Hub_Mouseover.prefab:40130da7b734190479c527d6bca1a4a8");
		m_highlightState.ChangeState(ActorStateType.HIGHLIGHT_MOUSE_OVER);
	}

	protected override void OnOut(InteractionState oldState)
	{
		m_highlightState.ChangeState(ActorStateType.NONE);
	}
}
