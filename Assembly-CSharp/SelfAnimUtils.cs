using Blizzard.T5.MaterialService.Extensions;
using UnityEngine;

public class SelfAnimUtils : MonoBehaviour
{
	public void PrintLog(string message)
	{
		Debug.Log(message);
	}

	public void PrintLogWarning(string message)
	{
		Debug.LogWarning(message);
	}

	public void PrintLogError(string message)
	{
		Debug.LogError(message);
	}

	public void PlayAnimation()
	{
		if (TryGetComponent<Animation>(out var anim))
		{
			anim.Play();
		}
	}

	public void StopAnimation()
	{
		if (TryGetComponent<Animation>(out var anim))
		{
			anim.Stop();
		}
	}

	public void ActivateHierarchy()
	{
		base.gameObject.SetActive(value: true);
	}

	public void DeactivateHierarchy()
	{
		base.gameObject.SetActive(value: false);
	}

	public void DestroyHierarchy()
	{
		Object.Destroy(base.gameObject);
	}

	public void FadeIn(float FadeSec)
	{
		iTween.FadeTo(base.gameObject, 1f, FadeSec);
	}

	public void FadeOut(float FadeSec)
	{
		iTween.FadeTo(base.gameObject, 0f, FadeSec);
	}

	public void SetAlphaHierarchy(float alpha)
	{
		Renderer[] componentsInChildren = GetComponentsInChildren<Renderer>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			Material material = componentsInChildren[i].GetMaterial();
			if (material.HasProperty("_Color"))
			{
				Color color = material.color;
				color.a = alpha;
				material.color = color;
			}
		}
	}

	public void PlayDefaultSound()
	{
		if (!TryGetComponent<AudioSource>(out var source))
		{
			Debug.LogError($"SelfAnimUtils.PlayDefaultSound() - Tried to play the AudioSource on {base.gameObject} but it has no AudioSource. You need an AudioSource to use this function.");
		}
		else if (SoundManager.Get() == null)
		{
			source.Play();
		}
		else
		{
			SoundManager.Get().Play(source);
		}
	}

	public void PlaySound(SoundDef clip)
	{
		AudioSource source;
		if (clip == null)
		{
			Debug.LogError($"SelfAnimUtils.PlayDefaultSound() - No clip was given when trying to play the AudioSource on {base.gameObject}. You need a clip to use this function.");
		}
		else if (!TryGetComponent<AudioSource>(out source))
		{
			Debug.LogError($"SelfAnimUtils.PlayDefaultSound() - Tried to play clip {clip} on {base.gameObject} but it has no AudioSource. You need an AudioSource to use this function.");
		}
		else if (SoundManager.Get() == null)
		{
			Debug.LogErrorFormat("TargetAnimutils2: SoundManager is null attempting to play {0}", clip.m_AudioClip);
		}
		else
		{
			SoundManager.Get().PlayOneShot(source, clip);
		}
	}

	public void RandomRotationX()
	{
		TransformUtil.SetEulerAngleX(base.gameObject, Random.Range(0f, 360f));
	}

	public void RandomRotationY()
	{
		TransformUtil.SetEulerAngleY(base.gameObject, Random.Range(0f, 360f));
	}

	public void RandomRotationZ()
	{
		TransformUtil.SetEulerAngleZ(base.gameObject, Random.Range(0f, 360f));
	}
}
