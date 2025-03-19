using System;
using System.Collections.Generic;
using Blizzard.GameService.SDK.Client.Integration;
using Blizzard.T5.Core;
using Blizzard.T5.Jobs;
using Blizzard.T5.Services;
using Blizzard.Telemetry.WTCG.Client;
using Hearthstone;
using Hearthstone.Core;
using Hearthstone.DataModels;
using Hearthstone.UI;

public class CurrencyManager : IService, IHasUpdate
{
	private class CurrencyCache
	{
		[Flags]
		private enum StatusFlags
		{
			REFRESHING = 1,
			CACHED = 2,
			DIRTY = 4,
			REFRESH_FAILED = 8
		}

		private StatusFlags m_status;

		private int m_requestAttempts;

		private float m_secondsBetweenRequests;

		private DateTime m_lastGetBalanceRequestTime;

		private bool m_lastIsAvailable;

		private bool m_hasAttemptedToRefreshBalance;

		public long Amount { get; private set; }

		public PriceDataModel PriceDataModel { get; }

		public CurrencyType Type { get; }

		public event Action<CurrencyBalanceChangedEventArgs> OnBalanceChanged;

		public event Action OnFirstCache;

		public event Action BalanceAvailabilityChanged;

		public CurrencyCache(CurrencyType type)
		{
			Type = type;
			Amount = 0L;
			PriceDataModel = new PriceDataModel
			{
				Currency = type,
				Amount = 0f,
				DisplayText = string.Empty
			};
			m_status = (StatusFlags)0;
			m_requestAttempts = 0;
			m_secondsBetweenRequests = 8f;
			m_lastGetBalanceRequestTime = DateTime.MinValue;
		}

		public void TryRefresh()
		{
			if (Type == CurrencyType.NONE)
			{
				return;
			}
			if (NeedsRefresh())
			{
				if (ServiceManager.Get<HearthstoneCheckout>().IsClientCreationInProgress() || !IsRefreshableCurrency() || IsRefreshing())
				{
					return;
				}
				string currencyCode = ShopUtils.GetCurrencyCode(Type);
				HearthstoneCheckout commerce = ServiceManager.Get<HearthstoneCheckout>();
				if (commerce == null || !commerce.IsAvailable())
				{
					Log.Store.PrintError("Cannot request virtual currency balance. Commerce service unavailable");
					m_status |= StatusFlags.REFRESH_FAILED;
					FireAvailabilityChangedIfNeeded();
					return;
				}
				m_requestAttempts++;
				m_status |= StatusFlags.REFRESHING;
				m_lastGetBalanceRequestTime = DateTime.UtcNow;
				Log.Store.PrintDebug("Requesting Virtual Currency balance for {0} (attempt #{1})", Type, m_requestAttempts);
				Processor.RunCoroutine(commerce.GetVirtualCurrencyBalance(currencyCode, HandleVirtualCurrencyBalanceCallback, delegate(bool succeeded)
				{
					if (!succeeded)
					{
						Log.Store.PrintWarning("Failed to send getBalance request");
						m_hasAttemptedToRefreshBalance = true;
						m_status |= StatusFlags.REFRESH_FAILED;
						FireAvailabilityChangedIfNeeded();
					}
					else if (m_requestAttempts > 0)
					{
						m_secondsBetweenRequests *= 2f;
						if (m_secondsBetweenRequests >= 64f)
						{
							m_secondsBetweenRequests = 64f;
							Log.Store.PrintError("Request for virtual currency type {0} is taking a very long time.", Type);
						}
					}
				}));
			}
			else if (IsCacheableCurrency())
			{
				long balance = 0L;
				switch (Type)
				{
				case CurrencyType.DUST:
					balance = NetCache.Get()?.GetArcaneDustBalance() ?? 0;
					break;
				case CurrencyType.GOLD:
					balance = NetCache.Get()?.GetGoldBalance() ?? 0;
					break;
				case CurrencyType.RENOWN:
					balance = NetCache.Get()?.GetRenownBalance() ?? 0;
					break;
				case CurrencyType.TAVERN_TICKET:
					balance = NetCache.Get()?.GetArenaTicketBalance() ?? 0;
					break;
				default:
					Log.Store.PrintWarning($"Unsupported currency type to cacheable: {Type}");
					return;
				}
				UpdateBalance(balance);
			}
		}

		public void UpdateBalance(long newAmount)
		{
			bool num = IsCached();
			m_status = StatusFlags.CACHED;
			long oldAmount = Amount;
			Amount = newAmount;
			PriceDataModel.Amount = newAmount;
			PriceDataModel.DisplayText = newAmount.ToString();
			if (this.OnBalanceChanged != null && oldAmount != newAmount)
			{
				this.OnBalanceChanged(new CurrencyBalanceChangedEventArgs(Type, oldAmount, newAmount));
			}
			if (!num && this.OnFirstCache != null)
			{
				this.OnFirstCache();
			}
		}

