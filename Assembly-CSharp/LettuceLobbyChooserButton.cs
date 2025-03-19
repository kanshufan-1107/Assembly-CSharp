using System;
using System.Collections.Generic;
using Assets;
using Blizzard.T5.MaterialService.Extensions;
using Hearthstone.UI;
using Hearthstone.UI.Core;
using UnityEngine;

[CustomEditClass]
public class LettuceLobbyChooserButton : ChooserButton
{
	[Serializable]
	public class DifficultyTextureAsset
	{
		public LettuceBounty.MercenariesBountyDifficulty m_difficulty;

		public string m_textureAsset;

		public Vector2 m_textureTiling = Vector2.one;

		public Vector2 m_textureOffset = Vector2.zero;
	}

	[Serializable]
	public class DifficultyBorderOverride
	{
		public LettuceBounty.MercenariesBountyDifficulty m_difficulty;

		public Material m_borderMaterial;
	}

	public MeshRenderer m_glowRenderer;

	public VisualController m_visualController;

	public GameObject m_newIndicator;

	[CustomEditField(Sections = "Portrait Assets")]
	public List<DifficultyTextureAsset> m_textureAssets = new List<DifficultyTextureAsset>();

	[CustomEditField(Sections = "Border Settings")]
	public List<DifficultyBorderOverride> m_difficultyBorderOverrides = new List<DifficultyBorderOverride>();

	public LettuceLobbyChooserSubButton CreateLettuceLobbySubButton(string buttonText, SceneMgr.Mode nextModeWhenChosen, LettuceBountySetDbfRecord bountySetRecord, LettuceBounty.MercenariesBountyDifficulty difficulty, string subButtonPrefab, bool useAsLastSelected, int numNew = 0)
	{
		LettuceLobbyChooserSubButton newSubButton = (LettuceLobbyChooserSubButton)CreateSubButton(subButtonPrefab, useAsLastSelected);
		newSubButton.SetButtonText(buttonText);
		newSubButton.SetUnlocks(numNew);
		newSubButton.SetMode(nextModeWhenChosen);
		newSubButton.SetBountySetRecord(bountySetRecord.ID);
		newSubButton.SetDifficulty(difficulty);
		DifficultyTextureAsset textureAsset = m_textureAssets.Find((DifficultyTextureAsset x) => x.m_difficulty == difficulty);
		if (textureAsset != null && !string.IsNullOrEmpty(textureAsset.m_textureAsset))
		{
			newSubButton.SetPortraitTexture(textureAsset.m_textureAsset);
			newSubButton.SetPortraitTiling(textureAsset.m_textureTiling);
			newSubButton.SetPortraitOffset(textureAsset.m_textureOffset);
		}
		DifficultyBorderOverride borderOverride = m_difficultyBorderOverrides.Find((DifficultyBorderOverride x) => x.m_difficulty == difficulty);
		if (borderOverride != null && borderOverride.m_borderMaterial != null)
		{
			m_BorderRenderer.SetMaterial(borderOverride.m_borderMaterial);
		}
		GetComponentInParent<IPopupRoot>()?.ApplyPopupRendering(base.transform, new HashSet<IPopupRendering>(), overrideLayer: true, 31);
		return newSubButton;
	}

	public void SetNewCount(int newCount)
	{
		if (!(m_newIndicator == null))
		{
			m_newIndicator.SetActive(newCount > 0);
		}
	}
}
