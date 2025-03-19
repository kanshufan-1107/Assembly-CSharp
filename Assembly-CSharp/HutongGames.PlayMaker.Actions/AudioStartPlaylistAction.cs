using Hearthstone;

namespace HutongGames.PlayMaker.Actions;

[ActionCategory("Pegasus Audio")]
[Tooltip("Start playing the specified music playlist.")]
public class AudioStartPlaylistAction : FsmStateAction
{
	[RequiredField]
	[Tooltip("The playlist you want to start playing.")]
	public MusicPlaylistType m_Playlist;

	public override void Reset()
	{
	}

	public override void OnEnter()
	{
		if (MusicManager.Get() == null)
		{
			if (HearthstoneApplication.Get() != null)
			{
				Error.AddDevWarning("No Music Manager", "Playmaker attempted to play an audio playlist when the music manager isn't instantiated.");
			}
		}
		else
		{
			MusicManager.Get().StartPlaylist(m_Playlist);
		}
		Finish();
	}
}
