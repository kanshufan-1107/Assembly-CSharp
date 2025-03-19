using System;
using Blizzard.T5.Core;
using UnityEngine;

[RequireComponent(typeof(Renderer))]
[ExecuteAlways]
public class VertexAnimation : MonoBehaviour
{
	[Serializable]
	public class AnimationClipInfo
	{
		public string name;

		public Vector2Int frameRange;
	}

	private const string ANIMATION_TIME = "_AnimTime";

	private const string VERTEX_COUNT_KEY = "_VertCount";

	private const string FRAME_COUNT_KEY = "_FrameCount";

	private const string ANIMATION_TEXTURE_KEY = "_MorphTex";

	private const string TIME_OFFSET = "_TimeOffset";

	[Min(0.0001f)]
	[Tooltip("The animation's default speed. Even if the current speed is modified via Timeline or PlayMaker, this variable will not change.")]
	public float AnimationSpeed;

	public int VertexCount;

	public int RecordedFrameCount;

	public int OriginalFrameCount;

	public int OriginalFPS;

	public Texture2D AnimationTexture;

	public AnimationClipInfo[] AnimationData;

	private float m_currentAnimationSpeed;

	private MaterialPropertyBlock m_properties;

	private Renderer m_renderer;

	private bool m_animationIsActive;

	private string m_animationName = string.Empty;

	public float CurrentAnimationCompletionPercent
	{
		get
		{
			if (!m_animationIsActive)
			{
				return 0f;
			}
			if (!m_renderer)
			{
				m_renderer = GetComponent<Renderer>();
			}
			if (m_properties == null)
			{
				m_properties = new MaterialPropertyBlock();
			}
			m_renderer.GetPropertyBlock(m_properties);
			if (!Mathf.Approximately(m_properties.GetFloat("_VertCount"), VertexCount))
			{
				UpdateProperties();
			}
			return (0f - (m_properties.GetFloat("_TimeOffset") - Time.timeSinceLevelLoad + CalculateStartFrameTime(m_animationName))) / GetAnimationLength(m_animationName);
		}
	}

	public float CurrentAnimationSpeed => m_currentAnimationSpeed;

	private void Awake()
	{
		m_properties = new MaterialPropertyBlock();
		m_renderer = GetComponent<Renderer>();
		UpdateProperties();
	}

	private void OnValidate()
	{
		ValidateAnimationData();
		UpdateProperties();
	}

	private void ValidateAnimationData()
	{
		if (AnimationData == null)
		{
			return;
		}
		AnimationClipInfo[] animationData = AnimationData;
		foreach (AnimationClipInfo checkInfo in animationData)
		{
			OriginalFPS = Mathf.Max(OriginalFPS, 1);
			AnimationSpeed = Mathf.Max(AnimationSpeed, 0.01f);
			OriginalFrameCount = Mathf.Max(OriginalFrameCount, 2);
			checkInfo.frameRange.x = Mathf.Clamp(checkInfo.frameRange.x, 0, OriginalFrameCount - 2);
			checkInfo.frameRange.y = Mathf.Clamp(checkInfo.frameRange.y, 1, OriginalFrameCount - 1);
			if (checkInfo.frameRange.y <= checkInfo.frameRange.x)
			{
				checkInfo.frameRange.y = checkInfo.frameRange.x + 1;
			}
		}
	}

	private void UpdateProperties()
	{
		if ((bool)m_renderer)
		{
			m_currentAnimationSpeed = AnimationSpeed;
			m_renderer.GetPropertyBlock(m_properties);
			float animationTime = CalculateAnimationTime();
			m_properties.SetFloat("_AnimTime", animationTime);
			m_properties.SetFloat("_VertCount", VertexCount);
			m_properties.SetFloat("_FrameCount", RecordedFrameCount);
			m_properties.SetTexture("_MorphTex", AnimationTexture);
			m_renderer.SetPropertyBlock(m_properties);
		}
	}

	public void StartAnimation(string animationName)
	{
		if ((bool)m_renderer)
		{
			m_renderer.GetPropertyBlock(m_properties);
			if (!Mathf.Approximately(m_properties.GetFloat("_VertCount"), VertexCount))
			{
				UpdateProperties();
			}
			m_animationIsActive = true;
			m_animationName = animationName;
			float animationOffet = Time.timeSinceLevelLoad - CalculateStartFrameTime(m_animationName);
			m_properties.SetFloat("_TimeOffset", animationOffet);
			m_renderer.SetPropertyBlock(m_properties);
		}
	}

