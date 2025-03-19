using System.Linq;

namespace Shared.UI.Scripts.Carousel;

public class MomentumHistory
{
	private int m_counter;

	private readonly float[] m_values;

	private int Capacity => m_values.Length;

	public MomentumHistory(int capacity)
	{
		m_values = new float[capacity];
	}

	public void Put(float value)
	{
		m_values[m_counter] = value;
		m_counter = (m_counter + 1) % Capacity;
	}

	public void Clear()
	{
		m_counter = 0;
		for (int index = 0; index < m_values.Length; index++)
		{
			m_values[index] = 0f;
		}
	}

	public float CalculateVelocity()
	{
		if (m_counter <= 0)
		{
			return 0f;
		}
		return m_values.Take(m_counter).Average();
	}
}
