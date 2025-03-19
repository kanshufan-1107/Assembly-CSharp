using System;
using System.Collections.Generic;
using Assets;
using Blizzard.T5.Core;
using UnityEngine;

public class EnchantmentBanner : MonoBehaviour
{
	public enum Icon
	{
		Nothing,
		Checkmark,
		Xmark
	}

	public GameObject m_EnchantmentBanner;

	public GameObject m_EnchantmentBannerBottom;

	public GameObject m_EnchantmentBannerIcon;

	public GameObject m_EnchantmentBannerCheckmark;

	public GameObject m_EnchantmentBannerXmark;

	public UberText m_EnchantmentBannerText;

	public int m_RenderQueueEnchantmentPanel;

	private float m_initialBannerHeight;

	private Vector3 m_initialBannerScale;

	private Vector3 m_initialBannerBottomScale;

	private Vector3 m_initialBannerTextScale;

	private Vector3 m_initialBannerIconScale;

	private readonly Pool<BigCardEnchantmentPanel> m_enchantmentPool = new Pool<BigCardEnchantmentPanel>();

	private Map<Tuple<string, string>, BigCardEnchantmentPanel> m_uniqueEnchantmentLookup = new Map<Tuple<string, string>, BigCardEnchantmentPanel>();

	public void Awake()
	{
		m_initialBannerHeight = m_EnchantmentBanner.GetComponent<Renderer>().bounds.size.z;
		m_initialBannerScale = m_EnchantmentBanner.transform.localScale;
		m_initialBannerBottomScale = m_EnchantmentBannerBottom.transform.localScale;
		m_initialBannerTextScale = m_EnchantmentBannerText.transform.localScale;
		m_initialBannerIconScale = m_EnchantmentBannerIcon.transform.localScale;
		m_enchantmentPool.SetCreateItemCallback(CreateEnchantmentPanel);
		m_enchantmentPool.SetDestroyItemCallback(DestroyEnchantmentPanel);
		m_enchantmentPool.SetExtensionCount(1);
		m_enchantmentPool.SetMaxReleasedItemCount(2);
		ResetEnchantments();
	}

	private BigCardEnchantmentPanel CreateEnchantmentPanel(int index)
	{
		BigCardEnchantmentPanel component = AssetLoader.Get().InstantiatePrefab("BigCardEnchantmentPanel.prefab:5af69938cd435a5488e4c9a7b8070e6e").GetComponent<BigCardEnchantmentPanel>();
		component.name = string.Format("{0}{1}", "BigCardEnchantmentPanel", index);
		RenderUtils.SetRenderQueue(component.gameObject, m_RenderQueueEnchantmentPanel);
		return component;
	}

	private void DestroyEnchantmentPanel(BigCardEnchantmentPanel panel)
	{
		UnityEngine.Object.Destroy(panel.gameObject);
	}

	public void UpdateEnchantments(Card card, Actor bigCardActor, float enchantmentScalingFactor)
	{
		ResetEnchantments();
		GameObject enchantmentBone = bigCardActor.FindBone("EnchantmentTooltip");
		if (enchantmentBone == null)
		{
			return;
		}
		Entity entity = card.GetEntity();
		bool useUniqueEnchantmentsOnly = GameState.Get() != null && GameState.Get().GetGameEntity() != null && GameState.Get().GetBooleanGameOption(GameEntityOption.USE_COMPACT_ENCHANTMENT_BANNERS);
		List<Entity> enchantments = entity.GetDisplayedEnchantments(useUniqueEnchantmentsOnly);
		List<BigCardEnchantmentPanel> activePanels = m_enchantmentPool.GetActiveList();
		int newCount = enchantments.Count;
		if (newCount == 0 && !entity.HasTag(GAME_TAG.ENCHANTMENT_BANNER_TEXT) && !entity.IsSideQuest() && !entity.IsObjective())
		{
			return;
		}
		m_uniqueEnchantmentLookup.Clear();
		int oldCount = activePanels.Count;
		int diff = newCount - oldCount;
		if (diff > 0)
		{
			m_enchantmentPool.AcquireBatch(diff);
		}
		else if (diff < 0)
		{
			m_enchantmentPool.ReleaseBatch(newCount, -diff);
		}
		for (int i = 0; i < activePanels.Count; i++)
		{
			Entity enchantment = enchantments[i];
			BigCardEnchantmentPanel activePanel = activePanels[i];
			if (activePanel != null)
			{
				activePanel.SetEnchantment(enchantment);
			}
			if (useUniqueEnchantmentsOnly)
			{
				Tuple<string, string> key = new Tuple<string, string>(enchantment.GetCardId(), enchantment.GetCardTextInHand());
				m_uniqueEnchantmentLookup.Add(key, activePanel);
			}
		}
		if (useUniqueEnchantmentsOnly)
		{
			HashSet<Tuple<string, string>> enchantmentSeen = new HashSet<Tuple<string, string>>();
			foreach (Entity enchantment2 in entity.GetDisplayedEnchantments())
			{
				Tuple<string, string> key2 = new Tuple<string, string>(enchantment2.GetCardId(), enchantment2.GetCardTextInHand());
				if (!enchantmentSeen.Contains(key2))
				{
					enchantmentSeen.Add(key2);
					continue;
				}
				uint amount = (uint)Mathf.Max(enchantment2.GetTag(GAME_TAG.SPAWN_TIME_COUNT), 1);
				m_uniqueEnchantmentLookup[key2].IncrementEnchantmentMultiplier(amount);
			}
		}
		LayoutEnchantments(enchantmentBone, card, bigCardActor, enchantmentScalingFactor);
		LayerUtils.SetLayer(enchantmentBone, GameLayer.Tooltip);
	}

