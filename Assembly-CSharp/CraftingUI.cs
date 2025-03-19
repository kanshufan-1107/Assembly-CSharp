using System.Collections;
using System.Collections.Generic;
using PegasusShared;
using UnityEngine;

public class CraftingUI : MonoBehaviour
{
	public UberText m_bankAmountText;

	public CreateButton m_buttonCreate;

	public DisenchantButton m_buttonDisenchant;

	public GameObject m_soulboundNotification;

	public UberText m_soulboundTitle;

	public UberText m_soulboundDesc;

	public UberText m_disenchantValue;

	public UberText m_craftValue;

	public GameObject m_wildTheming;

	[SerializeField]
	private GameObject m_createCostBar;

	[SerializeField]
	private GameObject m_disenchantCostBar;

	public float m_disenchantDelayBeforeCardExplodes;

	public float m_disenchantDelayBeforeCardFlips;

	public float m_disenchantDelayBeforeBallsComeOut;

	public float m_craftDelayBeforeConstructSpell;

	public float m_craftDelayBeforeGhostDeath;

	public GameObject m_glowballs;

	public SoundDef m_craftingSound;

	public SoundDef m_disenchantSound;

	public Collider m_mouseOverCollider;

	private Actor m_explodingActor;

	private Actor m_constructingActor;

	private bool m_isAnimating;

	private List<GameObject> m_thingsToDestroy = new List<GameObject>();

	private GameObject m_activeObject;

	private bool m_enabled;

	private bool m_mousedOver;

	private Notification m_craftNotification;

	private bool m_initializedPositions;

	private void Update()
	{
		if (!m_enabled)
		{
			return;
		}
		RaycastHit hitInfo;
		if (m_isAnimating)
		{
			m_mousedOver = false;
		}
		else if (Physics.Raycast(Camera.main.ScreenPointToRay(InputCollection.GetMousePosition()), layerMask: (LayerMask)512, hitInfo: out hitInfo, maxDistance: Camera.main.farClipPlane))
		{
			if (hitInfo.collider == m_mouseOverCollider)
			{
				NotifyOfMouseOver();
			}
			else
			{
				NotifyOfMouseOut();
			}
		}
	}

	private void OnDisable()
	{
		StopCurrentAnim(forceCleanup: true);
	}

	public void UpdateWildTheming()
	{
		if (!(m_wildTheming == null))
		{
			FormatType formatType = CollectionManager.Get().GetThemeShowing();
			m_wildTheming.SetActive(formatType == FormatType.FT_WILD);
		}
	}

