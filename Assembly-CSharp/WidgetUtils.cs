using Hearthstone.DataModels;
using Hearthstone.UI;

public static class WidgetUtils
{
	public static EventDataModel GetEventDataModel(VisualController vc)
	{
		if (vc == null)
		{
			return null;
		}
		Widget widget = vc.GetComponent<Widget>();
		if (widget == null)
		{
			return null;
		}
		return widget.GetDataModel<EventDataModel>();
	}

	public static void BindorCreateDataModel<T>(Widget owner, int modelId, ref T dataModel) where T : class, IDataModel, new()
	{
		if (dataModel == null)
		{
			if (!owner.GetDataModel(modelId, out var newDM))
			{
				newDM = new T();
				owner.BindDataModel(newDM);
				dataModel = newDM as T;
			}
		}
		else
		{
			owner.BindDataModel(dataModel);
		}
	}
}
