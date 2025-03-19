using System;
using System.Collections.Generic;
using System.Linq;
using Blizzard.T5.Core.Utils;
using Hearthstone.DataModels;
using Hearthstone.UI;
using PegasusLettuce;
using UnityEngine;

public class PackOpeningCard : MonoBehaviour
{
	public delegate void RevealedCallback(object userData);

	private class RevealedListener : EventListener<RevealedCallback>
	{
		public void Fire()
		{
			m_callback(m_userData);
		}
	}

	public GameObject m_CardParent;

	public GameObject m_SharedHiddenCardObject;

	public GameObject m_IsNewObject;

	public Spell m_ClassNameSpell;

	public AsyncReference m_packOpeningMercenaryReference;

	public AsyncReference m_packOpeningMercenaryPortraitReference;

	public AsyncReference m_packOpeningCoinReference;

	private const TAG_RARITY FALLBACK_RARITY = TAG_RARITY.COMMON;

	private NetCache.BoosterCard m_boosterCard;

	private LettucePackComponent m_mercenaryPackComponent;

	private TAG_PREMIUM m_premium;

	private EntityDef m_entityDef;

	private Actor m_actor;

	private PackOpeningCardRarityInfo m_rarityInfo;

	private Spell m_spell;

	private PegUIElement m_revealButton;

	private PackOpeningCardMercenary m_packOpeningMercenary;

	private PackOpeningPortrait m_packOpeningMercenaryPortrait;

	private PackOpeningCoin m_packOpeningCoin;

	private bool m_ready;

	private bool m_inputEnabled;

	private bool m_revealEnabled;

	private bool m_revealed;

	private bool m_isNew;

	private List<RevealedListener> m_revealedListeners = new List<RevealedListener>();

	private bool m_showClassName;

	private bool m_isMassPackOpening;

	private int m_cardNumber;

	public event EventHandler OnSpellFinishedEvent;

	public event EventHandler<Spell> OnSpellStateFinishedEvent;

	private void Awake()
	{
		m_packOpeningMercenaryReference.RegisterReadyListener<VisualController>(OnPackOpeningMercenaryReady);
		m_packOpeningMercenaryPortraitReference.RegisterReadyListener<VisualController>(OnPackOpeningMercenaryPortraitReady);
		m_packOpeningCoinReference.RegisterReadyListener<VisualController>(OnPackOpeningCoinReady);
	}

	public string GetCardId()
	{
		if (m_boosterCard != null)
		{
			return m_boosterCard.Def.Name;
		}
		if (m_mercenaryPackComponent != null)
		{
			return GameUtils.GetCardIdFromMercenaryId(m_mercenaryPackComponent.MercenaryId);
		}
		return null;
	}

	public EntityDef GetEntityDef()
	{
		return m_entityDef;
	}

	public Actor GetActor()
	{
		return m_actor;
	}

	public TAG_PREMIUM GetPremium()
	{
		return m_premium;
	}

	private void ResetForNewCard()
	{
		m_isMassPackOpening = false;
		m_boosterCard = null;
		m_mercenaryPackComponent = null;
	}

	public void SetCardNumber(int number)
	{
		m_cardNumber = number;
	}

	public void AttachBoosterCard(NetCache.BoosterCard boosterCard, bool isMassPackOpening = false)
	{
		if (boosterCard != null)
		{
			ResetForNewCard();
			m_isMassPackOpening = isMassPackOpening;
			m_boosterCard = boosterCard;
			m_premium = m_boosterCard.Def.Premium;
			m_showClassName = true;
			Destroy();
			LoadEntityDef(m_boosterCard.Def.Name);
		}
	}

	public void AttachBoosterMercenary(LettucePackComponent packComponent)
	{
		if (packComponent != null)
		{
			ResetForNewCard();
			m_mercenaryPackComponent = packComponent;
			m_premium = TAG_PREMIUM.NORMAL;
			m_showClassName = false;
			Destroy();
			string cardId = GameUtils.GetCardIdFromMercenaryId(packComponent.MercenaryId);
			LoadEntityDef(cardId);
		}
	}

	public bool IsReady()
	{
		return m_ready;
	}

	public bool IsRevealed()
	{
		return m_revealed;
	}

	public void Destroy()
	{
		m_ready = false;
		if (m_actor != null)
		{
			m_actor.Destroy();
			m_actor = null;
		}
		m_entityDef = null;
		m_rarityInfo = null;
		m_spell = null;
		m_revealButton = null;
		m_revealed = false;
	}

	public bool IsInputEnabled()
	{
		return m_inputEnabled;
	}

	public void EnableInput(bool enable)
	{
		m_inputEnabled = enable;
		UpdateInput();
	}

	public bool IsRevealEnabled()
	{
		return m_revealEnabled;
	}

