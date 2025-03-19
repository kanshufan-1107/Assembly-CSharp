public class BaconEndGameScreen : EndGameScreen
{
	public GamesWonIndicator m_gamesWonIndicator;

	private const int ShowWinProgressPlacement = 4;

	private const int ShowAppRatingPromptPlacement = 4;

	private bool m_showWinProgress;

	private int m_place = int.MaxValue;

	private bool m_hasAlreadyCheckedForAppRating;

	private int Place
	{
		get
		{
			if (m_place == int.MaxValue && GameState.Get() != null)
			{
				Player myPlayer = GameState.Get().GetFriendlySidePlayer();
				if (myPlayer != null && myPlayer.GetHero() != null)
				{
					m_place = myPlayer.GetHero().GetRealTimePlayerLeaderboardPlace();
				}
			}
			return m_place;
		}
	}

	protected override void Awake()
	{
		base.Awake();
		m_gamesWonIndicator.Hide();
		if (ShouldMakeUtilRequests())
		{
			NetCache.Get().RegisterScreenEndOfGame(OnNetCacheReady);
		}
	}

	protected override void ShowStandardFlow()
	{
		base.ShowStandardFlow();
		m_hitbox.AddEventListener(UIEventType.RELEASE, base.ContinueButtonPress_PrevMode);
	}

	protected override void OnTwoScoopShown()
	{
		if (BnetBar.Get() != null)
		{
			BnetBar.Get().SuppressLoginTooltip(val: true);
		}
		if (m_showWinProgress)
		{
			m_gamesWonIndicator.Show();
		}
	}

	protected override void OnTwoScoopHidden()
	{
		if (m_showWinProgress)
		{
			m_gamesWonIndicator.Hide();
		}
	}

	protected override void InitGoldRewardUI()
	{
		m_showWinProgress = Place <= 4;
	}

	protected override void ShowAppRatingPrompt()
	{
		if (!m_hasAlreadyCheckedForAppRating)
		{
			m_hasAlreadyCheckedForAppRating = true;
			if (!GameMgr.Get().IsBattlegroundsTutorial() && Place <= 4)
			{
				MobileCallbackManager.RequestAppReview(AppRatingPromptTrigger.BATTLEGROUNDS_WIN);
			}
		}
	}
}
