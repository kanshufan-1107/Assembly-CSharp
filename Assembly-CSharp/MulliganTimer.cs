using UnityEngine;

public class MulliganTimer : MonoBehaviour
{
	public UberText m_timeText;

	public static float m_baconRerollButtonCutOffSecond = 5f;

	private bool m_remainingTimeSet;

	private float m_endTimeStamp;

	private void Start()
	{
		if (!(MulliganManager.Get() == null))
		{
			base.transform.position = GetNewPosition();
		}
	}

	private void Update()
	{
		if (!m_remainingTimeSet)
		{
			return;
		}
		Vector3 newPosition = GetNewPosition();
		if (newPosition != base.transform.position)
		{
			base.transform.position = newPosition;
		}
		float num = ComputeCountdownRemainingSec();
		int secondsRemaining = Mathf.RoundToInt(num);
		if (secondsRemaining < 0)
		{
			secondsRemaining = 0;
		}
		m_timeText.Text = $":{secondsRemaining:D2}";
		if (num <= m_baconRerollButtonCutOffSecond && MulliganManager.Get() != null)
		{
			MulliganManager.Get().OnMulliganTimerExceedBaconRerollButtonCutoff();
		}
		if (!(num > 0f))
		{
			if ((bool)MulliganManager.Get())
			{
				MulliganManager.Get().AutomaticContinueMulligan();
			}
			else
			{
				SelfDestruct();
			}
		}
	}

	private Vector3 GetNewPosition()
	{
		if (MulliganManager.Get() == null)
		{
			return new Vector3(100f, 0f, 0f);
		}
		Vector3 mulliganButtonPosition = MulliganManager.Get().GetMulliganTimerPosition();
		return (!UniversalInputManager.UsePhoneUI) ? new Vector3(mulliganButtonPosition.x, mulliganButtonPosition.y, mulliganButtonPosition.z - 1f) : new Vector3(mulliganButtonPosition.x + 1.8f, mulliganButtonPosition.y, mulliganButtonPosition.z);
	}

	public float ComputeCountdownRemainingSec()
	{
		float countdownRemainingSec = m_endTimeStamp - Time.realtimeSinceStartup;
		if (countdownRemainingSec < 0f)
		{
			return 0f;
		}
		return countdownRemainingSec;
	}

	public void SetEndTime(float endTimeStamp)
	{
		m_endTimeStamp = endTimeStamp;
		m_remainingTimeSet = true;
	}

	public void SelfDestruct()
	{
		Object.Destroy(base.gameObject);
	}
}
