using Hearthstone;
using UnityEngine;

public class CardSound
{
	private string m_path;

	private AudioSource m_source;

	private Card m_owner;

	private bool m_alwaysValid;

	public CardSound(string path, Card owner, bool alwaysValid)
	{
		m_path = path;
		m_owner = owner;
		m_alwaysValid = alwaysValid;
	}

	public AudioSource GetSound(bool loadIfNeeded = true)
	{
		if (m_source == null && loadIfNeeded)
		{
			LoadSound();
		}
		return m_source;
	}

	public void Clear()
	{
		if (!(m_source == null))
		{
			Object.Destroy(m_source.gameObject);
		}
	}

	private void LoadSound()
	{
		if (string.IsNullOrEmpty(m_path) || !AssetLoader.Get().IsAssetAvailable(m_path))
		{
			return;
		}
		GameObject obj = SoundLoader.LoadSound(m_path);
		if (obj == null)
		{
			if (m_alwaysValid)
			{
				string errorMsg = $"CardSound.LoadSound() - Failed to load \"{m_path}\"";
				if (HearthstoneApplication.UseDevWorkarounds())
				{
					Debug.LogError(errorMsg);
				}
				else
				{
					Error.AddDevFatal(errorMsg);
				}
			}
			return;
		}
		m_source = obj.GetComponent<AudioSource>();
		if (m_source == null)
		{
			Object.Destroy(obj);
			if (m_alwaysValid)
			{
				string errorMsg2 = $"CardSound.LoadSound() - \"{m_path}\" does not have an AudioSource component.";
				if (HearthstoneApplication.UseDevWorkarounds())
				{
					Debug.LogError(errorMsg2);
				}
				else
				{
					Error.AddDevFatal(errorMsg2);
				}
			}
		}
		else
		{
			SetupSound();
		}
	}

	private void SetupSound()
	{
		if (!(m_source == null) && !(m_owner == null))
		{
			m_source.transform.parent = m_owner.transform;
			TransformUtil.Identity(m_source.transform);
		}
	}
}
