using System;
using System.Collections.Generic;
using UnityEngine;

[CustomEditClass]
public class AdventureRewardsPreview : MonoBehaviour
{
	public delegate void OnHide();

	[CustomEditField(Sections = "Cards Preview")]
	public GameObject m_CardsContainer;

	[SerializeField]
	private float m_CardWidth = 30f;

	[SerializeField]
	private float m_CardSpacing = 5f;

	[SerializeField]
	private float m_CardClumpAngleIncrement = 10f;

	[SerializeField]
	private Vector3 m_CardClumpSpacing = Vector3.zero;

	[CustomEditField(Sections = "Cards Preview")]
	public UberText m_HeaderTextObject;

	[CustomEditField(Sections = "Cards Preview")]
	public PegUIElement m_BackButton;

	[CustomEditField(Sections = "Cards Preview")]
	public GameObject m_ClickBlocker;

	[CustomEditField(Sections = "Cards Preview")]
	public UIBScrollable m_DisableScrollbar;

	[CustomEditField(Sections = "Cards Preview")]
	public float m_ShowHideAnimationTime = 0.15f;

	[CustomEditField(Sections = "Cards Preview")]
	public bool m_PreviewCardsExpandable;

	[CustomEditField(Sections = "Cards Preview/Hidden Cards")]
	public GameObject m_HiddenCardsLabelObject;

	[CustomEditField(Sections = "Cards Preview/Hidden Cards")]
	public UberText m_HiddenCardsLabel;

	[CustomEditField(Sections = "Cards Preview", Parent = "m_PreviewCardsExpandable")]
	public AdventureRewardsDisplayArea m_CardsPreviewDisplay;

	[CustomEditField(Sections = "Sounds", T = EditType.SOUND_PREFAB)]
	public string m_PreviewAppearSound;

	[CustomEditField(Sections = "Sounds", T = EditType.SOUND_PREFAB)]
	public string m_PreviewShrinkSound;

	private List<List<GameObject>> m_GameObjectBatches = new List<List<GameObject>>();

	private List<OnHide> m_OnHideListeners = new List<OnHide>();

	private int m_HiddenCardCount;

	private ScreenEffectsHandle m_screenEffectsHandle;

	[CustomEditField(Sections = "Cards Preview")]
	public float CardWidth
	{
		get
		{
			return m_CardWidth;
		}
		set
		{
			m_CardWidth = value;
			UpdateRewardPositions();
		}
	}

	[CustomEditField(Sections = "Cards Preview")]
	public float CardSpacing
	{
		get
		{
			return m_CardSpacing;
		}
		set
		{
			m_CardSpacing = value;
			UpdateRewardPositions();
		}
	}

	[CustomEditField(Sections = "Cards Preview")]
	public float CardClumpAngleIncrement
	{
		get
		{
			return m_CardClumpAngleIncrement;
		}
		set
		{
			m_CardClumpAngleIncrement = value;
			UpdateRewardPositions();
		}
	}

	[CustomEditField(Sections = "Cards Preview")]
	public Vector3 CardClumpSpacing
	{
		get
		{
			return m_CardClumpSpacing;
		}
		set
		{
			m_CardClumpSpacing = value;
			UpdateRewardPositions();
		}
	}

	public void Init()
	{
		if (m_BackButton != null && !m_BackButton.HasEventListener(UIEventType.PRESS))
		{
			m_BackButton.AddEventListener(UIEventType.PRESS, delegate
			{
				Navigation.GoBack();
			});
		}
		if (m_screenEffectsHandle == null)
		{
			m_screenEffectsHandle = new ScreenEffectsHandle(this);
		}
	}

	public void AddHideListener(OnHide dlg)
	{
		m_OnHideListeners.Add(dlg);
	}

	public void RemoveHideListener(OnHide dlg)
	{
		m_OnHideListeners.Remove(dlg);
	}

	private bool OnNavigateBack()
	{
		Show(show: false);
		return true;
	}

	public void SetHeaderText(string text)
	{
		m_HeaderTextObject.Text = GameStrings.Format("GLUE_ADVENTURE_REWARDS_PREVIEW_HEADER", text);
	}

	public void AddSpecificCards(List<string> cardIds)
	{
		foreach (string cardId in cardIds)
		{
			List<string> cardBatch = new List<string>();
			cardBatch.Add(cardId);
			AddCardBatch(cardBatch);
		}
	}

	public void AddSpecificCardBacks(List<int> cardBackIds)
	{
		foreach (int cardBackId in cardBackIds)
		{
			AddCardBackBatch(new List<int> { cardBackId });
		}
	}

	public void AddSpecificBoosters(List<BoosterDbId> boosterIds)
	{
		foreach (BoosterDbId boosterId in boosterIds)
		{
			AddBoosterBatch(new List<BoosterDbId> { boosterId });
		}
	}

	public void AddRewardBatch(int scenarioId)
	{
		List<RewardData> rewards = AdventureProgressMgr.Get().GetImmediateRewardsForDefeatingScenario(scenarioId);
		AddRewardBatch(rewards);
	}

