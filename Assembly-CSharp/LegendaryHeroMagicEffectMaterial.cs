using System;
using UnityEngine;

[CreateAssetMenu(fileName = "Magic Effect Material", menuName = "ScriptableObjects/Legendary Hero/Magic Effect Material")]
public class LegendaryHeroMagicEffectMaterial : ScriptableObject
{
	[Serializable]
	public struct StreamSubData
	{
		[Header("Texture")]
		public Texture Texture;

		[Header("Color")]
		public Color ColorHigh;

		public Color ColorLow;

		[Range(0f, 4f)]
		public float Intensity;

		[Header("Animation")]
		public float Scale;

		public Vector2 ScrollRate;
	}

	[Serializable]
	public struct StreamData
	{
		[Range(0f, 1f)]
		public float SoftEdgeControl;

		public StreamSubData Stream1;

		public StreamSubData Stream2;
	}

	[Serializable]
	public struct NoiseData
	{
		[Range(0f, 1f)]
		public float Magnitude;

		public float FrequencyScale;

		public SinNoiseFunction RadialNoiseFunction;

		public SinNoiseFunction VerticalNoiseFunction;
	}

	[Serializable]
	public struct SoulsData
	{
		[Header("Texture")]
		public Texture Texture;

		[Header("Color")]
		public Color ColorHigh;

		public Color ColorLow;

		[Header("Blurring (Texture Mip Map)")]
		public float MipMapRate;

		[Range(0f, 10f)]
		public float MipMapMin;

		[Range(0f, 10f)]
		public float MipMapMax;

		public Vector3 MipMapHash;

		[Range(0f, 1f)]
		[Header("Position and Size")]
		public float VerticalSpread;

		[Range(0f, 1f)]
		public float VerticalScale;

		public float HorizontalScale;

		[Min(0f)]
		public float HorizontalSpacing;

		[Header("Offsets From Primary")]
		[Range(0f, 1f)]
		public float Offset1;

		[Range(0f, 1f)]
		public float Offset2;

		[Range(0f, 1f)]
		public float Offset3;

		[Header("Animation")]
		public float Speed;
	}

	[Header("Mesh")]
	public Mesh Mesh;

	public NoiseData Noise;

	[Header("Shader")]
	public Shader Shader;

	[Header("Effects")]
	public StreamData Stream;

	public SoulsData Souls;

	private static readonly int s_stream1TextureID = Shader.PropertyToID("_Stream1Tex");

	private static readonly int s_stream2TextureID = Shader.PropertyToID("_Stream2Tex");

	private static readonly int s_stream1ColorLowID = Shader.PropertyToID("_Stream1ColorLow");

	private static readonly int s_stream2ColorLowID = Shader.PropertyToID("_Stream2ColorLow");

	private static readonly int s_stream1ColorHighID = Shader.PropertyToID("_Stream1ColorHigh");

	private static readonly int s_stream2ColorHighID = Shader.PropertyToID("_Stream2ColorHigh");

	private static readonly int s_soulsTextureID = Shader.PropertyToID("_SoulsTex");

	private static readonly int s_soulsColorLowID = Shader.PropertyToID("_SoulsColorLow");

	private static readonly int s_soulsColorHighID = Shader.PropertyToID("_SoulsColorHigh");

	private static readonly int s_mipMapRangeID = Shader.PropertyToID("_MipMapRange");

	private static readonly int s_softEdgeControlID = Shader.PropertyToID("_SoftEdgeControl");

	private static readonly int s_verticalNoiseFrequencyID = Shader.PropertyToID("_VerticalNoiseFrequency");

	private static readonly int s_verticalNoiseOffsetID = Shader.PropertyToID("_VerticalNoiseOffset");

	private static readonly int s_verticalNoiseAmplitudeID = Shader.PropertyToID("_VerticalNoiseAmplitude");

	private static readonly int s_radialNoiseFrequencyID = Shader.PropertyToID("_RadialNoiseFrequency");

	private static readonly int s_radialNoiseOffsetID = Shader.PropertyToID("_RadialNoiseOffset");

	private static readonly int s_radialNoiseAmplitudeID = Shader.PropertyToID("_RadialNoiseAmplitude");

	private static readonly int s_stream1UVScaleAndOffsetID = Shader.PropertyToID("_Stream1UVScaleAndOffset");

	private static readonly int s_stream2UVScaleAndOffsetID = Shader.PropertyToID("_Stream2UVScaleAndOffset");

	private static readonly int s_mipMapControlID = Shader.PropertyToID("_MipMapControl");

	private static readonly int s_soulsHorizontalSpaceID = Shader.PropertyToID("_SoulsHorizontalSpacing");

