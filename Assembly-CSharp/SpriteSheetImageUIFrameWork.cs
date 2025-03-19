using Blizzard.T5.MaterialService.Extensions;
using Hearthstone.UI.Core;
using UnityEngine;

public class SpriteSheetImageUIFrameWork : MonoBehaviour
{
	private int m_spriteIndex;

	private Material spriteMaterial;

	[Overridable]
	public int spriteIndex
	{
		get
		{
			return m_spriteIndex;
		}
		set
		{
			m_spriteIndex = value;
			UpdateSprite();
		}
	}

	private void Start()
	{
		UpdateSprite();
	}

	private void UpdateSprite()
	{
		spriteMaterial = GetComponent<MeshRenderer>().GetMaterial();
		if (!(spriteMaterial == null))
		{
			float textureTilingX = spriteMaterial.mainTextureScale.x;
			float textureTilingY = spriteMaterial.mainTextureScale.y;
			int spritesPerRow = (int)(1f / textureTilingX);
			float offsetX = 0f;
			offsetX = ((spriteIndex <= spritesPerRow) ? ((float)spriteIndex * textureTilingX) : ((float)(spriteIndex % spritesPerRow) * textureTilingX));
			float offsetY = 1f - ((float)Mathf.CeilToInt(spriteIndex / spritesPerRow) * textureTilingY + textureTilingY);
			spriteMaterial.mainTextureOffset = new Vector2(offsetX, offsetY);
			spriteMaterial.renderQueue = RenderUtils.ClampRenderQueueValue(spriteMaterial.renderQueue + 1000);
		}
	}
}
