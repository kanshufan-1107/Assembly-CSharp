using Assets;
using Blizzard.T5.MaterialService.Extensions;
using UnityEngine;

public class DeckReward : Reward
{
	protected int DeckId;

	protected int ClassId;

	protected string DeckNameOverride;

	private DefLoader.DisposableCardDef m_heroCardDef;

	public UberText deckNameWild;

	public UberText deckNameStandard;

	public UberText deckNameTwist;

	public GameObject deckFrameWild;

	public GameObject deckFrameStandard;

	public GameObject deckFrameTwist;

	public MeshRenderer deckMeshWild;

	public MeshRenderer deckMeshTwist;

	public MeshRenderer deckMeshStandard;

	protected override void InitData()
	{
		SetData(new DeckRewardData(0, 0, 0, null), updateVisuals: false);
	}

	protected override void ShowReward(bool updateCacheValues)
	{
		if (!(base.Data is DeckRewardData))
		{
			Debug.LogWarning($"SimpleReward.ShowReward() - Data {base.Data} is not SimpleRewardData");
			return;
		}
		Vector3 endScale = m_root.transform.localScale;
		m_root.SetActive(value: true);
		m_root.transform.localScale = Vector3.zero;
		iTween.ScaleTo(m_root, iTween.Hash("scale", endScale, "time", 0.5f, "easetype", iTween.EaseType.easeOutElastic));
	}

	protected override void HideReward()
	{
		base.HideReward();
		m_root.SetActive(value: false);
	}

	protected override void OnDataSet(bool updateVisuals)
	{
		if (!updateVisuals || !(base.Data is DeckRewardData { DeckTemplateId: var deckTemplateId } rewardData))
		{
			return;
		}
		DeckId = rewardData.DeckId;
		ClassId = rewardData.ClassId;
		DeckNameOverride = rewardData.DeckNameOverride;
		DeckTemplate.FormatType deckFormat = DeckTemplate.FormatType.FT_UNKNOWN;
		if (deckTemplateId != 0)
		{
			DeckTemplateDbfRecord deckTemplateRecord = GameDbf.DeckTemplate.GetRecord(deckTemplateId);
			if (deckTemplateRecord != null && (deckTemplateRecord.FormatType == DeckTemplate.FormatType.FT_TWIST || deckTemplateRecord.FormatType == DeckTemplate.FormatType.FT_WILD))
			{
				deckFormat = deckTemplateRecord.FormatType;
			}
		}
		if (deckFormat == DeckTemplate.FormatType.FT_UNKNOWN)
		{
			deckFormat = (GameUtils.DeckIncludesRotatedCards(DeckId) ? DeckTemplate.FormatType.FT_WILD : DeckTemplate.FormatType.FT_STANDARD);
		}
		SetupVisualsForFormat(deckFormat);
		UberText uberText = deckNameWild;
		UberText uberText2 = deckNameStandard;
		string text = (deckNameTwist.Text = GetDeckName());
		string text3 = (uberText2.Text = text);
		uberText.Text = text3;
	}

	private void SetupVisualsForFormat(DeckTemplate.FormatType deckFormat)
	{
		switch (deckFormat)
		{
		case DeckTemplate.FormatType.FT_TWIST:
			deckFrameStandard.SetActive(value: false);
			deckFrameWild.SetActive(value: false);
			deckMeshTwist.SetMaterial(GetClassMaterial());
			break;
		case DeckTemplate.FormatType.FT_WILD:
			deckFrameStandard.SetActive(value: false);
			deckFrameTwist.SetActive(value: false);
			deckMeshWild.SetMaterial(GetClassMaterial());
			break;
		default:
			deckFrameWild.SetActive(value: false);
			deckFrameTwist.SetActive(value: false);
			deckMeshStandard.SetMaterial(GetClassMaterial());
			break;
		}
	}

	private Material GetClassMaterial()
	{
		ReleaseCardDef();
		string heroCardId = CollectionManager.GetVanillaHero((TAG_CLASS)ClassId);
		m_heroCardDef = DefLoader.Get().GetCardDef(heroCardId);
		return m_heroCardDef.CardDef.GetCustomDeckPortrait();
	}

	private string GetDeckName()
	{
		if (!string.IsNullOrEmpty(DeckNameOverride))
		{
			return DeckNameOverride;
		}
		return GameDbf.Deck.GetRecord(DeckId).Name;
	}

	protected override void OnDestroy()
	{
		ReleaseCardDef();
		base.OnDestroy();
	}

	private void ReleaseCardDef()
	{
		m_heroCardDef?.Dispose();
		m_heroCardDef = null;
	}
}
