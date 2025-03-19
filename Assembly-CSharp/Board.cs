using System;
using System.Collections;
using System.Collections.Generic;
using Blizzard.T5.AssetManager;
using Blizzard.T5.Core.Utils;
using Blizzard.T5.MaterialService.Extensions;
using UnityEngine;

[CustomEditClass]
public class Board : MonoBehaviour
{
	[Serializable]
	public class CustomTraySettings
	{
		public BoardDdId m_Board;

		public Color m_Tint = Color.white;
	}

	[Serializable]
	public class BoardSpecialEvents
	{
		public EventTimingType EventType;

		[CustomEditField(T = EditType.GAME_OBJECT)]
		public string Prefab;

		public Color AmbientColorOverride = Color.white;
	}

	public delegate void AllAssetsLoadedCallback();

	private readonly string[] DEFAULT_BOARD_CLICK_SOUNDS = new string[5] { "board_common_dirt_poke_1.prefab:db7d81ea320f3bb4b9fa44bcd371d379", "board_common_dirt_poke_2.prefab:a078131beb0546444b4ccfc41ec5c547", "board_common_dirt_poke_3.prefab:7fbdaca211c05b94382e3142dfdbb306", "board_common_dirt_poke_4.prefab:d2713c07dcb56904da5ce08da04b5d26", "board_common_dirt_poke_5.prefab:c7234b85b15bca047b7ce32dc96bc851" };

	private const string GOLDEN_HERO_TRAY_FRIENDLY = "HeroTray_Golden_Friendly.prefab:53559bff3e3c2414d8ea4c731e363ff7";

	private const string GOLDEN_HERO_TRAY_OPPONENT = "HeroTray_Golden_Opponent.prefab:073fa61999554054e9cc93c518349e15";

	private const string DECK_SWAP_SPELL_NAME = "ReuseFX_Generic_DeckSwap_AE.prefab:5f5b7d1747ecaee43adbce86abaa87a1";

	private readonly Color MULLIGAN_AMBIENT_LIGHT_COLOR = new Color(0.1607843f, 0.1921569f, 0.282353f, 1f);

	private const float MULLIGAN_LIGHT_INTENSITY = 0f;

	public Color m_AmbientColor = Color.white;

	public Light m_DirectionalLight;

	public float m_DirectionalLightIntensity = 0.275f;

	public GameObject m_FriendlyHeroTray;

	public GameObject m_OpponentHeroTray;

	public GameObject m_FriendlyHeroPhoneTray;

	public GameObject m_OpponentHeroPhoneTray;

	public Transform m_BoneParent;

	public GameObject m_SplitPlaySurface;

	public GameObject m_CombinedPlaySurface;

	public Transform m_ColliderParent;

	public GameObject m_MouseClickDustEffect;

	public Color m_ShadowColor = new Color(0.098f, 0.098f, 0.235f, 0.45f);

	public Color m_DeckColor = Color.white;

	public Color m_EndTurnButtonColor = Color.white;

	public Color m_HistoryTileColor = Color.white;

	public Color m_GoldenHeroTrayColor = Color.white;

	public List<PlayMakerFSM> m_BoardStateChangingObjects;

	public Spell m_leaderboardDamageCapFX;

	public List<BoardSpecialEvents> m_SpecialEvents;

	public MusicPlaylistType m_BoardMusic = MusicPlaylistType.InGame_Default;

	public Texture m_GemManaPhoneTexture;

	public GameObject m_tableTop;

	public GameObject m_frame;

	public GameObject m_playArea;

	public List<GameObject> m_TopLeftBoardAssets;

	public List<GameObject> m_TopRightBoardAssets;

	public List<GameObject> m_BottomLeftBoardAssets;

	public List<GameObject> m_BottomRightBoardAssets;

	private static Board s_instance;

	private bool m_raisedLights;

	private Spell m_FriendlyTraySpellEffect;

	private Spell m_OpponentTraySpellEffect;

	private Spell m_deckSwapSpellEffect;

	private int m_boardDbId;

	private Color m_TrayTint = Color.white;

	private AssetHandle<Texture> m_friendlyHeroTrayTexture;

	private AssetHandle<Texture> m_friendlyHeroPhoneTrayTexture;

	private AssetHandle<Texture> m_opponentHeroTrayTexture;

