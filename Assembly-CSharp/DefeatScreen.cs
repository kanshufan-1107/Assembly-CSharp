public class DefeatScreen : EndGameScreen
{
	protected override void Awake()
	{
		base.Awake();
		if (ShouldMakeUtilRequests())
		{
			NetCache.Get().RegisterScreenEndOfGame(OnNetCacheReady);
		}
	}

	protected override void ShowStandardFlow()
	{
		base.ShowStandardFlow();
		if (GameMgr.Get().IsTraditionalTutorial() && !GameMgr.Get().IsSpectator())
		{
			m_hitbox.AddEventListener(UIEventType.RELEASE, base.ContinueButtonPress_TutorialProgress);
		}
		else
		{
			m_hitbox.AddEventListener(UIEventType.RELEASE, base.ContinueButtonPress_PrevMode);
		}
		if (BattlegroundsEmoteHandler.TryGetActiveInstance(out var battlegroundsEmoteHandler))
		{
			battlegroundsEmoteHandler.HideEmotes();
		}
		else if (EmoteHandler.Get() != null)
		{
			EmoteHandler.Get().HideEmotes();
		}
		if (TargetReticleManager.Get() != null)
		{
			TargetReticleManager.Get().DestroyEnemyTargetArrow();
			TargetReticleManager.Get().DestroyFriendlyTargetArrow(isLocallyCanceled: false);
		}
	}

	protected override void InitGoldRewardUI()
	{
		string goldRewardMsg = EndGameScreen.GetFriendlyChallengeRewardText();
		if (!string.IsNullOrEmpty(goldRewardMsg))
		{
			m_noGoldRewardText.gameObject.SetActive(value: true);
			m_noGoldRewardText.Text = goldRewardMsg;
		}
	}
}
