using System;
using System.Collections.Generic;
using Hearthstone.DataModels;
using Hearthstone.Progression;
using Hearthstone.UI;

public class RedundantNDERerollPopups : IDisposable
{
	private class RerollPopupInfo
	{
		public List<NetCache.ProfileNoticeRedundantNDEReroll> m_notices = new List<NetCache.ProfileNoticeRedundantNDEReroll>();

		public bool m_waitForReward;

		public int m_cardId = -1;

		public int m_quantityToReroll = -1;

		public List<int> m_premiumsToReroll = new List<int>(2);

		public bool m_isForcedPremium;
	}

	private Action OnPopupShown;

	private Action OnPopupClosed;

	private Action<bool> SetIsShowing;

	private static readonly AssetReference POPUP_PREFAB = new AssetReference("RedundantNDEPopup.prefab:f547c99ed5cef4b419d9ba11c141e89f");

	private Widget m_currentPopup;

	private RedundantNDEPopup m_currentPopupComponent;

	private bool m_isResultDisplaying;

	private Queue<RerollPopupInfo> m_RerollNotices = new Queue<RerollPopupInfo>();

	private Queue<NetCache.ProfileNoticeRedundantNDERerollResult> m_rerollResults = new Queue<NetCache.ProfileNoticeRedundantNDERerollResult>();

	private NetCache.ProfileNoticeRedundantNDERerollResult m_currentRerollResults;

	private RerollPopupInfo m_currentPopupInfo;

	private RewardPresenter m_rewardPresenter = new RewardPresenter();

	private HashSet<int> m_queuedRewardPresenterRewardAssetIds = new HashSet<int>();

	public bool SuppressNDEPopups { get; set; }

	public bool IsWaitingToShow()
	{
		return m_RerollNotices.Count > 0;
	}

	public RedundantNDERerollPopups(Action<bool> setIsShowing, Action onPopupShown, Action onPopupClosed)
	{
		SetIsShowing = setIsShowing;
		OnPopupShown = onPopupShown;
		OnPopupClosed = onPopupClosed;
		SuppressNDEPopups = false;
		StartupRegistration();
	}

	private void StartupRegistration()
	{
		NetCache.Get().RegisterNewNoticesListener(OnNewNotices);
	}

	public void Dispose()
	{
		NetCache.Get().RemoveNewNoticesListener(OnNewNotices);
	}

	public void OnRewardPresenterScrollQueued(int rewardId)
	{
		m_queuedRewardPresenterRewardAssetIds.Add(rewardId);
	}

