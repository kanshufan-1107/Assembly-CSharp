using System.Collections.Generic;

namespace Hearthstone.UI;

public interface IWidgetState
{
	string Name { get; }

	IEnumerable<StateAction> Actions { get; }
}
