using System.Runtime.CompilerServices;
using Hearthstone.DataModels;
using Hearthstone.UI;
using PegasusLettuce;
using UnityEngine;

public class MythicUpgradeDisplay : MonoBehaviour
{
	protected class PendingRequest
	{
		[CompilerGenerated]
		private int _003CUpgradeId_003Ek__BackingField;

		[CompilerGenerated]
		private MERCENARY_MYTHIC_UPGRADE_TYPE _003CUpgradeType_003Ek__BackingField;

		[CompilerGenerated]
		private long _003CMythicLevel_003Ek__BackingField;

		[CompilerGenerated]
		private int _003CRequestedCount_003Ek__BackingField;

		public int UpgradeId
		{
			[CompilerGenerated]
			set
			{
				_003CUpgradeId_003Ek__BackingField = value;
			}
		}

		public MERCENARY_MYTHIC_UPGRADE_TYPE UpgradeType
		{
			[CompilerGenerated]
			set
			{
				_003CUpgradeType_003Ek__BackingField = value;
			}
		}

		public long MythicLevel
		{
			[CompilerGenerated]
			set
			{
				_003CMythicLevel_003Ek__BackingField = value;
			}
		}

		public int RequestedCount
		{
			[CompilerGenerated]
			set
			{
				_003CRequestedCount_003Ek__BackingField = value;
			}
		}
	}

	[CustomEditField(Sections = "Widgets")]
	public AsyncReference m_detailsDisplayReference;

	[CustomEditField(Sections = "Widgets")]
	public AsyncReference m_popupHandlerReference;

	[CustomEditField(Sections = "Widgets")]
	public AsyncReference m_mythicUpgradePopupInstanceReference;

	[CustomEditField(Sections = "Widgets")]
	public AsyncReference m_mythicUpgradePopupWidgetReference;

	[CustomEditField(Sections = "Widgets")]
	public AsyncReference m_abilityUpgradeCardReference;

	protected VisualController m_detailsDisplayVisualController;

	protected VisualController m_popupHandlerVisualController;

	protected VisualController m_mythicUpgradePopupInstanceVisualController;

	protected Widget m_mythicUpgradePopupWidget;

	protected Hearthstone.UI.Card m_abilityUpgradeCard;

	protected MaterialDataModel m_abilityMaterialData = new MaterialDataModel();

	protected PendingRequest m_pendingRequest;

	protected void Start()
	{
		m_detailsDisplayReference.RegisterReadyListener<VisualController>(OnDetailsDisplayReady);
		m_popupHandlerReference.RegisterReadyListener<VisualController>(OnPopupHandlerReady);
		m_mythicUpgradePopupInstanceReference.RegisterReadyListener<VisualController>(OnMythicUpgradePopupInstanceReady);
		m_mythicUpgradePopupWidgetReference.RegisterReadyListener(delegate(Widget widget)
		{
			m_mythicUpgradePopupWidget = widget;
		});
		m_abilityUpgradeCardReference.RegisterReadyListener(delegate(Hearthstone.UI.Card card)
		{
			m_abilityUpgradeCard = card;
		});
		NetCache netCache = NetCache.Get();
		netCache.RegisterUpdatedListener(typeof(NetCache.NetCacheMercenariesMythicTreasureInfo), NetCache_OnTreasureInfoUpdated);
		netCache.RegisterUpdatedListener(typeof(NetCache.NetCacheRenownBalance), NetCache_OnRenownBalanceUpdated);
		Network network = Network.Get();
		network.RegisterNetHandler(MercenariesMythicUpgradeAbilityResponse.PacketID.ID, OnMercenariesMythicUpgradeAbilityResponse);
		network.RegisterNetHandler(MercenariesMythicUpgradeEquipmentResponse.PacketID.ID, OnMercenariesMythicUpgradeEquipmentResponse);
	}

	public void Unload()
	{
		NetCache netCache = NetCache.Get();
		if (netCache != null)
		{
			netCache.RemoveUpdatedListener(typeof(NetCache.NetCacheMercenariesMythicTreasureInfo), NetCache_OnTreasureInfoUpdated);
			netCache.RemoveUpdatedListener(typeof(NetCache.NetCacheMercenariesMythicTreasureInfo), NetCache_OnRenownBalanceUpdated);
		}
		Network net = Network.Get();
		if (net != null)
		{
			net.RemoveNetHandler(MercenariesMythicUpgradeAbilityResponse.PacketID.ID, OnMercenariesMythicUpgradeAbilityResponse);
			net.RemoveNetHandler(MercenariesMythicUpgradeEquipmentResponse.PacketID.ID, OnMercenariesMythicUpgradeEquipmentResponse);
		}
	}

