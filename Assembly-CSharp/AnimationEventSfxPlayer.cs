using UnityEngine;

public class AnimationEventSfxPlayer : MonoBehaviour
{
	private void PlaySFX(Object obj)
	{
		PlaySfx(obj);
	}

	private void PlaySfx(Object obj)
	{
		SoundPlayClipArgs args = null;
		if (obj is GameObject go && (bool)go)
		{
			Debug.Log("PlaySFX prefab: " + obj.name);
			SoundDef soundDef = go.GetComponent<SoundDef>();
			if (soundDef != null)
			{
				args = new SoundPlayClipArgs();
				args.m_def = soundDef;
				args.m_parentObject = base.gameObject;
			}
		}
		else if (obj is AudioClip ac && (bool)ac)
		{
			Debug.Log("PlaySFX clip: " + ac.name);
			args = new SoundPlayClipArgs();
			args.m_forcedAudioClip = ac;
			args.m_parentObject = base.gameObject;
		}
		if (args != null && SoundManager.Get() != null)
		{
			SoundManager.Get().PlayClip(args);
		}
	}
}
