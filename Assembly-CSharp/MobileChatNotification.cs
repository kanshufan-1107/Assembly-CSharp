using System.ComponentModel;
using UnityEngine;

public class MobileChatNotification : MonoBehaviour
{
	public delegate void NotifiedEvent(string text);

	private enum State
	{
		None,
		[Description("GLOBAL_MOBILECHAT_NOTIFICATION_MULLIGAIN")]
		GameStarted,
		[Description("GLOBAL_MOBILECHAT_NOTIFICATION_YOUR_TURN")]
		YourTurn,
		[Description("GLOBAL_MOBILECHAT_NOTIFICATION_TURN_COUNTDOWN")]
		TurnCountdown
	}

	private State state;

	public event NotifiedEvent Notified;

	private void OnEnable()
	{
		state = State.None;
	}

	private void OnDestroy()
	{
		if (GameState.Get() != null && !SpectatorManager.Get().IsSpectatingOrWatching)
		{
			GameState.Get().UnregisterTurnChangedListener(OnTurnChanged);
		}
		TurnTimer turnTimer = TurnTimer.Get();
		if (turnTimer != null)
		{
			turnTimer.OnStateChanged -= OnTurnTimerUpdate;
		}
	}

	private void Update()
	{
		if (GameState.Get() == null || SpectatorManager.Get().IsSpectatingOrWatching)
		{
			state = State.None;
			return;
		}
		GameState gameState = GameState.Get();
		gameState.RegisterTurnChangedListener(OnTurnChanged);
		if (gameState.IsMulliganPhase())
		{
			if (state == State.None)
			{
				state = State.GameStarted;
				FireNotification();
			}
			return;
		}
		if (state == State.GameStarted)
		{
			state = (GameState.Get().IsFriendlySidePlayerTurn() ? State.YourTurn : State.None);
			FireNotification();
		}
		TurnTimer turnTimer = TurnTimer.Get();
		if (turnTimer != null)
		{
			turnTimer.OnStateChanged -= OnTurnTimerUpdate;
			turnTimer.OnStateChanged += OnTurnTimerUpdate;
		}
	}

	private string GetStateText(State state)
	{
		if (state == State.None)
		{
			return string.Empty;
		}
		return GameStrings.Get((typeof(State).GetField(state.ToString()).GetCustomAttributes(inherit: false)[0] as DescriptionAttribute).Description);
	}

	private void OnTurnChanged(int oldTurn, int newTurn, object userData)
	{
		bool isFriendlyTurn = GameState.Get().IsFriendlySidePlayerTurn();
		if (isFriendlyTurn && state != State.YourTurn)
		{
			state = State.YourTurn;
			FireNotification();
		}
		else if (!isFriendlyTurn && state == State.YourTurn)
		{
			state = State.None;
		}
	}

	private void OnTurnTimerUpdate(TurnTimerState turnTimerState)
	{
		if (turnTimerState == TurnTimerState.COUNTDOWN && GameState.Get().IsFriendlySidePlayerTurn() && state != State.TurnCountdown)
		{
			state = State.TurnCountdown;
			FireNotification();
		}
	}

	private void FireNotification()
	{
		if (this.Notified != null && state != 0)
		{
			this.Notified(GetStateText(state));
		}
	}
}
