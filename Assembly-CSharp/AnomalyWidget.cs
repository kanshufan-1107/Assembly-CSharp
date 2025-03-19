using Blizzard.T5.MaterialService.Extensions;
using UnityEngine;

public class AnomalyWidget : MonoBehaviour
{
	public MeshRenderer m_portraitMesh;

	public MeshRenderer m_medallionMesh;

	public GameObject m_model;

	public SimplePopup m_popup;

	public int portraitIndex;

	public float m_tilingX = 0.7f;

	public float m_tilingY = 0.65f;

	public float m_offsetX = 0.14f;

	public float m_offsetY = 0.25f;

	public Color m_fallbackMatColor = Color.white;

	private bool setup;

	private Actor m_actor;

	private Texture m_portraitTexture;

	protected virtual void LateUpdate()
	{
		UpdatePortrait();
		if (!(m_actor == null) && !(m_actor.PortraitTexture == m_portraitTexture))
		{
			setup = false;
		}
	}

	protected void UpdatePortrait()
	{
		if (!setup)
		{
			Actor actor = base.gameObject.GetComponent<Actor>();
			if (actor == null || !actor.IsShown() || actor.PortraitTexture == null)
			{
				m_model.SetActive(value: false);
				return;
			}
			m_actor = actor;
			m_portraitTexture = actor.PortraitTexture;
			m_portraitMesh.GetMaterials()[portraitIndex].mainTexture = actor.PortraitTexture;
			m_model.SetActive(value: true);
			SetupDefaultPortraitMaterialScaleOffset();
			setup = true;
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
