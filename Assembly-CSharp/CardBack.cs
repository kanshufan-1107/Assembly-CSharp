using System;
using UnityEngine;

public class CardBack : MonoBehaviour
{
	[Serializable]
	public struct CustomDeckMeshes
	{
		public Mesh ThicknessFull;

		public Mesh Thickness75;

		public Mesh Thickness50;

		public Mesh Thickness25;

		public Mesh Thickness1;

		public bool IsComplete
		{
			get
			{
				if (ThicknessFull != null && Thickness75 != null && Thickness50 != null && Thickness25 != null)
				{
					return Thickness1 != null;
				}
				return false;
			}
		}
	}

	public enum cardBackHelpers
	{
		None,
		CardBackHelperBubbleLevel,
		CardBackHelperFlipbook
	}

	[Tooltip("Mesh for hidden card")]
	public Mesh m_CardBackMesh;

	[Tooltip("Material for hidden card")]
	public Material m_CardBackMaterial;

	[Tooltip("2nd Material for effects")]
	public Material m_CardBackMaterial1;

	[Tooltip("Alternative card back material for actors")]
	public Material m_CardBackMaterial2D;

	[Tooltip("Flat texture for decks and back of cards")]
	public Texture2D m_CardBackTexture;

	[SerializeField]
	[Tooltip("Alternative meshes for the card deck. All must be provided to display in game!")]
	public CustomDeckMeshes m_CustomDeckMeshes;

	[Tooltip("Summon in echo effect texture")]
	public Texture2D m_HiddenCardEchoTexture;

	[Tooltip("Texture for card back highlight")]
	public Texture2D m_CardBackHighlightTexture;

	[Tooltip("Drag effects prefab")]
	public GameObject m_DragEffect;

	[Tooltip("Min Velocity that triggers effect")]
	public float m_EffectMinVelocity = 2f;

	[Tooltip("Max Velocity that stops the effect")]
	public float m_EffectMaxVelocity = 40f;

	public cardBackHelpers cardBackHelper;

	public bool GetCustomDeckMeshes(out CustomDeckMeshes meshes)
	{
		if (m_CustomDeckMeshes.IsComplete)
		{
			meshes = m_CustomDeckMeshes;
			return true;
		}
		meshes = default(CustomDeckMeshes);
		return false;
	}
}
