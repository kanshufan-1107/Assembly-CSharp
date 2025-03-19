using System;
using Hearthstone.UI;
using UnityEngine;

public abstract class AbsSceneDisplay : MonoBehaviour
{
	public GameObject m_clickBlocker;

	public SlidingTray m_slidingTray;

	public AsyncReference m_sceneDisplayWidgetReference;

	private Action m_onSceneTransitionCompleteCallback;

	protected object m_sceneTransitionPayload;

	protected Widget m_sceneDisplayWidget;

	private bool m_sceneDisplayWidgetDoneChangingStates;

	protected abstract bool ShouldStartShown();

	public abstract bool IsFinishedLoading(out string failureMessage);

	public virtual void Start()
	{
		SetClickBlockerActive(active: false);
		if (m_slidingTray != null)
		{
			m_slidingTray.OnTransitionComplete += OnSlidingTrayAnimationComplete;
			InitializeSlidingTray();
		}
		if (m_sceneDisplayWidgetReference != null)
		{
			m_sceneDisplayWidgetReference.RegisterReadyListener<VisualController>(OnSceneDisplayWidgetReady);
		}
		else
		{
			m_sceneDisplayWidgetDoneChangingStates = true;
		}
	}

	public void ShowSlidingTrayAfterSceneLoad(Action onCompleteCallback)
	{
		SetClickBlockerActive(active: true);
		m_onSceneTransitionCompleteCallback = onCompleteCallback;
		if (m_slidingTray != null)
		{
			m_slidingTray.ShowTray();
		}
		else
		{
			OnSlidingTrayAnimationComplete();
		}
	}

	public void SetSceneTransitionPayload(object payload)
	{
		m_sceneTransitionPayload = payload;
	}

	public void SetClickBlockerActive(bool active)
	{
		if (m_clickBlocker != null)
		{
			m_clickBlocker.SetActive(active);
		}
	}

	public bool IsRootWidgetDoneChangingStates()
	{
		return m_sceneDisplayWidgetDoneChangingStates;
	}

	public void SetNextModeAndHandleTransition(SceneMgr.Mode nextMode, object sceneTransitionPayload = null)
	{
		SetNextModeAndHandleTransition(nextMode, SceneMgr.TransitionHandlerType.CURRENT_SCENE, sceneTransitionPayload);
	}

	public void SetNextModeAndHandleTransition(SceneMgr.Mode nextMode, SceneMgr.TransitionHandlerType type, object sceneTransitionPayload = null)
	{
		SetClickBlockerActive(active: true);
		SceneMgr.Get().SetNextMode(nextMode, type, OnSceneLoadCompleteHandleTransition, sceneTransitionPayload);
	}

	public virtual bool IsBlockingPopupDisplayManager()
	{
		return false;
	}

	protected void InitializeSlidingTray()
	{
		if (!(m_slidingTray == null))
		{
			bool startShown = ShouldStartShown();
			m_slidingTray.ToggleTraySlider(startShown, null, animate: false);
		}
	}

	protected void OnSceneLoadCompleteHandleTransition(Action onTransitionComplete)
	{
		m_onSceneTransitionCompleteCallback = onTransitionComplete;
		if (m_slidingTray != null)
		{
			m_slidingTray.HideTray();
		}
		else
		{
			OnSlidingTrayAnimationComplete();
		}
	}

	protected virtual void OnSlidingTrayAnimationComplete()
	{
		SetClickBlockerActive(active: false);
		if (m_onSceneTransitionCompleteCallback != null)
		{
			m_onSceneTransitionCompleteCallback();
			m_onSceneTransitionCompleteCallback = null;
		}
	}

	private void OnSceneDisplayWidgetReady(VisualController visualController)
	{
		if (visualController == null)
		{
			m_sceneDisplayWidgetDoneChangingStates = true;
			return;
		}
		m_sceneDisplayWidget = visualController.Owner;
		m_sceneDisplayWidget.RegisterDoneChangingStatesListener(delegate
		{
			m_sceneDisplayWidgetDoneChangingStates = true;
		});
	}
}
