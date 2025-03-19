using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Blizzard.T5.Core.Utils;
using Hearthstone.Streaming;
using UnityEngine;

namespace Hearthstone.Core.Streaming;

public class TagIndicatorManager
{
	private Dictionary<string, int> m_lastQualityTags = new Dictionary<string, int>();

	private List<string> m_allQualityTags = new List<string>();

	private List<string> m_allContentTags = new List<string>();

	private List<string> m_allExpectedQualityTags = new List<string>();

	private string m_dataPath;

	private IIndicatorChecker m_indicatorChecker;

	private bool m_ready;

	private HashSet<string> m_availableBundles = new HashSet<string>();

	private IAssetManifest m_assetManifest;

	private TagCombinatorHelper m_tagCombinatorHelper;

	private List<string> m_tempIndicators = new List<string>();

	private Func<string, string, bool> m_isDownloadedDelegate;

	private bool m_isInitialized;

	private Coroutine m_resetAfterVersionStepCoroutine;

	public bool IsInitialized => m_isInitialized;

	private bool IsVersionStepCompleted
	{
		get
		{
			if (GameDownloadManagerProvider.Get() != null)
			{
				return GameDownloadManagerProvider.Get().IsVersionStepCompleted;
			}
			return false;
		}
	}

	public void Initialize(string dataPath)
	{
		m_dataPath = dataPath;
		m_indicatorChecker = new IndicatorChecker();
		m_tagCombinatorHelper = new TagCombinatorHelper();
		if (m_resetAfterVersionStepCoroutine == null)
		{
			m_resetAfterVersionStepCoroutine = Processor.RunCoroutine(ResetAfterVersionStep());
		}
		m_isInitialized = true;
	}

	public void Check()
	{
		Reset(forcibly: false);
		foreach (string key in m_lastQualityTags.Keys.ToList())
		{
			while (m_lastQualityTags[key] < m_allExpectedQualityTags.Count() - 1)
			{
				int nextQuality = m_lastQualityTags[key] + 1;
				string nextQualityTag = m_allExpectedQualityTags[nextQuality];
				if (!m_indicatorChecker.Exists(FindIndicators(new string[2] { key, nextQualityTag }, fullPath: true)))
				{
					break;
				}
				Log.Downloader.PrintInfo("Found the indicator for {0} of {1}", nextQualityTag, key);
				m_lastQualityTags[key] = nextQuality;
				RecordAvailBundles(new string[2] { key, nextQualityTag });
			}
		}
	}

	public bool IsReady(string[] tags)
	{
		bool ready = false;
		if (m_ready)
		{
			if (m_isDownloadedDelegate == null)
			{
				m_isDownloadedDelegate = IsAlreadyDownloaded;
			}
			ready = m_tagCombinatorHelper.ForEachCombination(tags, m_allQualityTags, m_allContentTags, m_isDownloadedDelegate);
		}
		else
		{
			ready = m_indicatorChecker.Exists(FindIndicators(tags, fullPath: true));
		}
		if (ready)
		{
			Log.Downloader.PrintInfo("ready = {0}, tags = {1}", ready, string.Join(" ", tags));
		}
		return ready;
	}

	public bool IsReady(string bundlename)
	{
		if (m_ready)
		{
			return m_availableBundles.Contains(bundlename);
		}
		return false;
	}

	public void ClearAllIndicators()
	{
		string[] tagIndicatorFiles;
		try
		{
			tagIndicatorFiles = Directory.GetFiles(m_dataPath, "tag_*");
		}
		catch (Exception ex)
		{
			Log.Downloader.PrintError("Failed to get the file list from {0}: {1}", m_dataPath, ex.Message);
			return;
		}
		string[] array = tagIndicatorFiles;
		foreach (string tagIndicator in array)
		{
			Log.Downloader.PrintInfo("Delete the indicator - " + tagIndicator);
			try
			{
				File.Delete(tagIndicator);
			}
			catch (Exception ex2)
			{
				Log.Downloader.PrintError("Failed to delete the indicator({0}): {1}", tagIndicator, ex2.Message);
			}
		}
		ClearTagValues();
	}

