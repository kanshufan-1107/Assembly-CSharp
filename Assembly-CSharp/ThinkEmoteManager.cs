using UnityEngine;

public class ThinkEmoteManager : MonoBehaviour
{
	private float m_secondsSinceAction;

	public const float DEFAULT_THINK_EMOTE_DELAY = 20f;

	private static ThinkEmoteManager s_instance;

	private void Awake()
	{
		s_instance = this;
	}

	private void OnDestroy()
	{
		s_instance = null;
	}

	public static ThinkEmoteManager Get()
	{
		return s_instance;
	}

	private void Update()
	{
		GameState state = GameState.Get();
		if (state != null && state.IsMainPhase())
		{
			float? secondsBeforeEmote = state.GetGameEntity().GetThinkEmoteDelayOverride();
			if (!secondsBeforeEmote.HasValue)
			{
				secondsBeforeEmote = 20f;
			}
			m_secondsSinceAction += Time.deltaTime;
			if (m_secondsSinceAction > secondsBeforeEmote && !TurnTimer.Get().IsRopeActive() && (!EndTurnButton.Get().IsInWaitingState() || GameMgr.Get().IsBattlegrounds()))
			{
				PlayThinkEmote();
			}
		}
	}

	private void PlayThinkEmote()
	{
		m_secondsSinceAction = 0f;
		GameState.Get().GetGameEntity().OnPlayThinkEmote();
	}

	public void NotifyOfActivity()
	{
		m_secondsSinceAction = 0f;
	}
}
