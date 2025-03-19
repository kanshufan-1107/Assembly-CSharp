using System;
using Assets;

namespace Hearthstone.DungeonCrawl;

public class DungeonCrawlSubscenControllerAdapter : ISubsceneController
{
	public event EventHandler TransitionComplete;

	public void ChangeSubScene(AdventureData.Adventuresubscene subscene, bool pushToBackStack = true)
	{
		AdventureConfig ac = AdventureConfig.Get();
		if (!(ac == null))
		{
			ac.ChangeSubScene(subscene, pushToBackStack);
		}
	}

	public void SubSceneGoBack(bool fireEvent = true)
	{
		AdventureConfig ac = AdventureConfig.Get();
		if (!(ac == null))
		{
			ac.SubSceneGoBack(fireEvent);
		}
	}

	public void RemoveSubSceneIfOnTopOfStack(AdventureData.Adventuresubscene subscene)
	{
		AdventureConfig ac = AdventureConfig.Get();
		if (!(ac == null))
		{
			ac.RemoveSubSceneIfOnTopOfStack(subscene);
		}
	}

	public void RemoveSubScenesFromStackUntilTargetReached(AdventureData.Adventuresubscene targetSubscene)
	{
		AdventureConfig ac = AdventureConfig.Get();
		if (!(ac == null))
		{
			ac.RemoveSubScenesFromStackUntilTargetReached(targetSubscene);
		}
	}

	public void OnTransitionComplete()
	{
		if (this.TransitionComplete != null)
		{
			this.TransitionComplete(this, EventArgs.Empty);
		}
	}
}
