using System;
using UnityEngine;

[Serializable]
public class TagVisualActorConditionParameters
{
	[CustomEditField(SortPopupByName = true, Label = "Tag to Compare", HidePredicate = "ShouldHideTagValueParameters", HidePredicateInParent = true)]
	[Tooltip("Required for DOES_TAG_HAVE_VALUE")]
	public GAME_TAG m_Tag;

	[CustomEditField(Label = "Tag Comparison Operator", HidePredicate = "ShouldHideTagValueParameters", HidePredicateInParent = true)]
	[Tooltip("Required for DOES_TAG_HAVE_VALUE")]
	public TagVisualActorConditionComparisonOperator m_ComparisonOperator;

	[CustomEditField(Label = "Tag Value to Compare", HidePredicate = "ShouldHideTagValueParameters", HidePredicateInParent = true)]
	[Tooltip("Required for DOES_TAG_HAVE_VALUE")]
	public int m_Value;

	[CustomEditField(Label = "Tag Owner Entity", HidePredicate = "ShouldHideTagValueParameters", HidePredicateInParent = true)]
	[Tooltip("Required for DOES_TAG_HAVE_VALUE")]
	public TagVisualActorConditionEntity m_TagComparisonEntity;

	[CustomEditField(SortPopupByName = true, HidePredicate = "ShouldHideSpellStateParameters", HidePredicateInParent = true)]
	[Tooltip("Required for DOES_SPELL_HAVE_STATE")]
	public SpellType m_SpellType;

	[Tooltip("Required for DOES_SPELL_HAVE_STATE")]
	[CustomEditField(HidePredicate = "ShouldHideSpellStateParameters", HidePredicateInParent = true)]
	public SpellStateType m_SpellState;

	[CustomEditField(HidePredicate = "ShouldHideCompoundConditionParameters", HidePredicateInParent = true)]
	[Tooltip("Required for AND/OR")]
	public TagVisualActorCondition m_ConditionLHS;

	[CustomEditField(HidePredicate = "ShouldHideCompoundConditionParameters", HidePredicateInParent = true)]
	[Tooltip("Evaluate this condition opposite to the initial result")]
	public bool m_InvertConditionLHS;

	[CustomEditField(SortPopupByName = true, Label = "Tag to Compare LHS", HidePredicate = "ShouldHideTagValueParametersLHS")]
	[Tooltip("Required for AND/OR + DOES_TAG_HAVE_VALUE")]
	public GAME_TAG m_TagLHS;

	[CustomEditField(Label = "Tag Comparison Operator LHS", HidePredicate = "ShouldHideTagValueParametersLHS")]
	[Tooltip("Required for AND/OR + DOES_TAG_HAVE_VALUE")]
	public TagVisualActorConditionComparisonOperator m_ComparisonOperatorLHS;

	[Tooltip("Required for AND/OR + DOES_TAG_HAVE_VALUE")]
	[CustomEditField(Label = "Tag Value to Compare LHS", HidePredicate = "ShouldHideTagValueParametersLHS")]
	public int m_ValueLHS;

	[Tooltip("Required for AND/OR + DOES_TAG_HAVE_VALUE")]
	[CustomEditField(Label = "Tag Owner Entity LHS", HidePredicate = "ShouldHideTagValueParametersLHS")]
	public TagVisualActorConditionEntity m_TagComparisonEntityLHS;

	[Tooltip("Required for AND/OR + DOES_SPELL_HAVE_STATE")]
	[CustomEditField(SortPopupByName = true, HidePredicate = "ShouldHideSpellStateParametersLHS")]
	public SpellType m_SpellTypeLHS;

	[Tooltip("Required for AND/OR + DOES_SPELL_HAVE_STATE")]
	[CustomEditField(HidePredicate = "ShouldHideSpellStateParametersLHS")]
	public SpellStateType m_SpellStateLHS;

	[CustomEditField(HidePredicate = "ShouldHideCompoundConditionParameters", HidePredicateInParent = true)]
	[Tooltip("Required for AND/OR")]
	public TagVisualActorCondition m_ConditionRHS;

	[CustomEditField(HidePredicate = "ShouldHideCompoundConditionParameters", HidePredicateInParent = true)]
	[Tooltip("Evaluate this condition opposite to the initial result")]
	public bool m_InvertConditionRHS;

	[CustomEditField(SortPopupByName = true, Label = "Tag to Compare RHS", HidePredicate = "ShouldHideTagValueParametersRHS")]
	[Tooltip("Required for AND/OR + DOES_TAG_HAVE_VALUE")]
	public GAME_TAG m_TagRHS;

	[CustomEditField(Label = "Tag Comparison Operator RHS", HidePredicate = "ShouldHideTagValueParametersRHS")]
	[Tooltip("Required for AND/OR + DOES_TAG_HAVE_VALUE")]
	public TagVisualActorConditionComparisonOperator m_ComparisonOperatorRHS;

	[Tooltip("Required for AND/OR + DOES_TAG_HAVE_VALUE")]
	[CustomEditField(Label = "Tag Value to Compare RHS", HidePredicate = "ShouldHideTagValueParametersRHS")]
	public int m_ValueRHS;

	[CustomEditField(Label = "Tag Owner Entity RHS", HidePredicate = "ShouldHideTagValueParametersRHS")]
	[Tooltip("Required for AND/OR + DOES_TAG_HAVE_VALUE")]
	public TagVisualActorConditionEntity m_TagComparisonEntityRHS;

	[Tooltip("Required for AND/OR + DOES_SPELL_HAVE_STATE")]
	[CustomEditField(SortPopupByName = true, HidePredicate = "ShouldHideSpellStateParametersRHS")]
	public SpellType m_SpellTypeRHS;

	[CustomEditField(HidePredicate = "ShouldHideSpellStateParametersRHS")]
	[Tooltip("Required for AND/OR + DOES_SPELL_HAVE_STATE")]
	public SpellStateType m_SpellStateRHS;
}
