using System;
using System.Collections.Generic;
using Blizzard.T5.MaterialService.Extensions;
using UnityEngine;

[RequireComponent(typeof(AdventureWing))]
[CustomEditClass]
public class AdventureWingKarazhanHelper : MonoBehaviour
{
	[Serializable]
	public class WingSpecificObject
	{
		public WingDbId m_wingDbId;

		public GameObject m_ObjectSpecificToWing;

		public float m_backgroundOffset;
	}

	public List<WingSpecificObject> m_WingSpecificObjects = new List<WingSpecificObject>();

	public List<MeshRenderer> m_backgroundRenderers = new List<MeshRenderer>();

	public List<Animator> m_adventureCompleteAnimators = new List<Animator>();

	public PlayMakerFSM m_doorOpenPlayMakerFSM;

	private AdventureWing m_adventureWing;

	private GameObject m_objectForThisWing;

	private float m_backgroundOffsetForThisWing;

	public void Initialize()
	{
		m_adventureWing = GetComponent<AdventureWing>();
		if (m_adventureWing == null)
		{
			Debug.LogError("AdventureWingKarazhanHelper could not find an AdventureWing component on the same GameObject!");
			return;
		}
		WingDbId wingId = m_adventureWing.GetWingId();
		for (int i = 0; i < m_WingSpecificObjects.Count; i++)
		{
			WingSpecificObject element = m_WingSpecificObjects[i];
			element.m_ObjectSpecificToWing.SetActive(value: false);
			if (element.m_wingDbId != wingId)
			{
				continue;
			}
			m_objectForThisWing = element.m_ObjectSpecificToWing;
			foreach (MeshRenderer backgroundRenderer in m_backgroundRenderers)
			{
				Material material = backgroundRenderer.GetMaterial();
				Vector2 newOffset = material.GetTextureOffset("_MainTex");
				newOffset.y = element.m_backgroundOffset;
				material.SetTextureOffset("_MainTex", newOffset);
			}
		}
		if (m_objectForThisWing == null)
		{
			Debug.LogError("AdventureWingKarazhanHelper could not find an object for m_objectForThisWing!");
			return;
		}
		m_objectForThisWing.SetActive(value: true);
		PegUIElement button = m_objectForThisWing.GetComponentInChildren<PegUIElement>();
		if (button == null)
		{
			Debug.LogError("AdventureWingKarazhanHelper could not find the unlock button!");
			return;
		}
		m_adventureWing.m_UnlockButton = button;
		PlayMakerFSM[] playmakers = m_adventureWing.m_WingEventTable.GetComponentsInChildren<PlayMakerFSM>();
		for (int j = 0; j < playmakers.Length; j++)
		{
			playmakers[j].FsmVariables.GetFsmGameObject("KnockerRootVar").Value = button.gameObject;
		}
		m_doorOpenPlayMakerFSM.FsmVariables.GetFsmGameObject("KnockerHeadVar").Value = m_objectForThisWing;
		AdventureConfig adventureConfig = AdventureConfig.Get();
		AdventureDbId selectedAdv = adventureConfig.GetSelectedAdventure();
		AdventureModeDbId selectedMode = adventureConfig.GetSelectedMode();
		if (!AdventureProgressMgr.Get().IsAdventureModeAndSectionComplete(selectedAdv, selectedMode))
		{
			return;
		}
		foreach (Animator adventureCompleteAnimator in m_adventureCompleteAnimators)
		{
			adventureCompleteAnimator.enabled = true;
		}
	}
}
