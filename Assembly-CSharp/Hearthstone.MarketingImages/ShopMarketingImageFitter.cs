using System;
using System.Collections.Generic;
using Hearthstone.UI;
using Hearthstone.UI.Core;
using UnityEngine;

namespace Hearthstone.MarketingImages;

public class ShopMarketingImageFitter : WidgetPositioningElement
{
	[SerializeField]
	[Header("Corners")]
	private SpriteRenderer m_topLeft;

	[SerializeField]
	private SpriteRenderer m_topRight;

	[SerializeField]
	private SpriteRenderer m_bottomRight;

	[SerializeField]
	private SpriteRenderer m_bottomLeft;

	[Header("Edges")]
	[SerializeField]
	private SpriteRenderer m_top;

	[SerializeField]
	private SpriteRenderer m_right;

	[SerializeField]
	private SpriteRenderer m_bottom;

	[SerializeField]
	private SpriteRenderer m_left;

	[SerializeField]
	[Header("Sprite renderer to set up")]
	private SpriteRenderer m_spriteRenderer;

	[Header("Padding")]
	[SerializeField]
	private float m_framedInflate;

	[SerializeField]
	private float m_framelessInflate;

	private Texture2D m_texture;

	private Vector2 m_textureNormOffset;

	private Sprite m_sprite;

	private bool m_showFrame;

	private List<SpriteRenderer> m_frameSprites = new List<SpriteRenderer>();

	[Overridable]
	public bool UpdateSizeToggle
	{
		get
		{
			return false;
		}
		set
		{
			if (value)
			{
				Refresh();
			}
		}
	}

	private void Awake()
	{
		if ((bool)m_topLeft)
		{
			m_frameSprites.Add(m_topLeft);
		}
		if ((bool)m_topRight)
		{
			m_frameSprites.Add(m_topRight);
		}
		if ((bool)m_bottomRight)
		{
			m_frameSprites.Add(m_bottomRight);
		}
		if ((bool)m_bottomLeft)
		{
			m_frameSprites.Add(m_bottomLeft);
		}
		if ((bool)m_top)
		{
			m_frameSprites.Add(m_top);
		}
		if ((bool)m_right)
		{
			m_frameSprites.Add(m_right);
		}
		if ((bool)m_bottom)
		{
			m_frameSprites.Add(m_bottom);
		}
		if ((bool)m_left)
		{
			m_frameSprites.Add(m_left);
		}
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		if ((bool)m_sprite)
		{
			UnityEngine.Object.Destroy(m_sprite);
			m_sprite = null;
		}
	}

	public void SetTexture(MarketingImageConfig mimgConfig)
	{
		if (mimgConfig != null)
		{
			m_texture = mimgConfig.Texture;
			m_textureNormOffset = mimgConfig.TextureOffset;
			m_showFrame = mimgConfig.AutoFrame;
		}
		else
		{
			m_texture = null;
			m_textureNormOffset = Vector2.zero;
			m_showFrame = true;
		}
		Refresh();
	}

	public void SetTexture(Texture2D texture, Vector2? offset = null)
	{
		m_texture = texture;
		m_textureNormOffset = offset ?? Vector2.zero;
		m_showFrame = true;
		Refresh();
	}

	protected override void InternalRefresh()
	{
		if (m_left == null || m_top == null || m_right == null || m_bottom == null || m_spriteRenderer == null)
		{
			return;
		}
		if (!m_texture || m_texture.width <= 0 || m_texture.height <= 0)
		{
			m_spriteRenderer.sprite = null;
			return;
		}
		int texWidth = m_texture.width;
		int texHeight = m_texture.height;
		Transform parent = m_spriteRenderer.transform.parent;
		Matrix4x4 worldToLocal = (parent ? parent.worldToLocalMatrix : Matrix4x4.identity);
		Vector3 left = worldToLocal.MultiplyPoint(m_left.bounds.center);
		Vector3 right = worldToLocal.MultiplyPoint(m_right.bounds.center);
		Vector3 top = worldToLocal.MultiplyPoint(m_top.bounds.center);
		Vector3 bottom = worldToLocal.MultiplyPoint(m_bottom.bounds.center);
		float slotWidth;
		float slotHeight;
		if (m_showFrame)
		{
			foreach (SpriteRenderer frameSprite in m_frameSprites)
			{
				frameSprite.enabled = true;
			}
			slotWidth = (right - left).x + 2f * m_framedInflate;
			slotHeight = (top - bottom).z + 2f * m_framedInflate;
		}
		else
		{
			foreach (SpriteRenderer frameSprite2 in m_frameSprites)
			{
				frameSprite2.enabled = false;
			}
			slotWidth = (right - left).x + 2f * m_framelessInflate;
			slotHeight = (top - bottom).z + 2f * m_framelessInflate;
		}
		if (slotWidth <= 0f || slotHeight <= 0f)
		{
			m_spriteRenderer.sprite = null;
			return;
		}
		float scale = Mathf.Max(slotWidth / (float)texWidth, slotHeight / (float)texHeight);
		float invScale = 1f / scale;
		float scaledWidth = Math.Min(slotWidth * invScale, texWidth);
		float scaledHeight = Math.Min(slotHeight * invScale, texHeight);
		Rect rect = new Rect(0f, 0f, scaledWidth, scaledHeight);
		if (m_textureNormOffset.sqrMagnitude > 0f)
		{
			float maxDX = 0.5f * ((float)texWidth - rect.width);
			float maxDY = 0.5f * ((float)texHeight - rect.height);
			float texDX = Mathf.Clamp((float)texWidth * m_textureNormOffset.x, 0f - maxDX, maxDX);
			float texDY = Mathf.Clamp((float)texHeight * m_textureNormOffset.y, 0f - maxDY, maxDY);
			rect.center = new Vector2((float)texWidth * 0.5f + texDX, (float)texHeight * 0.5f + texDY);
		}
		else
		{
			rect.center = new Vector2((float)texWidth * 0.5f, (float)texHeight * 0.5f);
		}
		Sprite sprite = Sprite.Create(pivot: new Vector2(0.5f, 0.5f), texture: m_texture, rect: rect, pixelsPerUnit: invScale);
		sprite.name = "MarketingImage (" + m_texture.name + ")";
		m_spriteRenderer.sprite = sprite;
		if ((bool)m_sprite)
		{
			UnityEngine.Object.Destroy(m_sprite);
		}
		m_sprite = sprite;
	}
}
