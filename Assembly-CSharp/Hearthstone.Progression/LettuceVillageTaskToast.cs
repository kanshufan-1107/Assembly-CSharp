using Hearthstone.DataModels;
using Hearthstone.UI;
using UnityEngine;

namespace Hearthstone.Progression;

[RequireComponent(typeof(WidgetTemplate))]
public class LettuceVillageTaskToast : MonoBehaviour
{
	private WidgetTemplate m_toast;

	public static Vector3 TASK_TOAST_OFFSET = new Vector3(0f, 0f, 25f);

	public static Vector3 TASK_TOAST_OFFSET_PHONE = new Vector3(0f, 0f, 50f);

	private const string CODE_HIDE = "CODE_HIDE";

	private void Awake()
	{
		m_toast = GetComponent<WidgetTemplate>();
		m_toast.RegisterEventListener(delegate(string eventName)
		{
			if (eventName == "CODE_HIDE")
			{
				Hide();
			}
		});
	}

	public void Initialize(MercenaryVillageTaskItemDataModel taskItemModel)
	{
		m_toast.BindDataModel(taskItemModel);
	}

	public void Show()
	{
		if (!(m_toast == null))
		{
			OverlayUI.Get().AddGameObject(base.gameObject.transform.parent.gameObject);
			m_toast.Show();
		}
	}

	public void Hide()
	{
		if (!(m_toast == null))
		{
			m_toast.Hide();
			Object.Destroy(base.transform.parent.gameObject);
		}
	}
}
