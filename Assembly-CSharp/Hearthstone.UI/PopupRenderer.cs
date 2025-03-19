using UnityEngine;

namespace Hearthstone.UI;

[ExecuteAlways]
[AddComponentMenu("")]
public class PopupRenderer : MonoBehaviour, ILayerOverridable, IVisibleWidgetComponent
{
	private GameLayer? m_layerOverride;

	private uint m_renderingLayerMask = 1u;

	private bool m_isVisible = true;

	private int m_defaultLayer;

	public PopupRoot PopupRoot { get; private set; }

	public bool HandlesChildLayers => false;

	public bool IsDesiredHidden => m_layerOverride == GameLayer.InvisibleRender;

	public bool IsDesiredHiddenInHierarchy => IsDesiredHidden;

	public bool HandlesChildVisibility => false;

	private void Awake()
	{
		base.hideFlags = HideFlags.DontSave | HideFlags.NotEditable;
		m_defaultLayer = base.gameObject.layer;
	}

	private void OnDestroy()
	{
		if (!(this == null) && !(base.gameObject == null) && !(PopupRoot == null))
		{
			PopupRoot.RemoveDisabledListener(ResetRenderer);
			PopupRoot = null;
		}
	}

	public void Initialize(PopupRoot popupRoot, bool isVisible, uint renderingLayerMask)
	{
		if (PopupRoot == popupRoot && m_isVisible == isVisible && m_renderingLayerMask == renderingLayerMask)
		{
			return;
		}
		if (PopupRoot != popupRoot)
		{
			if (PopupRoot != null)
			{
				PopupRoot.RemoveDisabledListener(ResetRenderer);
			}
			PopupRoot = popupRoot;
			if (PopupRoot != null)
			{
				PopupRoot.RegisterDisabledListener(ResetRenderer);
			}
		}
		m_isVisible = isVisible;
		m_renderingLayerMask = renderingLayerMask;
		UpdateRenderer();
	}

	private void UpdateRenderer()
	{
		Renderer renderer = GetComponent<Renderer>();
		if (!m_isVisible)
		{
			renderer.renderingLayerMask = 0u;
			return;
		}
		base.gameObject.layer = GetLayer();
		renderer.renderingLayerMask = m_renderingLayerMask;
	}

	private int GetLayer()
	{
		if (m_isVisible)
		{
			if (m_layerOverride.HasValue)
			{
				return (int)m_layerOverride.Value;
			}
			if (m_renderingLayerMask != 0)
			{
				return m_defaultLayer;
			}
		}
		return 28;
	}

	public void ResetRenderer()
	{
		if (!(this == null) && !(base.gameObject == null))
		{
			ClearLayerOverride();
			ClearRenderingLayerMaskOverride();
			base.gameObject.layer = m_defaultLayer;
			GetComponent<Renderer>().renderingLayerMask = 1u;
			if (Application.IsPlaying(this))
			{
				Object.Destroy(this);
			}
			else
			{
				Object.DestroyImmediate(this);
			}
		}
	}

	public void SetLayerOverride(GameLayer layer)
	{
		if (layer != GameLayer.InvisibleRender)
		{
			m_layerOverride = layer;
			UpdateRenderer();
		}
	}

	public void ClearLayerOverride()
	{
		m_layerOverride = null;
		UpdateRenderer();
	}

	public void SetRenderingLayerMaskOverride(uint renderingLayerMask)
	{
		m_renderingLayerMask = renderingLayerMask;
		UpdateRenderer();
	}

	public void ClearRenderingLayerMaskOverride()
	{
		m_renderingLayerMask = 1u;
		UpdateRenderer();
	}

	public void SetVisibility(bool isVisible, bool isInternal)
	{
		m_isVisible = isVisible;
		UpdateRenderer();
	}
}
