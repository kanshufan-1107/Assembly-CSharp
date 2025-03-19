using Hearthstone.DataModels;
using Hearthstone.UI;
using UnityEngine;

public class LettuceTeamPreviewEntry : MonoBehaviour
{
	public Widget m_teamWidget;

	public AsyncReference m_mercenaryReference;

	private VisualController m_mercVisualController;

	private void Start()
	{
		m_teamWidget.RegisterEventListener(MercenaryEventListener);
		m_mercenaryReference.RegisterReadyListener(delegate(VisualController vs)
		{
			m_mercVisualController = vs;
		});
	}

	private void OnDestroy()
	{
		m_teamWidget.RemoveEventListener(MercenaryEventListener);
	}

	private void MercenaryEventListener(string eventName)
	{
		if (eventName == "MERC_LOADOUT_RELEASED")
		{
			OpenMercDetailsScreen();
		}
	}

	private void OpenMercDetailsScreen()
	{
		IMercDetailsDisplayProvider detailsDisplayProvider = GetComponentInParent<IMercDetailsDisplayProvider>();
		if (detailsDisplayProvider == null)
		{
			Debug.LogError("Unable to find IMercDetailsDisplayProvider in parents");
			return;
		}
		if (!(WidgetUtils.GetEventDataModel(m_mercVisualController).Payload is LettuceMercenaryDataModel mercDataModel))
		{
			Debug.LogError("Unable to find LettuceMercenaryDataModel from latest Widget events");
			return;
		}
		LettuceMercenary mercenary = CollectionManager.Get().GetMercenary(mercDataModel.MercenaryId);
		if (mercenary == null)
		{
			Debug.LogError($"Unable to find mercenary with ID {mercDataModel.MercenaryId}");
		}
		else
		{
			detailsDisplayProvider.ShowMercDetailsDisplay(mercenary);
		}
	}
}