	public bool ShowRerollPopup()
	{
		if (m_isResultDisplaying)
		{
			return true;
		}
		if (m_currentPopup != null)
		{
			return true;
		}
		if (SuppressNDEPopups)
		{
			return false;
		}
		while (m_rerollResults.Count > 0 && !m_isResultDisplaying)
		{
			m_currentRerollResults = m_rerollResults.Dequeue();
			CardDbfRecord grantedCardRecord = GameDbf.Card.GetRecord(m_currentRerollResults.GrantedCardID);
			if (grantedCardRecord != null)
			{
				RewardScrollDataModel dataModel = new RewardScrollDataModel
				{
					DisplayName = GameStrings.Get("GLOBAL_REDUNDANT_NDE_REROLL_RESULT_HEADER"),
					Description = grantedCardRecord.Name,
					RewardList = new RewardListDataModel
					{
						Items = new DataModelList<RewardItemDataModel>
						{
							new RewardItemDataModel
							{
								ItemType = RewardItemType.CARD,
								ItemId = m_currentRerollResults.GrantedCardID,
								Quantity = 1,
								Card = new CardDataModel
								{
									CardId = grantedCardRecord.NoteMiniGuid,
									Premium = m_currentRerollResults.Premium
								}
							}
						}
					}
				};
				m_rewardPresenter.EnqueueReward(dataModel, delegate
				{
					Network.Get().AckNotice(m_currentRerollResults.NoticeID);
				});
				m_isResultDisplaying = true;
				m_rewardPresenter.ShowNextReward(DismissResults);
				OnPopupShown?.Invoke();
				SetIsShowing?.Invoke(obj: true);
				return true;
			}
		}
		while (m_RerollNotices.Count > 0)
		{
			RerollPopupInfo nextPopupInfo = m_RerollNotices.Peek();
			if (nextPopupInfo.m_waitForReward && !m_queuedRewardPresenterRewardAssetIds.Contains((int)nextPopupInfo.m_notices[0].OriginData))
			{
				return false;
			}
			m_currentPopupInfo = m_RerollNotices.Dequeue();
			if (m_currentPopupInfo == null)
			{
				return false;
			}
			int ownedSignature = 0;
			CollectionManager.Get().GetOwnedCardCount(m_currentPopupInfo.m_notices[0].CardID, out var ownedNormal, out var ownedGolden, out ownedSignature, out var _);
			CollectibleCard goldCard = CollectionManager.Get().GetCard(m_currentPopupInfo.m_notices[0].CardID, TAG_PREMIUM.GOLDEN);
			CollectibleCard normalCard = CollectionManager.Get().GetCard(m_currentPopupInfo.m_notices[0].CardID, TAG_PREMIUM.NORMAL);
			if (NetCache.Get().GetNetObject<NetCache.NetCacheFeatures>().EnableNDERerollSpecialCases)
			{
				switch (m_currentPopupInfo.m_notices[0].RerollPremiumOverride)
				{
				case TAG_PREMIUM.NORMAL:
					ownedGolden = 0;
					m_currentPopupInfo.m_isForcedPremium = true;
					break;
				case TAG_PREMIUM.GOLDEN:
					ownedNormal = 0;
					m_currentPopupInfo.m_isForcedPremium = true;
					break;
				}
			}
			int quantityToReroll = Math.Min((goldCard.IsEverCraftable ? ownedGolden : 0) + (normalCard.IsEverCraftable ? ownedNormal : 0), m_currentPopupInfo.m_notices.Count);
			if (quantityToReroll == 0)
			{
				foreach (NetCache.ProfileNoticeRedundantNDEReroll rerollNotice in m_currentPopupInfo.m_notices)
				{
					Network.Get().AckNotice(rerollNotice.NoticeID);
				}
				continue;
			}
			m_currentPopupInfo.m_quantityToReroll = quantityToReroll;
			m_currentPopupInfo.m_cardId = normalCard.CardDbId;
			OnPopupShown?.Invoke();
			SetIsShowing?.Invoke(obj: true);
			m_currentPopup = WidgetInstance.Create(POPUP_PREFAB);
			m_currentPopup.RegisterReadyListener(delegate
			{
				m_currentPopupComponent = m_currentPopup.GetComponentInChildren<RedundantNDEPopup>();
				if (m_currentPopupComponent != null)
				{
					m_currentPopupComponent.Show();
					m_currentPopupComponent.RerollSelected += OnRerollSelected;
					m_currentPopupComponent.RefuseSelected += OnRefuseSelected;
				}
			});
			TAG_PREMIUM premium1 = TAG_PREMIUM.NORMAL;
			TAG_PREMIUM premium2 = TAG_PREMIUM.NORMAL;
			premium1 = ((ownedGolden > 0 && goldCard.IsEverCraftable) ? TAG_PREMIUM.GOLDEN : TAG_PREMIUM.NORMAL);
			premium2 = ((ownedGolden > 1 && goldCard.IsEverCraftable) ? TAG_PREMIUM.GOLDEN : TAG_PREMIUM.NORMAL);
			CardDataModel cardDataModel1 = new CardDataModel
			{
				CardId = m_currentPopupInfo.m_notices[0].CardID,
				Premium = premium1
			};
			CardDataModel cardDataModel2 = new CardDataModel
			{
				CardId = m_currentPopupInfo.m_notices[0].CardID,
				Premium = premium2
			};
			RandomCardDataModel randomCardDataModel1 = new RandomCardDataModel
			{
				Rarity = normalCard.Rarity,
				Premium = premium1
			};
			RandomCardDataModel randomCardDataModel2 = new RandomCardDataModel
			{
				Rarity = normalCard.Rarity,
				Premium = premium2
			};
			NDERerollPopupDataModel popupDataModel = new NDERerollPopupDataModel();
			popupDataModel.RerollCards.Add(cardDataModel1);
			popupDataModel.RerollCards.Add(cardDataModel2);
			popupDataModel.RandomCards.Add(randomCardDataModel1);
			popupDataModel.RandomCards.Add(randomCardDataModel2);
			popupDataModel.Quantity = quantityToReroll;
			GameStrings.PluralNumber[] quantityToRerollPlurals = GameStrings.MakePlurals(quantityToReroll);
			TAG_CARD_SET cardSet = normalCard.Set;
			if (GameUtils.IsLegacySet(cardSet))
			{
				cardSet = TAG_CARD_SET.LEGACY;
			}
			if (premium1 == premium2 || quantityToReroll == 1)
			{
				popupDataModel.HeaderText = GameStrings.FormatPlurals("GLOBAL_REDUNDANT_NDE_TITLE", quantityToRerollPlurals, GameStrings.GetPremiumText(premium1));
				popupDataModel.BodyText = GameStrings.FormatPlurals("GLOBAL_REDUNDANT_NDE_BODY", quantityToRerollPlurals, GameStrings.GetPremiumText(m_currentPopupInfo.m_notices[0].Premium), GameStrings.GetPremiumText(premium1), GameStrings.GetCardSetName(cardSet), GameStrings.GetRarityText(normalCard.Rarity));
				for (int i = 0; i < quantityToReroll; i++)
				{
					m_currentPopupInfo.m_premiumsToReroll.Add((int)premium1);
				}
			}
			else
			{
				popupDataModel.HeaderText = GameStrings.Format("GLOBAL_REDUNDANT_NDE_TITLE_MULTIPLE_PREMIUMS", GameStrings.GetPremiumText(premium1), GameStrings.GetPremiumText(premium2));
				popupDataModel.BodyText = GameStrings.FormatPlurals("GLOBAL_REDUNDANT_NDE_BODY_MULTIPLE_PREMIUMS", quantityToRerollPlurals, GameStrings.GetPremiumText(m_currentPopupInfo.m_notices[0].Premium), GameStrings.GetPremiumText(premium1), GameStrings.GetPremiumText(premium2), GameStrings.GetCardSetName(cardSet), GameStrings.GetRarityText(normalCard.Rarity));
				m_currentPopupInfo.m_premiumsToReroll.Add((int)premium1);
				m_currentPopupInfo.m_premiumsToReroll.Add((int)premium2);
			}
			m_currentPopup.BindDataModel(popupDataModel);
			return true;
		}
		return false;
	}

