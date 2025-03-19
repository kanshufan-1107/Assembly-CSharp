using UnityEngine;

namespace Hearthstone.UI;

public class SpriteRendererDynamicPropertyResolverProxy : RendererDynamicPropertyResolverProxy
{
	private SpriteRenderer m_spriteRenderer;

	public override void SetTarget(object target)
	{
		base.SetTarget(target);
		m_spriteRenderer = (SpriteRenderer)target;
		m_properties.Add(new DynamicPropertyInfo
		{
			Id = "flipX",
			Name = "FlipX",
			Type = typeof(bool),
			Value = m_spriteRenderer.flipX
		});
		m_properties.Add(new DynamicPropertyInfo
		{
			Id = "flipY",
			Name = "FlipY",
			Type = typeof(bool),
			Value = m_spriteRenderer.flipY
		});
		m_properties.Add(new DynamicPropertyInfo
		{
			Id = "color",
			Name = "Color",
			Type = typeof(Color),
			Value = m_spriteRenderer.color
		});
	}

	public override bool GetDynamicPropertyValue(string id, out object value)
	{
		if (base.GetDynamicPropertyValue(id, out value))
		{
			return true;
		}
		switch (id)
		{
		case "flipX":
			value = m_spriteRenderer.flipX;
			return true;
		case "flipY":
			value = m_spriteRenderer.flipY;
			return true;
		case "color":
			value = m_spriteRenderer.color;
			return true;
		default:
			return false;
		}
	}

	public override bool SetDynamicPropertyValue(string id, object value)
	{
		if (base.SetDynamicPropertyValue(id, value))
		{
			return true;
		}
		switch (id)
		{
		case "flipX":
			m_spriteRenderer.flipX = (bool)value;
			return true;
		case "flipY":
			m_spriteRenderer.flipY = (bool)value;
			return true;
		case "color":
			m_spriteRenderer.color = (Color)value;
			return true;
		default:
			return false;
		}
	}
}
