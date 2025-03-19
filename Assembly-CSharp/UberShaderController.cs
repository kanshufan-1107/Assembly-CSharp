using System;
using System.Collections.Generic;
using Blizzard.T5.MaterialService.Extensions;
using UnityEngine;

[ExecuteAlways]
public class UberShaderController : MonoBehaviour
{
	private const int GUI_PROPERTY_LABEL_WIDTH = 130;

	[SerializeField]
	private UberShaderAnimation m_UberShaderAnimation;

	public int m_MaterialIndex = -1;

	private bool m_firstFrame;

	private float m_time;

	private float m_deltaTime;

	private Renderer m_renderer;

	private float m_lastTime;

	private float m_randomOffset;

	private string m_copyBuffer;

	private UberShaderAnimation.PropertyType m_copyBufferType;

	private string m_copyBufferLayer;

	private int m_copyBufferLayerCount;

	private DateTime? m_lastSaveTime;

	private float m_maxTime = 65535f;

	private List<Material> m_sharedAnimationMaterials = new List<Material>();

	private static bool s_autoSave = false;

	private static float s_autoSaveInterval = 30f;

	public UberShaderAnimation UberShaderAnimation
	{
		get
		{
			return m_UberShaderAnimation;
		}
		set
		{
			if (m_UberShaderAnimation != null)
			{
				UnityEngine.Object.Destroy(m_UberShaderAnimation);
			}
			m_UberShaderAnimation = value;
			UpdateShaderIDs();
		}
	}

	public DateTime? LastSaveTime => m_lastSaveTime;

	public static bool GetAutoSaveEnabled()
	{
		return s_autoSave;
	}

	public static float GetAutoSaveInterval()
	{
		return s_autoSaveInterval;
	}

	private void Awake()
	{
		if (m_UberShaderAnimation == null)
		{
			m_UberShaderAnimation = ScriptableObject.CreateInstance<UberShaderAnimation>();
		}
		m_firstFrame = true;
		m_randomOffset = UnityEngine.Random.Range(0f, 10f);
		m_time += m_randomOffset;
		m_renderer = GetComponent<Renderer>();
	}

	private void OnEnable()
	{
		LoadUberShaderAnimation();
	}

	private void Update()
	{
		UpdateAnimation();
	}

	[ContextMenu("Reload Animation File")]
	private void LoadUberShaderAnimation()
	{
		m_firstFrame = true;
		if (m_UberShaderAnimation == null)
		{
			m_UberShaderAnimation = ScriptableObject.CreateInstance<UberShaderAnimation>();
		}
		UpdateShaderIDs();
	}

	private void UpdateTime()
	{
		m_deltaTime = Time.deltaTime;
		m_time += m_deltaTime;
		if (m_time > m_maxTime)
		{
			m_time = 0.0001f;
		}
	}

	private void UpdateEditorTime()
	{
		float time = Time.realtimeSinceStartup + m_randomOffset;
		m_deltaTime = time - m_lastTime;
		m_lastTime = time;
		m_time += m_deltaTime;
		if (m_time > m_maxTime)
		{
			m_time = 0.0001f;
		}
	}