	public void OnDestroy()
	{
		Unload();
	}

	public static bool ShouldBeHandledByMythic(LettuceMercenaryDataModel mercenaryDataModel, LettuceAbilityDataModel abilityData)
	{
		if (mercenaryDataModel.MythicView && !mercenaryDataModel.IsMythicFirstUnlock && (abilityData == null || abilityData.CanMythicScale))
		{
			return true;
		}
		return false;
	}

	protected void OnDetailsDisplayReady(VisualController visualController)
	{
		m_detailsDisplayVisualController = visualController;
		m_detailsDisplayVisualController.GetComponent<Widget>().RegisterEventListener(Display_EventListener);
	}

	protected void OnPopupHandlerReady(VisualController visualController)
	{
		m_popupHandlerVisualController = visualController;
	}

	protected void OnMythicUpgradePopupInstanceReady(VisualController visualController)
	{
		m_mythicUpgradePopupInstanceVisualController = visualController;
		m_mythicUpgradePopupInstanceVisualController.BindDataModel(m_abilityMaterialData);
	}

	protected void Display_EventListener(string eventName)
	{
		switch (eventName)
		{
		case "MYTHIC_UPGRADE_code":
		{
			MercenaryMythicUpgradeDisplayDataModel dataModel = WidgetUtils.GetEventDataModel(m_detailsDisplayVisualController).Payload as MercenaryMythicUpgradeDisplayDataModel;
			OnUpgradeRequest(dataModel);
			break;
		}
		case "ABILITY_CLICKED_code":
			OnMythicUpgradeableClicked(WidgetUtils.GetEventDataModel(m_detailsDisplayVisualController)?.Payload as IDataModel);
			break;
		case "MYTHIC_FIRST_UNLOCK_code":
			OnMythicUnlockingShown();
			break;
		}
	}

	protected void NetCache_OnTreasureInfoUpdated()
	{
		if (m_pendingRequest != null)
		{
			m_pendingRequest = null;
			RefreshDataModels();
		}
	}

	protected void OnMercenariesMythicUpgradeAbilityResponse()
	{
		if (m_pendingRequest != null)
		{
			m_pendingRequest = null;
			ApplyMythicUpgradeAbilityResponse(Network.Get().MercenariesMythicUpgradeAbilityResponse());
			RefreshDataModels();
		}
	}

	protected void OnMercenariesMythicUpgradeEquipmentResponse()
	{
		if (m_pendingRequest != null)
		{
			m_pendingRequest = null;
			ApplyMythicUpgradeEquipmentResponse(Network.Get().MercenariesMythicUpgradeEquipmentResponse());
			RefreshDataModels();
		}
	}

	protected void NetCache_OnRenownBalanceUpdated()
	{
		MercenaryMythicUpgradeDisplayDataModel upgradeDisplayDataModel = GetUpgradeDisplayDataModel();
		if (upgradeDisplayDataModel == null)
		{
			Log.Lettuce.PrintError("NetCache_OnRenownBalanceUpdated - failed to get upgrade data model!");
			return;
		}
		upgradeDisplayDataModel.CurrentRenown = NetCache.Get().GetRenownBalance();
		LettuceMercenaryDataModel mercDataModel = GetMercenaryDataModel();
		if (mercDataModel == null)
		{
			Log.Lettuce.PrintError("NetCache_OnRenownBalanceUpdated - failed to get mercenary data model!");
			return;
		}
		LettuceMercenary merc = CollectionManager.Get()?.GetMercenary(mercDataModel.MercenaryId);
		if (merc != null)
		{
			CollectionUtils.UpdateMythicUpgradability(mercDataModel, merc);
		}
	}

	protected void OnMythicUnlockingShown()
	{
		Options.Get().SetBool(Option.HAS_SEEN_MYTHIC_UNLOCK, val: true);
		LettuceMercenaryDataModel mercenaryDataModel = GetMercenaryDataModel();
		if (mercenaryDataModel == null)
		{
			Log.Lettuce.PrintError("OnMythicUnlockingShown - failed to get mercenary data model!");
		}
		else
		{
			mercenaryDataModel.IsMythicFirstUnlock = false;
		}
	}

