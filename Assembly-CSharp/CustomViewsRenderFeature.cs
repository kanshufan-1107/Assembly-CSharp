using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class CustomViewsRenderFeature : ScriptableRendererFeature
{
	public CustomViewEntryPoint entryPoint;

	public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
	{
		List<CustomViewPass> passes = CustomViewPass.GetQueue(entryPoint);
		if (passes == null)
		{
			return;
		}
		foreach (CustomViewPass pass in passes)
		{
			renderer.EnqueuePass(pass);
		}
	}

	public override void Create()
	{
		if (entryPoint == CustomViewEntryPoint.Count)
		{
			Debug.LogError("invalid entrypoint selected");
			SetActive(active: false);
		}
	}
}
