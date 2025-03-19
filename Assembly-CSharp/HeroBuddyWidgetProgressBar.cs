public class HeroBuddyWidgetProgressBar : HeroBuddyWidgetBase
{
	public ProgressBar m_progressBarLeft;

	public ProgressBar m_progressBarRight;

	public UberText m_ProgressTextFriendly;

	public UberText m_ProgressTextEnemy;

	public bool m_alwaysShowProgressText;

	private float m_currentProgressValue;

	private UberText m_ProgressText;

	private bool m_showProgressText;

	private bool m_initialized;

	private void Init()
	{
		if (m_initialized)
		{
			return;
		}
		GameState gameState = GameState.Get();
		if (gameState == null)
		{
			return;
		}
		Actor actor = base.gameObject.GetComponent<Actor>();
		if (!(actor == null) && actor.GetEntity() != null)
		{
			Entity hero = (actor.GetEntity().IsControlledByOpposingSidePlayer() ? gameState.GetOpposingSidePlayer() : gameState.GetFriendlySidePlayer())?.GetHero();
			int numBuddiesGained = hero?.GetTag(GAME_TAG.BACON_PLAYER_NUM_HERO_BUDDIES_GAINED) ?? 0;
			int currentProgress = hero?.GetTag(GAME_TAG.BACON_HERO_BUDDY_PROGRESS) ?? 0;
			int barProgress = 100 * numBuddiesGained + currentProgress;
			if (barProgress > 200)
			{
				barProgress = 200;
			}
			UpdateProgressBar((float)barProgress / 200f);
			m_initialized = true;
		}
	}

	public void ShowProgressText(bool value)
	{
		m_showProgressText = value;
	}

	public void UpdateProgressBar(float newValue)
	{
		if (!(m_progressBarLeft == null) && !(m_progressBarRight == null))
		{
			if (newValue < 0.5f || m_currentProgressValue < 0.5f)
			{
				m_progressBarLeft.AnimateProgress(m_currentProgressValue, newValue);
			}
			if (newValue > 0.5f || m_currentProgressValue > 0.5f)
			{
				m_progressBarRight.AnimateProgress(m_currentProgressValue, newValue);
			}
			m_currentProgressValue = newValue;
			if (m_ProgressText == null)
			{
				SetProgressText();
			}
			if (m_ProgressText != null)
			{
				m_ProgressText.Text = $"{(((double)m_currentProgressValue < 0.5) ? ((double)(200f * m_currentProgressValue)) : (200.0 * ((double)m_currentProgressValue - 0.5))):0}%";
			}
		}
	}

	protected override void LateUpdate()
	{
		Init();
		UpdateProgressBarVisibility();
		base.LateUpdate();
	}

	private void SetProgressText()
	{
		Actor actor = base.gameObject.GetComponent<Actor>();
		Player.Side playerSide = Player.Side.FRIENDLY;
		if (actor != null && actor.GetEntity() != null && actor.GetEntity().IsControlledByOpposingSidePlayer())
		{
			playerSide = Player.Side.OPPOSING;
		}
		m_ProgressText = ((playerSide == Player.Side.FRIENDLY) ? m_ProgressTextFriendly : m_ProgressTextEnemy);
	}

	private void UpdateProgressBarVisibility()
	{
		SetProgressText();
		if (m_ProgressText != null)
		{
			m_ProgressText.gameObject.SetActive(m_alwaysShowProgressText || m_showProgressText);
		}
		else
		{
			Log.All.Print("Hero Buddy ProgressText is null");
		}
	}
}
