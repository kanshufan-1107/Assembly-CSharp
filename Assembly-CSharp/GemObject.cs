using UnityEngine;

public class GemObject : MonoBehaviour
{
	public Vector3 startingScale;

	public float jiggleAmount;

	public GameObject m_gemMesh;

	public UberText m_gemText;

	private bool initialized;

	private bool m_hiddenFlag;

	private void Awake()
	{
		startingScale = base.transform.localScale;
	}

	public void Enlarge(float scaleFactor)
	{
		iTween.Stop(base.gameObject);
		iTween.ScaleTo(base.gameObject, iTween.Hash("scale", new Vector3(startingScale.x * scaleFactor, startingScale.y * scaleFactor, startingScale.z * scaleFactor), "time", 0.5f, "easetype", iTween.EaseType.easeOutElastic));
	}

	public void Shrink()
	{
		iTween.ScaleTo(base.gameObject, startingScale, 0.5f);
	}

	public void ImmediatelyScaleToZero()
	{
		base.transform.localScale = Vector3.zero;
	}

	public void ScaleToZero()
	{
		iTween.Stop(base.gameObject);
		iTween.ScaleTo(base.gameObject, Vector3.zero, 0.5f);
	}

	public void SetToZeroThenEnlarge()
	{
		base.transform.localScale = Vector3.zero;
		Enlarge(1f);
	}

	public void Hide()
	{
		base.gameObject.SetActive(value: false);
	}

	public void Show()
	{
		base.gameObject.SetActive(value: true);
	}

	public void Initialize()
	{
		initialized = true;
	}

	public void SetHideNumberFlag(bool enable)
	{
		m_hiddenFlag = enable;
	}

	public bool IsNumberHidden()
	{
		return m_hiddenFlag;
	}

	public void Jiggle()
	{
		if (!initialized)
		{
			initialized = true;
			return;
		}
		iTween.Stop(base.gameObject);
		Vector3 amount = new Vector3(jiggleAmount, jiggleAmount, jiggleAmount);
		float time = 1f;
		iTween.PunchScale(base.gameObject, iTween.Hash("amount", amount, "time", time, "onstart", "ResetScale"));
	}

	public void ResetScale()
	{
		base.transform.localScale = startingScale;
	}

	public void SetMeshActive(bool active)
	{
		if (!(m_gemMesh == null))
		{
			m_gemMesh.SetActive(active);
		}
	}

	public void SetTextActive(bool active)
	{
		if (!(m_gemText == null))
		{
			m_gemText.gameObject.SetActive(active);
		}
	}
}
