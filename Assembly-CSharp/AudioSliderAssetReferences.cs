using UnityEngine;

[CustomEditClass]
public class AudioSliderAssetReferences : MonoBehaviour
{
	[CustomEditField(Sections = "Sound", T = EditType.SOUND_PREFAB)]
	public string m_onMasterVolumeReleasedAudio;

	[CustomEditField(Sections = "Sound", T = EditType.SOUND_PREFAB)]
	public string m_onMusicVolumeReleasedAudio;

	[CustomEditField(Sections = "Sound", T = EditType.SOUND_PREFAB)]
	public string m_onDialogVolumeReleasedAudio;

	[CustomEditField(Sections = "Sound", T = EditType.SOUND_PREFAB)]
	public string m_onAmbienceVolumeReleasedAudio;

	[CustomEditField(Sections = "Sound", T = EditType.SOUND_PREFAB)]
	public string m_onSoundEffectVolumeReleasedAudio;
}
