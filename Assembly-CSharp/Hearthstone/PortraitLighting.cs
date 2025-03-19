using UnityEngine;

namespace Hearthstone;

[CreateAssetMenu(fileName = "Portrait Lighting", menuName = "ScriptableObjects/Legendary Heros/Portrait Lighting")]
public class PortraitLighting : ScriptableObject
{
	[Header("Shadows")]
	public int resolution = 512;

	public Color ShadowColor;

	[Range(0.1f, 1f)]
	public float DepthBias = 0.25f;

	[Range(0.5f, 6f)]
	public float SoftnessFalloff = 3f;

	[Header("Specular Lighting")]
	public Texture Cubemap;

	[Range(0f, 1f)]
	public float CubemapRotation = 0.5f;

	[Header("Rim Lighting")]
	public Color RimLightColor;

	public Color HairRimLightColor;

	[Range(0f, 360f)]
	public float RimLightDirection = 45f;

	[Range(0f, 360f)]
	public float RimLightAngle = 45f;

	[Range(0f, 1f)]
	public float RimLightAngleSoftness = 0.5f;

	[Range(0f, 1f)]
	public float RimLightMinNormal = 0.85f;

	[Range(0f, 1f)]
	public float RimLightMaxNormal = 0.95f;

	public void OnValidate()
	{
		RimLightMaxNormal = Mathf.Max(RimLightMinNormal, RimLightMaxNormal);
	}
}
