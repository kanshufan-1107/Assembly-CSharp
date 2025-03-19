using System.Collections.Generic;
using UnityEngine;

public class GamesWonIndicator : MonoBehaviour
{
	public enum InnKeeperTrigger
	{
		NONE
	}

	public GameObject m_root;

	public GameObject m_segmentContainer;

	public UberText m_winCountText;

	public GamesWonIndicatorSegment m_gamesWonSegmentPrefab;

	private List<GamesWonIndicatorSegment> m_segments = new List<GamesWonIndicatorSegment>();

	private int m_numActiveSegments;

	private InnKeeperTrigger m_innkeeperTrigger;

	private const float FUDGE_FACTOR = 0.01f;

	public void Init(Reward.Type rewardType, int rewardAmount, int numSegments, int numActiveSegments, InnKeeperTrigger trigger)
	{
		m_innkeeperTrigger = trigger;
		m_numActiveSegments = numActiveSegments;
		Vector3 lastSegmentWorldPos = m_segmentContainer.transform.position;
		float lastSegmentWidth = 0f;
		float totalWidth = 0f;
		for (int i = 0; i < numSegments; i++)
		{
			GamesWonIndicatorSegment.Type segmentType = ((i != 0) ? ((i != numSegments - 1) ? GamesWonIndicatorSegment.Type.MIDDLE : GamesWonIndicatorSegment.Type.RIGHT) : GamesWonIndicatorSegment.Type.LEFT);
			bool hideCrown = i >= numActiveSegments - 1;
			GamesWonIndicatorSegment segment = Object.Instantiate(m_gamesWonSegmentPrefab);
			segment.Init(segmentType, rewardType, rewardAmount, hideCrown);
			segment.transform.parent = m_segmentContainer.transform;
			segment.transform.localScale = Vector3.one;
			float width = segment.GetWidth() - 0.01f;
			if (segmentType != GamesWonIndicatorSegment.Type.RIGHT)
			{
				lastSegmentWorldPos.x += width;
			}
			else
			{
				lastSegmentWorldPos.x -= 0.01f;
			}
			segment.transform.position = lastSegmentWorldPos;
			segment.transform.rotation = Quaternion.identity;
			lastSegmentWidth = width;
			totalWidth += width;
			m_segments.Add(segment);
		}
		Vector3 segmentContainerWorldPos = m_segmentContainer.transform.position;
		segmentContainerWorldPos.x -= totalWidth / 2f;
		segmentContainerWorldPos.x += lastSegmentWidth / 5f;
		m_segmentContainer.transform.position = segmentContainerWorldPos;
		m_winCountText.Text = GameStrings.Format("GAMEPLAY_WIN_REWARD_PROGRESS", m_numActiveSegments, numSegments);
	}

	public void Show()
	{
		m_root.SetActive(value: true);
		if (m_numActiveSegments > 0)
		{
			if (m_numActiveSegments > m_segments.Count)
			{
				Debug.LogError($"GamesWonIndicator.Show(): cannot do animation; numActiveSegments = {m_numActiveSegments} but m_segments.Count = {m_segments.Count}");
				return;
			}
			m_segments[m_numActiveSegments - 1].AnimateReward();
			_ = m_innkeeperTrigger;
		}
	}

	public void Hide()
	{
		m_root.SetActive(value: false);
	}
}
