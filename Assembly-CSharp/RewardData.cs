using System.Collections.Generic;
using Hearthstone.Streaming;
using UnityEngine;

public abstract class RewardData
{
	private class PendingRewardData
	{
		public string m_assetPath;

		public PrefabCallback<GameObject> m_callback;

		public Reward.LoadRewardCallbackData m_callbackData;
	}

	private Reward.Type m_type;

	private NetCache.ProfileNotice.NoticeOrigin m_origin = NetCache.ProfileNotice.NoticeOrigin.UNKNOWN;

	private long m_originData;

	protected List<long> m_noticeIDs = new List<long>();

	private bool m_showQuestToast;

	private bool m_isDummyReward;

	private static readonly Queue<PendingRewardData> m_pendingRewardData = new Queue<PendingRewardData>();

	public Reward.Type RewardType => m_type;

	public NetCache.ProfileNotice.NoticeOrigin Origin => m_origin;

	public long OriginData => m_originData;

	public bool ShowQuestToast => m_showQuestToast;

	public bool IsDummyReward => m_isDummyReward;

	public string NameOverride { get; set; }

	public string DescriptionOverride { get; set; }

	public int? RewardChestAssetId { get; set; }

	public int? RewardChestBagNum { get; set; }

	public void LoadRewardObject(Reward.DelOnRewardLoaded callback)
	{
		LoadRewardObject(callback, null);
	}

	public void LoadRewardObject(Reward.DelOnRewardLoaded callback, object callbackData)
	{
		string assetPath = GetAssetPath();
		if (string.IsNullOrEmpty(assetPath))
		{
			Debug.LogError($"Reward.LoadRewardObject(): Do not know how to load reward object for {this}.");
			return;
		}
		Reward.LoadRewardCallbackData loadRewardCallbackData = new Reward.LoadRewardCallbackData
		{
			m_callback = callback,
			m_callbackData = callbackData
		};
		if (!GameDownloadManagerProvider.Get().IsReadyToPlay || !AssetLoader.Get().IsAssetAvailable(assetPath))
		{
			Debug.LogWarning($"Reward.LoadRewardObject(): Loading {assetPath} before downloaded. Waiting for initial download first.");
			PendingRewardData pendingData = new PendingRewardData
			{
				m_assetPath = assetPath,
				m_callback = OnRewardObjectLoaded,
				m_callbackData = loadRewardCallbackData
			};
			m_pendingRewardData.Enqueue(pendingData);
		}
		else
		{
			AssetLoader.Get().InstantiatePrefab(assetPath, OnRewardObjectLoaded, loadRewardCallbackData);
		}
	}

	public static void LoadPendingRewards()
	{
		foreach (PendingRewardData pendingRewardData in m_pendingRewardData)
		{
			AssetLoader.Get().InstantiatePrefab(pendingRewardData.m_assetPath, pendingRewardData.m_callback, pendingRewardData.m_callbackData);
		}
		m_pendingRewardData.Clear();
	}

	public void SetOrigin(NetCache.ProfileNotice.NoticeOrigin origin, long originData)
	{
		m_origin = origin;
		m_originData = originData;
	}

	public void AddNoticeID(long noticeID)
	{
		if (!m_noticeIDs.Contains(noticeID))
		{
			m_noticeIDs.Add(noticeID);
		}
	}

	public List<long> GetNoticeIDs()
	{
		return m_noticeIDs;
	}

	public bool HasNotices()
	{
		return m_noticeIDs.Count > 0;
	}

	public void AcknowledgeNotices()
	{
		long[] array = m_noticeIDs.ToArray();
		m_noticeIDs.Clear();
		long[] array2 = array;
		foreach (long noticeID in array2)
		{
			Network.Get().AckNotice(noticeID);
		}
	}

	public void MarkAsDummyReward()
	{
		m_isDummyReward = true;
	}

	protected RewardData(Reward.Type type, bool showQuestToast = false)
	{
		m_type = type;
		m_showQuestToast = showQuestToast;
	}

	protected abstract string GetAssetPath();

	private void OnRewardObjectLoaded(AssetReference assetRef, GameObject go, object callbackData)
	{
		if (go == null)
		{
			Debug.LogWarning($"Reward.OnRewardObjectLoaded() - game object is null assetRef={assetRef}");
			return;
		}
		Reward reward = go.GetComponent<Reward>();
		if (reward == null)
		{
			Debug.LogErrorFormat("Reward.OnRewardObjectLoaded() - loaded game object has no reward component assetRef={0}", assetRef);
			return;
		}
		go.transform.parent = SceneMgr.Get().SceneObject.transform;
		reward.SetData(this, updateVisuals: true);
		Reward.LoadRewardCallbackData loadRewardCallbackData = callbackData as Reward.LoadRewardCallbackData;
		reward.NotifyLoadedWhenReady(loadRewardCallbackData);
	}
}
