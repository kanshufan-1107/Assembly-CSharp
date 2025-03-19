using System;
using UnityEngine;
using UnityEngine.Playables;

namespace Hearthstone.Timeline;

[Serializable]
public class LightModifierBehaviour : PlayableBehaviour
{
	[Serializable]
	private enum FindType
	{
		NameAndTag,
		Name,
		Tag
	}

	[Serializable]
	private enum MixType
	{
		None,
		Override,
		Additive,
		Multiplicative
	}

	[Serializable]
	private struct ColorEntry
	{
		public float time;

		public Color color;
	}

	[SerializeField]
	private FindType m_lightFindType;

	[SerializeField]
	private string m_lightName = "Directional light Main";

	[SerializeField]
	private string m_lightTag = string.Empty;

	[SerializeField]
	private Gradient m_color;

	[SerializeField]
	[Tooltip("Define colors using a gradient field or an array?\n\nThe gradient field is cleaner and easier to work with, but only supports a maximum of eight colors.")]
	private bool m_useGradient = true;

	[SerializeField]
	private ColorEntry[] m_colorArray;

	[SerializeField]
	private MixType m_colorMix;

	[SerializeField]
	private AnimationCurve m_intensity = new AnimationCurve(new Keyframe(0f, 1f), new Keyframe(1f, 1f));

	[SerializeField]
	private MixType m_intensityMix;

	private Light m_light;

	private bool m_firstFrameHappened;

	private Color m_originalColor;

	private float m_originalIntensity = 1f;

	public override void ProcessFrame(Playable playable, FrameData info, object playerData)
	{
		if (!m_firstFrameHappened)
		{
			m_firstFrameHappened = true;
			m_light = null;
			if (m_lightFindType == FindType.NameAndTag && m_lightTag != "Untagged" && !string.IsNullOrEmpty(m_lightTag))
			{
				GameObject[] possibleObjects = GameObject.FindGameObjectsWithTag(m_lightTag);
				for (int i = 0; i < possibleObjects.Length && (!(possibleObjects[i].name == m_lightName) || !possibleObjects[i].TryGetComponent<Light>(out m_light)); i++)
				{
				}
			}
			else if (m_lightFindType == FindType.Name || (m_lightFindType == FindType.NameAndTag && (m_lightTag == "Untagged" || string.IsNullOrEmpty(m_lightTag))))
			{
				if (!GameObject.Find(m_lightName).TryGetComponent<Light>(out m_light))
				{
					return;
				}
			}
			else if (m_lightFindType == FindType.Tag)
			{
				if (m_lightTag == "Untagged" || string.IsNullOrEmpty(m_lightTag))
				{
					m_light = UnityEngine.Object.FindObjectOfType<Light>();
				}
				else
				{
					GameObject[] possibleObjects2 = GameObject.FindGameObjectsWithTag(m_lightTag);
					for (int j = 0; j < possibleObjects2.Length && !possibleObjects2[j].TryGetComponent<Light>(out m_light); j++)
					{
					}
				}
			}
			if (m_light == null)
			{
				return;
			}
			m_originalColor = m_light.color;
			m_originalIntensity = m_light.intensity;
		}
		if (!(m_light == null))
		{
			float t = (float)playable.GetTime() / (float)playable.GetDuration();
			Color color = GetColorAtTime(t);
			switch (m_colorMix)
			{
			case MixType.Override:
				m_light.color = Color.Lerp(m_originalColor, color, color.a);
				break;
			case MixType.Additive:
				m_light.color = Color.Lerp(m_originalColor, m_originalColor + color, color.a);
				break;
			case MixType.Multiplicative:
				m_light.color = Color.Lerp(m_originalColor, m_originalColor * color, color.a);
				break;
			default:
				m_light.color = m_originalColor;
				break;
			}
			float intensity = m_intensity.Evaluate(t);
			switch (m_intensityMix)
			{
			case MixType.Override:
				m_light.intensity = intensity;
				break;
			case MixType.Additive:
				m_light.intensity = m_originalIntensity + intensity;
				break;
			case MixType.Multiplicative:
				m_light.intensity = m_originalIntensity * intensity;
				break;
			default:
				m_light.intensity = m_originalIntensity;
				break;
			}
		}
	}

	private Color GetColorAtTime(float time)
	{
		if (m_useGradient)
		{
			return m_color.Evaluate(time);
		}
		if (m_colorArray.Length == 0)
		{
			return new Color(1f, 1f, 1f, 0f);
		}
		if (m_colorArray.Length == 1)
		{
			return m_colorArray[0].color;
		}
		if (m_colorArray[0].time > time)
		{
			return m_colorArray[0].color;
		}
		if (m_colorArray[m_colorArray.Length - 1].time < time)
		{
			return m_colorArray[m_colorArray.Length - 1].color;
		}
		for (int i = 1; i < m_colorArray.Length; i++)
		{
			if (m_colorArray[i - 1].time <= time && m_colorArray[i].time >= time)
			{
				float t = time - m_colorArray[i - 1].time;
				t /= m_colorArray[i].time - m_colorArray[i - 1].time;
				return Color.Lerp(m_colorArray[i - 1].color, m_colorArray[i].color, t);
			}
		}
		return new Color(1f, 1f, 1f, 0f);
	}

	public override void OnBehaviourPause(Playable playable, FrameData info)
	{
		m_firstFrameHappened = false;
		if (!(m_light == null))
		{
			m_light.color = m_originalColor;
			m_light.intensity = m_originalIntensity;
			base.OnBehaviourPause(playable, info);
		}
	}
}
