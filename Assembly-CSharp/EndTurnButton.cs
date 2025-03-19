using System;
using System.Collections;
using System.Collections.Generic;
using Blizzard.T5.MaterialService.Extensions;
using UnityEngine;

public class EndTurnButton : MonoBehaviour
{
	public delegate void OnButtonUnblocked(object userData);

	private class ButtonUnblockedListener : EventListener<OnButtonUnblocked>
	{
		public void Fire()
		{
			m_callback(m_userData);
		}
	}

	public ActorStateMgr m_ActorStateMgr;

	public UberText m_MyTurnText;

	public UberText m_WaitingText;

	public GameObject m_GreenHighlight;

	public GameObject m_WhiteHighlight;

	public GameObject m_EndTurnButtonMesh;

	public List<Material> m_AlternativeMaterials;

	private static EndTurnButton s_instance;

	private bool m_inputBlockedInternally;

	private bool m_pressed;

	private bool m_playedNmpSoundThisTurn;

	private bool m_mousedOver;

	private bool m_disabled;

	private int m_inputBlockers;

	private List<ButtonUnblockedListener> m_buttonUnblockedListeners = new List<ButtonUnblockedListener>();

	private Notification m_arrow;

	private bool m_canShowBouncingArrow;

	public bool IsDisabled => m_disabled;

	private bool InputBlockedInternally
	{
		get
		{
			return m_inputBlockedInternally;
		}
		set
		{
			bool num = IsInputBlocked();
			m_inputBlockedInternally = value;
			bool isBlocked = IsInputBlocked();
			if (num && !isBlocked)
			{
				FireButtonUnblockedEvent();
			}
		}
	}

	public event Action<ActorStateType> OnButtonStateChanged;

	private void Awake()
	{
		s_instance = this;
		m_MyTurnText.Text = GetEndTurnText();
		m_WaitingText.Text = "";
		GetComponent<Collider>().enabled = false;
	}

	private void OnDestroy()
	{
		s_instance = null;
	}

	private void Start()
	{
		StartCoroutine(WaitAFrameAndThenChangeState());
	}

	public bool RegisterButtonUnblockedListener(OnButtonUnblocked callback)
	{
		ButtonUnblockedListener listener = new ButtonUnblockedListener();
		listener.SetCallback(callback);
		if (m_buttonUnblockedListeners.Contains(listener))
		{
			return false;
		}
		m_buttonUnblockedListeners.Add(listener);
		return true;
	}

	public bool UnregisterButtonUnblockedListener(OnButtonUnblocked callback)
	{
		ButtonUnblockedListener listener = new ButtonUnblockedListener();
		listener.SetCallback(callback);
		return m_buttonUnblockedListeners.Remove(listener);
	}

	private void FireButtonUnblockedEvent()
	{
		ButtonUnblockedListener[] array = m_buttonUnblockedListeners.ToArray();
		for (int i = 0; i < array.Length; i++)
		{
			array[i].Fire();
		}
	}

	public static EndTurnButton Get()
	{
		return s_instance;
	}

	public void Reset()
	{
		bool friendlyExtraTurn = HasExtraTurn();
		TurnStartManager.Get().NotifyOfExtraTurn(ExtraTurnSpellConfig.Get().GetSpell(), !friendlyExtraTurn);
		bool opponentExtraTurn = OpponentHasExtraTurn();
		TurnStartManager.Get().NotifyOfExtraTurn(ExtraTurnSpellConfig.Get().GetSpell(isFriendly: false), !opponentExtraTurn, isFriendly: false);
		UpdateState();
		GameState state = GameState.Get();
		Collider endTurnCollider = GetComponent<Collider>();
		if (state.IsPastBeginPhase() && state.IsLocalSidePlayerTurn())
		{
			endTurnCollider.enabled = true;
		}
		else
		{
			endTurnCollider.enabled = false;
		}
	}

	public GameObject GetButtonContainer()
	{
		return base.transform.Find("ButtonContainer").gameObject;
	}