	public void EnableReveal(bool enable)
	{
		m_revealEnabled = enable;
		UpdateActor();
	}

	public void AddRevealedListener(RevealedCallback callback)
	{
		AddRevealedListener(callback, this);
	}

	public void AddRevealedListener(RevealedCallback callback, object userData)
	{
		RevealedListener listener = new RevealedListener();
		listener.SetCallback(callback);
		listener.SetUserData(userData);
		m_revealedListeners.Add(listener);
	}

	public void RemoveRevealedListener(RevealedCallback callback)
	{
		RemoveRevealedListener(callback, null);
	}

	public void RemoveRevealedListener(RevealedCallback callback, object userData)
	{
		RevealedListener listener = new RevealedListener();
		listener.SetCallback(callback);
		listener.SetUserData(userData);
		m_revealedListeners.Remove(listener);
	}

	public void RemoveOnOverWhileFlippedListeners()
	{
		m_revealButton.RemoveEventListener(UIEventType.ROLLOVER, OnOverWhileFlipped);
		m_revealButton.RemoveEventListener(UIEventType.ROLLOUT, OnOutWhileFlipped);
	}

	public void ForceReveal(bool suppressVO = false)
	{
		if (suppressVO)
		{
			OnPressNoSound(null);
		}
		else
		{
			OnPress(null);
		}
	}

	public void ShowRarityGlow()
	{
		if (!IsRevealed())
		{
			OnOver(null);
		}
	}

	public void HideRarityGlow()
	{
		if (!IsRevealed())
		{
			OnOut(null);
		}
	}

	public void Dissipate()
	{
		CardBackDisplay cbd = GetComponentInChildren<CardBackDisplay>();
		if (cbd != null)
		{
			cbd.gameObject.GetComponent<Renderer>().enabled = false;
		}
		Spell classNameSpell = m_ClassNameSpell;
		classNameSpell.AddFinishedCallback(SpellFinishedCallback);
		classNameSpell.AddStateFinishedCallback(SpellStateFinishedCallback);
		classNameSpell.ActivateState(SpellStateType.DEATH);
		if (m_IsNewObject != null)
		{
			m_IsNewObject.SetActive(value: false);
		}
		Actor actor = GetActor();
		if (actor != null)
		{
			Spell spell2 = actor.GetSpell(SpellType.DEATH);
			spell2.AddFinishedCallback(SpellFinishedCallback);
			spell2.AddStateFinishedCallback(SpellStateFinishedCallback);
			spell2.Activate();
		}
		else
		{
			PackOpeningType packType = DeterminePackOpeningType();
			if (packType == PackOpeningType.COIN && m_packOpeningCoin != null)
			{
				m_packOpeningCoin.ActivateDeathVisuals(SpellFinishedCallback, SpellStateFinishedCallback);
				if (m_spell != null)
				{
					m_spell.ActivateState(SpellStateType.DEATH);
				}
			}
			else if (packType == PackOpeningType.CARD && m_packOpeningMercenary != null)
			{
				m_packOpeningMercenary.ActivateDeathVisuals(SpellFinishedCallback, SpellStateFinishedCallback);
			}
			else if (packType == PackOpeningType.MERC_PORTRAIT && m_packOpeningMercenaryPortrait != null)
			{
				m_packOpeningMercenaryPortrait.ActivateDeathVisuals(SpellFinishedCallback, SpellStateFinishedCallback);
			}
		}
		if (TemporaryAccountManager.IsTemporaryAccount())
		{
			EntityDef entityDef = GetEntityDef();
			if (entityDef != null && (entityDef.GetRarity() == TAG_RARITY.EPIC || entityDef.GetRarity() == TAG_RARITY.LEGENDARY))
			{
				TemporaryAccountManager.Get().ShowEarnCardEventHealUpDialog(TemporaryAccountManager.HealUpReason.OPEN_PACK);
			}
		}
		void SpellFinishedCallback(Spell spell, object sender)
		{
			this.OnSpellFinishedEvent?.Invoke(sender, EventArgs.Empty);
		}
		void SpellStateFinishedCallback(Spell spell, SpellStateType type, object sender)
		{
			this.OnSpellStateFinishedEvent?.Invoke(sender, spell);
		}
	}

