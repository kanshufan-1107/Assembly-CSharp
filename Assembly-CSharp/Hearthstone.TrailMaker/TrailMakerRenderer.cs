using System;
using System.Collections.Generic;
using Blizzard.T5.MaterialService.Extensions;
using UnityEngine;

namespace Hearthstone.TrailMaker;

[ExecuteAlways]
[DisallowMultipleComponent]
[RequireComponent(typeof(TrailGenerator))]
[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(MeshFilter))]
[AddComponentMenu("Hearthstone/HearthTrail")]
public class TrailMakerRenderer : MonoBehaviour
{
	private const string MAIN_COLOR_SHADER_PROPERTY = "_Color";

	[SerializeField]
	private ParticleSystemTrailTextureMode m_textureMode;

	[SerializeField]
	private AnimationCurve m_sizeOverTime;

	[SerializeField]
	private AnimationCurve m_sizeOverDistance;

	[SerializeField]
	private Gradient m_colorOverTime;

	[SerializeField]
	private Gradient m_colorOverDistance;

	private MeshFilter m_meshFilter;

	private MeshRenderer m_meshRenderer;

	private Mesh m_mesh;

	private readonly List<Vector3> m_vertices = new List<Vector3>();

	private readonly List<int> m_triangles = new List<int>();

	private readonly List<Vector2> m_uvs = new List<Vector2>();

	private readonly List<Color> m_colors = new List<Color>();

	private TrailPosition m_firstTrailPosition;

	public TrailGenerator TrailGenerator { get; private set; }

	private void Reset()
	{
		m_textureMode = ParticleSystemTrailTextureMode.Stretch;
		m_sizeOverTime = new AnimationCurve(new Keyframe(0f, 1f), new Keyframe(1f, 0f));
		m_sizeOverDistance = new AnimationCurve(new Keyframe(0f, 1f), new Keyframe(1f, 1f));
		m_colorOverTime = new Gradient
		{
			colorKeys = new GradientColorKey[2]
			{
				new GradientColorKey(Color.white, 0f),
				new GradientColorKey(Color.white, 1f)
			},
			alphaKeys = new GradientAlphaKey[2]
			{
				new GradientAlphaKey(1f, 0f),
				new GradientAlphaKey(0f, 1f)
			}
		};
		m_colorOverDistance = new Gradient
		{
			colorKeys = new GradientColorKey[2]
			{
				new GradientColorKey(Color.white, 0f),
				new GradientColorKey(Color.white, 1f)
			},
			alphaKeys = new GradientAlphaKey[2]
			{
				new GradientAlphaKey(1f, 0f),
				new GradientAlphaKey(1f, 1f)
			}
		};
	}

	private void OnEnable()
	{
		TrailGenerator component = GetComponent<TrailGenerator>();
		component.OnUpdate = (TrailGenerator.UpdateHandler)Delegate.Combine(component.OnUpdate, new TrailGenerator.UpdateHandler(OnUpdate));
	}

	private void OnDisable()
	{
		TrailGenerator trailGenerator = TrailGenerator;
		trailGenerator.OnUpdate = (TrailGenerator.UpdateHandler)Delegate.Remove(trailGenerator.OnUpdate, new TrailGenerator.UpdateHandler(OnUpdate));
	}

	private void Awake()
	{
		m_meshFilter = GetComponent<MeshFilter>();
		if (m_meshFilter.sharedMesh == null)
		{
			m_meshFilter.sharedMesh = new Mesh();
			m_meshFilter.sharedMesh.name = base.name + " Mesh";
		}
		m_mesh = m_meshFilter.sharedMesh;
		m_meshRenderer = GetComponent<MeshRenderer>();
		TrailGenerator = GetComponent<TrailGenerator>();
		if (Application.isPlaying && m_meshRenderer.material != null)
		{
			m_meshRenderer.SetMaterial(UnityEngine.Object.Instantiate(m_meshRenderer.GetMaterial()));
		}
	}