	private AssetHandle<Texture> m_opponentHeroPhoneTrayTexture;

	private Pool<GameObject> m_pooledDustEffects;

	private Dictionary<int, ParticleSystem[]> m_cachedParticleSystems;

	private const int MAX_DUST_VFX = 10;

	protected AllAssetsLoadedCallback m_AllAssetsLoadedCallback;

	private void Awake()
	{
		s_instance = this;
		LoadingScreen.Get()?.NotifyMainSceneObjectAwoke(base.gameObject);
		ValidateInspectorReferences();
		InitDustEffectsCache();
	}

	protected virtual void OnDestroy()
	{
		if (m_pooledDustEffects != null)
		{
			m_pooledDustEffects.ReleaseAll();
			m_pooledDustEffects.Clear();
		}
		m_cachedParticleSystems?.Clear();
		if (s_instance == this)
		{
			s_instance = null;
		}
		AssetHandle.SafeDispose(ref m_friendlyHeroTrayTexture);
		AssetHandle.SafeDispose(ref m_friendlyHeroPhoneTrayTexture);
		AssetHandle.SafeDispose(ref m_opponentHeroTrayTexture);
		AssetHandle.SafeDispose(ref m_opponentHeroPhoneTrayTexture);
	}

	public virtual void Start()
	{
		ProjectedShadow.SetShadowColor(m_ShadowColor);
		float[] shadowCullDistances = new float[32];
		shadowCullDistances[14] = 0.1f;
		m_DirectionalLight.layerShadowCullDistances = shadowCullDistances;
		Animation animation = GetComponent<Animation>();
		if (animation != null)
		{
			string clipName = animation.clip.name;
			animation[clipName].normalizedTime = 0.25f;
			animation[clipName].speed = -3f;
			animation.Play(clipName);
		}
		StartCoroutine(GoldenHeroes());
		if (GameMgr.Get() == null || GameMgr.Get().IsTraditionalTutorial())
		{
			return;
		}
		foreach (BoardSpecialEvents boardSpecialEvent in m_SpecialEvents)
		{
			if (EventTimingManager.Get().IsEventActive(boardSpecialEvent.EventType))
			{
				LoadBoardSpecialEvent(boardSpecialEvent);
			}
		}
	}

	public static Board Get()
	{
		return s_instance;
	}

	public void SetBoardDbId(int id)
	{
		m_boardDbId = id;
	}

	public virtual bool AreAllAssetsLoaded()
	{
		return true;
	}

	public void RegisterAllAssetsLoadedCallback(AllAssetsLoadedCallback callback)
	{
		m_AllAssetsLoadedCallback = callback;
	}

	public void ResetAmbientColor()
	{
		RenderSettings.ambientLight = m_AmbientColor;
	}

	[ContextMenu("RaiseTheLights")]
	public void RaiseTheLights()
	{
		RaiseTheLights(1f);
	}

	public void RaiseTheLightsQuickly()
	{
		RaiseTheLights(5f);
	}

	public void RaiseTheLights(float speed)
	{
		if (!m_raisedLights)
		{
			float animTime = 3f / speed;
			Action<object> ambientUpdate = delegate(object amount)
			{
				RenderSettings.ambientLight = (Color)amount;
			};
			Hashtable cArgs = iTweenManager.Get().GetTweenHashTable();
			cArgs.Add("from", RenderSettings.ambientLight);
			cArgs.Add("to", m_AmbientColor);
			cArgs.Add("time", animTime);
			cArgs.Add("easetype", iTween.EaseType.easeInOutQuad);
			cArgs.Add("onupdate", ambientUpdate);
			cArgs.Add("onupdatetarget", base.gameObject);
			iTween.ValueTo(base.gameObject, cArgs);
			Action<object> lightIntensityUpdate = delegate(object amount)
			{
				m_DirectionalLight.intensity = (float)amount;
			};
			Hashtable iArgs = iTweenManager.Get().GetTweenHashTable();
			iArgs.Add("from", m_DirectionalLight.intensity);
			iArgs.Add("to", m_DirectionalLightIntensity);
			iArgs.Add("time", animTime);
			iArgs.Add("easetype", iTween.EaseType.easeInOutQuad);
			iArgs.Add("onupdate", lightIntensityUpdate);
			iArgs.Add("onupdatetarget", base.gameObject);
			iTween.ValueTo(base.gameObject, iArgs);
			m_raisedLights = true;
			if (GameMgr.Get().IsTraditionalTutorial())
			{
				Gameplay.Get().DoTraditionalTutorialShow();
			}
		}
	}

