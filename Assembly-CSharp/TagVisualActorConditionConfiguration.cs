using System;
using UnityEngine;

[Serializable]
public class TagVisualActorConditionConfiguration
{
	[Tooltip("Condition to evaluate")]
	[CustomEditField(Sections = "Condition Configuration")]
	public TagVisualActorCondition m_Condition;

	[Tooltip("Evaluate this condition opposite to the initial result")]
	[CustomEditField(Sections = "Condition Configuration")]
	public bool m_InvertCondition;

	[CustomEditField(Sections = "Condition Configuration", HidePredicate = "ShouldHideAllConditionParameters")]
	public TagVisualActorConditionParameters m_Parameters;
}
