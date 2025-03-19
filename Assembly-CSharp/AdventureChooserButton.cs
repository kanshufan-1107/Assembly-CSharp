using UnityEngine;

[CustomEditClass]
public class AdventureChooserButton : ChooserButton
{
	private AdventureDbId m_AdventureId;

	public void SetAdventure(AdventureDbId id)
	{
		m_AdventureId = id;
	}

	public AdventureDbId GetAdventure()
	{
		return m_AdventureId;
	}

	public AdventureChooserSubButton CreateSubButton(AdventureDbId adventureDbId, AdventureModeDbId adventureModeDbId, AdventureSubDef subDef, string subButtonPrefab, bool useAsLastSelected)
	{
		ChooserSubButton newChooserButton = CreateSubButton(subButtonPrefab, useAsLastSelected);
		AdventureChooserSubButton newAdvSubButton = ((newChooserButton != null) ? ((AdventureChooserSubButton)newChooserButton) : null);
		if (newAdvSubButton == null)
		{
			Debug.LogError("newAdvSubButton cannot be null. Unable to create newAdvSubButton.", this);
			return null;
		}
		string buttonText = subDef.GetShortName();
		if (!AdventureConfig.CanPlayMode(m_AdventureId, adventureModeDbId, checkEventTimings: false) && !string.IsNullOrEmpty(subDef.GetLockedShortName()))
		{
			buttonText = subDef.GetLockedShortName();
		}
		newAdvSubButton.gameObject.name = $"{newAdvSubButton.gameObject.name}_{adventureModeDbId}";
		newAdvSubButton.SetAdventure(adventureDbId, adventureModeDbId);
		newAdvSubButton.SetButtonText(buttonText);
		newAdvSubButton.SetPortraitTexture(subDef.m_Texture);
		newAdvSubButton.SetPortraitTiling(subDef.m_TextureTiling);
		newAdvSubButton.SetPortraitOffset(subDef.m_TextureOffset);
		return newAdvSubButton;
	}

	public AdventureChooserSubButton CreateComingSoonSubButton(AdventureModeDbId adventureModeDbId, string comingSoonSubButtonPrefab)
	{
		ChooserSubButton newChooserButton = CreateSubButton(comingSoonSubButtonPrefab, useAsLastSelected: true);
		AdventureChooserSubButton comingSoonSubButton = ((newChooserButton != null) ? ((AdventureChooserSubButton)newChooserButton) : null);
		if (comingSoonSubButton == null)
		{
			Debug.LogError("comingSoonSubButton cannot be null. Unable to create comingSoonSubButton.", this);
			return null;
		}
		comingSoonSubButton.SetEnabled(enabled: false);
		string buttonText = GameStrings.Get("GLOBAL_DATETIME_COMING_SOON");
		base.SubButtonHeight = comingSoonSubButton.m_ComingSoonBannerHeightOverride;
		comingSoonSubButton.SetAdventure(m_AdventureId, adventureModeDbId);
		comingSoonSubButton.SetButtonText(buttonText);
		return comingSoonSubButton;
	}
}
