using System;
using UnityEngine;

[Serializable]
public class TagVisualActionConfiguration
{
	[Tooltip("Action to perform")]
	public TagVisualActorFunction m_Action;

	[CustomEditField(SortPopupByName = true, HidePredicate = "ShouldHideSpellStateActionParameters")]
	[Tooltip("Required for ACTIVATE_SPELL_STATE")]
	public SpellType m_SpellType;

	[CustomEditField(HidePredicate = "ShouldHideSpellStateActionParameters")]
	[Tooltip("Required for ACTIVATE_SPELL_STATE")]
	public SpellStateType m_SpellState;

	[Tooltip("Required for ACTIVATE_STATE_FUNCTION/DEACTIVATE_STATE_FUNCTION")]
	[CustomEditField(SortPopupByName = true, HidePredicate = "ShouldHideStateFunctionActionParameters")]
	public TagVisualActorStateFunction m_StateFunctionParameters;

	[CustomEditField(T = EditType.SOUND_PREFAB, HidePredicate = "ShouldHideSoundActionParameters")]
	[Tooltip("Required for PLAY_SOUND_PREFAB")]
	public string m_PlaySoundPrefabParameters;

	[Tooltip("Some actions may only need to be executed under certain conditions (defaults to ALWAYS)")]
	public TagVisualActorConditionConfiguration m_Condition;
}