	public void AddRewardBatch(List<RewardData> rewards)
	{
		List<string> rewardCards = new List<string>();
		List<int> rewardCardBacks = new List<int>();
		List<BoosterDbId> rewardBoosters = new List<BoosterDbId>();
		foreach (RewardData reward in rewards)
		{
			switch (reward.RewardType)
			{
			case Reward.Type.CARD:
				rewardCards.Add(((CardRewardData)reward).CardID);
				break;
			case Reward.Type.BOOSTER_PACK:
				rewardBoosters.Add((BoosterDbId)((BoosterPackRewardData)reward).Id);
				break;
			case Reward.Type.CARD_BACK:
				rewardCardBacks.Add(((CardBackRewardData)reward).CardBackID);
				break;
			case Reward.Type.RANDOM_CARD:
				Debug.LogWarning("Random Card Rewards are not currently handled by adventure batch rewards.");
				break;
			}
		}
		AddCardBatch(rewardCards);
		AddCardBackBatch(rewardCardBacks);
		AddBoosterBatch(rewardBoosters);
	}

	public void AddCardBatch(List<string> cardIds)
	{
		if (cardIds != null && cardIds.Count != 0)
		{
			List<GameObject> newCardBatch = new List<GameObject>();
			m_GameObjectBatches.Add(newCardBatch);
			AddCardBatch(cardIds, newCardBatch);
		}
	}

	public void AddCardBackBatch(List<int> cardBackIds)
	{
		if (cardBackIds != null && cardBackIds.Count != 0)
		{
			List<GameObject> newCardBackBatch = new List<GameObject>();
			m_GameObjectBatches.Add(newCardBackBatch);
			AddCardBackBatch(cardBackIds, newCardBackBatch);
		}
	}

	public void AddBoosterBatch(List<BoosterDbId> boosterIds)
	{
		if (boosterIds != null && boosterIds.Count != 0)
		{
			List<GameObject> newBoosterBatch = new List<GameObject>();
			m_GameObjectBatches.Add(newBoosterBatch);
			AddBoosterBatch(boosterIds, newBoosterBatch);
		}
	}

	public void SetHiddenCardCount(int hiddenCardCount)
	{
		m_HiddenCardCount = hiddenCardCount;
	}

	public void Reset()
	{
		foreach (List<GameObject> gameObjectBatch in m_GameObjectBatches)
		{
			foreach (GameObject card in gameObjectBatch)
			{
				if (card != null)
				{
					UnityEngine.Object.Destroy(card.gameObject);
				}
			}
		}
		m_HiddenCardCount = 0;
		m_GameObjectBatches.Clear();
	}

	public void Show(bool show)
	{
		if (m_ClickBlocker != null)
		{
			m_ClickBlocker.SetActive(show);
		}
		if (m_DisableScrollbar != null)
		{
			m_DisableScrollbar.Enable(!show);
		}
		if (show)
		{
			UpdateRewardPositions();
			ScreenEffectParameters screenEffectParameters = ScreenEffectParameters.BlurVignetteDesaturatePerspective;
			screenEffectParameters.Time = m_ShowHideAnimationTime;
			m_screenEffectsHandle.StartEffect(screenEffectParameters);
			base.gameObject.SetActive(value: true);
			iTween.ScaleFrom(base.gameObject, iTween.Hash("scale", Vector3.one * 0.05f, "time", m_ShowHideAnimationTime));
			if (!string.IsNullOrEmpty(m_PreviewAppearSound))
			{
				SoundManager.Get().LoadAndPlay(m_PreviewAppearSound);
			}
			Navigation.Push(OnNavigateBack);
			return;
		}
		Vector3 origScale = base.transform.localScale;
		iTween.ScaleTo(base.gameObject, iTween.Hash("scale", Vector3.one * 0.05f, "time", m_ShowHideAnimationTime, "oncomplete", (Action<object>)delegate
		{
			base.gameObject.SetActive(value: false);
			base.transform.localScale = origScale;
			FireHideEvent();
		}));
		if (!string.IsNullOrEmpty(m_PreviewShrinkSound))
		{
			SoundManager.Get().LoadAndPlay(m_PreviewShrinkSound);
		}
		m_screenEffectsHandle.StopEffect();
	}

