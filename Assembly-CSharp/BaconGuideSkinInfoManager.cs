using System.Collections;
using System.Text;
using Blizzard.T5.AssetManager;
using Hearthstone.UI;
using UnityEngine;

[CustomEditClass]
public class BaconGuideSkinInfoManager : BaconBaseSkinInfoManager
{
	public GameObject m_defaultFrame;

	private GameObject m_frameMesh;

	private Actor m_guideActor;

	private static BaconGuideSkinInfoManager s_instance;

	private static bool s_isReadyingInstance;

	public static BaconGuideSkinInfoManager Get()
	{
		return s_instance;
	}

	public static void EnterPreviewWhenReady(CollectionCardVisual cardVisual)
	{
		BaconGuideSkinInfoManager infoManager = Get();
		if (infoManager != null)
		{
			infoManager.EnterPreview(cardVisual);
			return;
		}
		if (s_isReadyingInstance)
		{
			Debug.LogWarning("BaconGuideSkinInfoManager:EnterPreviewWhenReady called while the info manager instance was being readied");
			return;
		}
		string baconHeroSkinInfoManagerPrefab = "BaconGuideSkinInfoManager.prefab:2201365483a4bd748ab41038e3b56d91";
		Widget widget = WidgetInstance.Create(baconHeroSkinInfoManagerPrefab);
		if (widget == null)
		{
			Debug.LogError("BaconGuideSkinInfoManager:EnterPreviewWhenReady failed to create widget instance");
			return;
		}
		s_isReadyingInstance = true;
		widget.RegisterReadyListener(delegate
		{
			s_instance = widget.GetComponentInChildren<BaconGuideSkinInfoManager>();
			s_isReadyingInstance = false;
			if (s_instance == null)
			{
				Debug.LogError("BaconGuideSkinInfoManager:EnterPreviewWhenReady created widget instance but failed to get BaconGuideSkinInfoManager component");
			}
			else
			{
				s_instance.EnterPreview(cardVisual);
			}
		});
	}

	public static bool IsLoadedAndShowingPreview()
	{
		if (!s_instance)
		{
			return false;
		}
		return s_instance.IsShowingPreview;
	}

	public override void EnterPreview(CollectionCardVisual cardVisual)
	{
		if (m_animating)
		{
			return;
		}
		base.EnterPreview(cardVisual);
		if (m_currentEntityDef != null)
		{
			m_guideActor = cardVisual.GetActor();
			if (m_guideActor != null && m_guideActor.HasCardDef)
			{
				LoadFrameMesh();
				return;
			}
			InstantiateFrameMesh(m_defaultFrame);
			StartCoroutine(WaitForCardDef());
		}
	}

	private void OnDestroy()
	{
		m_currentHeroCardDef?.Dispose();
		m_currentHeroCardDef = null;
		AssetHandle.SafeDispose(ref m_currentHeroGoldenAnimation);
		CancelPreview();
		s_instance = null;
	}

	private IEnumerator WaitForCardDef()
	{
		while (m_guideActor != null && !m_guideActor.HasCardDef)
		{
			yield return null;
		}
		LoadFrameMesh();
	}

	private void LoadFrameMesh()
	{
		if (!(m_guideActor == null))
		{
			DefLoader.DisposableCardDef cardDef = m_guideActor.ShareDisposableCardDef();
			if (cardDef != null && cardDef.CardDef.m_FrameMeshOverride != null)
			{
				InstantiateFrameMesh(cardDef.CardDef.m_FrameMeshOverride);
			}
			else
			{
				InstantiateFrameMesh(m_defaultFrame);
			}
		}
	}

	private void InstantiateFrameMesh(GameObject frameObject)
	{
		if (m_frameMesh != null)
		{
			Object.Destroy(m_frameMesh);
		}
		m_frameMesh = Object.Instantiate(frameObject, m_vanillaHeroFrame.transform);
		m_frameMesh.transform.localPosition = new Vector3(0f, 0.1f, 0f);
		LayerUtils.SetLayer(m_frameMesh, GameLayer.IgnoreFullScreenEffects);
	}

	protected override void PushNavigateBack()
	{
		Navigation.PushUnique(OnNavigateBack);
	}

	protected override void RemoveNavigateBack()
	{
		Navigation.RemoveHandler(OnNavigateBack);
	}

	private static bool OnNavigateBack()
	{
		BaconGuideSkinInfoManager hsim = Get();
		if (hsim != null)
		{
			hsim.CancelPreview();
		}
		return true;
	}

	protected override void ToggleFavoriteSkin()
	{
		if (!CollectionManager.Get().IsBattlegroundsGuideCardId(m_currentEntityDef.GetCardId()))
		{
			return;
		}
		int dbId = GameUtils.TranslateCardIdToDbId(m_currentEntityDef.GetCardId());
		if (CollectionManager.Get().GetBattlegroundsGuideSkinIdForCardId(dbId, out var associatedGuideSkinId))
		{
			if (CollectionManager.Get().OwnsBattlegroundsGuideSkin(dbId))
			{
				Network.Get().SetBattlegroundsFavoriteGuideSkin(associatedGuideSkinId);
			}
		}
		else
		{
			Network.Get().ClearBattlegroundsFavoriteGuideSkin();
		}
	}

	protected override bool CanToggleFavorite()
	{
		return BaconHeroSkinUtils.CanFavoriteBattlegroundsGuideSkin(m_currentEntityDef);
	}

	protected override void AppendDebugTextForCurrentCard(StringBuilder builder)
	{
		base.AppendDebugTextForCurrentCard(builder);
		int dbId = GameUtils.TranslateCardIdToDbId(m_currentEntityDef.GetCardId());
		if (CollectionManager.Get().GetBattlegroundsGuideSkinIdForCardId(dbId, out var guideSkinId))
		{
			builder.Append("Guide Skin Id: ");
			builder.Append(guideSkinId.ToValue());
			builder.AppendLine();
		}
		else
		{
			builder.AppendLine("No Guide Skin Id");
		}
	}
}