	private void UpdateAnimation()
	{
		UpdateTime();
		if (m_renderer == null)
		{
			return;
		}
		m_sharedAnimationMaterials.Clear();
		m_renderer.GetSharedMaterials(m_sharedAnimationMaterials);
		if (m_sharedAnimationMaterials.Count < 1 || (object)m_UberShaderAnimation == null || m_UberShaderAnimation.animations == null)
		{
			return;
		}
		for (int i = 0; i < m_UberShaderAnimation.animations.Count; i++)
		{
			UberShaderAnimation.UberAnimation animation = m_UberShaderAnimation.animations[i];
			int materialPropertyID = m_UberShaderAnimation.materialPropertyIDs[i];
			int matIdx = ((m_MaterialIndex >= 0 && m_MaterialIndex < m_sharedAnimationMaterials.Count) ? m_MaterialIndex : animation.materialIndex);
			Material material = ((matIdx >= 0 && matIdx < m_sharedAnimationMaterials.Count) ? m_sharedAnimationMaterials[matIdx] : null);
			if (material == null)
			{
				continue;
			}
			if (animation.propertyType == UberShaderAnimation.PropertyType.Color)
			{
				UberShaderAnimation.UberAnimationElement animElement = animation.animationElement[0];
				if (animElement == null)
				{
					continue;
				}
				UberShaderAnimation.UberAnimationColor colorAnim = animElement.colorAnimation;
				if (colorAnim != null && !colorAnim.enabled)
				{
					continue;
				}
			}
			if (!material.HasProperty(materialPropertyID))
			{
				continue;
			}
			Vector4 matVec = Vector4.zero;
			if (animation.propertyType == UberShaderAnimation.PropertyType.Vector)
			{
				matVec = material.GetVector(materialPropertyID);
			}
			else if (animation.propertyType == UberShaderAnimation.PropertyType.Float)
			{
				matVec[0] = material.GetFloat(materialPropertyID);
			}
			Vector4 animVec = matVec;
			for (int a = 0; a < animation.animationElement.Count; a++)
			{
				UberShaderAnimation.UberAnimationElement animElement2 = animation.animationElement[a];
				UberShaderAnimation.UberAnimationCurve curve = animElement2.animationCurve;
				UberShaderAnimation.UberAnimationRandom randomAnim = animElement2.randomAnimation;
				int element = animElement2.element;
				float animValue = 0f;
				if (!animElement2.incrementingValue)
				{
					switch (element)
					{
					case 0:
						animValue = matVec.x;
						break;
					case 1:
						animValue = matVec.y;
						break;
					case 2:
						animValue = matVec.z;
						break;
					case 3:
						animValue = matVec.w;
						break;
					}
				}
				if (curve.animationCurve != null && curve.enabled)
				{
					animValue = (curve.animationCurve.Evaluate(m_time * curve.speed) + curve.offset) * curve.scale;
				}
				if (randomAnim != null && randomAnim.enabled)
				{
					if (curve.animationCurve == null || !curve.enabled)
					{
						animValue = 0f;
					}
					float randomIntensity = 1f;
					if (randomAnim.intensityCurve != null)
					{
						randomIntensity = randomAnim.intensityCurve.Evaluate(m_time * randomAnim.intensitySpeed);
					}
					animValue += Mathf.Lerp(randomAnim.minValue, randomAnim.maxValue, (UberMath.SimplexNoise(m_time * randomAnim.speed + randomAnim.seed, 0.5f) + 1f) * 0.5f * randomIntensity) * randomAnim.scale;
				}
				if (animElement2.incrementingValue)
				{
					if (m_firstFrame)
					{
						animElement2.incrementingLastValue = 0f;
					}
					if (animElement2.incrementingLastValue > m_maxTime)
					{
						animElement2.incrementingLastValue = 0.0001f;
					}
					float speed = animValue + animElement2.incrementingSpeed;
					float deltaTime = m_deltaTime * speed;
					animValue = (animElement2.incrementingLastValue += deltaTime);
				}
				switch (element)
				{
				case 0:
					animVec.x = animValue;
					break;
				case 1:
					animVec.y = animValue;
					break;
				case 2:
					animVec.z = animValue;
					break;
				case 3:
					animVec.w = animValue;
					break;
				}
			}
			if (animation.propertyType == UberShaderAnimation.PropertyType.Color)
			{
				Color color = animation.animationElement[0].colorAnimation.gradient.Evaluate(animVec.x);
				material.SetColor(materialPropertyID, color);
			}
			else
			{
				material.SetVector(materialPropertyID, animVec);
			}
		}
		m_firstFrame = false;
	}

	private void UpdateShaderIDs()
	{
		if (m_renderer == null)
		{
			return;
		}
		List<Material> materials = m_renderer.GetSharedMaterials();
		if (materials != null && materials.Count >= 1 && (object)m_UberShaderAnimation != null && m_UberShaderAnimation.animations != null)
		{
			m_UberShaderAnimation.materialPropertyIDs = new List<int>(m_UberShaderAnimation.animations.Count);
			for (int i = 0; i < m_UberShaderAnimation.animations.Count; i++)
			{
				int propertyID = Shader.PropertyToID(m_UberShaderAnimation.animations[i].materialPropertyName);
				UberShaderAnimation.materialPropertyIDs.Add(propertyID);
			}
		}
	}
}
