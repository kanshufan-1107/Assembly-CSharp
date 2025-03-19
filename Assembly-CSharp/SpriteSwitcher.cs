using System;
using System.Collections.Generic;
using Hearthstone.UI.Core;
using UnityEngine;

public class SpriteSwitcher : MonoBehaviour
{
	[Serializable]
	private struct SpriteMapping
	{
		public string spriteName;

		public Sprite sprite;
	}

	[SerializeField]
	private SpriteRenderer m_target;

	private string m_currentSpriteName;

	[SerializeField]
	private Sprite m_default;

	[SerializeField]
	private List<SpriteMapping> m_spriteMap;

	[Overridable]
	[SerializeField]
	public string CurrentSpriteName
	{
		get
		{
			return m_currentSpriteName;
		}
		set
		{
			m_currentSpriteName = value;
			RefreshSprite();
		}
	}

	private void Awake()
	{
		RefreshSprite();
	}

	private void RefreshSprite()
	{
		Sprite sprite = m_default;
		foreach (SpriteMapping spriteMapping in m_spriteMap)
		{
			if (spriteMapping.spriteName == CurrentSpriteName)
			{
				sprite = spriteMapping.sprite;
				break;
			}
		}
		if (!(sprite == null))
		{
			m_target.sprite = sprite;
		}
	}
}
