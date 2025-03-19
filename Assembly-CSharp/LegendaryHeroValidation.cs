using UnityEngine;

public class LegendaryHeroValidation : MonoBehaviour
{
	public enum ValidationType
	{
		CardDefinition,
		HeroPortrait,
		CollectionManager
	}

	public enum VariantType
	{
		Base,
		PC,
		Phone
	}

	public ValidationType m_testToRun;

	public VariantType m_variant;
}