	private void AddCardBatch(List<string> cardIds, List<GameObject> cardBatch)
	{
		if (cardIds == null || cardIds.Count == 0)
		{
			return;
		}
		for (int i = 0; i < cardIds.Count; i++)
		{
			string cardId = cardIds[i];
			using DefLoader.DisposableFullDef fullDef = DefLoader.Get().GetFullDef(cardId);
			GameObject actorObj = AssetLoader.Get().InstantiatePrefab(ActorNames.GetHandActor(fullDef.EntityDef, TAG_PREMIUM.NORMAL), AssetLoadingOptions.IgnorePrefabPosition);
			Actor actor = actorObj.GetComponent<Actor>();
			actor.SetFullDef(fullDef);
			actor.CreateBannedRibbon();
			GameUtils.SetParent(actor, m_CardsContainer);
			LayerUtils.SetLayer(actor, m_CardsContainer.gameObject.layer);
			cardBatch.Add(actorObj);
			if (!m_PreviewCardsExpandable || !(m_CardsPreviewDisplay != null))
			{
				continue;
			}
			PegUIElement pegUIElement = actor.m_cardMesh.gameObject.AddComponent<PegUIElement>();
			pegUIElement.GetComponent<Collider>().enabled = true;
			pegUIElement.AddEventListener(UIEventType.RELEASE, delegate
			{
				if (!m_CardsPreviewDisplay.IsShowing())
				{
					List<RewardData> rewards = new List<RewardData>
					{
						new CardRewardData(cardId, TAG_PREMIUM.NORMAL, 1)
					};
					m_CardsPreviewDisplay.ShowRewards(rewards, actor.transform.position, actor.transform.position);
				}
			});
		}
	}

	private void AddCardBackBatch(List<int> cardBackIds, List<GameObject> cardBackBatch)
	{
		if (cardBackIds == null || cardBackIds.Count == 0)
		{
			return;
		}
		foreach (int cardBackId in cardBackIds)
		{
			GameObject cardBackObject = CardBackManager.Get().LoadCardBackByIndex(cardBackId).m_GameObject;
			GameUtils.SetParent(cardBackObject, m_CardsContainer);
			LayerUtils.SetLayer(cardBackObject, m_CardsContainer.gameObject.layer, null);
			cardBackBatch.Add(cardBackObject);
		}
	}

	private void AddBoosterBatch(List<BoosterDbId> boosterIds, List<GameObject> boosterBatch)
	{
		if (boosterIds == null || boosterIds.Count == 0)
		{
			return;
		}
		foreach (BoosterDbId boosterId in boosterIds)
		{
			BoosterDbfRecord boosterRecord = GameDbf.Booster.GetRecord((int)boosterId);
			if (boosterRecord != null)
			{
				GameObject boosterObject = AssetLoader.Get().InstantiatePrefab(boosterRecord.PackOpeningPrefab, AssetLoadingOptions.IgnorePrefabPosition);
				boosterObject.GetComponent<UnopenedPack>().m_SingleStack.m_RootObject.SetActive(value: true);
				GameUtils.SetParent(boosterObject, m_CardsContainer);
				LayerUtils.SetLayer(boosterObject, m_CardsContainer.gameObject.layer, null);
				boosterBatch.Add(boosterObject);
			}
		}
	}

	private void UpdateRewardPositions()
	{
		int elementCount = m_GameObjectBatches.Count;
		bool showHiddenCardCount = m_HiddenCardCount > 0;
		bool hasHiddenCardObject = m_HiddenCardsLabelObject != null;
		if (showHiddenCardCount && hasHiddenCardObject)
		{
			elementCount++;
		}
		float shiftleft = ((float)(elementCount - 1) * m_CardSpacing + (float)elementCount * m_CardWidth) * 0.5f - m_CardWidth * 0.5f;
		int clumpCount = 0;
		foreach (List<GameObject> objectBatch in m_GameObjectBatches)
		{
			if (objectBatch.Count == 0)
			{
				continue;
			}
			int rewardCount = 0;
			foreach (GameObject cardObject in objectBatch)
			{
				if (!(cardObject == null))
				{
					Vector3 newPos = m_CardClumpSpacing * rewardCount;
					newPos.x += (float)clumpCount * (m_CardSpacing + m_CardWidth) - shiftleft;
					cardObject.transform.localScale = Vector3.one * 5f;
					cardObject.transform.localRotation = Quaternion.identity;
					cardObject.transform.Rotate(new Vector3(0f, 1f, 0f), (float)rewardCount * m_CardClumpAngleIncrement);
					cardObject.transform.localPosition = newPos;
					Actor cardActor = cardObject.GetComponent<Actor>();
					if (cardActor != null)
					{
						cardActor.SetUnlit();
						cardActor.ContactShadow(visible: true);
						cardActor.UpdateAllComponents();
						cardActor.Show();
					}
					rewardCount++;
				}
			}
			clumpCount++;
		}
		if (showHiddenCardCount && hasHiddenCardObject)
		{
			Vector3 newPos2 = Vector3.zero;
			newPos2.x += (float)clumpCount * (m_CardSpacing + m_CardWidth) - shiftleft;
			m_HiddenCardsLabelObject.transform.localPosition = newPos2;
			m_HiddenCardsLabel.Text = $"+{m_HiddenCardCount}";
		}
		if (hasHiddenCardObject)
		{
			m_HiddenCardsLabelObject.SetActive(showHiddenCardCount);
		}
	}

	private void FireHideEvent()
	{
		OnHide[] array = m_OnHideListeners.ToArray();
		for (int i = 0; i < array.Length; i++)
		{
			array[i]();
		}
	}
}