	public void UpdateCraftingButtonsAndSoulboundText()
	{
		UpdateBankText();
		CraftingManager craftingManager = CraftingManager.Get();
		if (!craftingManager.GetShownCardInfo(out var entityDef, out var premium))
		{
			m_buttonDisenchant.DisableButton();
			SetDisenchantCostBarActive(active: false);
			m_buttonCreate.DisableButton();
			SetCreateCostBarActive(active: false);
			return;
		}
		NetCache.CardDefinition cardDef = new NetCache.CardDefinition
		{
			Name = entityDef.GetCardId(),
			Premium = premium
		};
		int numOwnedCopies = craftingManager.GetNumOwnedIncludePending();
		TAG_CARD_SET cardSet = entityDef.GetCardSet();
		string cardSetName = GameStrings.GetCardSetName(cardSet);
		NetCache.CardValue cardValue = CraftingManager.GetCardValue(cardDef.Name, cardDef.Premium);
		if (GameUtils.IsSavedZilliaxVersion(entityDef))
		{
			numOwnedCopies = 0;
			cardValue = null;
		}
		string soulboundTitle = GameStrings.Get("GLUE_CRAFTING_SOULBOUND");
		string soulboundDescription = string.Empty;
		if (numOwnedCopies <= 0 || GameUtils.IsSavedZilliaxVersion(entityDef))
		{
			if (GameUtils.IsZilliaxModule(entityDef))
			{
				soulboundTitle = GameStrings.Get("GLUE_CRAFTING_ZILLIAX_MODULE");
				soulboundDescription = GameStrings.Get("GLUE_CRAFTING_ZILLIAX_MODULE_DESC");
			}
			else if (GameUtils.IsSavedZilliaxVersion(entityDef))
			{
				soulboundTitle = GameStrings.Get("GLUE_CRAFTING_ZILLIAX_VERSION");
				soulboundDescription = GameStrings.Get("GLUE_CRAFTING_ZILLIAX_VERSION_DESC");
			}
			else
			{
				soulboundTitle = cardSetName;
				soulboundDescription = ((!Network.IsLoggedIn()) ? GameStrings.Get("GLUE_CRAFTING_SOULBOUND_OFFLINE_DESC") : ((cardSet != TAG_CARD_SET.CORE_HIDDEN) ? entityDef.GetHowToEarnText(cardDef.Premium) : GameStrings.Get("GLUE_CRAFTING_SOULBOUND_CORE_ROTATED_DESC")));
			}
		}
		else
		{
			CardSetDbfRecord cardSetRecord = GameDbf.GetIndex().GetCardSet(cardSet);
			soulboundDescription = (cardSetRecord.IsFeaturedCardSet ? GameStrings.Get("GLUE_CRAFTING_SOULBOUND_FEATURED_DESC") : (cardSetRecord.IsCoreCardSet ? GameStrings.Get("GLUE_CRAFTING_SOULBOUND_CORE_DESC") : (Network.IsLoggedIn() ? GameStrings.Get("GLUE_CRAFTING_SOULBOUND_DESC") : GameStrings.Get("GLUE_CRAFTING_SOULBOUND_OFFLINE_DESC"))));
		}
		bool hasPendingDisenchant = craftingManager.GetNumClientTransactions() < 0;
		CraftingManager.CanCraftCardResult canCraftResult = CraftingManager.Get().CanCraftCardRightNow(entityDef, premium);
		CraftingManager.CanCraftCardResult canUpgradeResult = CraftingManager.Get().CanUpgradeCardToGolden(cardDef.Name, cardDef.Premium, entityDef);
		bool canCraft = canCraftResult == CraftingManager.CanCraftCardResult.CanCraft;
		bool canUpgrade = canUpgradeResult == CraftingManager.CanCraftCardResult.CanUpgrade;
		bool shouldBeInCreateUpgradeState = canCraft && canUpgrade;
		bool shouldBeInUpgradeState = m_buttonCreate.GetCraftingState() == CraftingButton.CraftingState.Upgrade || (!canCraft && canUpgrade);
		bool disabledAndNotEnoughDust = m_buttonCreate.GetCraftingState() == CraftingButton.CraftingState.Disabled && canUpgradeResult == CraftingManager.CanCraftCardResult.NotEnoughDust;
		bool shouldUseUpgradeValue = craftingManager.HasEnoughCopiesToUpgrade(cardDef.Name, cardDef.Premium) && (shouldBeInUpgradeState || craftingManager.GetPendingClientTransaction().GetLastTransactionWasUpgrade() || disabledAndNotEnoughDust);
		if (canCraft || canUpgrade || hasPendingDisenchant)
		{
			m_buttonCreate.EnableButton();
			if (hasPendingDisenchant)
			{
				SetCreateCostBarActive(active: true);
			}
			else
			{
				SetCreateCostBarActive(!shouldBeInCreateUpgradeState);
			}
		}
		else if (canCraftResult == CraftingManager.CanCraftCardResult.NotCraftable)
		{
			m_buttonCreate.DisableButton();
			SetCreateCostBarActive(active: false);
		}
		else if (canCraftResult == CraftingManager.CanCraftCardResult.TooManyCopies && canUpgradeResult == CraftingManager.CanCraftCardResult.TooManyCopies)
		{
			m_buttonCreate.DisableButton();
			SetCreateCostBarActive(active: false);
		}
		else
		{
			SetCreateCostBarActive(active: true);
			if (shouldUseUpgradeValue)
			{
				m_buttonCreate.DisableButton(GameStrings.Get("GLUE_CRAFTING_UPGRADE"));
				m_buttonCreate.SetTextEnlargedForNoDustJarOnPhone(enlarge: false);
			}
			else
			{
				m_buttonCreate.DisableButton(GameStrings.Get("GLUE_CRAFTING_CREATE"));
				m_buttonCreate.SetTextEnlargedForNoDustJarOnPhone(enlarge: false);
			}
		}
		bool canCraftAndDisenchantNow = false;
		bool willBecomeActiveInFuture;
		if (cardValue == null)
		{
			canCraftAndDisenchantNow = false;
		}
		else if ((premium == TAG_PREMIUM.SIGNATURE || premium == TAG_PREMIUM.DIAMOND) && !hasPendingDisenchant && numOwnedCopies <= 0)
		{
			canCraftAndDisenchantNow = false;
		}
		else if (IsCraftingEventForCardActive(cardDef.Name, premium, out willBecomeActiveInFuture) && Network.IsLoggedIn())
		{
			int numClientTransactions = craftingManager.GetNumClientTransactions();
			int costToPurchase = (shouldUseUpgradeValue ? GetUpgradeValue(cardDef) : cardValue.GetBuyValue());
			if (numClientTransactions < 0)
			{
				costToPurchase = cardValue.GetSellValue();
			}
			int costToDisenchant = cardValue.GetSellValue();
			if (numClientTransactions > 0)
			{
				costToDisenchant = (craftingManager.GetPendingClientTransaction().GetLastTransactionWasUpgrade() ? GetUpgradeValue(cardDef) : cardValue.GetBuyValue());
			}
			m_disenchantValue.Text = "+" + costToDisenchant;
			m_craftValue.Text = "-" + costToPurchase;
			canCraftAndDisenchantNow = true;
		}
		else
		{
			canCraftAndDisenchantNow = false;
			if (willBecomeActiveInFuture)
			{
				soulboundTitle = GameStrings.Get("GLUE_CRAFTING_EVENT_NOT_ACTIVE_TITLE");
				soulboundDescription = GameStrings.Format("GLUE_CRAFTING_EVENT_NOT_ACTIVE_DESCRIPTION", cardSetName);
			}
		}
		m_soulboundTitle.Text = soulboundTitle;
		m_soulboundDesc.Text = soulboundDescription;
		if (!canCraftAndDisenchantNow)
		{
			m_buttonDisenchant.DisableButton();
			SetDisenchantCostBarActive(active: false);
			m_buttonCreate.DisableButton();
			SetCreateCostBarActive(active: false);
			m_soulboundNotification.SetActive(value: true);
			m_activeObject = m_soulboundNotification;
			return;
		}
		if (!FixedRewardsMgr.Get().CanCraftCard(cardDef.Name, cardDef.Premium))
		{
			m_buttonDisenchant.DisableButton();
			SetDisenchantCostBarActive(active: false);
			m_buttonCreate.DisableButton();
			SetCreateCostBarActive(active: false);
			m_soulboundNotification.SetActive(value: true);
			m_activeObject = m_soulboundNotification;
			return;
		}
		m_soulboundNotification.SetActive(value: false);
		m_activeObject = base.gameObject;
		if (numOwnedCopies <= 0)
		{
			m_buttonDisenchant.DisableButton();
			SetDisenchantCostBarActive(active: false);
		}
		else
		{
			m_buttonDisenchant.EnableButton();
			SetDisenchantCostBarActive(active: true);
		}
	}

