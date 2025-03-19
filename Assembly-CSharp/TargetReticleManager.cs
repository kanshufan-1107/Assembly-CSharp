using System.Collections;
using System.Collections.Generic;
using Blizzard.T5.MaterialService.Extensions;
using UnityEngine;

public class TargetReticleManager : MonoBehaviour
{
	private const int MAX_TARGET_ARROW_LINKS = 15;

	private const float LINK_Y_LENGTH = 1f;

	private const float LENGTH_BETWEEN_LINKS = 1.2f;

	private const float LINK_PARABOLA_HEIGHT_NORMAL = 1.5f;

	public const float LINK_PARABOLA_HEIGHT_MERCENARIES = 0.4f;

	private const float LINK_ANIMATION_SPEED = 0.5f;

	private const float STARTING_X_ROTATION_FOR_DEFAULT_ARROW = 300f;

	private static readonly PlatformDependentValue<bool> SHOW_DAMAGE_INDICATOR_ON_ENTITY = new PlatformDependentValue<bool>(PlatformCategory.Input)
	{
		Mouse = false,
		Touch = true
	};

	private static readonly PlatformDependentValue<float> DAMAGE_INDICATOR_SCALE = new PlatformDependentValue<float>(PlatformCategory.Screen)
	{
		PC = 2.5f,
		Tablet = 3.75f
	};

	private static readonly PlatformDependentValue<float> DAMAGE_INDICATOR_Z_OFFSET = new PlatformDependentValue<float>(PlatformCategory.Screen)
	{
		PC = 0.75f,
		Tablet = -1.2f
	};

	private const float FRIENDLY_HERO_ORIGIN_Z_OFFSET = 1f;

	private const float LINK_FADE_OFFSET = -1.2f;

	private static TargetReticleManager s_instance;

	private bool m_isActive;

	private bool m_showArrow = true;

	private bool m_bullseyeAlwaysOn;

	private Entity m_originLocationEntity;

	private int m_sourceEntityID = -1;

	private int m_numActiveLinks;

	private float m_linkAnimationZOffset;

	private float m_parabolaHeight = 1.5f;

	private Vector3 m_targetArrowOrigin;

	private Vector3 m_remoteArrowPosition;

	private GameObject m_arrow;

	private TargetDamageIndicator m_damageIndicator;

	private GameObject m_hunterReticle;

	private GameObject m_questionMark;

	private GameObject m_gruntyReticle;

	private List<GameObject> m_targetArrowLinks;

	private TARGET_RETICLE_TYPE m_reticleType;

	private TARGET_ARROW_TYPE m_targetArrowType;

	private bool m_useHandAsOrigin;

	public int ArrowSourceEntityID
	{
		get
		{
			if (m_originLocationEntity != null)
			{
				return m_originLocationEntity.GetEntityId();
			}
			return 0;
		}
	}

	public Vector3 LocalTargetArrowPosition { get; private set; }

	public Vector3 RemoteTargetArrowPosition => m_remoteArrowPosition;

	public int SourceEntityID => m_sourceEntityID;

	private void Awake()
	{
		s_instance = this;
	}

	private void OnDestroy()
	{
		s_instance = null;
	}

	public static TargetReticleManager Get()
	{
		return s_instance;
	}

	public bool IsActive()
	{
		if (GetAppropriateReticle() != null)
		{
			return m_isActive;
		}
		return false;
	}

	public bool IsLocalArrow()
	{
		return m_targetArrowType != TARGET_ARROW_TYPE.Enemy;
	}

	public bool IsEnemyArrow()
	{
		return m_targetArrowType == TARGET_ARROW_TYPE.Enemy;
	}

	public bool IsStaticArrow()
	{
		return m_targetArrowType == TARGET_ARROW_TYPE.Static;
	}

	public bool IsLocalArrowActive()
	{
		if (m_targetArrowType == TARGET_ARROW_TYPE.Enemy)
		{
			return false;
		}
		return IsActive();
	}

	public bool IsEnemyArrowActive()
	{
		if (m_targetArrowType != TARGET_ARROW_TYPE.Enemy)
		{
			return false;
		}
		return IsActive();
	}

	public bool ShouldPreventMouseOverBigCard()
	{
		if (IsActive())
		{
			return !IsStaticArrow();
		}
		return false;
	}

