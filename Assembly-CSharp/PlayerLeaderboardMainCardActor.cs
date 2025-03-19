using Hearthstone.UI;
using UnityEngine;

[CustomEditClass]
public class PlayerLeaderboardMainCardActor : Actor
{
	[CustomEditField(Sections = "Player Leaderboard Main Actor Fields")]
	public UberText m_playerNameText;

	[CustomEditField(Sections = "Player Leaderboard Main Actor Fields")]
	public UberText m_alternateNameText;

	[CustomEditField(Sections = "Player Leaderboard Main Actor Fields")]
	public GameObject m_playerNameBackground;

	[CustomEditField(Sections = "Player Leaderboard Main Actor Fields")]
	public GameObject m_fullSelectionHighlight;

	[CustomEditField(Sections = "Player Leaderboard Main Actor Fields")]
	public GameObject m_confirmSelectionHighlight;

	[CustomEditField(Sections = "Player Leaderboard Main Actor Fields")]
	public GameObject m_lockIcon;

	[CustomEditField(Sections = "Player Leaderboard Main Actor Fields")]
	public GameObject m_lockedHeroBackground;

	[CustomEditField(Sections = "Player Leaderboard Main Actor Fields")]
	public GameObject m_playerLeaderboardShadow;

	[CustomEditField(Sections = "Player Leaderboard Main Actor Fields")]
	public AsyncReference m_rerollButtonReference;

	[CustomEditField(Sections = "Player Leaderboard Main Actor Fields")]
	public GameObject m_partiallyRevealedLockIcon;

	[CustomEditField(Sections = "Player Leaderboard Main Actor Fields")]
	public GameObject m_mulliganBanner;

	[CustomEditField(Sections = "Player Leaderboard Main Actor Fields")]
	public bool m_showNumbersForRerollButton;

	private const string BACON_ALTERNATE_NAME_STRING_ID = "GAMEPLAY_BACON_ALTERNATE_PLAYER_NAME";

	private UberText m_pausedHealthTextMesh;

	private bool m_showHeroRerollButton;

	private bool m_HeroRerollButtonEnabled;

	private BaconRerollButton m_heroRerollButton;

	private bool m_forcedDisabledAfterUse;

	private readonly Vector3 REROLL_BUTTON_TOOLTIP_OFFSET = new Vector3(-2.8f, 1f, 0.8f);

	private void Start()
	{
		m_rerollButtonReference.RegisterReadyListener<VisualController>(OnRerollButtonReady);
	}

	public void OnRerollButtonReady(VisualController buttonVisualController)
	{
		if (buttonVisualController == null)
		{
			Error.AddDevWarning("UI Error!", "Reroll Button could not be found! You will not be able to click 'Reroll'!");
			return;
		}
		m_heroRerollButton = buttonVisualController.gameObject.GetComponent<BaconRerollButton>();
		m_heroRerollButton.AddEventListener(UIEventType.RELEASE, OnMulliganHeroRerollButtonReleased);
		m_heroRerollButton.SetEntity(m_entity);
		m_heroRerollButton.RegisterTooltipListeners();
		m_heroRerollButton.EnableTooltipListener(enabled: false);
		m_heroRerollButton.gameObject.SetActive(value: false);
		UpdateMulliganRerollButton(null);
	}

	public void SetShowHeroRerollButton(bool show, bool? overrideEnabled = null)
	{
		m_showHeroRerollButton = show;
		UpdateMulliganRerollButton(overrideEnabled);
	}

	public bool ShowHeroRerollButton()
	{
		return m_showHeroRerollButton;
	}

	public BaconRerollButton GetHeroRerollButton()
	{
		return m_heroRerollButton;
	}

	private void ShowRerollButtonTooltip(TooltipZone tooltipZone, string tooltipHeader, string tooltipDescription)
	{
		TooltipPanel tooltipPanel = tooltipZone.ShowTooltip(tooltipHeader, tooltipDescription, 0.7f);
		LayerUtils.SetLayer(tooltipPanel.gameObject, GameLayer.Tooltip);
		tooltipPanel.transform.localPosition = new Vector3(0f, 0f, 1.3f);
		tooltipPanel.transform.localEulerAngles = new Vector3(0f, 0f, 0f);
		TransformUtil.SetPoint(tooltipPanel, Anchor.BOTTOM, base.gameObject, Anchor.TOP, REROLL_BUTTON_TOOLTIP_OFFSET);
	}

	private void OnMulliganHeroRerollButtonReleased(UIEvent e)
	{
		if (!InputManager.Get().PermitDecisionMakingInput())
		{
			return;
		}
		MulliganManager mulliganManager = MulliganManager.Get();
		if (mulliganManager == null || !mulliganManager.IsMulliganActive())
		{
			Log.All.Print("Error: Mulligan Hero Reroll button pressed when mulligan is not active");
			return;
		}
		if (m_entity != null && m_entity.GetCard() != null && m_entity.GetCard().GetActor() != null)
		{
			m_entity.GetCard().GetActor().RemovePing();
		}
		mulliganManager.RequestHeroReroll(m_entity);
		m_forcedDisabledAfterUse = true;
		UpdateMulliganRerollButton(null);
	}

