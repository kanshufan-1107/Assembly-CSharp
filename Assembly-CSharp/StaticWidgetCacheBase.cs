using System.Collections.Generic;
using Hearthstone.UI;
using UnityEngine;

public abstract class StaticWidgetCacheBase : MonoBehaviour
{
	public abstract string GetUniqueIdentifier(IDataModel dataModel);

	public abstract void RequestWidget(StaticWidgetCacheLender lender, IDataModel dataModelBase, GameObject handler = null, GameLayer overrideLayer = GameLayer.Default);

	public abstract void ReturnWidgets(StaticWidgetCacheLender lender);

	public abstract void Preload(IEnumerable<IDataModel> dataModels, bool createNew = false);

	public abstract void Rebind(IDataModel dataModelBase);

	public abstract void Pause(bool pause, string pauseRequestId);
}
