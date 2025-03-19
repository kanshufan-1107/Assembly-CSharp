using Blizzard.T5.MaterialService.Extensions;
using UnityEngine;

public class BigCardEnchantmentPanel : MonoBehaviour
{
	public Actor m_Actor;

	public UberText m_HeaderText;

	public UberText m_BodyText;

	public GameObject m_Background;

	public Material m_FallbackEnchantmentPortrait;

	private Entity m_enchantment;

	private DefLoader.DisposableCardDef m_enchantmentCardDef;

	private DefLoader.DisposableCardDef m_creatorCardDef;

	private Vector3 m_initialScale;

	private float m_initialBackgroundHeight;

	private Vector3 m_initialBackgroundScale;

	private bool m_shown;

	private int m_multiplier = 1;

	private string m_header = "";

	private void Awake()
	{
		m_initialScale = base.transform.localScale;
		m_initialBackgroundHeight = m_Background.GetComponentInChildren<MeshRenderer>().bounds.size.z;
		m_initialBackgroundScale = m_Background.transform.localScale;
	}

	private void OnDestroy()
	{
		m_enchantmentCardDef?.Dispose();
		m_enchantmentCardDef = null;
		m_creatorCardDef?.Dispose();
		m_creatorCardDef = null;
	}

	public void SetEnchantment(Entity enchantment)
	{
		m_enchantment = enchantment;
		string cardId = m_enchantment.GetCardId();
		DefLoader.Get().LoadCardDef(cardId, OnEnchantmentCardDefLoaded, null, new CardPortraitQuality(1, m_enchantment.GetPremiumType()));
	}

	public void Show()
	{
		if (!m_shown)
		{
			m_shown = true;
			base.gameObject.SetActive(value: true);
			UpdateLayout();
		}
	}

	public void Hide()
	{
		if (m_shown)
		{
			m_shown = false;
			base.gameObject.SetActive(value: false);
		}
	}

	public void ResetScale()
	{
		base.transform.localScale = m_initialScale;
		m_Background.transform.localScale = m_initialBackgroundScale;
	}

	public bool IsShown()
	{
		return m_shown;
	}

	public float GetHeight()
	{
		return m_Background.GetComponentInChildren<MeshRenderer>().bounds.size.z;
	}

	private void OnEnchantmentCardDefLoaded(string cardId, DefLoader.DisposableCardDef cardDef, object callbackData)
	{
		bool materialLoaded = false;
		if (cardDef != null)
		{
			m_enchantmentCardDef?.Dispose();
			m_enchantmentCardDef = cardDef;
			Material fullHistoryPortrait;
			Texture portraitTexture;
			if (m_enchantmentCardDef.CardDef.TryGetEnchantmentPortrait(out var enchantmentPortrait))
			{
				m_Actor.GetMeshRenderer().SetMaterial(enchantmentPortrait);
				materialLoaded = true;
			}
			else if (m_enchantmentCardDef.CardDef.TryGetHistoryTileFullPortrait(m_Actor.GetPremium(), out fullHistoryPortrait))
			{
				m_Actor.GetMeshRenderer().SetMaterial(fullHistoryPortrait);
				materialLoaded = true;
			}
			else if (m_enchantmentCardDef.CardDef.TryGetPortraitTexture(m_Actor.GetPremium(), out portraitTexture))
			{
				m_Actor.SetPortraitTextureOverride(portraitTexture);
				materialLoaded = true;
			}
		}
		m_HeaderText.Text = m_enchantment.GetName();
		m_header = m_enchantment.GetName();
		SetMultiplier(Mathf.Max(m_enchantment.GetTag(GAME_TAG.SPAWN_TIME_COUNT), 1));
		m_BodyText.Text = m_enchantment.GetCardTextInHand();
		if (!materialLoaded)
		{
			LoadCreatorCardDef();
		}
	}