	public void SetAnimationCompletionPercent(string animationName, float completionPercent)
	{
		if (!m_renderer)
		{
			m_renderer = GetComponent<Renderer>();
		}
		if (m_properties == null)
		{
			m_properties = new MaterialPropertyBlock();
		}
		m_renderer.GetPropertyBlock(m_properties);
		m_animationIsActive = true;
		m_animationName = animationName;
		if (!Mathf.Approximately(m_properties.GetFloat("_VertCount"), VertexCount))
		{
			UpdateProperties();
		}
		completionPercent = Mathf.Clamp01(completionPercent);
		float animationLength = GetAnimationLength(m_animationName);
		float animationOffset = Time.timeSinceLevelLoad - CalculateStartFrameTime(m_animationName) - completionPercent * animationLength;
		m_properties.SetFloat("_TimeOffset", animationOffset);
		m_renderer.SetPropertyBlock(m_properties);
	}

	[Obsolete("Use SetCurrentAnimationCompletionPercent().")]
	public void SetAnimationCompletionPercent(float completionPercent)
	{
		SetCurrentAnimationCompletionPercent(completionPercent);
	}

	public void SetCurrentAnimationCompletionPercent(float completionPercent)
	{
		SetAnimationCompletionPercent(m_animationName, completionPercent);
	}

	private void SetAnimationSpeed(float animationSpeed)
	{
		if (!base.gameObject.activeInHierarchy)
		{
			return;
		}
		if (m_renderer == null)
		{
			throw new NullReferenceException($"Attempting to set a null renderer properties. GameObject: {base.gameObject}, full path: {base.gameObject.GetFullPath()}");
		}
		if (!(animationSpeed > 0f) || animationSpeed == m_currentAnimationSpeed)
		{
			return;
		}
		try
		{
			m_renderer.GetPropertyBlock(m_properties);
			m_currentAnimationSpeed = animationSpeed;
			m_properties.SetFloat("_AnimTime", CalculateAnimationTime());
			m_renderer.SetPropertyBlock(m_properties);
		}
		catch (Exception ex)
		{
			Log.Graphics.Log(LogLevel.Error, $"Could not set animation speed.  GameObject: {base.gameObject}, full path: {base.gameObject.GetFullPath()}, exception:{ex.Message}, {ex.StackTrace}");
		}
	}

	public void OverwriteAnimationSpeed(float animationSpeed)
	{
		float completionPercent = 1f;
		if (m_animationIsActive)
		{
			completionPercent = CurrentAnimationCompletionPercent;
		}
		SetAnimationSpeed(animationSpeed);
		if (m_animationIsActive)
		{
			SetAnimationCompletionPercent(m_animationName, completionPercent);
		}
	}

	public void UseDefaultAnimationSpeed()
	{
		SetAnimationSpeed(AnimationSpeed);
	}

	public float GetAnimationLengthUnscaled(string animationName)
	{
		AnimationClipInfo info = GetAnimationInfo(animationName);
		if (info != null)
		{
			return (float)(info.frameRange.y - info.frameRange.x) / (float)OriginalFPS;
		}
		return 0f;
	}

	public float GetAnimationLength(string animationName)
	{
		return GetAnimationLengthUnscaled(animationName) / CurrentAnimationSpeed;
	}

	private float CalculateStartFrameTime(string animationName)
	{
		AnimationClipInfo info = GetAnimationInfo(animationName);
		if (info != null)
		{
			return (float)info.frameRange.x / (float)OriginalFPS / m_currentAnimationSpeed;
		}
		return 0f;
	}

	private AnimationClipInfo GetAnimationInfo(string animationName)
	{
		AnimationClipInfo[] animationData = AnimationData;
		foreach (AnimationClipInfo checkData in animationData)
		{
			if (checkData.name == animationName)
			{
				return checkData;
			}
		}
		return null;
	}

	private float CalculateAnimationTime()
	{
		return (float)OriginalFrameCount / (float)OriginalFPS / m_currentAnimationSpeed;
	}
}
