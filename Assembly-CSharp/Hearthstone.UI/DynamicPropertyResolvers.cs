using System;
using System.Collections.Generic;
using UnityEngine;

namespace Hearthstone.UI;

public static class DynamicPropertyResolvers
{
	private static Dictionary<Type, IDynamicPropertyResolverProxy> s_proxies = new Dictionary<Type, IDynamicPropertyResolverProxy>
	{
		{
			typeof(Transform),
			new TransformDynamicPropertyResolverProxy()
		},
		{
			typeof(MeshRenderer),
			new RendererDynamicPropertyResolverProxy()
		},
		{
			typeof(ParticleSystemRenderer),
			new RendererDynamicPropertyResolverProxy()
		},
		{
			typeof(SkinnedMeshRenderer),
			new RendererDynamicPropertyResolverProxy()
		},
		{
			typeof(PlayMakerFSM),
			new PlayMakerDynamicPropertyResolverProxy()
		},
		{
			typeof(ParticleSystem),
			new ParticleSystemDynamicPropertyResolverProxy()
		},
		{
			typeof(SpriteRenderer),
			new SpriteRendererDynamicPropertyResolverProxy()
		}
	};

	public static IDynamicPropertyResolver TryGetResolver(object target)
	{
		if (target == null)
		{
			return null;
		}
		if (target is IDynamicPropertyResolver resolver)
		{
			return resolver;
		}
		if (s_proxies.TryGetValue(target.GetType(), out var proxy))
		{
			proxy.SetTarget(target);
			return proxy;
		}
		return null;
	}
}
