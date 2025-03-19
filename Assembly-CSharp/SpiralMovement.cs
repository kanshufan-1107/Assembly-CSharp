using System;
using System.Collections.Generic;
using Blizzard.T5.MaterialService.Extensions;
using UnityEngine;

public class SpiralMovement : MonoBehaviour
{
	private enum States
	{
		START,
		UPDATE,
		HIDE
	}

	[Serializable]
	public class MaterialParam
	{
		[SerializeField]
		public string paramName;

		[SerializeField]
		public float valueMul = 1f;

		[SerializeField]
		public float timeMul = 1f;

		[SerializeField]
		public AnimationCurve curveAnimation;
	}

	private class Part
	{
		private enum PartStates
		{
			DELAY,
			UPDATE,
			END
		}

		private float lifetime;

		private float time;

		private Vector3 endPos;

		private Quaternion endRot = Quaternion.identity;

		private Vector3 endScale;

		private Vector3 pos;

		private Quaternion rot;

		private Vector3 scale;

		private float spawnPointRotSpeed;

		private GameObject go;

		private Transform goTransform;

		private MeshRenderer goMeshRenderer;

		private PartStates partState;

		private float delay;

		private float spawnPointIniRot;

		private float partSpawnPointOffset;

		private float currentAngle;

		private float spawnY;

		private AnimationCurve scaleAnimCurve;

		private AnimationCurve magnetTimeRemap;

		private AnimationCurve rotTimeRemap;

		public Part(GameObject _go, float _lifetime, float _spawnPointRotSpeed, float _spawnPointRot, float _partSpawnPointOffset, float _spawnY, Material _sharedMaterial, float _delay, AnimationCurve _magnetTimeRemap, AnimationCurve _rotTimeRemap, AnimationCurve _scaleAnimCurve)
		{
			lifetime = _lifetime;
			go = _go;
			goTransform = go.transform;
			goMeshRenderer = _go.GetComponent<MeshRenderer>();
			endPos = goTransform.localPosition;
			spawnPointIniRot = _spawnPointRot;
			partSpawnPointOffset = _partSpawnPointOffset;
			spawnPointRotSpeed = _spawnPointRotSpeed;
			goMeshRenderer.SetSharedMaterial(_sharedMaterial);
			delay = _delay;
			goMeshRenderer.enabled = false;
			currentAngle = 0f;
			spawnY = _spawnY;
			endScale = goTransform.localScale;
			scaleAnimCurve = _scaleAnimCurve;
			magnetTimeRemap = _magnetTimeRemap;
			rotTimeRemap = _rotTimeRemap;
		}

		public void Update(float _deltatime)
		{
			if (partState == PartStates.END)
			{
				return;
			}
			if (partState == PartStates.DELAY && time > delay)
			{
				time -= delay;
				goMeshRenderer.enabled = true;
				partState = PartStates.UPDATE;
			}
			if (partState == PartStates.UPDATE)
			{
				if (!(time < lifetime))
				{
					partState = PartStates.END;
					goTransform.localRotation = endRot;
					goTransform.localPosition = endPos;
					goTransform.localScale = endScale;
					return;
				}
				float normalizedTime = time / lifetime;
				float magnetTime = magnetTimeRemap.Evaluate(normalizedTime);
				float rotTime = rotTimeRemap.Evaluate(normalizedTime);
				currentAngle = spawnPointRotSpeed * rotTime + spawnPointIniRot;
				Vector3 pos = new Vector3(Mathf.Cos(currentAngle) * partSpawnPointOffset, spawnY, Mathf.Sin(currentAngle) * partSpawnPointOffset);
				pos = Vector3.Lerp(pos, endPos, magnetTime);
				goTransform.localPosition = pos;
				goTransform.localScale = endScale * scaleAnimCurve.Evaluate(magnetTime);
			}
			time += _deltatime;
		}

		public void Hide()
		{
			goMeshRenderer.enabled = false;
			partState = PartStates.END;
		}
	}

	public GameObject[] parts;

	public float partFlytimeMul = 1f;

	public AnimationCurve partFlytime;

	public float partSpawnPointCount = 3f;

	public float partSpawnPointOffset = 2f;

	public float partSpawnLifetime = 0.5f;

	public float partsHideTime = 2f;

	public float partSpawnPointRotSpeed = 40f;

	public float spawnY = 1f;

	public AnimationCurve magnetTimeRemap;

	public AnimationCurve partRotTimeRemap;

	public AnimationCurve partScaleAnimation;

	[SerializeField]
	public MaterialParam[] materialParams;

	private float time;

	private List<Part> partsP;

	private float partsCount;

	private Material partMaterial;

	private States state;

	public void ResetAnim()
	{
		partsCount = parts.Length;
		if (partsCount != 0f)
		{
			partMaterial = parts[0].GetComponent<MeshRenderer>().GetSharedMaterial();
			if (partsP != null)
			{
				partsP.Clear();
			}
			else
			{
				partsP = new List<Part>();
			}
			for (int i = 0; (float)i < partsCount; i++)
			{
				float partSpawnDelay = (float)i / partsCount * partSpawnLifetime;
				float spawnPointRot = Mathf.Floor(UnityEngine.Random.Range(0f, partSpawnPointCount - float.Epsilon)) * (float)Math.PI * 2f / partSpawnPointCount;
				Part part = new Part(parts[i], partFlytime.Evaluate((float)i / partsCount) * partFlytimeMul, partSpawnPointRotSpeed, spawnPointRot, partSpawnPointOffset, spawnY, partMaterial, partSpawnDelay, magnetTimeRemap, partRotTimeRemap, partScaleAnimation);
				partsP.Add(part);
			}
			time = 0f;
			state = States.UPDATE;
		}
	}

	private void Update()
	{
		if (state == States.HIDE || partsP == null)
		{
			return;
		}
		foreach (Part item in partsP)
		{
			item.Update(Time.deltaTime);
		}
		if (state == States.UPDATE)
		{
			if (!(time < partsHideTime))
			{
				state = States.HIDE;
				{
					foreach (Part item2 in partsP)
					{
						item2.Hide();
					}
					return;
				}
			}
			MaterialParam[] array = materialParams;
			foreach (MaterialParam matParam in array)
			{
				partMaterial.SetFloat(matParam.paramName, matParam.curveAnimation.Evaluate(time * matParam.timeMul) * matParam.valueMul);
			}
		}
		time += Time.deltaTime;
	}
}