	public void DoDisenchant()
	{
		CraftingManager craftingManager = CraftingManager.Get();
		if (craftingManager.GetNumOwnedIncludePending() > 0)
		{
			UpdateTips();
			bool num = m_buttonDisenchant.GetCraftingState() == CraftingButton.CraftingState.Undo;
			bool wasUpgrade = craftingManager.GetPendingClientTransaction().GetLastTransactionWasUpgrade();
			bool wasUpgradeFromNormal = craftingManager.GetPendingClientTransaction().GetLastOperation() == CraftingPendingTransaction.Operation.UpgradeToGoldenFromNormal;
			int uncommittedDustChanges;
			if (num && wasUpgrade)
			{
				craftingManager.TryGetCardUpgradeValue(craftingManager.GetShownActor().GetEntityDef().GetCardId(), out uncommittedDustChanges);
			}
			else
			{
				craftingManager.TryGetCardSellValue(craftingManager.GetShownActor().GetEntityDef().GetCardId(), craftingManager.GetShownActor().GetPremium(), out uncommittedDustChanges);
			}
			craftingManager.AdjustUnCommitedArcaneDustChanges(uncommittedDustChanges);
			Options.Get().SetBool(Option.HAS_DISENCHANTED, val: true);
			craftingManager.NotifyOfTransaction(-1);
			UpdateCraftingButtonsAndSoulboundText();
			if (m_isAnimating)
			{
				craftingManager.FinishFlipCurrentActorEarly();
			}
			StopCurrentAnim();
			if (num && wasUpgrade)
			{
				StartCoroutine(DoUndoUpgradeAnims(wasUpgradeFromNormal));
			}
			else
			{
				StartCoroutine(DoDisenchantAnims());
			}
			craftingManager.StartCoroutine(StartCraftCooldown());
		}
	}

