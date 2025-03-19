using Blizzard.T5.MaterialService.Extensions;
using Hearthstone.UI.Core;
using UnityEngine;

public class ShopHeroClassIconLoader : MonoBehaviour
{
	[Tooltip("Optional mesh renderer for setting a class icon.")]
	[SerializeField]
	private MeshRenderer m_classIconMeshRenderer;

	private string m_cardId;

	[Overridable]
	public string CardId
	{
		get
		{
			return m_cardId;
		}
		set
		{
			if (!(m_cardId == value) && !string.IsNullOrEmpty(value))
			{
				m_cardId = value;
				SetClassIcon();
			}
		}
	}

	private void SetClassIcon()
	{
		if (!(m_classIconMeshRenderer == null) && !string.IsNullOrEmpty(CardId))
		{
			TAG_CLASS classTag = GameUtils.GetTagClassFromCardId(CardId);
			Vector2 textureOffset = CollectionPageManager.s_classTextureOffsets[classTag];
			(Application.isPlaying ? m_classIconMeshRenderer.GetMaterial() : m_classIconMeshRenderer.GetSharedMaterial()).SetTextureOffset("_MainTex", textureOffset);
		}
	}
}
