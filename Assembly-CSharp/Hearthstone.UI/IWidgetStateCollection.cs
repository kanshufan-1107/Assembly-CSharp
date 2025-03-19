using System.Collections.Generic;

namespace Hearthstone.UI;

public interface IWidgetStateCollection
{
	IWidgetState ActiveState { get; }

	bool IsChangingStates { get; }

	IList<IWidgetState> GetStateList();
}
