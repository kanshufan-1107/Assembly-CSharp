using System;
using UnityEngine;

[Serializable]
public class TagVisualActorConditionConfiguration
{
	[CustomEditField(Sections = "Condition Configuration")]
	[Tooltip("Condition to evaluate")]
	public TagVisualActorCondition m_Condition;

	[CustomEditField(Sections = "Condition Configuration")]
	[Tooltip("Evaluate this condition opposite to the initial result")]
	public bool m_InvertCondition;

	[CustomEditField(Sections = "Condition Configuration", HidePredicate = "ShouldHideAllConditionParameters")]
	public TagVisualActorConditionParameters m_Parameters;
}
