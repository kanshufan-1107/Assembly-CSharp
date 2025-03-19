using System;
using System.Collections;
using Blizzard.T5.MaterialService.Extensions;
using UnityEngine;

public class BaconTrinketWidget : MonoBehaviour
{
	public MeshRenderer m_portraitMesh;

	public int portraitIndex;

	public Animator m_doorAnimator;

	public GameObject m_lockIcon;

	public PlayMakerFSM m_VFXPlayerMaker;

	public float m_tilingX = 0.69f;

	public float m_tilingY = 0.69f;

	public float m_offsetX = 0.17f;

	public float m_offsetY = 0.2f;

	private bool m_warningSent;

	private Actor m_actor;

	private Coroutine m_updateCoroutine;

	private bool m_isUsedInStatsPage;

	private void Awake()
	{
		m_actor = base.gameObject.GetComponent<Actor>();
		if (m_actor != null)
		{
			Actor actor = m_actor;
			actor.OnPortraitMaterialUpdated = (Action)Delegate.Combine(actor.OnPortraitMaterialUpdated, new Action(OnPortraitMaterialUpdated));
		}
		else
		{
			Debug.LogWarning("BaconTrinketWidget - Is missing an Actor Component");
		}
	}

	public void SetIsStatsPageTrinket(bool isUsedInStatsPage)
	{
		m_isUsedInStatsPage = isUsedInStatsPage;
		UpdateTrinketState();
	}

	private void Start()
	{
		UpdateTrinketState(turnLeftChanged: true, isPotentialTrinketChanged: true);
		OnPortraitMaterialUpdated();
	}

	public void UpdateTrinketState(bool turnLeftChanged = false, bool isPotentialTrinketChanged = false, int turnsLeftToDiscover = -1)
	{
		if (m_updateCoroutine != null)
		{
			StopCoroutine(m_updateCoroutine);
		}
		m_updateCoroutine = StartCoroutine(UpdateVFX(turnLeftChanged, isPotentialTrinketChanged, turnsLeftToDiscover));
	}

	private IEnumerator UpdateVFX(bool turnLeftChanged = false, bool isPotentialTrinketChanged = false, int turnsLeftToDiscover = -1)
	{
		if (m_isUsedInStatsPage)
		{
			if (m_doorAnimator != null)
			{
				m_doorAnimator.SetBool("DoorClosed", !m_isUsedInStatsPage);
				m_lockIcon.SetActive(!m_isUsedInStatsPage);
			}
			yield break;
		}
		while (m_actor.GetEntity() == null)
		{
			yield return null;
		}
		Entity entity = m_actor.GetEntity();
		if (turnLeftChanged)
		{
			if (turnsLeftToDiscover == -1)
			{
				turnsLeftToDiscover = entity.GetTag(GAME_TAG.BACON_TURNS_LEFT_TO_DISCOVER_TRINKET);
			}
			if (turnsLeftToDiscover == 1 && m_VFXPlayerMaker != null)
			{
				m_VFXPlayerMaker.SendEvent("LightningShake");
			}
		}
		if (!isPotentialTrinketChanged)
		{
			yield break;
		}
		if (entity.HasTag(GAME_TAG.BACON_IS_POTENTIAL_TRINKET))
		{
			if (m_doorAnimator != null && !m_doorAnimator.GetBool("DoorClosed"))
			{
				m_doorAnimator.SetBool("DoorClosed", value: true);
			}
		}
		else if (m_VFXPlayerMaker != null)
		{
			m_VFXPlayerMaker.SendEvent("Burst");
		}
	}

	private void OnDestroy()
	{
		if (m_actor != null)
		{
			Actor actor = m_actor;
			actor.OnPortraitMaterialUpdated = (Action)Delegate.Remove(actor.OnPortraitMaterialUpdated, new Action(OnPortraitMaterialUpdated));
		}
	}

	private void OnPortraitMaterialUpdated()
	{
		using DefLoader.DisposableCardDef cardDef = m_actor.GetCard()?.ShareDisposableCardDef();
		UpdatePortrait(cardDef);
	}

	private void UpdatePortrait(DefLoader.DisposableCardDef disposableCardDef)
	{
		if (m_portraitMesh == null)
		{
			return;
		}
		Material portraitMaterial = m_portraitMesh.GetMaterials()[portraitIndex];
		if (disposableCardDef != null)
		{
			Material portraitMat = disposableCardDef.CardDef.GetBattlegroundsQuestRewardPortraitMaterial();
			if (!(portraitMat == null))
			{
				portraitMaterial.mainTexture = portraitMat.mainTexture;
				Texture shadowTexture = portraitMaterial.GetTexture("_SecondTex");
				portraitMaterial.CopyPropertiesFromMaterial(portraitMat);
				portraitMaterial.SetTexture("_SecondTex", shadowTexture);
				return;
			}
			if (!m_warningSent)
			{
				Debug.LogWarning("BaconTrinketWidget.UpdatePortrait() - Missing portrait Mat");
				m_warningSent = true;
			}
		}
		SetupDefaultPortraitMaterial(portraitMaterial);
	}

	private void SetupDefaultPortraitMaterial(Material portraitMaterial)
	{
		portraitMaterial.SetTextureOffset("_MainTex", new Vector2(m_offsetX, m_offsetY));
		portraitMaterial.SetTextureScale("_MainTex", new Vector2(m_tilingX, m_tilingY));
	}
}
