using System;
using System.Collections;
using Assets;
using Hearthstone;
using UnityEngine;

[CustomEditClass]
public class BasicScene : PegasusScene
{
	[CustomEditField(T = EditType.GAME_OBJECT)]
	public String_MobileOverride m_displayPrefab;

	public GameSaveKeyId RequiredGameSaveDataKey;

	public Global.PresenceStatus m_PresenceStatus = Global.PresenceStatus.UNKNOWN;

	protected const float IS_FINISHED_LOADING_TIMEOUT_SECONDS_NETWORK = 15f;

	protected const float IS_FINISHED_LOADING_TIMEOUT_SECONDS = 30f;

	protected bool m_displayPrefabLoaded;

	private bool m_gameSaveDataReceived;

	protected GameObject m_displayRoot;

	protected AbsSceneDisplay m_sceneDisplay;

	protected float m_isFinishedLoadingTimer;

	protected virtual void Start()
	{
		if (RequiredGameSaveDataKey != GameSaveKeyId.INVALID)
		{
			GameSaveDataManager.Get().Request(RequiredGameSaveDataKey, OnGameSaveDataReceived);
		}
		else
		{
			m_gameSaveDataReceived = true;
		}
		StartCoroutine(NotifySceneLoadedWhenReady());
	}

	private void Update()
	{
		Network.Get().ProcessNetwork();
	}

	public override void Unload()
	{
		if ((bool)UniversalInputManager.UsePhoneUI)
		{
			BnetBar bnetBar = BnetBar.Get();
			if (bnetBar != null)
			{
				bnetBar.ToggleActive(active: true);
			}
		}
		if (m_displayRoot != null && m_displayRoot.gameObject != null)
		{
			UnityEngine.Object.Destroy(m_displayRoot.gameObject);
		}
	}

	public override void ExecuteSceneDrivenTransition(Action onTransitionCompleteCallback)
	{
		if (m_sceneDisplay == null)
		{
			Debug.LogError("BasicScene.ExecuteSceneDrivenTransition() - Null Scene Display.");
			onTransitionCompleteCallback?.Invoke();
		}
		else
		{
			m_sceneDisplay.ShowSlidingTrayAfterSceneLoad(onTransitionCompleteCallback);
		}
	}

	public override bool IsBlockingPopupDisplayManager()
	{
		if (m_sceneDisplay != null)
		{
			return m_sceneDisplay.IsBlockingPopupDisplayManager();
		}
		return false;
	}

	private void OnGameSaveDataReceived(bool success)
	{
		m_gameSaveDataReceived = true;
	}

	private void OnDisplayPrefabLoaded(AssetReference assetRef, GameObject go, object callbackData)
	{
		m_displayPrefabLoaded = true;
		if (go == null)
		{
			Debug.LogError($"BasicScene.OnScreenLoaded() - failed to load screen {assetRef}");
		}
		else
		{
			m_displayRoot = go;
		}
	}

	protected virtual IEnumerator NotifySceneLoadedWhenReady()
	{
		while (!m_gameSaveDataReceived)
		{
			m_isFinishedLoadingTimer += Time.unscaledDeltaTime;
			if (m_isFinishedLoadingTimer > 15f)
			{
				Error.AddFatal(FatalErrorReason.LOAD_SCENE_NETWORK_TIMEOUT, "GLOBAL_ERROR_NETWORK_DISCONNECT");
				yield break;
			}
			yield return null;
		}
		if (m_PresenceStatus != Global.PresenceStatus.UNKNOWN)
		{
			PresenceMgr.Get().SetStatus(m_PresenceStatus);
		}
		AssetReference assetReference = (string)m_displayPrefab;
		if (!AssetLoader.Get().InstantiatePrefab(assetReference, OnDisplayPrefabLoaded))
		{
			OnDisplayPrefabLoaded(assetReference, null, null);
		}
		while (!m_displayPrefabLoaded)
		{
			m_isFinishedLoadingTimer += Time.unscaledDeltaTime;
			if (m_isFinishedLoadingTimer > 30f)
			{
				DisplayFailedToLoadDialog("Display prefab never instantiated.");
				yield break;
			}
			yield return null;
		}
		while (m_sceneDisplay == null)
		{
			m_sceneDisplay = m_displayRoot.GetComponentInChildren<AbsSceneDisplay>();
			m_isFinishedLoadingTimer += Time.unscaledDeltaTime;
			if (m_isFinishedLoadingTimer > 30f)
			{
				DisplayFailedToLoadDialog("SceneDisplay script was not found on the display prefab.");
				yield break;
			}
			yield return null;
		}
		PassSceneTransitionPayloadToSceneDisplay();
		string failureMessage = string.Empty;
		while (!m_sceneDisplay.IsFinishedLoading(out failureMessage) || !m_sceneDisplay.IsRootWidgetDoneChangingStates())
		{
			m_isFinishedLoadingTimer += Time.unscaledDeltaTime;
			if (m_isFinishedLoadingTimer > 30f)
			{
				DisplayFailedToLoadDialog(failureMessage);
				yield break;
			}
			yield return null;
		}
		SceneMgr.Get().NotifySceneLoaded();
	}

	protected void DisplayFailedToLoadDialog(string devFailureMessage)
	{
		if (HearthstoneApplication.IsPublic())
		{
			AlertPopup.PopupInfo info = new AlertPopup.PopupInfo();
			info.m_headerText = GameStrings.Get("GLUE_SCENE_LOAD_ERROR_TITLE");
			info.m_text = GameStrings.Get("GLUE_SCENE_LOAD_ERROR_BODY");
			info.m_iconSet = AlertPopup.PopupInfo.IconSet.Default;
			info.m_showAlertIcon = true;
			info.m_alertTextAlignment = UberText.AlignmentOptions.Center;
			info.m_responseDisplay = AlertPopup.ResponseDisplay.OK;
			info.m_okText = GameStrings.Get("GLOBAL_OKAY");
			DialogManager.Get().ShowPopup(info);
			SceneMgr.Get().SetNextMode(SceneMgr.Mode.HUB);
		}
		else
		{
			AlertPopup.PopupInfo info2 = new AlertPopup.PopupInfo();
			info2.m_headerText = GameStrings.Get("GLUE_SCENE_LOAD_ERROR_TITLE");
			info2.m_text = string.Format("{0}\n<color=red>{1}</color>", GameStrings.Get("GLUE_SCENE_LOAD_ERROR_BODY"), devFailureMessage);
			info2.m_iconSet = AlertPopup.PopupInfo.IconSet.Default;
			info2.m_showAlertIcon = true;
			info2.m_alertTextAlignment = UberText.AlignmentOptions.Center;
			info2.m_responseDisplay = AlertPopup.ResponseDisplay.OK;
			info2.m_okText = GameStrings.Get("GLOBAL_OKAY");
			DialogManager.Get().ShowPopup(info2);
			SceneMgr.Get().SetNextMode(SceneMgr.Mode.HUB);
		}
	}

	private void PassSceneTransitionPayloadToSceneDisplay()
	{
		m_sceneDisplay.SetSceneTransitionPayload(m_sceneTransitionPayload);
	}
}