	private static readonly int s_soul1UVScaleAndOffsetID = Shader.PropertyToID("_Soul1UVScaleAndOffset");

	private static readonly int s_soul2UVScaleAndOffsetID = Shader.PropertyToID("_Soul2UVScaleAndOffset");

	private static readonly int s_soul3UVScaleAndOffsetID = Shader.PropertyToID("_Soul3UVScaleAndOffset");

	private static readonly int s_soul4UVScaleAndOffsetID = Shader.PropertyToID("_Soul4UVScaleAndOffset");

	public LegendaryHeroMagicEffectState UpdateState(float deltaTime, in LegendaryHeroMagicEffectState oldState)
	{
		Vector4 rate = (Noise.VerticalNoiseFunction ? Noise.VerticalNoiseFunction.OffsetRate : Vector4.zero);
		Vector4 newRotationState = oldState.RotationState + deltaTime * rate;
		newRotationState.x = Mathf.Repeat(newRotationState.x, (float)Math.PI * 2f);
		newRotationState.y = Mathf.Repeat(newRotationState.y, (float)Math.PI * 2f);
		newRotationState.z = Mathf.Repeat(newRotationState.z, (float)Math.PI * 2f);
		newRotationState.w = Mathf.Repeat(newRotationState.w, (float)Math.PI * 2f);
		Vector4 radialRate = (Noise.RadialNoiseFunction ? Noise.RadialNoiseFunction.OffsetRate : Vector4.zero);
		Vector4 newRadialState = oldState.RadialState + deltaTime * radialRate;
		newRadialState.x = Mathf.Repeat(newRadialState.x, (float)Math.PI * 2f);
		newRadialState.y = Mathf.Repeat(newRadialState.y, (float)Math.PI * 2f);
		newRadialState.z = Mathf.Repeat(newRadialState.z, (float)Math.PI * 2f);
		newRadialState.w = Mathf.Repeat(newRadialState.w, (float)Math.PI * 2f);
		Vector2 newUV1State = oldState.UV1State + deltaTime * Stream.Stream1.ScrollRate;
		newUV1State.x = Mathf.Repeat(newUV1State.x, 1f);
		newUV1State.y = Mathf.Repeat(newUV1State.y, 1f);
		Vector2 newUV2State = oldState.UV2State + deltaTime * Stream.Stream2.ScrollRate;
		newUV2State.x = Mathf.Repeat(newUV2State.x, 1f);
		newUV2State.y = Mathf.Repeat(newUV2State.y, 1f);
		float newMipMapState = oldState.MipMapState + deltaTime * Souls.MipMapRate;
		newMipMapState = Mathf.Repeat(newMipMapState, 1f);
		float newSoulsState = oldState.SoulsState + deltaTime * Souls.Speed;
		newSoulsState = Mathf.Repeat(newSoulsState, 1f);
		LegendaryHeroMagicEffectState result = default(LegendaryHeroMagicEffectState);
		result.RotationState = newRotationState;
		result.RadialState = newRadialState;
		result.UV1State = newUV1State;
		result.UV2State = newUV2State;
		result.MipMapState = newMipMapState;
		result.SoulsState = newSoulsState;
		return result;
	}

