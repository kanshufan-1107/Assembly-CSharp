using System;
using UnityEngine;

public class PegasusScene : MonoBehaviour
{
	protected object m_sceneTransitionPayload;

	protected string m_sceneName;

	public string SceneName => m_sceneName;

	protected virtual void Awake()
	{
		SceneMgr sceneMgr = SceneMgr.Get();
		if (sceneMgr != null)
		{
			sceneMgr.SetScene(this);
		}
		else
		{
			Log.All.PrintWarning("PegasusScene.Awake called when SceneMgr is null!");
		}
	}

	public virtual void PreUnload()
	{
	}

	public virtual bool IsUnloading()
	{
		return false;
	}

	public virtual void Unload()
	{
	}

	public virtual bool IsTransitioning()
	{
		return false;
	}

	public virtual bool HandleKeyboardInput()
	{
		if (BackButton.backKey != 0 && InputCollection.GetKeyUp(BackButton.backKey))
		{
			if (DialogManager.Get().ShowingDialog())
			{
				DialogManager.Get().GoBack();
				return true;
			}
			if (ChatMgr.Get().IsFriendListShowing() || ChatMgr.Get().IsChatLogFrameShown())
			{
				ChatMgr.Get().GoBack();
				return true;
			}
			if (OptionsMenu.Get() != null && OptionsMenu.Get().IsShown())
			{
				OptionsMenu.Get().Hide();
				return true;
			}
			if (MiscellaneousMenu.Get() != null && MiscellaneousMenu.Get().IsShown())
			{
				MiscellaneousMenu.Get().Hide();
				return true;
			}
			if (BnetBar.Get() != null && BnetBar.Get().IsGameMenuShown())
			{
				BnetBar.Get().HideGameMenu();
				return true;
			}
			if (Navigation.GoBack())
			{
				return true;
			}
		}
		return false;
	}

	public virtual void ExecuteSceneDrivenTransition(Action onTransitionCompleteCallback)
	{
		Log.All.PrintError("Scene.ExecuteSceneDrivenTransition - Function was not overridden!");
		onTransitionCompleteCallback();
	}

	public void SetSceneTransitionPayload(object payload)
	{
		m_sceneTransitionPayload = payload;
	}

	public object GetSceneTransitionPayload()
	{
		return m_sceneTransitionPayload;
	}

	public void SetSceneName(string sceneName)
	{
		m_sceneName = sceneName;
	}

	public virtual bool IsBlockingPopupDisplayManager()
	{
		return false;
	}
}
