using Blizzard.T5.Core;
using Blizzard.T5.MaterialService.Extensions;
using PegasusShared;
using UnityEngine;

public class CollectionCardCount : MonoBehaviour
{
	public GameObject m_normalTab;

	public UberText m_normalCountText;

	public GameObject m_normalBorder;

	public GameObject m_normalWideBorder;

	public GameObject m_normalBone;

	public GameObject m_goldenTab;

	public UberText m_goldenCountText;

	public GameObject m_goldenBorder;

	public GameObject m_goldenWideBorder;

	public GameObject m_goldenBone;

	public GameObject m_centerBone;

	[SerializeField]
	private Transform m_signature25Bone;

	[SerializeField]
	private Transform m_diamondBone;

	public Color m_standardTextColor;

	public Color m_wildTextColor;

	public Color m_classicTextColor;

	public Material m_standardBorderMaterial;

	public Material m_wildBorderMaterial;

	public Material m_classicBorderMaterial;

	public Color m_standardGoldTextColor;

	public Material m_standardGoldBorderMaterial;

	private TAG_PREMIUM m_premium;

	private int m_normalCount;

	private int m_goldenCount;

	private int m_signatureCount;

	private int m_diamondCount;

	private int m_signatureFrameId;

	public void SetData(int normalCount, int goldenCount, int signatureCount, int diamondCount, TAG_PREMIUM premium, int signatureFrameId)
	{
		m_normalCount = normalCount;
		m_goldenCount = goldenCount;
		m_signatureCount = signatureCount;
		m_diamondCount = diamondCount;
		m_premium = premium;
		m_signatureFrameId = signatureFrameId;
		UpdateVisibility();
	}

	public int GetCount(TAG_PREMIUM premium)
	{
		return premium switch
		{
			TAG_PREMIUM.NORMAL => m_normalCount, 
			TAG_PREMIUM.GOLDEN => m_goldenCount, 
			TAG_PREMIUM.SIGNATURE => m_signatureCount, 
			TAG_PREMIUM.DIAMOND => m_diamondCount, 
			_ => 0, 
		};
	}

	public void Show()
	{
		UpdateVisibility();
	}

	public void Hide()
	{
		base.gameObject.SetActive(value: false);
	}

	private void UpdateVisibility()
	{
		int count = GetCount(m_premium);
		if (count <= 1)
		{
			Hide();
			return;
		}
		base.gameObject.SetActive(value: true);
		m_normalTab.SetActive(value: false);
		m_normalBorder.SetActive(value: false);
		m_normalWideBorder.SetActive(value: false);
		m_goldenTab.SetActive(value: false);
		m_goldenBorder.SetActive(value: false);
		m_goldenWideBorder.SetActive(value: false);
		FormatType currentFormatType = CollectionManager.Get().GetThemeShowing();
		if (new Map<FormatType, Color>
		{
			{
				FormatType.FT_STANDARD,
				m_standardTextColor
			},
			{
				FormatType.FT_WILD,
				m_wildTextColor
			},
			{
				FormatType.FT_CLASSIC,
				m_classicTextColor
			},
			{
				FormatType.FT_TWIST,
				m_classicTextColor
			}
		}.TryGetValue(currentFormatType, out var color))
		{
			m_normalCountText.TextColor = color;
		}
		else
		{
			Debug.LogWarning("CollectionCardCount.UpdateVisibility failed to find text color for format" + currentFormatType);
		}
		if (new Map<FormatType, Material>
		{
			{
				FormatType.FT_STANDARD,
				m_standardBorderMaterial
			},
			{
				FormatType.FT_WILD,
				m_wildBorderMaterial
			},
			{
				FormatType.FT_CLASSIC,
				m_classicBorderMaterial
			},
			{
				FormatType.FT_TWIST,
				m_classicBorderMaterial
			}
		}.TryGetValue(currentFormatType, out var mat))
		{
			m_normalWideBorder.GetComponent<Renderer>().SetMaterial(mat);
			m_normalBorder.GetComponent<Renderer>().SetMaterial(mat);
		}
		else
		{
			Debug.LogWarning("CollectionCardCount.UpdateVisibility failed to find material for format" + currentFormatType);
		}
		m_goldenCountText.TextColor = m_standardGoldTextColor;
		m_goldenWideBorder.GetComponent<Renderer>().SetMaterial(m_standardGoldBorderMaterial);
		m_goldenBorder.GetComponent<Renderer>().SetMaterial(m_standardGoldBorderMaterial);
		if (count < 10)
		{
			m_normalBorder.SetActive(value: true);
			m_normalCountText.Text = GameStrings.Format("GLUE_COLLECTION_CARD_COUNT", count);
		}
		else if (count > 0)
		{
			m_normalWideBorder.SetActive(value: true);
			m_normalCountText.Text = GameStrings.Get("GLUE_COLLECTION_CARD_COUNT_LARGE");
		}
		if (count > 0)
		{
			m_normalTab.SetActive(value: true);
			if (m_premium == TAG_PREMIUM.SIGNATURE && m_signatureFrameId == 1 && m_signature25Bone != null)
			{
				m_normalTab.transform.position = m_signature25Bone.position;
			}
			else if (m_premium == TAG_PREMIUM.DIAMOND && m_diamondBone != null)
			{
				m_normalTab.transform.position = m_diamondBone.position;
			}
			else
			{
				m_normalTab.transform.position = m_centerBone.transform.position;
			}
		}
	}
}
