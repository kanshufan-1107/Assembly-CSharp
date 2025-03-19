using System;

namespace Hearthstone.InGameMessage.UI;

public interface IInGameMessageEventControl : IDisposable
{
	void Initialize(Action<PopupEvent> onTiggerEventCallback);
}
