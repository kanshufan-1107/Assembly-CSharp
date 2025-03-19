using System;
using System.Collections.Generic;

[CustomEditClass]
public class ScenarioSpecificSoundDef : SoundDef, IMultipleRandomClipSoundDef
{
	[Serializable]
	[CustomEditClass]
	public class ScenarioClipPair
	{
		public ScenarioDbId m_ScenarioID;

		public List<RandomAudioClip> m_RandomClips;
	}

	public List<ScenarioClipPair> m_ScenarioSpecificRandomClips = new List<ScenarioClipPair>();

	public List<RandomAudioClip> GetRandomAudioClips()
	{
		ScenarioDbId currentScenario = ((GameMgr.Get() != null) ? ((ScenarioDbId)GameMgr.Get().GetMissionId()) : ScenarioDbId.MULTIPLAYER_1v1);
		ScenarioClipPair scenarioClipPair = m_ScenarioSpecificRandomClips.Find((ScenarioClipPair pair) => pair.m_ScenarioID == currentScenario);
		if (scenarioClipPair == null)
		{
			return m_RandomClips;
		}
		return scenarioClipPair.m_RandomClips;
	}
}
