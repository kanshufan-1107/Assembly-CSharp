using System;

[Serializable]
[CustomEditClass]
public class BaconCosmeticPreviewAction
{
	[CustomEditField(SortPopupByName = true)]
	public BaconCosmeticPreviewActionType actionType;

	public float delay;

	public float duration;

	public bool waitUntilFinished;

	[CustomEditField(HidePredicate = "ShouldHideBoardState")]
	public TAG_BOARD_VISUAL_STATE boardState;

	[CustomEditField(HidePredicate = "ShouldHideFsmParameter")]
	public string fsmParameter;

	[CustomEditField(SortPopupByName = true, HidePredicate = "ShouldHideFinisherParams")]
	public KeyboardFinisherSettings.DamageLevel strikeDamageLevel;

	[CustomEditField(SortPopupByName = true, HidePredicate = "ShouldHideFinisherParams")]
	public KeyboardFinisherSettings.LethalLevel strikeLethalLevel;

	[CustomEditField(HidePredicate = "ShouldHideFinisherParams")]
	public int strikeImpactDamage;

	public BaconCosmeticDisplayStage displayStage;

	public string GetDisplayText()
	{
		return displayStage switch
		{
			BaconCosmeticDisplayStage.STAGE_1 => "GLUE_COLLECTION_STAGE_ONE", 
			BaconCosmeticDisplayStage.STAGE_2 => "GLUE_COLLECTION_STAGE_TWO", 
			BaconCosmeticDisplayStage.STAGE_3 => "GLUE_COLLECTION_STAGE_THREE", 
			BaconCosmeticDisplayStage.STAGE_4 => "GLUE_COLLECTION_STAGE_FOUR", 
			_ => string.Empty, 
		};
	}
}