	public void CleanUpEffects()
	{
		if (m_explodingActor != null)
		{
			Spell explodeSpell = m_explodingActor.GetSpell(SpellType.DECONSTRUCT);
			if (explodeSpell != null && explodeSpell.GetActiveState() != 0)
			{
				m_explodingActor.GetSpell(SpellType.DECONSTRUCT).GetComponent<PlayMakerFSM>().SendEvent("Cancel");
				m_explodingActor.Hide();
			}
		}
		if (m_constructingActor != null)
		{
			Spell constructSpell = m_constructingActor.GetSpell(SpellType.CONSTRUCT);
			if (constructSpell != null && constructSpell.GetActiveState() != 0)
			{
				m_constructingActor.GetSpell(SpellType.CONSTRUCT).GetComponent<PlayMakerFSM>().SendEvent("Cancel");
				m_constructingActor.Hide();
			}
			constructSpell = m_constructingActor.GetSpell(SpellType.DEATH_KNIGHT_CONSTRUCT);
			if (constructSpell != null && constructSpell.GetActiveState() != 0)
			{
				m_constructingActor.GetSpell(SpellType.DEATH_KNIGHT_CONSTRUCT).GetComponent<PlayMakerFSM>().SendEvent("Cancel");
				m_constructingActor.Hide();
			}
		}
		SoundManager soundManager = SoundManager.Get();
		if (soundManager != null)
		{
			soundManager.Stop(m_craftingSound.GetComponent<AudioSource>());
			soundManager.Stop(m_disenchantSound.GetComponent<AudioSource>());
		}
		GetComponent<PlayMakerFSM>().SendEvent("Cancel");
		m_isAnimating = false;
	}

	public void DoCreate(bool isUpgrade)
	{
		CraftingManager craftingManager = CraftingManager.Get();
		if (craftingManager.GetShownCardInfo(out var _, out var _))
		{
			int uncommittedDustChanges;
			if (isUpgrade)
			{
				craftingManager.TryGetCardUpgradeValue(craftingManager.GetShownActor().GetEntityDef().GetCardId(), out uncommittedDustChanges);
			}
			else
			{
				craftingManager.TryGetCardBuyValue(craftingManager.GetShownActor().GetEntityDef().GetCardId(), craftingManager.GetShownActor().GetPremium(), out uncommittedDustChanges);
			}
			craftingManager.AdjustUnCommitedArcaneDustChanges(-uncommittedDustChanges);
			if (!Options.Get().GetBool(Option.HAS_CRAFTED))
			{
				Options.Get().SetBool(Option.HAS_CRAFTED, val: true);
			}
			UpdateTips();
			craftingManager.NotifyOfTransaction(1);
			if (craftingManager.GetNumOwnedIncludePending() > 1)
			{
				craftingManager.ForceNonGhostFlagOn();
			}
			UpdateCraftingButtonsAndSoulboundText();
			StopCurrentAnim();
			StartCoroutine(DoCreateAnims());
			craftingManager.StartCoroutine(StartDisenchantCooldown());
		}
	}

