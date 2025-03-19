using System.Collections.Generic;
using Assets;
using Blizzard.T5.MaterialService.Extensions;
using UnityEngine;

public class CardNerfGlows : MonoBehaviour
{
	[Header("Buff/Nerf Group 1")]
	[SerializeField]
	private Material m_buffMaterial;

	[SerializeField]
	private Material m_nerfMaterial;

	[SerializeField]
	private GameObject m_attack;

	[SerializeField]
	private GameObject m_health;

	[SerializeField]
	private GameObject m_manaCost;

	[SerializeField]
	private GameObject m_rarityGem;

	[SerializeField]
	private GameObject m_art;

	[SerializeField]
	private GameObject m_cardText;

	[SerializeField]
	private GameObject m_cardName;

	[SerializeField]
	private GameObject m_race;

	[SerializeField]
	private GameObject m_armor;

	[Header("Buff/Nerf Group 2")]
	[SerializeField]
	private Material m_buffMaterial2;

	[SerializeField]
	private Material m_nerfMaterial2;

	[SerializeField]
	private GameObject m_dkRunes;

	private void Awake()
	{
		HideAll();
	}

	public void SetGlowsForCard(List<CardChangeDbfRecord> cardChanges)
	{
		HideAll();
		if (cardChanges == null)
		{
			return;
		}
		foreach (CardChangeDbfRecord change in cardChanges)
		{
			if (change.ChangeType == CardChange.ChangeType.BUFF || change.ChangeType == CardChange.ChangeType.NERF)
			{
				Material matToUse = ((change.ChangeType == CardChange.ChangeType.BUFF) ? m_buffMaterial : m_nerfMaterial);
				Material matToUse2 = ((change.ChangeType == CardChange.ChangeType.BUFF) ? m_buffMaterial2 : m_nerfMaterial2);
				switch (change.TagId)
				{
				case 47:
					m_attack.GetComponent<Renderer>().SetMaterial(matToUse);
					m_attack.SetActive(value: true);
					break;
				case 184:
					m_cardText.GetComponent<Renderer>().SetMaterial(matToUse);
					m_cardText.SetActive(value: true);
					break;
				case 45:
					m_health.GetComponent<Renderer>().SetMaterial(matToUse);
					m_health.SetActive(value: true);
					break;
				case 48:
					m_manaCost.GetComponent<Renderer>().SetMaterial(matToUse);
					m_manaCost.SetActive(value: true);
					break;
				case 292:
					m_armor.GetComponent<Renderer>().SetMaterial(matToUse);
					m_armor.SetActive(value: true);
					break;
				case 2196:
				case 2197:
				case 2198:
					m_dkRunes.GetComponent<Renderer>().SetMaterial(matToUse2);
					m_dkRunes.SetActive(value: true);
					break;
				}
			}
		}
	}

	private void HideAll()
	{
		foreach (Transform item in base.transform)
		{
			item.gameObject.SetActive(value: false);
		}
	}
}