	private void OnNewNotices(List<NetCache.ProfileNotice> newNotices, bool isInitialNoticeList)
	{
		newNotices.ForEach(delegate(NetCache.ProfileNotice notice)
		{
			if (notice.Origin == NetCache.ProfileNotice.NoticeOrigin.NOTICE_ORIGIN_NDE_REDUNDANT_REROLL && notice.Type == NetCache.ProfileNotice.NoticeType.REDUNDANT_NDE_REROLL)
			{
				bool flag = false;
				NetCache.ProfileNoticeRedundantNDEReroll profileNoticeRedundantNDEReroll = notice as NetCache.ProfileNoticeRedundantNDEReroll;
				foreach (RerollPopupInfo current in m_RerollNotices)
				{
					NetCache.ProfileNoticeRedundantNDEReroll profileNoticeRedundantNDEReroll2 = current.m_notices[0];
					if (profileNoticeRedundantNDEReroll2 != null && profileNoticeRedundantNDEReroll2.CardID == profileNoticeRedundantNDEReroll.CardID && profileNoticeRedundantNDEReroll2.Premium == profileNoticeRedundantNDEReroll.Premium)
					{
						current.m_notices.Add(profileNoticeRedundantNDEReroll);
						flag = true;
						break;
					}
				}
				if (!flag)
				{
					RerollPopupInfo item = new RerollPopupInfo
					{
						m_notices = { profileNoticeRedundantNDEReroll },
						m_waitForReward = (!isInitialNoticeList && profileNoticeRedundantNDEReroll.OriginData != 0)
					};
					m_RerollNotices.Enqueue(item);
				}
			}
			else if (notice.Origin == NetCache.ProfileNotice.NoticeOrigin.NOTICE_ORIGIN_NDE_REDUNDANT_REROLL && notice.Type == NetCache.ProfileNotice.NoticeType.REDUNDANT_NDE_REROLL_RESULT)
			{
				m_rerollResults.Enqueue(notice as NetCache.ProfileNoticeRedundantNDERerollResult);
			}
		});
	}

	private void OnRerollSelected()
	{
		OnPopupOptionSelected(isRerollSelected: true);
	}

	private void OnRefuseSelected()
	{
		OnPopupOptionSelected(isRerollSelected: false);
	}

	private void OnPopupOptionSelected(bool isRerollSelected)
	{
		List<long> noticeIDs = new List<long>(m_currentPopupInfo.m_notices.Count);
		foreach (NetCache.ProfileNoticeRedundantNDEReroll rerollNotice in m_currentPopupInfo.m_notices)
		{
			noticeIDs.Add(rerollNotice.NoticeID);
		}
		Network.Get().RespondToRedundantNDEReroll(noticeIDs, isRerollSelected);
		TelemetryManager.Client().SendNDERerollPopupClicked(isRerollSelected, noticeIDs, m_currentPopupInfo.m_cardId, m_currentPopupInfo.m_premiumsToReroll, m_currentPopupInfo.m_quantityToReroll, (int)m_currentPopupInfo.m_notices[0].Premium, m_currentPopupInfo.m_isForcedPremium);
		DismissPopup();
	}

	private void DismissPopup()
	{
		m_currentPopupComponent.RerollSelected -= OnRerollSelected;
		m_currentPopupComponent.RefuseSelected -= OnRefuseSelected;
		m_currentPopupComponent.OnDismissAnimationComplete += OnDismissAnimationComplete;
		m_currentPopupComponent.StartCoroutine(m_currentPopupComponent.Hide());
	}

	private void OnDismissAnimationComplete()
	{
		m_currentPopup.Hide();
		m_currentPopup = null;
		m_currentPopupComponent = null;
		m_currentPopupInfo = null;
		OnPopupClosed?.Invoke();
		SetIsShowing?.Invoke(obj: false);
	}

	private void DismissResults()
	{
		m_currentRerollResults = null;
		m_isResultDisplaying = false;
		OnPopupClosed?.Invoke();
		SetIsShowing?.Invoke(obj: false);
	}
}
