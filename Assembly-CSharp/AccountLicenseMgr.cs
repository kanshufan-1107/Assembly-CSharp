using System;
using System.Collections;
using System.Collections.Generic;
using Blizzard.T5.Core;
using Blizzard.T5.Jobs;
using Blizzard.T5.Services;
using Hearthstone;
using Hearthstone.Core;
using PegasusShared;
using PegasusUtil;
using UnityEngine;

public class AccountLicenseMgr : IService
{
	public enum LicenseUpdateState
	{
		UNKNOWN,
		SUCCESS,
		FAIL
	}

	public delegate void AccountLicensesChangedCallback(List<AccountLicenseInfo> changedLicensesInfo, object userData);

	private class AccountLicensesChangedListener : EventListener<AccountLicensesChangedCallback>
	{
		public void Fire(List<AccountLicenseInfo> changedLicensesInfo)
		{
			m_callback(changedLicensesInfo, m_userData);
		}
	}

	private Map<long, long> m_seenLicenseNotices;

	private LicenseUpdateState m_consumableLicensesUpdateState;

	private LicenseUpdateState m_fixedLicensesUpdateState;

	private List<AccountLicensesChangedListener> m_accountLicensesChangedListeners = new List<AccountLicensesChangedListener>();

	public LicenseUpdateState FixedLicensesState => m_fixedLicensesUpdateState;

	public IEnumerator<IAsyncJobResult> Initialize(ServiceLocator serviceLocator)
	{
		HearthstoneApplication.Get().WillReset += WillReset;
		serviceLocator.Get<Network>().RegisterNetHandler(UpdateAccountLicensesResponse.PacketID.ID, OnAccountLicensesUpdatedResponse);
		serviceLocator.Get<NetCache>().RegisterNewNoticesListener(OnNewNotices);
		yield break;
	}

	public Type[] GetDependencies()
	{
		return new Type[2]
		{
			typeof(Network),
			typeof(NetCache)
		};
	}

	public void Shutdown()
	{
	}

	private void WillReset()
	{
		if (m_seenLicenseNotices != null)
		{
			m_seenLicenseNotices.Clear();
		}
		m_consumableLicensesUpdateState = LicenseUpdateState.UNKNOWN;
		m_fixedLicensesUpdateState = LicenseUpdateState.UNKNOWN;
	}

	public static AccountLicenseMgr Get()
	{
		return ServiceManager.Get<AccountLicenseMgr>();
	}

	public void InitRequests()
	{
		Network.Get().RequestAccountLicensesUpdate();
	}

	public bool OwnsAccountLicense(long license)
	{
		NetCache.NetCacheAccountLicenses netCacheAccountLicenseInfo = NetCache.Get().GetNetObject<NetCache.NetCacheAccountLicenses>();
		if (netCacheAccountLicenseInfo == null)
		{
			return false;
		}
		if (!netCacheAccountLicenseInfo.AccountLicenses.ContainsKey(license))
		{
			return false;
		}
		return OwnsAccountLicense(netCacheAccountLicenseInfo.AccountLicenses[license]);
	}

	public bool OwnsAccountLicense(AccountLicenseInfo accountLicenseInfo)
	{
		if (accountLicenseInfo == null)
		{
			return false;
		}
		return (accountLicenseInfo.Flags_ & 1) == 1;
	}

	public List<AccountLicenseInfo> GetAllOwnedAccountLicenseInfo()
	{
		List<AccountLicenseInfo> allAccountLicenseInfo = new List<AccountLicenseInfo>();
		NetCache.NetCacheAccountLicenses netCacheAccountLicenseInfo = NetCache.Get().GetNetObject<NetCache.NetCacheAccountLicenses>();
		if (netCacheAccountLicenseInfo != null)
		{
			foreach (AccountLicenseInfo netCacheAccountLicense in netCacheAccountLicenseInfo.AccountLicenses.Values)
			{
				if (OwnsAccountLicense(netCacheAccountLicense))
				{
					allAccountLicenseInfo.Add(netCacheAccountLicense);
				}
			}
		}
		return allAccountLicenseInfo;
	}

	public bool RegisterAccountLicensesChangedListener(AccountLicensesChangedCallback callback)
	{
		return RegisterAccountLicensesChangedListener(callback, null);
	}

	public bool RegisterAccountLicensesChangedListener(AccountLicensesChangedCallback callback, object userData)
	{
		AccountLicensesChangedListener listener = new AccountLicensesChangedListener();
		listener.SetCallback(callback);
		listener.SetUserData(userData);
		if (m_accountLicensesChangedListeners.Contains(listener))
		{
			return false;
		}
		m_accountLicensesChangedListeners.Add(listener);
		return true;
	}

	public bool RemoveAccountLicensesChangedListener(AccountLicensesChangedCallback callback)
	{
		return RemoveAccountLicensesChangedListener(callback, null);
	}

	public bool RemoveAccountLicensesChangedListener(AccountLicensesChangedCallback callback, object userData)
	{
		AccountLicensesChangedListener listener = new AccountLicensesChangedListener();
		listener.SetCallback(callback);
		listener.SetUserData(userData);
		return m_accountLicensesChangedListeners.Remove(listener);
	}

