using Hearthstone.UI;
using UnityEngine;

public class BaconEmoteTrayWidget : MonoBehaviour
{
	public bool m_pointBubbleRight;

	[SerializeField]
	private AsyncReference m_imageWidget;

	private void Start()
	{
		m_imageWidget.RegisterReadyListener(delegate(Transform t)
		{
			BoxCollider[] componentsInChildren = t.GetComponentsInChildren<BoxCollider>();
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				componentsInChildren[i].enabled = false;
			}
		});
	}
}