	public void UpdateBankText()
	{
		long arcaneDustBalance = NetCache.Get().GetArcaneDustBalance();
		m_bankAmountText.Text = arcaneDustBalance.ToString();
		BnetBar.Get().RefreshCurrency();
		if ((bool)UniversalInputManager.UsePhoneUI && CraftingTray.Get() != null)
		{
			ArcaneDustAmount.Get().UpdateCurrentDustAmount();
		}
	}

	public void Disable(Vector3 hidePosition)
	{
		m_enabled = false;
		iTween.MoveTo(m_activeObject, iTween.Hash("time", 0.4f, "position", hidePosition, "oncomplete", "FinishActorMoveAway"));
		HideTips();
		StopCurrentAnim(forceCleanup: true);
	}

	public void FinishDisable()
	{
		m_activeObject.SetActive(m_enabled);
	}

	public bool IsEnabled()
	{
		return m_enabled;
	}

	public void Enable(Vector3 showPosition, Vector3 hidePosition)
	{
		if (!m_initializedPositions)
		{
			base.transform.position = hidePosition;
			m_soulboundNotification.transform.position = base.transform.position;
			m_soulboundTitle.Text = GameStrings.Get("GLUE_CRAFTING_SOULBOUND");
			m_soulboundDesc.Text = GameStrings.Get("GLUE_CRAFTING_SOULBOUND_DESC");
			m_activeObject = base.gameObject;
			m_initializedPositions = true;
		}
		m_enabled = true;
		UpdateCraftingButtonsAndSoulboundText();
		UpdateWildTheming();
		m_activeObject.SetActive(value: true);
		iTween.MoveTo(m_activeObject, iTween.Hash("time", 0.5f, "position", showPosition));
		ShowFirstTimeTips();
	}

	public void SetStartingActive()
	{
		m_soulboundNotification.SetActive(value: false);
		base.gameObject.SetActive(value: false);
	}

	public void DoUpgradeToGoldenAnimations()
	{
		UpdateCraftingButtonsAndSoulboundText();
		StopCurrentAnim(forceCleanup: true);
		StartCoroutine(DoCreateAnims());
		CraftingManager.Get().StartCoroutine(StartDisenchantCooldown());
	}

	private int GetUpgradeValue(NetCache.CardDefinition cardDef)
	{
		return CraftingManager.GetCardValue(cardDef.Name, TAG_PREMIUM.NORMAL).BaseUpgradeValue;
	}

	private void ShowFirstTimeTips()
	{
		if (!(m_activeObject == m_soulboundNotification) && !Options.Get().GetBool(Option.HAS_CRAFTED) && UserAttentionManager.CanShowAttentionGrabber("CraftingUI.ShowFirstTimeTips"))
		{
			CreateCraftNotification();
		}
	}

	private void CreateCraftNotification()
	{
		if (m_buttonCreate.IsButtonEnabled() && m_buttonCreate.GetCraftingState() == CraftingButton.CraftingState.Create)
		{
			Vector3 popupPosition;
			Notification.PopUpArrowDirection direction;
			if ((bool)UniversalInputManager.UsePhoneUI)
			{
				popupPosition = new Vector3(73.3f, 1f, 55.4f);
				direction = Notification.PopUpArrowDirection.Down;
			}
			else
			{
				popupPosition = new Vector3(55f, 1f, -56f);
				direction = Notification.PopUpArrowDirection.Left;
			}
			if (m_craftNotification == null)
			{
				m_craftNotification = NotificationManager.Get().CreatePopupText(UserAttentionBlocker.NONE, popupPosition, 16f * Vector3.one, GameStrings.Get("GLUE_COLLECTION_TUTORIAL06"), convertLegacyPosition: false);
			}
			if (m_craftNotification != null)
			{
				m_craftNotification.ShowPopUpArrow(direction);
			}
		}
	}

