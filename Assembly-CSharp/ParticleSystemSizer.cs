using UnityEngine;

[ExecuteInEditMode]
public class ParticleSystemSizer : MonoBehaviour
{
	[Header("WARNING: DELETE THIS SCRIPT WHEN FINISHED!")]
	public float m_particleSystemScale = 1f;

	public bool m_scaleGameObject = true;

	[Header("Generated Values")]
	public Vector3 m_velocityModule;

	public float m_clampVelocityModuleMagnitude;

	public Vector3 m_clampVelocityModule;

	public Vector3 m_forceModule;

	public Vector2 m_colorBySpeedModuleRange;

	public Vector2 m_sizeBySpeedModuleRange;

	public Vector2 m_rotationBySpeedModuleRange;

	private float m_prevScale;

	private void Start()
	{
		Debug.LogError(base.gameObject.name + "::ParticleSystemSizer - This component should only ever be used when testing particle system size. You must delete any Particle System Sizers from the game object before submitting your work.");
	}
}
