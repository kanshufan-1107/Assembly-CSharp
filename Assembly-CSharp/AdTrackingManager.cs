using System;
using System.Collections.Generic;
using Blizzard.T5.Jobs;
using Blizzard.T5.Services;
using Blizzard.Telemetry;
using Hearthstone;
using Hearthstone.Attribution;
using Hearthstone.CRM;
using Hearthstone.Store;
using PegasusShared;
using UnityEngine;

public class AdTrackingManager : IService
{
	private static long s_lastTrackedGoldBalanceThisSession;

	private static long s_lastTrackedDustBalanceThisSession;

	private bool m_isLoginFlowCompleted;

	public IEnumerator<IAsyncJobResult> Initialize(ServiceLocator serviceLocator)
	{
		NetCache netCache = serviceLocator.Get<NetCache>();
		try
		{
			GameState.RegisterGameStateInitializedListener(HandleGameCreated);
			netCache.RegisterUpdatedListener(typeof(NetCache.NetCacheGoldBalance), TrackGoldBalanceChanges);
			netCache.RegisterUpdatedListener(typeof(NetCache.NetCacheArcaneDustBalance), TrackDustBalanceChanges);
			serviceLocator.Get<LoginManager>().OnFullLoginFlowComplete += delegate
			{
				m_isLoginFlowCompleted = true;
			};
			HearthstoneApplication.Get().WillReset += delegate
			{
				m_isLoginFlowCompleted = false;
			};
			StoreManager.Get().RegisterSuccessfulPurchaseListener(HandleItemPurchase);
			StoreManager.Get().RegisterStoreShownListener(OnStoreShown);
			SceneMgr.Get().RegisterSceneLoadedEvent(OnSceneLoaded);
		}
		catch (Exception message)
		{
			Debug.LogError(message);
		}
		yield break;
	}

	public Type[] GetDependencies()
	{
		return new Type[2]
		{
			typeof(NetCache),
			typeof(LoginManager)
		};
	}

	public void Shutdown()
	{
	}

	public static AdTrackingManager Get()
	{
		return ServiceManager.Get<AdTrackingManager>();
	}

	public void TrackLogin()
	{
		BlizzardAttributionManager.Get().SendEvent_Login();
		BlizzardCRMManager.Get().SendEvent_SessionStart(null);
	}

	public void TrackFirstLogin()
	{
		BlizzardAttributionManager.Get().SendEvent_FirstLogin();
	}

	public void TrackAccountCreated()
	{
		BlizzardAttributionManager.Get().SendEvent_Registration();
	}

	public void TrackHeadlessAccountCreated(string accountId = null)
	{
		Context messageContext = null;
		if (ulong.TryParse(accountId, out var battlenetId))
		{
			messageContext = new Context
			{
				BnetId = battlenetId
			};
		}
		BlizzardAttributionManager.Get().SendEvent_HeadlessAccountCreated(messageContext);
	}

	public void TrackHeadlessAccountHealedUp(string temporaryGameAccountId)
	{
		BlizzardAttributionManager.Get().SendEvent_HeadlessAccountHealedUp(temporaryGameAccountId);
	}

	public void TrackAdventureProgress(string description)
	{
		Log.AdTracking.Print("Adventure Progress=" + description);
		string eventString = $"Adventure_{description}";
		BlizzardAttributionManager.Get().SendEvent_ContentUnlocked(eventString);
	}

	public void TrackTutorialProgress(TutorialProgress description, bool isVictory = true)
	{
	}

	public void TrackSale(double price, string currencyCode, string productId, string transactionId)
	{
		BlizzardAttributionManager.Get().SendEvent_Purchase(productId, transactionId, 1, currencyCode, isVirtualCurrency: false, (float)price);
		BlizzardCRMManager.Get().SendEvent_RealMoneyTransaction(productId, transactionId, 1, currencyCode, (float)price);
	}

	private void TrackGoldBalanceChanges()
	{
		NetCache.NetCacheGoldBalance balanceObject = NetCache.Get().GetNetObject<NetCache.NetCacheGoldBalance>();
		if (balanceObject != null)
		{
			TrackGenericBalanceChanges("gold", ref s_lastTrackedGoldBalanceThisSession, () => balanceObject.GetTotal());
		}
	}

