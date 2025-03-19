using Blizzard.T5.MaterialService.Extensions;
using UnityEngine;

public class CheckoutMesh : MonoBehaviour, IScreenSpace
{
	private static readonly Color kBlizzardBlue = new Color(0f, 0.14901961f, 16f / 51f, 1f);

	public MeshRenderer MeshRenderer { get; private set; }

	public Texture2D Texture { get; private set; }

	public GameObject CloseButton { get; private set; }

	public static CheckoutMesh GenerateCheckoutMesh(int browserWidth, int browserHeight, float meshWidth, float meshHeight)
	{
		CheckoutMesh checkoutMesh = new GameObject("CheckoutMesh").AddComponent<CheckoutMesh>();
		checkoutMesh.Initialize(browserWidth, browserHeight, meshWidth, meshHeight);
		return checkoutMesh;
	}

	private void Initialize(int browserWidth, int browserHeight, float meshWidth, float meshHeight)
	{
		MeshFilter meshFilter = base.gameObject.AddComponent<MeshFilter>();
		MeshCollider meshCol = base.gameObject.AddComponent<MeshCollider>();
		MeshRenderer = base.gameObject.AddComponent<MeshRenderer>();
		CreateBrowserMesh(meshFilter, meshCol, meshWidth, meshHeight);
		CreateTexture(browserWidth, browserHeight);
	}

	public void UpdateTexture(byte[] bytes)
	{
		if (!(Texture == null))
		{
			Texture.LoadRawTextureData(bytes);
			Texture.Apply();
		}
	}

	public void ResizeTexture(int width, int height)
	{
		CreateTexture(width, height);
	}

	public Rect GetScreenRect()
	{
		float height = (float)Screen.height * base.transform.localScale.x;
		float width = height * 1.5f;
		return GetScreenRect((int)width, (int)height);
	}

	public Rect GetScreenRect(int width, int height)
	{
		return new Rect((Screen.width - width) / 2, (Screen.height - height) / 2, width, height);
	}

	public float GetScreenSpaceScale()
	{
		float num = (float)Screen.height * base.transform.localScale.x;
		int heightInSpace = Texture.height;
		return num / (float)heightInSpace;
	}

	private void CreateBrowserMesh(MeshFilter filter, MeshCollider collider, float width, float height)
	{
		int x = 0;
		int y = 0;
		int z = 0;
		Vector3[] verts = new Vector3[4]
		{
			new Vector3(x, y, z),
			new Vector3((float)x + width, y, z),
			new Vector3(x, (float)y + height, z),
			new Vector3((float)x + width, (float)y + height, z)
		};
		Vector4[] tangents = new Vector4[4]
		{
			new Vector4(1f, 0f, 0f, -1f),
			new Vector4(1f, 0f, 0f, -1f),
			new Vector4(1f, 0f, 0f, -1f),
			new Vector4(1f, 0f, 0f, -1f)
		};
		int[] triangles = new int[6];
		triangles[0] = 0;
		triangles[3] = (triangles[2] = 1);
		triangles[4] = (triangles[1] = 2);
		triangles[5] = 3;
		Vector2[] uv = new Vector2[4];
		uv[2] = new Vector2(0f, 0f);
		uv[3] = new Vector2(1f, 0f);
		uv[0] = new Vector2(0f, 1f);
		uv[1] = new Vector2(1f, 1f);
		filter.mesh = new Mesh
		{
			name = "Blizzard Checkout",
			vertices = verts,
			triangles = triangles,
			uv = uv,
			tangents = tangents
		};
		filter.mesh.RecalculateNormals();
		collider.sharedMesh = filter.mesh;
	}

	private void CreateTexture(int width, int height)
	{
		Object.Destroy(Texture);
		Texture = null;
		Texture2D tempTexture = new Texture2D(width, height, TextureFormat.BGRA32, mipChain: false, linear: false);
		tempTexture.filterMode = FilterMode.Point;
		tempTexture.wrapMode = TextureWrapMode.Clamp;
		Texture = tempTexture;
		Color color = kBlizzardBlue;
		for (int x = 0; x < width; x++)
		{
			for (int y = 0; y < height; y++)
			{
				Texture.SetPixel(x, y, color);
			}
		}
		Texture.Apply(updateMipmaps: false, makeNoLongerReadable: false);
		Material material = new Material(Shader.Find("Hero/Unlit/Unlit_Texture"));
		material.SetTexture("_MainTex", Texture);
		MeshRenderer.SetMaterial(material);
	}

	private void OnDestroy()
	{
		Object.Destroy(Texture);
		Texture = null;
	}
}
