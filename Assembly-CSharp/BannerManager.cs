using System;
using System.Collections.Generic;
using Blizzard.T5.Configuration;
using UnityEngine;

public class BannerManager
{
	public delegate void DelOnCloseBanner();

	private static BannerManager s_instance;

	private bool m_bannerWasAcknowledged;

	private List<int> m_seenBanners = new List<int>();

	private bool m_isShowing;

	private VarKey m_bannerIdOverride = new VarKey("Events.BannerIdOverride");

	public bool IsShowing => m_isShowing;

	public static BannerManager Get()
	{
		if (s_instance == null)
		{
			s_instance = new BannerManager();
		}
		return s_instance;
	}

	private int GetOutstandingDisplayBannerId()
	{
		int bannerIdOverride = m_bannerIdOverride.GetInt(0);
		if (bannerIdOverride != 0)
		{
			return bannerIdOverride;
		}
		return NetCache.Get().GetNetObject<NetCache.NetCacheDisplayBanner>()?.Id ?? 0;
	}

	private bool AcknowledgeBanner(int banner)
	{
		m_seenBanners.Add(banner);
		if (banner != GetOutstandingDisplayBannerId() || m_bannerWasAcknowledged)
		{
			return false;
		}
		m_bannerWasAcknowledged = true;
		NetCache.NetCacheDisplayBanner netDisplayBanner = NetCache.Get().GetNetObject<NetCache.NetCacheDisplayBanner>();
		if (netDisplayBanner != null)
		{
			netDisplayBanner.Id = banner;
			NetCache.Get().NetCacheChanged<NetCache.NetCacheDisplayBanner>();
		}
		Network.Get().AcknowledgeBanner(banner);
		return true;
	}

	public void AutoAcknowledgeOutstandingBanner()
	{
		int bannerID = GetOutstandingDisplayBannerId();
		if (bannerID != 0)
		{
			AcknowledgeBanner(bannerID);
		}
	}

	public bool ShowOutstandingBannerEvent(DelOnCloseBanner callback = null)
	{
		int bannerID = GetOutstandingDisplayBannerId();
		if (bannerID == 0)
		{
			return false;
		}
		if (!Options.Get().GetBool(Option.HAS_SEEN_HUB, defaultVal: false))
		{
			return false;
		}
		if (m_seenBanners.Contains(bannerID))
		{
			return false;
		}
		if (ReturningPlayerMgr.Get().SuppressOldPopups)
		{
			AcknowledgeBanner(bannerID);
			return false;
		}
		if (ShowBanner(bannerID, callback))
		{
			AcknowledgeBanner(bannerID);
			return true;
		}
		return false;
	}

	public bool ShowBanner(string prefabAssetPath, string headerText, string text, DelOnCloseBanner callback = null, Action<BannerPopup> onCreateCallback = null)
	{
		BannerPopup popup = GameUtils.LoadGameObjectWithComponent<BannerPopup>(prefabAssetPath);
		if (popup == null)
		{
			return false;
		}
		onCreateCallback?.Invoke(popup);
		m_isShowing = true;
		popup.Show(headerText, text, delegate
		{
			OnBannerClose();
			if (callback != null)
			{
				callback();
			}
		});
		return true;
	}

	public bool ShowBanner(int bannerID, DelOnCloseBanner callback = null)
	{
		if (bannerID == 0)
		{
			return false;
		}
		BannerDbfRecord rec = GameDbf.Banner.GetRecord(bannerID);
		string prefabAssetPath = rec?.Prefab;
		if (rec == null || prefabAssetPath == null)
		{
			Debug.LogWarning($"No banner defined for bannerID={bannerID}");
			return false;
		}
		return ShowBanner(prefabAssetPath, rec.HeaderText, rec.Text, callback);
	}

	public void Cheat_ClearSeenBannersNewerThan(int bannerId)
	{
		m_seenBanners.RemoveAll((int i) => i >= bannerId);
	}

	public void Cheat_ClearSeenBanners()
	{
		m_seenBanners.Clear();
	}

	private BannerManager()
	{
	}

	private void OnBannerClose()
	{
		m_isShowing = false;
	}
}
