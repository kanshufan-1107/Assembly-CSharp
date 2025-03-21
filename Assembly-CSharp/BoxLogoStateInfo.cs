using System;

[Serializable]
public class BoxLogoStateInfo
{
	public float m_ShownAlpha = 1f;

	public float m_ShownDelaySec;

	public float m_ShownFadeSec = 0.3f;

	public iTween.EaseType m_ShownFadeEaseType = iTween.EaseType.linear;

	public float m_HiddenAlpha;

	public float m_HiddenDelaySec;

	public float m_HiddenFadeSec = 0.3f;

	public iTween.EaseType m_HiddenFadeEaseType = iTween.EaseType.linear;
}
