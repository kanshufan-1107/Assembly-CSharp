using System;
using Hearthstone.UI;
using UnityEngine;

[RequireComponent(typeof(WidgetTemplate))]
public class RewardTrackInfographic : BasicPopup
{
	public class RewardTrackInfographicInfo : PopupInfo
	{
		public Action m_onHiddenCallback;
	}

	public static readonly AssetReference PrefabReference = new AssetReference("NPPG_RewardTrackFlow.prefab:950486367395976479668ac78af0d9e7");

	private const string SHOW_EVENT_NAME = "SHOW_POPUP";

	private const string HIDE_FINISHED_EVENT_NAME = "CODE_HIDE_FINISHED";

	private WidgetTemplate m_widget;

	protected override void Awake()
	{
		m_widget = GetComponent<WidgetTemplate>();
		m_widget.RegisterEventListener(delegate(string eventName)
		{
			if (eventName == "CODE_HIDE_FINISHED")
			{
				Hide();
			}
		});
	}

	protected override void OnDestroy()
	{
		GameObject go = base.transform.parent.gameObject;
		if (go != null && go.GetComponent<WidgetInstance>() != null)
		{
			UnityEngine.Object.Destroy(base.transform.parent.gameObject);
		}
		base.OnDestroy();
	}

	public override void Show()
	{
		if (!(m_widget == null))
		{
			OverlayUI.Get().AddGameObject(base.gameObject, CanvasAnchor.CENTER, destroyOnSceneLoad: false, (!UniversalInputManager.UsePhoneUI) ? CanvasScaleMode.HEIGHT : CanvasScaleMode.WIDTH);
			m_widget.TriggerEvent("SHOW_POPUP");
		}
	}

	public override void Hide()
	{
		if (m_popupInfo is RewardTrackInfographicInfo { m_onHiddenCallback: not null } popupInfo)
		{
			popupInfo.m_onHiddenCallback();
		}
		IncrementPopupSeenFlag();
		if (m_readyToDestroyCallback != null)
		{
			m_readyToDestroyCallback(this);
		}
	}

	private void IncrementPopupSeenFlag()
	{
		GameSaveDataManager.Get().SaveSubkey(new GameSaveDataManager.SubkeySaveRequest(GameSaveKeyId.FTUE, GameSaveKeySubkeyId.FTUE_HAS_SEEN_REWARD_TRACK_INFOGRAPHIC, 1L));
	}
}
