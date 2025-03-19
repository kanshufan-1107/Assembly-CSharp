using System;
using System.Collections.Generic;

[Serializable]
public class MusicPlaylist
{
	[CustomEditField(ListSortable = true)]
	public MusicPlaylistType m_type;

	[CustomEditField(ListTable = true)]
	public List<MusicTrack> m_tracks = new List<MusicTrack>();

	public List<MusicTrack> GetMusicTracks()
	{
		return GetRandomizedTracks(m_tracks, MusicTrackType.Music);
	}

	public List<MusicTrack> GetAmbienceTracks()
	{
		return GetRandomizedTracks(m_tracks, MusicTrackType.Ambience);
	}

	private List<MusicTrack> GetRandomizedTracks(List<MusicTrack> trackList, MusicTrackType type)
	{
		List<MusicTrack> finalTracks = new List<MusicTrack>();
		List<MusicTrack> randomTracks = new List<MusicTrack>();
		foreach (MusicTrack track in trackList)
		{
			if (type == track.m_trackType && !string.IsNullOrEmpty(track.m_name))
			{
				if (track.m_shuffle)
				{
					randomTracks.Add(track.Clone());
				}
				else
				{
					finalTracks.Add(track.Clone());
				}
			}
		}
		Random rand = new Random();
		while (randomTracks.Count > 0)
		{
			int idx = rand.Next(0, randomTracks.Count);
			finalTracks.Add(randomTracks[idx]);
			randomTracks.RemoveAt(idx);
		}
		return finalTracks;
	}
}
