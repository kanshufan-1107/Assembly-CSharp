using System;
using UnityEngine;

namespace Hearthstone.Timeline;

[Serializable]
public class AnimatedVector4 : AnimatedVector
{
	public Vector4 value;

	public Vector4 valueB;

	public override object Get(float normalizedTime, float randomValue)
	{
		Vector4 vector = default(Vector4);
		switch (type)
		{
		case Type.Vector:
			vector = value;
			break;
		case Type.Curve:
			if (curve.Length != 0)
			{
				vector.x = curve[0].Evaluate(normalizedTime);
			}
			if (curve.Length > 1)
			{
				vector.y = curve[1].Evaluate(normalizedTime);
			}
			if (curve.Length > 2)
			{
				vector.z = curve[2].Evaluate(normalizedTime);
			}
			if (curve.Length > 3)
			{
				vector.w = curve[3].Evaluate(normalizedTime);
			}
			break;
		case Type.RandomBetweenTwoVectors:
			vector = Vector4.Lerp(value, valueB, randomValue);
			break;
		default:
			if (curve.Length != 0 && curveB.Length != 0)
			{
				vector.x = Mathf.Lerp(curve[0].Evaluate(normalizedTime), curveB[0].Evaluate(normalizedTime), randomValue);
			}
			if (curve.Length > 1 && curveB.Length > 1)
			{
				vector.y = Mathf.Lerp(curve[1].Evaluate(normalizedTime), curveB[1].Evaluate(normalizedTime), randomValue);
			}
			if (curve.Length > 2 && curveB.Length > 2)
			{
				vector.z = Mathf.Lerp(curve[2].Evaluate(normalizedTime), curveB[2].Evaluate(normalizedTime), randomValue);
			}
			if (curve.Length > 3 && curveB.Length > 3)
			{
				vector.w = Mathf.Lerp(curve[3].Evaluate(normalizedTime), curveB[3].Evaluate(normalizedTime), randomValue);
			}
			break;
		}
		return vector;
	}
}