		public void MarkDirty()
		{
			if (ShopUtils.IsCurrencyVirtual(Type))
			{
				m_status |= StatusFlags.DIRTY;
			}
		}

		public bool IsDirty()
		{
			return (m_status & StatusFlags.DIRTY) != 0;
		}

		public bool IsCached()
		{
			return (m_status & StatusFlags.CACHED) != 0;
		}

		public bool IsRefreshing()
		{
			return (m_status & StatusFlags.REFRESHING) != 0;
		}

		public bool HasError()
		{
			return (m_status & StatusFlags.REFRESH_FAILED) != 0;
		}

		public bool NeedsRefresh()
		{
			if (IsRefreshableCurrency())
			{
				if (IsCached() && !IsDirty())
				{
					return HasError();
				}
				return true;
			}
			return false;
		}

		public bool IsBalanceAvailable()
		{
			if (!IsRefreshableCurrency())
			{
				return true;
			}
			if ((m_status & StatusFlags.CACHED) == 0)
			{
				return false;
			}
			if ((m_status & StatusFlags.REFRESH_FAILED) != 0 && m_requestAttempts >= 3)
			{
				return false;
			}
			return true;
		}

		private bool IsRefreshableCurrency()
		{
			if (ShopUtils.IsCurrencyVirtual(Type))
			{
				return ShopUtils.IsVirtualCurrencyEnabled();
			}
			return false;
		}

		private bool IsCacheableCurrency()
		{
			return !ShopUtils.IsCurrencyVirtual(Type);
		}

		private void FireAvailabilityChangedIfNeeded()
		{
			bool isAvailable = IsBalanceAvailable();
			if (m_lastIsAvailable != isAvailable)
			{
				m_lastIsAvailable = isAvailable;
				this.BalanceAvailabilityChanged?.Invoke();
			}
		}

		private void HandleVirtualCurrencyBalanceCallback(HearthstoneCheckout.VirtualCurrencyBalanceResult result)
		{
			m_hasAttemptedToRefreshBalance = true;
			if (result.isSuccess)
			{
				Log.Store.PrintDebug($"Virtual Currency balance received for {Type}: {result.balance}");
				m_requestAttempts = 0;
				m_secondsBetweenRequests = 8f;
				UpdateBalance(result.balance);
			}
			else
			{
				m_status |= StatusFlags.REFRESH_FAILED;
				Log.Store.PrintError($"Virtual Currency '{Type}' balance refresh failed and will retry shortly. Error: {result.errorMessage}");
			}
			FireAvailabilityChangedIfNeeded();
		}
	}

	private readonly Map<CurrencyType, CurrencyCache> m_currencyCaches = new Map<CurrencyType, CurrencyCache>();

	private Action<CurrencyBalanceChangedEventArgs> m_onCurrencyBalanceChanged;

	Type[] IService.GetDependencies()
	{
		return new Type[2]
		{
			typeof(Network),
			typeof(NetCache)
		};
	}

	IEnumerator<IAsyncJobResult> IService.Initialize(ServiceLocator serviceLocator)
	{
		Network.Get().OnConnectedToBattleNet += OnBattleNetConnectionStateChanged;
		OnBattleNetConnectionStateChanged(BattleNetErrors.ERROR_OK);
		NetCache.Get().RegisterGoldBalanceListener(OnGoldBalanceUpdate);
		NetCache.Get().RegisterUpdatedListener(typeof(NetCache.NetCacheArcaneDustBalance), OnDustBalanceUpdate);
		NetCache.Get().RegisterRenownBalanceListener(OnRenownBalanceUpdate);
		NetCache.Get().RegisterBattlegroundsTokenBalanceListener(OnBattlegroundsTokenBalanceUpdate);
		NetCache.Get().RegisterUpdatedListener(typeof(NetCache.NetPlayerArenaTickets), OnTavernTicketBalanceUpdate);
		HearthstoneApplication.Get().WillReset += OnWillReset;
		yield break;
	}

	void IService.Shutdown()
	{
		foreach (CurrencyCache allCurrencyCache in GetAllCurrencyCaches(forceIncludeVc: true))
		{
			allCurrencyCache.OnFirstCache -= HandleOnCurrencyFirstCached;
			allCurrencyCache.OnBalanceChanged -= SendCurrencyBalanceChanged;
		}
		NetCache netCache = NetCache.Get();
		if (netCache != null)
		{
			netCache.RemoveGoldBalanceListener(OnGoldBalanceUpdate);
			netCache.RemoveUpdatedListener(typeof(NetCache.NetCacheArcaneDustBalance), OnDustBalanceUpdate);
			netCache.RemoveRenownBalanceListener(OnRenownBalanceUpdate);
			netCache.RemoveBattlegroundsTokenBalanceListener(OnBattlegroundsTokenBalanceUpdate);
			netCache.RemoveUpdatedListener(typeof(NetCache.NetPlayerArenaTickets), OnTavernTicketBalanceUpdate);
		}
		HearthstoneApplication.Get().WillReset -= OnWillReset;
	}