	public void SetMulliganLighting()
	{
		RenderSettings.ambientLight = MULLIGAN_AMBIENT_LIGHT_COLOR;
		m_DirectionalLight.intensity = 0f;
	}

	public void DimTheLights()
	{
		DimTheLights(5f);
	}

	public void DimTheLights(float speed)
	{
		if (m_raisedLights)
		{
			float animTime = 3f / speed;
			Action<object> ambientUpdate = delegate(object amount)
			{
				RenderSettings.ambientLight = (Color)amount;
			};
			Hashtable cArgs = iTweenManager.Get().GetTweenHashTable();
			cArgs.Add("from", RenderSettings.ambientLight);
			cArgs.Add("to", MULLIGAN_AMBIENT_LIGHT_COLOR);
			cArgs.Add("time", animTime);
			cArgs.Add("easetype", iTween.EaseType.easeInOutQuad);
			cArgs.Add("onupdate", ambientUpdate);
			cArgs.Add("onupdatetarget", base.gameObject);
			iTween.ValueTo(base.gameObject, cArgs);
			Action<object> lightIntensityUpdate = delegate(object amount)
			{
				m_DirectionalLight.intensity = (float)amount;
			};
			Hashtable iArgs = iTweenManager.Get().GetTweenHashTable();
			iArgs.Add("from", m_DirectionalLight.intensity);
			iArgs.Add("to", 0f);
			iArgs.Add("time", animTime);
			iArgs.Add("easetype", iTween.EaseType.easeInOutQuad);
			iArgs.Add("onupdate", lightIntensityUpdate);
			iArgs.Add("onupdatetarget", base.gameObject);
			iTween.ValueTo(base.gameObject, iArgs);
			m_raisedLights = false;
		}
	}

	public Transform FindBone(string name)
	{
		if (m_BoneParent != null)
		{
			Transform bone = m_BoneParent.Find(name);
			if (bone != null)
			{
				return bone;
			}
		}
		if (Gameplay.Get() != null && Gameplay.Get().GetBoardLayout() != null)
		{
			return Gameplay.Get().GetBoardLayout().FindBone(name);
		}
		return null;
	}

	public Collider FindCollider(string name)
	{
		if (m_ColliderParent != null)
		{
			Transform t = m_ColliderParent.Find(name);
			if (t != null)
			{
				if (!(t == null))
				{
					return t.GetComponent<Collider>();
				}
				return null;
			}
		}
		return Gameplay.Get().GetBoardLayout().FindCollider(name);
	}

	public GameObject GetMouseClickDustEffectPrefab()
	{
		return m_MouseClickDustEffect;
	}

	public void CombinedSurface()
	{
		if (m_CombinedPlaySurface != null && m_SplitPlaySurface != null)
		{
			m_CombinedPlaySurface.SetActive(value: true);
			m_SplitPlaySurface.SetActive(value: false);
		}
	}

	public void SplitSurface()
	{
		if (m_CombinedPlaySurface != null && m_SplitPlaySurface != null)
		{
			m_CombinedPlaySurface.SetActive(value: false);
			m_SplitPlaySurface.SetActive(value: true);
		}
	}

	public Spell GetFriendlyTraySpell()
	{
		return m_FriendlyTraySpellEffect;
	}

	public Spell GetOpponentTraySpell()
	{
		return m_OpponentTraySpellEffect;
	}

	public virtual void ChangeBoardVisualState(TAG_BOARD_VISUAL_STATE boardState)
	{
		if (m_BoardStateChangingObjects == null || m_BoardStateChangingObjects.Count == 0)
		{
			return;
		}
		foreach (PlayMakerFSM boardStateChangingObject in m_BoardStateChangingObjects)
		{
			boardStateChangingObject.SetState(EnumUtils.GetString(boardState));
		}
	}

	public void ReturnDisabledDustVFX(GameObject dustVFX)
	{
		m_pooledDustEffects.Release(dustVFX);
	}

