using System;
using System.Collections.Generic;
using Blizzard.T5.Jobs;
using Blizzard.T5.Services;
using Hearthstone;
using Hearthstone.Core;
using UnityEngine;

public class MusicManager : IService
{
	private MusicPlaylistType m_currentPlaylist;

	public MusicConfig Config { get; private set; }

	public IEnumerator<IAsyncJobResult> Initialize(ServiceLocator serviceLocator)
	{
		InstantiatePrefab loadMusicConfig = new InstantiatePrefab("MusicConfig.prefab:0af92217368c85f42ae37bec9a4e3625");
		yield return loadMusicConfig;
		Config = loadMusicConfig.InstantiatedPrefab.GetComponent<MusicConfig>();
		if (HearthstoneApplication.Get() != null)
		{
			HearthstoneApplication.Get().WillReset += WillReset;
		}
	}

	public Type[] GetDependencies()
	{
		return new Type[1] { typeof(IAssetLoader) };
	}

	public void Shutdown()
	{
	}

	public static MusicManager Get()
	{
		return ServiceManager.Get<MusicManager>();
	}

	public bool StartPlaylist(MusicPlaylistType type)
	{
		if (m_currentPlaylist == type)
		{
			return true;
		}
		if (!ServiceManager.TryGet<SoundManager>(out var sndMgr))
		{
			Debug.LogError("MusicManager.StartPlaylist() - SoundManager does not exist.");
			return false;
		}
		MusicPlaylist playlist = FindPlaylist(type);
		if (playlist == null)
		{
			Debug.LogWarning($"MusicManager.StartPlaylist() - failed to find playlist for type {type}");
			return false;
		}
		List<MusicTrack> newMusic = playlist.GetMusicTracks();
		List<MusicTrack> currMusic = sndMgr.GetCurrentMusicTracks();
		if (!AreTracksEqual(newMusic, currMusic))
		{
			sndMgr.NukeMusicAndStopPlayingCurrentTrack();
			if (newMusic != null && newMusic.Count > 0)
			{
				sndMgr.AddMusicTracks(newMusic);
			}
		}
		List<MusicTrack> newAmbience = playlist.GetAmbienceTracks();
		List<MusicTrack> currAmbience = sndMgr.GetCurrentAmbienceTracks();
		if (!AreTracksEqual(newAmbience, currAmbience))
		{
			sndMgr.NukeAmbienceAndStopPlayingCurrentTrack();
			if (newAmbience != null && newAmbience.Count > 0)
			{
				sndMgr.AddAmbienceTracks(newAmbience);
			}
		}
		m_currentPlaylist = playlist.m_type;
		return true;
	}

	public bool StopPlaylist()
	{
		SoundManager sndMgr = SoundManager.Get();
		if (sndMgr == null)
		{
			Debug.LogError("MusicManager.StopPlaylist() - SoundManager does not exist.");
			return false;
		}
		if (m_currentPlaylist == MusicPlaylistType.Invalid)
		{
			return false;
		}
		m_currentPlaylist = MusicPlaylistType.Invalid;
		sndMgr.NukePlaylistsAndStopPlayingCurrentTracks();
		return true;
	}

	public MusicPlaylistBookmark CreateBookmarkOfCurrentPlaylist()
	{
		SoundManager sndMgr = SoundManager.Get();
		if (sndMgr == null)
		{
			Debug.LogError("MusicManager.CreateBookmarkOfCurrentPlaylist() - SoundManager does not exist.");
			return new MusicPlaylistBookmark();
		}
		MusicPlaylistBookmark bookmark = new MusicPlaylistBookmark();
		bookmark.m_playListType = m_currentPlaylist;
		bookmark.m_playListIndex = sndMgr.GetCurrentMusicTrackIndex();
		bookmark.m_timeStamp = Time.unscaledTime;
		AudioSource currentTrack = sndMgr.GetCurrentMusicTrack();
		if ((bool)currentTrack)
		{
			bookmark.m_trackTime = currentTrack.time;
			bookmark.m_currentTrack = currentTrack;
		}
		return bookmark;
	}

	public bool PlayFromBookmark(MusicPlaylistBookmark bookmark)
	{
		if (bookmark == null || bookmark.m_playListType == MusicPlaylistType.Invalid)
		{
			return false;
		}
		SoundManager sndMgr = SoundManager.Get();
		if (sndMgr == null)
		{
			Debug.LogError("MusicManager.PlayFromBookmark() - SoundManager does not exist.");
			return false;
		}
		Action syncMusic = null;
		syncMusic = delegate
		{
			SoundManager soundManager = sndMgr;
			soundManager.OnMusicStarted = (Action)Delegate.Remove(soundManager.OnMusicStarted, syncMusic);
			if (m_currentPlaylist == bookmark.m_playListType && sndMgr.GetCurrentMusicTrackIndex() == bookmark.m_playListIndex)
			{
				if (bookmark.m_currentTrack != null)
				{
					sndMgr.SetCurrentMusicTrackTime(bookmark.m_currentTrack.time);
				}
				else
				{
					sndMgr.SetCurrentMusicTrackTime(bookmark.m_trackTime);
				}
			}
		};
		SoundManager soundManager2 = sndMgr;
		soundManager2.OnMusicStarted = (Action)Delegate.Combine(soundManager2.OnMusicStarted, syncMusic);
		StartPlaylist(bookmark.m_playListType);
		sndMgr.SetCurrentMusicTrackIndex(bookmark.m_playListIndex);
		return true;
	}

	public MusicPlaylistType GetCurrentPlaylist()
	{
		return m_currentPlaylist;
	}

	private void WillReset()
	{
		SoundManager sndMgr = SoundManager.Get();
		if (sndMgr == null)
		{
			Debug.LogError("MusicManager.WillReset() - SoundManager does not exist.");
			return;
		}
		m_currentPlaylist = MusicPlaylistType.Invalid;
		sndMgr.ImmediatelyKillMusicAndAmbience();
	}

	private MusicPlaylist FindPlaylist(MusicPlaylistType type)
	{
		if (Config == null)
		{
			Debug.LogError("MusicManager.FindPlaylist() - MusicConfig does not exist.");
			return null;
		}
		MusicPlaylist playlist = Config.FindPlaylist(type);
		if (playlist == null)
		{
			Debug.LogWarning($"MusicManager.FindPlaylist() - {type} playlist is not defined.");
			return null;
		}
		return playlist;
	}

	private bool AreTracksEqual(List<MusicTrack> newTracks, List<MusicTrack> curTracks)
	{
		if (newTracks.Count != curTracks.Count)
		{
			return false;
		}
		foreach (MusicTrack newT in newTracks)
		{
			if (curTracks.Find(delegate(MusicTrack curT)
			{
				string obj = (AssetLoader.Get().IsAssetAvailable(curT.m_name) ? curT.m_name : curT.m_fallback);
				string text = (AssetLoader.Get().IsAssetAvailable(newT.m_name) ? newT.m_name : newT.m_fallback);
				return obj == text;
			}) == null)
			{
				return false;
			}
		}
		return true;
	}
}