	public void PlayPushDownAnimation()
	{
		if (!InputBlockedInternally && !IsInWaitingState() && !m_pressed)
		{
			m_pressed = true;
			GetButtonContainer().GetComponent<Animation>().Play("ENDTURN_PRESSED_DOWN");
			SoundManager.Get().LoadAndPlay("FX_EndTurn_Down.prefab:7f967e178760e5d409cec10ad56cc3ff");
		}
	}

	public void PlayButtonUpAnimation()
	{
		if (!InputBlockedInternally && !IsInWaitingState() && m_pressed)
		{
			m_pressed = false;
			GetButtonContainer().GetComponent<Animation>().Play("ENDTURN_PRESSED_UP");
			SoundManager.Get().LoadAndPlay("FX_EndTurn_Up.prefab:aa092f360d27b5244b030e737d720ba6");
		}
	}

	public bool IsInWaitingState()
	{
		return m_ActorStateMgr.GetActiveStateType() switch
		{
			ActorStateType.ENDTURN_WAITING => true, 
			ActorStateType.ENDTURN_NMP_2_WAITING => true, 
			ActorStateType.ENDTURN_WAITING_TIMER => true, 
			_ => false, 
		};
	}

	public bool IsInNMPState()
	{
		return m_ActorStateMgr.GetActiveStateType() switch
		{
			ActorStateType.ENDTURN_NO_MORE_PLAYS => true, 
			ActorStateType.EXTRATURN_NO_MORE_PLAYS => true, 
			_ => false, 
		};
	}

	public bool IsInYouHavePlaysState()
	{
		return m_ActorStateMgr.GetActiveStateType() switch
		{
			ActorStateType.ENDTURN_YOUR_TURN => true, 
			ActorStateType.EXTRATURN_YOUR_TURN => true, 
			_ => false, 
		};
	}

	private bool OnlyValidOptionsAreAbortStarshipLaunchOrEndTurn(Network.Options options)
	{
		if (options == null)
		{
			return false;
		}
		GameState gs = GameState.Get();
		if (gs == null)
		{
			return false;
		}
		foreach (Network.Options.Option option in options.List)
		{
			if (option.Type == Network.Options.Option.OptionType.END_TURN)
			{
				continue;
			}
			Entity mainOptionEntity = gs.GetEntity(option.Main.ID);
			if (mainOptionEntity == null)
			{
				continue;
			}
			bool hasSuboptions = false;
			if (option.Subs != null)
			{
				hasSuboptions = option.Subs.Count > 0;
			}
			if (mainOptionEntity.HasTag(GAME_TAG.LAUNCHPAD) && hasSuboptions)
			{
				foreach (Network.Options.Option.SubOption suboption in option.Subs)
				{
					if (!(gs.GetEntity(suboption.ID).GetCardId() == "GDB_906") && suboption.PlayErrorInfo.IsValid())
					{
						return false;
					}
				}
			}
			else if (option.Main.PlayErrorInfo.IsValid())
			{
				return false;
			}
		}
		return true;
	}

	public bool HasNoMorePlays()
	{
		GameEntity gameEntity = GameState.Get().GetGameEntity();
		if (gameEntity != null && gameEntity.ShouldOverwriteEndTurnButtonNoMorePlaysState(out var hasNoMorePlays))
		{
			return hasNoMorePlays;
		}
		Network.Options options = GameState.Get().GetOptionsPacket();
		if (options == null)
		{
			return false;
		}
		return OnlyValidOptionsAreAbortStarshipLaunchOrEndTurn(options);
	}

	public bool IsInputBlocked()
	{
		if (!InputBlockedInternally)
		{
			return m_inputBlockers > 0;
		}
		return true;
	}

	public void AddInputBlocker()
	{
		m_inputBlockers++;
	}

	public void RemoveInputBlocker()
	{
		bool num = IsInputBlocked();
		m_inputBlockers--;
		bool isBlocked = IsInputBlocked();
		if (num && !isBlocked)
		{
			FireButtonUnblockedEvent();
		}
	}

