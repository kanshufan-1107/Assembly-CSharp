using System;
using UnityEngine;

[Serializable]
public class CornerReplacementSpellTableEntry
{
	[CustomEditField(SortPopupByName = true)]
	public CornerReplacementSpellType m_Type;

	public Texture m_TableTopTexture;

	public Texture m_FrameTexture;

	public Texture m_PlayAreaTexture;

	public Texture m_PlayAreaMaskTexture;

	[CustomEditField(Hide = true)]
	public Spell m_TopLeftSpell;

	[CustomEditField(T = EditType.SPELL)]
	public string m_TopLeftSpellPrefabName = "";

	[CustomEditField(Hide = true)]
	public Spell m_TopRightSpell;

	[CustomEditField(T = EditType.SPELL)]
	public string m_TopRightSpellPrefabName = "";

	[CustomEditField(Hide = true)]
	public Spell m_BottomLeftSpell;

	[CustomEditField(T = EditType.SPELL)]
	public string m_BottomLeftSpellPrefabName = "";

	[CustomEditField(Hide = true)]
	public Spell m_BottomRightSpell;

	[CustomEditField(T = EditType.SPELL)]
	public string m_BottomRightSpellPrefabName = "";

	[CustomEditField(T = EditType.ACTOR)]
	public string m_WeaponActorReplacement = "";

	[CustomEditField(T = EditType.ACTOR)]
	public string m_HeroPowerPlayActorReplacement = "";

	[CustomEditField(T = EditType.ACTOR)]
	public string m_HeroPowerGoldenPlayActorReplacement = "";

	[CustomEditField(T = EditType.ACTOR)]
	public string m_HeroPowerOpponentHandActorReplacement = "";

	[CustomEditField(T = EditType.ACTOR)]
	public string m_HeroPowerOpponentGoldenHandActorReplacement = "";

	[CustomEditField(T = EditType.ACTOR)]
	public string m_HeroPowerHandActorReplacement = "";

	[CustomEditField(T = EditType.ACTOR)]
	public string m_HeroPowerGoldenHandActorReplacement = "";

	public MusicPlaylistType m_MulliganMusic;
}
