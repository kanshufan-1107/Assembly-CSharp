using System.Collections.Generic;
using Blizzard.T5.AssetManager;
using Blizzard.T5.MaterialService.Extensions;
using PegasusShared;
using UnityEngine;

[CustomEditClass]
public class DeckTemplatePickerButton : PegUIElement
{
	public MeshRenderer m_deckTexture;

	public MeshRenderer m_packRibbon;

	public GameObject m_selectGlow;

	public UberText m_title;

	public List<UberText> m_cardCountTexts = new List<UberText>();

	public GameObject m_incompleteTextRibbon;

	public GameObject m_completeTextRibbon;

	public static readonly int s_MinimumRecommendedSize = 30;

	private bool m_isCoreDeck;

	private int m_ownedCardCount;

	private AssetHandle<Texture> m_deckArtTextureHandle;

	private Material m_deckArtMaterial;

	public MeshRenderer m_packFlap;

	public MeshRenderer m_packInner;

	public Material m_packGoldenMaterial;

	public Material m_packCustomMaterial;

	public GameObject m_runes;

	public Material m_runeNoneMaterial;

	public Material m_runeBloodSlotMaterial;

	public Material m_runeFrostSlotMaterial;

	public Material m_runeUnholySlotMaterial;

	public Material m_runeBloodStandardMaterial;

	public Material m_runeFrostStandardMaterial;

	public Material m_runeUnholyStandardMaterial;

	public Material m_runeBloodGoldenMaterial;

	public Material m_runeFrostGoldenMaterial;

	public Material m_runeUnholyGoldenMaterial;

	public MeshRenderer m_rune1;

	public MeshRenderer m_rune2;

	public MeshRenderer m_rune3;

	public MeshRenderer m_slot1;

	public MeshRenderer m_slot2;

	public MeshRenderer m_slot3;

	protected override void OnDestroy()
	{
		UnloadDeckArt();
		base.OnDestroy();
	}

	public void SetIsCoreDeck(bool isCore)
	{
		m_isCoreDeck = isCore;
	}

	public bool IsCoreDeck()
	{
		return m_isCoreDeck;
	}

	public void SetSelected(bool selected)
	{
		if (m_selectGlow != null)
		{
			m_selectGlow.SetActive(selected);
		}
	}

	public void SetTitleText(string titleText)
	{
		if (m_title != null)
		{
			m_title.Text = titleText;
		}
	}

	public void SetCardCountText(int count, int total)
	{
		m_ownedCardCount = count;
		foreach (UberText cardCountText in m_cardCountTexts)
		{
			cardCountText.Text = $"{count}/{total}";
		}
		bool incomplete = count < s_MinimumRecommendedSize && !m_isCoreDeck;
		if (m_incompleteTextRibbon != null)
		{
			m_incompleteTextRibbon.SetActive(incomplete);
		}
		if (m_completeTextRibbon != null)
		{
			m_completeTextRibbon.SetActive(!incomplete);
		}
	}

	public int GetOwnedCardCount()
	{
		return m_ownedCardCount;
	}

	public void SetDeckArtByMaterialPath(string materialPath, DeckTemplateDbfRecord record)
	{
		UnloadDeckArt();
		if (!(m_deckTexture == null) && !string.IsNullOrEmpty(materialPath))
		{
			AssetLoader.Get().LoadMaterial(materialPath, SetDeckMaterial);
			bool isPremium = false;
			Material packMaterial = m_packCustomMaterial;
			if (isPremium)
			{
				packMaterial = m_packGoldenMaterial;
			}
			m_packFlap.SetMaterial(packMaterial);
			m_packInner.SetMaterial(packMaterial);
			SetAllRuneMaterials(record, isPremium);
		}
	}

	public void SetDeckArtByCardId(int cardId, Material sourceMaterial, DeckTemplateDbfRecord record)
	{
		UnloadDeckArt();
		if (m_deckTexture == null)
		{
			return;
		}
		using DefLoader.DisposableCardDef displayCard = DefLoader.Get().GetCardDef(cardId);
		if (displayCard != null)
		{
			AssetHandle.Set(ref m_deckArtTextureHandle, displayCard.CardDef.GetPortraitTextureHandle());
			m_deckArtMaterial = new Material(sourceMaterial);
			m_deckArtMaterial.mainTexture = m_deckArtTextureHandle;
			m_deckTexture.SetMaterial(m_deckArtMaterial);
			bool isPremium = false;
			Material packMaterial = m_packCustomMaterial;
			if (isPremium)
			{
				packMaterial = m_packGoldenMaterial;
			}
			m_packFlap.SetMaterial(packMaterial);
			m_packInner.SetMaterial(packMaterial);
			SetAllRuneMaterials(record, isPremium);
		}
	}