	public void HandleMouseOver()
	{
		m_mousedOver = true;
		if (!InputBlockedInternally)
		{
			PutInMouseOverState();
		}
	}

	public void HandleMouseOut()
	{
		m_mousedOver = false;
		if (!InputBlockedInternally)
		{
			if (m_pressed)
			{
				PlayButtonUpAnimation();
			}
			PutInMouseOffState();
		}
	}

	private void PutInMouseOverState()
	{
		if (IsInNMPState())
		{
			m_WhiteHighlight.SetActive(value: false);
			m_GreenHighlight.SetActive(value: true);
			Hashtable tweenArgs = iTweenManager.Get().GetTweenHashTable();
			tweenArgs.Add("from", m_GreenHighlight.GetComponent<Renderer>().GetMaterial().GetFloat("_Intensity"));
			tweenArgs.Add("to", 1.4f);
			tweenArgs.Add("time", 0.15f);
			tweenArgs.Add("easetype", iTween.EaseType.linear);
			tweenArgs.Add("onupdate", "OnUpdateIntensityValue");
			tweenArgs.Add("onupdatetarget", base.gameObject);
			tweenArgs.Add("name", "ENDTURN_INTENSITY");
			iTween.StopByName(base.gameObject, "ENDTURN_INTENSITY");
			iTween.ValueTo(base.gameObject, tweenArgs);
		}
		else if (IsInYouHavePlaysState())
		{
			m_WhiteHighlight.SetActive(value: true);
			m_GreenHighlight.SetActive(value: false);
		}
		else
		{
			m_WhiteHighlight.SetActive(value: false);
			m_GreenHighlight.SetActive(value: false);
		}
	}

	private void PutInMouseOffState()
	{
		m_WhiteHighlight.SetActive(value: false);
		if (IsInNMPState())
		{
			m_GreenHighlight.SetActive(value: true);
			Hashtable tweenArgs = iTweenManager.Get().GetTweenHashTable();
			tweenArgs.Add("from", m_GreenHighlight.GetComponent<Renderer>().GetMaterial().GetFloat("_Intensity"));
			tweenArgs.Add("to", 1.1f);
			tweenArgs.Add("time", 0.15f);
			tweenArgs.Add("easetype", iTween.EaseType.linear);
			tweenArgs.Add("onupdate", "OnUpdateIntensityValue");
			tweenArgs.Add("onupdatetarget", base.gameObject);
			tweenArgs.Add("name", "ENDTURN_INTENSITY");
			iTween.StopByName(base.gameObject, "ENDTURN_INTENSITY");
			iTween.ValueTo(base.gameObject, tweenArgs);
		}
		else
		{
			m_GreenHighlight.SetActive(value: false);
		}
	}

	private void OnUpdateIntensityValue(float newValue)
	{
		m_GreenHighlight.GetComponent<Renderer>().GetMaterial().SetFloat("_Intensity", newValue);
	}

	private IEnumerator WaitAFrameAndThenChangeState()
	{
		yield return null;
		if (GameState.Get() == null)
		{
			Log.Gameplay.PrintError("EndTurnButton.WaitAFrameAndThenChangeState(): Game state does not exist.");
			yield break;
		}
		if (GameState.Get().IsGameCreated())
		{
			HandleGameStart();
			yield break;
		}
		m_ActorStateMgr.ChangeState(ActorStateType.ENDTURN_WAITING);
		GameState.Get().RegisterCreateGameListener(OnCreateGame);
	}

	private void HandleGameStart()
	{
		UpdateState();
		ApplyAlternativeAppearance();
		GameState state = GameState.Get();
		if (state.IsPastBeginPhase() && state.IsLocalSidePlayerTurn())
		{
			GetComponent<Collider>().enabled = true;
			GameState.Get().RegisterOptionsReceivedListener(OnOptionsReceived);
		}
	}

