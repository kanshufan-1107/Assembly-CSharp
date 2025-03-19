using Hearthstone.DataModels;
using UnityEngine;

namespace Hearthstone.UI.Tests;

public class TestFindReferencesScriptRoot : MonoBehaviour
{
	private WidgetTemplate m_rootTestWidget;

	private void Awake()
	{
		m_rootTestWidget = GetComponent<WidgetTemplate>();
		PrototypeDataModel parentDataModel = new PrototypeDataModel
		{
			Bool1 = true,
			Int1 = 0,
			Float1 = 0f,
			String1 = "Zero"
		};
		PrototypeDataModel prototypeDataModel = new PrototypeDataModel().CopyFromDataModel(parentDataModel);
		PrototypeDataModel listDataModel2 = new PrototypeDataModel().CopyFromDataModel(parentDataModel);
		prototypeDataModel.Int1 = 1;
		listDataModel2.Int1 = 2;
		parentDataModel.List1.Add(new PrototypeDataModel().CopyFromDataModel(parentDataModel));
		parentDataModel.List1.Add(listDataModel2);
		m_rootTestWidget.BindDataModel(parentDataModel);
		m_rootTestWidget.RegisterEventListener(EventReceived);
	}

	private void EventReceived(string eventName)
	{
		if (eventName == "BUBBLE_UP_TO_ROOT")
		{
			Debug.Log("Event bubbled up to root widget!");
		}
	}
}
