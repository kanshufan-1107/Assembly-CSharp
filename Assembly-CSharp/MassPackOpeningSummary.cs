using System;
using System.Collections.Generic;
using System.Linq;
using Hearthstone.DataModels;
using Hearthstone.UI;
using UnityEngine;

public class MassPackOpeningSummary : MonoBehaviour
{
	private enum CLASS
	{
		INVALID,
		DEATHKNIGHT,
		DEMONHUNTER,
		DRUID,
		HUNTER,
		MAGE,
		PALADIN,
		PRIEST,
		ROGUE,
		SHAMAN,
		WARLOCK,
		WARRIOR,
		MULTICLASS,
		NEUTRAL
	}

	public class PremiumAndCount
	{
		public TAG_PREMIUM premium;

		public int count;

		public PremiumAndCount(TAG_PREMIUM prem)
		{
			premium = prem;
			count = 1;
		}
	}

	[SerializeField]
	private Spawnable m_topSpawnable;

	[SerializeField]
	private Spawnable m_leftSpawnable;

	[SerializeField]
	private Spawnable m_rightSpawnable;

	[SerializeField]
	private Spawnable m_bottomSpawnable;

	private GameObject m_buttonDone;

	[SerializeField]
	private GameObject m_buttonDonePC;

	[SerializeField]
	private GameObject m_buttonDonePhone;

	[SerializeField]
	private VisualController m_bannerVisualController;

	private const string CODE_DONE_BUTTON_PRESSED = "button_floaty_clicked";

	private bool m_doneButtonPressed;

	private MassPackOpeningSummaryDataModel m_dataModel;

	private int m_numPacksOpened;

	private List<NetCache.BoosterCard> m_cards;

	private const int LEGENDARY_FALLBACK_THRESHOLD = 12;

	public void Awake()
	{
		if ((bool)UniversalInputManager.UsePhoneUI)
		{
			m_buttonDone = m_buttonDonePhone;
		}
		else
		{
			m_buttonDone = m_buttonDonePC;
		}
	}

	public bool IsDoneButtonShowing()
	{
		if (m_buttonDone != null)
		{
			return m_buttonDone.activeInHierarchy;
		}
		return false;
	}

	public void SetNumPacksOpened(int numPacks)
	{
		m_numPacksOpened = numPacks;
	}

	public void SetCardsOpened(List<NetCache.BoosterCard> cards)
	{
		m_cards = cards;
	}

	public void InitDataModel()
	{
		m_doneButtonPressed = false;
		PegUI.Get().RegisterForRenderPassPriorityHitTest(this);
		m_dataModel = new MassPackOpeningSummaryDataModel();
		m_dataModel.NumPacksOpened = m_numPacksOpened;
		Dictionary<TAG_RARITY, Dictionary<CLASS, PremiumAndCount>> dataModelDictionary = new Dictionary<TAG_RARITY, Dictionary<CLASS, PremiumAndCount>>();
		dataModelDictionary.Add(TAG_RARITY.COMMON, new Dictionary<CLASS, PremiumAndCount>());
		dataModelDictionary.Add(TAG_RARITY.RARE, new Dictionary<CLASS, PremiumAndCount>());
		dataModelDictionary.Add(TAG_RARITY.EPIC, new Dictionary<CLASS, PremiumAndCount>());
		List<NetCache.BoosterCard> legendaryList = new List<NetCache.BoosterCard>();
		foreach (NetCache.BoosterCard card in m_cards)
		{
			EntityDef entity = DefLoader.Get().GetEntityDef(card.Def.Name);
			if (entity == null)
			{
				Log.All.PrintWarning("Card has no entity [" + card.Def.Name + "]");
				continue;
			}
			TAG_RARITY rarity = entity.GetRarity();
			if (rarity == TAG_RARITY.LEGENDARY)
			{
				legendaryList.Add(card);
				CardDataModel cardDataModel = new CardDataModel
				{
					Premium = card.Def.Premium,
					Rarity = rarity.ToString(),
					Name = entity.GetName(),
					CardId = entity.GetCardId()
				};
				m_dataModel.LegendariesOpened.Add(cardDataModel);
				continue;
			}
			CLASS cardClass = GetClass(entity);
			dataModelDictionary.TryGetValue(rarity, out var dictionary);
			if (dictionary == null)
			{
				continue;
			}
			if (dictionary.TryGetValue(cardClass, out var value))
			{
				if (value != null)
				{
					value.count++;
					if (value.premium < card.Def.Premium)
					{
						value.premium = card.Def.Premium;
					}
				}
			}
			else
			{
				dictionary.Add(cardClass, new PremiumAndCount(card.Def.Premium));
			}
		}
		m_dataModel.NumLegendariesOpened = legendaryList.Count;
		if (legendaryList.Count >= 12)
		{
			dataModelDictionary.Add(TAG_RARITY.LEGENDARY, MakeLegendaryFallbackDictionary(legendaryList));
			m_dataModel.LegendariesOpened.Clear();
		}
		foreach (CLASS c in Enum.GetValues(typeof(CLASS)))
		{
			foreach (KeyValuePair<TAG_RARITY, Dictionary<CLASS, PremiumAndCount>> item in dataModelDictionary)
			{
				TAG_RARITY rarity2 = item.Key;
				Dictionary<CLASS, PremiumAndCount> dictionary2 = item.Value;
				if (dictionary2.ContainsKey(c))
				{
					InsertClassIntoDataModel(c, dictionary2, rarity2);
				}
			}
		}
		List<TAG_RARITY> bannerOrder = new List<TAG_RARITY>();
		foreach (KeyValuePair<TAG_RARITY, Dictionary<CLASS, PremiumAndCount>> item2 in dataModelDictionary)
		{
			if (item2.Value.Count > 0)
			{
				bannerOrder.Add(item2.Key);
			}
		}
		if (legendaryList.Count > 0 && legendaryList.Count < 12)
		{
			bannerOrder.Add(TAG_RARITY.LEGENDARY);
		}
		bannerOrder = DetermineBannerOrder(bannerOrder);
		foreach (TAG_RARITY banner in bannerOrder)
		{
			m_dataModel.BannerTypeOrder.Add(banner.ToString());
		}
	}