	public void BoardClicked(RaycastHit hitInfo)
	{
		if (m_MouseClickDustEffect == null)
		{
			return;
		}
		GameState gameState = GameState.Get();
		if (gameState == null || gameState.IsMulliganManagerActive())
		{
			return;
		}
		GameObject dustEffect = m_pooledDustEffects.Acquire();
		if (dustEffect == null)
		{
			return;
		}
		dustEffect.transform.position = hitInfo.point;
		if (!m_cachedParticleSystems.TryGetValue(dustEffect.GetInstanceID(), out var particles))
		{
			return;
		}
		Vector3 force = new Vector3(Input.GetAxis("Mouse Y") * 40f, Input.GetAxis("Mouse X") * 40f, 0f);
		int i = 0;
		for (int iMax = particles.Length; i < iMax; i++)
		{
			ParticleSystem particleSystem = particles[i];
			if (particleSystem.name == "Rocks")
			{
				particleSystem.transform.localRotation = Quaternion.Euler(force);
			}
			particleSystem.Play();
		}
		string[] boardClickSounds = null;
		GameEntity gameEntity = gameState.GetGameEntity();
		if (gameEntity != null)
		{
			boardClickSounds = gameEntity.GetOverrideBoardClickSounds();
		}
		if (boardClickSounds == null || boardClickSounds.Length == 0)
		{
			boardClickSounds = DEFAULT_BOARD_CLICK_SOUNDS;
		}
		string boardClickSoundPrefab = boardClickSounds[UnityEngine.Random.Range(0, boardClickSounds.Length)];
		SoundManager.Get().LoadAndPlay(boardClickSoundPrefab, dustEffect);
	}

	public void PlayDeckSwapSpell()
	{
		if (m_deckSwapSpellEffect != null)
		{
			SpellManager.Get().ReleaseSpell(m_deckSwapSpellEffect);
			m_deckSwapSpellEffect = null;
		}
		m_deckSwapSpellEffect = SpellManager.Get().GetSpell("ReuseFX_Generic_DeckSwap_AE.prefab:5f5b7d1747ecaee43adbce86abaa87a1");
		if (m_deckSwapSpellEffect != null)
		{
			m_deckSwapSpellEffect.Activate();
		}
	}

	public virtual void UpdateCustomHeroTray(Player.Side side)
	{
		StartCoroutine(UpdateHeroTray(side, isGolden: true));
	}

	protected virtual void ValidateInspectorReferences()
	{
		if (m_FriendlyHeroTray == null)
		{
			Debug.LogError("Friendly Hero Tray is not assigned!");
		}
		if (m_OpponentHeroTray == null)
		{
			Debug.LogError("Opponent Hero Tray is not assigned!");
		}
	}

	private void InitDustEffectsCache()
	{
		if (!(m_MouseClickDustEffect == null))
		{
			m_pooledDustEffects = new Pool<GameObject>();
			m_pooledDustEffects.SetCreateItemCallback(CreateDustEffect);
			m_pooledDustEffects.SetDestroyItemCallback(DestroyDustEffect);
			m_pooledDustEffects.SetExtensionCount(0);
			m_pooledDustEffects.SetMaxReleasedItemCount(10);
			m_cachedParticleSystems = new Dictionary<int, ParticleSystem[]>();
			m_pooledDustEffects.AddFreeItems(10);
		}
	}

	private GameObject CreateDustEffect(int i)
	{
		GameObject dustEffect = UnityEngine.Object.Instantiate(m_MouseClickDustEffect);
		m_cachedParticleSystems.Add(dustEffect.GetInstanceID(), dustEffect.GetComponentsInChildren<ParticleSystem>());
		return dustEffect;
	}

	private void DestroyDustEffect(GameObject dustEffect)
	{
		if (dustEffect != null)
		{
			UnityEngine.Object.Destroy(dustEffect);
		}
	}

