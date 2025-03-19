using UnityEngine;

public class CameraFade : MonoBehaviour
{
	public Color m_Color = Color.black;

	public float m_Fade = 1f;

	private FullScreenEffects m_FullScreenEffects;

	private bool m_hasFullScreenEffects;

	private void Update()
	{
		if (!m_hasFullScreenEffects)
		{
			FullScreenFXMgr mgr = FullScreenFXMgr.Get();
			if (mgr == null)
			{
				return;
			}
			m_FullScreenEffects = mgr.SecondaryCameraFullScreenEffects;
			m_hasFullScreenEffects = true;
		}
		if (m_FullScreenEffects == null)
		{
			m_hasFullScreenEffects = false;
		}
		else if (m_Fade <= 0f)
		{
			m_FullScreenEffects.DisableBlendToColorOverride();
		}
		else
		{
			m_FullScreenEffects.SetBlendToColorOverride(m_Fade, m_Color);
		}
	}
}
