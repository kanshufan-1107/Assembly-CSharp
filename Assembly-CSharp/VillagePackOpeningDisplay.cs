using System.Collections;

public class VillagePackOpeningDisplay : AbsSceneDisplay
{
	public PackOpening m_packOpeningView;

	public override void Start()
	{
		base.Start();
		if (m_packOpeningView != null)
		{
			m_packOpeningView.SetVillageDisplay(this);
		}
		StartCoroutine(InitializeWhenReady());
	}

	private IEnumerator InitializeWhenReady()
	{
		string failureMessage;
		while (!IsFinishedLoading(out failureMessage))
		{
			yield return null;
		}
		if (m_packOpeningView != null)
		{
			m_packOpeningView.Show();
		}
	}

	public void NavigateBack()
	{
		SetNextModeAndHandleTransition(SceneMgr.Mode.LETTUCE_VILLAGE, SceneMgr.TransitionHandlerType.CURRENT_SCENE);
	}

	public void PreunloadPackOpeningView()
	{
		if (m_packOpeningView != null)
		{
			m_packOpeningView.PreUnload();
		}
	}

	public override bool IsFinishedLoading(out string failureMessage)
	{
		if (m_packOpeningView == null || !m_packOpeningView.IsReady())
		{
			failureMessage = "VillagePackOpeningDisplay - Display never loaded.";
			return false;
		}
		failureMessage = string.Empty;
		return true;
	}

	protected override bool ShouldStartShown()
	{
		return false;
	}
}