	private int GetCurrentAlternativeAppearanceIndex()
	{
		GameState state = GameState.Get();
		if (state == null)
		{
			return 0;
		}
		return state.GetGameEntity()?.GetTag(GAME_TAG.END_TURN_BUTTON_ALTERNATIVE_APPEARANCE) ?? 0;
	}

	public void ApplyAlternativeAppearance()
	{
		int alternativeAppearance = GetCurrentAlternativeAppearanceIndex();
		if (alternativeAppearance != 1)
		{
			_ = 2;
			return;
		}
		if (m_AlternativeMaterials.Count >= alternativeAppearance && m_AlternativeMaterials[alternativeAppearance - 1] != null)
		{
			m_EndTurnButtonMesh.GetComponent<Renderer>().SetMaterial(m_AlternativeMaterials[alternativeAppearance - 1]);
		}
		else
		{
			Log.Gameplay.PrintError("EndTurnButton.ApplyAlternativeAppearance(): No material exists for appearance  {0}.", alternativeAppearance);
		}
		UpdateButtonText();
	}

	private void SetButtonState(ActorStateType stateType)
	{
		if (m_ActorStateMgr == null)
		{
			Debug.Log("End Turn Button Actor State Manager is missing!");
		}
		else if (m_ActorStateMgr.GetActiveStateType() != stateType && (!IsInputBlocked() || stateType == ActorStateType.ENDTURN_NO_MORE_PLAYS) && (!m_disabled || stateType == ActorStateType.ENDTURN_WAITING))
		{
			m_ActorStateMgr.ChangeState(stateType);
			this.OnButtonStateChanged?.Invoke(stateType);
			if (stateType == ActorStateType.ENDTURN_YOUR_TURN || stateType == ActorStateType.ENDTURN_WAITING_TIMER)
			{
				InputBlockedInternally = true;
				StartCoroutine(WaitUntilAnimationIsCompleteAndThenUnblockInput(stateType));
			}
		}
	}

	private IEnumerator WaitUntilAnimationIsCompleteAndThenUnblockInput(ActorStateType stateType)
	{
		yield return new WaitForSeconds(m_ActorStateMgr.GetMaximumAnimationTimeOfActiveStates());
		InputBlockedInternally = false;
		if (stateType == ActorStateType.ENDTURN_YOUR_TURN)
		{
			m_EndTurnButtonMesh.transform.localEulerAngles = Vector3.zero;
			if (HasNoMorePlays())
			{
				SetStateToNoMorePlays();
			}
		}
	}

	private void UpdateState()
	{
		if (!GameState.Get().IsMulliganManagerActive() && !GameState.Get().IsTurnStartManagerBlockingInput())
		{
			if (!GameState.Get().IsLocalSidePlayerTurn() || !GameState.Get().GetGameEntity().IsCurrentTurnRealTime())
			{
				UpdateButtonText();
				SetStateToWaiting();
			}
			else if (GameState.Get().GetResponseMode() != 0)
			{
				SetStateToYourTurn();
			}
		}
	}

	public void DisplayExtraTurnState()
	{
		UpdateState();
	}

	private bool HasExtraTurn()
	{
		return GameState.Get().GetFriendlySidePlayer().GetTag(GAME_TAG.NUM_TURNS_LEFT) > 1;
	}

	private bool OpponentHasExtraTurn()
	{
		return GameState.Get().GetOpposingSidePlayer().GetTag(GAME_TAG.NUM_TURNS_LEFT) > 1;
	}

	private ActorStateType GetAppropriateYourTurnState()
	{
		if (HasExtraTurn())
		{
			if (IsInWaitingState())
			{
				return ActorStateType.WAITING_TO_EXTRATURN;
			}
			return ActorStateType.EXTRATURN_YOUR_TURN;
		}
		return ActorStateType.ENDTURN_YOUR_TURN;
	}

