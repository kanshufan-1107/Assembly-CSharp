using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MercenariesAbilityTray : MonoBehaviour
{
	[Serializable]
	public class AbilityTrayBackgroundMapping
	{
		[SerializeField]
		public TAG_ROLE m_role;

		[SerializeField]
		public GameObject m_background;
	}

	[Serializable]
	public class AbilityCoverMapping
	{
		[SerializeField]
		public TAG_ROLE m_role;

		[SerializeField]
		public List<GameObject> m_covers;
	}

	[Serializable]
	public class AbilityBoneMapping
	{
		[SerializeField]
		public Transform m_abilityBone;

		[SerializeField]
		public Transform m_shownPosition;

		[SerializeField]
		public Transform m_socketedPosition;
	}

	public float m_showTweenTime = 0.2f;

	public float m_hideTweenTime = 0.1f;

	public int m_maxAbilitiesOnTray = 4;

	public GameObject m_pcLeftBigCardBone;

	public GameObject m_pcRightBigCard3TrayBone;

	public GameObject m_pcRightBigCard4TrayBone;

	public GameObject m_mobileLeftBigCardBone;

	public GameObject m_mobileRightBigCard3TrayBone;

	public GameObject m_mobileRightBigCard4TrayBone;

	public Float_MobileOverride m_abilityPreviewScale;

	[SerializeField]
	public List<AbilityTrayBackgroundMapping> m_threeAbilityBackgrounds;

	[SerializeField]
	public List<AbilityTrayBackgroundMapping> m_fourAbilityBackgrounds;

	[SerializeField]
	public List<AbilityCoverMapping> m_abilityCovers;

	[SerializeField]
	public List<AbilityBoneMapping> m_abilityBones;

	public PlayMakerFSM PlaymakerFsm;

	private Entity m_abilityOwnerEntity;

	private List<Card> m_abilityCards;

	private List<Card> m_lastShownAbilityCards = new List<Card>();

	private bool m_isAnimatingHide;

	private bool m_isAnimatingShow;

	private Coroutine m_showCoroutine;

	private Coroutine m_hideCoroutine;

	private bool m_isVisible;

	private readonly Vector3 OFFSCREEN_POSITION = new Vector3(-5000f, -5000f, -5000f);

	public void Start()
	{
		PlaymakerFsm.FsmVariables.GetFsmFloat("ShowTweenTime").Value = m_showTweenTime;
		PlaymakerFsm.FsmVariables.GetFsmFloat("HideTweenTime").Value = m_hideTweenTime;
		if (!Debug.isDebugBuild)
		{
			return;
		}
		float[] values = m_abilityPreviewScale.GetValues();
		for (int i = 0; i < values.Length; i++)
		{
			if (values[i] <= 0f)
			{
				Debug.LogError("m_abilityPreviewScale on object \"" + base.gameObject.name + "\" contains at least one invalid value for scale. All values should be positive numbers, roughly in range 1.0-3.0.");
			}
		}
	}

	public void SetupForMercenary(Entity mercenaryEntity, List<Card> abilityCards)
	{
		if (mercenaryEntity == null)
		{
			Log.Lettuce.PrintError("MercenariesAbilityTray.SetupForMercenary - null mercenary entity");
		}
		m_abilityOwnerEntity = mercenaryEntity;
		m_abilityCards = new List<Card>(abilityCards);
	}

	public void Show()
	{
		m_isVisible = true;
		if (m_showCoroutine != null)
		{
			StopCoroutine(m_showCoroutine);
			m_isAnimatingShow = false;
		}
		m_showCoroutine = StartCoroutine(ShowCoroutine());
	}

	public void Hide()
	{
		m_isVisible = false;
		if (m_showCoroutine != null)
		{
			StopCoroutine(m_showCoroutine);
			m_isAnimatingShow = false;
		}
		if (m_hideCoroutine != null)
		{
			StopCoroutine(m_hideCoroutine);
			m_isAnimatingHide = false;
		}
		m_hideCoroutine = StartCoroutine(HideCoroutine());
	}

	public bool IsAnimating()
	{
		if (!m_isAnimatingHide)
		{
			return m_isAnimatingShow;
		}
		return true;
	}

	public bool IsVisible()
	{
		return m_isVisible;
	}

	private void SetBackgroundForEntity(Entity entity, int numberOfAbilities)
	{
		int role = entity.GetTag(GAME_TAG.LETTUCE_ROLE);
		HideAllBackgrounds();
		GameObject background = (((uint)(role - 1) > 2u) ? GetBackgroundForRole(TAG_ROLE.INVALID, numberOfAbilities) : GetBackgroundForRole((TAG_ROLE)role, numberOfAbilities));
		if (background != null)
		{
			background.SetActive(value: true);
		}
		List<GameObject> covers = GetAbilityCoversForRole((TAG_ROLE)role);
		if (covers != null)
		{
			for (int i = numberOfAbilities; i < covers.Count; i++)
			{
				covers[i].SetActive(value: true);
			}
		}
	}

	private void HideAllBackgrounds()
	{
		foreach (AbilityTrayBackgroundMapping threeAbilityBackground in m_threeAbilityBackgrounds)
		{
			threeAbilityBackground.m_background.SetActive(value: false);
		}
		foreach (AbilityTrayBackgroundMapping fourAbilityBackground in m_fourAbilityBackgrounds)
		{
			fourAbilityBackground.m_background.SetActive(value: false);
		}
		foreach (AbilityCoverMapping abilityCover in m_abilityCovers)
		{
			foreach (GameObject cover in abilityCover.m_covers)
			{
				cover.SetActive(value: false);
			}
		}
	}

	private GameObject GetBackgroundForRole(TAG_ROLE role, int numberOfAbilities)
	{
		foreach (AbilityTrayBackgroundMapping kvp in (numberOfAbilities > 3) ? m_fourAbilityBackgrounds : m_threeAbilityBackgrounds)
		{
			if (kvp.m_role == role)
			{
				return kvp.m_background;
			}
		}
		return null;
	}

	private List<GameObject> GetAbilityCoversForRole(TAG_ROLE role)
	{
		foreach (AbilityCoverMapping kvp in m_abilityCovers)
		{
			if (kvp.m_role == role)
			{
				return kvp.m_covers;
			}
		}
		return null;
	}

	private IEnumerator HideCoroutine()
	{
		m_isAnimatingHide = true;
		PlaymakerFsm.SendEvent("Death");
		yield return new WaitForSeconds(m_hideTweenTime);
		EnsureLastShownAbilityCardsAreHidden();
		m_isAnimatingHide = false;
	}

	private void EnsureLastShownAbilityCardsAreHidden()
	{
		if (m_lastShownAbilityCards.Count == 0)
		{
			return;
		}
		foreach (Card lastShownAbilityCard in m_lastShownAbilityCards)
		{
			Actor actor = lastShownAbilityCard.GetActor();
			if (!(actor == null))
			{
				actor.gameObject.transform.position = OFFSCREEN_POSITION;
			}
		}
		m_lastShownAbilityCards.Clear();
	}

	private IEnumerator ShowCoroutine()
	{
		while (m_isAnimatingHide)
		{
			yield return null;
		}
		EnsureLastShownAbilityCardsAreHidden();
		m_isAnimatingShow = true;
		if (m_abilityOwnerEntity != null)
		{
			int numAbilities = m_abilityCards.Count;
			foreach (Card abilityCard2 in m_abilityCards)
			{
				if (abilityCard2.GetEntity().GetZone() == TAG_ZONE.SETASIDE)
				{
					numAbilities--;
				}
			}
			SetBackgroundForEntity(m_abilityOwnerEntity, numAbilities);
			PlaymakerFsm.FsmVariables.GetFsmVector3("MercenaryPosition").Value = m_abilityOwnerEntity.GetCard().transform.position;
		}
		PlaymakerFsm.FsmVariables.GetFsmFloat("ShowTweenTime").Value = m_showTweenTime;
		for (int i = 0; i < m_maxAbilitiesOnTray; i++)
		{
			string variableName = "AbilityActor" + (i + 1);
			if (i >= m_abilityCards.Count)
			{
				PlaymakerFsm.FsmVariables.GetFsmGameObject(variableName).Value = null;
				continue;
			}
			Card abilityCard = m_abilityCards[i];
			if (abilityCard == null || abilityCard.GetActor() == null)
			{
				PlaymakerFsm.FsmVariables.GetFsmGameObject(variableName).Value = null;
				continue;
			}
			abilityCard.UpdateActorState();
			if (abilityCard.GetActor() is LettuceAbilityActor lettuceAbilityActor)
			{
				lettuceAbilityActor.UpdateCheckMarkObject();
			}
			SetAbilityBonePositionByTags(abilityCard, i);
			PlaymakerFsm.FsmVariables.GetFsmGameObject(variableName).Value = abilityCard.GetActor().gameObject;
		}
		PlaymakerFsm.SendEvent("Birth");
		m_lastShownAbilityCards.AddRange(m_abilityCards);
		yield return new WaitForSeconds(m_showTweenTime);
		m_isAnimatingShow = false;
	}

	private void SetAbilityBonePositionByTags(Card abilityCard, int abilityBoneIndex)
	{
		if (abilityBoneIndex < 0 || abilityBoneIndex > m_abilityBones.Count)
		{
			Log.Lettuce.PrintError("SetAbilityBonePositionByTags - Invalid index {0}", abilityBoneIndex);
			return;
		}
		AbilityBoneMapping abilityBoneData = m_abilityBones[abilityBoneIndex];
		if (abilityCard.GetEntity().HasTag(GAME_TAG.LETTUCE_CURRENT_COOLDOWN))
		{
			abilityBoneData.m_abilityBone.localPosition = abilityBoneData.m_socketedPosition.localPosition;
		}
		else
		{
			abilityBoneData.m_abilityBone.localPosition = abilityBoneData.m_shownPosition.localPosition;
		}
	}

	private int CountNonSubcardAbilities()
	{
		int nonSubcardAbilities = 0;
		foreach (Card card in m_abilityCards)
		{
			if (!(card == null))
			{
				Entity ent = card.GetEntity();
				if (ent != null && !ent.HasTag(GAME_TAG.PARENT_CARD))
				{
					nonSubcardAbilities++;
				}
			}
		}
		return nonSubcardAbilities;
	}

	public void GetBigCardBones(out GameObject left, out GameObject right)
	{
		if ((bool)UniversalInputManager.UsePhoneUI)
		{
			left = m_mobileLeftBigCardBone;
			if (CountNonSubcardAbilities() <= 3)
			{
				right = m_mobileRightBigCard3TrayBone;
			}
			else
			{
				right = m_mobileRightBigCard4TrayBone;
			}
		}
		else
		{
			left = m_pcLeftBigCardBone;
			if (CountNonSubcardAbilities() <= 3)
			{
				right = m_pcRightBigCard3TrayBone;
			}
			else
			{
				right = m_pcRightBigCard4TrayBone;
			}
		}
	}

	public int GetTrayPositionOfAbility(Card abilityCard)
	{
		int sourceEntityId = abilityCard.GetEntity().GetEntityId();
		for (int i = 0; i < m_abilityCards.Count; i++)
		{
			if (sourceEntityId == m_abilityCards[i].GetEntity().GetEntityId())
			{
				return i;
			}
		}
		return -1;
	}

	public float GetAbilityPreviewScaleForCurrentPlatform()
	{
		return m_abilityPreviewScale.GetValueForScreen(PlatformSettings.Screen, 1f);
	}
}
