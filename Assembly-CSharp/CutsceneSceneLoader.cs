using System;
using Blizzard.T5.AssetManager;
using Blizzard.T5.Services;
using Hearthstone.UI;
using UnityEngine;

[CustomEditClass]
public class CutsceneSceneLoader : MonoBehaviour
{
	private const string WIDGET_TRIGGER_EVENT = "LOAD_CUTSCENE";

	[Tooltip("The cutscene prefab to be loaded on demand.")]
	[CustomEditField(T = EditType.GAME_OBJECT, Sections = "Scene Prefab To Load")]
	public string m_CutsceneObject;

	[Tooltip("Optional: When loading is triggered via widget events, reference the widget owner of this component so events can be listen for.")]
	[CustomEditField(T = EditType.DEFAULT, Sections = "Optional Widget Trigger (Event=LOAD_CUTSCENE)")]
	public Widget m_OwnerWidget;

	private AssetHandle<GameObject> m_cutSceneAssetHandle;

	private void Awake()
	{
		if (string.IsNullOrEmpty(m_CutsceneObject))
		{
			Log.All.PrintWarning("CutsceneSceneLoader had no scene to load, skipping...");
			base.enabled = false;
		}
	}

	private void Start()
	{
		if (m_OwnerWidget != null)
		{
			m_OwnerWidget.RegisterEventListener(OnWidgetEventReceived);
		}
	}

	private void OnDestroy()
	{
		if (m_OwnerWidget != null)
		{
			m_OwnerWidget.RemoveEventListener(OnWidgetEventReceived);
		}
	}

	private void OnWidgetEventReceived(string eventName)
	{
		if (eventName.Equals("LOAD_CUTSCENE", StringComparison.OrdinalIgnoreCase))
		{
			if (m_OwnerWidget != null)
			{
				m_OwnerWidget.RemoveEventListener(OnWidgetEventReceived);
			}
			LoadCutscene();
		}
	}

	public void Cleanup()
	{
		if (m_cutSceneAssetHandle != null)
		{
			m_cutSceneAssetHandle.Dispose();
		}
	}

	public bool LoadCutscene()
	{
		if (!ServiceManager.TryGet<IAssetLoader>(out var assetLoader))
		{
			Log.CosmeticPreview.PrintError("CutsceneSceneLoader failed to load provided cutscene asset as IAssetLoader was null!");
			return false;
		}
		m_cutSceneAssetHandle = assetLoader.GetOrInstantiateSharedPrefab(m_CutsceneObject);
		return m_cutSceneAssetHandle;
	}
}
