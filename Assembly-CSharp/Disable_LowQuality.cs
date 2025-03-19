using Blizzard.T5.Services;
using UnityEngine;

public class Disable_LowQuality : MonoBehaviour
{
	private IGraphicsManager m_graphicsManager;

	private void Awake()
	{
		m_graphicsManager = ServiceManager.Get<IGraphicsManager>();
		m_graphicsManager.RegisterLowQualityDisableObject(base.gameObject);
		if (m_graphicsManager.RenderQualityLevel == GraphicsQuality.Low)
		{
			base.gameObject.SetActive(value: false);
		}
	}

	private void OnDestroy()
	{
		if (m_graphicsManager != null)
		{
			m_graphicsManager.DeregisterLowQualityDisableObject(base.gameObject);
		}
	}
}
