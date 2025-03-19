[CustomEditClass]
public class VillagePackOpeningScene : BasicScene
{
	public override void PreUnload()
	{
		base.PreUnload();
		if (m_displayRoot != null)
		{
			VillagePackOpeningDisplay vpoDisplay = m_displayRoot.GetComponentInChildren<VillagePackOpeningDisplay>();
			if (vpoDisplay != null)
			{
				vpoDisplay.PreunloadPackOpeningView();
			}
		}
	}
}
