using System;
using System.Collections;
using System.Collections.Generic;
using Hearthstone.UI;
using PegasusUtil;
using UnityEngine;

public class RibbonButtonsUI : MonoBehaviour
{
	[Serializable]
	public class RibbonButtonObject
	{
		public PegUIElement m_Ribbon;

		public Transform m_HiddenBone;

		public Transform m_ShownBone;

		public bool m_LeftSide;

		public float m_AnimateInDelay;

		public Box.ButtonType m_buttonType;
	}

	public List<RibbonButtonObject> m_Ribbons;

	public Transform m_LeftBones;

	public Transform m_RightBones;

	public float m_EaseInTime = 1f;

	public float m_EaseOutTime = 0.4f;

	public GameObject m_rootObject;

	public PegUIElement m_collectionManagerRibbon;

	public PegUIElement m_questLogRibbon;

	public PegUIElement m_packOpeningRibbon;

	public PegUIElement m_storeRibbon;

	public UberText m_packCount;

	public GameObject m_packCountFrame;

	public float m_minAspectRatioAdjustment = 0.24f;

	public float m_wideAspectRatioAdjustment;

	public float m_extraWideAspectRatioAdjustment = 0.24f;

	public float m_minAspectRatioZPos;

	public float m_wideAspectRatioZPos;

	public float m_extraWideAspectRatioZPos = 0.35f;

	public Widget m_journalButtonWidget;

	public GameObject m_legacyQuestButtonGameObject;

	private bool m_shown = true;

	private BoxRailroadManager m_railroadManager;

	private Dictionary<Box.ButtonType, bool> m_ribbonEnabledStates;

	public void Awake()
	{
		m_rootObject.SetActive(value: false);
		float adjustment = TransformUtil.GetAspectRatioDependentValue(m_minAspectRatioAdjustment, m_wideAspectRatioAdjustment, m_extraWideAspectRatioAdjustment);
		TransformUtil.SetLocalPosX(m_LeftBones, m_LeftBones.localPosition.x + adjustment);
		TransformUtil.SetLocalPosX(m_RightBones, m_RightBones.localPosition.x - adjustment);
		if (NetCache.Get().GetNetObject<NetCache.NetCacheFeatures>() == null)
		{
			Network.Get().RegisterNetHandler(InitialClientState.PacketID.ID, OnInitialClientState);
		}
		else
		{
			SetupJournalButton();
		}
		m_railroadManager = Box.Get().GetRailroadManager();
		m_ribbonEnabledStates = new Dictionary<Box.ButtonType, bool>();
		foreach (RibbonButtonObject ribbon in m_Ribbons)
		{
			m_ribbonEnabledStates[ribbon.m_buttonType] = false;
		}
	}

	private void Start()
	{
		float zPos = TransformUtil.GetAspectRatioDependentValue(m_minAspectRatioZPos, m_wideAspectRatioZPos, m_extraWideAspectRatioZPos);
		TransformUtil.SetLocalPosZ(base.transform, zPos);
	}

	private void OnDestroy()
	{
		Network.Get()?.RemoveNetHandler(InitialClientState.PacketID.ID, OnInitialClientState);
	}

	public void UpdateRibbons(bool shouldShow)
	{
		if (shouldShow || m_shown)
		{
			if (shouldShow)
			{
				StartCoroutine(ShowRibbons(!m_shown));
			}
			else
			{
				StartCoroutine(HideRibbons());
			}
			m_shown = shouldShow;
		}
	}

	private IEnumerator ShowRibbons(bool shouldDelay)
	{
		float startDelay = 0f;
		if (shouldDelay)
		{
			m_rootObject.SetActive(value: false);
			startDelay = 1f;
			foreach (RibbonButtonObject ribbon in m_Ribbons)
			{
				if (ribbon.m_AnimateInDelay < startDelay)
				{
					startDelay = ribbon.m_AnimateInDelay;
				}
			}
			yield return new WaitForSeconds(startDelay);
		}
		m_rootObject.SetActive(value: true);
		foreach (RibbonButtonObject ribbon2 in m_Ribbons)
		{
			if (m_railroadManager.ShouldHideButtonType(ribbon2.m_buttonType))
			{
				HideRibbon(ribbon2);
			}
			else
			{
				ShowRibbon(ribbon2, startDelay);
			}
		}
	}

	private void ShowRibbon(RibbonButtonObject ribbon, float startDelay)
	{
		if (!m_ribbonEnabledStates.ContainsKey(ribbon.m_buttonType) || !m_ribbonEnabledStates[ribbon.m_buttonType])
		{
			ribbon.m_Ribbon.transform.position = ribbon.m_HiddenBone.position;
			iTween.Stop(ribbon.m_Ribbon.gameObject);
			AnimateRibbonToPosition(ribbon, ribbon.m_ShownBone.position, ribbon.m_AnimateInDelay - startDelay);
			m_ribbonEnabledStates[ribbon.m_buttonType] = true;
		}
	}

	private IEnumerator HideRibbons()
	{
		foreach (RibbonButtonObject ribbon in m_Ribbons)
		{
			HideRibbon(ribbon);
		}
		yield return new WaitForSeconds(m_EaseOutTime);
		if (!m_shown)
		{
			m_rootObject.SetActive(value: false);
		}
	}

	private void HideRibbon(RibbonButtonObject ribbon)
	{
		if ((!m_ribbonEnabledStates.ContainsKey(ribbon.m_buttonType) || m_ribbonEnabledStates[ribbon.m_buttonType]) && !(ribbon.m_Ribbon.transform.position == ribbon.m_HiddenBone.position))
		{
			iTween.Stop(ribbon.m_Ribbon.gameObject);
			AnimateRibbonToPosition(ribbon, ribbon.m_HiddenBone.position, 0f);
			m_ribbonEnabledStates[ribbon.m_buttonType] = false;
		}
	}

	private void AnimateRibbonToPosition(RibbonButtonObject ribbon, Vector3 position, float delay)
	{
		Hashtable args = iTweenManager.Get().GetTweenHashTable();
		args.Add("position", position);
		args.Add("delay", delay);
		args.Add("time", m_EaseInTime);
		args.Add("easetype", iTween.EaseType.easeOutBack);
		iTween.MoveTo(ribbon.m_Ribbon.gameObject, args);
	}

	public void SetPackCount(int packs)
	{
		if (packs <= 0)
		{
			m_packCount.Text = "";
			m_packCountFrame.SetActive(value: false);
		}
		else
		{
			m_packCount.Text = GameStrings.Format("GLUE_PACK_OPENING_BOOSTER_COUNT", packs);
			m_packCountFrame.SetActive(value: true);
		}
	}

	private void OnInitialClientState()
	{
		Network.Get().RemoveNetHandler(InitialClientState.PacketID.ID, OnInitialClientState);
		SetupJournalButton();
	}

	private void SetupJournalButton()
	{
		m_journalButtonWidget.Show();
		m_legacyQuestButtonGameObject.SetActive(value: false);
	}
}
