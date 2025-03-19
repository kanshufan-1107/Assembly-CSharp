using System;
using System.Collections.Generic;
using Blizzard.T5.Jobs;
using Blizzard.T5.Services;
using UnityEngine;

public class ShaderTime : IService, IHasUpdate
{
	private float m_maxTime = 65535f;

	private float m_time;

	private bool m_enabled = true;

	public IEnumerator<IAsyncJobResult> Initialize(ServiceLocator serviceLocator)
	{
		yield break;
	}

	public Type[] GetDependencies()
	{
		return new Type[1] { typeof(IGraphicsManager) };
	}

	public void Shutdown()
	{
		Shader.SetGlobalFloat("_ShaderTime", 0f);
	}

	public void Update()
	{
		UpdateShaderAnimationTime();
		UpdateGyro();
	}

	private void UpdateShaderAnimationTime()
	{
		if (!m_enabled)
		{
			m_time = 1f;
		}
		else
		{
			m_time += Time.deltaTime / 20f;
			if (m_time > m_maxTime)
			{
				m_time -= m_maxTime;
				if (m_time <= 0f)
				{
					m_time = 0.0001f;
				}
			}
		}
		Shader.SetGlobalFloat("_ShaderTime", m_time);
	}

	private void UpdateGyro()
	{
		Vector4 gyro = Input.gyro.gravity;
		Shader.SetGlobalVector("_Gyroscope", gyro);
	}
}