	void IHasUpdate.Update()
	{
		if (!ShopUtils.IsVirtualCurrencyEnabled())
		{
			return;
		}
		HearthstoneCheckout hearthstoneCheckout = ServiceManager.Get<HearthstoneCheckout>();
		if (hearthstoneCheckout == null || !hearthstoneCheckout.IsAvailable())
		{
			return;
		}
		HearthstoneCheckout hearthstoneCheckout2 = ServiceManager.Get<HearthstoneCheckout>();
		if ((hearthstoneCheckout2 == null || !hearthstoneCheckout2.IsClientCreationInProgress()) && (StoreManager.Get().GetCurrentStore() != null || (!(Box.Get() == null) && Box.Get().GetState() != Box.State.OPEN)))
		{
			if (ShopUtils.TryGetMainVirtualCurrencyType(out var vcType))
			{
				GetCurrencyCache(vcType).TryRefresh();
			}
			if (ShopUtils.TryGetBoosterVirtualCurrencyType(out var bcType))
			{
				GetCurrencyCache(bcType).TryRefresh();
			}
		}
	}

	public void RefreshWallet()
	{
		GetCurrencyCache(CurrencyType.GOLD).TryRefresh();
		GetCurrencyCache(CurrencyType.DUST).TryRefresh();
		GetCurrencyCache(CurrencyType.RENOWN).TryRefresh();
		GetCurrencyCache(CurrencyType.BG_TOKEN).TryRefresh();
		GetCurrencyCache(CurrencyType.TAVERN_TICKET).TryRefresh();
	}

	public long GetBalance(CurrencyType currencyType)
	{
		return GetCurrencyCache(currencyType).Amount;
	}

	public PriceDataModel GetPriceDataModel(CurrencyType currencyType)
	{
		return GetCurrencyCache(currencyType).PriceDataModel;
	}

	public void AddCurrencyBalanceChangedCallback(Action<CurrencyBalanceChangedEventArgs> e)
	{
		m_onCurrencyBalanceChanged = (Action<CurrencyBalanceChangedEventArgs>)Delegate.Remove(m_onCurrencyBalanceChanged, e);
		m_onCurrencyBalanceChanged = (Action<CurrencyBalanceChangedEventArgs>)Delegate.Combine(m_onCurrencyBalanceChanged, e);
	}

	public void RemoveCurrencyBalanceChangedCallback(Action<CurrencyBalanceChangedEventArgs> e)
	{
		m_onCurrencyBalanceChanged = (Action<CurrencyBalanceChangedEventArgs>)Delegate.Remove(m_onCurrencyBalanceChanged, e);
	}

	public void MarkCurrencyDirty(CurrencyType currencyType)
	{
		GetCurrencyCache(currencyType).MarkDirty();
	}

	public void MarkVirtualCurrencyDirty()
	{
		if (ShopUtils.TryGetMainVirtualCurrencyType(out var vcType))
		{
			GetCurrencyCache(vcType).MarkDirty();
		}
		if (ShopUtils.TryGetBoosterVirtualCurrencyType(out var bcType))
		{
			GetCurrencyCache(bcType).MarkDirty();
		}
	}

	public bool IsBalanceAvailable(CurrencyType currencyType)
	{
		return GetCurrencyCache(currencyType).IsBalanceAvailable();
	}

	public bool DoesCurrencyNeedRefresh(CurrencyType currencyType)
	{
		return GetCurrencyCache(currencyType).NeedsRefresh();
	}

	public bool IsAnyCurrencyCacheRefreshing()
	{
		foreach (CurrencyCache currencyCache in GetAllCurrencyCaches(forceIncludeVc: true))
		{
			if (currencyCache.Type != 0 && currencyCache.IsRefreshing())
			{
				return true;
			}
		}
		return false;
	}

	public bool DoesCurrencyHaveError(CurrencyType currencyType)
	{
		return GetCurrencyCache(currencyType).HasError();
	}

	private IEnumerable<CurrencyCache> GetAllCurrencyCaches(bool forceIncludeVc = false)
	{
		List<CurrencyCache> cacheList = new List<CurrencyCache>
		{
			GetCurrencyCache(CurrencyType.GOLD),
			GetCurrencyCache(CurrencyType.DUST),
			GetCurrencyCache(CurrencyType.RENOWN),
			GetCurrencyCache(CurrencyType.BG_TOKEN),
			GetCurrencyCache(CurrencyType.TAVERN_TICKET)
		};
		if (forceIncludeVc || ShopUtils.IsVirtualCurrencyEnabled())
		{
			if (ShopUtils.TryGetMainVirtualCurrencyType(out var vcType))
			{
				cacheList.Add(GetCurrencyCache(vcType));
			}
			if (ShopUtils.TryGetBoosterVirtualCurrencyType(out var bcType))
			{
				cacheList.Add(GetCurrencyCache(bcType));
			}
		}
		return cacheList;
	}

