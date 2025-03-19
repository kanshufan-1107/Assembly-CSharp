using Hearthstone.DataModels;
using Hearthstone.UI;

public class CatchupPackCustomVisualReward : CustomVisualReward
{
	public override void Start()
	{
		Widget catchupLetter = GetComponent<Widget>();
		catchupLetter.RegisterReadyListener(delegate
		{
			catchupLetter.TriggerEvent("GROW_INVITE");
			catchupLetter.RegisterEventListener(HandleCatchupLetterEvent);
		});
	}

	protected override void OnNewAssociatedAchievement()
	{
		if (base.AssociatedAchievement != null && base.AssociatedAchievement.Rewards.Count > 0)
		{
			Widget catchupLetter = GetComponent<Widget>();
			if (catchupLetter != null && base.AssociatedAchievement.Rewards[0] is BoosterPackRewardData achieveReward)
			{
				PackDataModel packDataModel = new PackDataModel
				{
					Type = (BoosterDbId)achieveReward.Id,
					Quantity = achieveReward.Count
				};
				catchupLetter.BindDataModel(packDataModel);
			}
		}
	}

	private void HandleCatchupLetterEvent(string eventName)
	{
		if (eventName == "CODE_ON_LETTER_HIDDEN")
		{
			QuestToast.ShowQuestToast(base.AssociatedAchievement.GetUserAttentionBlocker(), delegate
			{
				Complete();
			}, updateCacheValues: false, base.AssociatedAchievement);
		}
	}
}
