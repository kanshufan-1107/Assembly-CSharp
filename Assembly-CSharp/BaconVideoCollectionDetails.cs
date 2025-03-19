using Hearthstone.UI;
using UnityEngine;

public abstract class BaconVideoCollectionDetails : BaconCollectionDetails
{
	[SerializeField]
	private VisualController m_videoPreviewController;

	[SerializeField]
	private DynamicVideoLoader m_videoPreview;

	public override void Show()
	{
		base.Show();
		if (!(m_videoPreviewController == null))
		{
			EventFunctions.TriggerEvent(m_videoPreviewController.transform, "LOAD_VIDEO", TriggerEventParameters.Standard);
		}
	}

	public void ClearVideo()
	{
		if (!(m_videoPreviewController == null))
		{
			EventFunctions.TriggerEvent(m_videoPreviewController.transform, "CLEAR_VIDEO", TriggerEventParameters.Standard);
			if (m_videoPreview != null)
			{
				m_videoPreview.OnClosed();
			}
		}
	}

	public override void Hide()
	{
		base.Hide();
		ClearVideo();
	}

	public void SetScaleParentOverride(GameObject target)
	{
		m_scaleParent = target;
	}
}
