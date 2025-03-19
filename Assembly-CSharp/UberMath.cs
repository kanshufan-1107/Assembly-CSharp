using UnityEngine;

public static class UberMath
{
	private static readonly int[,] grad3;

	private static readonly int[,] grad4;

	private static readonly int[,] simplex;

	private static int[] perm;

	static UberMath()
	{
		grad3 = new int[12, 3]
		{
			{ 1, 1, 0 },
			{ -1, 1, 0 },
			{ 1, -1, 0 },
			{ -1, -1, 0 },
			{ 1, 0, 1 },
			{ -1, 0, 1 },
			{ 1, 0, -1 },
			{ -1, 0, -1 },
			{ 0, 1, 1 },
			{ 0, -1, 1 },
			{ 0, 1, -1 },
			{ 0, -1, -1 }
		};
		grad4 = new int[32, 4]
		{
			{ 0, 1, 1, 1 },
			{ 0, 1, 1, -1 },
			{ 0, 1, -1, 1 },
			{ 0, 1, -1, -1 },
			{ 0, -1, 1, 1 },
			{ 0, -1, 1, -1 },
			{ 0, -1, -1, 1 },
			{ 0, -1, -1, -1 },
			{ 1, 0, 1, 1 },
			{ 1, 0, 1, -1 },
			{ 1, 0, -1, 1 },
			{ 1, 0, -1, -1 },
			{ -1, 0, 1, 1 },
			{ -1, 0, 1, -1 },
			{ -1, 0, -1, 1 },
			{ -1, 0, -1, -1 },
			{ 1, 1, 0, 1 },
			{ 1, 1, 0, -1 },
			{ 1, -1, 0, 1 },
			{ 1, -1, 0, -1 },
			{ -1, 1, 0, 1 },
			{ -1, 1, 0, -1 },
			{ -1, -1, 0, 1 },
			{ -1, -1, 0, -1 },
			{ 1, 1, 1, 0 },
			{ 1, 1, -1, 0 },
			{ 1, -1, 1, 0 },
			{ 1, -1, -1, 0 },
			{ -1, 1, 1, 0 },
			{ -1, 1, -1, 0 },
			{ -1, -1, 1, 0 },
			{ -1, -1, -1, 0 }
		};
		simplex = new int[64, 4]
		{
			{ 0, 1, 2, 3 },
			{ 0, 1, 3, 2 },
			{ 0, 0, 0, 0 },
			{ 0, 2, 3, 1 },
			{ 0, 0, 0, 0 },
			{ 0, 0, 0, 0 },
			{ 0, 0, 0, 0 },
			{ 1, 2, 3, 0 },
			{ 0, 2, 1, 3 },
			{ 0, 0, 0, 0 },
			{ 0, 3, 1, 2 },
			{ 0, 3, 2, 1 },
			{ 0, 0, 0, 0 },
			{ 0, 0, 0, 0 },
			{ 0, 0, 0, 0 },
			{ 1, 3, 2, 0 },
			{ 0, 0, 0, 0 },
			{ 0, 0, 0, 0 },
			{ 0, 0, 0, 0 },
			{ 0, 0, 0, 0 },
			{ 0, 0, 0, 0 },
			{ 0, 0, 0, 0 },
			{ 0, 0, 0, 0 },
			{ 0, 0, 0, 0 },
			{ 1, 2, 0, 3 },
			{ 0, 0, 0, 0 },
			{ 1, 3, 0, 2 },
			{ 0, 0, 0, 0 },
			{ 0, 0, 0, 0 },
			{ 0, 0, 0, 0 },
			{ 2, 3, 0, 1 },
			{ 2, 3, 1, 0 },
			{ 1, 0, 2, 3 },
			{ 1, 0, 3, 2 },
			{ 0, 0, 0, 0 },
			{ 0, 0, 0, 0 },
			{ 0, 0, 0, 0 },
			{ 2, 0, 3, 1 },
			{ 0, 0, 0, 0 },
			{ 2, 1, 3, 0 },
			{ 0, 0, 0, 0 },
			{ 0, 0, 0, 0 },
			{ 0, 0, 0, 0 },
			{ 0, 0, 0, 0 },
			{ 0, 0, 0, 0 },
			{ 0, 0, 0, 0 },
			{ 0, 0, 0, 0 },
			{ 0, 0, 0, 0 },
			{ 2, 0, 1, 3 },
			{ 0, 0, 0, 0 },
			{ 0, 0, 0, 0 },
			{ 0, 0, 0, 0 },
			{ 3, 0, 1, 2 },
			{ 3, 0, 2, 1 },
			{ 0, 0, 0, 0 },
			{ 3, 1, 2, 0 },
			{ 2, 1, 0, 3 },
			{ 0, 0, 0, 0 },
			{ 0, 0, 0, 0 },
			{ 0, 0, 0, 0 },
			{ 3, 1, 0, 2 },
			{ 0, 0, 0, 0 },
			{ 3, 2, 0, 1 },
			{ 3, 2, 1, 0 }
		};
		perm = new int[512];
		for (int i = 0; i < 512; i++)
		{
			perm[i] = Random.Range(5, 250);
		}
	}

