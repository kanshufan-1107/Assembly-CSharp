using System;
using System.Collections;
using System.Collections.Generic;
using Blizzard.T5.MaterialService.Extensions;
using UnityEngine;

public class ElectroScript : MonoBehaviour
{
	[Serializable]
	public class Prefabs
	{
		public LineRenderer lightning;

		public LineRenderer branch;

		public Transform sparks;

		public Transform source;

		public Transform destination;

		public Transform target;
	}

	[Serializable]
	public class Timers
	{
		public float timeToUpdate = 0.05f;

		public float timeToPowerUp = 0.5f;

		public float branchLife = 0.1f;
	}

	[Serializable]
	public class Dynamics
	{
		public float chanceToArc = 0.2f;
	}

	[Serializable]
	public class LineSettings
	{
		public float keyVertexDist = 3f;

		public float keyVertexRange = 4f;

		public int numInterpoles = 5;

		public float minBranchLength = 11f;

		public float maxBranchLength = 16f;
	}

	[Serializable]
	public class TextureSettings
	{
		public float scaleX;

		public float scaleY;

		public float animateSpeed;

		public float offsetY;
	}

	public Prefabs prefabs;

	public Timers timers;

	public Dynamics dynamics;

	public LineSettings lines;

	public TextureSettings tex;

	private int numVertices;

	private Vector3 deltaV1;

	private Vector3 deltaV2;

	private float srcTrgDist;

	private float srcDstDist;

	private float lastUpdate;

	private Dictionary<int, LineRenderer> branches;

	private void Start()
	{
		srcTrgDist = 0f;
		srcDstDist = 0f;
		numVertices = 0;
		deltaV1 = prefabs.destination.position - prefabs.source.position;
		lastUpdate = 0f;
		branches = new Dictionary<int, LineRenderer>();
	}

	private void Update()
	{
		srcTrgDist = Vector3.Distance(prefabs.source.position, prefabs.target.position);
		srcDstDist = Vector3.Distance(prefabs.source.position, prefabs.destination.position);
		if (branches.Count > 0)
		{
			foreach (int key in (IEnumerable)new Dictionary<int, LineRenderer>(branches).Keys)
			{
				LineRenderer branch = branches[key];
				if (branch.GetComponent<BranchScript>().timeSpawned + timers.branchLife < Time.time)
				{
					branches.Remove(key);
					UnityEngine.Object.Destroy(branch.gameObject);
				}
			}
		}
		if (prefabs.target.localPosition != prefabs.destination.localPosition)
		{
			if (Vector3.Distance(Vector3.zero, deltaV1) * Time.deltaTime * (1f / timers.timeToPowerUp) > Vector3.Distance(prefabs.target.position, prefabs.destination.position))
			{
				prefabs.target.position = prefabs.destination.position;
			}
			else
			{
				prefabs.target.Translate(deltaV1 * Time.deltaTime * (1f / timers.timeToPowerUp));
			}
		}
		if (!(Time.time - lastUpdate < timers.timeToUpdate))
		{
			lastUpdate = Time.time;
			AnimateArc();
			DrawArc();
			RayCast();
		}
	}

