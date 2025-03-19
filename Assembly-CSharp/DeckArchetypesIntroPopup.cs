using System;
using Blizzard.T5.MaterialService.Extensions;
using Hearthstone.UI;
using UnityEngine;

[RequireComponent(typeof(WidgetTemplate))]
public class DeckArchetypesIntroPopup : BasicPopup
{
	public class DeckArchetypesIntroPopupInfo : PopupInfo
	{
		public Action m_onHiddenCallback;
	}

	public static readonly AssetReference PrefabReference = new AssetReference("NPPG_DeckPickerArchetypes.prefab:6b850c6784b6e6044af0bcef1d4e506f");

	public MeshRenderer m_hunterPortraitMesh;

	public int m_hunterPortraitMaterialIndex;

	public MeshRenderer m_magePortraitMesh;

	public int m_magePortraitMaterialIndex;

	private const string SHOW_EVENT_NAME = "SHOW_POPUP";

	private const string HIDE_EVENT_NAME = "OKAY_CLICKED";

	private const string HIDE_FINISHED_EVENT_NAME = "CODE_HIDE_FINISHED";

	private WidgetTemplate m_widget;

	private DefLoader.DisposableCardDef m_hunterCardDef;

	private DefLoader.DisposableCardDef m_mageCardDef;

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
		InitHeroPortaits();
	}

	protected override void OnDestroy()
	{
		GameObject go = base.transform.parent.gameObject;
		if (go != null && go.GetComponent<WidgetInstance>() != null)
		{
			UnityEngine.Object.Destroy(base.transform.parent.gameObject);
		}
		m_hunterCardDef?.Dispose();
		m_mageCardDef?.Dispose();
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
		if (m_popupInfo is DeckArchetypesIntroPopupInfo { m_onHiddenCallback: not null } popupInfo)
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
		GameSaveDataManager.Get().SaveSubkey(new GameSaveDataManager.SubkeySaveRequest(GameSaveKeyId.FTUE, GameSaveKeySubkeyId.FTUE_HAS_SEEN_DECK_DIFFERENCES_DIALOGUE, 1L));
	}

	private void InitHeroPortaits()
	{
		m_hunterCardDef?.Dispose();
		m_hunterCardDef = DefLoader.Get().GetCardDef("HERO_05");
		if (m_hunterPortraitMesh != null && m_hunterCardDef != null)
		{
			Material portraitMaterial = m_hunterCardDef.CardDef.GetCustomDeckPortrait();
			m_hunterPortraitMesh.SetSharedMaterial(m_hunterPortraitMaterialIndex, portraitMaterial);
		}
		m_mageCardDef?.Dispose();
		m_mageCardDef = DefLoader.Get().GetCardDef("HERO_08");
		if (m_magePortraitMesh != null && m_mageCardDef != null)
		{
			Material portraitMaterial2 = m_mageCardDef.CardDef.GetCustomDeckPortrait();
			m_magePortraitMesh.SetSharedMaterial(m_magePortraitMaterialIndex, portraitMaterial2);
		}
	}
}