	private CurrencyCache GetCurrencyCache(CurrencyType currencyType)
	{
		if (!m_currencyCaches.TryGetValue(currencyType, out var cache))
		{
			cache = new CurrencyCache(currencyType);
			cache.BalanceAvailabilityChanged += OnCacheBalanceAvailabilityChanged;
			m_currencyCaches.Add(currencyType, cache);
		}
		return cache;
	}

	private void SendCurrencyBalanceChanged(CurrencyBalanceChangedEventArgs args)
	{
		m_onCurrencyBalanceChanged?.Invoke(args);
	}

	private void HandleOnCurrencyFirstCached()
	{
		List<Balance> balances = new List<Balance>();
		foreach (CurrencyCache cache in GetAllCurrencyCaches())
		{
			if (cache.IsCached())
			{
				balances.Add(new Balance
				{
					Name = Enum.GetName(typeof(CurrencyType), cache.Type).ToLowerInvariant(),
					Amount = cache.PriceDataModel.Amount
				});
				continue;
			}
			return;
		}
		TelemetryManager.Client().SendShopBalanceAvailable(balances);
	}

	private void OnGoldBalanceUpdate(NetCache.NetCacheGoldBalance goldBalance)
	{
		long total = goldBalance.GetTotal();
		Log.Store.PrintDebug("Gold balance updated to {0}", total);
		GetCurrencyCache(CurrencyType.GOLD).UpdateBalance(total);
	}

	private void OnDustBalanceUpdate()
	{
		NetCache netCache = NetCache.Get();
		if (netCache != null)
		{
			long balance = netCache.GetArcaneDustBalance();
			Log.Store.PrintDebug("Arcane Dust balance updated to {0}", balance);
			GetCurrencyCache(CurrencyType.DUST).UpdateBalance(balance);
		}
	}

	private void OnTavernTicketBalanceUpdate()
	{
		NetCache netCache = NetCache.Get();
		if (netCache != null)
		{
			long balance = netCache.GetArenaTicketBalance();
			Log.Store.PrintDebug("Tavern ticket balance updated to {0}", balance);
			GetCurrencyCache(CurrencyType.TAVERN_TICKET).UpdateBalance(balance);
			GlobalDataContext.Get().GetDataModel(24, out var dataModel);
			ShopDataModel shopData = (ShopDataModel)dataModel;
			if (shopData != null)
			{
				shopData.TavernTicketBalance = (int)balance;
			}
		}
	}

	private void OnRenownBalanceUpdate(NetCache.NetCacheRenownBalance renownBalance)
	{
		long total = renownBalance.Balance;
		Log.Store.PrintDebug("Renown balance updated to {0}", total);
		GetCurrencyCache(CurrencyType.RENOWN).UpdateBalance(total);
	}

	private void OnBattlegroundsTokenBalanceUpdate(NetCache.NetCacheBattlegroundsTokenBalance bgTokenBalance)
	{
		long total = bgTokenBalance.Balance;
		Log.Store.PrintDebug("Bg Token balance updated to {0}", total);
		GetCurrencyCache(CurrencyType.BG_TOKEN).UpdateBalance(total);
	}

	private void OnBattleNetConnectionStateChanged(BattleNetErrors bnetErrors)
	{
		if (!BattleNet.IsConnected())
		{
			return;
		}
		IEnumerable<CurrencyCache> allCurrencyCaches = GetAllCurrencyCaches(forceIncludeVc: true);
		bool isAnyCurrencyCached = false;
		foreach (CurrencyCache cache in allCurrencyCaches)
		{
			cache.OnFirstCache -= HandleOnCurrencyFirstCached;
			cache.OnFirstCache += HandleOnCurrencyFirstCached;
			cache.OnBalanceChanged -= SendCurrencyBalanceChanged;
			cache.OnBalanceChanged += SendCurrencyBalanceChanged;
			isAnyCurrencyCached |= cache.IsCached();
		}
		if (isAnyCurrencyCached)
		{
			HandleOnCurrencyFirstCached();
		}
	}

	private void OnCacheBalanceAvailabilityChanged()
	{
		StoreManager.Get().HandleShopAvailabilityChange();
	}

	private void OnWillReset()
	{
		foreach (CurrencyCache value in m_currencyCaches.Values)
		{
			value.BalanceAvailabilityChanged -= OnCacheBalanceAvailabilityChanged;
		}
		m_currencyCaches.Clear();
	}
}