	private void LoadCreatorCardDef()
	{
		if (m_enchantment != null)
		{
			string creatorCardID = m_enchantment.GetEnchantmentCreatorCardIDForPortrait();
			if (string.IsNullOrEmpty(creatorCardID))
			{
				m_Actor.GetMeshRenderer().SetMaterial(m_FallbackEnchantmentPortrait);
			}
			else
			{
				DefLoader.Get().LoadCardDef(creatorCardID, OnCreatorCardDefLoaded, null, new CardPortraitQuality(1, m_enchantment.GetPremiumType()));
			}
		}
	}

	private void OnCreatorCardDefLoaded(string cardId, DefLoader.DisposableCardDef cardDef, object callbackData)
	{
		if (cardDef != null)
		{
			m_creatorCardDef?.Dispose();
			m_creatorCardDef = cardDef;
			Material fullHistoryPortrait;
			if (m_creatorCardDef.CardDef.TryGetEnchantmentPortrait(out var enchantmentPortrait))
			{
				m_Actor.GetMeshRenderer().SetMaterial(enchantmentPortrait);
			}
			else if (m_creatorCardDef.CardDef.TryGetHistoryTileFullPortrait(m_Actor.GetPremium(), out fullHistoryPortrait))
			{
				m_Actor.GetMeshRenderer().SetMaterial(fullHistoryPortrait);
			}
			else if (m_creatorCardDef.CardDef.GetPortraitTexture(m_Actor.GetPremium()) != null)
			{
				m_Actor.SetPortraitTextureOverride(m_creatorCardDef.CardDef.GetPortraitTexture(m_Actor.GetPremium()));
			}
		}
	}

	private void UpdateLayout()
	{
		m_HeaderText.UpdateNow();
		m_BodyText.UpdateNow();
		Bounds ZERO_BOUNDS = new Bounds(Vector3.zero, Vector3.zero);
		float maxZ = -99999f;
		float minZ = 99999f;
		Bounds actorBounds = m_Actor.GetMeshRenderer().bounds;
		Bounds headerBounds = m_HeaderText.GetTextWorldSpaceBounds();
		Bounds bodyBounds = m_BodyText.GetTextWorldSpaceBounds();
		if (actorBounds != ZERO_BOUNDS)
		{
			maxZ = Mathf.Max(maxZ, actorBounds.max.z);
			minZ = Mathf.Min(minZ, actorBounds.min.z);
		}
		if (headerBounds != ZERO_BOUNDS)
		{
			maxZ = Mathf.Max(maxZ, headerBounds.max.z);
			minZ = Mathf.Min(minZ, headerBounds.min.z);
		}
		if (bodyBounds != ZERO_BOUNDS)
		{
			maxZ = Mathf.Max(maxZ, bodyBounds.max.z);
			minZ = Mathf.Min(minZ, bodyBounds.min.z);
		}
		float scaleMultiplier = 1f;
		if (m_initialBackgroundHeight != 0f && (maxZ != -99999f || minZ != 99999f))
		{
			scaleMultiplier = (maxZ - minZ + 0.1f) / m_initialBackgroundHeight;
		}
		base.transform.localScale = m_initialScale;
		base.transform.localEulerAngles = Vector3.zero;
		TransformUtil.SetLocalScaleZ(m_Background, m_initialBackgroundScale.z * scaleMultiplier);
	}

	public string GetEnchantmentId()
	{
		if (m_enchantment == null)
		{
			return null;
		}
		return m_enchantment.GetCardId();
	}

	public Entity GetEnchantment()
	{
		return m_enchantment;
	}

	public void IncrementEnchantmentMultiplier(uint amount = 1u)
	{
		SetMultiplier(m_multiplier + (int)amount);
	}

	public void SetMultiplier(int multiplier)
	{
		m_multiplier = multiplier;
		if (m_multiplier > 1)
		{
			m_HeaderText.Text = GameStrings.Format("GAMEPLAY_ENCHANTMENT_MULTIPLIER_HEADER", m_multiplier, m_header);
		}
		else
		{
			m_HeaderText.Text = m_header;
		}
	}
}