	public void ShowBullseye(bool show)
	{
		if (m_bullseyeAlwaysOn && !show)
		{
			return;
		}
		if (m_reticleType == TARGET_RETICLE_TYPE.DefaultArrow)
		{
			if (IsActive() && m_showArrow)
			{
				Transform arrowTransform = m_arrow.transform.Find("TargetArrow_TargetMesh");
				if ((bool)arrowTransform)
				{
					RenderUtils.EnableRenderers(arrowTransform.gameObject, show);
				}
			}
		}
		else if (m_reticleType == TARGET_RETICLE_TYPE.HunterReticle)
		{
			if (m_hunterReticle == null)
			{
				return;
			}
			BlitToTexture b2t = m_hunterReticle.GetComponent<BlitToTexture>();
			if (b2t == null)
			{
				return;
			}
			Material mat = b2t.DrawAfterBlit.GetComponent<Renderer>().GetMaterial();
			if (!(mat == null))
			{
				if (show)
				{
					mat.color = Color.red;
				}
				else
				{
					mat.color = Color.white;
				}
			}
		}
		else if (m_reticleType == TARGET_RETICLE_TYPE.GruntyReticle)
		{
			if (!(m_gruntyReticle == null))
			{
				GruntyReticle reticle = m_gruntyReticle.GetComponent<GruntyReticle>();
				if (!(reticle == null))
				{
					reticle.SetHoveringState(show);
				}
			}
		}
		else if (m_reticleType == TARGET_RETICLE_TYPE.QuestionMark && IsActive() && m_showArrow)
		{
			Transform questionTransform = m_questionMark.transform.Find("TargetQuestionMark_TargetMesh");
			if ((bool)questionTransform)
			{
				RenderUtils.EnableRenderers(questionTransform.gameObject, show);
			}
		}
	}

	public void CreateFriendlyTargetArrow(Entity sourceEntity, bool showDamageIndicatorText, bool showArrow = true, string overrideText = null, bool useHandAsOrigin = false, bool isAttackArrow = false)
	{
		bool num = GameMgr.Get() != null && GameMgr.Get().IsSpectator();
		bool isViewingTeammate = TeammateBoardViewer.Get() != null && TeammateBoardViewer.Get().IsViewingTeammate();
		if (!num && !isViewingTeammate)
		{
			DisableCollidersForUntargetableCards(sourceEntity.GetCard());
		}
		if (GameState.Get().GetGameEntity().HasTag(GAME_TAG.ALL_TARGETS_RANDOM) || sourceEntity.HasTag(GAME_TAG.FORGETFUL) || sourceEntity.HasTag(GAME_TAG.FORGETFUL_ATTACK_VISUAL))
		{
			m_reticleType = TARGET_RETICLE_TYPE.QuestionMark;
		}
		else if (sourceEntity.HasTag(GAME_TAG.TARGETING_ARROW_TYPE) && !isAttackArrow)
		{
			m_reticleType = (TARGET_RETICLE_TYPE)sourceEntity.GetTag(GAME_TAG.TARGETING_ARROW_TYPE);
		}
		else
		{
			Spell spell = sourceEntity.GetCard().GetPlaySpell(0);
			if (spell != null)
			{
				m_reticleType = spell.m_TargetReticle;
			}
			else
			{
				m_reticleType = TARGET_RETICLE_TYPE.DefaultArrow;
			}
		}
		SetParabolaHeight(1.5f);
		string damageIndicatorText = null;
		if (overrideText != null)
		{
			damageIndicatorText = overrideText;
		}
		else if (showDamageIndicatorText)
		{
			damageIndicatorText = sourceEntity.GetTargetingArrowText();
		}
		Entity originLocationEntity = sourceEntity;
		if (sourceEntity.IsSpell())
		{
			originLocationEntity = ((!isViewingTeammate) ? sourceEntity.GetHero() : TeammateBoardViewer.Get().GetTeammateHero());
		}
		else if (sourceEntity.IsLettuceAbility())
		{
			Entity abilityOwnerEntity = sourceEntity.GetLettuceAbilityOwner();
			if (abilityOwnerEntity != null)
			{
				originLocationEntity = abilityOwnerEntity;
			}
		}
		CreateTargetArrow(TARGET_ARROW_TYPE.Friendly, originLocationEntity, sourceEntity.GetEntityId(), damageIndicatorText, showArrow, useHandAsOrigin);
		AttachLinksToAppropriateReticle();
		SetTargetArrowLinkLayer(GameLayer.Tooltip);
	}