	private void TrackDustBalanceChanges()
	{
		NetCache.NetCacheArcaneDustBalance balanceObject = NetCache.Get().GetNetObject<NetCache.NetCacheArcaneDustBalance>();
		if (balanceObject != null)
		{
			TrackGenericBalanceChanges("dust", ref s_lastTrackedDustBalanceThisSession, () => balanceObject.Balance);
		}
	}

	private void TrackGenericBalanceChanges(string currencyName, ref long lastTrackedBalance, Func<long> balanceGetter)
	{
		long balance = balanceGetter();
		if (!m_isLoginFlowCompleted)
		{
			lastTrackedBalance = balance;
			return;
		}
		long delta = balance - lastTrackedBalance;
		if (delta != 0L)
		{
			BlizzardAttributionManager.Get().SendEvent_VirtualCurrencyTransaction((int)delta, currencyName);
		}
		lastTrackedBalance = balance;
	}

	private void HandleItemPurchase(ProductInfo bundle, PaymentMethod purchaseMethod)
	{
		StorePackId currentItem = StoreManager.Get().CurrentlySelectedId;
		if (currentItem.Type == StorePackType.BOOSTER && purchaseMethod != 0)
		{
			BlizzardAttributionManager.Get().SendEvent_Purchase(currentItem.Id.ToString(), "", 1, "gold", isVirtualCurrency: true, 100f);
			BlizzardCRMManager.Get().SendEvent_VirtualCurrencyTransaction(currentItem.Id.ToString(), 100, 1, "gold", null);
		}
	}

	private void HandleGameCreated(GameState instance, object userData)
	{
		try
		{
			instance.RegisterGameOverListener(HandleGameEnded);
			FormatType format = GameMgr.Get().GetFormatType();
			BlizzardAttributionManager.Get().SendEvent_GameRoundStart(GameMgr.Get().GetGameType().ToString(), format);
		}
		catch (Exception message)
		{
			Debug.LogError(message);
		}
	}

	private void HandleGameEnded(TAG_PLAYSTATE playState, object userData)
	{
		try
		{
			GameState gameState = GameState.Get();
			if (GameMgr.Get().IsAI())
			{
				int bossId = 0;
				Player opposingPlayer = gameState.GetOpposingSidePlayer();
				if (opposingPlayer != null)
				{
					Card bossCard = opposingPlayer.GetHeroCard();
					if (bossCard != null && bossCard.GetEntity() != null)
					{
						bossId = GameUtils.TranslateCardIdToDbId(bossCard.GetEntity().GetCardId());
					}
				}
				BlizzardAttributionManager.Get().SendEvent_ScenarioResult(GameMgr.Get().GetMissionId(), playState.ToString(), bossId);
			}
			else
			{
				FormatType format = GameMgr.Get().GetFormatType();
				BlizzardAttributionManager.Get().SendEvent_GameRoundEnd(GameMgr.Get().GetGameType().ToString(), playState.ToString(), format);
			}
		}
		catch (Exception message)
		{
			Debug.LogError(message);
		}
	}

	private void OnStoreShown()
	{
		BlizzardAttributionManager.Get().SendEvent_FirstShopVisit();
	}

	private void OnSceneLoaded(SceneMgr.Mode mode, PegasusScene scene, object userData)
	{
		if (mode == SceneMgr.Mode.HUB && GameUtils.IsAnyTutorialComplete())
		{
			string completedTutorial = string.Empty;
			if (GameUtils.IsTraditionalTutorialComplete())
			{
				completedTutorial = "traditional";
			}
			else if (GameUtils.IsMercenariesVillageTutorialComplete())
			{
				completedTutorial = "mercenaries";
			}
			else if (GameUtils.IsBattleGroundsTutorialComplete())
			{
				completedTutorial = "battlegrounds";
			}
			BlizzardAttributionManager.Get().SendEvent_BoxAfterTutorial(completedTutorial);
		}
	}
}
