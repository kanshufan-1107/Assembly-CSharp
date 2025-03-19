using System;
using UnityEngine;

namespace Hearthstone.UI;

[Serializable]
public class SpriteItemParameters : SpawnableLibraryItemParameters
{
	[SerializeField]
	private WeakAssetReference m_spriteAtlas;

	[SerializeField]
	private Color m_color = Color.white;

	[SerializeField]
	private WeakAssetReference m_material;

	[SerializeField]
	private bool m_flipX;

	[SerializeField]
	private bool m_flipY;

	[SerializeField]
	private SpriteDrawMode m_drawMode;

	[SerializeField]
	private Vector2 m_size = Vector2.one;

	[SerializeField]
	private SpriteTileMode m_tileMode;

	[SerializeField]
	private float m_tileStretchValue = 0.5f;

	[SerializeField]
	private SpriteMaskInteraction m_maskInteraction;

	[SerializeField]
	private SpriteSortPoint m_sortPoint;

	[SerializeField]
	private int m_sortingOrder;

	public override SpawnableLibrary.ItemType ItemType => SpawnableLibrary.ItemType.Sprite;

	public WeakAssetReference SpriteAtlasReference => m_spriteAtlas;

	public Color Color => m_color;

	public WeakAssetReference MaterialReference => m_material;

	public bool FlipX => m_flipX;

	public bool FlipY => m_flipY;

	public SpriteDrawMode DrawMode => m_drawMode;

	public Vector2 Size => m_size;

	public SpriteTileMode TileMode => m_tileMode;

	public float TileStretchValue => m_tileStretchValue;

	public SpriteMaskInteraction MaskInteraction => m_maskInteraction;

	public SpriteSortPoint SortPoint => m_sortPoint;

	public int SortingOrder => m_sortingOrder;
}
