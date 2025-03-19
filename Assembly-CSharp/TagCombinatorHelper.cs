using System;
using System.Collections.Generic;

public class TagCombinatorHelper
{
	private List<string> m_tempQualityTags = new List<string>();

	private List<string> m_tempContentTags = new List<string>();

	public bool ForEachCombination(string[] inputTags, List<string> qualityTags, List<string> contentTags, Func<string, string, bool> action)
	{
		m_tempContentTags.Clear();
		m_tempQualityTags.Clear();
		UpdateUtils.ResizeListIfNeeded(m_tempQualityTags, qualityTags.Count);
		UpdateUtils.ResizeListIfNeeded(m_tempContentTags, contentTags.Count);
		foreach (string tag in inputTags)
		{
			if (qualityTags.Contains(tag))
			{
				m_tempQualityTags.Add(tag);
			}
			if (contentTags.Contains(tag))
			{
				m_tempContentTags.Add(tag);
			}
		}
		bool hasIterated = false;
		bool result = true;
		foreach (string quality in m_tempQualityTags)
		{
			foreach (string content in m_tempContentTags)
			{
				hasIterated = true;
				if (!action(quality, content))
				{
					result = false;
					break;
				}
			}
		}
		return hasIterated && result;
	}
}
