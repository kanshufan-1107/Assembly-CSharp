using System;
using System.Linq;
using Blizzard.T5.MaterialService.Extensions;
using UnityEngine;

public class TextureOffsetStates : MonoBehaviour
{
	public TextureOffsetState[] m_states;

	private string m_currentState;

	private Material m_originalMaterial;

	public string CurrentState
	{
		get
		{
			return m_currentState;
		}
		set
		{
			TextureOffsetState state = m_states.FirstOrDefault((TextureOffsetState s) => s.Name.Equals(value, StringComparison.InvariantCultureIgnoreCase));
			if (state != null)
			{
				m_currentState = value;
				Renderer renderer = GetComponent<Renderer>();
				if (state.Material == null)
				{
					renderer.SetMaterial(m_originalMaterial);
				}
				else
				{
					renderer.SetMaterial(state.Material);
				}
				renderer.GetMaterial().mainTextureOffset = state.Offset;
			}
		}
	}

	private void Awake()
	{
		m_originalMaterial = GetComponent<Renderer>().GetSharedMaterial();
	}
}