	protected void OnMythicUpgradeableClicked(IDataModel dataModel)
	{
		MercenaryMythicUpgradeDisplayDataModel upgradeData = null;
		LettuceMercenaryDataModel mercenaryDataModel = GetMercenaryDataModel();
		if (mercenaryDataModel == null)
		{
			Log.Lettuce.PrintError("OnMythicUpgradeableClicked - failed to get mercenary data model!");
			return;
		}
		LettuceAbilityDataModel abilityData = dataModel as LettuceAbilityDataModel;
		if (!ShouldBeHandledByMythic(mercenaryDataModel, abilityData))
		{
			return;
		}
		LettuceMercenary mercenary = CollectionManager.Get().GetMercenary(mercenaryDataModel.MercenaryId);
		if (mercenary == null)
		{
			Log.Lettuce.PrintError($"OnMythicUpgradeableClicked - could not load mercenary with id {mercenaryDataModel.MercenaryId}");
			return;
		}
		if (abilityData != null)
		{
			CardDataModel templateCardModel = abilityData.AbilityTiers[abilityData.MaxTier - 1].AbilityTierCard;
			upgradeData = new MercenaryMythicUpgradeDisplayDataModel();
			upgradeData.UpgradeId = abilityData.AbilityId;
			upgradeData.UpgradeType = ((!abilityData.IsEquipment) ? MERCENARY_MYTHIC_UPGRADE_TYPE.ABILITY : MERCENARY_MYTHIC_UPGRADE_TYPE.EQUIPMENT);
			upgradeData.CurrentCard = new CardDataModel
			{
				CardId = templateCardModel.CardId,
				FlavorText = templateCardModel.FlavorText,
				Premium = templateCardModel.Premium
			};
			upgradeData.CurrentMythicLevel = abilityData.MythicModifier;
		}
		if (dataModel is MercenaryMythicTreasureDataModel treasureData)
		{
			upgradeData = new MercenaryMythicUpgradeDisplayDataModel();
			upgradeData.UpgradeId = treasureData.TreasureId;
			upgradeData.UpgradeType = MERCENARY_MYTHIC_UPGRADE_TYPE.TREASURE;
			upgradeData.CurrentCard = treasureData.MyticTreasure;
			upgradeData.CurrentMythicLevel = treasureData.TreasureScalar;
		}
		if (upgradeData == null)
		{
			Log.Lettuce.PrintError("MercenaryDetailDisplay.OnMythicUpgradeableClicked - payload was not a valid datamodel!");
			return;
		}
		CollectionUtils.PopulateMercenaryMythicUpgradeDisplay(upgradeData, mercenaryDataModel, mercenary);
		if (m_mythicUpgradePopupInstanceVisualController != null)
		{
			m_mythicUpgradePopupInstanceVisualController.BindDataModel(upgradeData);
			m_popupHandlerVisualController.SetState("SHOW_MYTHIC_UPGRADE_POPUP");
		}
	}

	protected void OnUpgradeRequest(MercenaryMythicUpgradeDisplayDataModel dataModel)
	{
		if (m_pendingRequest != null)
		{
			return;
		}
		if (dataModel == null)
		{
			Log.Lettuce.PrintError("OnUpgradeRequest - data model was null");
			return;
		}
		if (dataModel.UpgradeType == MERCENARY_MYTHIC_UPGRADE_TYPE.INVALID)
		{
			Log.Lettuce.PrintError($"OnUpgradeRequest - wrong upgrade type {dataModel.UpgradeType}");
			return;
		}
		if (dataModel.CurrentChoice < 0 && dataModel.CurrentChoice >= dataModel.UpgradeChoices.Count)
		{
			Log.Lettuce.PrintError($"OnUpgradeRequest - current choice out of range {dataModel.CurrentChoice}");
			return;
		}
		LettuceMercenaryDataModel mercDataModel = GetMercenaryDataModel();
		if (mercDataModel == null)
		{
			Log.Lettuce.PrintError("OnUpgradeRequest - mercenary data model is null.");
			return;
		}
		MercenaryMythicUpgradeChoiceDataModel choice = dataModel.UpgradeChoices[dataModel.CurrentChoice];
		m_pendingRequest = new PendingRequest
		{
			UpgradeId = dataModel.UpgradeId,
			UpgradeType = dataModel.UpgradeType,
			MythicLevel = dataModel.CurrentMythicLevel,
			RequestedCount = choice.LevelCount
		};
		switch (dataModel.UpgradeType)
		{
		case MERCENARY_MYTHIC_UPGRADE_TYPE.ABILITY:
		{
			MercenariesMythicUpgradeAbilityRequest request3 = new MercenariesMythicUpgradeAbilityRequest
			{
				MercenaryId = mercDataModel.MercenaryId,
				AbilityId = dataModel.UpgradeId,
				StartingModifier = (uint)dataModel.CurrentMythicLevel
			};
			request3.PurchaseCount = (uint)choice.LevelCount;
			Network.Get().MercenariesMythicUpgradeAbilityRequest(request3);
			break;
		}
		case MERCENARY_MYTHIC_UPGRADE_TYPE.EQUIPMENT:
		{
			MercenariesMythicUpgradeEquipmentRequest request2 = new MercenariesMythicUpgradeEquipmentRequest
			{
				MercenaryId = mercDataModel.MercenaryId,
				EquipmentId = dataModel.UpgradeId,
				StartingModifier = (uint)dataModel.CurrentMythicLevel
			};
			request2.PurchaseCount = (uint)choice.LevelCount;
			Network.Get().MercenariesMythicUpgradeEquipmentRequest(request2);
			break;
		}
		case MERCENARY_MYTHIC_UPGRADE_TYPE.TREASURE:
		{
			MercenariesMythicTreasureScalarPurchaseRequest request = new MercenariesMythicTreasureScalarPurchaseRequest();
			request.TreasureScalar = new MercenaryMythicTreasureScalar
			{
				TreasureId = dataModel.UpgradeId,
				Scalar = (uint)dataModel.CurrentMythicLevel
			};
			request.PurchaseCount = (uint)choice.LevelCount;
			Network.Get().MercenariesMythicTreasureScalarPurchaseRequest(request);
			break;
		}
		}
		m_abilityMaterialData.Material = m_abilityUpgradeCard.CardActor.GetPortraitMaterial();
		if (m_mythicUpgradePopupWidget != null)
		{
			m_mythicUpgradePopupWidget.TriggerEvent("PLAY_ANIMATION");
		}
	}

