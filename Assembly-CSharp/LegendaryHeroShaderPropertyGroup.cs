using System;
using System.Collections.Generic;
using UnityEngine;

public class LegendaryHeroShaderPropertyGroup : MonoBehaviour
{
	public enum Assignment
	{
		Amount1,
		Amount2,
		Amount3,
		Amount4,
		Amount5
	}

	public abstract class Property<T>
	{
		public bool ApplyOnActivation;

		public bool StartIsMaterialValue = true;

		public T StartValue;

		public bool StopIsMaterialValue;

		public T StopValue;

		public string PropertyName;

		public Assignment Assignment;

		protected int PropertyID => Shader.PropertyToID(PropertyName);

		public abstract void SetBlendedProperty(float blendAmount, Material mat, MaterialPropertyBlock block);
	}

	[Serializable]
	public class FloatProperty : Property<float>
	{
		public override void SetBlendedProperty(float blendAmount, Material mat, MaterialPropertyBlock block)
		{
			float blendedAmount = Mathf.Lerp(StartIsMaterialValue ? mat.GetFloat(base.PropertyID) : StartValue, StopIsMaterialValue ? mat.GetFloat(base.PropertyID) : StopValue, Mathf.Clamp01(blendAmount));
			block.SetFloat(base.PropertyID, blendedAmount);
		}
	}

	[Serializable]
	public class ColorProperty : Property<Color>
	{
		public override void SetBlendedProperty(float blendAmount, Material mat, MaterialPropertyBlock block)
		{
			Color blendedAmount = Color.Lerp(StartIsMaterialValue ? mat.GetColor(base.PropertyID) : StartValue, StopIsMaterialValue ? mat.GetColor(base.PropertyID) : StopValue, blendAmount);
			block.SetColor(base.PropertyID, blendedAmount);
		}
	}

	[Serializable]
	public class VectorProperty : Property<Vector4>
	{
		public override void SetBlendedProperty(float blendAmount, Material mat, MaterialPropertyBlock block)
		{
			Vector4 blendedAmount = Vector4.Lerp(StartIsMaterialValue ? mat.GetVector(base.PropertyID) : StartValue, StopIsMaterialValue ? mat.GetVector(base.PropertyID) : StopValue, blendAmount);
			block.SetVector(base.PropertyID, blendedAmount);
		}
	}

	[Serializable]
	public class PropertyList
	{
		public List<FloatProperty> FloatProperties;

		public List<ColorProperty> ColorProperties;

		public List<VectorProperty> VectorProperties;
	}

	public PropertyList Properties = new PropertyList();

	[Range(0f, 1f)]
	public float Amount;

	[Range(0f, 1f)]
	public float Amount2;

	[Range(0f, 1f)]
	public float Amount3;

	[Range(0f, 1f)]
	public float Amount4;

	[Range(0f, 1f)]
	public float Amount5;

	private float m_previousAmount;

	private float m_previousAmount2;

	private float m_previousAmount3;

	private float m_previousAmount4;

	private float m_previousAmount5;

	private bool m_active;

	private bool m_wasActive;

	public void SetProperties(Material mat, MaterialPropertyBlock propertyBlock, bool isEditMode)
	{
		if (!m_active)
		{
			m_wasActive = false;
			return;
		}
		bool becameActive = !isEditMode && !m_wasActive;
		foreach (FloatProperty property in Properties.FloatProperties)
		{
			if (ShouldUpdateProperty(property, out var amount) || becameActive)
			{
				property.SetBlendedProperty(amount, mat, propertyBlock);
			}
		}
		foreach (ColorProperty property2 in Properties.ColorProperties)
		{
			if (ShouldUpdateProperty(property2, out var amount2) || becameActive)
			{
				property2.SetBlendedProperty(amount2, mat, propertyBlock);
			}
		}
		foreach (VectorProperty property3 in Properties.VectorProperties)
		{
			if (ShouldUpdateProperty(property3, out var amount3) || becameActive)
			{
				property3.SetBlendedProperty(amount3, mat, propertyBlock);
			}
		}
		UpdatePreviousAmounts();
		m_active = false;
		m_wasActive = true;
	}

	public void Reset()
	{
		Amount = 0f;
		Amount2 = 0f;
		Amount3 = 0f;
		Amount4 = 0f;
		Amount5 = 0f;
		m_previousAmount = 0f;
		m_previousAmount2 = 0f;
		m_previousAmount3 = 0f;
		m_previousAmount4 = 0f;
		m_previousAmount5 = 0f;
		m_active = false;
		m_wasActive = false;
	}

	private bool ShouldUpdateProperty<T>(Property<T> property, out float amount)
	{
		amount = GetAmountForAssignment(property.Assignment);
		if (!m_wasActive && property.ApplyOnActivation)
		{
			return true;
		}
		float previousAmount = GetPreviousAmountForAssignment(property.Assignment);
		return !Mathf.Approximately(amount, previousAmount);
	}

	private float GetAmountForAssignment(Assignment assignment)
	{
		return assignment switch
		{
			Assignment.Amount1 => Amount, 
			Assignment.Amount2 => Amount2, 
			Assignment.Amount3 => Amount3, 
			Assignment.Amount4 => Amount4, 
			Assignment.Amount5 => Amount5, 
			_ => 0f, 
		};
	}

	private float GetPreviousAmountForAssignment(Assignment assignment)
	{
		return assignment switch
		{
			Assignment.Amount1 => m_previousAmount, 
			Assignment.Amount2 => m_previousAmount2, 
			Assignment.Amount3 => m_previousAmount3, 
			Assignment.Amount4 => m_previousAmount4, 
			Assignment.Amount5 => m_previousAmount5, 
			_ => 0f, 
		};
	}

	private void UpdatePreviousAmounts()
	{
		m_previousAmount = Amount;
		m_previousAmount2 = Amount2;
		m_previousAmount3 = Amount3;
		m_previousAmount4 = Amount4;
		m_previousAmount5 = Amount5;
	}

	private void OnDidApplyAnimationProperties()
	{
		m_active = true;
	}
}