	private void LoadEntityDef(string cardId)
	{
		m_entityDef = DefLoader.Get().GetEntityDef(cardId);
		if (m_entityDef == null)
		{
			Debug.LogError("PackOpeningCard.LoadEntityDef() - FAILED to load \"" + cardId + "\"");
			return;
		}
		PackOpeningType packRewardType = DeterminePackOpeningType();
		if (packRewardType == PackOpeningType.NONE)
		{
			Debug.LogError("PackOpeningCard.OnFullDefLoaded() - FAILED to determine pack reward type for " + GetCardId());
		}
		else
		{
			if (!DetermineRarityInfo(packRewardType))
			{
				return;
			}
			if (m_mercenaryPackComponent != null)
			{
				switch (packRewardType)
				{
				case PackOpeningType.CARD:
					SetUpPackOpeningMercenaryDataModel(m_entityDef);
					break;
				case PackOpeningType.MERC_PORTRAIT:
					SetUpPackOpeningMercenaryPortraitDataModel(m_entityDef);
					break;
				case PackOpeningType.COIN:
					SetUpPackOpeningCoinDataModel(m_entityDef);
					break;
				}
			}
			else if (m_boosterCard != null)
			{
				AssetLoader.Get().InstantiatePrefab(ActorNames.GetHandActor(m_entityDef, m_premium), OnActorLoaded, null, AssetLoadingOptions.IgnorePrefabPosition);
				CollectibleCard collectibleCard = CollectionManager.Get().GetCard(m_entityDef.GetCardId(), m_premium);
				m_isNew = collectibleCard.SeenCount < 1 && collectibleCard.OwnedCount < 2;
			}
			else
			{
				Debug.LogError($"PackOpeningCard.OnFullDefLoaded() - No card data provided \"{cardId}\"");
			}
		}
	}

	private void OnActorLoaded(AssetReference assetRef, GameObject go, object callbackData)
	{
		if (go == null)
		{
			Debug.LogError($"PackOpeningCard.OnActorLoaded() - FAILED to load actor \"{assetRef}\"");
			return;
		}
		Actor actor = go.GetComponent<Actor>();
		if (actor == null)
		{
			Debug.LogError($"PackOpeningCard.OnActorLoaded() - ERROR actor \"{base.name}\" has no Actor component");
			return;
		}
		m_actor = actor;
		m_actor.TurnOffCollider();
		SetupActor();
		LayerUtils.SetLayer(actor.gameObject, GameLayer.IgnoreFullScreenEffects);
		m_ready = true;
		UpdateInput();
		UpdateActor();
	}

	private PackOpeningType DeterminePackOpeningType()
	{
		if (m_mercenaryPackComponent != null)
		{
			if (m_mercenaryPackComponent.CurrencyAmount > 0)
			{
				return PackOpeningType.COIN;
			}
			if (IsPortrait())
			{
				return PackOpeningType.MERC_PORTRAIT;
			}
		}
		return PackOpeningType.CARD;
	}

	private bool DetermineRarityInfo(PackOpeningType packRewardType)
	{
		TAG_RARITY rarityTag = m_entityDef?.GetRarity() ?? TAG_RARITY.COMMON;
		PackOpeningRarity rarity = GameUtils.GetPackOpeningRarity(rarityTag);
		if (rarity == PackOpeningRarity.NONE)
		{
			Debug.LogError("PackOpeningCard.DetermineRarityInfo() - FAILED to determine rarity for " + GetCardId());
			return false;
		}
		PackOpening packOpening = GameObjectUtils.FindComponentInParents<PackOpening>(this);
		GameObject cardEffects = packRewardType switch
		{
			PackOpeningType.MERC_PORTRAIT => packOpening.GetPackOpeningPortraitEffects(), 
			PackOpeningType.COIN => packOpening.GetPackOpeningCoinEffects(), 
			_ => packOpening.GetPackOpeningCardEffects(), 
		};
		if (cardEffects == null)
		{
			Debug.LogError("PackOpeningCard.DetermineRarityInfo() - Fail to get card effect from PackOpening");
			return false;
		}
		PackOpeningCardRarityInfo[] rarityInfos = cardEffects.GetComponentsInChildren<PackOpeningCardRarityInfo>();
		if (rarityInfos == null)
		{
			Debug.LogError($"PackOpeningCard.DetermineRarityInfo() - {this} has no rarity info list. cardId={GetCardId()}");
			return false;
		}
		int signatureFrameId = 0;
		if (m_premium == TAG_PREMIUM.SIGNATURE && !string.IsNullOrEmpty(GetCardId()))
		{
			signatureFrameId = ActorNames.GetSignatureFrameId(GetCardId());
		}
		PackOpeningCardRarityInfo rarityInfo = rarityInfos.FirstOrDefault((PackOpeningCardRarityInfo info) => rarity == info.m_RarityType && info.m_SignatureFrameId == signatureFrameId);
		if (rarityInfo == null)
		{
			Debug.LogError($"PackOpeningCard.DetermineRarityInfo() - {this} has no rarity info for {rarity}, {packRewardType}. cardId={GetCardId()}");
			return false;
		}
		m_rarityInfo = rarityInfo;
		SetupRarity();
		return true;
	}

	private void SetupActor()
	{
		m_actor.SetEntityDef(m_entityDef);
		m_actor.SetPremium(m_premium);
		m_actor.UpdateAllComponents();
	}

