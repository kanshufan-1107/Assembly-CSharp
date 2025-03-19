using System;
using System.Collections.Generic;
using Blizzard.T5.Core;
using PegasusGame;

public class TagMap
{
	private Dictionary<int, int> m_values;

	public TagMap()
	{
		m_values = new Dictionary<int, int>();
	}

	public TagMap(int size)
	{
		m_values = new Dictionary<int, int>(size);
	}

	public void SetTag(int tag, int tagValue)
	{
		m_values[tag] = tagValue;
	}

	public void SetTag(GAME_TAG tag, int tagValue)
	{
		SetTag((int)tag, tagValue);
	}

	public void SetTags(Dictionary<int, int> tagMap)
	{
		foreach (KeyValuePair<int, int> pair in tagMap)
		{
			SetTag(pair.Key, pair.Value);
		}
	}

	public void SetTags(Map<GAME_TAG, int> tagMap)
	{
		foreach (KeyValuePair<GAME_TAG, int> pair in tagMap)
		{
			SetTag(pair.Key, pair.Value);
		}
	}

	public void SetTags(List<Network.Entity.Tag> tags)
	{
		foreach (Network.Entity.Tag tag in tags)
		{
			SetTag(tag.Name, tag.Value);
		}
	}

	public void SetTags(List<Tag> tags)
	{
		foreach (Tag tag in tags)
		{
			SetTag(tag.Name, tag.Value);
		}
	}

	public Dictionary<int, int> GetMap()
	{
		return m_values;
	}

	public int GetTag(int tag)
	{
		int tagValue = 0;
		m_values.TryGetValue(tag, out tagValue);
		return tagValue;
	}

	public int GetTag(GAME_TAG enumTag)
	{
		int tag = Convert.ToInt32(enumTag);
		return GetTag(tag);
	}

	public void Replace(TagMap tags)
	{
		Clear();
		SetTags(tags.m_values);
	}

	public void Clear()
	{
		m_values = new Dictionary<int, int>();
	}

	public TagDeltaList CreateDeltas(List<Network.Entity.Tag> comp)
	{
		TagDeltaList deltaList = new TagDeltaList();
		foreach (Network.Entity.Tag item in comp)
		{
			int tag = item.Name;
			int oldValue = 0;
			m_values.TryGetValue(tag, out oldValue);
			int newValue = item.Value;
			if (oldValue != newValue)
			{
				deltaList.Add(tag, oldValue, newValue);
			}
		}
		return deltaList;
	}
}
