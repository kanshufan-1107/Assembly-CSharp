using System;
using System.Collections;
using Hearthstone.DataModels;
using Hearthstone.UI;
using UnityEngine;

public class MercenariesZoneUnlockDialog : DialogBase
{
	public class Info
	{
		public int m_zoneId;

		public Action m_onCompleteCallback;
	}

	public AsyncReference m_rootWidgetReference;

	public PegUIElement m_clickCatcher;

	private Info m_info;

	private Widget m_rootWidget;

	private LettuceLobbyChooserButton m_zoneButton;

	private void Start()
	{
		m_rootWidgetReference.RegisterReadyListener(delegate(Widget w)
		{
			m_rootWidget = w;
		});
		m_clickCatcher.AddEventListener(UIEventType.RELEASE, OnClickCatcherReleased);
	}

	public void SetInfo(Info info)
	{
		m_info = info;
	}

	public override void Show()
	{
		StartCoroutine(ShowWhenReady());
	}

	private IEnumerator ShowWhenReady()
	{
		while (m_rootWidget == null || !m_rootWidget.IsReady)
		{
			yield return null;
		}
		LettuceBountySetDbfRecord bountySetRecord = GameDbf.LettuceBountySet.GetRecord(m_info.m_zoneId);
		if (bountySetRecord != null)
		{
			LettuceZoneUnlockDataModel dataModel = new LettuceZoneUnlockDataModel
			{
				FooterText = bountySetRecord.UnlockPopupText,
				ZoneNameText = bountySetRecord.Name
			};
			bool textureLoaded = false;
			if (!string.IsNullOrEmpty(bountySetRecord.TileArtTexture))
			{
				ObjectCallback callback = delegate(AssetReference assetRef, UnityEngine.Object obj, object callbackData)
				{
					dataModel.ZoneTexture = obj as Texture;
					textureLoaded = true;
				};
				if (!AssetLoader.Get().LoadTexture(bountySetRecord.TileArtTexture, callback))
				{
					callback(bountySetRecord.TileArtTexture, null, null);
				}
			}
			while (!textureLoaded)
			{
				yield return null;
			}
			m_rootWidget.BindDataModel(dataModel);
			while (m_rootWidget.IsChangingStates)
			{
				yield return null;
			}
			base.Show();
			DoShowAnimation();
			DialogBase.DoBlur();
			UniversalInputManager.Get().SetGameDialogActive(active: true);
		}
		else
		{
			Debug.LogError("Zone unlock dialog attempted to show invalid zone with id: " + m_info.m_zoneId);
			base.Hide();
			m_info.m_onCompleteCallback?.Invoke();
		}
	}

	public override void Hide()
	{
		base.Hide();
		DialogBase.EndBlur();
		m_info.m_onCompleteCallback?.Invoke();
		UniversalInputManager.Get().SetGameDialogActive(active: false);
	}

	private void OnClickCatcherReleased(UIEvent e)
	{
		Hide();
	}
}
