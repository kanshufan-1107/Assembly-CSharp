using System;
using System.Collections.Generic;
using Hearthstone.Core.Streaming;
using UnityEngine;

public class ScriptableAssetTagsMetadata : ScriptableObject
{
	[SerializeField]
	private List<string> m_tags = new List<string>();

	[SerializeField]
	private List<string> m_tagGroups = new List<string>();

	[SerializeField]
	private List<int> m_tagIdToTagGroupId = new List<int>();

	[SerializeField]
	private List<int> m_overrideId = new List<int>();

	private int m_tagGroupId;

	private List<string> m_qualityTagsInGroup;

	private List<string> m_contentTagsInGroup;

	public void Clear()
	{
		m_tags.Clear();
		m_tagGroups.Clear();
		m_tagIdToTagGroupId.Clear();
		m_overrideId.Clear();
	}

	public void AddTag(string tag, string tagGroup, string overrideTag)
	{
		if (!m_tags.Contains(tag))
		{
			int tagGroupId = m_tagGroups.IndexOf(tagGroup);
			if (tagGroupId == -1)
			{
				m_tagGroups.Add(tagGroup);
				tagGroupId = m_tagGroups.Count - 1;
			}
			m_tags.Add(tag);
			m_tagIdToTagGroupId.Add(tagGroupId);
			int overrideTagId = m_tags.IndexOf(overrideTag);
			if (overrideTagId == -1)
			{
				throw new Exception($"The override tag '{overrideTag}' must added before tag '{tag}'.");
			}
			m_overrideId.Add(overrideTagId);
		}
	}

	public string[] GetTagGroups()
	{
		return m_tagGroups.ToArray();
	}

	public void GetTagsInTagGroup(string tagGroup, ref List<string> tags)
	{
		GetTagsInTagGroup(m_tagGroups.IndexOf(tagGroup), ref tags);
	}

	public void GetTagsInTagGroup(int tagGroupId, ref List<string> tags)
	{
		tags.Clear();
		if (tagGroupId == -1)
		{
			return;
		}
		int i = 0;
		for (int iMax = m_tagIdToTagGroupId.Count; i < iMax; i++)
		{
			if (tagGroupId == m_tagIdToTagGroupId[i])
			{
				tags.Add(m_tags[i]);
			}
		}
	}

	public string ConvertToOverrideTag(string tag, string tagGroup)
	{
		return ConvertToOverrideTag(tag, m_tagGroups.IndexOf(tagGroup));
	}

	public string ConvertToOverrideTag(string tag, int tagGroupId)
	{
		if (tagGroupId == -1)
		{
			return tag;
		}
		int tagId = m_tags.IndexOf(tag);
		if (tagId == -1)
		{
			return tag;
		}
		return m_tags[m_overrideId[tagId]];
	}

	public string GetTagGroupForTag(string tag)
	{
		int tagId = m_tags.IndexOf(tag);
		if (tagId >= 0)
		{
			return m_tagGroups[m_tagIdToTagGroupId[tagId]];
		}
		return string.Empty;
	}

	public void GetTagsFromAssetBundle(string assetBundleName, List<string> tagList)
	{
		tagList.Clear();
		if (m_qualityTagsInGroup == null)
		{
			m_tagGroupId = m_tagGroups.IndexOf(DownloadTags.GetTagGroupString(DownloadTags.TagGroup.Quality));
			m_qualityTagsInGroup = new List<string>();
			GetTagsInTagGroup(m_tagGroupId, ref m_qualityTagsInGroup);
		}
		int i = 0;
		for (int iMax = m_qualityTagsInGroup.Count; i < iMax; i++)
		{
			string tag = m_qualityTagsInGroup[i];
			int index = assetBundleName.IndexOf(tag, StringComparison.Ordinal);
			if (index >= 0 && (index == 0 || assetBundleName[index - 1] == '_') && index + tag.Length <= assetBundleName.Length && assetBundleName[index + tag.Length] == '_')
			{
				tagList.Add(ConvertToOverrideTag(tag, m_tagGroupId));
			}
		}
		if (m_contentTagsInGroup == null)
		{
			m_contentTagsInGroup = new List<string>();
			GetTagsInTagGroup(DownloadTags.GetTagGroupString(DownloadTags.TagGroup.Content), ref m_contentTagsInGroup);
		}
		int j = 0;
		for (int iMax2 = m_contentTagsInGroup.Count; j < iMax2; j++)
		{
			string tag2 = m_contentTagsInGroup[j];
			int index2 = assetBundleName.IndexOf(tag2, StringComparison.Ordinal);
			if (index2 >= 0 && (index2 == 0 || assetBundleName[index2 - 1] == '_') && index2 + tag2.Length <= assetBundleName.Length && assetBundleName[index2 + tag2.Length] == '_')
			{
				tagList.Add(ConvertToOverrideTag(tag2, m_tagGroupId));
			}
		}
	}

	public List<string> GetAllTags(string tagGroup, bool excludeOverridenTag)
	{
		List<string> allTags = new List<string>();
		GetTagsInTagGroup(tagGroup, ref allTags);
		List<string> tagList = new List<string>();
		foreach (string tag in allTags)
		{
			if (!excludeOverridenTag || m_overrideId[m_tags.IndexOf(tag)] == m_tags.IndexOf(tag))
			{
				tagList.Add(tag);
			}
		}
		return tagList;
	}
}