	public void DeleteIndicatorsForTags(string[] tags)
	{
		string[] array = FindIndicators(tags, fullPath: true);
		foreach (string tagIndicator in array)
		{
			try
			{
				File.Delete(tagIndicator);
				Log.Downloader.PrintInfo("Deleted the indicator : " + tagIndicator);
			}
			catch (Exception ex)
			{
				Log.Downloader.PrintError("Failed to delete the indicator : " + tagIndicator + " : Error = " + ex.Message);
			}
		}
		array = tags;
		foreach (string tag in array)
		{
			if (DownloadTags.GetContentTag(tag) != 0)
			{
				m_lastQualityTags[tag] = -1;
			}
		}
		Log.Downloader.PrintInfo("DeleteIndicatorsForTags complete. Deleted tag indicators for tags : " + string.Join(',', tags));
		Check();
	}

	public void RemoveAvailableBundle(string bundleName)
	{
		m_availableBundles.Remove(bundleName);
	}

	private void RecordAvailBundles(string[] availableTags)
	{
		Log.Downloader.PrintDebug(string.Format("RecordAvailBundles({0}) with '{1}'", Localization.GetLocale(), string.Join(" ", availableTags)));
		string[] bundles = GetAssetManifest().GetAllAssetBundleNames(Localization.GetLocale());
		List<string> tags = new List<string>();
		int i = 0;
		for (int iMax = bundles.Length; i < iMax; i++)
		{
			string b = bundles[i];
			bool validBundle = true;
			GetAssetManifest().GetTagsFromAssetBundle(b, tags);
			int j = 0;
			for (int jMax = tags.Count; j < jMax; j++)
			{
				if (!availableTags.Contains(tags[j]))
				{
					validBundle = false;
					break;
				}
			}
			if (validBundle)
			{
				if (File.Exists(AssetBundleInfo.GetAssetBundlePath(b)))
				{
					m_availableBundles.Add(b);
					continue;
				}
				Log.Downloader.PrintError("unavailable still(file not found): {0}", b);
			}
		}
	}

	private bool IsAlreadyDownloaded(string quality, string content)
	{
		Reset(forcibly: false);
		if (!m_ready)
		{
			return false;
		}
		if (!m_lastQualityTags.TryGetValue(content, out var lastQuality))
		{
			Log.Downloader.PrintInfo("Ignored the content key which does not exist: '{0}'", content);
			return false;
		}
		int inputQuality = m_allExpectedQualityTags.IndexOf(quality);
		if (inputQuality == -1)
		{
			Log.Downloader.PrintInfo("Ignored the quality key which does not exist: '{0}'", quality);
			return false;
		}
		return lastQuality >= inputQuality;
	}

	private IEnumerator ResetAfterVersionStep()
	{
		while (!IsVersionStepCompleted)
		{
			yield return null;
		}
		Log.Downloader.PrintDebug("TagIndicatorManager : ResetAfterVersionStep");
		Reset(forcibly: false);
		m_resetAfterVersionStepCoroutine = null;
	}