	protected void ApplyMythicUpgradeAbilityResponse(MercenariesMythicUpgradeAbilityResponse packet)
	{
		if (packet == null || !packet.Success || !packet.HasMercenaryId || !packet.HasAbilityId || !packet.HasFinalModifier || !packet.HasPurchaseCount)
		{
			return;
		}
		LettuceMercenary merc = CollectionManager.Get()?.GetMercenary(packet.MercenaryId);
		if (merc != null)
		{
			LettuceAbility ability = merc.GetLettuceAbility(packet.AbilityId);
			if (ability != null)
			{
				ability.m_mythicModifier = (int)packet.FinalModifier;
			}
		}
	}

	protected void ApplyMythicUpgradeEquipmentResponse(MercenariesMythicUpgradeEquipmentResponse packet)
	{
		if (packet == null || !packet.Success || !packet.HasMercenaryId || !packet.HasEquipmentId || !packet.HasFinalModifier || !packet.HasPurchaseCount)
		{
			return;
		}
		LettuceMercenary merc = CollectionManager.Get()?.GetMercenary(packet.MercenaryId);
		if (merc != null)
		{
			LettuceAbility equipment = merc.GetLettuceEquipment(packet.EquipmentId);
			if (equipment != null)
			{
				equipment.m_mythicModifier = (int)packet.FinalModifier;
			}
		}
	}

	private LettuceMercenaryDataModel GetMercenaryDataModel()
	{
		if (m_detailsDisplayVisualController == null)
		{
			return null;
		}
		m_detailsDisplayVisualController.Owner.GetDataModel(216, out var dataModel);
		return dataModel as LettuceMercenaryDataModel;
	}

	private MercenaryMythicUpgradeDisplayDataModel GetUpgradeDisplayDataModel()
	{
		if (m_mythicUpgradePopupInstanceVisualController == null)
		{
			return null;
		}
		m_mythicUpgradePopupInstanceVisualController.GetDataModel(767, out var dataModel);
		return dataModel as MercenaryMythicUpgradeDisplayDataModel;
	}

	protected void RefreshDataModels()
	{
		LettuceMercenaryDataModel mercDataModel = GetMercenaryDataModel();
		if (mercDataModel != null)
		{
			LettuceMercenary merc = CollectionManager.Get()?.GetMercenary(mercDataModel.MercenaryId);
			if (merc != null)
			{
				CollectionUtils.PopulateMercenaryDataModel(mercDataModel, merc, CollectionUtils.MercenaryDataPopluateExtra.MythicAll | CollectionUtils.MercenaryDataPopluateExtra.Abilities | CollectionUtils.MercenaryDataPopluateExtra.Coin | CollectionUtils.MercenaryDataPopluateExtra.Appearances | CollectionUtils.MercenaryDataPopluateExtra.UpdateValuesWithSlottedEquipment | CollectionUtils.MercenaryDataPopluateExtra.ShowMercCardText);
				mercDataModel.MercenarySelected = true;
				mercDataModel.ChildUpgradeAvailable = false;
			}
		}
	}
}
