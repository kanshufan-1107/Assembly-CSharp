using System.Collections;
using System.Text;
using Blizzard.T5.AssetManager;
using Hearthstone.UI;
using UnityEngine;

[CustomEditClass]
public class BaconHeroSkinInfoManager : BaconBaseSkinInfoManager
{
	private static readonly Vector3 HERO_POWER_START_SCALE = new Vector3(0.1f, 0.1f, 0.1f);

	private const float HERO_POWER_TWEEN_TIME = 0.5f;

	public GameObject m_heroPowerParent;

	public GameObject m_heroPowerStartBone;

	public GameObject m_defaultFrame;

	private GameObject m_frameMesh;

	private Actor m_heroActor;

	private Actor m_heroPowerActor;

	private static BaconHeroSkinInfoManager s_instance;

	private static bool s_isReadyingInstance;

	protected AudioSource m_previewEmoteAudioSource;

	public static BaconHeroSkinInfoManager Get()
	{
		return s_instance;
	}

	public static void EnterPreviewWhenReady(CollectionCardVisual cardVisual)
	{
		BaconHeroSkinInfoManager infoManager = Get();
		if (infoManager != null)
		{
			infoManager.EnterPreview(cardVisual);
			return;
		}
		if (s_isReadyingInstance)
		{
			Debug.LogWarning("BaconHeroSkinInfoManager:EnterPreviewWhenReady called while the info manager instance was being readied");
			return;
		}
		string baconHeroSkinInfoManagerPrefab = "BaconHeroSkinInfoManager.prefab:5cf5b98d116cb2543b44577a4b5ab97c";
		Widget widget = WidgetInstance.Create(baconHeroSkinInfoManagerPrefab);
		if (widget == null)
		{
			Debug.LogError("BaconHeroSkinInfoManager:EnterPreviewWhenReady failed to create widget instance");
			return;
		}
		s_isReadyingInstance = true;
		widget.RegisterReadyListener(delegate
		{
			s_instance = widget.GetComponentInChildren<BaconHeroSkinInfoManager>();
			s_isReadyingInstance = false;
			if (s_instance == null)
			{
				Debug.LogError("BaconHeroSkinInfoManager:EnterPreviewWhenReady created widget instance but failed to get BaconHeroSkinInfoManager component");
			}
			else
			{
				s_instance.EnterPreview(cardVisual);
			}
		});
	}

	public override void CancelPreview()
	{
		if (m_previewEmoteAudioSource != null)
		{
			SoundManager.Get().FadeTrackOut(m_previewEmoteAudioSource, m_animationTime);
		}
		base.CancelPreview();
	}

	protected override void OnFinishedClosing(Vector3 originalScale)
	{
		m_previewEmoteAudioSource = null;
		base.OnFinishedClosing(originalScale);
	}

	public static bool IsLoadedAndShowingPreview()
	{
		if (!s_instance)
		{
			return false;
		}
		return s_instance.IsShowingPreview;
	}

	private void OnDestroy()
	{
		m_currentHeroCardDef?.Dispose();
		m_currentHeroCardDef = null;
		AssetHandle.SafeDispose(ref m_currentHeroGoldenAnimation);
		CancelPreview();
		s_instance = null;
	}

	public override void EnterPreview(CollectionCardVisual cardVisual)
	{
		if (m_animating)
		{
			return;
		}
		base.EnterPreview(cardVisual);
		if (m_currentEntityDef == null)
		{
			return;
		}
		string heroPowerId = GameUtils.GetHeroPowerCardIdFromHero(m_currentEntityDef.GetCardId());
		SetHeroPower(heroPowerId);
		m_heroActor = cardVisual.GetActor();
		if (m_heroActor != null && m_heroActor.HasCardDef)
		{
			LoadFrameMesh();
			if (m_heroActor != null && m_heroActor.LegendaryHeroSkinConfig != null)
			{
				string line = m_heroActor.LegendaryHeroSkinConfig.GetPickedLine();
				if (line != null)
				{
					SoundManager.Get().LoadAndPlay(line, null, 1f, OnSoundLoaded);
				}
			}
		}
		else
		{
			InstantiateFrameMesh(m_defaultFrame);
			StartCoroutine(WaitForCardDef());
		}
	}

	protected override void Awake()
	{
		base.Awake();
		AssetLoader.Get().InstantiatePrefab("BaconCollectionDetails_HeroPower.prefab:effbe7f7919e2f34b9535d11fe149d0f", OnHeroPowerActorLoaded, null, AssetLoadingOptions.IgnorePrefabPosition);
	}

	private IEnumerator WaitForCardDef()
	{
		while (m_heroActor != null && !m_heroActor.HasCardDef)
		{
			yield return null;
		}
		LoadFrameMesh();
	}

