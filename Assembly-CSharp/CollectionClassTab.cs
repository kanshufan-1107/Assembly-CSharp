using UnityEngine;

public class CollectionClassTab : BookTab
{
	private CollectionTabInfo m_tabInfo;

	public GameObject m_persistentClassGlow;

	public GameObject m_spawnVFX;

	private bool m_shouldShowPersistentClassGlow;

	public CollectionTabInfo TabInfo => m_tabInfo;

	public bool ShouldShowPersistentClassGlow
	{
		get
		{
			return m_shouldShowPersistentClassGlow;
		}
		set
		{
			m_shouldShowPersistentClassGlow = value;
			if (m_persistentClassGlow != null)
			{
				m_persistentClassGlow.SetActive(value);
			}
		}
	}

	public void Init(TAG_CLASS classTag)
	{
		m_tabInfo = new CollectionTabInfo
		{
			tagClass = classTag
		};
		if (m_persistentClassGlow != null)
		{
			m_persistentClassGlow.SetActive(value: false);
		}
		Init();
	}

	protected override Vector2 GetTextureOffset()
	{
		if (CollectionPageManager.s_classTextureOffsets.ContainsKey(m_tabInfo.tagClass))
		{
			return CollectionPageManager.s_classTextureOffsets[m_tabInfo.tagClass];
		}
		Debug.LogWarning($"CollectionClassTab.GetTextureOffset(): No class texture offsets exist for class {TabInfo.tagClass}");
		return Vector2.zero;
	}

	public void PlaySpawnVFX()
	{
		if (m_spawnVFX != null)
		{
			PlayMakerFSM playMakerFSM = m_spawnVFX.GetComponentInChildren<PlayMakerFSM>();
			if (playMakerFSM != null)
			{
				playMakerFSM.SendEvent("Activate");
			}
		}
	}
}
