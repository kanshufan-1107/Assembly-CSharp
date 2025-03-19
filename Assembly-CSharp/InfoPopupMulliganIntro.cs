using System.Collections;
using Hearthstone.UI;
using UnityEngine;

public class InfoPopupMulliganIntro
{
	private WidgetInstance m_introPopup;

	private Spell m_introSpell;

	protected IEnumerator ShowPopup(string introPopupWidgetName, string boardBoneName, bool skipPopup = false)
	{
		SceneMgr.Get().NotifySceneLoaded();
		while (LoadingScreen.Get().IsPreviousSceneActive() || LoadingScreen.Get().IsFadingOut())
		{
			yield return null;
		}
		GameMgr.Get().UpdatePresence();
		if (!skipPopup)
		{
			m_introPopup = WidgetInstance.Create(introPopupWidgetName);
			if (!m_introPopup)
			{
				yield break;
			}
			while (!m_introPopup.IsReady)
			{
				yield return null;
			}
			Vector3 centerPos = Board.Get().FindBone(boardBoneName).position;
			m_introPopup.transform.localPosition = centerPos;
			m_introSpell = m_introPopup.GetComponentInChildren<Spell>();
			if (!m_introSpell)
			{
				yield break;
			}
			m_introSpell.AddStateFinishedCallback(OnIntroSpellStateFinished);
			m_introSpell.ActivateState(SpellStateType.BIRTH);
			while (!m_introSpell.IsFinished())
			{
				yield return null;
			}
		}
		Board.Get().RaiseTheLights();
		EndTurnButton.Get().RemoveInputBlocker();
		TurnStartManager.Get().BeginListeningForTurnEvents();
		MulliganManager.Get().SkipMulligan();
	}

	private void OnIntroSpellStateFinished(Spell spell, SpellStateType prevStateType, object userData)
	{
		if (m_introSpell.GetActiveState() == SpellStateType.NONE)
		{
			Object.Destroy(m_introSpell);
			m_introSpell = null;
			Object.Destroy(m_introPopup);
			m_introPopup = null;
		}
	}
}
