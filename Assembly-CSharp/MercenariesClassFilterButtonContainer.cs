using Hearthstone.UI;
using UnityEngine;

public class MercenariesClassFilterButtonContainer : MonoBehaviour
{
	public AsyncReference m_campfireButton;

	public SlidingTray m_slidingTray;

	public MercenariesClassFilterButton[] m_filterButtons;

	public static readonly string CAMPFIRE_BUTTON_CLICKED = "Campfire_Button_Clicked";

	public void Start()
	{
		m_campfireButton.RegisterReadyListener(delegate(Widget w)
		{
			OnCampfireButtonReady(w);
		});
		PegUI.Get().RegisterForRenderPassPriorityHitTest(this);
	}

	public void OnDestroy()
	{
		PegUI.Get().UnregisterFromRenderPassPriorityHitTest(this);
	}

	private void OnCampfireButtonReady(Widget w)
	{
		w.RegisterEventListener(OnCampfireButtonEvent);
	}

	private void OnCampfireButtonEvent(string eventName)
	{
		if (eventName == CAMPFIRE_BUTTON_CLICKED)
		{
			m_slidingTray.HideTray();
		}
	}

	public void UpdateRoleButtons()
	{
		LettuceCollectionPageManager pageManager = CollectionManager.Get().GetCollectibleDisplay().GetPageManager() as LettuceCollectionPageManager;
		MercenariesClassFilterButton[] filterButtons = m_filterButtons;
		foreach (MercenariesClassFilterButton filterButton in filterButtons)
		{
			filterButton.SetNewCardCount(0);
			if (pageManager.HasRoleCardsAvailable(filterButton.Role))
			{
				int numNewCardsForClass = pageManager.GetNumNewCardsForRole(filterButton.Role);
				filterButton.SetNewCardCount(numNewCardsForClass);
			}
		}
	}
}