	private Dictionary<CLASS, PremiumAndCount> MakeLegendaryFallbackDictionary(List<NetCache.BoosterCard> legendaryList)
	{
		Dictionary<CLASS, PremiumAndCount> legendaryDictionary = new Dictionary<CLASS, PremiumAndCount>();
		foreach (NetCache.BoosterCard card in legendaryList)
		{
			EntityDef entity = DefLoader.Get().GetEntityDef(card.Def.Name);
			CLASS cardClass = GetClass(entity);
			if (legendaryDictionary.TryGetValue(cardClass, out var value))
			{
				if (value != null)
				{
					value.count++;
					if (value.premium < card.Def.Premium)
					{
						value.premium = card.Def.Premium;
					}
				}
			}
			else
			{
				legendaryDictionary.Add(cardClass, new PremiumAndCount(card.Def.Premium));
			}
		}
		return legendaryDictionary;
	}

	public MassPackOpeningSummaryDataModel GetDataModel()
	{
		return m_dataModel;
	}

	public void PressDoneButton()
	{
		if (m_bannerVisualController != null && !m_doneButtonPressed && m_bannerVisualController.HasState("button_floaty_clicked"))
		{
			PegUI.Get().UnregisterFromRenderPassPriorityHitTest(this);
			m_bannerVisualController.SetState("button_floaty_clicked");
			m_doneButtonPressed = true;
		}
	}

	private void InsertClassIntoDataModel(CLASS classToInsert, Dictionary<CLASS, PremiumAndCount> dictionary, TAG_RARITY rarity)
	{
		if (dictionary.ContainsKey(classToInsert))
		{
			ClassCardCountDataModel cardCountDataModel = new ClassCardCountDataModel
			{
				ClassName = GetClassName(classToInsert),
				CardCount = dictionary[classToInsert].count,
				HighestPremium = dictionary[classToInsert].premium
			};
			switch (rarity)
			{
			case TAG_RARITY.COMMON:
				m_dataModel.CommonsOpened.Add(cardCountDataModel);
				break;
			case TAG_RARITY.RARE:
				m_dataModel.RaresOpened.Add(cardCountDataModel);
				break;
			case TAG_RARITY.EPIC:
				m_dataModel.EpicsOpened.Add(cardCountDataModel);
				break;
			case TAG_RARITY.LEGENDARY:
				m_dataModel.LegendariesOpenedFallback.Add(cardCountDataModel);
				break;
			case TAG_RARITY.FREE:
				break;
			}
		}
	}