	public void RefreshTargetingArrowText(Entity sourceEntity)
	{
		string damageIndicatorText = sourceEntity.GetTargetingArrowText();
		if (!IsEnemyArrow())
		{
			StartCoroutine(SetDamageText(damageIndicatorText));
		}
	}

	private void AttachLinksToAppropriateReticle()
	{
		GameObject arrowObject = GetAppropriateReticle();
		foreach (GameObject targetArrowLink in m_targetArrowLinks)
		{
			targetArrowLink.transform.parent = arrowObject.transform;
		}
	}

	public void CreateEnemyTargetArrow(Entity originEntity)
	{
		if (GameState.Get().GetGameEntity().HasTag(GAME_TAG.ALL_TARGETS_RANDOM))
		{
			m_reticleType = TARGET_RETICLE_TYPE.QuestionMark;
		}
		else
		{
			m_reticleType = TARGET_RETICLE_TYPE.DefaultArrow;
		}
		SetParabolaHeight(1.5f);
		CreateTargetArrow(TARGET_ARROW_TYPE.Enemy, originEntity, originEntity.GetEntityId(), null, showArrow: true);
		AttachLinksToAppropriateReticle();
		SetTargetArrowLinkLayer(GameLayer.Tooltip);
	}

	public void CreateStaticTargetArrow(Entity originEntity, Entity targetEntity)
	{
		if (originEntity == null || targetEntity == null)
		{
			Log.Gameplay.PrintError("Unable to create static target arrow. Null entities provided.");
			return;
		}
		m_reticleType = TARGET_RETICLE_TYPE.DefaultArrow;
		m_targetArrowOrigin = targetEntity.GetCard().transform.position;
		m_remoteArrowPosition = m_targetArrowOrigin;
		m_arrow.transform.position = m_targetArrowOrigin;
		SetParabolaHeight(1.5f);
		CreateTargetArrow(TARGET_ARROW_TYPE.Static, originEntity, originEntity.GetEntityId(), null, showArrow: true);
		m_bullseyeAlwaysOn = true;
		ShowBullseye(show: true);
		AttachLinksToAppropriateReticle();
		SetTargetArrowLinkLayer(GameLayer.Tooltip);
	}

	public void DestroyEnemyTargetArrow()
	{
		DestroyTargetArrow(TARGET_ARROW_TYPE.Enemy, isLocallyCanceled: false);
	}

	public void DestroyStaticTargetArrow()
	{
		DestroyTargetArrow(TARGET_ARROW_TYPE.Static, isLocallyCanceled: false);
	}

	public void DestroyFriendlyTargetArrow(bool isLocallyCanceled)
	{
		EnableCollidersThatWereDisabled();
		DestroyTargetArrow(TARGET_ARROW_TYPE.Friendly, isLocallyCanceled);
	}