	private static int floor(float x)
	{
		if (!(x > 0f))
		{
			return (int)x - 1;
		}
		return (int)x;
	}

	private static float dot(int gx, int gy, float x, float y)
	{
		return (float)gx * x + (float)gy * y;
	}

	private static float dot(int gx, int gy, int gz, float x, float y, float z)
	{
		return (float)gx * x + (float)gy * y + (float)gz * z;
	}

	public static float SimplexNoise(float xin, float yin)
	{
		float F2 = 0.36602542f;
		float s = (xin + yin) * F2;
		int num = floor(xin + s);
		int j = floor(yin + s);
		float G2 = 0.21132487f;
		float t = (float)(num + j) * G2;
		float Y0 = (float)j - t;
		float X0 = (float)num - t;
		float y0 = yin - Y0;
		float x0 = xin - X0;
		int i1;
		int j2;
		if (x0 > y0)
		{
			i1 = 1;
			j2 = 0;
		}
		else
		{
			i1 = 0;
			j2 = 1;
		}
		float x1 = x0 - (float)i1 + G2;
		float y1 = y0 - (float)j2 + G2;
		float x2 = x0 - 1f + 2f * G2;
		float y2 = y0 - 1f + 2f * G2;
		int ii = num & 0xFF;
		int jj = j & 0xFF;
		int gi0 = perm[ii + perm[jj]] % 12;
		int gi1 = perm[ii + i1 + perm[jj + j2]] % 12;
		int gi2 = perm[ii + 1 + perm[jj + 1]] % 12;
		float t2 = 0.5f - x0 * x0 - y0 * y0;
		float n0;
		if (t2 < 0f)
		{
			n0 = 0f;
		}
		else
		{
			t2 *= t2;
			n0 = t2 * t2 * dot(grad3[gi0, 0], grad3[gi0, 1], x0, y0);
		}
		float t3 = 0.5f - x1 * x1 - y1 * y1;
		float n1;
		if (t3 < 0f)
		{
			n1 = 0f;
		}
		else
		{
			t3 *= t3;
			n1 = t3 * t3 * dot(grad3[gi1, 0], grad3[gi1, 1], x1, y1);
		}
		float t4 = 0.5f - x2 * x2 - y2 * y2;
		float n2;
		if (t4 < 0f)
		{
			n2 = 0f;
		}
		else
		{
			t4 *= t4;
			n2 = t4 * t4 * dot(grad3[gi2, 0], grad3[gi2, 1], x2, y2);
		}
		return 70f * (n0 + n1 + n2);
	}