	private IEnumerator GoldenHeroes()
	{
		bool friendlyHeroIsGolden = false;
		bool opposingHeroIsGolden = false;
		GameState gameState = GameState.Get();
		while (gameState == null)
		{
			gameState = GameState.Get();
			yield return null;
		}
		Player friendlyPlayer = gameState.GetFriendlySidePlayer();
		while (friendlyPlayer == null)
		{
			friendlyPlayer = gameState.GetFriendlySidePlayer();
			yield return null;
		}
		Player opposingPlayer = gameState.GetOpposingSidePlayer();
		Card friendlyHeroCard = friendlyPlayer.GetHeroCard();
		while (friendlyHeroCard == null)
		{
			friendlyHeroCard = friendlyPlayer.GetHeroCard();
			yield return null;
		}
		Card opposingHeroCard = opposingPlayer.GetHeroCard();
		while (opposingHeroCard == null)
		{
			opposingHeroCard = opposingPlayer.GetHeroCard();
			yield return null;
		}
		while (friendlyHeroCard.GetEntity() == null)
		{
			yield return null;
		}
		while (opposingHeroCard.GetEntity() == null)
		{
			yield return null;
		}
		if (friendlyHeroCard.GetPremium() == TAG_PREMIUM.GOLDEN)
		{
			friendlyHeroIsGolden = true;
		}
		if (opposingHeroCard.GetPremium() == TAG_PREMIUM.GOLDEN)
		{
			opposingHeroIsGolden = true;
		}
		if (friendlyHeroIsGolden && !friendlyHeroCard.DisablePremiumHeroTray)
		{
			AssetLoader.Get().InstantiatePrefab("HeroTray_Golden_Friendly.prefab:53559bff3e3c2414d8ea4c731e363ff7", ShowFriendlyHeroTray);
		}
		else if (friendlyHeroCard.HeroFrameFriendlyPath != null && friendlyHeroCard.HeroFrameFriendlyPath.Length > 0)
		{
			AssetLoader.Get().InstantiatePrefab(friendlyHeroCard.HeroFrameFriendlyPath, ShowFriendlyHeroTray);
		}
		else
		{
			StartCoroutine(UpdateHeroTray(Player.Side.FRIENDLY, isGolden: false));
		}
		if (opposingHeroIsGolden && !opposingHeroCard.DisablePremiumHeroTray)
		{
			AssetLoader.Get().InstantiatePrefab("HeroTray_Golden_Opponent.prefab:073fa61999554054e9cc93c518349e15", ShowOpponentHeroTray);
		}
		else if (opposingHeroCard.HeroFrameEnemyPath != null && opposingHeroCard.HeroFrameEnemyPath.Length > 0)
		{
			AssetLoader.Get().InstantiatePrefab(opposingHeroCard.HeroFrameEnemyPath, ShowOpponentHeroTray);
		}
		else
		{
			StartCoroutine(UpdateHeroTray(Player.Side.OPPOSING, isGolden: false));
		}
	}