	private ActorStateType GetAppropriateYourTurnNMPState()
	{
		if (HasExtraTurn())
		{
			return ActorStateType.EXTRATURN_NO_MORE_PLAYS;
		}
		return ActorStateType.ENDTURN_NO_MORE_PLAYS;
	}

	private string GetEndTurnText()
	{
		switch (GetCurrentAlternativeAppearanceIndex())
		{
		case 1:
		case 3:
			return "";
		case 2:
			return GameStrings.Get("GAMEPLAY_DONE_TURN");
		default:
			return GameStrings.Get("GAMEPLAY_END_TURN");
		}
	}

	private string GetEnemyTurnText()
	{
		if (GameState.Get().GetGameEntity().GetAlternativeEndTurnButtonText(out var _, out var waitingText))
		{
			return waitingText;
		}
		int alternativeAppearance = GetCurrentAlternativeAppearanceIndex();
		if ((uint)(alternativeAppearance - 1) <= 2u)
		{
			return "";
		}
		return GameStrings.Get("GAMEPLAY_ENEMY_TURN");
	}

	public void UpdateButtonText()
	{
		if (GameState.Get().GetGameEntity().GetAlternativeEndTurnButtonText(out var myTurnText, out var waitingText))
		{
			m_MyTurnText.SetText(GameStrings.Get(myTurnText));
			m_WaitingText.SetText(GameStrings.Get(waitingText));
		}
		else
		{
			switch (GetCurrentAlternativeAppearanceIndex())
			{
			case 1:
				m_MyTurnText.SetText(GameStrings.Get(""));
				m_WaitingText.SetText(GameStrings.Get(""));
				break;
			case 2:
				m_MyTurnText.SetText(GameStrings.Get("GAMEPLAY_DONE_TURN"));
				m_WaitingText.SetText(GameStrings.Get(""));
				break;
			case 3:
				m_MyTurnText.SetText(GameStrings.Get(""));
				m_WaitingText.SetText(GameStrings.Get(""));
				break;
			default:
				if (HasExtraTurn())
				{
					m_MyTurnText.SetText(GameStrings.Get("GAMEPLAY_NEXT_TURN"));
					m_WaitingText.SetText(GameStrings.Get("GAMEPLAY_NEXT_TURN"));
				}
				else
				{
					m_MyTurnText.SetText(GameStrings.Get("GAMEPLAY_END_TURN"));
					m_WaitingText.SetText(GameStrings.Get("GAMEPLAY_ENEMY_TURN"));
				}
				break;
			}
		}
		m_MyTurnText.UpdateText();
		m_WaitingText.UpdateText();
	}

	private void SetStateToYourTurn()
	{
		UpdateButtonText();
		if (m_ActorStateMgr == null)
		{
			return;
		}
		if (HasNoMorePlays())
		{
			if (GameState.Get().GetBooleanGameOption(GameEntityOption.FLIP_END_TURN_BUTTON_WHEN_ENTERING_NO_MORE_PLAY) && !IsInNMPState())
			{
				SetStateToWaiting();
			}
			SetStateToNoMorePlays();
		}
		else
		{
			SetButtonState(GetAppropriateYourTurnState());
			if (m_mousedOver)
			{
				PutInMouseOverState();
			}
			else
			{
				PutInMouseOffState();
			}
		}
	}

	private void SetStateToNoMorePlays()
	{
		if (m_ActorStateMgr == null)
		{
			return;
		}
		if (IsInWaitingState())
		{
			SetButtonState(GetAppropriateYourTurnState());
		}
		else
		{
			SetButtonState(GetAppropriateYourTurnNMPState());
			if (m_mousedOver)
			{
				PutInMouseOverState();
			}
			else
			{
				PutInMouseOffState();
			}
		}
		if (!m_playedNmpSoundThisTurn && !GameState.Get().GetGameEntity().HasTag(GAME_TAG.SUPPRESS_JOBS_DONE_VO))
		{
			m_playedNmpSoundThisTurn = true;
			StartCoroutine(PlayEndTurnSound());
		}
	}

