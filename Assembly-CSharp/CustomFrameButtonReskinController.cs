using System;
using System.Collections.Generic;
using Blizzard.T5.MaterialService.Extensions;
using UnityEngine;

public class CustomFrameButtonReskinController : MonoBehaviour, ISerializationCallbackReceiver
{
	[Header("Button")]
	public UIBButton[] Buttons;

	public Material ButtonMaterial;

	public Material HolderMaterial;

	[Header("Artist Name")]
	public GameObject ArtistCredit;

	public Material ArtistCreditMaterial;

	[NonSerialized]
	public Material OverrideButtonMaterial;

	[NonSerialized]
	public Material OverrideHolderMaterial;

	[NonSerialized]
	public Material OverrideArtistCreditMaterial;

	[NonSerialized]
	public float VerticalOffset;

	[NonSerialized]
	public float ArtistCreditVerticalOffset;

	private readonly HashSet<Renderer> m_buttonRenderers = new HashSet<Renderer>();

	private readonly HashSet<Renderer> m_holderRenderers = new HashSet<Renderer>();

	private readonly HashSet<Renderer> m_creditRenderers = new HashSet<Renderer>();

	[HideInInspector]
	[SerializeField]
	private Vector3[] m_buttonPositions;

	[HideInInspector]
	[SerializeField]
	private Vector3 m_creditPosition;

	private void Awake()
	{
		Renderer[] componentsInChildren;
		if (Buttons != null)
		{
			UIBButton[] buttons = Buttons;
			for (int i = 0; i < buttons.Length; i++)
			{
				componentsInChildren = buttons[i].GetComponentsInChildren<Renderer>();
				foreach (Renderer renderer in componentsInChildren)
				{
					Material material = renderer.GetSharedMaterial();
					if (material == ButtonMaterial)
					{
						m_buttonRenderers.Add(renderer);
					}
					else if (material == HolderMaterial)
					{
						m_holderRenderers.Add(renderer);
					}
				}
			}
		}
		if (!(ArtistCredit != null))
		{
			return;
		}
		componentsInChildren = ArtistCredit.GetComponentsInChildren<Renderer>();
		foreach (Renderer renderer2 in componentsInChildren)
		{
			if (renderer2.GetSharedMaterial() == ArtistCreditMaterial)
			{
				m_creditRenderers.Add(renderer2);
			}
		}
	}

	private void OnEnable()
	{
		AssignMaterials();
	}

	private void OnDisable()
	{
		RestoreMaterials();
	}

	public void UpdateMaterials(CustomFrameButtonReskinData reskinData)
	{
		if (reskinData != null)
		{
			OverrideButtonMaterial = reskinData.ButtonMaterial;
			OverrideHolderMaterial = reskinData.HolderMaterial;
			VerticalOffset = reskinData.VerticalOffset;
			ArtistCreditVerticalOffset = reskinData.ArtistCreditVerticalOffset;
			if (base.isActiveAndEnabled)
			{
				AssignMaterials();
			}
		}
		else
		{
			OverrideButtonMaterial = null;
			OverrideHolderMaterial = null;
			VerticalOffset = 0f;
			ArtistCreditVerticalOffset = 0f;
			AssignMaterials();
		}
	}

	public void AssignMaterials()
	{
		if (OverrideButtonMaterial != null)
		{
			foreach (Renderer buttonRenderer in m_buttonRenderers)
			{
				buttonRenderer.SetSharedMaterial(OverrideButtonMaterial);
			}
		}
		if (OverrideHolderMaterial != null)
		{
			foreach (Renderer holderRenderer in m_holderRenderers)
			{
				holderRenderer.SetSharedMaterial(OverrideHolderMaterial);
			}
		}
		if (OverrideArtistCreditMaterial != null)
		{
			foreach (Renderer creditRenderer in m_creditRenderers)
			{
				creditRenderer.SetSharedMaterial(OverrideArtistCreditMaterial);
			}
		}
		if (Buttons != null && m_buttonPositions != null)
		{
			int numButtons = Buttons.Length;
			for (int idx = 0; idx < numButtons; idx++)
			{
				Buttons[idx].transform.localPosition = m_buttonPositions[idx] + new Vector3(0f, 0f, VerticalOffset);
			}
		}
		if (ArtistCredit != null)
		{
			ArtistCredit.transform.localPosition = m_creditPosition + new Vector3(0f, 0f, ArtistCreditVerticalOffset);
		}
	}

	public void RestoreMaterials()
	{
		foreach (Renderer buttonRenderer in m_buttonRenderers)
		{
			buttonRenderer.SetSharedMaterial(ButtonMaterial);
		}
		foreach (Renderer holderRenderer in m_holderRenderers)
		{
			holderRenderer.SetSharedMaterial(HolderMaterial);
		}
		foreach (Renderer creditRenderer in m_creditRenderers)
		{
			creditRenderer.SetSharedMaterial(ArtistCreditMaterial);
		}
		if (Buttons != null && m_buttonPositions != null)
		{
			int numButtons = Buttons.Length;
			for (int idx = 0; idx < numButtons; idx++)
			{
				Buttons[idx].transform.localPosition = m_buttonPositions[idx];
			}
		}
		if (ArtistCredit != null)
		{
			ArtistCredit.transform.localPosition = m_creditPosition;
		}
	}

	void ISerializationCallbackReceiver.OnBeforeSerialize()
	{
	}

	void ISerializationCallbackReceiver.OnAfterDeserialize()
	{
	}
}