	private void OnUpdate(MutableArray<TrailPosition> trail, bool isEditor)
	{
		m_mesh.Clear();
		if (trail == null || trail.Count < 2)
		{
			m_meshRenderer.enabled = false;
			return;
		}
		m_vertices.Clear();
		m_triangles.Clear();
		m_uvs.Clear();
		m_colors.Clear();
		int count = trail.Count;
		float fullLength = 0f;
		for (int i = 1; i < count; i++)
		{
			fullLength += Vector3.Distance(trail.Get(i - 1).origin, trail.Get(i).origin);
		}
		m_firstTrailPosition = trail.GetFirst();
		Vector3 previousOrigin = m_firstTrailPosition.origin;
		float length = 0f;
		SetVerticesUVsAndColors(m_firstTrailPosition, 0, count, m_vertices, m_uvs, m_colors, fullLength, ref previousOrigin, ref length);
		for (int j = 1; j < count; j++)
		{
			SetVerticesUVsAndColors(trail.Get(j), j, count, m_vertices, m_uvs, m_colors, fullLength, ref previousOrigin, ref length);
			int a = j * 2 - 2;
			int b = j * 2 - 1;
			int c = j * 2;
			int d = j * 2 + 1;
			m_triangles.Add(a);
			m_triangles.Add(c);
			m_triangles.Add(d);
			m_triangles.Add(d);
			m_triangles.Add(b);
			m_triangles.Add(a);
		}
		m_mesh.SetVertices(m_vertices);
		m_mesh.SetTriangles(m_triangles, 0);
		m_mesh.SetUVs(0, m_uvs);
		m_mesh.SetColors(m_colors);
		m_mesh.RecalculateNormals();
		m_mesh.RecalculateBounds();
		m_meshRenderer.enabled = true;
	}

	private void SetVerticesUVsAndColors(TrailPosition trailPosition, int index, int count, List<Vector3> vertices, List<Vector2> uvs, List<Color> colors, float _, ref Vector3 previousOrigin, ref float length)
	{
		var (min, max) = ResizeTrailPosition(trailPosition);
		vertices.Add(min);
		vertices.Add(max);
		length += Vector3.Distance(trailPosition.origin, previousOrigin);
		previousOrigin = trailPosition.origin;
		float uv = m_textureMode switch
		{
			ParticleSystemTrailTextureMode.Tile => length, 
			ParticleSystemTrailTextureMode.RepeatPerSegment => index, 
			ParticleSystemTrailTextureMode.DistributePerSegment => (float)index / (float)(count - 1), 
			_ => trailPosition.distance, 
		};
		uvs.Add(new Vector2(0f, uv));
		uvs.Add(new Vector2(1f, uv));
		float time = Mathf.Clamp01((TrailUtility.GetTime() - trailPosition.time) / TrailGenerator.Lifespan);
		colors.Add(m_colorOverTime.Evaluate(time) * m_colorOverDistance.Evaluate(1f - trailPosition.distance));
		colors.Add(m_colorOverTime.Evaluate(time) * m_colorOverDistance.Evaluate(1f - trailPosition.distance));
	}

	private (Vector3 min, Vector3 max) ResizeTrailPosition(TrailPosition trailPosition)
	{
		float sizeLerp = m_sizeOverTime.Evaluate(Mathf.Clamp01((TrailUtility.GetTime() - trailPosition.time) / TrailGenerator.Lifespan));
		sizeLerp *= m_sizeOverDistance.Evaluate(1f - trailPosition.distance);
		Vector3 a = base.transform.InverseTransformPoint(trailPosition.origin);
		Vector3 min = base.transform.InverseTransformPoint(trailPosition.min);
		Vector3 max = base.transform.InverseTransformPoint(trailPosition.max);
		min = Vector3.LerpUnclamped(a, min, sizeLerp);
		max = Vector3.LerpUnclamped(a, max, sizeLerp);
		return (min: min, max: max);
	}
}
