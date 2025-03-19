using UnityEngine;

public class Hero_03ac_CoinGlintAnimation : MonoBehaviour
{
	public Transform CoinJoint;

	public Transform GlintObject;

	[Header("Coin dimensions")]
	public float Radius;

	public float Thickness;

	[Header("Lighting")]
	public Vector3 GlintDirection;

	[Min(0.01f)]
	public float VerticalThreshold = 0.1f;

	private void Awake()
	{
		if (GlintObject != null && CoinJoint != null)
		{
			GlintObject.SetParent(CoinJoint, worldPositionStays: false);
			GlintObject.localPosition = Vector3.zero;
			GlintObject.localRotation = Quaternion.identity;
			GlintObject.localScale = Vector3.one;
		}
		else
		{
			GlintObject.gameObject.SetActive(value: false);
		}
	}

	private void LateUpdate()
	{
		Vector3 glintDirectionLocal = GlintDirection;
		if (CoinJoint != null)
		{
			glintDirectionLocal = CoinJoint.InverseTransformDirection(GlintDirection);
		}
		if (GlintObject != null)
		{
			Vector3 projected = glintDirectionLocal;
			projected.y = 0f;
			projected.Normalize();
			float depth = Mathf.InverseLerp(0f - VerticalThreshold, VerticalThreshold, glintDirectionLocal.y);
			GlintObject.localPosition = projected * Radius + Vector3.up * Mathf.Lerp(0f - Thickness, Thickness, depth);
			GlintObject.rotation = Quaternion.identity;
		}
	}
}
