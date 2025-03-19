using System;
using System.Linq;
using Blizzard.T5.Core;
using Hearthstone.UI.Internal;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Hearthstone.UI;

public static class NestedReferenceUtils
{
	private static readonly Map<Type, NestedReference.TargetTypes> s_targetTypeMap = new Map<Type, NestedReference.TargetTypes>
	{
		{
			typeof(string),
			NestedReference.TargetTypes.String
		},
		{
			typeof(Color),
			NestedReference.TargetTypes.Color
		},
		{
			typeof(float),
			NestedReference.TargetTypes.Float
		},
		{
			typeof(double),
			NestedReference.TargetTypes.Double
		},
		{
			typeof(long),
			NestedReference.TargetTypes.Long
		},
		{
			typeof(int),
			NestedReference.TargetTypes.Int
		},
		{
			typeof(bool),
			NestedReference.TargetTypes.Bool
		},
		{
			typeof(Material),
			NestedReference.TargetTypes.Material
		},
		{
			typeof(Texture2D),
			NestedReference.TargetTypes.Texture
		},
		{
			typeof(Vector2),
			NestedReference.TargetTypes.Vector2
		},
		{
			typeof(Vector3),
			NestedReference.TargetTypes.Vector3
		},
		{
			typeof(Vector4),
			NestedReference.TargetTypes.Vector4
		},
		{
			typeof(Mesh),
			NestedReference.TargetTypes.Mesh
		}
	};

	public static Scene FindSceneThatGameObjectBelongsTo(GameObject gameObject)
	{
		for (int i = 0; i < SceneManager.sceneCount; i++)
		{
			Scene scene = SceneManager.GetSceneAt(i);
			if (scene.isLoaded && scene.GetRootGameObjects().Any((GameObject a) => gameObject.transform.IsChildOf(a.transform) || a == gameObject.transform))
			{
				return scene;
			}
		}
		return default(Scene);
	}

	public static INestedReferenceResolver FindParentNestedResolver(Component component)
	{
		Transform t = component.transform;
		while (t != null)
		{
			if (t.GetComponents<Component>().FirstOrDefault((Component a) => a is INestedReferenceResolver) is INestedReferenceResolver resolver)
			{
				return resolver;
			}
			t = t.parent;
		}
		return null;
	}

	public static bool IsSupportedType(Type type)
	{
		if (!type.IsEnum)
		{
			return s_targetTypeMap.ContainsKey(type);
		}
		return true;
	}

	public static NestedReference.TargetTypes ResolveTargetType(Type type)
	{
		if (type.IsEnum)
		{
			return NestedReference.TargetTypes.Enum;
		}
		NestedReference.TargetTypes targetType = NestedReference.TargetTypes.Unknown;
		if (s_targetTypeMap.TryGetValue(type, out targetType))
		{
			return targetType;
		}
		return NestedReference.TargetTypes.Unknown;
	}
}