	public void UpdateArrowPosition()
	{
		if (!IsActive())
		{
			return;
		}
		if (!m_showArrow)
		{
			UpdateArrowOriginPosition();
			UpdateDamageIndicator();
			return;
		}
		bool friendlySideIsRemote = GameMgr.Get() != null && GameMgr.Get().IsSpectator();
		bool isViewingTeammate = TeammateBoardViewer.Get() != null && TeammateBoardViewer.Get().IsViewingTeammate();
		Vector3 newTargetArrowPos = default(Vector3);
		if (IsEnemyArrow() || friendlySideIsRemote || isViewingTeammate || IsStaticArrow())
		{
			Vector3 pos = Vector3.zero;
			pos = GetAppropriateReticle().transform.position;
			newTargetArrowPos.x = Mathf.Lerp(pos.x, m_remoteArrowPosition.x, 0.1f);
			newTargetArrowPos.y = Mathf.Lerp(pos.y, m_remoteArrowPosition.y, 0.1f);
			newTargetArrowPos.z = Mathf.Lerp(pos.z, m_remoteArrowPosition.z, 0.1f);
			Card heldCard = RemoteActionHandler.Get().GetActualHeldCard(IsEnemyArrow(), isViewingTeammate);
			if (heldCard != null)
			{
				if (heldCard.GetEntity().GetZone() != TAG_ZONE.DECK)
				{
					m_targetArrowOrigin = heldCard.transform.position;
				}
				if (heldCard.GetActor() == null)
				{
					Card enemyHeroCard = GameState.Get().GetOpposingSidePlayer().GetHeroCard();
					Card friendHeroCard = GameState.Get().GetFriendlySidePlayer().GetHeroCard();
					if (m_targetArrowType == TARGET_ARROW_TYPE.Enemy && enemyHeroCard != null)
					{
						m_targetArrowOrigin = enemyHeroCard.transform.position;
					}
					else if (m_targetArrowType == TARGET_ARROW_TYPE.Friendly && friendHeroCard != null)
					{
						m_targetArrowOrigin = friendHeroCard.transform.position;
					}
				}
			}
		}
		else
		{
			if (!UniversalInputManager.Get().GetInputHitInfo(Camera.main, GameLayer.DragPlane, out var hitInfo))
			{
				return;
			}
			newTargetArrowPos = hitInfo.point;
			UpdateArrowOriginPosition();
		}
		float yRotationRadians = Mathf.Atan2(newTargetArrowPos.x - m_targetArrowOrigin.x, newTargetArrowPos.z - m_targetArrowOrigin.z);
		float yRotationDeg = 57.29578f * yRotationRadians;
		if (m_reticleType == TARGET_RETICLE_TYPE.DefaultArrow || m_reticleType == TARGET_RETICLE_TYPE.QuestionMark)
		{
			GameObject appropriateReticle = GetAppropriateReticle();
			appropriateReticle.transform.localEulerAngles = new Vector3(0f, yRotationDeg, 0f);
			appropriateReticle.transform.position = newTargetArrowPos;
			float num = Mathf.Pow(m_targetArrowOrigin.x - newTargetArrowPos.x, 2f);
			float zDifferenceSquared = Mathf.Pow(m_targetArrowOrigin.z - newTargetArrowPos.z, 2f);
			float distanceFromOriginToMouse = Mathf.Sqrt(num + zDifferenceSquared);
			UpdateTargetArrowLinks(distanceFromOriginToMouse);
		}
		else if (m_reticleType == TARGET_RETICLE_TYPE.HunterReticle)
		{
			m_hunterReticle.transform.position = newTargetArrowPos;
		}
		else if (m_reticleType == TARGET_RETICLE_TYPE.GruntyReticle)
		{
			m_gruntyReticle.transform.position = newTargetArrowPos;
		}
		else
		{
			Debug.LogError("Unknown Target Reticle Type!");
		}
		LocalTargetArrowPosition = newTargetArrowPos;
		UpdateDamageIndicator();
	}

	public void SetRemotePlayerArrowPosition(Vector3 newPosition)
	{
		m_remoteArrowPosition = newPosition;
	}

	private void DestroyCurrentArrow(bool isLocallyCanceled)
	{
		if (IsEnemyArrow())
		{
			DestroyEnemyTargetArrow();
		}
		else
		{
			DestroyFriendlyTargetArrow(isLocallyCanceled);
		}
	}

	private void DisableCollidersForUntargetableCards(Card sourceCard)
	{
		List<Card> cardsToDisable = new List<Card>();
		foreach (Player player in GameState.Get().GetPlayerMap().Values)
		{
			AddUntargetableCard(sourceCard, cardsToDisable, player.GetHeroPowerCard());
			AddUntargetableCard(sourceCard, cardsToDisable, player.GetWeaponCard());
			foreach (Card secretCard in player.GetSecretZone().GetCards())
			{
				AddUntargetableCard(sourceCard, cardsToDisable, secretCard);
			}
		}
		foreach (Card card in cardsToDisable)
		{
			if (!(card == null))
			{
				Actor actor = card.GetActor();
				if (!(actor == null))
				{
					actor.TurnOffCollider();
				}
			}
		}
	}

	private void AddUntargetableCard(Card sourceCard, List<Card> cards, Card card)
	{
		if (!(sourceCard == card))
		{
			cards.Add(card);
		}
	}

	private void EnableCollidersThatWereDisabled()
	{
		List<Card> cardsToEnable = new List<Card>();
		foreach (Player player in GameState.Get().GetPlayerMap().Values)
		{
			cardsToEnable.Add(player.GetHeroPowerCard());
			cardsToEnable.Add(player.GetWeaponCard());
			foreach (Card secretCard in player.GetSecretZone().GetCards())
			{
				cardsToEnable.Add(secretCard);
			}
		}
		foreach (Card card in cardsToEnable)
		{
			if (!(card == null) && !(card.GetActor() == null))
			{
				card.GetActor().TurnOnCollider();
			}
		}
	}