	private void UpdateTips()
	{
		if (Options.Get().GetBool(Option.HAS_CRAFTED) || !UserAttentionManager.CanShowAttentionGrabber("CraftingUI.UpdateTips") || m_buttonCreate.GetCraftingState() == CraftingButton.CraftingState.Upgrade || m_buttonCreate.GetCraftingState() == CraftingButton.CraftingState.CreateUpgrade)
		{
			HideTips();
		}
		else if (m_craftNotification == null)
		{
			CreateCraftNotification();
		}
		else if (!m_buttonCreate.IsButtonEnabled())
		{
			NotificationManager.Get().DestroyNotification(m_craftNotification, 0f);
		}
	}

	private void HideTips()
	{
		if (m_craftNotification != null)
		{
			NotificationManager.Get().DestroyNotificationNowWithNoAnim(m_craftNotification);
		}
	}

	private void NotifyOfMouseOver()
	{
		if (!m_mousedOver)
		{
			m_mousedOver = true;
			GetComponent<PlayMakerFSM>().SendEvent("Idle");
		}
	}

	private void NotifyOfMouseOut()
	{
		if (m_mousedOver)
		{
			m_mousedOver = false;
			GetComponent<PlayMakerFSM>().SendEvent("IdleCancel");
		}
	}

	public void SetCreateCostBarActive(bool active)
	{
		if (!(m_createCostBar == null))
		{
			m_createCostBar.SetActive(active);
		}
	}

	public void SetDisenchantCostBarActive(bool active)
	{
		if (!(m_disenchantCostBar == null))
		{
			m_disenchantCostBar.SetActive(active);
		}
	}

	public static bool IsCraftingEventForCardActive(string cardID, TAG_PREMIUM premium, out bool willBecomeActiveInFuture)
	{
		willBecomeActiveInFuture = false;
		if (GameUtils.IsClassicCard(cardID))
		{
			return IsCraftingEventForCardActive(GameUtils.GetLegacyCounterpartCardId(cardID), premium, out willBecomeActiveInFuture);
		}
		CardDbfRecord cardRecord = GameUtils.GetCardRecord(cardID);
		if (cardRecord == null)
		{
			Debug.LogWarning($"CraftingUI.IsCraftingEventForCardActive could not find DBF record for card {cardID}, assuming it cannot be crafted or disenchanted");
			return false;
		}
		EventTimingType craftingEventName = cardRecord.CraftingEvent;
		switch (premium)
		{
		case TAG_PREMIUM.GOLDEN:
		{
			if (cardRecord.GoldenCraftingEvent != EventTimingType.UNKNOWN)
			{
				craftingEventName = cardRecord.GoldenCraftingEvent;
				break;
			}
			CardSetDbfRecord cardSetRecord2 = GameUtils.GetCardSetRecord(cardID);
			if (cardSetRecord2 != null)
			{
				craftingEventName = cardSetRecord2.ContentLaunchEvent;
			}
			break;
		}
		case TAG_PREMIUM.SIGNATURE:
		{
			if (cardRecord.SignatureCraftingEvent != EventTimingType.UNKNOWN)
			{
				craftingEventName = cardRecord.SignatureCraftingEvent;
				break;
			}
			CardSetDbfRecord cardSetRecord3 = GameUtils.GetCardSetRecord(cardID);
			if (cardSetRecord3 != null)
			{
				craftingEventName = cardSetRecord3.ContentLaunchEvent;
			}
			break;
		}
		case TAG_PREMIUM.DIAMOND:
		{
			if (cardRecord.DiamondCraftingEvent != EventTimingType.UNKNOWN)
			{
				craftingEventName = cardRecord.DiamondCraftingEvent;
				break;
			}
			CardSetDbfRecord cardSetRecord4 = GameUtils.GetCardSetRecord(cardID);
			if (cardSetRecord4 != null)
			{
				craftingEventName = cardSetRecord4.ContentLaunchEvent;
			}
			break;
		}
		default:
			if (craftingEventName == EventTimingType.UNKNOWN)
			{
				CardSetDbfRecord cardSetRecord = GameUtils.GetCardSetRecord(cardID);
				if (cardSetRecord != null)
				{
					craftingEventName = cardSetRecord.ContentLaunchEvent;
				}
			}
			break;
		}
		bool num = EventTimingManager.Get().IsEventActive(craftingEventName);
		if (!num)
		{
			willBecomeActiveInFuture = EventTimingManager.Get().IsStartTimeInTheFuture(craftingEventName);
		}
		return num;
	}