	protected void Reset(bool forcibly)
	{
		if (GetAssetManifest() == null || (m_ready && !forcibly) || !IsVersionStepCompleted)
		{
			return;
		}
		ClearTagValues();
		GetAssetManifest().GetAllTags(DownloadTags.GetTagGroupString(DownloadTags.TagGroup.Content), excludeOverridenTag: true).ForEach(delegate(string t)
		{
			m_lastQualityTags[t] = -1;
		});
		List<string> allTags = GetAssetManifest().GetAllTags(DownloadTags.GetTagGroupString(DownloadTags.TagGroup.Quality), excludeOverridenTag: true);
		UpdateUtils.ResizeListIfNeeded(minSize: allTags.Count, list: m_allExpectedQualityTags);
		m_allExpectedQualityTags.Add(DownloadTags.GetTagString(DownloadTags.Quality.Dbf));
		m_allExpectedQualityTags.Add(DownloadTags.GetTagString(DownloadTags.Quality.Strings));
		foreach (string tag in allTags)
		{
			if (!string.IsNullOrEmpty(tag) && !(tag == DownloadTags.GetTagString(DownloadTags.Quality.Essential)) && (!(tag == DownloadTags.GetTagString(DownloadTags.Quality.PortHigh)) || !PlatformSettings.ShouldFallbackToLowRes))
			{
				m_allExpectedQualityTags.Add(tag);
			}
		}
		List<string> allQualityTags = new List<string>();
		GetAssetManifest().GetTagsInTagGroup(DownloadTags.GetTagGroupString(DownloadTags.TagGroup.Quality), ref allQualityTags);
		int qualityTagsLength = allQualityTags.Count;
		UpdateUtils.ResizeListIfNeeded(m_allQualityTags, qualityTagsLength);
		m_allQualityTags.Add(DownloadTags.GetTagString(DownloadTags.Quality.Dbf));
		m_allQualityTags.Add(DownloadTags.GetTagString(DownloadTags.Quality.Strings));
		foreach (string tag2 in allQualityTags)
		{
			if (!m_allQualityTags.Contains(tag2) && tag2 != DownloadTags.GetTagString(DownloadTags.Quality.Essential))
			{
				m_allQualityTags.Add(tag2);
			}
		}
		List<string> contentTags = new List<string>();
		GetAssetManifest().GetTagsInTagGroup(DownloadTags.GetTagGroupString(DownloadTags.TagGroup.Content), ref contentTags);
		int contentTagsLength = contentTags.Count;
		UpdateUtils.ResizeListIfNeeded(m_allContentTags, contentTagsLength);
		foreach (string tag3 in contentTags)
		{
			if (!m_allContentTags.Contains(tag3))
			{
				m_allContentTags.Add(tag3);
			}
		}
		UpdateUtils.ResizeListIfNeeded(m_tempIndicators, contentTagsLength * qualityTagsLength);
		m_lastQualityTags.ForEach(delegate(KeyValuePair<string, int> p)
		{
			Log.Downloader.PrintDebug("{0} = {1}", p.Key, p.Value);
		});
		Log.Downloader.PrintDebug("Expected qualities = {0}", string.Join(" ", m_allExpectedQualityTags));
		Log.Downloader.PrintDebug("All qualities = {0}", string.Join(" ", m_allQualityTags));
		Log.Downloader.PrintDebug("All contents = {0}", string.Join(" ", m_allContentTags));
		m_allQualityTags.ForEach(delegate(string t)
		{
			Log.Downloader.PrintDebug("Quality Tag '" + t + "' overridden to '" + GetAssetManifest().ConvertToOverrideTag(t, DownloadTags.GetTagGroupString(DownloadTags.TagGroup.Quality)) + "'");
		});
		m_allContentTags.ForEach(delegate(string t)
		{
			Log.Downloader.PrintDebug("Content Tag '" + t + "' overridden to '" + GetAssetManifest().ConvertToOverrideTag(t, DownloadTags.GetTagGroupString(DownloadTags.TagGroup.Content)) + "'");
		});
		m_ready = true;
		Check();
	}

	private void ClearTagValues()
	{
		m_allExpectedQualityTags.Clear();
		m_lastQualityTags.Clear();
		m_availableBundles.Clear();
		m_allQualityTags.Clear();
		m_allContentTags.Clear();
		m_ready = false;
	}

	protected string[] FindIndicators(string[] tags, bool fullPath)
	{
		if (!m_ready)
		{
			string indicator = GetIndicatorName(tags);
			return new string[1] { fullPath ? Path.Combine(m_dataPath, indicator) : indicator };
		}
		m_tempIndicators.Clear();
		m_tagCombinatorHelper.ForEachCombination(tags, m_allQualityTags, m_allContentTags, delegate(string quality, string content)
		{
			string text = "tag_" + quality + "_" + content;
			m_tempIndicators.Add(fullPath ? Path.Combine(m_dataPath, text) : text);
			return true;
		});
		return m_tempIndicators.ToArray();
	}

	protected string GetIndicatorName(string[] tags)
	{
		string indicator = "tag";
		foreach (string tag in tags)
		{
			indicator = indicator + "_" + tag;
		}
		return indicator;
	}

	private IAssetManifest GetAssetManifest()
	{
		if (m_assetManifest == null)
		{
			m_assetManifest = AssetManifest.Get();
		}
		return m_assetManifest;
	}
}