	private void UpdateRerollButtonText()
	{
		if (m_heroRerollButton == null)
		{
			return;
		}
		if (m_partiallyRevealedLockIcon != null && m_partiallyRevealedLockIcon.activeSelf)
		{
			m_heroRerollButton.SetText(GameStrings.Get("GLUE_BACON_REROLL_UNLOCK"));
			m_heroRerollButton.DisableCostText();
			return;
		}
		Entity gameEntity = GameState.Get().GetGameEntity();
		if (m_entity != null && gameEntity != null)
		{
			int rerollCost = ((m_entity.BaconFreeRerollLeft() <= 0) ? 1 : 0);
			m_heroRerollButton.SetCostText(rerollCost);
			if (m_showNumbersForRerollButton)
			{
				int rerollsUsed = ((m_entity != null) ? m_entity.GetTag(GAME_TAG.BACON_NUM_MULLIGAN_REFRESH_USED) : 0);
				int maxReroll = gameEntity.GetTag(GAME_TAG.BACON_NUM_MAX_REROLL_PER_HERO);
				m_heroRerollButton.SetText($"{rerollsUsed}/{maxReroll}");
			}
			else
			{
				m_heroRerollButton.SetText("GLUE_BACON_REROLL");
			}
		}
	}

	private void EnableRerollButton(bool enable)
	{
		if (!(m_heroRerollButton == null))
		{
			m_heroRerollButton.EnableTooltipListener(!enable);
			enable &= !m_forcedDisabledAfterUse;
			m_heroRerollButton.SetEnabled(enable);
			Clickable clickable = m_heroRerollButton.GetComponentInChildren<Clickable>();
			if (clickable != null)
			{
				clickable.Active = enable;
			}
		}
	}

	public void UpdateRerollButtonEnabledState(bool? overrideEnabled = null)
	{
		m_HeroRerollButtonEnabled = false;
		if (overrideEnabled.HasValue)
		{
			m_HeroRerollButtonEnabled = overrideEnabled.Value;
		}
		else if (m_entity != null)
		{
			m_HeroRerollButtonEnabled = m_entity.ShouldEnableRerollButton(null, null) <= Entity.RerollButtonEnableResult.UNLOCK;
		}
		EnableRerollButton(m_HeroRerollButtonEnabled);
	}

	public void ClearRerollButtonForcedDisabledState()
	{
		m_forcedDisabledAfterUse = false;
	}

	public void UpdateMulliganRerollButton(bool? overrideEnabled = null, bool clearForceDisabledState = false)
	{
		if (clearForceDisabledState)
		{
			m_forcedDisabledAfterUse = false;
		}
		if (m_heroRerollButton != null)
		{
			GameEntity gameEntity = GameState.Get().GetGameEntity();
			bool showButton = m_showHeroRerollButton && (gameEntity?.HasTag(GAME_TAG.BACON_MULLIGAN_HERO_REROLL_ACTIVE) ?? true);
			m_heroRerollButton.gameObject.SetActive(showButton);
			UpdateRerollButtonEnabledState(overrideEnabled);
			if (showButton)
			{
				UpdateRerollButtonText();
			}
		}
	}

	public void UpdatePlayerNameText(string text)
	{
		if (m_playerNameText != null)
		{
			m_playerNameText.Text = text;
		}
	}

	public void UpdateAlternateNameText(string text)
	{
		if (m_alternateNameText != null)
		{
			m_alternateNameText.SetText(GameStrings.Get(GameStrings.Format("GAMEPLAY_BACON_ALTERNATE_PLAYER_NAME", text)));
			m_alternateNameText.UpdateNow(updateIfInactive: true);
		}
	}

	protected override void ShowImpl(bool ignoreSpells)
	{
		SetSkipArmorAnimationActive(active: true);
		base.ShowImpl(ignoreSpells);
	}

	public void SetAlternateNameTextActive(bool active)
	{
		if (m_alternateNameText != null)
		{
			m_alternateNameText.gameObject.SetActive(active);
		}
	}

	public void SetFullyHighlighted(bool highlighted)
	{
		m_fullSelectionHighlight.SetActive(highlighted);
	}

	public void SetConfirmHighlighted(bool highlighted)
	{
		m_confirmSelectionHighlight.SetActive(highlighted);
	}

	public void PauseHealthUpdates()
	{
		if (!(m_healthTextMesh == null))
		{
			m_pausedHealthTextMesh = m_healthTextMesh;
			m_healthTextMesh = null;
		}
	}

	public void ResumeHealthUpdates()
	{
		if (!(m_pausedHealthTextMesh == null))
		{
			m_healthTextMesh = m_pausedHealthTextMesh;
			m_pausedHealthTextMesh = null;
			UpdateMinionStatsImmediately();
		}
	}

	public void ToggleLockedHeroView(bool isOn)
	{
		m_lockedHeroBackground.SetActive(isOn);
		m_lockIcon.SetActive(isOn);
		if (isOn)
		{
			SetAlternateNameTextActive(active: false);
			m_playerNameBackground.SetActive(value: false);
			m_nameTextMesh.gameObject.SetActive(value: false);
			GetHealthObject().Hide();
			GetAttackObject().Hide();
		}
		SetFullyHighlighted(highlighted: false);
		SetConfirmHighlighted(highlighted: false);
	}

	public void DisableMulliganOnlyElements()
	{
		m_mulliganBanner.gameObject.SetActive(value: false);
		m_lockedHeroBackground.SetActive(value: false);
		m_lockIcon.SetActive(value: false);
	}

	public void TogglePartiallyRevealedLockedHeroView(bool isOn)
	{
		m_partiallyRevealedLockIcon.SetActive(isOn);
		m_mulliganBanner.SetActive(!isOn);
		SetFullyHighlighted(highlighted: false);
		SetConfirmHighlighted(highlighted: false);
	}

	public bool TryLegendarySlotIn()
	{
		if (base.LegendaryHeroSkinConfig == null)
		{
			return false;
		}
		return base.LegendaryHeroSkinConfig.TryActivateVFX_SocketIn();
	}
}
