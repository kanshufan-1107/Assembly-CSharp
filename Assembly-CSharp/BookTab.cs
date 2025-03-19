using System.Collections;
using System.Collections.Generic;
using Blizzard.T5.MaterialService.Extensions;
using UnityEngine;

public class BookTab : PegUIElement
{
	public GameObject m_glowMesh;

	public GameObject m_newItemCount;

	public UberText m_newItemCountText;

	public CollectionUtils.ViewMode m_tabViewMode;

	public Vector3 m_DeselectedLocalScale = new Vector3(0.44f, 0.44f, 0.44f);

	public Vector3 m_SelectedLocalScale = new Vector3(0.66f, 0.66f, 0.66f);

	public float m_SelectedLocalYPos = 0.1259841f;

	public float m_DeselectedLocalYPos;

	public string m_IconTextureName;

	[Tooltip("Local position offset. Applied after absolute local position.")]
	public Vector3 m_SelectedLocalOffset = Vector3.zero;

	protected int m_numNewItems;

	protected bool m_selected;

	protected Vector3 m_targetLocalPos;

	protected bool m_shouldBeVisible = true;

	protected bool m_isVisible = true;

	protected bool m_showLargeTab;

	protected MaterialPropertyBlock m_propertyBlock;

	public static readonly float SELECT_TAB_ANIM_TIME = 0.2f;

	public void Init()
	{
		SetTabIconsTextureOffset(base.gameObject.GetComponent<Renderer>());
		if (m_glowMesh != null)
		{
			SetTabIconsTextureOffset(m_glowMesh.GetComponent<Renderer>());
		}
		SetGlowActive(active: false);
		UpdateNewItemCount(0);
	}

	public void SetGlowActive(bool active)
	{
		if (m_selected)
		{
			active = true;
		}
		if (m_glowMesh != null)
		{
			m_glowMesh.SetActive(active);
		}
	}

	public void SetSelected(bool selected)
	{
		if (m_selected != selected)
		{
			m_selected = selected;
			SetGlowActive(m_selected);
		}
	}

	public void UpdateNewItemCount(int numNewItems)
	{
		m_numNewItems = numNewItems;
		UpdateNewItemCountVisuals();
	}

	public void SetTargetLocalPosition(Vector3 targetLocalPos)
	{
		m_targetLocalPos = targetLocalPos;
	}

	public void SetIsVisible(bool isVisible)
	{
		m_isVisible = isVisible;
		SetEnabled(m_isVisible);
	}

	public bool IsVisible()
	{
		return m_isVisible;
	}

	public void SetTargetVisibility(bool visible)
	{
		m_shouldBeVisible = visible;
	}

	public bool ShouldBeVisible()
	{
		return m_shouldBeVisible;
	}

	public bool WillSlide()
	{
		if (Mathf.Abs(m_targetLocalPos.x - base.transform.localPosition.x) > 0.05f)
		{
			return true;
		}
		return false;
	}

	public void AnimateToTargetPosition(float animationTime, iTween.EaseType easeType)
	{
		Hashtable moveArgs = iTweenManager.Get().GetTweenHashTable();
		moveArgs.Add("position", m_targetLocalPos);
		moveArgs.Add("islocal", true);
		moveArgs.Add("time", animationTime);
		moveArgs.Add("easetype", easeType);
		moveArgs.Add("name", "position");
		moveArgs.Add("oncomplete", "OnMovedToTargetPos");
		moveArgs.Add("oncompletetarget", base.gameObject);
		iTween.StopByName(base.gameObject, "position");
		iTween.MoveTo(base.gameObject, moveArgs);
	}

	public virtual void SetLargeTab(bool large)
	{
		if (large == m_showLargeTab)
		{
			return;
		}
		if (large)
		{
			Vector3 pos = base.transform.localPosition;
			pos.y = m_SelectedLocalYPos;
			pos += m_SelectedLocalOffset;
			base.transform.localPosition = pos;
			if (iTweenManager.Get() != null)
			{
				Hashtable scaleArgs = iTweenManager.Get().GetTweenHashTable();
				if (scaleArgs != null)
				{
					scaleArgs.Add("scale", m_SelectedLocalScale);
					scaleArgs.Add("time", SELECT_TAB_ANIM_TIME);
					scaleArgs.Add("name", "scale");
					iTween.ScaleTo(base.gameObject, scaleArgs);
				}
			}
			SoundManager.Get()?.LoadAndPlay("class_tab_click.prefab:d9cb832f0de5c1947a97685e134ba0da", base.gameObject);
		}
		else
		{
			Vector3 pos2 = base.transform.localPosition;
			pos2.y = m_DeselectedLocalYPos;
			pos2.x -= m_SelectedLocalOffset.x;
			pos2.z -= m_SelectedLocalOffset.z;
			base.transform.localPosition = pos2;
			iTween.StopByName(base.gameObject, "scale");
			base.transform.localScale = m_DeselectedLocalScale;
		}
		m_showLargeTab = large;
	}

	protected virtual Vector2 GetTextureOffset()
	{
		return Vector2.zero;
	}

	protected void SetTabIconsTextureOffset(Renderer renderer)
	{
		if (renderer == null || string.IsNullOrEmpty(m_IconTextureName))
		{
			return;
		}
		if (m_propertyBlock == null)
		{
			m_propertyBlock = new MaterialPropertyBlock();
		}
		Vector2 textureOffset = GetTextureOffset();
		Vector4 textureOffsetTiling = new Vector4(1f, 1f, textureOffset.x, textureOffset.y);
		List<Material> sharedMaterials = renderer.GetSharedMaterials();
		for (int i = 0; i < sharedMaterials.Count; i++)
		{
			Material material = sharedMaterials[i];
			if (!(material.mainTexture == null) && material.mainTexture.name.Contains(m_IconTextureName))
			{
				renderer.GetPropertyBlock(m_propertyBlock, i);
				m_propertyBlock.SetVector("_MainTex_ST", textureOffsetTiling);
				renderer.SetPropertyBlock(m_propertyBlock, i);
			}
		}
	}

	private void UpdateNewItemCountVisuals()
	{
		if (m_newItemCountText != null)
		{
			m_newItemCountText.Text = GameStrings.Format("GLUE_COLLECTION_NEW_CARD_CALLOUT", m_numNewItems);
		}
		if (m_newItemCount != null)
		{
			m_newItemCount.SetActive(m_numNewItems > 0);
		}
	}

	private void OnMovedToTargetPos()
	{
		if (!m_showLargeTab)
		{
			Vector3 localPos = base.transform.localPosition;
			localPos.y = m_DeselectedLocalYPos;
			base.transform.localPosition = localPos;
		}
	}
}