	private void UpdateActor()
	{
		if (m_actor == null)
		{
			return;
		}
		if (!IsRevealEnabled())
		{
			m_actor.Hide();
			return;
		}
		if (!IsRevealed())
		{
			m_actor.Hide();
		}
		Vector3 localScale = m_actor.transform.localScale;
		m_actor.transform.parent = m_rarityInfo.m_RevealedCardObject.transform;
		m_actor.transform.localPosition = Vector3.zero;
		m_actor.transform.localRotation = Quaternion.identity;
		m_actor.transform.localScale = localScale;
		if (m_isNew)
		{
			m_actor.SetActorState(ActorStateType.CARD_RECENTLY_ACQUIRED);
		}
	}

	private void SetupRarity()
	{
		GameObject rarityObj = UnityEngine.Object.Instantiate(m_rarityInfo.gameObject);
		if (!(rarityObj == null))
		{
			rarityObj.transform.parent = m_CardParent.transform;
			m_rarityInfo = rarityObj.GetComponent<PackOpeningCardRarityInfo>();
			m_rarityInfo.m_RarityObject.SetActive(value: true);
			m_rarityInfo.m_HiddenCardObject.SetActive(value: true);
			Vector3 orgPos = m_rarityInfo.m_HiddenCardObject.transform.localPosition;
			m_rarityInfo.m_HiddenCardObject.transform.parent = m_CardParent.transform;
			m_rarityInfo.m_HiddenCardObject.transform.localPosition = orgPos;
			m_rarityInfo.m_HiddenCardObject.transform.localRotation = Quaternion.identity;
			m_rarityInfo.m_HiddenCardObject.transform.localScale = new Vector3(7.646f, 7.646f, 7.646f);
			TransformUtil.AttachAndPreserveLocalTransform(m_rarityInfo.m_RarityObject.transform, m_CardParent.transform);
			m_spell = m_rarityInfo.m_RarityObject.GetComponent<Spell>();
			m_revealButton = m_rarityInfo.m_RarityObject.GetComponent<PegUIElement>();
			if (UniversalInputManager.Get().IsTouchMode())
			{
				m_revealButton.SetReceiveReleaseWithoutMouseDown(receiveReleaseWithoutMouseDown: true);
			}
			m_SharedHiddenCardObject.transform.parent = m_rarityInfo.m_HiddenCardObject.transform;
			TransformUtil.Identity(m_SharedHiddenCardObject.transform);
		}
	}

	public static int ComparePackOpeningCards(PackOpeningCard x, PackOpeningCard y)
	{
		NetCache.BoosterCard boosterCard = x.m_boosterCard;
		NetCache.BoosterCard cardY = y.m_boosterCard;
		float xImportance = MassPackOpeningHighlights.GetImportance(boosterCard);
		float yImportance = MassPackOpeningHighlights.GetImportance(cardY);
		if (xImportance < yImportance)
		{
			return 1;
		}
		if (xImportance > yImportance)
		{
			return -1;
		}
		return 0;
	}

	private void OnOver(UIEvent e)
	{
		if (!(m_spell == null) && IsReady())
		{
			m_spell.ActivateState(SpellStateType.BIRTH);
		}
	}

	private void OnOut(UIEvent e)
	{
		if (!(m_spell == null) && IsReady())
		{
			m_spell.ActivateState(SpellStateType.CANCEL);
		}
	}

	private void OnOverWhileFlipped(UIEvent e)
	{
		if (!(m_actor == null))
		{
			if (m_isNew)
			{
				m_actor.SetActorState(ActorStateType.CARD_RECENTLY_ACQUIRED_MOUSE_OVER);
			}
			else
			{
				m_actor.SetActorState(ActorStateType.CARD_HISTORY);
			}
			bool useMassPackOpeningPhoneScale = m_isMassPackOpening && (bool)UniversalInputManager.UsePhoneUI;
			bool showRight = useMassPackOpeningPhoneScale && IsCardOnTheLeftOfTheMassPackOpeningHighlightScreen();
			TooltipPanelManager.Get().UpdateKeywordHelpForPackOpening(m_actor.GetEntityDef(), m_actor, showRight ? TooltipPanelManager.TooltipBoneSource.TOP_RIGHT : TooltipPanelManager.TooltipBoneSource.TOP_LEFT, useMassPackOpeningPhoneScale);
		}
	}

	public bool IsCardOnTheLeftOfTheMassPackOpeningHighlightScreen()
	{
		if (m_cardNumber != 4)
		{
			return m_cardNumber == 2;
		}
		return true;
	}

	private void OnOutWhileFlipped(UIEvent e)
	{
		if (!(m_actor == null))
		{
			if (m_isNew)
			{
				m_actor.SetActorState(ActorStateType.CARD_RECENTLY_ACQUIRED);
			}
			else
			{
				m_actor.SetActorState(ActorStateType.CARD_IDLE);
			}
			TooltipPanelManager.Get().HideKeywordHelp();
		}
	}