	private List<TAG_RARITY> DetermineBannerOrder(List<TAG_RARITY> bannerOrder)
	{
		if ((bool)UniversalInputManager.UsePhoneUI)
		{
			return bannerOrder.OrderBy((TAG_RARITY x) => (int)x).ToList();
		}
		if (bannerOrder.Count == 4)
		{
			bannerOrder[0] = TAG_RARITY.LEGENDARY;
			bannerOrder[1] = TAG_RARITY.RARE;
			bannerOrder[2] = TAG_RARITY.EPIC;
			bannerOrder[3] = TAG_RARITY.COMMON;
			return bannerOrder;
		}
		bool hasLegendary = bannerOrder.Contains(TAG_RARITY.LEGENDARY);
		bool hasEpic = bannerOrder.Contains(TAG_RARITY.EPIC);
		bool hasRare = bannerOrder.Contains(TAG_RARITY.RARE);
		bool hasCommon = bannerOrder.Contains(TAG_RARITY.COMMON);
		if (bannerOrder.Count == 3)
		{
			if (hasLegendary)
			{
				if (hasCommon)
				{
					if (hasRare)
					{
						bannerOrder[0] = TAG_RARITY.LEGENDARY;
						bannerOrder[2] = TAG_RARITY.RARE;
						bannerOrder[1] = TAG_RARITY.COMMON;
					}
					else if (hasEpic)
					{
						bannerOrder[0] = TAG_RARITY.LEGENDARY;
						bannerOrder[2] = TAG_RARITY.EPIC;
						bannerOrder[1] = TAG_RARITY.COMMON;
					}
				}
				else
				{
					bannerOrder[0] = TAG_RARITY.LEGENDARY;
					bannerOrder[2] = TAG_RARITY.EPIC;
					bannerOrder[1] = TAG_RARITY.RARE;
				}
			}
			else
			{
				bannerOrder[0] = TAG_RARITY.EPIC;
				bannerOrder[2] = TAG_RARITY.RARE;
				bannerOrder[1] = TAG_RARITY.COMMON;
			}
		}
		if (bannerOrder.Count == 2)
		{
			if (hasLegendary)
			{
				if (hasEpic)
				{
					bannerOrder[0] = TAG_RARITY.LEGENDARY;
					bannerOrder[1] = TAG_RARITY.EPIC;
				}
				else if (hasRare)
				{
					bannerOrder[0] = TAG_RARITY.LEGENDARY;
					bannerOrder[1] = TAG_RARITY.RARE;
				}
				else if (hasCommon)
				{
					bannerOrder[0] = TAG_RARITY.LEGENDARY;
					bannerOrder[1] = TAG_RARITY.COMMON;
				}
			}
			else if (hasCommon)
			{
				if (hasEpic)
				{
					bannerOrder[0] = TAG_RARITY.EPIC;
					bannerOrder[1] = TAG_RARITY.COMMON;
				}
				else if (hasRare)
				{
					bannerOrder[0] = TAG_RARITY.RARE;
					bannerOrder[1] = TAG_RARITY.COMMON;
				}
			}
			else
			{
				bannerOrder[0] = TAG_RARITY.EPIC;
				bannerOrder[1] = TAG_RARITY.RARE;
			}
		}
		return bannerOrder;
	}

	private CLASS GetClass(EntityDef entity)
	{
		if (entity == null)
		{
			return CLASS.INVALID;
		}
		switch (entity.GetClass())
		{
		case TAG_CLASS.INVALID:
			return CLASS.INVALID;
		case TAG_CLASS.DEATHKNIGHT:
			return CLASS.DEATHKNIGHT;
		case TAG_CLASS.DRUID:
			return CLASS.DRUID;
		case TAG_CLASS.HUNTER:
			return CLASS.HUNTER;
		case TAG_CLASS.MAGE:
			return CLASS.MAGE;
		case TAG_CLASS.PALADIN:
			return CLASS.PALADIN;
		case TAG_CLASS.PRIEST:
			return CLASS.PRIEST;
		case TAG_CLASS.ROGUE:
			return CLASS.ROGUE;
		case TAG_CLASS.SHAMAN:
			return CLASS.SHAMAN;
		case TAG_CLASS.WARLOCK:
			return CLASS.WARLOCK;
		case TAG_CLASS.WARRIOR:
			return CLASS.WARRIOR;
		case TAG_CLASS.DEMONHUNTER:
			return CLASS.DEMONHUNTER;
		case TAG_CLASS.NEUTRAL:
			if (entity.IsMultiClass())
			{
				return CLASS.MULTICLASS;
			}
			return CLASS.NEUTRAL;
		default:
			return CLASS.INVALID;
		}
	}

	private string GetClassName(CLASS tagClass)
	{
		return tagClass switch
		{
			CLASS.INVALID => "INVALID", 
			CLASS.DEATHKNIGHT => GameStrings.GetClassName(TAG_CLASS.DEATHKNIGHT), 
			CLASS.DEMONHUNTER => GameStrings.GetClassName(TAG_CLASS.DEMONHUNTER), 
			CLASS.DRUID => GameStrings.GetClassName(TAG_CLASS.DRUID), 
			CLASS.HUNTER => GameStrings.GetClassName(TAG_CLASS.HUNTER), 
			CLASS.MAGE => GameStrings.GetClassName(TAG_CLASS.MAGE), 
			CLASS.PALADIN => GameStrings.GetClassName(TAG_CLASS.PALADIN), 
			CLASS.PRIEST => GameStrings.GetClassName(TAG_CLASS.PRIEST), 
			CLASS.ROGUE => GameStrings.GetClassName(TAG_CLASS.ROGUE), 
			CLASS.SHAMAN => GameStrings.GetClassName(TAG_CLASS.SHAMAN), 
			CLASS.WARLOCK => GameStrings.GetClassName(TAG_CLASS.WARLOCK), 
			CLASS.WARRIOR => GameStrings.GetClassName(TAG_CLASS.WARRIOR), 
			CLASS.MULTICLASS => GameStrings.Get("GLUE_MASS_PACK_OPEN_MULTI_CLASS"), 
			CLASS.NEUTRAL => GameStrings.GetClassName(TAG_CLASS.NEUTRAL), 
			_ => "INVALID", 
		};
	}
}