	private void StopCurrentAnim(bool forceCleanup = false)
	{
		if (!m_isAnimating && !forceCleanup)
		{
			return;
		}
		StopAllCoroutines();
		CleanUpEffects();
		foreach (GameObject go in m_thingsToDestroy)
		{
			if (!(go == null))
			{
				Log.Crafting.Print("StopCurrentAnim: Destroying GameObject {0}", go);
				Object.Destroy(go);
			}
		}
	}

	private IEnumerator StartDisenchantCooldown()
	{
		Collider buttonDisenchatCollider = m_buttonDisenchant.GetComponent<Collider>();
		if (buttonDisenchatCollider.enabled)
		{
			buttonDisenchatCollider.enabled = false;
			yield return new WaitForSeconds(1f);
			buttonDisenchatCollider.enabled = true;
		}
	}

	private IEnumerator StartCraftCooldown()
	{
		Collider buttonDisenchatCollider = m_buttonDisenchant.GetComponent<Collider>();
		if (buttonDisenchatCollider.enabled)
		{
			buttonDisenchatCollider.enabled = false;
			yield return new WaitForSeconds(1f);
			buttonDisenchatCollider.enabled = true;
		}
	}

	private IEnumerator DoDisenchantAnims()
	{
		SoundManager.Get().Play(m_disenchantSound.GetComponent<AudioSource>());
		SoundManager.Get().Stop(m_craftingSound.GetComponent<AudioSource>());
		m_isAnimating = true;
		CraftingManager.Get().m_cardCountTab.gameObject.SetActive(value: false);
		PlayMakerFSM playmaker = GetComponent<PlayMakerFSM>();
		playmaker.SendEvent("Birth");
		yield return new WaitForSeconds(m_disenchantDelayBeforeCardExplodes);
		while (CraftingManager.Get().GetShownActor() == null)
		{
			yield return null;
		}
		m_explodingActor = CraftingManager.Get().GetShownActor();
		Actor oldActor = m_explodingActor;
		m_thingsToDestroy.Add(m_explodingActor.gameObject);
		Log.Crafting.Print("Adding {0} to thingsToDestroy", m_explodingActor.gameObject);
		UpdateBankText();
		if (CraftingManager.Get().IsCancelling())
		{
			yield break;
		}
		CraftingManager.Get().LoadGhostActorIfNecessary();
		m_explodingActor.ActivateSpellBirthState(SpellType.DECONSTRUCT);
		yield return new WaitForSeconds(m_disenchantDelayBeforeCardFlips);
		if (CraftingManager.Get().IsCancelling())
		{
			yield break;
		}
		CraftingManager.Get().FlipUpsideDownCard(m_explodingActor);
		yield return new WaitForSeconds(m_disenchantDelayBeforeBallsComeOut);
		if (!CraftingManager.Get().IsCancelling())
		{
			playmaker.SendEvent("Action");
			yield return new WaitForSeconds(1f);
			CraftingManager.Get().m_cardCountTab.gameObject.SetActive(value: true);
			m_isAnimating = false;
			yield return new WaitForSeconds(10f);
			if (oldActor != null)
			{
				Object.Destroy(oldActor.gameObject);
			}
		}
	}

