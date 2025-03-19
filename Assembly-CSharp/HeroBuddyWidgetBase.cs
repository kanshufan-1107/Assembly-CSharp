using Blizzard.T5.MaterialService.Extensions;
using UnityEngine;

public class HeroBuddyWidgetBase : MonoBehaviour
{
	public MeshRenderer m_portraitMesh;

	public int portraitIndex;

	public float m_tilingX = 0.64f;

	public float m_tilingY = 0.64f;

	public float m_offsetX = 0.18f;

	public float m_offsetY = 0.21f;

	public string m_desiredShaderName = "Custom/Card/Unlit_Portrait";

	public Color m_fallbackMatColor = Color.white;

	private int m_heroBuddyIDOverride = -1;

	private bool m_warningSent;

	protected virtual void LateUpdate()
	{
		UpdatePortrait();
	}

	public void SetHeroBuddyIDOverride(int value)
	{
		m_heroBuddyIDOverride = value;
		UpdatePortrait();
	}

	protected void UpdatePortrait()
	{
		Actor actor = base.gameObject.GetComponent<Actor>();
		if (actor == null || !actor.IsShown())
		{
			return;
		}
		int buddyCardID = m_heroBuddyIDOverride;
		if (buddyCardID == -1)
		{
			if (GameState.Get() == null)
			{
				Debug.LogWarning("game state is null for a buddy widget without override ID");
				return;
			}
			Player.Side playerSide = Player.Side.FRIENDLY;
			if (actor.GetEntity() != null && actor.GetEntity().IsControlledByOpposingSidePlayer())
			{
				playerSide = Player.Side.OPPOSING;
			}
			Entity hero = ((playerSide == Player.Side.FRIENDLY) ? GameState.Get().GetFriendlySidePlayer().GetHero() : GameState.Get().GetOpposingSidePlayer().GetHero());
			if (hero == null)
			{
				return;
			}
			buddyCardID = hero.GetHeroBuddyCardId();
		}
		using DefLoader.DisposableCardDef cardDef = DefLoader.Get().GetCardDef(buddyCardID);
		if (cardDef == null)
		{
			return;
		}
		Material heroBuddyMat = cardDef.CardDef.GetBattlegroundHeroBuddyMaterial();
		if (heroBuddyMat == null)
		{
			if (!m_warningSent)
			{
				Debug.LogWarning("HeroBuddyWidgetBase.UpdatePortrait() - Missing hero buddy Mat");
				m_warningSent = true;
			}
			m_portraitMesh.GetMaterials()[portraitIndex].mainTexture = cardDef.CardDef.GetPortraitTexture(actor.GetPremium());
			SetupDefaultPortraitMaterialScaleOffset();
		}
		else
		{
			m_portraitMesh.GetMaterials()[portraitIndex].mainTexture = heroBuddyMat.mainTexture;
			if (heroBuddyMat.shader.name != "Custom/Card/Unlit_Portrait")
			{
				SetupPortraitScaleOffsetFromMat(heroBuddyMat);
				m_portraitMesh.GetMaterials()[portraitIndex].SetTexture("_SecondTex", null);
				SetupDefaultPortraitMaterialColor();
			}
			else
			{
				m_portraitMesh.GetMaterials()[portraitIndex].CopyPropertiesFromMaterial(heroBuddyMat);
			}
		}
	}

	protected void SetupPortraitScaleOffsetFromMat(Material mat)
	{
		m_portraitMesh.GetMaterials()[portraitIndex].SetTextureOffset("_MainTex", mat.GetTextureOffset("_MainTex"));
		m_portraitMesh.GetMaterials()[portraitIndex].SetTextureScale("_MainTex", mat.GetTextureScale("_MainTex"));
	}

	protected void SetupDefaultPortraitMaterialScaleOffset()
	{
		m_portraitMesh.GetMaterials()[portraitIndex].SetTextureOffset("_MainTex", new Vector2(m_offsetX, m_offsetY));
		m_portraitMesh.GetMaterials()[portraitIndex].SetTextureScale("_MainTex", new Vector2(m_tilingX, m_tilingY));
	}

	protected void SetupDefaultPortraitMaterialColor()
	{
		if (m_portraitMesh.GetMaterials()[portraitIndex].GetColor("_Color") != m_fallbackMatColor)
		{
			m_portraitMesh.GetMaterials()[portraitIndex].SetColor("_Color", m_fallbackMatColor);
		}
	}
}