	private void CreateTargetArrow(TARGET_ARROW_TYPE targetArrowType, Entity originLocationEntity, int sourceEntityID, string damageIndicatorText, bool showArrow, bool useHandAsOrigin = false)
	{
		if (IsActive())
		{
			Log.Gameplay.Print("Uh-oh... creating a targeting arrow but one is already active...");
			DestroyCurrentArrow(isLocallyCanceled: false);
		}
		bool isViewingTeammate = TeammateBoardViewer.Get() != null && TeammateBoardViewer.Get().IsViewingTeammate();
		m_targetArrowType = targetArrowType;
		m_sourceEntityID = sourceEntityID;
		m_originLocationEntity = originLocationEntity;
		m_showArrow = showArrow;
		m_useHandAsOrigin = useHandAsOrigin;
		UpdateArrowOriginPosition();
		bool friendlySideIsRemote = GameMgr.Get() != null && GameMgr.Get().IsSpectator();
		if ((IsEnemyArrow() || friendlySideIsRemote || isViewingTeammate) && !IsStaticArrow())
		{
			m_remoteArrowPosition = m_targetArrowOrigin;
			m_arrow.transform.position = m_targetArrowOrigin;
		}
		ActivateArrow(active: true);
		ShowBullseye(show: false);
		ShowDamageIndicator(!IsEnemyArrow());
		UpdateArrowPosition();
		if (!IsEnemyArrow())
		{
			StartCoroutine(SetDamageText(damageIndicatorText));
			if (!friendlySideIsRemote && !IsStaticArrow() && !isViewingTeammate)
			{
				PegCursor.Get().Hide();
			}
		}
	}

	public void PreloadTargetArrows(PrefabInstanceLoadTracker.Context context)
	{
		m_targetArrowLinks = new List<GameObject>();
		PrefabInstanceLoadTracker.Get().InstantiatePrefab(context, "Target_Arrow_Bullseye.prefab:7afe007e5f455b04b9407307d8df1983", LoadArrowCallback, null, AssetLoadingOptions.IgnorePrefabPosition);
		PrefabInstanceLoadTracker.Get().InstantiatePrefab(context, "TargetDamageIndicator.prefab:91b47a1196e64e946a974becc0fb29f1", LoadDamageIndicatorCallback, null, AssetLoadingOptions.IgnorePrefabPosition);
		PrefabInstanceLoadTracker.Get().InstantiatePrefab(context, "Target_Arrow_Link.prefab:eb929158148ae954881c5684d27a1aa2", LoadLinkCallback, null, AssetLoadingOptions.IgnorePrefabPosition);
		PrefabInstanceLoadTracker.Get().InstantiatePrefab(context, "HunterReticle.prefab:83c7a1ebe50ef476f891c1b39dd5fd88", LoadHunterReticleCallback, null, AssetLoadingOptions.IgnorePrefabPosition);
		PrefabInstanceLoadTracker.Get().InstantiatePrefab(context, "Target_Question_Mark.prefab:adc81f6922c3de840b0e071ac55c7d62", LoadQuestionCallback, null, AssetLoadingOptions.IgnorePrefabPosition);
		PrefabInstanceLoadTracker.Get().InstantiatePrefab(context, "GruntyReticle.prefab:7183d57d0cc0ac84cafd80ec0e7ec54e", LoadGruntyReticleCallback, null, AssetLoadingOptions.IgnorePrefabPosition);
	}

	private void DestroyTargetArrow(TARGET_ARROW_TYPE arrowType, bool isLocallyCanceled)
	{
		if (!IsActive())
		{
			return;
		}
		if (arrowType != m_targetArrowType)
		{
			Log.Gameplay.Print($"trying to destroy {arrowType.ToString()} arrow but the active arrow is {m_targetArrowType.ToString()}");
			return;
		}
		if (isLocallyCanceled)
		{
			GameState.Get().GetEntity(m_sourceEntityID)?.GetCard().NotifyTargetingCanceled();
		}
		m_originLocationEntity = null;
		m_sourceEntityID = -1;
		if (!IsEnemyArrow())
		{
			if (!IsStaticArrow())
			{
				RemoteActionHandler.Get().NotifyOpponentOfTargetEnd();
			}
			PegCursor.Get().Show();
		}
		ActivateArrow(active: false);
		ShowDamageIndicator(show: false);
	}