	private IEnumerator DoUndoUpgradeAnims(bool wasUpgradeFromNormal)
	{
		SoundManager.Get().Play(m_disenchantSound.GetComponent<AudioSource>());
		SoundManager.Get().Stop(m_craftingSound.GetComponent<AudioSource>());
		m_isAnimating = true;
		CraftingManager.Get().m_cardCountTab.gameObject.SetActive(value: false);
		PlayMakerFSM playmaker = GetComponent<PlayMakerFSM>();
		playmaker.SendEvent("Birth");
		yield return new WaitForSeconds(m_disenchantDelayBeforeCardExplodes);
		while (CraftingManager.Get().GetShownActor() == null)
		{
			yield return null;
		}
		m_explodingActor = CraftingManager.Get().GetShownActor();
		Actor oldActor = m_explodingActor;
		m_thingsToDestroy.Add(m_explodingActor.gameObject);
		Log.Crafting.Print("Adding {0} to thingsToDestroy", m_explodingActor.gameObject);
		UpdateBankText();
		if (CraftingManager.Get().IsCancelling())
		{
			yield break;
		}
		CraftingManager.Get().LoadGhostActorIfNecessary();
		m_explodingActor.ActivateSpellBirthState(SpellType.DECONSTRUCT);
		yield return new WaitForSeconds(m_disenchantDelayBeforeCardFlips);
		if (CraftingManager.Get().IsCancelling())
		{
			yield break;
		}
		if (wasUpgradeFromNormal)
		{
			CraftingManager.Get().SwitchPremiumView(TAG_PREMIUM.NORMAL);
		}
		else
		{
			CraftingManager.Get().FlipUpsideDownCard(m_explodingActor);
		}
		yield return new WaitForSeconds(m_disenchantDelayBeforeBallsComeOut);
		if (!CraftingManager.Get().IsCancelling())
		{
			playmaker.SendEvent("Action");
			yield return new WaitForSeconds(1f);
			CraftingManager.Get().m_cardCountTab.gameObject.SetActive(value: true);
			m_isAnimating = false;
			if (oldActor != null)
			{
				Object.Destroy(oldActor.gameObject);
			}
		}
	}

	private IEnumerator DoCreateAnims()
	{
		Actor shownActor = CraftingManager.Get().GetShownActor();
		SoundManager.Get().Play(m_craftingSound.GetComponent<AudioSource>());
		SoundManager.Get().Stop(m_disenchantSound.GetComponent<AudioSource>());
		m_isAnimating = true;
		CraftingManager.Get().HideAndDestroyRelatedInfo();
		CraftingManager.Get().m_cardCountTab.gameObject.SetActive(value: false);
		CraftingManager.Get().FlipCurrentActor();
		GetComponent<PlayMakerFSM>().SendEvent("Birth");
		yield return new WaitForSeconds(m_craftDelayBeforeConstructSpell);
		if (CraftingManager.Get().IsCancelling())
		{
			yield break;
		}
		m_constructingActor = CraftingManager.Get().LoadNewActorAndConstructIt();
		UpdateBankText();
		yield return new WaitForSeconds(m_craftDelayBeforeGhostDeath);
		if (!CraftingManager.Get().IsCancelling())
		{
			if (shownActor.HasCardDef && shownActor.PlayEffectDef != null)
			{
				GameUtils.PlayCardEffectDefSounds(shownActor.PlayEffectDef);
			}
			CraftingManager.Get().m_cardCountTab.gameObject.SetActive(value: true);
			CraftingManager.Get().FinishCreateAnims();
			yield return new WaitForSeconds(1f);
			m_isAnimating = false;
		}
	}
}
