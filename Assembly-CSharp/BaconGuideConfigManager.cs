using System.Collections.Generic;
using Blizzard.T5.Core;
using UnityEngine;

[CustomEditClass]
public class BaconGuideConfigManager : MonoBehaviour
{
	[CustomEditField(Sections = "Guide Config")]
	public List<BaconGuideConfig> m_GuideConfigs;

	private Map<string, BaconGuideConfig> m_GuideConfigLookup;

	public BaconGuideConfig GetGuideConfigForSkinCardId(string skinCardId)
	{
		InitGuideLookup();
		if (m_GuideConfigLookup == null || !m_GuideConfigLookup.ContainsKey(skinCardId))
		{
			Log.All.PrintError("BaconGuideConfigManager: no matching config for skin ID: {0}", skinCardId);
			return new GameObject().AddComponent<BaconGuideConfig>();
		}
		return m_GuideConfigLookup[skinCardId];
	}

	private void InitGuideLookup()
	{
		if (m_GuideConfigs == null)
		{
			Log.All.PrintError("BaconGuideConfigManager: no GuideConfigs set");
		}
		else
		{
			if (m_GuideConfigLookup != null)
			{
				return;
			}
			m_GuideConfigLookup = new Map<string, BaconGuideConfig>();
			foreach (BaconGuideConfig guideConfig in m_GuideConfigs)
			{
				if (guideConfig == null || guideConfig.m_GuideCardId == null)
				{
					Log.All.PrintError("BaconGuideConfigManager: invalid config in list");
				}
				else
				{
					m_GuideConfigLookup[guideConfig.m_GuideCardId] = guideConfig;
				}
			}
		}
	}
}
