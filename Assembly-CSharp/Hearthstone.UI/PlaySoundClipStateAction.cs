using Assets;
using Hearthstone.UI.Logging;

namespace Hearthstone.UI;

public class PlaySoundClipStateAction : StateActionImplementation
{
	public static readonly int s_SoundDefIndex = 0;

	public static readonly int s_SoundCategoryIndex = 0;

	public static readonly int s_VolumeIndex = 0;

	public static readonly int s_PitchIndex = 1;

	public static readonly int s_SpatialBlendIndex = 2;

	public static readonly int s_DelayIndex = 3;

	public override void Run(bool loadSynchronously = false)
	{
		GetOverride(0).RegisterReadyListener(HandleReady);
	}

	private void HandleReady(object unused)
	{
		GetOverride(0).RemoveReadyOrInactiveListener(HandleReady);
		WeakAssetReference weakAsset = GetAssetAtIndex(s_SoundDefIndex);
		if (string.IsNullOrEmpty(weakAsset.AssetString))
		{
			Hearthstone.UI.Logging.Log.Get().AddMessage("Tried playing sound but sound def was missing.", this, LogLevel.Error);
			Complete(success: false);
			return;
		}
		GetOverride(0).Resolve(out var parentGO);
		SoundPlayClipArgs soundPlayArgs = new SoundPlayClipArgs
		{
			m_category = (Global.SoundCategory)GetIntValueAtIndex(s_SoundCategoryIndex),
			m_volume = GetFloatValueAtIndex(s_VolumeIndex),
			m_pitch = GetFloatValueAtIndex(s_PitchIndex),
			m_spatialBlend = GetFloatValueAtIndex(s_SpatialBlendIndex),
			m_parentObject = parentGO
		};
		bool result = false;
		if (SoundManager.Get() != null)
		{
			result = SoundManager.Get().LoadAndPlayClip(AssetReference.CreateFromAssetString(weakAsset.AssetString), soundPlayArgs);
		}
		Complete(result);
	}
}
