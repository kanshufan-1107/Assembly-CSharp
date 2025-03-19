using System;
using System.Collections.Generic;
using Blizzard.T5.Jobs;
using Blizzard.T5.Services;
using PegasusShared;
using UnityEngine;

public class GenericRewardChestNoticeManager : IService
{
	private class GenericRewardChestAssetStatus
	{
		public bool m_isReady;

		public HashSet<long> m_noticeIds = new HashSet<long>();
	}

	public delegate void GenericRewardUpdatedCallback(long receivedRewardNoticeIds, object userData);

	private class GenericRewardChestUpdatedListener : EventListener<GenericRewardUpdatedCallback>
	{
		public void Fire(long receivedRewardNoticeIds)
		{
			m_callback(receivedRewardNoticeIds, m_userData);
		}
	}

	private Dictionary<int, GenericRewardChestAssetStatus> m_mapOfRewardChestAssetIdToNoticeIds = new Dictionary<int, GenericRewardChestAssetStatus>();

	private List<GenericRewardChestUpdatedListener> m_genericRewardUpdatedListeners = new List<GenericRewardChestUpdatedListener>();

	public IEnumerator<IAsyncJobResult> Initialize(ServiceLocator serviceLocator)
	{
		serviceLocator.Get<NetCache>().RegisterNewNoticesListener(OnNewNotices);
		yield break;
	}

	public Type[] GetDependencies()
	{
		return new Type[1] { typeof(NetCache) };
	}

	public void Shutdown()
	{
	}

	public static GenericRewardChestNoticeManager Get()
	{
		return ServiceManager.Get<GenericRewardChestNoticeManager>();
	}

	public HashSet<long> GetReadyGenericRewardChestNotices()
	{
		HashSet<long> readyNoticeIds = new HashSet<long>();
		foreach (GenericRewardChestAssetStatus assetStatus in m_mapOfRewardChestAssetIdToNoticeIds.Values)
		{
			if (assetStatus.m_isReady)
			{
				readyNoticeIds.UnionWith(assetStatus.m_noticeIds);
			}
		}
		return readyNoticeIds;
	}

	public bool RegisterRewardsUpdatedListener(GenericRewardUpdatedCallback callback, object userData = null)
	{
		if (callback == null)
		{
			return false;
		}
		GenericRewardChestUpdatedListener listener = new GenericRewardChestUpdatedListener();
		listener.SetCallback(callback);
		listener.SetUserData(userData);
		if (m_genericRewardUpdatedListeners.Contains(listener))
		{
			return false;
		}
		m_genericRewardUpdatedListeners.Add(listener);
		return true;
	}

	public bool RemoveRewardsUpdatedListener(GenericRewardUpdatedCallback callback)
	{
		return RemoveRewardsUpdatedListener(callback, null);
	}

	public bool RemoveRewardsUpdatedListener(GenericRewardUpdatedCallback callback, object userData)
	{
		if (callback == null)
		{
			return false;
		}
		GenericRewardChestUpdatedListener listener = new GenericRewardChestUpdatedListener();
		listener.SetCallback(callback);
		listener.SetUserData(userData);
		if (!m_genericRewardUpdatedListeners.Contains(listener))
		{
			return false;
		}
		m_genericRewardUpdatedListeners.Remove(listener);
		return true;
	}

	private void OnNewNotices(List<NetCache.ProfileNotice> newNotices, bool isInitialNoticeList)
	{
		if (NetCache.Get().GetNetObject<NetCache.NetCacheProfileNotices>() == null)
		{
			return;
		}
		bool requestInProgress = false;
		foreach (NetCache.ProfileNotice notice in newNotices)
		{
			if (NetCache.ProfileNotice.NoticeType.GENERIC_REWARD_CHEST != notice.Type)
			{
				continue;
			}
			NetCache.ProfileNoticeGenericRewardChest genericRewardChestNotice = notice as NetCache.ProfileNoticeGenericRewardChest;
			if (genericRewardChestNotice == null || genericRewardChestNotice.RewardChestHash == null || genericRewardChestNotice.RewardChestByteSize == 0)
			{
				Debug.LogError($"ProfileNoticeGenericRewardChest with asset id [{genericRewardChestNotice.RewardChestAssetId}] with no hash or a byte size of 0. Unable to request reward chest record information.");
				if (GameDbf.RewardChest.HasRecord(genericRewardChestNotice.RewardChestAssetId))
				{
					Debug.LogWarning($"Local RewardChest record found for asset id {genericRewardChestNotice.RewardChestAssetId}. Using cached value.");
					InformListenersThatNoticeIsReady(notice.NoticeID);
				}
				continue;
			}
			AssetRecordInfo info = new AssetRecordInfo();
			info.Asset = new AssetKey();
			info.Asset.Type = AssetType.ASSET_TYPE_REWARD_CHEST;
			info.Asset.AssetId = genericRewardChestNotice.RewardChestAssetId;
			info.RecordByteSize = genericRewardChestNotice.RewardChestByteSize;
			info.RecordHash = genericRewardChestNotice.RewardChestHash;
			if (!m_mapOfRewardChestAssetIdToNoticeIds.ContainsKey(genericRewardChestNotice.RewardChestAssetId))
			{
				m_mapOfRewardChestAssetIdToNoticeIds[genericRewardChestNotice.RewardChestAssetId] = new GenericRewardChestAssetStatus();
			}
			m_mapOfRewardChestAssetIdToNoticeIds[genericRewardChestNotice.RewardChestAssetId].m_noticeIds.Add(notice.NoticeID);
			if (!requestInProgress)
			{
				requestInProgress = DownloadableDbfCache.Get().IsAssetRequestInProgress(genericRewardChestNotice.RewardChestAssetId, AssetType.ASSET_TYPE_REWARD_CHEST);
			}
			DownloadableDbfCache.Get().LoadCachedAssets(!requestInProgress, OnRewardChestDownloadableDbfAssetsLoaded, info);
		}
	}

	private void OnRewardChestDownloadableDbfAssetsLoaded(AssetKey requestedKey, ErrorCode code, byte[] assetBytes)
	{
		if (code != 0)
		{
			Debug.LogError($"Unable to get reward chest asset information for Reward Chest ID: {requestedKey.AssetId}, ErrorCode: {code}");
			return;
		}
		GenericRewardChestAssetStatus genericRewardChestAssetStatus = m_mapOfRewardChestAssetIdToNoticeIds[requestedKey.AssetId];
		genericRewardChestAssetStatus.m_isReady = true;
		foreach (long noticeId in genericRewardChestAssetStatus.m_noticeIds)
		{
			InformListenersThatNoticeIsReady(noticeId);
		}
	}

	private void InformListenersThatNoticeIsReady(long noticeId)
	{
		GenericRewardChestUpdatedListener[] array = m_genericRewardUpdatedListeners.ToArray();
		for (int i = 0; i < array.Length; i++)
		{
			array[i].Fire(noticeId);
		}
	}
}
