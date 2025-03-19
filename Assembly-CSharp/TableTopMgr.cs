using UnityEngine;

public class TableTopMgr : MonoBehaviour
{
	public MeshRenderer m_renderer;

	private bool m_active = true;

	private void Start()
	{
		if (!UniversalInputManager.UsePhoneUI)
		{
			m_active = false;
			base.enabled = false;
		}
		else if (GameUtils.CanCheckTutorialCompletion() && GameUtils.IsAnyTutorialComplete())
		{
			DisableTableTopRenderer();
			m_active = false;
			base.enabled = false;
		}
		else
		{
			Box.Get().AddTransitionFinishedListener(OnBoxTransitionFinished);
		}
	}

	private void OnDestroy()
	{
		if (Box.Get() != null)
		{
			Box.Get().RemoveTransitionFinishedListener(OnBoxTransitionFinished);
		}
	}

	private void OnBoxTransitionFinished(object userData)
	{
		if (m_active)
		{
			if (GameUtils.CanCheckTutorialCompletion() && GameUtils.IsAnyTutorialComplete())
			{
				Box.Get().RemoveTransitionFinishedListener(OnBoxTransitionFinished);
				DisableTableTopRenderer();
				m_active = false;
				base.enabled = false;
			}
			else if (Box.Get().GetState() == Box.State.HUB)
			{
				EnableTableTopRenderer();
			}
		}
	}

	public void HideTableTop()
	{
		DisableTableTopRenderer();
	}

	private void EnableTableTopRenderer()
	{
		m_renderer.enabled = true;
	}

	private void DisableTableTopRenderer()
	{
		m_renderer.enabled = false;
	}
}