	private void DrawArc()
	{
		numVertices = Mathf.RoundToInt(srcTrgDist / lines.keyVertexDist) * (1 + lines.numInterpoles) + 1;
		deltaV2 = (prefabs.target.localPosition - prefabs.source.localPosition) / numVertices;
		Vector3 currentV = prefabs.source.localPosition;
		Vector3[] keyVertices = new Vector3[numVertices];
		prefabs.lightning.positionCount = numVertices;
		int i;
		Vector3 brTmpV2 = default(Vector3);
		Vector3 brTmpV4 = default(Vector3);
		for (i = 0; i < numVertices; i++)
		{
			Vector3 tmpV = currentV;
			tmpV.y += (UnityEngine.Random.value * 2f - 1f) * lines.keyVertexRange;
			tmpV.z += (UnityEngine.Random.value * 2f - 1f) * lines.keyVertexRange;
			prefabs.lightning.SetPosition(i, tmpV);
			keyVertices[i] = tmpV;
			if (!branches.ContainsKey(i))
			{
				if (UnityEngine.Random.value < dynamics.chanceToArc)
				{
					LineRenderer tmpLR = UnityEngine.Object.Instantiate(prefabs.branch, Vector3.zero, Quaternion.identity);
					tmpLR.GetComponent<BranchScript>().timeSpawned = Time.time;
					tmpLR.transform.parent = prefabs.lightning.transform;
					branches.Add(i, tmpLR);
					tmpLR.transform.position = prefabs.lightning.transform.TransformPoint(tmpV);
					tmpV.x = UnityEngine.Random.value - 0.5f;
					tmpV.y = (UnityEngine.Random.value - 0.5f) * 2f;
					tmpV.z = (UnityEngine.Random.value - 0.5f) * 2f;
					tmpLR.transform.LookAt(tmpLR.transform.TransformPoint(tmpV));
					tmpLR.transform.Find("stop").localPosition = tmpLR.transform.Find("start").localPosition + new Vector3(0f, 0f, UnityEngine.Random.Range(lines.minBranchLength, lines.maxBranchLength));
					int brNumVertices = Mathf.RoundToInt(Vector3.Distance(tmpLR.transform.Find("start").position, tmpLR.transform.Find("stop").position) / lines.keyVertexDist) * (1 + lines.numInterpoles) + 1;
					Vector3 brDeltaV = (tmpLR.transform.Find("stop").localPosition - tmpLR.transform.Find("start").localPosition) / brNumVertices;
					Vector3 brCurrentV = tmpLR.transform.Find("start").localPosition;
					Vector3[] brKeyVertices = new Vector3[brNumVertices];
					tmpLR.positionCount = brNumVertices;
					int j;
					for (j = 0; j < brNumVertices; j++)
					{
						Vector3 brTmpV = brCurrentV;
						brTmpV.x += (UnityEngine.Random.value * 2f - 1f) * lines.keyVertexRange;
						brTmpV.y += (UnityEngine.Random.value * 2f - 1f) * lines.keyVertexRange;
						tmpLR.SetPosition(j, brTmpV);
						brKeyVertices[j] = brTmpV;
						brCurrentV += brDeltaV * (lines.numInterpoles + 1);
						j += lines.numInterpoles;
					}
					tmpLR.SetPosition(0, tmpLR.transform.Find("start").localPosition);
					tmpLR.SetPosition(brNumVertices - 1, tmpLR.transform.Find("stop").localPosition);
					for (int k = 0; k < brNumVertices; k++)
					{
						if (k % (lines.numInterpoles + 1) != 0)
						{
							Vector3 tmp1 = brKeyVertices[k - 1];
							Vector3 tmp2 = brKeyVertices[k + lines.numInterpoles];
							float x = Vector3.Distance(tmp1, tmp2) / (float)(lines.numInterpoles + 1) / Vector3.Distance(tmp1, tmp2) * (float)Math.PI;
							for (int l = 0; l < lines.numInterpoles; l++)
							{
								brTmpV2.x = tmp1.x + brDeltaV.x * (float)(1 + l);
								brTmpV2.y = tmp1.y + (Mathf.Sin(x - (float)Math.PI / 2f) / 2f + 0.5f) * (tmp2.y - tmp1.y);
								brTmpV2.z = tmp1.z + (Mathf.Sin(x - (float)Math.PI / 2f) / 2f + 0.5f) * (tmp2.z - tmp1.z);
								tmpLR.SetPosition(k + l, brTmpV2);
								x += x;
							}
							k += lines.numInterpoles;
						}
					}
				}
			}
			else
			{
				LineRenderer branch = branches[i];
				int brNumVertices2 = Mathf.RoundToInt(Vector3.Distance(branch.transform.Find("start").position, branch.transform.Find("stop").position) / lines.keyVertexDist) * (1 + lines.numInterpoles) + 1;
				Vector3 brDeltaV2 = (branch.transform.Find("stop").localPosition - branch.transform.Find("start").localPosition) / brNumVertices2;
				Vector3 brCurrentV2 = branch.transform.Find("start").localPosition;
				Vector3[] brKeyVertices2 = new Vector3[brNumVertices2];
				branch.positionCount = brNumVertices2;
				branch.SetPosition(0, branch.transform.Find("start").localPosition);
				int j2;
				for (j2 = 0; j2 < brNumVertices2; j2++)
				{
					Vector3 brTmpV3 = brCurrentV2;
					brTmpV3.x += (UnityEngine.Random.value * 2f - 1f) * lines.keyVertexRange;
					brTmpV3.y += (UnityEngine.Random.value * 2f - 1f) * lines.keyVertexRange;
					branch.SetPosition(j2, brTmpV3);
					brKeyVertices2[j2] = brTmpV3;
					brCurrentV2 += brDeltaV2 * (lines.numInterpoles + 1);
					j2 += lines.numInterpoles;
				}
				branch.SetPosition(0, branch.transform.Find("start").localPosition);
				branch.SetPosition(brNumVertices2 - 1, branch.transform.Find("stop").localPosition);
				for (int m = 0; m < brNumVertices2; m++)
				{
					if (m % (lines.numInterpoles + 1) != 0)
					{
						Vector3 tmp3 = brKeyVertices2[m - 1];
						Vector3 tmp4 = brKeyVertices2[m + lines.numInterpoles];
						float x2 = Vector3.Distance(tmp3, tmp4) / (float)(lines.numInterpoles + 1) / Vector3.Distance(tmp3, tmp4) * (float)Math.PI;
						for (int n = 0; n < lines.numInterpoles; n++)
						{
							brTmpV4.x = tmp3.x + brDeltaV2.x * (float)(1 + n);
							brTmpV4.y = tmp3.y + (Mathf.Sin(x2 - (float)Math.PI / 2f) / 2f + 0.5f) * (tmp4.y - tmp3.y);
							brTmpV4.z = tmp3.z + (Mathf.Sin(x2 - (float)Math.PI / 2f) / 2f + 0.5f) * (tmp4.z - tmp3.z);
							branch.SetPosition(m + n, brTmpV4);
							x2 += x2;
						}
						m += lines.numInterpoles;
					}
				}
			}
			currentV += deltaV2 * (lines.numInterpoles + 1);
			i += lines.numInterpoles;
		}
		prefabs.lightning.SetPosition(0, prefabs.source.localPosition);
		prefabs.lightning.SetPosition(numVertices - 1, prefabs.target.localPosition);
		Vector3 tmpV2 = default(Vector3);
		for (int num = 0; num < numVertices; num++)
		{
			if (num % (lines.numInterpoles + 1) != 0)
			{
				Vector3 tmp5 = keyVertices[num - 1];
				Vector3 tmp6 = keyVertices[num + lines.numInterpoles];
				float x3 = Vector3.Distance(tmp5, tmp6) / (float)(lines.numInterpoles + 1) / Vector3.Distance(tmp5, tmp6) * (float)Math.PI;
				for (int num2 = 0; num2 < lines.numInterpoles; num2++)
				{
					tmpV2.x = tmp5.x + deltaV2.x * (float)(1 + num2);
					tmpV2.y = tmp5.y + (Mathf.Sin(x3 - (float)Math.PI / 2f) / 2f + 0.5f) * (tmp6.y - tmp5.y);
					tmpV2.z = tmp5.z + (Mathf.Sin(x3 - (float)Math.PI / 2f) / 2f + 0.5f) * (tmp6.z - tmp5.z);
					prefabs.lightning.SetPosition(num + num2, tmpV2);
					x3 += x3;
				}
				num += lines.numInterpoles;
			}
		}
	}

