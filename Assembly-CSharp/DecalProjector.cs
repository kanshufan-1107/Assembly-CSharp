using Blizzard.T5.MaterialService.Extensions;
using UnityEngine;

public class DecalProjector : MonoBehaviour
{
	private float m_aspectRatio = 1f;

	private float m_orthographicSize = 1f;

	private float m_nearClipPlane;

	private float m_farClipPlane = 1f;

	public GameObject m_RenderCube;

	public MeshRenderer m_RenderCubeMeshRenderer;

	public float AspectRatio
	{
		get
		{
			return m_aspectRatio;
		}
		set
		{
			m_aspectRatio = value;
			UpdateCubePosition();
		}
	}

	public float OrthographicSize
	{
		get
		{
			return m_orthographicSize;
		}
		set
		{
			m_orthographicSize = value;
			UpdateCubePosition();
		}
	}

	public float NearClipPlane
	{
		get
		{
			return m_nearClipPlane;
		}
		set
		{
			m_nearClipPlane = value;
			UpdateCubePosition();
		}
	}

	public float FarClipPlane
	{
		get
		{
			return m_farClipPlane;
		}
		set
		{
			m_farClipPlane = value;
			UpdateCubePosition();
		}
	}

	public Material Material
	{
		get
		{
			return m_RenderCubeMeshRenderer.material;
		}
		set
		{
			m_RenderCubeMeshRenderer.SetMaterial(value);
		}
	}

	public Renderer Renderer => m_RenderCubeMeshRenderer;

	private void UpdateCubePosition()
	{
		Vector3 lossyScale = base.transform.lossyScale;
		if (lossyScale.x != 0f && lossyScale.y != 0f && lossyScale.z != 0f)
		{
			Transform obj = m_RenderCube.transform;
			obj.localScale = new Vector3(m_orthographicSize * m_aspectRatio * 2f / lossyScale.x, m_orthographicSize * 2f / lossyScale.y, (m_farClipPlane - m_nearClipPlane) / lossyScale.z);
			obj.localPosition = new Vector3(0f, 0f, ((m_farClipPlane - m_nearClipPlane) * 0.5f + m_nearClipPlane) / lossyScale.z);
		}
	}

	private void OnEnable()
	{
		DecalRendererFeature.s_decals.Add(this);
	}

	private void OnDisable()
	{
		DecalRendererFeature.s_decals.Remove(this);
	}
}