	private void LoadArrowCallback(AssetReference assetRef, GameObject go, object callbackData)
	{
		m_arrow = go;
		ShowBullseye(show: false);
	}

	private void LoadQuestionCallback(AssetReference assetRef, GameObject go, object callbackData)
	{
		m_questionMark = go;
		ShowBullseye(show: false);
	}

	private void LoadLinkCallback(AssetReference assetRef, GameObject go, object callbackData)
	{
		StartCoroutine(OnLinkLoaded(go));
	}

	private void LoadDamageIndicatorCallback(AssetReference assetRef, GameObject go, object callbackData)
	{
		m_damageIndicator = go.GetComponent<TargetDamageIndicator>();
		if (m_damageIndicator == null)
		{
			Log.Gameplay.PrintError("LoadDamageIndicatorCallback - No TargetDamageIndicator script attached to '{0}'!", go.name);
		}
		else
		{
			m_damageIndicator.transform.eulerAngles = new Vector3(90f, 0f, 0f);
			m_damageIndicator.transform.localScale = new Vector3(DAMAGE_INDICATOR_SCALE, DAMAGE_INDICATOR_SCALE, DAMAGE_INDICATOR_SCALE);
		}
	}

	private void LoadHunterReticleCallback(AssetReference assetRef, GameObject go, object callbackData)
	{
		m_hunterReticle = go;
		m_hunterReticle.transform.parent = base.transform;
		m_hunterReticle.SetActive(value: false);
	}

	private void LoadGruntyReticleCallback(AssetReference assetRef, GameObject go, object callbackData)
	{
		m_gruntyReticle = go;
		if (m_gruntyReticle == null)
		{
			Log.Gameplay.PrintError("LoadGruntyReticleCallback - failed to load GruntyReticle.prefab");
			return;
		}
		m_gruntyReticle.transform.parent = base.transform;
		m_gruntyReticle.SetActive(value: false);
	}

	private IEnumerator OnLinkLoaded(GameObject linkActorObject)
	{
		while (m_arrow == null)
		{
			yield return null;
		}
		for (int i = 0; i < 14; i++)
		{
			GameObject newLink = Object.Instantiate(linkActorObject);
			newLink.transform.parent = m_arrow.transform;
			m_targetArrowLinks.Add(newLink);
		}
		linkActorObject.transform.parent = m_arrow.transform;
		m_targetArrowLinks.Add(linkActorObject);
	}

	private int NumberOfRequiredLinks(float lengthOfArrow)
	{
		int numLinksRequired = (int)Mathf.Floor(lengthOfArrow / 1.2f) + 1;
		if (numLinksRequired == 1)
		{
			numLinksRequired = 0;
		}
		return numLinksRequired;
	}

	private GameObject GetAppropriateReticle()
	{
		switch (m_reticleType)
		{
		case TARGET_RETICLE_TYPE.DefaultArrow:
			return m_arrow;
		case TARGET_RETICLE_TYPE.HunterReticle:
			return m_hunterReticle;
		case TARGET_RETICLE_TYPE.QuestionMark:
			return m_questionMark;
		case TARGET_RETICLE_TYPE.GruntyReticle:
			return m_gruntyReticle;
		default:
			Log.All.PrintError("Unknown Target Reticle Type!");
			return null;
		}
	}

	private Transform GetAppropriateArrowMeshTransform()
	{
		switch (m_reticleType)
		{
		case TARGET_RETICLE_TYPE.DefaultArrow:
		case TARGET_RETICLE_TYPE.HunterReticle:
		case TARGET_RETICLE_TYPE.GruntyReticle:
			return m_arrow.transform.Find("TargetArrow_ArrowMesh");
		case TARGET_RETICLE_TYPE.QuestionMark:
			return m_questionMark.transform.Find("TargetQuestionMark_QuestionMarkMesh");
		default:
			Log.All.PrintError("Unknown Target Reticle Type!");
			return null;
		}
	}

	private float GetStartingXRotationForArrowMesh()
	{
		switch (m_reticleType)
		{
		case TARGET_RETICLE_TYPE.DefaultArrow:
		case TARGET_RETICLE_TYPE.HunterReticle:
		case TARGET_RETICLE_TYPE.GruntyReticle:
			return 300f;
		case TARGET_RETICLE_TYPE.QuestionMark:
			return 0f;
		default:
			Log.All.PrintError("Unknown Target Reticle Type!");
			return 0f;
		}
	}

