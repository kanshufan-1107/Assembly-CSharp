using System.Collections;

namespace Blizzard.T5.Game.Cutscene;

public abstract class TimelineAction
{
	protected bool m_isReady;

	protected bool m_hasFinishedPlaying;

	protected string m_actionCaptionLocString;

	protected float? m_actionStartDelayOverrideSec;

	protected float? m_actionEndDelayOverrideSec;

	protected float? m_actionTimeoutOverrideSec;

	public bool ResetAfterPlay { get; set; }

	public bool IsReady => m_isReady;

	public string CaptionLocalizedString
	{
		get
		{
			return m_actionCaptionLocString;
		}
		set
		{
			m_actionCaptionLocString = value;
		}
	}

	public float? StartDelayOverrideSeconds
	{
		get
		{
			return m_actionStartDelayOverrideSec;
		}
		set
		{
			m_actionStartDelayOverrideSec = value;
		}
	}

	public float? EndDelayOverrideSeconds
	{
		get
		{
			return m_actionEndDelayOverrideSec;
		}
		set
		{
			m_actionEndDelayOverrideSec = value;
		}
	}

	public float? TimeoutOverrideSeconds
	{
		get
		{
			return m_actionTimeoutOverrideSec;
		}
		set
		{
			m_actionTimeoutOverrideSec = value;
		}
	}

	protected bool HasFinished => m_hasFinishedPlaying;

	public abstract void Init();

	public abstract IEnumerator Play();

	public abstract void Stop();

	public abstract void Dispose();

	public abstract void Reset();
}