	private void OnPressImpl(UIEvent e)
	{
		if (m_spell == null || !IsReady())
		{
			return;
		}
		m_revealed = true;
		UpdateInput();
		List<GameObject> packTypeObjects = new List<GameObject>(3);
		if (m_packOpeningMercenary != null)
		{
			packTypeObjects.Add(m_packOpeningMercenary.gameObject);
		}
		if (m_packOpeningMercenaryPortrait != null)
		{
			packTypeObjects.Add(m_packOpeningMercenaryPortrait.gameObject);
		}
		if (m_packOpeningCoin != null)
		{
			packTypeObjects.Add(m_packOpeningCoin.gameObject);
		}
		foreach (GameObject packObject in packTypeObjects)
		{
			VisualController vc = packObject.GetComponent<VisualController>();
			if (vc != null)
			{
				vc.SetState("REVEAL");
			}
			else
			{
				Debug.LogError("PackOpeningCard.OnPress() - Fail to get VisualController from " + packObject.name);
			}
		}
		m_spell.AddFinishedCallback(OnSpellFinished);
		m_spell.ActivateState(SpellStateType.ACTION);
	}

	private void OnPress(UIEvent e)
	{
		OnPressImpl(e);
		PlayCorrectSound();
	}

	private void OnPressNoSound(UIEvent e)
	{
		OnPressImpl(e);
	}

	private void UpdateInput()
	{
		if (!IsReady())
		{
			return;
		}
		bool enabled = !IsRevealed() && IsInputEnabled();
		if (!(m_revealButton != null) || (!m_isMassPackOpening && (bool)UniversalInputManager.UsePhoneUI))
		{
			return;
		}
		if (enabled)
		{
			m_revealButton.AddEventListener(UIEventType.ROLLOVER, OnOver);
			m_revealButton.AddEventListener(UIEventType.ROLLOUT, OnOut);
			m_revealButton.AddEventListener(UIEventType.RELEASE, OnPress);
			if (PegUI.Get().FindHitElement() == m_revealButton)
			{
				OnOver(null);
			}
		}
		else
		{
			m_revealButton.RemoveEventListener(UIEventType.ROLLOVER, OnOver);
			m_revealButton.RemoveEventListener(UIEventType.ROLLOUT, OnOut);
			m_revealButton.RemoveEventListener(UIEventType.RELEASE, OnPress);
		}
	}

	public void OnPackOpeningMercenaryReady(VisualController visualController)
	{
		if (!(visualController == null))
		{
			m_packOpeningMercenary = visualController.GetComponent<PackOpeningCardMercenary>();
		}
	}

	public void OnPackOpeningMercenaryPortraitReady(VisualController visualController)
	{
		if (!(visualController == null))
		{
			m_packOpeningMercenaryPortrait = visualController.GetComponent<PackOpeningPortrait>();
		}
	}

	private LettuceMercenaryDataModel GetPackOpeningMercenaryDataModel()
	{
		Widget owner = m_packOpeningMercenary.GetComponent<VisualController>().Owner;
		if (!owner.GetDataModel(216, out var dataModel))
		{
			dataModel = MercenaryFactory.CreateEmptyMercenaryDataModel();
			owner.BindDataModel(dataModel);
		}
		return dataModel as LettuceMercenaryDataModel;
	}

	private LettuceMercenaryCoinDataModel GetPackOpeningMercenaryCoinDataModel()
	{
		Widget owner = m_packOpeningMercenary.GetComponent<VisualController>().Owner;
		if (!owner.GetDataModel(238, out var dataModel))
		{
			dataModel = new LettuceMercenaryCoinDataModel();
			owner.BindDataModel(dataModel);
		}
		return dataModel as LettuceMercenaryCoinDataModel;
	}