	private void UpdateTargetArrowLinks(float lengthOfArrow)
	{
		m_numActiveLinks = NumberOfRequiredLinks(lengthOfArrow);
		int numAvailableLinks = m_targetArrowLinks.Count;
		Transform arrowTransform = GetAppropriateArrowMeshTransform();
		if (m_numActiveLinks == 0)
		{
			arrowTransform.localEulerAngles = new Vector3(GetStartingXRotationForArrowMesh(), 180f, 0f);
			for (int i = 0; i < numAvailableLinks; i++)
			{
				RenderUtils.EnableRenderers(m_targetArrowLinks[i].gameObject, enable: false);
			}
			return;
		}
		float h = (0f - lengthOfArrow) / 2f;
		float a = (0f - m_parabolaHeight) / (h * h);
		for (int j = 0; j < numAvailableLinks; j++)
		{
			if (m_targetArrowLinks[j] == null)
			{
				continue;
			}
			if (j >= m_numActiveLinks)
			{
				RenderUtils.EnableRenderers(m_targetArrowLinks[j].gameObject, enable: false);
				continue;
			}
			float zLocalPos = 0f - 1.2f * (float)(j + 1) + m_linkAnimationZOffset;
			float yLocalPos = a * Mathf.Pow(zLocalPos - h, 2f) + m_parabolaHeight;
			float xRotationRadInv = Mathf.Atan(2f * a * (zLocalPos - h));
			float xRotationDeg = 180f - xRotationRadInv * 57.29578f;
			RenderUtils.EnableRenderers(m_targetArrowLinks[j].gameObject, enable: true);
			m_targetArrowLinks[j].transform.localPosition = new Vector3(0f, yLocalPos, zLocalPos);
			m_targetArrowLinks[j].transform.eulerAngles = new Vector3(xRotationDeg, GetAppropriateReticle().transform.localEulerAngles.y, 0f);
			float alpha = 1f;
			if (j == 0)
			{
				if (zLocalPos > -1.2f)
				{
					alpha = zLocalPos / -1.2f;
					alpha = Mathf.Pow(alpha, 6f);
				}
			}
			else if (j == m_numActiveLinks - 1)
			{
				alpha = m_linkAnimationZOffset / 1.2f;
				alpha *= alpha;
			}
			SetLinkAlpha(m_targetArrowLinks[j], alpha);
		}
		float arrowHeadYPos = a * Mathf.Pow(arrowTransform.localPosition.z - h, 2f) + m_parabolaHeight;
		float arrow_xRotationDeg = 0f;
		if (m_reticleType != TARGET_RETICLE_TYPE.QuestionMark)
		{
			arrow_xRotationDeg = Mathf.Atan(2f * a * (arrowTransform.localPosition.z - h)) * 57.29578f;
			if (arrow_xRotationDeg < 0f)
			{
				arrow_xRotationDeg += 360f;
			}
		}
		arrowTransform.localPosition = new Vector3(0f, arrowHeadYPos, arrowTransform.localPosition.z);
		arrowTransform.localEulerAngles = new Vector3(arrow_xRotationDeg, 180f, 0f);
		m_linkAnimationZOffset += Time.deltaTime * 0.5f;
		if (m_linkAnimationZOffset > 1.2f)
		{
			m_linkAnimationZOffset -= 1.2f;
		}
	}

	private void SetLinkAlpha(GameObject linkGameObject, float alpha)
	{
		alpha = Mathf.Clamp(alpha, 0f, 1f);
		Renderer[] components = linkGameObject.GetComponents<Renderer>();
		for (int i = 0; i < components.Length; i++)
		{
			Material material = components[i].GetMaterial();
			Color newColor = material.color;
			newColor.a = alpha;
			material.color = newColor;
		}
	}

	private void UpdateDamageIndicator()
	{
		if (!(m_damageIndicator == null))
		{
			Vector3 newPosition = Vector3.zero;
			if ((bool)SHOW_DAMAGE_INDICATOR_ON_ENTITY)
			{
				newPosition = m_targetArrowOrigin;
				newPosition.z += DAMAGE_INDICATOR_Z_OFFSET;
			}
			else
			{
				newPosition = GetAppropriateReticle().transform.position;
				newPosition.z += DAMAGE_INDICATOR_Z_OFFSET;
			}
			m_damageIndicator.transform.position = newPosition;
		}
	}