	public void ResetEnchantments()
	{
		if (m_EnchantmentBanner != null)
		{
			m_EnchantmentBanner.SetActive(value: false);
			m_EnchantmentBanner.transform.parent = base.transform;
		}
		if (m_EnchantmentBannerBottom != null)
		{
			m_EnchantmentBannerBottom.SetActive(value: false);
			m_EnchantmentBannerBottom.transform.parent = base.transform;
		}
		if (m_EnchantmentBannerText != null)
		{
			m_EnchantmentBannerText.gameObject.SetActive(value: false);
			m_EnchantmentBannerText.transform.parent = base.transform;
		}
		if (m_EnchantmentBannerIcon != null)
		{
			m_EnchantmentBannerIcon.SetActive(value: false);
			m_EnchantmentBannerIcon.transform.parent = base.transform;
		}
		if (m_EnchantmentBannerCheckmark != null)
		{
			m_EnchantmentBannerCheckmark.gameObject.SetActive(value: false);
		}
		if (m_EnchantmentBannerXmark != null)
		{
			m_EnchantmentBannerXmark.gameObject.SetActive(value: false);
		}
		if (m_enchantmentPool == null)
		{
			return;
		}
		foreach (BigCardEnchantmentPanel activePanel in m_enchantmentPool.GetActiveList())
		{
			if (!(activePanel == null))
			{
				activePanel.transform.parent = base.transform;
				activePanel.ResetScale();
				activePanel.Hide();
			}
		}
	}

	private void LayoutEnchantments(GameObject bone, Card card, Actor bigCardActor, float enchantmentScalingFactor)
	{
		GameObject enchantmentBannerAnchorObject = ((bigCardActor.m_enchantmentBannerAnchorObject == null) ? bigCardActor.GetMeshRenderer().gameObject : bigCardActor.m_enchantmentBannerAnchorObject);
		float totalHeight = 0.1f;
		List<BigCardEnchantmentPanel> activeList = m_enchantmentPool.GetActiveList();
		BigCardEnchantmentPanel prevPanel = null;
		Entity entity = card.GetEntity();
		foreach (BigCardEnchantmentPanel activePanel in activeList)
		{
			if (activePanel == null)
			{
				continue;
			}
			activePanel.Show();
			activePanel.transform.localScale *= enchantmentScalingFactor;
			if (prevPanel == null)
			{
				Vector3 offset = new Vector3(0.01f, 0.01f, 0f);
				if (entity != null && entity.IsLettuceMercenary() && entity.GetRaceCount() >= 2)
				{
					offset.z -= 0.23f;
					totalHeight += 0.23f;
				}
				TransformUtil.SetPoint(activePanel.gameObject, new Vector3(0.5f, 0f, 1f), enchantmentBannerAnchorObject, new Vector3(0.5f, 0f, 0f), offset);
			}
			else
			{
				TransformUtil.SetPoint(activePanel.gameObject, new Vector3(0f, 0f, 1f), prevPanel.gameObject, new Vector3(0f, 0f, 0f), new Vector3(0f, 0f, 0f));
			}
			prevPanel = activePanel;
			activePanel.transform.parent = bone.transform;
			float panelHeight = activePanel.GetHeight();
			totalHeight += panelHeight;
		}
		GameDbf.GetIndex().GetClientString(entity.GetTag(GAME_TAG.ENCHANTMENT_BANNER_TEXT));
		bool hasEnchantmentBannerText = UpdateEnchantmentBannerText(entity, bone, prevPanel, enchantmentScalingFactor);
		UpdateCustomBannerIcon(entity);
		CreateCustomObjectiveGlows(bigCardActor);
		m_EnchantmentBannerText.gameObject.SetActive(hasEnchantmentBannerText);
		m_EnchantmentBanner.SetActive(value: true);
		m_EnchantmentBannerBottom.SetActive(value: true);
		m_EnchantmentBannerIcon.SetActive(value: true);
		if (hasEnchantmentBannerText)
		{
			m_EnchantmentBannerText.transform.localScale = m_initialBannerTextScale * enchantmentScalingFactor;
			m_EnchantmentBannerText.transform.parent = bone.transform;
			if (prevPanel == null)
			{
				m_EnchantmentBannerText.transform.localPosition = new Vector3(0f, 0f, -0.25f);
			}
			else
			{
				TransformUtil.SetPoint(m_EnchantmentBannerText.gameObject, new Vector3(0.5f, 0f, 1f), prevPanel.gameObject, new Vector3(0.5f, 0f, 0f), new Vector3(0f, 0f, -0.05f));
			}
			entity.ApplyCustomBannerTextOffset(m_EnchantmentBannerText);
			totalHeight += m_EnchantmentBannerText.Height;
		}
		m_EnchantmentBanner.transform.localEulerAngles = new Vector3(0f, 180f, 0f);
		TransformUtil.SetPoint(m_EnchantmentBanner, new Vector3(0.5f, 0f, 1f), enchantmentBannerAnchorObject, new Vector3(0.5f, 0f, 0f), new Vector3(0f, 0f, 0.2f));
		m_EnchantmentBanner.transform.localScale = m_initialBannerScale * enchantmentScalingFactor;
		TransformUtil.SetLocalScaleZ(m_EnchantmentBanner.gameObject, totalHeight / m_initialBannerHeight / m_initialBannerScale.z);
		m_EnchantmentBanner.transform.parent = bone.transform;
		m_EnchantmentBannerBottom.transform.localScale = m_initialBannerBottomScale * enchantmentScalingFactor;
		m_EnchantmentBannerBottom.transform.localEulerAngles = new Vector3(0f, 180f, 0f);
		TransformUtil.SetPoint(m_EnchantmentBannerBottom, Anchor.FRONT, m_EnchantmentBanner, Anchor.BACK);
		m_EnchantmentBannerBottom.transform.parent = bone.transform;
		m_EnchantmentBannerBottom.transform.position += new Vector3(0f, -0.01f, 0.01f);
		m_EnchantmentBannerIcon.transform.position = m_EnchantmentBanner.transform.position;
		m_EnchantmentBannerIcon.transform.localScale = m_initialBannerIconScale * enchantmentScalingFactor;
		m_EnchantmentBannerIcon.transform.parent = bone.transform;
	}

