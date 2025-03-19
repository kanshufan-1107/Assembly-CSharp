using System;
using Blizzard.T5.Jobs;
using Blizzard.T5.Services;
using Hearthstone.Core;
using Hearthstone.DataModels;
using UnityEngine;

namespace Hearthstone.UI;

[WidgetBehaviorDescription(Path = "Hearthstone/CardTile", UniqueWithinCategory = "asset")]
[AddComponentMenu("")]
public class CardTile : CustomWidgetBehavior
{
	private const string DEFAULT_CARD_ID = "EX1_009";

	public GameObject m_legendaryVfx;

	private CollectionDeckTileActor m_actor;

	private string m_displayedCardId;

	private TAG_PREMIUM m_displayedPremium;

	private int m_displayedCount;

	private bool m_displayedSelected;

	public override bool IsChangingStates => base.IsChangingStatesInternally;

	public override bool GetIsChangingStates(Func<GameObject, bool> includeGameObject)
	{
		return base.IsChangingStatesInternally;
	}

	protected override void OnInitialize()
	{
		base.OnInitialize();
		ServiceManager.InitializeDynamicServicesIfEditor(out var serviceDependencies, typeof(IAssetLoader), typeof(GameDbf), typeof(WidgetRunner), typeof(IAliasedAssetResolver));
		Processor.QueueJob(HearthstoneJobs.CreateJobFromAction("CardTile.CreatePreviewableObject", CreatePreviewableObject, JobFlags.StartImmediately, serviceDependencies));
	}

	private CardTileDataModel GetDesiredData()
	{
		CardTileDataModel dataModel = null;
		if (!GetDataModel(262, out var dm))
		{
			dataModel = new CardTileDataModel
			{
				CardId = "EX1_009",
				Count = 1
			};
		}
		else
		{
			dataModel = dm as CardTileDataModel;
			if (string.IsNullOrEmpty(dataModel.CardId))
			{
				dataModel.CardId = "EX1_009";
			}
		}
		return dataModel;
	}

	private void CreatePreviewableObject()
	{
		if (DefLoader.Get().GetAllEntityDefs().Count == 0)
		{
			DefLoader.Get().LoadAllEntityDefs();
		}
		CreatePreviewableObject(delegate(IPreviewableObject previewable, Action<GameObject> callback)
		{
			HandleStartChangingStates();
			CardTileDataModel desiredData = GetDesiredData();
			string cardId = desiredData.CardId;
			EntityDef entityDef = DefLoader.Get().GetEntityDef(cardId);
			if (entityDef != null)
			{
				entityDef = entityDef.Clone();
				GameObject gameObject = AssetLoader.Get().InstantiatePrefab("DeckCardBar.prefab:c2bab6eea6c3a3a4d90dcd7572075291", AssetLoadingOptions.IgnorePrefabPosition);
				m_actor = gameObject.GetComponent<CollectionDeckTileActor>();
				if (!Application.isPlaying)
				{
					m_actor.Awake();
				}
				m_actor.SetPremium(desiredData.Premium);
				m_actor.SetEntityDef(entityDef);
				m_actor.SetGhosted(desiredData.ForceGhostDisplayStyle);
				bool flag = entityDef.GetRarity() == TAG_RARITY.LEGENDARY;
				bool flag2 = desiredData.ForceGhostDisplayStyle == CollectionDeckTileActor.GhostedState.NOT_INCLUDED;
				m_actor.UpdateDeckCardProperties(flag, isMultiCard: false, desiredData.Count, useSliderAnimations: false);
				m_legendaryVfx.SetActive(flag && !flag2);
				m_actor.UpdateNameTextForRuneBar(entityDef.HasRuneCost);
				using (DefLoader.DisposableCardDef cardDef = DefLoader.Get().GetCardDef(cardId, desiredData.Premium))
				{
					m_actor.SetCardDef(cardDef);
					m_actor.UpdateAllComponents();
				}
				m_actor.m_highlight.SetActive(desiredData.Selected);
				m_actor.UpdateGhostTileEffect();
				m_displayedCardId = cardId;
				m_displayedPremium = desiredData.Premium;
				m_displayedCount = desiredData.Count;
				m_displayedSelected = desiredData.Selected;
				m_actor.transform.SetParent(base.transform, worldPositionStays: false);
				m_actor.transform.localPosition = Vector3.zero;
				callback(m_actor.gameObject);
			}
			HandleDoneChangingStates();
		}, delegate
		{
			CardTileDataModel desiredData2 = GetDesiredData();
			return desiredData2.CardId != m_displayedCardId || desiredData2.Premium != m_displayedPremium || desiredData2.Count != m_displayedCount || desiredData2.Selected != m_displayedSelected;
		});
	}
}