	private void ShowDamageIndicator(bool show)
	{
		if ((bool)m_damageIndicator && m_damageIndicator.gameObject.activeInHierarchy)
		{
			m_damageIndicator.Show(show);
		}
	}

	private IEnumerator SetDamageText(string damageText)
	{
		while (m_damageIndicator == null)
		{
			yield return null;
		}
		m_damageIndicator.SetText(damageText);
		if (string.IsNullOrEmpty(damageText))
		{
			m_damageIndicator.Show(active: false);
		}
	}

	private void UpdateArrowOriginPosition()
	{
		Entity originEntity = m_originLocationEntity;
		if (originEntity == null && !m_useHandAsOrigin)
		{
			string message = $"Can't update arrow origin position because nothing was specified! (m_originLocationEntity = {originEntity}, m_useHandAsOrigin = {m_useHandAsOrigin})";
			Log.Gameplay.Print(message);
			DestroyCurrentArrow(isLocallyCanceled: false);
			return;
		}
		if (originEntity != null)
		{
			m_targetArrowOrigin = originEntity.GetCard().transform.position;
		}
		if (m_useHandAsOrigin || (originEntity != null && originEntity.GetZone() == TAG_ZONE.DECK))
		{
			if (IsEnemyArrow())
			{
				m_targetArrowOrigin = InputManager.Get().GetEnemyHand().transform.position;
			}
			else
			{
				m_targetArrowOrigin = InputManager.Get().GetFriendlyHand().transform.position;
			}
		}
		if (originEntity != null && originEntity.IsHero() && !IsEnemyArrow())
		{
			m_targetArrowOrigin.z += 1f;
		}
	}

	private void ActivateArrow(bool active)
	{
		m_isActive = active;
		RenderUtils.EnableRenderers(m_arrow.gameObject, enable: false);
		m_hunterReticle.SetActive(value: false);
		m_gruntyReticle.SetActive(value: false);
		RenderUtils.EnableRenderers(m_questionMark.gameObject, enable: false);
		if (active)
		{
			if (m_reticleType == TARGET_RETICLE_TYPE.DefaultArrow)
			{
				RenderUtils.EnableRenderers(m_arrow.gameObject, active && m_showArrow);
			}
			else if (m_reticleType == TARGET_RETICLE_TYPE.HunterReticle)
			{
				m_hunterReticle.SetActive(active && m_showArrow);
			}
			else if (m_reticleType == TARGET_RETICLE_TYPE.QuestionMark)
			{
				RenderUtils.EnableRenderers(m_questionMark.gameObject, active && m_showArrow);
			}
			else if (m_reticleType == TARGET_RETICLE_TYPE.GruntyReticle)
			{
				m_gruntyReticle.SetActive(active && m_showArrow);
			}
			else
			{
				Debug.LogError("Unknown Target Reticle Type!");
			}
		}
	}

	public void ShowArrow(bool show)
	{
		m_showArrow = show;
		RenderUtils.EnableRenderers(m_arrow.gameObject, enable: false);
		m_hunterReticle.SetActive(value: false);
		m_gruntyReticle.SetActive(value: false);
		RenderUtils.EnableRenderers(m_questionMark.gameObject, enable: false);
		if (show)
		{
			if (m_reticleType == TARGET_RETICLE_TYPE.DefaultArrow)
			{
				RenderUtils.EnableRenderers(m_arrow.gameObject, show);
			}
			else if (m_reticleType == TARGET_RETICLE_TYPE.HunterReticle)
			{
				m_hunterReticle.SetActive(show);
			}
			else if (m_reticleType == TARGET_RETICLE_TYPE.QuestionMark)
			{
				RenderUtils.EnableRenderers(m_questionMark.gameObject, show);
			}
			else if (m_reticleType == TARGET_RETICLE_TYPE.GruntyReticle)
			{
				m_gruntyReticle.SetActive(show);
			}
			else
			{
				Debug.LogError("Unknown Target Reticle Type!");
			}
		}
	}

	public void SetTargetArrowLinkLayer(GameLayer layer)
	{
		foreach (GameObject targetArrowLink in m_targetArrowLinks)
		{
			targetArrowLink.layer = (int)layer;
		}
	}

	public void SetParabolaHeight(float newHeight)
	{
		m_parabolaHeight = newHeight;
	}
}
