using UnityEngine;

public class Hero03ac_SpeakerAnimation : MonoBehaviour
{
	public Renderer BackgroundRenderer;

	public int MaterialIndex;

	public AnimationCurve AmplitudeCurve;

	public float Frequency;

	private MaterialPropertyBlock m_propertyBlock;

	private float m_timer;

	private static readonly int s_amplitudeID = Shader.PropertyToID("_Amplitude");

	private void Awake()
	{
		m_propertyBlock = new MaterialPropertyBlock();
		m_timer = 0f;
	}

	private void Update()
	{
		m_timer += Time.deltaTime * Frequency;
		m_timer -= Mathf.Floor(m_timer);
		if (BackgroundRenderer != null)
		{
			float offset = 0f;
			if (AmplitudeCurve != null)
			{
				offset = AmplitudeCurve.Evaluate(m_timer);
			}
			BackgroundRenderer.GetPropertyBlock(m_propertyBlock, MaterialIndex);
			m_propertyBlock.SetFloat(s_amplitudeID, offset);
			BackgroundRenderer.SetPropertyBlock(m_propertyBlock, MaterialIndex);
		}
	}
}
