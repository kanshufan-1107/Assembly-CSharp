using System.Collections.Generic;
using UnityEngine;

public class HistoryTileInitInfo : HistoryItemInitInfo
{
	public HistoryInfoType m_type;

	public List<HistoryInfo> m_childInfos;

	public HistoryInfo m_ownerInfo;

	public Texture m_fatigueTexture;

	public Texture m_burnedCardsTexture;

	public Material m_fullTileMaterial;

	public Material m_halfTileMaterial;

	public bool m_dead;

	public bool m_burned;

	public bool m_isPoisonous;

	public bool m_isCriticalHit;

	public int m_splatAmount;

	public TAG_CARD_ALTERNATE_COST m_splatType;
}