	private void OnAccountLicensesUpdatedResponse()
	{
		UpdateAccountLicensesResponse licensesResponse = Network.Get().GetUpdateAccountLicensesResponse();
		m_consumableLicensesUpdateState = (licensesResponse.ConsumableLicenseSuccess ? LicenseUpdateState.SUCCESS : LicenseUpdateState.FAIL);
		m_fixedLicensesUpdateState = (licensesResponse.FixedLicenseSuccess ? LicenseUpdateState.SUCCESS : LicenseUpdateState.FAIL);
		Log.All.Print("OnAccountLicensesUpdatedResponse consumableLicensesUpdateState={0} fixedLicensesUpdateState={1}", m_consumableLicensesUpdateState, m_fixedLicensesUpdateState);
		if (LicenseUpdateState.SUCCESS != m_consumableLicensesUpdateState || LicenseUpdateState.SUCCESS != m_fixedLicensesUpdateState)
		{
			AlertPopup.PopupInfo info = new AlertPopup.PopupInfo();
			info.m_headerText = GameStrings.Get("GLOBAL_ERROR_GENERIC_HEADER");
			info.m_text = GameStrings.Get("GLOBAL_ERROR_ACCOUNT_LICENSES");
			info.m_showAlertIcon = false;
			info.m_responseDisplay = AlertPopup.ResponseDisplay.OK;
			DialogManager.Get().ShowPopup(info);
		}
	}

	private void OnNewNotices(List<NetCache.ProfileNotice> newNotices, bool isInitialNoticeList)
	{
		NetCache.NetCacheAccountLicenses licenses = NetCache.Get().GetNetObject<NetCache.NetCacheAccountLicenses>();
		if (licenses == null)
		{
			Processor.RunCoroutine(OnNewNotices_WaitForNetCacheAccountLicenses(newNotices));
		}
		else
		{
			OnNewNotices_Internal(newNotices, licenses);
		}
	}

	private IEnumerator OnNewNotices_WaitForNetCacheAccountLicenses(List<NetCache.ProfileNotice> newNotices)
	{
		float startTime = Time.realtimeSinceStartup;
		NetCache.NetCacheAccountLicenses licenses = NetCache.Get().GetNetObject<NetCache.NetCacheAccountLicenses>();
		while (licenses == null && Time.realtimeSinceStartup - startTime < 30f)
		{
			yield return null;
			licenses = NetCache.Get().GetNetObject<NetCache.NetCacheAccountLicenses>();
		}
		OnNewNotices_Internal(newNotices, licenses);
	}

	private void OnNewNotices_Internal(List<NetCache.ProfileNotice> newNotices, NetCache.NetCacheAccountLicenses netCacheAccountLicenses)
	{
		if (netCacheAccountLicenses == null)
		{
			Debug.LogWarning("AccountLicenses.OnNewNotices netCacheAccountLicenses is null -- going to ack all ACCOUNT_LICENSE notices assuming NetCache is not yet loaded");
		}
		HashSet<long> updatedAccountLicenseIDs = new HashSet<long>();
		foreach (NetCache.ProfileNotice notice in newNotices)
		{
			if (NetCache.ProfileNotice.NoticeType.ACCOUNT_LICENSE != notice.Type)
			{
				continue;
			}
			NetCache.ProfileNoticeAcccountLicense accountLicenseNotice = notice as NetCache.ProfileNoticeAcccountLicense;
			if (netCacheAccountLicenses != null)
			{
				if (!netCacheAccountLicenses.AccountLicenses.ContainsKey(accountLicenseNotice.License))
				{
					netCacheAccountLicenses.AccountLicenses[accountLicenseNotice.License] = new AccountLicenseInfo
					{
						License = accountLicenseNotice.License,
						Flags_ = 0uL,
						CasId = 0L
					};
				}
				if (accountLicenseNotice.CasID >= netCacheAccountLicenses.AccountLicenses[accountLicenseNotice.License].CasId)
				{
					netCacheAccountLicenses.AccountLicenses[accountLicenseNotice.License].CasId = accountLicenseNotice.CasID;
					if (notice.Origin == NetCache.ProfileNotice.NoticeOrigin.ACCOUNT_LICENSE_FLAGS)
					{
						netCacheAccountLicenses.AccountLicenses[accountLicenseNotice.License].Flags_ = (ulong)notice.OriginData;
					}
					else
					{
						Debug.LogWarning($"AccountLicenses.OnNewNotices unexpected notice origin {notice.Origin} (data={notice.OriginData}) for license {accountLicenseNotice.License} casID {accountLicenseNotice.CasID}");
					}
					long prevSeenCasId = accountLicenseNotice.CasID - 1;
					if (m_seenLicenseNotices != null)
					{
						m_seenLicenseNotices.TryGetValue(accountLicenseNotice.License, out prevSeenCasId);
					}
					if (prevSeenCasId < accountLicenseNotice.CasID)
					{
						updatedAccountLicenseIDs.Add(accountLicenseNotice.License);
					}
					if (m_seenLicenseNotices == null)
					{
						m_seenLicenseNotices = new Map<long, long>();
					}
					m_seenLicenseNotices[accountLicenseNotice.License] = accountLicenseNotice.CasID;
				}
			}
			Network.Get().AckNotice(notice.NoticeID);
		}
		if (netCacheAccountLicenses == null)
		{
			return;
		}
		List<AccountLicenseInfo> updatedAccountLicenses = new List<AccountLicenseInfo>();
		foreach (long updatedAccountLicenseID in updatedAccountLicenseIDs)
		{
			if (netCacheAccountLicenses.AccountLicenses.ContainsKey(updatedAccountLicenseID))
			{
				updatedAccountLicenses.Add(netCacheAccountLicenses.AccountLicenses[updatedAccountLicenseID]);
			}
		}
		if (updatedAccountLicenses.Count != 0)
		{
			AccountLicensesChangedListener[] array = m_accountLicensesChangedListeners.ToArray();
			for (int i = 0; i < array.Length; i++)
			{
				array[i].Fire(updatedAccountLicenses);
			}
		}
	}
}
