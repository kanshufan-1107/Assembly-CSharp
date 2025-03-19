using Hearthstone.UI.Core;
using UnityEngine;

namespace Hearthstone.UI.Tests;

[AddComponentMenu("")]
public class UIFrameworkTests_Targets : MonoBehaviour
{
	[Overridable]
	public string String { get; set; }

	[Overridable]
	public Color Color { get; set; }

	[Overridable]
	public float Float { get; set; }

	[Overridable]
	public double Double { get; set; }

	[Overridable]
	public bool Bool { get; set; }

	[Overridable]
	public int Int { get; set; }

	[Overridable]
	public long Long { get; set; }

	[Overridable]
	public Vector2 Vector2 { get; set; }

	[Overridable]
	public Vector3 Vector3 { get; set; }

	[Overridable]
	public Vector4 Vector4 { get; set; }

	[Overridable]
	public Texture2D Texture2D { get; set; }

	[Overridable]
	public Material Material { get; set; }

	[Overridable]
	public Mesh Mesh { get; set; }

	[Overridable]
	public Widget.InitializationState Enum { get; set; }
}