	private bool UpdateEnchantmentBannerText(Entity entity, GameObject bone, BigCardEnchantmentPanel prevPanel, float adjustedScalingFactor)
	{
		if (entity == null)
		{
			return false;
		}
		string customBannerTextString = "";
		if (entity.HasTag(GAME_TAG.ENCHANTMENT_BANNER_TEXT))
		{
			customBannerTextString = entity.GetCustomEnchantmentBannerText();
		}
		else if (entity.IsSideQuest())
		{
			customBannerTextString = entity.GetCustomSideQuestBannerText();
		}
		else
		{
			if (!entity.IsObjective())
			{
				return false;
			}
			customBannerTextString = entity.GetCustomObjectiveBannerText();
		}
		m_EnchantmentBannerText.Text = customBannerTextString;
		return !string.IsNullOrEmpty(m_EnchantmentBannerText.Text);
	}

	public bool IsBannerVisible()
	{
		return m_EnchantmentBanner.activeInHierarchy;
	}

	public int GetEnchantmentCount()
	{
		return m_enchantmentPool.GetActiveList().Count;
	}

	public Bounds GetLowerMeshBounds()
	{
		return m_EnchantmentBannerBottom.GetComponent<Renderer>().bounds;
	}

	public void UpdateCustomBannerIcon(Entity entity)
	{
		Icon iconType = entity?.GetCustomObjectiveBannerIcon() ?? Icon.Nothing;
		m_EnchantmentBannerCheckmark?.SetActive(iconType == Icon.Checkmark);
		m_EnchantmentBannerXmark?.SetActive(iconType == Icon.Xmark);
	}

	public void CreateCustomObjectiveGlows(Actor cardActor)
	{
		Entity entity = cardActor.GetEntity();
		if (entity == null)
		{
			return;
		}
		TAG_CARDTYPE cardType = entity.GetCardType();
		List<CardChangeDbfRecord> dummyChangeList = new List<CardChangeDbfRecord>();
		if (entity.GetCardId() == "ETC_335")
		{
			CardChangeDbfRecord dummyChange = new CardChangeDbfRecord();
			if (entity.GetTag(GAME_TAG.QUEST_PROGRESS) == 0)
			{
				dummyChange.SetChangeType(CardChange.ChangeType.NERF);
				dummyChange.SetTagId(184);
				dummyChangeList.Add(dummyChange);
			}
		}
		if (dummyChangeList.Count == 0)
		{
			return;
		}
		GameObject cardChangeGlow = AssetLoader.Get().InstantiatePrefab(ActorNames.GetNerfGlowsActor(cardType));
		if (cardChangeGlow != null)
		{
			CardNerfGlows nerfGlows = cardChangeGlow.GetComponent<CardNerfGlows>();
			if (nerfGlows != null)
			{
				TransformUtil.AttachAndPreserveLocalTransform(cardChangeGlow.transform, cardActor.transform);
				LayerUtils.SetLayer(nerfGlows, cardActor.gameObject.layer);
				nerfGlows.SetGlowsForCard(dummyChangeList);
			}
		}
	}
}
