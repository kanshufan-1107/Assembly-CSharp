using System;
using UnityEngine;

namespace Hearthstone.Timeline;

[Serializable]
public class MaterialModifierEntry
{
	public enum EntryType
	{
		[InspectorName("Color Modifier")]
		Color,
		[InspectorName("Float Modifier")]
		Float,
		[InspectorName("Vector Modifier")]
		Vector,
		[InspectorName("Texture Modifier")]
		Texture,
		[InspectorName("Texture Properties Modifier")]
		TextureProperties
	}

	public string key = "";

	public EntryType entryType;

	public AnimatedFloat floatValue = new AnimatedFloat();

	public AnimatedVector4 vectorValue = new AnimatedVector4();

	public AnimatedColor colorValue = new AnimatedColor();

	public Texture2D texture;

	public AnimatedTextureProperties texturePropertiesValue = new AnimatedTextureProperties();

	[HideInInspector]
	public float randomValue = 0.5f;
}
