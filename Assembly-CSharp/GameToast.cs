using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameToast : MonoBehaviour
{
	public List<Material> m_intensityMaterials = new List<Material>();

	private void Start()
	{
		UpdateIntensity(16f);
		Hashtable args = iTweenManager.Get().GetTweenHashTable();
		args.Add("time", 0.5f);
		args.Add("from", 16f);
		args.Add("to", 1f);
		args.Add("delay", 0.25f);
		args.Add("easetype", iTween.EaseType.easeOutCubic);
		args.Add("onupdate", "UpdateIntensity");
		iTween.ValueTo(base.gameObject, args);
	}

	private void UpdateIntensity(float intensity)
	{
		foreach (Material intensityMaterial in m_intensityMaterials)
		{
			intensityMaterial.SetFloat("_Intensity", intensity);
		}
	}
}