	private void LoadFrameMesh()
	{
		if (!(m_heroActor == null))
		{
			DefLoader.DisposableCardDef cardDef = m_heroActor.ShareDisposableCardDef();
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

	private void OnHeroPowerActorLoaded(AssetReference assetRef, GameObject go, object callbackData)
	{
		if (go == null)
		{
			Log.CollectionManager.PrintError($"CollectionDeckInfo.OnHeroPowerActorLoaded() - FAILED to load actor \"{assetRef}\"");
			return;
		}
		m_heroPowerActor = go.GetComponent<Actor>();
		if (m_heroPowerActor == null)
		{
			Log.CollectionManager.PrintError($"BaconHeroSkinInfoManager.OnHeroPowerActorLoaded() - ERROR actor \"{assetRef}\" has no Actor component");
			return;
		}
		m_heroPowerActor.SetUnlit();
		m_heroPowerActor.transform.parent = m_heroPowerParent.transform;
		RecursivelyCopyLayer(m_heroPowerParent, go);
		m_heroPowerActor.transform.localScale = Vector3.one;
		m_heroPowerActor.transform.localPosition = Vector3.zero;
		go.GetComponent<TokyoDrift>().enabled = true;
		if (UniversalInputManager.Get().IsTouchMode())
		{
			m_heroPowerActor.TurnOffCollider();
		}
	}

	private void RecursivelyCopyLayer(GameObject source, GameObject dest)
	{
		dest.layer = source.layer;
		foreach (Transform child in dest.transform)
		{
			RecursivelyCopyLayer(dest, child.gameObject);
		}
	}

	private void SetHeroPower(string heroPowerCardId)
	{
		DefLoader.Get().LoadFullDef(heroPowerCardId, OnHeroPowerFullDefLoaded);
	}

	private void OnHeroPowerFullDefLoaded(string cardID, DefLoader.DisposableFullDef def, object userData)
	{
		StartCoroutine(SetHeroPowerInfoWhenReady(cardID, def, TAG_PREMIUM.NORMAL));
	}

	private IEnumerator SetHeroPowerInfoWhenReady(string heroPowerCardID, DefLoader.DisposableFullDef def, TAG_PREMIUM premium)
	{
		using (def)
		{
			while (m_heroPowerActor == null)
			{
				yield return null;
			}
			SetHeroPowerInfo(heroPowerCardID, def, premium);
		}
	}

	private void SetHeroPowerInfo(string heroPowerCardID, DefLoader.DisposableFullDef def, TAG_PREMIUM premium)
	{
		m_heroPowerActor.Show();
		m_heroPowerActor.SetFullDef(def);
		m_heroPowerActor.UpdateAllComponents();
		m_heroPowerActor.ActivateSpellBirthState(SpellType.COIN_MANA_GEM);
		m_heroPowerActor.SetUnlit();
		Transform obj = m_heroPowerParent.transform;
		Vector3 startingPosition = obj.localPosition;
		Vector3 startingScale = obj.localScale;
		obj.position = m_heroPowerStartBone.transform.position;
		obj.localScale = HERO_POWER_START_SCALE;
		iTween.MoveTo(m_heroPowerParent, iTween.Hash("position", startingPosition, "islocal", true, "time", 0.5f));
		iTween.ScaleTo(m_heroPowerParent, iTween.Hash("scale", startingScale, "islocal", true, "time", 0.5f));
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
		BaconHeroSkinInfoManager hsim = Get();
		if (hsim != null)
		{
			hsim.CancelPreview();
		}
		return true;
	}

	protected override void ToggleFavoriteSkin()
	{
		bool num = BaconHeroSkinUtils.CanToggleFavoriteBattlegroundsHeroSkin(m_currentEntityDef);
		string miniGuid = m_currentEntityDef.GetCardId();
		if (!num)
		{
			Log.CollectionManager.PrintWarning("BaconHeroSkinInfoManager.ToggleFavoriteSkin() - Can not toggle favorite for " + miniGuid);
			return;
		}
		int baseHeroDbid = GameUtils.TranslateCardIdToDbId(CollectionManager.Get().GetBattlegroundsBaseHeroCardId(miniGuid));
		int cardDbid = GameUtils.TranslateCardIdToDbId(miniGuid);
		int battlegroundsHeroSkinId = BaconHeroSkinUtils.SKIN_ID_FOR_FAVORITED_BASE_HERO;
		if (CollectionManager.Get().IsBattlegroundsHeroSkinCard(cardDbid))
		{
			BattlegroundsHeroSkinDbfRecord battlegroundsHeroSkinRecord = GameDbf.BattlegroundsHeroSkin.GetRecord((BattlegroundsHeroSkinDbfRecord HeroSkin) => HeroSkin.SkinCardId == cardDbid);
			if (battlegroundsHeroSkinRecord == null)
			{
				Log.CollectionManager.PrintWarning($"BaconHeroSkinInfoManager.ToggleFavoriteSkin() - Could not find BattlegroundsHeroSkinDBFRecord for ID: {cardDbid}");
				return;
			}
			battlegroundsHeroSkinId = battlegroundsHeroSkinRecord.ID;
		}
		bool isFavorite = BaconHeroSkinUtils.IsBattlegroundsHeroSkinFavorited(m_currentEntityDef);
		Network.Get().UpdateFavoriteBattlegroundsHeroSkin(baseHeroDbid, battlegroundsHeroSkinId, !isFavorite);
	}

	protected override bool CanToggleFavorite()
	{
		return BaconHeroSkinUtils.CanToggleFavoriteBattlegroundsHeroSkin(m_currentEntityDef);
	}

	protected override void AppendDebugTextForCurrentCard(StringBuilder builder)
	{
		base.AppendDebugTextForCurrentCard(builder);
		int dbId = GameUtils.TranslateCardIdToDbId(m_currentEntityDef.GetCardId());
		if (CollectionManager.Get().GetBattlegroundsHeroSkinIdForSkinCardId(dbId, out var heroSkinId))
		{
			builder.Append("Hero Skin Id: ");
			builder.Append(heroSkinId.ToValue());
			builder.AppendLine();
		}
		else
		{
			builder.AppendLine("No Hero Skin Id");
		}
	}

	private void OnSoundLoaded(AudioSource source, object userData)
	{
		m_previewEmoteAudioSource = source;
	}
}
