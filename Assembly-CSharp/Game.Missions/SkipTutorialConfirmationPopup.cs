using Hearthstone.UI;
using UnityEngine;

namespace Game.Missions;

public class SkipTutorialConfirmationPopup : MonoBehaviour, IWidgetEventListener
{
	[SerializeField]
	private PlatformDependentVector3 m_parentLocalPosition = new PlatformDependentVector3(PlatformCategory.Screen)
	{
		PC = new Vector3(0f, 0f, -20f),
		Tablet = new Vector3(0f, 0f, -20f),
		Phone = new Vector3(0f, 0f, -50f)
	};

	[SerializeField]
	private PlatformDependentVector3 m_parentLocalScale = new PlatformDependentVector3(PlatformCategory.Screen)
	{
		PC = new Vector3(2f, 2f, 2f),
		Tablet = new Vector3(2f, 2f, 2f),
		Phone = new Vector3(3f, 3f, 3f)
	};

	private const string SkipTutorialConfirmEventName = "Button_Framed_Double_Left_Clicked";

	private const string DismissWindowEventName = "Button_Framed_Double_Right_Clicked";

	public WidgetTemplate OwningWidget { get; set; }

	private void Awake()
	{
		OwningWidget = GetComponent<WidgetTemplate>();
		base.transform.parent.localPosition = m_parentLocalPosition.Value;
		base.transform.parent.localScale = m_parentLocalScale.Value;
	}

	private void OnEnable()
	{
		ShowPopup();
		BnetBar.Get().HideSkipTutorialButton();
	}

	private void ShowPopup()
	{
		OverlayUI.Get().AddGameObject(base.transform.parent.gameObject);
		UIContext.GetRoot().ShowPopup(base.transform.parent.gameObject);
	}

	public WidgetEventListenerResponse EventReceived(string eventName, TriggerEventParameters eventParams)
	{
		WidgetEventListenerResponse result;
		if (!(eventName == "Button_Framed_Double_Left_Clicked"))
		{
			if (eventName == "Button_Framed_Double_Right_Clicked")
			{
				UIContext.GetRoot().DismissPopup(base.transform.parent.gameObject);
				base.transform.parent.gameObject.SetActive(value: false);
				BnetBar.Get().SkipTutorialPopupDismissed();
				result = default(WidgetEventListenerResponse);
				result.Consumed = true;
				return result;
			}
			result = default(WidgetEventListenerResponse);
			result.Consumed = false;
			return result;
		}
		UIContext.GetRoot().DismissPopup(base.transform.parent.gameObject);
		base.transform.parent.gameObject.SetActive(value: false);
		Gameplay.Get().SkipTutorial();
		result = default(WidgetEventListenerResponse);
		result.Consumed = true;
		return result;
	}
}
