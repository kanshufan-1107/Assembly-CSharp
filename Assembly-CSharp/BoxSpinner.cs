using Blizzard.T5.MaterialService.Extensions;
using UnityEngine;

public class BoxSpinner : MonoBehaviour
{
	private Box m_parent;

	private BoxSpinnerStateInfo m_info;

	private bool m_spinning;

	private float m_spinY;

	private Material m_spinnerMat;

	private void Awake()
	{
		MaterialChanged();
	}

	private void Update()
	{
		if (IsSpinning() && !(m_spinnerMat == null) && m_info != null)
		{
			m_spinnerMat.SetFloat("_RotAngle", m_spinY);
			m_spinY += m_info.m_DegreesPerSec * Time.deltaTime * 0.01f;
		}
	}

	private void OnDestroy()
	{
		Object.Destroy(m_spinnerMat);
		m_spinnerMat = null;
		m_parent = null;
		m_info = null;
	}

	public Box GetParent()
	{
		return m_parent;
	}

	public void SetParent(Box parent)
	{
		m_parent = parent;
	}

	public BoxSpinnerStateInfo GetInfo()
	{
		return m_info;
	}

	public void SetInfo(BoxSpinnerStateInfo info)
	{
		m_info = info;
	}

	public void Spin()
	{
		m_spinning = true;
	}

	public bool IsSpinning()
	{
		return m_spinning;
	}

	public void Stop()
	{
		m_spinning = false;
	}

	public void Reset()
	{
		m_spinning = false;
		m_spinnerMat?.SetFloat("_RotAngle", 0f);
	}

	public void MaterialChanged()
	{
		m_spinnerMat = GetComponent<Renderer>().GetMaterial();
	}
}
