using System;
using Blizzard.Telemetry.WTCG.Client;

namespace Hearthstone.InGameMessage.UI;

public class UIMessageCallbacks
{
	public Action<InGameMessageAction.ActionType> OnViewed { get; set; }

	public Action OnClosed { get; set; }

	public Action OnStoreOpened { get; set; }

	public Action OnDisplayed { get; set; }
}