	private void SetUpPackOpeningMercenaryDataModel(EntityDef entityDef)
	{
		LettuceMercenaryDataModel packOpeningMercenaryDataModel = GetPackOpeningMercenaryDataModel();
		LettuceMercenaryCoinDataModel mercenaryCoinDataModel = GetPackOpeningMercenaryCoinDataModel();
		LettuceMercenary merc = CollectionManager.Get().GetMercenary(m_mercenaryPackComponent.MercenaryId);
		CollectionUtils.PopulateMercenaryCardDataModel(packOpeningMercenaryDataModel, CreateArtVariation());
		CollectionUtils.SetMercenaryStatsByLevel(packOpeningMercenaryDataModel, merc.ID, merc.m_level, merc.m_isFullyUpgraded);
		bool isPortrait = IsPortrait();
		packOpeningMercenaryDataModel.HideXp = true;
		packOpeningMercenaryDataModel.HideWatermark = false;
		packOpeningMercenaryDataModel.HideStats = isPortrait;
		packOpeningMercenaryDataModel.Label = (isPortrait ? GameStrings.Get("GLUE_MERCENARY_LABEL_PORTRAIT") : string.Empty);
		string shortName = entityDef.GetShortName();
		string mercName = (string.IsNullOrEmpty(shortName) ? entityDef.GetName() : shortName);
		mercenaryCoinDataModel.MercenaryId = m_mercenaryPackComponent.MercenaryId;
		mercenaryCoinDataModel.MercenaryName = mercName;
		mercenaryCoinDataModel.Quantity = (int)m_mercenaryPackComponent.CurrencyAmount;
		mercenaryCoinDataModel.GlowActive = false;
		mercenaryCoinDataModel.NameActive = true;
		GameObject obj = m_packOpeningMercenary.gameObject;
		LayerUtils.SetLayer(obj, GameLayer.IgnoreFullScreenEffects);
		m_SharedHiddenCardObject.gameObject.SetActive(value: false);
		UpdateInput();
		Vector3 localScale = obj.transform.localScale;
		obj.transform.parent = m_rarityInfo.m_RevealedCardObject.transform;
		obj.transform.localPosition = Vector3.zero;
		obj.transform.localRotation = Quaternion.identity;
		obj.transform.localScale = localScale;
		m_ready = true;
	}

	private LettuceMercenaryDataModel GetPackOpeningMercenaryPortraitDataModel()
	{
		Widget owner = m_packOpeningMercenaryPortrait.GetComponent<VisualController>().Owner;
		if (!owner.GetDataModel(216, out var dataModel))
		{
			dataModel = new LettuceMercenaryDataModel();
			owner.BindDataModel(dataModel);
		}
		return dataModel as LettuceMercenaryDataModel;
	}

	private void SetUpPackOpeningMercenaryPortraitDataModel(EntityDef entityDef)
	{
		LettuceMercenaryDataModel packOpeningMercenaryPortraitDataModel = GetPackOpeningMercenaryPortraitDataModel();
		LettuceMercenary merc = CollectionManager.Get().GetMercenary(m_mercenaryPackComponent.MercenaryId);
		CollectionUtils.PopulateMercenaryCardDataModel(packOpeningMercenaryPortraitDataModel, CreateArtVariation());
		CollectionUtils.SetMercenaryStatsByLevel(packOpeningMercenaryPortraitDataModel, merc.ID, merc.m_level, merc.m_isFullyUpgraded);
		string shortName = entityDef.GetShortName();
		string mercName = (string.IsNullOrEmpty(shortName) ? entityDef.GetName() : shortName);
		packOpeningMercenaryPortraitDataModel.HideXp = true;
		packOpeningMercenaryPortraitDataModel.HideWatermark = false;
		packOpeningMercenaryPortraitDataModel.MercenaryName = mercName;
		packOpeningMercenaryPortraitDataModel.MercenaryShortName = mercName;
		packOpeningMercenaryPortraitDataModel.MercenaryRarity = merc.m_rarity;
		packOpeningMercenaryPortraitDataModel.MercenaryRole = merc.m_role;
		packOpeningMercenaryPortraitDataModel.HideStats = true;
		GameObject obj = m_packOpeningMercenaryPortrait.gameObject;
		LayerUtils.SetLayer(obj, GameLayer.IgnoreFullScreenEffects);
		m_SharedHiddenCardObject.gameObject.SetActive(value: false);
		VisualController vc = obj.GetComponent<VisualController>();
		if (vc != null)
		{
			vc.SetState("HIDDEN");
		}
		else
		{
			Debug.LogError("PackOpeningCard.SetUpPackOpeningMercenaryPortraitDataModel() - Fail to get VisualController from m_packOpeningMercenaryPortrait");
		}
		UpdateInput();
		Vector3 localScale = obj.transform.localScale;
		obj.transform.parent = m_rarityInfo.m_RevealedCardObject.transform;
		obj.transform.localPosition = Vector3.zero;
		obj.transform.localRotation = Quaternion.identity;
		obj.transform.localScale = localScale;
		m_ready = true;
	}

	private bool IsPortrait()
	{
		if (m_mercenaryPackComponent.MercenaryArtVariationPremium <= 0)
		{
			if (m_mercenaryPackComponent.HasMercenaryArtVariationId && GameDbf.MercenaryArtVariation.HasRecord(m_mercenaryPackComponent.MercenaryArtVariationId))
			{
				return !GameDbf.MercenaryArtVariation.GetRecord(m_mercenaryPackComponent.MercenaryArtVariationId).DefaultVariation;
			}
			return false;
		}
		return true;
	}

	public void OnPackOpeningCoinReady(VisualController visualController)
	{
		if (!(visualController == null))
		{
			m_packOpeningCoin = visualController.GetComponent<PackOpeningCoin>();
		}
	}

