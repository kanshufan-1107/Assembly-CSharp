using System;
using System.Collections.Generic;
using System.Linq;
using Blizzard.T5.MaterialService.Extensions;
using UnityEngine;

public class GeometryWeld
{
	protected class SuggestedTranslation
	{
		public Vector3 translation;

		public List<int> startIndicies = new List<int>();

		public List<int> endIndicies = new List<int>();

		public bool MergeWith(SuggestedTranslation other, float clampErrorAngle)
		{
			if (Vector3.Angle(translation, other.translation) > clampErrorAngle)
			{
				return false;
			}
			startIndicies.AddRange(other.startIndicies);
			endIndicies.AddRange(other.endIndicies);
			return true;
		}
	}

	private static readonly bool DEBUG;

	public GameObject weldedGameObject;

	private IEnumerable<MeshRenderer> meshRenderers;

	public GeometryWeld(GameObject root, params GameObject[] objectsToWeld)
	{
		if (!Application.IsPlaying(root))
		{
			return;
		}
		IEnumerable<GameObject> allObjects = from x in new GameObject[1] { root }.Concat(objectsToWeld.Where((GameObject x) => x != root))
			where x.GetComponent<MeshRenderer>() != null
			select x;
		IEnumerable<MeshFilter> meshFilters = allObjects.Select((GameObject x) => x.GetComponent<MeshFilter>());
		meshRenderers = allObjects.Select((GameObject x) => x.GetComponent<MeshRenderer>());
		List<Material> rootMaterials = meshRenderers.First().GetSharedMaterials();
		Func<Material[], bool> matchesRootMaterials = delegate(Material[] materials)
		{
			if (materials.Length != rootMaterials.Count)
			{
				return false;
			}
			for (int i = 0; i < rootMaterials.Count; i++)
			{
				if (materials[i] != rootMaterials[i])
				{
					return false;
				}
			}
			return true;
		};
		if (!(from x in meshRenderers.Skip(1)
			select x.GetSharedMaterials().ToArray()).All(matchesRootMaterials))
		{
			Error.AddDevFatal("Unable to weld {0} to {1}.  Materials differ.", root.name, string.Join(", ", objectsToWeld.Select((GameObject x) => x.name).ToArray()));
			return;
		}
		weldedGameObject = new GameObject("Welded_" + root.name);
		weldedGameObject.AddComponent<MeshFilter>().sharedMesh = CombineMeshes(meshFilters.Select(delegate(MeshFilter x)
		{
			CombineInstance result = default(CombineInstance);
			result.mesh = x.sharedMesh;
			result.transform = root.transform.worldToLocalMatrix * x.transform.localToWorldMatrix;
			return result;
		}).ToArray());
		weldedGameObject.AddComponent<MeshRenderer>().SetSharedMaterials(rootMaterials);
		weldedGameObject.transform.SetParent(root.transform.parent);
		weldedGameObject.transform.position = root.transform.position;
		weldedGameObject.transform.rotation = root.transform.rotation;
		weldedGameObject.transform.localScale = root.transform.localScale;
		weldedGameObject.hideFlags = HideFlags.DontSave | HideFlags.NotEditable;
		if (DEBUG)
		{
			return;
		}
		foreach (MeshRenderer meshRenderer in meshRenderers)
		{
			meshRenderer.enabled = false;
		}
	}

	private static Mesh CombineMeshes(IEnumerable<CombineInstance> combines)
	{
		Mesh mesh = new Mesh();
		int outerVertexIndex = 0;
		int outerTriangleIndex = 0;
		int num = combines.Select((CombineInstance x) => x.mesh.vertexCount).Sum();
		Vector3[] vertices = new Vector3[num];
		int[] triangles = new int[combines.Select((CombineInstance x) => x.mesh.triangles.Length).Sum()];
		Vector2[] uv = new Vector2[num];
		Vector3[] normals = new Vector3[num];
		foreach (CombineInstance combine in combines)
		{
			Vector3[] meshVertices = combine.mesh.vertices;
			int numVerticies = meshVertices.Length;
			Array.Copy(combine.mesh.uv, 0, uv, outerVertexIndex, numVerticies);
			Array.Copy(combine.mesh.normals, 0, normals, outerVertexIndex, numVerticies);
			for (int i = 0; i < numVerticies; i++)
			{
				Vector3 meshVertex = meshVertices[i];
				Vector4 vertexWithW = new Vector4(meshVertex.x, meshVertex.y, meshVertex.z, 1f);
				vertexWithW = combine.transform * vertexWithW;
				vertices[outerVertexIndex + i] = vertexWithW;
			}
			int[] meshTriangles = combine.mesh.triangles;
			int numTriangles = meshTriangles.Length;
			for (int j = 0; j < numTriangles; j++)
			{
				triangles[outerTriangleIndex++] = meshTriangles[j] + outerVertexIndex;
			}
			outerVertexIndex += numVerticies;
		}
		ClampMeshes(vertices, combines, 0.03f, 20f);
		StretchTriangles(vertices, combines, 0.005f);
		mesh.vertices = vertices;
		mesh.triangles = triangles;
		mesh.uv = uv;
		mesh.normals = normals;
		return mesh;
	}