	private void SetAllRuneMaterials(DeckTemplateDbfRecord record, bool isPremium = false)
	{
		if (record == null)
		{
			m_runes.SetActive(value: false);
			return;
		}
		List<DkRuneListDbfRecord> dkRuneList = record.DKRunes;
		if (record.ClassId != 1 || dkRuneList.Count == 0)
		{
			m_runes.SetActive(value: false);
			return;
		}
		m_runes.SetActive(value: true);
		if (dkRuneList[0] != null)
		{
			RuneType runeType = (RuneType)dkRuneList[0].Rune;
			SetRuneMaterials(m_rune1, m_slot1, runeType, isPremium);
		}
		if (dkRuneList[1] != null)
		{
			RuneType runeType2 = (RuneType)dkRuneList[1].Rune;
			SetRuneMaterials(m_rune2, m_slot2, runeType2, isPremium);
		}
		if (dkRuneList[2] != null)
		{
			RuneType runeType3 = (RuneType)dkRuneList[2].Rune;
			SetRuneMaterials(m_rune3, m_slot3, runeType3, isPremium);
		}
	}

	private void SetRuneMaterials(MeshRenderer runeMeshRenderer, MeshRenderer slotMeshRenderer, RuneType runeType, bool isPremium = false)
	{
		Material runeMaterial = GetRuneMaterialForRuneType(runeType, isPremium);
		if ((bool)runeMaterial)
		{
			runeMeshRenderer.enabled = true;
			runeMeshRenderer.SetMaterial(runeMaterial);
		}
		else
		{
			runeMeshRenderer.enabled = false;
		}
		Material slotMaterial = GetSlotMaterialForRuneType(runeType);
		if ((bool)slotMaterial)
		{
			slotMeshRenderer.SetMaterial(slotMaterial);
		}
	}

	private Material GetRuneMaterialForRuneType(RuneType runeType, bool isPremium = false)
	{
		Material material = null;
		switch (runeType)
		{
		case RuneType.RT_NONE:
			return null;
		case RuneType.RT_BLOOD:
			material = (isPremium ? m_runeBloodGoldenMaterial : m_runeBloodStandardMaterial);
			break;
		case RuneType.RT_FROST:
			material = (isPremium ? m_runeFrostGoldenMaterial : m_runeFrostStandardMaterial);
			break;
		case RuneType.RT_UNHOLY:
			material = (isPremium ? m_runeUnholyGoldenMaterial : m_runeUnholyStandardMaterial);
			break;
		default:
			Debug.LogError("DeckTemplatePickerButton::GetMaterialForRuneType material for rune type not found.");
			break;
		}
		return material;
	}

	private Material GetSlotMaterialForRuneType(RuneType runeType)
	{
		Material material = null;
		switch (runeType)
		{
		case RuneType.RT_NONE:
			material = m_runeNoneMaterial;
			break;
		case RuneType.RT_BLOOD:
			material = m_runeBloodSlotMaterial;
			break;
		case RuneType.RT_FROST:
			material = m_runeFrostSlotMaterial;
			break;
		case RuneType.RT_UNHOLY:
			material = m_runeUnholySlotMaterial;
			break;
		default:
			Debug.LogError("DeckTemplatePickerButton::GetSlotMaterialForRuneType material for rune type not found.");
			break;
		}
		return material;
	}

	private void SetDeckMaterial(AssetReference assetRef, Object obj, object callbackData)
	{
		Material displayMaterial = obj as Material;
		if (displayMaterial != null)
		{
			m_deckTexture.SetMaterial(displayMaterial);
		}
	}

	private void UnloadDeckArt()
	{
		if (m_deckArtMaterial != null)
		{
			Object.Destroy(m_deckArtMaterial);
			m_deckArtMaterial = null;
		}
		AssetHandle.SafeDispose(ref m_deckArtTextureHandle);
	}
}
