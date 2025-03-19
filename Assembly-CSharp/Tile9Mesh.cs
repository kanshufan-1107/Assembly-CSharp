using UnityEngine;

[RequireComponent(typeof(MeshRenderer), typeof(MeshFilter))]
[ExecuteInEditMode]
public class Tile9Mesh : MonoBehaviour
{
	public float width = 1f;

	public float height = 1f;

	[Range(0f, 0.5f)]
	public float uvLeft = 0.2f;

	[Range(0f, 0.5f)]
	public float uvRight = 0.2f;

	[Range(0f, 0.5f)]
	public float uvTop = 0.2f;

	[Range(0f, 0.5f)]
	public float uvBottom = 0.2f;

	public float uvToWorldScaleX = 1f;

	public float uvToWorldScaleY = 1f;

	public Vector2 pivot = new Vector2(0.5f, 0.5f);

	private Mesh mesh;

	private Vector3[] vertices;

	private Vector2[] uv;

	private void Start()
	{
		vertices = new Vector3[16];
		uv = new Vector2[16];
		mesh = new Mesh
		{
			name = "Tile9Mesh"
		};
		FillGeometry();
		FillMesh();
		mesh.triangles = new int[54]
		{
			0, 1, 12, 0, 12, 11, 1, 2, 13, 1,
			13, 12, 2, 3, 4, 2, 4, 13, 13, 4,
			5, 13, 5, 14, 14, 5, 6, 14, 6, 7,
			15, 14, 7, 15, 7, 8, 10, 15, 8, 10,
			8, 9, 11, 12, 15, 11, 15, 10, 12, 13,
			14, 12, 14, 15
		};
		RecalculateMesh();
		base.gameObject.GetComponent<MeshFilter>().mesh = mesh;
	}

	public void UpdateMesh()
	{
		if (mesh != null)
		{
			FillGeometry();
			FillMesh();
			mesh.RecalculateBounds();
			mesh.RecalculateNormals();
		}
	}

	private void FillGeometry()
	{
		float xOffset = pivot.x * width;
		float yOffset = pivot.y * height;
		float w = width;
		float h = height;
		float l = uvLeft * uvToWorldScaleX;
		float r = width - uvRight * uvToWorldScaleX;
		float t = height - uvTop * uvToWorldScaleY;
		float b = uvBottom * uvToWorldScaleY;
		vertices[0] = new Vector3(0f - xOffset, 0f - yOffset, 0f);
		vertices[1] = new Vector3(0f - xOffset, b - yOffset, 0f);
		vertices[2] = new Vector3(0f - xOffset, t - yOffset, 0f);
		vertices[3] = new Vector3(0f - xOffset, h - yOffset, 0f);
		vertices[4] = new Vector3(l - xOffset, h - yOffset, 0f);
		vertices[5] = new Vector3(r - xOffset, h - yOffset, 0f);
		vertices[6] = new Vector3(w - xOffset, h - yOffset, 0f);
		vertices[7] = new Vector3(w - xOffset, t - yOffset, 0f);
		vertices[8] = new Vector3(w - xOffset, b - yOffset, 0f);
		vertices[9] = new Vector3(w - xOffset, 0f - yOffset, 0f);
		vertices[10] = new Vector3(r - xOffset, 0f - yOffset, 0f);
		vertices[11] = new Vector3(l - xOffset, 0f - yOffset, 0f);
		vertices[12] = new Vector3(l - xOffset, b - yOffset, 0f);
		vertices[13] = new Vector3(l - xOffset, t - yOffset, 0f);
		vertices[14] = new Vector3(r - xOffset, t - yOffset, 0f);
		vertices[15] = new Vector3(r - xOffset, b - yOffset, 0f);
		float l2 = uvLeft;
		float r2 = 1f - uvRight;
		float t2 = 1f - uvTop;
		float b2 = uvBottom;
		uv[0] = new Vector2(0f, 0f);
		uv[1] = new Vector2(0f, b2);
		uv[2] = new Vector2(0f, t2);
		uv[3] = new Vector2(0f, 1f);
		uv[4] = new Vector2(l2, 1f);
		uv[5] = new Vector2(r2, 1f);
		uv[6] = new Vector2(1f, 1f);
		uv[7] = new Vector2(1f, t2);
		uv[8] = new Vector2(1f, b2);
		uv[9] = new Vector2(1f, 0f);
		uv[10] = new Vector2(r2, 0f);
		uv[11] = new Vector2(l2, 0f);
		uv[12] = new Vector2(l2, b2);
		uv[13] = new Vector2(l2, t2);
		uv[14] = new Vector2(r2, t2);
		uv[15] = new Vector2(r2, b2);
	}

	private void FillMesh()
	{
		mesh.vertices = vertices;
		mesh.uv = uv;
	}

	private void RecalculateMesh()
	{
		mesh.RecalculateBounds();
		mesh.RecalculateNormals();
	}
}