	private static void ClampMeshes(Vector3[] verticies, IEnumerable<CombineInstance> meshRanges, float clampSqrDistance, float clampErrorAngle)
	{
		List<SuggestedTranslation> suggestedTranslations = new List<SuggestedTranslation>();
		int lastIndexOfPreviousMesh = -1;
		int lastIndexOfThisMesh = -1;
		foreach (CombineInstance range in meshRanges)
		{
			suggestedTranslations.Clear();
			lastIndexOfPreviousMesh = lastIndexOfThisMesh;
			lastIndexOfThisMesh += range.mesh.vertexCount;
			for (int rootMeshVertexIndex = 0; rootMeshVertexIndex <= lastIndexOfPreviousMesh; rootMeshVertexIndex++)
			{
				Vector3 rootVertex = verticies[rootMeshVertexIndex];
				for (int thisMeshVertexIndex = lastIndexOfPreviousMesh + 1; thisMeshVertexIndex <= lastIndexOfThisMesh; thisMeshVertexIndex++)
				{
					Vector3 thisVertex = verticies[thisMeshVertexIndex];
					Vector3 delta = rootVertex - thisVertex;
					if (delta.sqrMagnitude <= clampSqrDistance)
					{
						Vector4 originalPosition = new Vector4(thisVertex.x, thisVertex.y, thisVertex.z, 1f);
						Vector3 directionOfMovement = range.transform * originalPosition - originalPosition;
						float angle = Vector3.Angle(delta, directionOfMovement);
						if (angle < clampErrorAngle)
						{
							SuggestedTranslation suggestedTranslation = new SuggestedTranslation
							{
								translation = delta
							};
							suggestedTranslation.startIndicies.Add(thisMeshVertexIndex);
							suggestedTranslation.endIndicies.Add(rootMeshVertexIndex);
							suggestedTranslations.Add(suggestedTranslation);
						}
						else if (angle + clampErrorAngle > 180f)
						{
							SuggestedTranslation suggestedTranslation2 = new SuggestedTranslation
							{
								translation = -delta
							};
							suggestedTranslation2.startIndicies.Add(thisMeshVertexIndex);
							suggestedTranslation2.endIndicies.Add(rootMeshVertexIndex);
							suggestedTranslations.Add(suggestedTranslation2);
						}
					}
				}
			}
			int totalSuggestions = suggestedTranslations.Count;
			for (int i = 0; i < suggestedTranslations.Count; i++)
			{
				for (int j = i + 1; j < suggestedTranslations.Count; j++)
				{
					if (suggestedTranslations[i].MergeWith(suggestedTranslations[j], clampErrorAngle))
					{
						suggestedTranslations.RemoveAt(j);
						j--;
					}
				}
			}
			SuggestedTranslation mostPopular = suggestedTranslations.OrderBy((SuggestedTranslation x) => x.startIndicies.Count).FirstOrDefault();
			if (mostPopular == null || mostPopular.startIndicies.Count <= totalSuggestions / 2)
			{
				continue;
			}
			for (int k = lastIndexOfPreviousMesh + 1; k <= lastIndexOfThisMesh; k++)
			{
				int startIndexIndex = mostPopular.startIndicies.IndexOf(k);
				if (startIndexIndex == -1)
				{
					verticies[k] += mostPopular.translation;
				}
				else
				{
					verticies[mostPopular.startIndicies[startIndexIndex]] = verticies[mostPopular.endIndicies[startIndexIndex]];
				}
			}
		}
	}

	private static void StretchTriangles(Vector3[] verticies, IEnumerable<CombineInstance> meshRanges, float strechSqrDistance)
	{
		int lastIndexOfPreviousMesh = -1;
		int lastIndexOfThisMesh = -1;
		foreach (CombineInstance meshRange in meshRanges)
		{
			_ = meshRange;
			for (int rootMeshVertexIndex = 0; rootMeshVertexIndex <= lastIndexOfPreviousMesh; rootMeshVertexIndex++)
			{
				for (int thisMeshVertexIndex = lastIndexOfPreviousMesh + 1; thisMeshVertexIndex <= lastIndexOfThisMesh; thisMeshVertexIndex++)
				{
					if ((verticies[rootMeshVertexIndex] - verticies[thisMeshVertexIndex]).sqrMagnitude <= strechSqrDistance)
					{
						verticies[thisMeshVertexIndex] = verticies[rootMeshVertexIndex];
					}
				}
			}
		}
	}

	public void Unweld()
	{
		if (weldedGameObject == null || !Application.IsPlaying(weldedGameObject))
		{
			return;
		}
		UnityEngine.Object.Destroy(weldedGameObject);
		foreach (MeshRenderer meshRenderer in meshRenderers)
		{
			meshRenderer.enabled = true;
		}
	}
}
