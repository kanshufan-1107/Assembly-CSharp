public class LockedHeroTooltipPanel : TooltipPanel
{
	private TAG_CLASS m_lockedClass;

	private void Awake()
	{
		LayerUtils.SetLayer(base.gameObject, GameLayer.Tooltip);
		m_scaleToUse = TooltipPanel.GAMEPLAY_SCALE;
	}

	public void SetLockedClass(TAG_CLASS lockedClass)
	{
		m_lockedClass = lockedClass;
	}
}
