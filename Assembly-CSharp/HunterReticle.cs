using Blizzard.T5.MaterialService.Extensions;
using UnityEngine;

public class HunterReticle : BlitToTexture
{
	private static readonly Vector3[] s_planeVertices = new Vector3[4]
	{
		new Vector3(-1f, 0f, -1f),
		new Vector3(1f, 0f, -1f),
		new Vector3(-1f, 0f, 1f),
		new Vector3(1f, 0f, 1f)
	};

	private static readonly Vector2[] s_planeUvs = new Vector2[4]
	{
		new Vector2(0f, 0f),
		new Vector2(1f, 0f),
		new Vector2(0f, 1f),
		new Vector2(1f, 1f)
	};

	private static readonly Vector3[] s_planeNormals = new Vector3[4]
	{
		Vector3.up,
		Vector3.up,
		Vector3.up,
		Vector3.up
	};

	private static readonly int[] s_planeTriangles = new int[6] { 3, 1, 2, 2, 1, 0 };

	public float ReticleSize = 1f;

	public Material Material;

	protected override void Awake()
	{
		base.Awake();
		GameObject obj = new GameObject();
		obj.name = base.name;
		obj.transform.parent = base.transform;
		obj.transform.localPosition = Vector3.zero;
		obj.transform.localRotation = Quaternion.identity;
		obj.transform.localScale = Vector3.one * ReticleSize;
		MeshFilter meshFilter = obj.AddComponent<MeshFilter>();
		MeshRenderer meshRenderer = obj.AddComponent<MeshRenderer>();
		meshRenderer.SetMaterial(Material);
		meshRenderer.enabled = false;
		base.DrawAfterBlit = meshRenderer;
		Mesh meshPlane = new Mesh();
		meshPlane.vertices = s_planeVertices;
		meshPlane.uv = s_planeUvs;
		meshPlane.normals = s_planeNormals;
		meshPlane.triangles = s_planeTriangles;
		meshPlane.RecalculateBounds();
		meshFilter.mesh = meshPlane;
		if (Material != null)
		{
			Material.mainTexture = base.TargetTexture;
		}
	}

	protected override void Update()
	{
		if (m_mainCamera == null)
		{
			m_mainCamera = CameraUtils.GetMainCamera();
		}
		Vector2 viewportPosition = m_mainCamera.WorldToScreenPoint(base.transform.position);
		Offset = viewportPosition;
		base.Update();
	}
}
