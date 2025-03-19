using System.Collections.Generic;
using Blizzard.T5.MaterialService.Extensions;
using UnityEngine;

public class QuickBlendShape : MonoBehaviour
{
	public enum BLEND_SHAPE_ANIMATION_TYPE
	{
		Curve,
		Float
	}

	public bool m_DisableOnMobile;

	public BLEND_SHAPE_ANIMATION_TYPE m_AnimationType;

	public float m_BlendValue;

	public AnimationCurve m_BlendCurve;

	public bool m_Loop = true;

	public bool m_PlayOnAwake;

	public Mesh[] m_Meshes;

	private List<Mesh> m_BlendMeshes;

	private bool m_Play;

	private MeshFilter m_MeshFilter;

	private float m_animTime;

	private List<Material> m_BlendMaterials;

	private Mesh m_OrgMesh;

	private void Awake()
	{
		m_MeshFilter = GetComponent<MeshFilter>();
		m_OrgMesh = m_MeshFilter.sharedMesh;
		if (m_DisableOnMobile && PlatformSettings.IsMobile())
		{
			if (m_MeshFilter.sharedMesh == null && m_Meshes.Length != 0 && m_Meshes[0] != null)
			{
				m_MeshFilter.sharedMesh = m_Meshes[0];
			}
		}
		else
		{
			CreateBlendMeshes();
		}
	}

	private void Update()
	{
		if ((!m_DisableOnMobile || !PlatformSettings.IsMobile()) && (m_Play || m_AnimationType == BLEND_SHAPE_ANIMATION_TYPE.Float))
		{
			BlendShapeAnimate();
		}
	}

	private void OnEnable()
	{
		if ((!m_DisableOnMobile || !PlatformSettings.IsMobile()) && m_PlayOnAwake)
		{
			PlayAnimation();
		}
	}

	private void OnDisable()
	{
		if (m_DisableOnMobile && PlatformSettings.IsMobile())
		{
			m_MeshFilter.sharedMesh = m_OrgMesh;
			return;
		}
		m_animTime = 0f;
		if (m_BlendMaterials != null)
		{
			foreach (Material blendMaterial in m_BlendMaterials)
			{
				blendMaterial.SetFloat("_Blend", 0f);
			}
		}
		if (m_MeshFilter != null && m_OrgMesh != null)
		{
			m_MeshFilter.sharedMesh = m_OrgMesh;
		}
	}

	public void PlayAnimation()
	{
		if ((!m_DisableOnMobile || !PlatformSettings.IsMobile()) && !(m_MeshFilter == null) && m_Meshes != null && m_BlendMeshes != null)
		{
			m_animTime = 0f;
			m_Play = true;
		}
	}

	public void StopAnimation()
	{
		if (!m_DisableOnMobile || !PlatformSettings.IsMobile())
		{
			m_Play = false;
		}
	}

	private void BlendShapeAnimate()
	{
		if (m_BlendMaterials == null)
		{
			m_BlendMaterials = GetComponent<Renderer>().GetMaterials();
		}
		MeshFilter meshFilter = GetComponent<MeshFilter>();
		if (m_MeshFilter == null)
		{
			m_MeshFilter = meshFilter;
		}
		float animLength = m_BlendCurve.keys[m_BlendCurve.length - 1].time;
		m_animTime += Time.deltaTime;
		float blendValue = m_BlendValue;
		if (blendValue < 0f)
		{
			return;
		}
		if (m_AnimationType == BLEND_SHAPE_ANIMATION_TYPE.Curve)
		{
			blendValue = m_BlendCurve.Evaluate(m_animTime);
		}
		int blendMeshIdx = Mathf.FloorToInt(blendValue);
		if (blendMeshIdx > m_BlendMeshes.Count - 1)
		{
			blendMeshIdx -= m_BlendMeshes.Count - 1;
		}
		m_MeshFilter.mesh = m_BlendMeshes[blendMeshIdx];
		foreach (Material blendMaterial in m_BlendMaterials)
		{
			blendMaterial.SetFloat("_Blend", blendValue - (float)Mathf.FloorToInt(blendValue));
		}
		if (m_animTime > animLength)
		{
			if (m_Loop)
			{
				m_animTime = 0f;
			}
			else
			{
				m_Play = false;
			}
		}
	}

	private void CreateBlendMeshes()
	{
		m_BlendMeshes = new List<Mesh>();
		for (int mIdx = 0; mIdx < m_Meshes.Length; mIdx++)
		{
			if (GetComponent<MeshFilter>() == null)
			{
				base.gameObject.AddComponent<MeshFilter>();
			}
			Mesh newMesh = Object.Instantiate(m_Meshes[mIdx]);
			int vertexCount = m_Meshes[mIdx].vertices.Length;
			int nextIdx = mIdx + 1;
			if (nextIdx > m_Meshes.Length - 1)
			{
				nextIdx = 0;
			}
			Mesh obj = m_Meshes[nextIdx];
			Vector4[] nextVerts = new Vector4[vertexCount];
			Vector3[] nextMeshVertices = obj.vertices;
			for (int nxIdx = 0; nxIdx < vertexCount; nxIdx++)
			{
				if (nxIdx <= nextMeshVertices.Length - 1)
				{
					Vector3 pv = nextMeshVertices[nxIdx];
					nextVerts[nxIdx] = new Vector4(pv.x, pv.y, pv.z, 1f);
				}
			}
			newMesh.vertices = m_Meshes[mIdx].vertices;
			newMesh.tangents = nextVerts;
			m_BlendMeshes.Add(newMesh);
		}
	}
}