	public void InitialiseMaterial(Material material)
	{
		Vector4 soulsColorLow = new Vector4(Souls.ColorLow.r, Souls.ColorLow.g, Souls.ColorLow.b, 1f) * Souls.ColorLow.a;
		Vector4 soulsColorHigh = new Vector4(Souls.ColorHigh.r, Souls.ColorHigh.g, Souls.ColorHigh.b, 1f) * Souls.ColorHigh.a;
		Vector4 stream1Intensity = new Vector4(Stream.Stream1.Intensity, Stream.Stream1.Intensity, Stream.Stream1.Intensity, 1f);
		Vector4 stream1ColorLow = Stream.Stream1.ColorLow * stream1Intensity;
		Vector4 stream1ColorHigh = Stream.Stream1.ColorHigh * stream1Intensity;
		Vector4 stream2Intensity = new Vector4(Stream.Stream2.Intensity, Stream.Stream2.Intensity, Stream.Stream2.Intensity, 1f);
		Vector4 stream2ColorLow = Stream.Stream2.ColorLow * stream2Intensity;
		Vector4 stream2ColorHigh = Stream.Stream2.ColorHigh * stream2Intensity;
		material.SetTexture(s_stream1TextureID, Stream.Stream1.Texture);
		material.SetTexture(s_stream2TextureID, Stream.Stream2.Texture);
		material.SetTexture(s_soulsTextureID, Souls.Texture);
		material.SetColor(s_stream1ColorLowID, stream1ColorLow);
		material.SetColor(s_stream2ColorLowID, stream2ColorLow);
		material.SetColor(s_stream1ColorHighID, stream1ColorHigh);
		material.SetColor(s_stream2ColorHighID, stream2ColorHigh);
		material.SetVector(s_soulsColorLowID, soulsColorLow);
		material.SetVector(s_soulsColorHighID, soulsColorHigh);
		material.SetVector(s_mipMapRangeID, new Vector2(Souls.MipMapMin, Souls.MipMapMax));
		material.SetFloat(s_softEdgeControlID, 2f / Mathf.Max(Mathf.Epsilon, Stream.SoftEdgeControl));
		SinNoiseFunction verticalNoise = Noise.VerticalNoiseFunction;
		if (verticalNoise != null)
		{
			material.SetVector(s_verticalNoiseFrequencyID, verticalNoise.Frequency * ((float)Math.PI * 2f) * Noise.FrequencyScale);
			material.SetVector(s_verticalNoiseAmplitudeID, verticalNoise.GetAmplitude(Noise.Magnitude));
		}
		else
		{
			material.SetVector(s_verticalNoiseFrequencyID, Vector4.zero);
			material.SetVector(s_verticalNoiseAmplitudeID, Vector4.zero);
		}
		SinNoiseFunction radialNoise = Noise.RadialNoiseFunction;
		if (radialNoise != null)
		{
			material.SetVector(s_radialNoiseFrequencyID, radialNoise.Frequency * ((float)Math.PI * 2f) * Noise.FrequencyScale);
			material.SetVector(s_radialNoiseAmplitudeID, radialNoise.GetAmplitude(Noise.Magnitude));
		}
		else
		{
			material.SetVector(s_radialNoiseFrequencyID, Vector4.zero);
			material.SetVector(s_radialNoiseAmplitudeID, Vector4.zero);
		}
	}

	public void UpdateMaterialState(Material material, in LegendaryHeroMagicEffectState state)
	{
		if (Noise.VerticalNoiseFunction != null)
		{
			material.SetVector(s_verticalNoiseOffsetID, state.RotationState);
		}
		else
		{
			material.SetVector(s_verticalNoiseOffsetID, Vector4.zero);
		}
		if (Noise.RadialNoiseFunction != null)
		{
			material.SetVector(s_radialNoiseOffsetID, state.RotationState);
		}
		else
		{
			material.SetVector(s_radialNoiseOffsetID, Vector4.zero);
		}
		material.SetVector(s_stream1UVScaleAndOffsetID, new Vector4(Stream.Stream1.Scale, 1f, state.UV1State.x, state.UV1State.y));
		material.SetVector(s_stream2UVScaleAndOffsetID, new Vector4(Stream.Stream2.Scale, 1f, state.UV2State.x, state.UV2State.y));
		material.SetVector(s_mipMapControlID, new Vector3(Souls.MipMapHash.x, Souls.MipMapHash.y, Souls.MipMapHash.z * Mathf.Cos(state.MipMapState * ((float)Math.PI * 2f))));
		float verticalTrack0 = 0.5f * (1f + Souls.VerticalSpread);
		float verticalTrack3 = 1f - verticalTrack0;
		float verticalTrack4 = (verticalTrack0 + 2f * verticalTrack3) / 3f;
		float verticalTrack5 = (2f * verticalTrack0 + verticalTrack3) / 3f;
		float invScale = 1f / Souls.VerticalScale;
		float soulSpacing = 1f + Souls.HorizontalSpacing;
		Vector4 uvScaleAndOffset1 = new Vector4(Souls.HorizontalScale, invScale, soulSpacing * state.SoulsState, 0.5f - verticalTrack0 * invScale);
		Vector4 uvScaleAndOffset2 = new Vector4(Souls.HorizontalScale, invScale, soulSpacing * (state.SoulsState + Souls.Offset1), 0.5f - verticalTrack5 * invScale);
		Vector4 uvScaleAndOffset3 = new Vector4(Souls.HorizontalScale, invScale, soulSpacing * (state.SoulsState + Souls.Offset2), 0.5f - verticalTrack4 * invScale);
		Vector4 uvScaleAndOffset4 = new Vector4(Souls.HorizontalScale, invScale, soulSpacing * (state.SoulsState + Souls.Offset3), 0.5f - verticalTrack3 * invScale);
		material.SetFloat(s_soulsHorizontalSpaceID, soulSpacing);
		material.SetVector(s_soul1UVScaleAndOffsetID, uvScaleAndOffset1);
		material.SetVector(s_soul2UVScaleAndOffsetID, uvScaleAndOffset2);
		material.SetVector(s_soul3UVScaleAndOffsetID, uvScaleAndOffset3);
		material.SetVector(s_soul4UVScaleAndOffsetID, uvScaleAndOffset4);
	}
}
