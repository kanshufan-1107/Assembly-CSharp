using System;
using UnityEngine;

public class BigCardBoneLayout : MonoBehaviour
{
	[Serializable]
	public class ScaleSettings
	{
		[Tooltip("The scale this should appear when moused over while in play.")]
		[Range(0.1f, 3f)]
		public float m_BigCardScale_Self = 1f;

		[Tooltip("The scale of the enchantment banner relative to the big card. Scaling the BigCardScale_Self parameter impacts this too. That parameter will scale the enchantment banner automatically with it. This one will scale the enchantment banner independent of the parent card.")]
		[Range(0.1f, 3f)]
		public float m_EnchantmentBannerScale = 1f;

		[Tooltip("The scale factor used to scale the Big Card size based on the number of enchants on Mobile.")]
		[Range(0.01f, 0.1f)]
		public float m_BigCardScaleFactorForEnchantments = 0.07f;

		[Tooltip("If a secondary card is shown and a minion, use this scale value.")]
		[Range(0.1f, 3f)]
		public float m_BigCardScale_Minion = 1f;

		[Tooltip("If a secondary card is shown and a lettuce ability, use this scale value.")]
		[Range(0.1f, 3f)]
		public float m_BigCardScale_LettuceAbility = 1f;

		[Range(0.1f, 3f)]
		[Tooltip("If a secondary card is shown and a hero, use this scale value.")]
		public float m_BigCardScale_Hero = 1f;

		[Tooltip("If a secondary card is shown and a spell, use this scale value.")]
		[Range(0.1f, 3f)]
		public float m_BigCardScale_Spell = 1f;

		[Tooltip("If a secondary card is shown and a weapon, use this scale value.")]
		[Range(0.1f, 3f)]
		public float m_BigCardScale_Weapon = 1f;

		[Tooltip("If a secondary card is shown and a hero power, use this scale value.")]
		[Range(0.1f, 3f)]
		public float m_BigCardScale_HeroPower = 1f;

		[Range(0.1f, 3f)]
		[Tooltip("If a secondary card is shown and a location, use this scale value.")]
		public float m_BigCardScale_Location = 1f;

		[Tooltip("If a secondary card is shown and a tavern spell, use this scale value.")]
		[Range(0.1f, 3f)]
		public float m_BigCardScale_TavernSpell = 1f;

		[Tooltip("If a secondary card is shown and a battlegrounds trinket, use this scale value.")]
		[Range(0.1f, 3f)]
		public float m_BigCardScale_BaconTrinket = 1f;

		[Range(0.1f, 3f)]
		[Tooltip("If a secondary card is shown and a battlegrounds trinket from hero power, use this scale value.")]
		public float m_BigCardScale_BaconTrinketHeropower = 1f;

		[Tooltip("If a secondary card is shown and a battlegrounds anomaly, use this scale value.")]
		[Range(0.1f, 3f)]
		public float m_BigCardScale_BaconAnomaly = 1f;

		[Range(0.1f, 3f)]
		[Tooltip("If we want to increase the size of the tooltip attached to a big card, use this scale value.")]
		public float m_BigCardScale_Tooltip = 1f;
	}

	public GameObject m_OuterLeftBone;

	public GameObject m_InnerLeftBone;

	public GameObject m_InnerRightBone;

	public GameObject m_OuterRightBone;

	public ScaleSettings m_scaleSettings = new ScaleSettings();

	private void Awake()
	{
		if (Application.isEditor)
		{
			ValidatePrefabSettings(ref m_scaleSettings.m_BigCardScale_Self, "m_BigCardScale_Self");
			ValidatePrefabSettings(ref m_scaleSettings.m_EnchantmentBannerScale, "m_EnchantmentBannerScale");
			ValidatePrefabSettings(ref m_scaleSettings.m_BigCardScaleFactorForEnchantments, "m_BigCardScaleFactorForEnchantments");
			ValidatePrefabSettings(ref m_scaleSettings.m_BigCardScale_Minion, "m_BigCardScale_Minion");
			ValidatePrefabSettings(ref m_scaleSettings.m_BigCardScale_LettuceAbility, "m_BigCardScale_LettuceAbility");
			ValidatePrefabSettings(ref m_scaleSettings.m_BigCardScale_Hero, "m_BigCardScale_Hero");
			ValidatePrefabSettings(ref m_scaleSettings.m_BigCardScale_Spell, "m_BigCardScale_Spell");
			ValidatePrefabSettings(ref m_scaleSettings.m_BigCardScale_Weapon, "m_BigCardScale_Weapon");
			ValidatePrefabSettings(ref m_scaleSettings.m_BigCardScale_HeroPower, "m_BigCardScale_HeroPower");
			ValidatePrefabSettings(ref m_scaleSettings.m_BigCardScale_Location, "m_BigCardScale_Location");
			ValidatePrefabSettings(ref m_scaleSettings.m_BigCardScale_BaconTrinket, "m_BigCardScale_Location");
			ValidatePrefabSettings(ref m_scaleSettings.m_BigCardScale_BaconTrinketHeropower, "m_BigCardScale_Location");
			ValidatePrefabSettings(ref m_scaleSettings.m_BigCardScale_BaconAnomaly, "m_BigCardScale_BaconAnomaly");
		}
	}

	private void ValidatePrefabSettings(ref float scaleSetting, string variableName)
	{
		if (scaleSetting <= 0f)
		{
			GameObject parentMostObject = base.gameObject;
			while (base.gameObject.transform.parent.gameObject != null)
			{
				parentMostObject = base.gameObject.transform.parent.gameObject;
			}
			Debug.LogError(variableName + " on object \"" + base.gameObject.name + "\" is an invalid value for the scale of a big card. Parent-most object is called \"" + parentMostObject.name + "\". This should be a positive number.  Value is being set to 1.");
			if (!StringUtils.CompareIgnoreCase(variableName, "m_BigCardScaleFactorForEnchantments"))
			{
				scaleSetting = 1f;
			}
			else
			{
				scaleSetting = 0.07f;
			}
		}
	}

	public bool HasAllBones()
	{
		if (m_OuterLeftBone != null && m_InnerLeftBone != null && m_InnerRightBone != null)
		{
			return m_OuterRightBone != null;
		}
		return false;
	}
}
