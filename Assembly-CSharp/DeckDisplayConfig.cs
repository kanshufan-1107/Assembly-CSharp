using UnityEngine;

[CustomEditClass]
[CreateAssetMenu(fileName = "DeckDisplayConfigData", menuName = "ScriptableObjects/DeckDisplayConfig", order = 1)]
public class DeckDisplayConfig : ScriptableObject
{
	public Vector3 m_heroNameOffset = Vector3.zero;

	public Vector3 m_playButtonOffset = Vector3.zero;

	public Vector3 m_rankedPlayHeroBonePostionOffset = Vector3.zero;

	public Vector3 m_cardPlayHeroPowerBoneDownOffset = Vector3.zero;

	public Vector3 m_rankedTrayShownBoneOffset = Vector3.zero;

	public Vector3 m_deckPickerHeroPortraitClickableOffset = Vector3.zero;

	public float m_heroNameTextWidthOffset;

	public float m_heroNameTextHeightOffset;

	public bool m_heroNameParagraphWordWrapToggle;

	public bool m_deckPickerHeroPortraitClickableToggle;
}