	private void AnimateArc()
	{
		Material material = prefabs.lightning.GetComponent<Renderer>().GetMaterial();
		Vector2 offset = material.mainTextureOffset;
		Vector2 scale = material.mainTextureScale;
		offset.x += Time.deltaTime * tex.animateSpeed;
		offset.y = tex.offsetY;
		scale.x = srcTrgDist / srcDstDist * tex.scaleX;
		scale.y = tex.scaleY;
		material.mainTextureOffset = offset;
		material.mainTextureScale = scale;
	}

	private void RayCast()
	{
		RaycastHit[] array = Physics.RaycastAll(prefabs.source.position, prefabs.target.position - prefabs.source.position, Vector3.Distance(prefabs.source.position, prefabs.target.position));
		foreach (RaycastHit hit in array)
		{
			UnityEngine.Object.Instantiate(prefabs.sparks, hit.point, Quaternion.identity);
		}
		if (branches.Count <= 0)
		{
			return;
		}
		foreach (int key in (IEnumerable)new Dictionary<int, LineRenderer>(branches).Keys)
		{
			LineRenderer branch = branches[key];
			array = Physics.RaycastAll(branch.transform.Find("start").position, branch.transform.Find("stop").position - branch.transform.Find("start").position, Vector3.Distance(branch.transform.Find("start").position, branch.transform.Find("stop").position));
			foreach (RaycastHit hit2 in array)
			{
				UnityEngine.Object.Instantiate(prefabs.sparks, hit2.point, Quaternion.identity);
			}
		}
	}
}