	private LettuceMercenaryCoinDataModel GetPackOpeningCoinDataModel()
	{
		Widget owner = m_packOpeningCoin.GetComponent<VisualController>().Owner;
		if (!owner.GetDataModel(238, out var dataModel))
		{
			dataModel = new LettuceMercenaryCoinDataModel();
			owner.BindDataModel(dataModel);
		}
		return dataModel as LettuceMercenaryCoinDataModel;
	}

	private void SetUpPackOpeningCoinDataModel(EntityDef entityDef)
	{
		LettuceMercenaryCoinDataModel packOpeningCoinDataModel = GetPackOpeningCoinDataModel();
		string shortName = entityDef.GetShortName();
		string mercName = (string.IsNullOrEmpty(shortName) ? entityDef.GetName() : shortName);
		packOpeningCoinDataModel.MercenaryId = m_mercenaryPackComponent.MercenaryId;
		packOpeningCoinDataModel.MercenaryName = mercName;
		packOpeningCoinDataModel.Quantity = (int)m_mercenaryPackComponent.CurrencyAmount;
		packOpeningCoinDataModel.GlowActive = false;
		packOpeningCoinDataModel.NameActive = true;
		GameObject obj = m_packOpeningCoin.gameObject;
		m_SharedHiddenCardObject.gameObject.SetActive(value: false);
		obj.GetComponent<VisualController>().SetState("HIDDEN");
		LayerUtils.SetLayer(obj, GameLayer.IgnoreFullScreenEffects);
		UpdateInput();
		Vector3 localScale = obj.transform.localScale;
		obj.transform.parent = m_rarityInfo.m_RevealedCardObject.transform;
		obj.transform.localPosition = Vector3.zero;
		obj.transform.localRotation = Quaternion.identity;
		obj.transform.localScale = localScale;
		m_ready = true;
	}

	private void FireRevealedEvent()
	{
		RevealedListener[] array = m_revealedListeners.ToArray();
		for (int i = 0; i < array.Length; i++)
		{
			array[i].Fire();
		}
	}

	public void ClearEventListeners()
	{
		m_revealedListeners.Clear();
		this.OnSpellFinishedEvent = null;
		this.OnSpellStateFinishedEvent = null;
	}

	private void OnSpellFinished(Spell spell, object userData)
	{
		FireRevealedEvent();
		UpdateInput();
		if (m_showClassName)
		{
			ShowClassName();
		}
		ShowIsNew();
		if (m_packOpeningMercenary != null)
		{
			m_SharedHiddenCardObject.SetActive(value: false);
			m_packOpeningMercenary.ShowMercenaryNameGlow();
			m_packOpeningMercenary.GetComponent<VisualController>().SetState("REVEAL_COMPLETE");
			m_packOpeningCoin.GetComponent<VisualController>().SetState("REVEAL_COMPLETE");
		}
		m_revealButton.AddEventListener(UIEventType.ROLLOVER, OnOverWhileFlipped);
		m_revealButton.AddEventListener(UIEventType.ROLLOUT, OnOutWhileFlipped);
	}

	public PegUIElement GetCardRevealButton()
	{
		return m_revealButton;
	}

	private void ShowClassName()
	{
		string className = GetClassName();
		UberText[] componentsInChildren = m_ClassNameSpell.GetComponentsInChildren<UberText>(includeInactive: true);
		foreach (UberText uberText in componentsInChildren)
		{
			uberText.Text = className;
			if (m_entityDef.IsMultiClass())
			{
				uberText.OutlineSize = 3f;
			}
		}
		m_ClassNameSpell.ActivateState(SpellStateType.BIRTH);
	}

	private void ShowIsNew()
	{
		if (m_isNew && m_IsNewObject != null)
		{
			m_IsNewObject.SetActive(value: true);
		}
	}

	private string GetClassName()
	{
		TAG_CLASS classTag = m_entityDef.GetClass();
		if (m_entityDef.IsMultiClass())
		{
			return GetFamilyClassNames();
		}
		if (classTag == TAG_CLASS.NEUTRAL)
		{
			return GameStrings.Get("GLUE_PACK_OPENING_ALL_CLASSES");
		}
		return GameStrings.GetClassName(classTag);
	}

	private string GetFamilyClassNames()
	{
		if (m_entityDef.HasTag(GAME_TAG.GRIMY_GOONS))
		{
			return GameStrings.Get("GLUE_GOONS_CLASS_NAMES");
		}
		if (m_entityDef.HasTag(GAME_TAG.JADE_LOTUS))
		{
			return GameStrings.Get("GLUE_LOTUS_CLASS_NAMES");
		}
		if (m_entityDef.HasTag(GAME_TAG.KABAL))
		{
			return GameStrings.Get("GLUE_KABAL_CLASS_NAMES");
		}
		List<TAG_CLASS> classes = new List<TAG_CLASS>();
		m_entityDef.GetClasses(classes);
		if (classes.Count() == 10)
		{
			return GameStrings.Get("GLUE_PACK_OPENING_ALL_CLASSES");
		}
		string className = "";
		foreach (TAG_CLASS classTag in classes)
		{
			className += GameStrings.GetClassName(classTag);
			if (classTag != classes.Last())
			{
				className = className + GameStrings.Get("GLOBAL_COMMA_SEPARATOR") + " ";
			}
		}
		return className;
	}