	private void SetStateToWaiting()
	{
		if (!(m_ActorStateMgr == null) && !IsInWaitingState() && !GameState.Get().IsGameOver())
		{
			if (IsInNMPState())
			{
				SetButtonState(ActorStateType.ENDTURN_NMP_2_WAITING);
			}
			else
			{
				SetButtonState(ActorStateType.ENDTURN_WAITING);
			}
			PutInMouseOffState();
		}
	}

	private IEnumerator PlayEndTurnSound()
	{
		yield return new WaitForSeconds(1.5f);
		if (IsInNMPState())
		{
			SoundManager.Get().LoadAndPlay("VO_JobsDone.prefab:88cda3fac32785c4d8101966b7604cc3", base.gameObject);
		}
	}

	private void OnCreateGame(GameState.CreateGamePhase phase, object userData)
	{
		if (phase == GameState.CreateGamePhase.CREATED)
		{
			GameState.Get().UnregisterCreateGameListener(OnCreateGame);
			HandleGameStart();
		}
	}

	public void OnMulliganEnded()
	{
		m_WaitingText.Text = GetEnemyTurnText();
	}

	public void OnTurnStartManagerFinished()
	{
		if (GameState.Get().GetGameEntity().IsCurrentTurnRealTime())
		{
			PegCursor.Get().SetMode(PegCursor.Mode.STOPWAITING);
			m_playedNmpSoundThisTurn = false;
			SetStateToYourTurn();
			GetComponent<Collider>().enabled = true;
			GameState.Get().RegisterOptionsReceivedListener(OnOptionsReceived);
		}
	}

	public void OnTurnChanged()
	{
		UpdateState();
	}

	public void OnEndTurnRequested()
	{
		PegCursor.Get().SetMode(PegCursor.Mode.WAITING);
		SetStateToWaiting();
		GetComponent<Collider>().enabled = false;
		GameState.Get().UnregisterOptionsReceivedListener(OnOptionsReceived);
	}

	private void OnOptionsReceived(object userData)
	{
		UpdateState();
	}

	public void OnTurnTimerStart()
	{
		if (!InputBlockedInternally)
		{
			_ = m_mousedOver;
		}
	}

	public void OnTurnTimerEnded(bool isFriendlyPlayerTurnTimer)
	{
		if (isFriendlyPlayerTurnTimer)
		{
			SetButtonState(ActorStateType.ENDTURN_WAITING_TIMER);
		}
	}

	public void SetDisabled(bool disabled)
	{
		m_disabled = disabled;
		if (m_disabled)
		{
			SetButtonState(ActorStateType.ENDTURN_WAITING);
		}
	}

	public void ShowEndTurnBouncingArrowButtonAfterWait(float waitTime, Vector3 offset)
	{
		m_canShowBouncingArrow = true;
		StartCoroutine(WaitAndShowBouncingOnArrowEndTurn(waitTime, offset));
	}

	private IEnumerator WaitAndShowBouncingOnArrowEndTurn(float waitTime, Vector3 offset)
	{
		if (IsInWaitingState())
		{
			yield return null;
			yield break;
		}
		yield return new WaitForSeconds(waitTime);
		if (m_arrow == null && m_canShowBouncingArrow)
		{
			m_canShowBouncingArrow = true;
			Vector3 endTurnPos = base.transform.position;
			Vector3 arrowPos = new Vector3(endTurnPos.x - 2f, endTurnPos.y, endTurnPos.z) + offset;
			m_arrow = NotificationManager.Get().CreateBouncingArrow(UserAttentionBlocker.NONE, arrowPos, new Vector3(0f, -90f, 0f));
		}
	}

	public void HideEndTurnBouncingArrow()
	{
		if (m_arrow != null)
		{
			NotificationManager.Get().DestroyNotification(m_arrow, 0f);
			m_arrow = null;
		}
		m_canShowBouncingArrow = false;
	}
}
