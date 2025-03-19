using Blizzard.T5.MaterialService.Extensions;
using UnityEngine;

public class FlameController : MonoBehaviour
{
	[Header("Renderers")]
	public Renderer LeftEye;

	public Renderer RightEye;

	[Header("Scroll Controls")]
	public float ScrollingSpeedMultiplier = 1f;

	[Header("Offsets")]
	public Vector2 LeftEyeOffset;

	public Vector2 RightEyeOffset;

	private MaterialPropertyBlock m_propertyBlock;

	private Vector2 m_scrollingSpeed;

	private Vector2 m_offset;

	private static readonly int s_secondTexStID = Shader.PropertyToID("_SecondTex_ST");

	private static readonly int s_secondTexSpeed_NoiseForceID = Shader.PropertyToID("_SecondTexSpeed_NoiseForce");

	public void Awake()
	{
		m_propertyBlock = new MaterialPropertyBlock();
		Renderer renderer = LeftEye;
		if (renderer == null)
		{
			renderer = RightEye;
		}
		if (renderer != null)
		{
			Material leftEyeMaterial = renderer.GetSharedMaterial();
			if (leftEyeMaterial != null)
			{
				Vector4 secondTexSpeed_NoiseForce = leftEyeMaterial.GetVector(s_secondTexSpeed_NoiseForceID);
				m_scrollingSpeed = new Vector2(secondTexSpeed_NoiseForce.x, secondTexSpeed_NoiseForce.y);
			}
		}
	}

	private void Update()
	{
		m_offset += m_scrollingSpeed * (ScrollingSpeedMultiplier * Time.deltaTime);
		m_offset.x %= 1f;
		m_offset.y %= 1f;
		if (LeftEye != null)
		{
			LeftEye.GetPropertyBlock(m_propertyBlock);
			UpdateBlock(LeftEye.GetSharedMaterial(), LeftEyeOffset + m_offset);
			LeftEye.SetPropertyBlock(m_propertyBlock);
		}
		if (RightEye != null)
		{
			RightEye.GetPropertyBlock(m_propertyBlock);
			UpdateBlock(RightEye.GetSharedMaterial(), RightEyeOffset + m_offset);
			RightEye.SetPropertyBlock(m_propertyBlock);
		}
	}

	private void UpdateBlock(Material sourceMaterial, Vector2 offset)
	{
		if (m_propertyBlock != null)
		{
			Vector4 secondTexST = GetVector(s_secondTexStID);
			secondTexST.z = offset.x;
			secondTexST.w = offset.y;
			m_propertyBlock.SetVector(s_secondTexStID, secondTexST);
			Vector4 secondTexSpeed_NoiseForceID = GetVector(s_secondTexSpeed_NoiseForceID);
			secondTexSpeed_NoiseForceID.x = 0f;
			secondTexSpeed_NoiseForceID.y = 0f;
			m_propertyBlock.SetVector(s_secondTexSpeed_NoiseForceID, secondTexSpeed_NoiseForceID);
		}
		Vector4 GetVector(int materialID)
		{
			Vector4 value = sourceMaterial.GetVector(materialID);
			if (m_propertyBlock.HasVector(materialID))
			{
				value = m_propertyBlock.GetVector(materialID);
			}
			return value;
		}
	}
}