	private void PlayCorrectSound()
	{
		if (DeterminePackOpeningType() == PackOpeningType.COIN)
		{
			SoundManager.Get().LoadAndPlay("MERC_Coin_FlipOver.prefab:b9874b13c58956341aff2030393b9a4c");
			return;
		}
		switch (m_rarityInfo.m_RarityType)
		{
		case PackOpeningRarity.COMMON:
			if (m_premium == TAG_PREMIUM.SIGNATURE)
			{
				SoundManager.Get().LoadAndPlay("VO_Innkeeper_Male_Dwarf_Rarity_Signature_Common_03.prefab:2309811536bcc4848abff75ffd6205e2");
			}
			else if (m_premium == TAG_PREMIUM.GOLDEN)
			{
				SoundManager.Get().LoadAndPlay("VO_ANNOUNCER_FOIL_C_29.prefab:69820e4999e4afa439761151e057a526");
			}
			break;
		case PackOpeningRarity.RARE:
			if (m_premium == TAG_PREMIUM.SIGNATURE)
			{
				SoundManager.Get().LoadAndPlay("VO_Innkeeper_Male_Dwarf_Rarity_Signature_Rare_01.prefab:6cebfb7484c0cda48831d0b0dae81689");
			}
			else if (m_premium == TAG_PREMIUM.GOLDEN)
			{
				SoundManager.Get().LoadAndPlay("VO_ANNOUNCER_FOIL_R_30.prefab:f5bf5bfd8e5f4d247aa8a6da966969cf");
			}
			else
			{
				SoundManager.Get().LoadAndPlay("VO_ANNOUNCER_RARE_27.prefab:8ff0de7a4fd144b4b983caea4c54da4d");
			}
			break;
		case PackOpeningRarity.EPIC:
			if (m_premium == TAG_PREMIUM.SIGNATURE)
			{
				SoundManager.Get().LoadAndPlay("VO_Innkeeper_Male_Dwarf_Rarity_Signature_Epic_05.prefab:cb3de39e03a22fb468a4900374d96f02");
			}
			else if (m_premium == TAG_PREMIUM.GOLDEN)
			{
				SoundManager.Get().LoadAndPlay("VO_ANNOUNCER_FOIL_E_31.prefab:d419d6eca0e2a72469544bae5f11542f");
			}
			else
			{
				SoundManager.Get().LoadAndPlay("VO_ANNOUNCER_EPIC_26.prefab:e76d67f55b976104794c3cf73382e82a");
			}
			break;
		case PackOpeningRarity.LEGENDARY:
			if (m_premium == TAG_PREMIUM.SIGNATURE)
			{
				SoundManager.Get().LoadAndPlay("VO_Innkeeper_Male_Dwarf_Rarity_Signature_Legendary_04.prefab:905e5f71e16de48429ac3371d6b26fd4");
			}
			else if (m_premium == TAG_PREMIUM.GOLDEN)
			{
				SoundManager.Get().LoadAndPlay("VO_ANNOUNCER_FOIL_L_32.prefab:caefd66acfc4e2b4f858035c274b257e");
			}
			else
			{
				SoundManager.Get().LoadAndPlay("VO_ANNOUNCER_LEGENDARY_25.prefab:e015c982aec12bc4893f36396d426750");
			}
			break;
		case PackOpeningRarity.UNCOMMON:
			break;
		}
	}

	private LettuceMercenary.ArtVariation CreateArtVariation()
	{
		if (m_mercenaryPackComponent == null)
		{
			return null;
		}
		MercenaryArtVariationDbfRecord artVariation = (m_mercenaryPackComponent.HasMercenaryArtVariationId ? GameDbf.MercenaryArtVariation.GetRecord(m_mercenaryPackComponent.MercenaryArtVariationId) : LettuceMercenary.GetDefaultArtVariationRecord(m_mercenaryPackComponent.MercenaryId));
		TAG_PREMIUM premium = (m_mercenaryPackComponent.HasMercenaryArtVariationPremium ? ((TAG_PREMIUM)m_mercenaryPackComponent.MercenaryArtVariationPremium) : TAG_PREMIUM.NORMAL);
		return new LettuceMercenary.ArtVariation(artVariation, premium, artVariation.DefaultVariation);
	}
}