	protected virtual void ShowFriendlyHeroTray(AssetReference assetRef, GameObject go, object callbackData)
	{
		go.transform.position = ZoneMgr.Get().FindZoneOfType<ZoneHero>(Player.Side.FRIENDLY).OriginalPosition;
		go.SetActive(value: true);
		Renderer[] componentsInChildren = go.GetComponentsInChildren<Renderer>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			componentsInChildren[i].GetMaterial().color = m_GoldenHeroTrayColor;
		}
		UnityEngine.Object.Destroy(m_FriendlyHeroTray);
		m_FriendlyHeroTray = go;
		StartCoroutine(UpdateHeroTray(Player.Side.FRIENDLY, isGolden: true));
	}

	private void ShowOpponentHeroTray(AssetReference assetRef, GameObject go, object callbackData)
	{
		go.transform.position = ZoneMgr.Get().FindZoneOfType<ZoneHero>(Player.Side.OPPOSING).OriginalPosition;
		go.SetActive(value: true);
		Renderer[] componentsInChildren = go.GetComponentsInChildren<Renderer>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			componentsInChildren[i].GetMaterial().color = m_GoldenHeroTrayColor;
		}
		if ((bool)m_OpponentHeroTray)
		{
			m_OpponentHeroTray.SetActive(value: false);
			UnityEngine.Object.Destroy(m_OpponentHeroTray);
		}
		m_OpponentHeroTray = go;
		StartCoroutine(UpdateHeroTray(Player.Side.OPPOSING, isGolden: true));
	}

	private IEnumerator UpdateHeroTray(Player.Side side, bool isGolden)
	{
		while (GameState.Get().GetPlayerMap().Count == 0)
		{
			yield return null;
		}
		Player p = null;
		while (p == null)
		{
			foreach (Player player in GameState.Get().GetPlayerMap().Values)
			{
				if (player.GetSide() == side)
				{
					p = player;
					break;
				}
			}
			yield return null;
		}
		while (p.GetHero() == null)
		{
			yield return null;
		}
		Entity hero = p.GetHero();
		while (hero.IsLoadingAssets())
		{
			yield return null;
		}
		while (hero.GetCard() == null)
		{
			yield return null;
		}
		Card heroCard = hero.GetCard();
		while (!heroCard.HasCardDef)
		{
			yield return null;
		}
		yield return ApplyHeroTrayFromCard(heroCard, side);
	}

	protected IEnumerator ApplyHeroTrayFromCard(Card card, Player.Side side, bool isFromOfflineRequest = false)
	{
		if ((bool)UniversalInputManager.UsePhoneUI)
		{
			while (!isFromOfflineRequest && ManaCrystalMgr.Get() == null)
			{
				yield return null;
			}
			if (side == Player.Side.FRIENDLY)
			{
				if (!string.IsNullOrEmpty(card.CustomHeroPhoneManaGem))
				{
					AssetLoader.Get().LoadAsset<Texture>(card.CustomHeroPhoneManaGem, OnHeroSkinManaGemTextureLoaded);
				}
				else if (m_GemManaPhoneTexture != null && ManaCrystalMgr.Get() != null)
				{
					ManaCrystalMgr.Get().SetFriendlyManaGemTexture(new AssetHandle<Texture>(m_GemManaPhoneTexture.name, m_GemManaPhoneTexture));
				}
			}
		}
		for (int i = 0; i < card.CustomHeroTraySettings.Count; i++)
		{
			if (m_boardDbId == (int)card.CustomHeroTraySettings[i].m_Board)
			{
				m_TrayTint = card.CustomHeroTraySettings[i].m_Tint;
			}
		}
		if (!string.IsNullOrEmpty(card.CustomHeroTray))
		{
			while (card.GetActor() == null)
			{
				yield return null;
			}
			if (card.GetActor().GetPremium() == TAG_PREMIUM.GOLDEN && !string.IsNullOrEmpty(card.CustomHeroTrayGolden))
			{
				AssetLoader.Get().LoadAsset<Texture>(card.CustomHeroTrayGolden, OnHeroTrayTextureLoaded, side);
			}
			else
			{
				AssetLoader.Get().LoadAsset<Texture>(card.CustomHeroTray, OnHeroTrayTextureLoaded, side);
			}
		}
		if ((bool)UniversalInputManager.UsePhoneUI && !string.IsNullOrEmpty(card.CustomHeroPhoneTray))
		{
			AssetLoader.Get().LoadAsset<Texture>(card.CustomHeroPhoneTray, OnHeroPhoneTrayTextureLoaded, side);
		}
	}

	protected virtual void OnHeroSkinManaGemTextureLoaded(AssetReference assetRef, AssetHandle<Texture> texture, object callbackData)
	{
		using (texture)
		{
			if (!texture)
			{
				Debug.LogError("OnHeroSkinManaGemTextureLoaded() loaded texture is null!");
				return;
			}
			ManaCrystalMgr.Get().SetFriendlyManaGemTexture(texture);
			ManaCrystalMgr.Get().SetFriendlyManaGemTint(m_TrayTint);
		}
	}

	private void OnHeroTrayTextureLoaded(AssetReference assetRef, AssetHandle<Texture> texture, object callbackData)
	{
		using (texture)
		{
			if (!texture)
			{
				Debug.LogError("Board.OnHeroTrayTextureLoaded() loaded texture is null!");
			}
			else if ((Player.Side)callbackData == Player.Side.FRIENDLY)
			{
				AssetHandle.Set(ref m_friendlyHeroTrayTexture, texture);
				Material material = m_FriendlyHeroTray.GetComponentInChildren<MeshRenderer>().GetMaterial();
				material.mainTexture = m_friendlyHeroTrayTexture;
				material.color = m_TrayTint;
			}
			else
			{
				AssetHandle.Set(ref m_opponentHeroTrayTexture, texture);
				Material material2 = m_OpponentHeroTray.GetComponentInChildren<MeshRenderer>().GetMaterial();
				material2.mainTexture = m_opponentHeroTrayTexture;
				material2.color = m_TrayTint;
			}
		}
	}

	private void OnHeroPhoneTrayTextureLoaded(AssetReference assetRef, AssetHandle<Texture> texture, object callbackData)
	{
		using (texture)
		{
			if (!texture)
			{
				Debug.LogError("Board.OnHeroTrayTextureLoaded() loaded texture is null!");
			}
			else if ((Player.Side)callbackData == Player.Side.FRIENDLY)
			{
				if (m_FriendlyHeroPhoneTray == null)
				{
					Debug.LogWarning("Friendly Hero Phone Tray Object on Board is null!");
					return;
				}
				AssetHandle.Set(ref m_friendlyHeroPhoneTrayTexture, texture);
				Material material = m_FriendlyHeroPhoneTray.GetComponentInChildren<MeshRenderer>().GetMaterial();
				material.mainTexture = m_friendlyHeroPhoneTrayTexture;
				material.color = m_TrayTint;
			}
			else if (m_OpponentHeroPhoneTray == null)
			{
				Debug.LogWarning("Opponent Hero Phone Tray Object on Board is null!");
			}
			else
			{
				AssetHandle.Set(ref m_opponentHeroPhoneTrayTexture, texture);
				Material material2 = m_OpponentHeroPhoneTray.GetComponentInChildren<MeshRenderer>().GetMaterial();
				material2.mainTexture = m_opponentHeroPhoneTrayTexture;
				material2.color = m_TrayTint;
			}
		}
	}

	private void OnHeroTrayEffectLoaded(AssetReference assetRef, GameObject go, object callbackData)
	{
		if (go == null)
		{
			Debug.LogError("Board.OnHeroTrayEffectLoaded() Hero tray effect is null!");
			return;
		}
		Spell traySpell = go.GetComponent<Spell>();
		if (traySpell == null)
		{
			Debug.LogError("Board.OnHeroTrayEffectLoaded() Hero tray effect: could not find spell component!");
		}
		else if ((Player.Side)callbackData == Player.Side.FRIENDLY)
		{
			go.transform.parent = base.transform;
			go.transform.position = FindBone("CustomSocketIn_Friendly").position;
			m_FriendlyTraySpellEffect = traySpell;
		}
		else
		{
			go.transform.parent = base.transform;
			go.transform.position = FindBone("CustomSocketIn_Opposing").position;
			m_OpponentTraySpellEffect = traySpell;
		}
	}

	private void LoadBoardSpecialEvent(BoardSpecialEvents boardSpecialEvent)
	{
		if (AssetLoader.Get().InstantiatePrefab(boardSpecialEvent.Prefab) == null)
		{
			Debug.LogWarning($"Failed to load special board event: {boardSpecialEvent.Prefab}");
		}
		m_AmbientColor = boardSpecialEvent.AmbientColorOverride;
	}

	public bool IsCornerReplacementCompatible()
	{
		string dummy;
		return IsCornerReplacementCompatible(out dummy);
	}

	public bool IsCornerReplacementCompatible(out string reason)
	{
		if (m_TopLeftBoardAssets.Count == 0 && m_TopRightBoardAssets.Count == 0 && m_BottomLeftBoardAssets.Count == 0 && m_BottomRightBoardAssets.Count == 0)
		{
			reason = "Assets not set to corners";
			return false;
		}
		if (!IsTableTopMaterialValid())
		{
			reason = "Invalid Table Top Material";
			return false;
		}
		if (!IsFrameMaterialValid())
		{
			reason = "Invalid Frame Material";
			return false;
		}
		if (!ArePlayAreaMaterialsValid())
		{
			reason = "Invalid Play Area Materials on children";
			return false;
		}
		reason = "";
		return true;
	}

	public void DisableCorner(CornerReplacementPosition corner)
	{
		switch (corner)
		{
		case CornerReplacementPosition.TOP_LEFT:
			ToggleTopLeft(active: false);
			break;
		case CornerReplacementPosition.TOP_RIGHT:
			ToggleTopRight(active: false);
			break;
		case CornerReplacementPosition.BOTTOM_LEFT:
			ToggleBottomLeft(active: false);
			break;
		case CornerReplacementPosition.BOTTOM_RIGHT:
			ToggleBottomRight(active: false);
			break;
		}
	}

	private bool IsTableTopMaterialValid()
	{
		if (m_tableTop == null)
		{
			return false;
		}
		MeshRenderer tableTopMeshRenderer = m_tableTop.GetComponent<MeshRenderer>();
		if (tableTopMeshRenderer == null)
		{
			return false;
		}
		Material tableTopMaterial = tableTopMeshRenderer.GetSharedMaterial();
		if (tableTopMaterial == null)
		{
			return false;
		}
		if (!tableTopMaterial.HasTexture("_BotTex") || !tableTopMaterial.HasTexture("_TopTex"))
		{
			return false;
		}
		return true;
	}

	public void SetTableTopTexture(Texture tex, Player.Side side)
	{
		if (IsTableTopMaterialValid())
		{
			Material tableTopMaterial = m_tableTop.GetComponent<MeshRenderer>().GetMaterial();
			switch (side)
			{
			case Player.Side.FRIENDLY:
				tableTopMaterial.SetTexture("_BotTex", tex);
				break;
			case Player.Side.OPPOSING:
				tableTopMaterial.SetTexture("_TopTex", tex);
				break;
			}
		}
	}

	private bool IsFrameMaterialValid()
	{
		if (m_frame == null)
		{
			return false;
		}
		MeshRenderer frameMeshRenderer = m_frame.GetComponent<MeshRenderer>();
		if (frameMeshRenderer == null)
		{
			return false;
		}
		Material frameMaterial = frameMeshRenderer.GetSharedMaterial();
		if (frameMaterial == null)
		{
			return false;
		}
		if (!frameMaterial.HasTexture("_BotTex") || !frameMaterial.HasTexture("_TopTex"))
		{
			return false;
		}
		return true;
	}

	public void SetFrameTexture(Texture tex, Player.Side side)
	{
		if (IsFrameMaterialValid())
		{
			Material frameMaterial = m_frame.GetComponent<MeshRenderer>().GetMaterial();
			switch (side)
			{
			case Player.Side.FRIENDLY:
				frameMaterial.SetTexture("_BotTex", tex);
				break;
			case Player.Side.OPPOSING:
				frameMaterial.SetTexture("_TopTex", tex);
				break;
			}
		}
	}

	private bool ArePlayAreaMaterialsValid()
	{
		if (m_playArea == null)
		{
			return false;
		}
		MeshRenderer[] playAreaMeshRenderers = m_playArea.GetComponentsInChildren<MeshRenderer>();
		if (playAreaMeshRenderers.Length == 0)
		{
			return false;
		}
		MeshRenderer[] array = playAreaMeshRenderers;
		for (int i = 0; i < array.Length; i++)
		{
			Material playAreaMaterial = array[i].GetSharedMaterial();
			if (playAreaMaterial == null)
			{
				return false;
			}
			if (!playAreaMaterial.HasTexture("_MainTex"))
			{
				return false;
			}
		}
		return true;
	}

	public void SetPlayAreaTexture(Texture tex, Texture maskTex, Player.Side side)
	{
		if (!ArePlayAreaMaterialsValid())
		{
			return;
		}
		MeshRenderer[] componentsInChildren = m_playArea.GetComponentsInChildren<MeshRenderer>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			Material playAreaMaterial = componentsInChildren[i].GetMaterial();
			switch (side)
			{
			case Player.Side.FRIENDLY:
				playAreaMaterial.SetTexture("_FriendlyTex", tex);
				playAreaMaterial.SetTexture("_FriendlyMaskTex", maskTex);
				break;
			case Player.Side.OPPOSING:
				playAreaMaterial.SetTexture("_EnemyTex", tex);
				playAreaMaterial.SetTexture("_EnemyMaskTex", maskTex);
				break;
			}
		}
	}

	public void ToggleTopLeft(bool active)
	{
		foreach (GameObject boardObject in m_TopLeftBoardAssets)
		{
			if (boardObject != null)
			{
				boardObject.SetActive(active);
			}
		}
	}

	public void ToggleTopRight(bool active)
	{
		foreach (GameObject boardObject in m_TopRightBoardAssets)
		{
			if (boardObject != null)
			{
				boardObject.SetActive(active);
			}
		}
	}

	public void ToggleBottomLeft(bool active)
	{
		foreach (GameObject boardObject in m_BottomLeftBoardAssets)
		{
			if (boardObject != null)
			{
				boardObject.SetActive(active);
			}
		}
	}

	public void ToggleBottomRight(bool active)
	{
		foreach (GameObject boardObject in m_BottomRightBoardAssets)
		{
			if (boardObject != null)
			{
				boardObject.SetActive(active);
			}
		}
	}
}