	public static float SimplexNoise(float xin, float yin, float zin)
	{
		float s = (xin + yin + zin) * (1f / 3f);
		int num = floor(xin + s);
		int j = floor(yin + s);
		int k = floor(zin + s);
		float t = (float)(num + j + k) * (1f / 6f);
		float X0 = (float)num - t;
		float Y0 = (float)j - t;
		float Z0 = (float)k - t;
		float x0 = xin - X0;
		float y0 = yin - Y0;
		float z0 = zin - Z0;
		int i1;
		int j2;
		int k2;
		int i2;
		int j3;
		int k3;
		if (x0 >= y0)
		{
			if (y0 >= z0)
			{
				i1 = 1;
				j2 = 0;
				k2 = 0;
				i2 = 1;
				j3 = 1;
				k3 = 0;
			}
			else if (x0 >= z0)
			{
				i1 = 1;
				j2 = 0;
				k2 = 0;
				i2 = 1;
				j3 = 0;
				k3 = 1;
			}
			else
			{
				i1 = 0;
				j2 = 0;
				k2 = 1;
				i2 = 1;
				j3 = 0;
				k3 = 1;
			}
		}
		else if (y0 < z0)
		{
			i1 = 0;
			j2 = 0;
			k2 = 1;
			i2 = 0;
			j3 = 1;
			k3 = 1;
		}
		else if (x0 < z0)
		{
			i1 = 0;
			j2 = 1;
			k2 = 0;
			i2 = 0;
			j3 = 1;
			k3 = 1;
		}
		else
		{
			i1 = 0;
			j2 = 1;
			k2 = 0;
			i2 = 1;
			j3 = 1;
			k3 = 0;
		}
		float x1 = x0 - (float)i1 + 1f / 6f;
		float y1 = y0 - (float)j2 + 1f / 6f;
		float z1 = z0 - (float)k2 + 1f / 6f;
		float x2 = x0 - (float)i2 + 1f / 3f;
		float y2 = y0 - (float)j3 + 1f / 3f;
		float z2 = z0 - (float)k3 + 1f / 3f;
		float x3 = x0 - 1f + 0.5f;
		float y3 = y0 - 1f + 0.5f;
		float z3 = z0 - 1f + 0.5f;
		int ii = num & 0xFF;
		int jj = j & 0xFF;
		int kk = k & 0xFF;
		int gi0 = perm[ii + perm[jj + perm[kk]]] % 12;
		int gi1 = perm[ii + i1 + perm[jj + j2 + perm[kk + k2]]] % 12;
		int gi2 = perm[ii + i2 + perm[jj + j3 + perm[kk + k3]]] % 12;
		int gi3 = perm[ii + 1 + perm[jj + 1 + perm[kk + 1]]] % 12;
		float t2 = 0.6f - x0 * x0 - y0 * y0 - z0 * z0;
		float n0;
		if (t2 < 0f)
		{
			n0 = 0f;
		}
		else
		{
			t2 *= t2;
			n0 = t2 * t2 * dot(grad3[gi0, 0], grad3[gi0, 1], grad3[gi0, 2], x0, y0, z0);
		}
		float t3 = 0.6f - x1 * x1 - y1 * y1 - z1 * z1;
		float n1;
		if (t3 < 0f)
		{
			n1 = 0f;
		}
		else
		{
			t3 *= t3;
			n1 = t3 * t3 * dot(grad3[gi1, 0], grad3[gi1, 1], grad3[gi1, 2], x1, y1, z1);
		}
		float t4 = 0.6f - x2 * x2 - y2 * y2 - z2 * z2;
		float n2;
		if (t4 < 0f)
		{
			n2 = 0f;
		}
		else
		{
			t4 *= t4;
			n2 = t4 * t4 * dot(grad3[gi2, 0], grad3[gi2, 1], grad3[gi2, 2], x2, y2, z2);
		}
		float t5 = 0.6f - x3 * x3 - y3 * y3 - z3 * z3;
		float n3;
		if (t5 < 0f)
		{
			n3 = 0f;
		}
		else
		{
			t5 *= t5;
			n3 = t5 * t5 * dot(grad3[gi3, 0], grad3[gi3, 1], grad3[gi3, 2], x3, y3, z3);
		}
		return 32f * (n0 + n1 + n2 + n3);
	}
}
