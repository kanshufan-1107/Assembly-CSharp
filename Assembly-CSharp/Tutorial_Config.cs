using System.Collections.Generic;
using UnityEngine;

[CustomEditClass]
[CreateAssetMenu(fileName = "Data", menuName = "ScriptableObjects/TutorialConfig", order = 1)]
public class Tutorial_Config : ScriptableObject
{
	[CustomEditField(Sections = "Tutorial_Fight1")]
	public Vector3 m_PlayMinionTooltipPosition = Vector3.zero;

	[CustomEditField(Sections = "Tutorial_Fight1")]
	public Vector3 m_PlayMinionTooltipPositionMobile = Vector3.zero;

	[CustomEditField(Sections = "Tutorial_Fight1")]
	public Vector3 m_TutorialNotificationScale = Vector3.one;

	[CustomEditField(Sections = "Tutorial_Fight1")]
	public Vector3 m_TutorialNotificationScaleMobile = Vector3.one;

	[CustomEditField(Sections = "Tutorial_Fight1")]
	public List<int> m_endTurnButtonDelay;

	[CustomEditField(Sections = "Tutorial_Fight1")]
	public Vector3 m_attackOpponentInstructionAnimationRot = Vector3.zero;

	[CustomEditField(Sections = "Tutorial_Fight1")]
	public Vector3 m_DamageSpellTutorialPosition = Vector3.zero;

	[CustomEditField(Sections = "Tutorial_Fight1")]
	public Vector3 m_DamageSpellTutorialPositionMobile = Vector3.zero;

	[CustomEditField(Sections = "Tutorial_Fight1")]
	public Vector3 m_DamageRexxarWithMinionTooltipOffset = Vector3.zero;

	[CustomEditField(Sections = "Tutorial_Fight1")]
	public Vector3 m_DamageRexxarWithMinionTooltipOffsetMobile = Vector3.zero;

	[CustomEditField(Sections = "Tutorial_Fight1")]
	public Vector3 m_WindContionTooltipPositionOffset = Vector3.one;

	[CustomEditField(Sections = "Tutorial_Fight1")]
	public Vector3 m_WinContionTooltipPositionOffsetMobile = Vector3.one;

	[CustomEditField(Sections = "Tutorial_Fight1")]
	public Vector3 m_MinionTradeTooltipPositionOffset = Vector3.zero;

	[CustomEditField(Sections = "Tutorial_Fight1")]
	public Vector3 m_MinionTradeTooltipPositionOffsetMobile = Vector3.zero;

	[CustomEditField(Sections = "Tutorial_Fight1")]
	public Vector3 m_attackAndHealthLabelScaleHand = Vector3.one;

	[CustomEditField(Sections = "Tutorial_Fight1")]
	public Vector3 m_attackAndHealthLabelScaleHandMobile = Vector3.one;

	[CustomEditField(Sections = "Tutorial_Fight1")]
	public Vector3 m_attackAndHealthLabelScalePlay = Vector3.one;

	[CustomEditField(Sections = "Tutorial_Fight1")]
	public Vector3 m_attackAndHealthLabelScalePlayMobile = Vector3.one;

	[CustomEditField(Sections = "Tutorial_Fight1")]
	public Vector3 m_attackLabelPositionOffsetHand = Vector3.zero;

	[CustomEditField(Sections = "Tutorial_Fight1")]
	public Vector3 m_attackLabelOffsetHandMobile = Vector3.zero;

	[CustomEditField(Sections = "Tutorial_Fight1")]
	public Vector3 m_healthLabelPositionOffsetHand = Vector3.zero;

	[CustomEditField(Sections = "Tutorial_Fight1")]
	public Vector3 m_healthLabelPositionOffsetHandMobile = Vector3.zero;

	[CustomEditField(Sections = "Tutorial_Fight1")]
	public Vector3 m_attackLabelPositionOffsetOnPlay = Vector3.zero;

	[CustomEditField(Sections = "Tutorial_Fight1")]
	public Vector3 m_attackLabelPositionOffsetOnPlayMobile = Vector3.zero;

	[CustomEditField(Sections = "Tutorial_Fight1")]
	public Vector3 m_healthLabelPositionOffsetOnPlay = Vector3.zero;

	[CustomEditField(Sections = "Tutorial_Fight1")]
	public Vector3 m_healthLabelPositionOffsetOnPlayMobile = Vector3.zero;

	[CustomEditField(Sections = "Tutorial_Fight1")]
	public Vector3 m_historyHelpNotificationPosition = Vector3.zero;

	[CustomEditField(Sections = "Tutorial_Fight1")]
	public Vector3 m_historyHelpNotificationPositionMobile = Vector3.zero;

	[CustomEditField(Sections = "Tutorial_Fight1")]
	public Vector3 m_handBouncingArrowOffsetMobile = Vector3.zero;

	[CustomEditField(Sections = "Tutorial_Fight1")]
	public float m_showArrowOnMiniHandDelay = 1f;

	[CustomEditField(Sections = "Tutorial_Fight1")]
	public float m_PlayerTurn1Delay;

	[CustomEditField(Sections = "Tutorial_Fight1")]
	public float m_PlayerTurn2Delay;

	[CustomEditField(Sections = "Tutorial_Fight1")]
	public float m_PlayerTurn3Delay;

	[CustomEditField(Sections = "Tutorial_Fight1")]
	public float m_PlayerTurn4Delay;

	[CustomEditField(Sections = "Tutorial_Fight1")]
	public float m_PlayerTurn5Delay;

	[CustomEditField(Sections = "Tutorial_Fight2")]
	public Vector3 m_HeroPowerTutorialPositionOffset = Vector3.zero;

	[CustomEditField(Sections = "Tutorial_Fight2")]
	public Vector3 m_HeroPowerTutorialPositionOffsetMobile = Vector3.zero;

	[CustomEditField(Sections = "Tutorial_Fight2")]
	public Vector3 m_endTurnWarningPositionOffset = Vector3.zero;

	[CustomEditField(Sections = "Tutorial_Fight2")]
	public Vector3 m_SpellIgnoreTauntTutorialPosition = Vector3.zero;

	[CustomEditField(Sections = "Tutorial_Fight2")]
	public Vector3 m_SpellIgnoreTauntTutorialPositionMobile = Vector3.zero;

	[CustomEditField(Sections = "Tutorial_Fight2")]
	public List<int> m_endTurnButtonDelayFight2;

	[CustomEditField(Sections = "Tutorial_Fight2")]
	public List<int> m_playMinionTooltipDelay;

	[CustomEditField(Sections = "Tutorial_AllFights")]
	public Vector3 m_EndTurnButtonArrowOffset = Vector3.zero;
}
