using UnityEngine;

public class MeshCombineUtility
{
	public struct MeshInstance
	{
		public Mesh mesh;

		public int subMeshIndex;

		public Matrix4x4 transform;
	}

	public static Mesh Combine(MeshInstance[] combines, bool generateStrips)
	{
		int vertexCount = 0;
		int triangleCount = 0;
		MeshInstance[] array = combines;
		for (int i = 0; i < array.Length; i++)
		{
			MeshInstance combine = array[i];
			if ((bool)combine.mesh)
			{
				vertexCount += combine.mesh.vertexCount;
			}
		}
		array = combines;
		for (int i = 0; i < array.Length; i++)
		{
			MeshInstance combine2 = array[i];
			if ((bool)combine2.mesh)
			{
				triangleCount += combine2.mesh.GetTriangles(combine2.subMeshIndex).Length;
			}
		}
		Vector3[] vertices = new Vector3[vertexCount];
		Vector3[] normals = new Vector3[vertexCount];
		Vector4[] tangents = new Vector4[vertexCount];
		Vector2[] uv = new Vector2[vertexCount];
		Vector2[] uv2 = new Vector2[vertexCount];
		Color[] colors = new Color[vertexCount];
		int[] triangles = new int[triangleCount];
		int offset = 0;
		array = combines;
		for (int i = 0; i < array.Length; i++)
		{
			MeshInstance combine3 = array[i];
			if ((bool)combine3.mesh)
			{
				Copy(combine3.mesh.vertexCount, combine3.mesh.vertices, vertices, ref offset, combine3.transform);
			}
		}
		offset = 0;
		array = combines;
		for (int i = 0; i < array.Length; i++)
		{
			MeshInstance combine4 = array[i];
			if ((bool)combine4.mesh)
			{
				Matrix4x4 invTranspose = combine4.transform;
				invTranspose = invTranspose.inverse.transpose;
				CopyNormal(combine4.mesh.vertexCount, combine4.mesh.normals, normals, ref offset, invTranspose);
			}
		}
		offset = 0;
		array = combines;
		for (int i = 0; i < array.Length; i++)
		{
			MeshInstance combine5 = array[i];
			if ((bool)combine5.mesh)
			{
				Matrix4x4 invTranspose2 = combine5.transform;
				invTranspose2 = invTranspose2.inverse.transpose;
				CopyTangents(combine5.mesh.vertexCount, combine5.mesh.tangents, tangents, ref offset, invTranspose2);
			}
		}
		offset = 0;
		array = combines;
		for (int i = 0; i < array.Length; i++)
		{
			MeshInstance combine6 = array[i];
			if ((bool)combine6.mesh)
			{
				Copy(combine6.mesh.vertexCount, combine6.mesh.uv, uv, ref offset);
			}
		}
		offset = 0;
		array = combines;
		for (int i = 0; i < array.Length; i++)
		{
			MeshInstance combine7 = array[i];
			if ((bool)combine7.mesh)
			{
				Copy(combine7.mesh.vertexCount, combine7.mesh.uv2, uv2, ref offset);
			}
		}
		offset = 0;
		array = combines;
		for (int i = 0; i < array.Length; i++)
		{
			MeshInstance combine8 = array[i];
			if ((bool)combine8.mesh)
			{
				CopyColors(combine8.mesh.vertexCount, combine8.mesh.colors, colors, ref offset);
			}
		}
		int triangleOffset = 0;
		int vertexOffset = 0;
		array = combines;
		for (int i = 0; i < array.Length; i++)
		{
			MeshInstance combine9 = array[i];
			if ((bool)combine9.mesh)
			{
				int[] inputtriangles = combine9.mesh.GetTriangles(combine9.subMeshIndex);
				for (int j = 0; j < inputtriangles.Length; j++)
				{
					triangles[j + triangleOffset] = inputtriangles[j] + vertexOffset;
				}
				triangleOffset += inputtriangles.Length;
				vertexOffset += combine9.mesh.vertexCount;
			}
		}
		return new Mesh
		{
			name = "Combined Mesh",
			vertices = vertices,
			normals = normals,
			colors = colors,
			uv = uv,
			uv2 = uv2,
			tangents = tangents,
			triangles = triangles
		};
	}

	private static void Copy(int vertexcount, Vector3[] src, Vector3[] dst, ref int offset, Matrix4x4 transform)
	{
		for (int i = 0; i < src.Length; i++)
		{
			dst[i + offset] = transform.MultiplyPoint(src[i]);
		}
		offset += vertexcount;
	}

	private static void CopyNormal(int vertexcount, Vector3[] src, Vector3[] dst, ref int offset, Matrix4x4 transform)
	{
		for (int i = 0; i < src.Length; i++)
		{
			dst[i + offset] = transform.MultiplyVector(src[i]).normalized;
		}
		offset += vertexcount;
	}

	private static void Copy(int vertexcount, Vector2[] src, Vector2[] dst, ref int offset)
	{
		for (int i = 0; i < src.Length; i++)
		{
			dst[i + offset] = src[i];
		}
		offset += vertexcount;
	}

	private static void CopyColors(int vertexcount, Color[] src, Color[] dst, ref int offset)
	{
		for (int i = 0; i < src.Length; i++)
		{
			dst[i + offset] = src[i];
		}
		offset += vertexcount;
	}

	private static void CopyTangents(int vertexcount, Vector4[] src, Vector4[] dst, ref int offset, Matrix4x4 transform)
	{
		for (int i = 0; i < src.Length; i++)
		{
			Vector4 p4 = src[i];
			Vector3 p5 = new Vector3(p4.x, p4.y, p4.z);
			p5 = transform.MultiplyVector(p5).normalized;
			dst[i + offset] = new Vector4(p5.x, p5.y, p5.z, p4.w);
		}
		offset += vertexcount;
	}
}
