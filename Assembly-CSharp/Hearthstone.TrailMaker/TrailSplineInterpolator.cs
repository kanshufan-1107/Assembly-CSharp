using System;
using System.Linq;
using UnityEngine;

namespace Hearthstone.TrailMaker;

public static class TrailSplineInterpolator
{
	private enum TrailVectorComponents
	{
		XY,
		XZ
	}

	private struct DoubleArray
	{
		public double[] a;

		public double[] b;
	}

	public static void Interpolate(MutableArray<TrailPosition> source, MutableArray<TrailPosition> target, float scale = 2f)
	{
		target.Count = Mathf.Max(Mathf.RoundToInt((float)source.Count * scale), 2);
		Interpolate2D(source, target, TrailVectorComponents.XY);
		Interpolate2D(source, target, TrailVectorComponents.XZ);
		for (int i = 0; i < target.Count; i++)
		{
			float t = (float)i / (float)target.Count;
			TrailPosition arbitraryPoint = source.GenerateDataForArbitraryTime(t);
			TrailPosition item = target.Get(i);
			item.min = item.origin + (arbitraryPoint.min - arbitraryPoint.origin);
			item.max = item.origin + (arbitraryPoint.max - arbitraryPoint.origin);
			item.time = arbitraryPoint.time;
			item.distance = arbitraryPoint.distance;
			target.Set(i, item);
		}
	}

	private static void Interpolate2D(MutableArray<TrailPosition> source, MutableArray<TrailPosition> target, TrailVectorComponents components)
	{
		if (source == null || source.Count <= 1 || target == null || target.Count <= 1)
		{
			return;
		}
		double[] xs = new double[source.Count];
		double[] ys = new double[source.Count];
		for (int i = 0; i < source.Count; i++)
		{
			TrailPosition item = source.Get(i);
			xs[i] = item.origin.x;
			ys[i] = ((components == TrailVectorComponents.XY) ? item.origin.y : item.origin.z);
		}
		DoubleArray xsAndYs = InterpolateXY(xs, ys, target.Count);
		for (int j = 0; j < xsAndYs.a.Length; j++)
		{
			TrailPosition item = target.Get(j);
			item.origin.x = (float)xsAndYs.a[j];
			if (components == TrailVectorComponents.XY)
			{
				item.origin.y = (float)xsAndYs.b[j];
			}
			else
			{
				item.origin.z = (float)xsAndYs.b[j];
			}
			target.Set(j, item);
		}
	}

	private static DoubleArray InterpolateXY(double[] xs, double[] ys, int count)
	{
		if (xs == null || ys == null || xs.Length != ys.Length)
		{
			return default(DoubleArray);
		}
		int inputPointCount = xs.Length;
		double[] inputDistances = new double[inputPointCount];
		for (int i = 1; i < inputPointCount; i++)
		{
			double num = xs[i] - xs[i - 1];
			double dy = ys[i] - ys[i - 1];
			double distance = Math.Sqrt(num * num + dy * dy);
			inputDistances[i] = inputDistances[i - 1] + distance;
		}
		double meanDistance = inputDistances.Last() / (double)(count - 1);
		double[] evenDistances = (from x in Enumerable.Range(0, count)
			select (double)x * meanDistance).ToArray();
		double[] xsOut = Interpolate(inputDistances, xs, evenDistances);
		double[] ysOut = Interpolate(inputDistances, ys, evenDistances);
		DoubleArray result = default(DoubleArray);
		result.a = xsOut;
		result.b = ysOut;
		return result;
	}

	private static double[] Interpolate(double[] xOrig, double[] yOrig, double[] xInterp)
	{
		DoubleArray ab = FitMatrix(xOrig, yOrig);
		double[] yInterp = new double[xInterp.Length];
		for (int i = 0; i < yInterp.Length; i++)
		{
			int j;
			for (j = 0; j < xOrig.Length - 2 && !(xInterp[i] <= xOrig[j + 1]); j++)
			{
			}
			double dx = xOrig[j + 1] - xOrig[j];
			double t = (xInterp[i] - xOrig[j]) / dx;
			double y = (1.0 - t) * yOrig[j] + t * yOrig[j + 1] + t * (1.0 - t) * (ab.a[j] * (1.0 - t) + ab.b[j] * t);
			if (!double.IsFinite(y))
			{
				y = 0.0;
			}
			yInterp[i] = y;
		}
		return yInterp;
	}

	private static DoubleArray FitMatrix(double[] x, double[] y)
	{
		int n = x.Length;
		double[] a = new double[n - 1];
		double[] b = new double[n - 1];
		double[] r = new double[n];
		double[] A = new double[n];
		double[] B = new double[n];
		double[] C = new double[n];
		double dx1 = x[1] - x[0];
		C[0] = 1.0 / dx1;
		B[0] = 2.0 * C[0];
		r[0] = 3.0 * (y[1] - y[0]) / (dx1 * dx1);
		double dy1;
		for (int i = 1; i < n - 1; i++)
		{
			dx1 = x[i] - x[i - 1];
			double dx2 = x[i + 1] - x[i];
			A[i] = 1.0 / dx1;
			C[i] = 1.0 / dx2;
			B[i] = 2.0 * (A[i] + C[i]);
			dy1 = y[i] - y[i - 1];
			double dy2 = y[i + 1] - y[i];
			r[i] = 3.0 * (dy1 / (dx1 * dx1) + dy2 / (dx2 * dx2));
		}
		dx1 = x[n - 1] - x[n - 2];
		dy1 = y[n - 1] - y[n - 2];
		A[n - 1] = 1.0 / dx1;
		B[n - 1] = 2.0 * A[n - 1];
		r[n - 1] = 3.0 * (dy1 / (dx1 * dx1));
		double[] cPrime = new double[n];
		cPrime[0] = C[0] / B[0];
		for (int j = 1; j < n; j++)
		{
			cPrime[j] = C[j] / (B[j] - cPrime[j - 1] * A[j]);
		}
		double[] dPrime = new double[n];
		dPrime[0] = r[0] / B[0];
		for (int k = 1; k < n; k++)
		{
			dPrime[k] = (r[k] - dPrime[k - 1] * A[k]) / (B[k] - cPrime[k - 1] * A[k]);
		}
		double[] k2 = new double[n];
		k2[n - 1] = dPrime[n - 1];
		for (int i2 = n - 2; i2 >= 0; i2--)
		{
			k2[i2] = dPrime[i2] - cPrime[i2] * k2[i2 + 1];
		}
		for (int l = 1; l < n; l++)
		{
			dx1 = x[l] - x[l - 1];
			dy1 = y[l] - y[l - 1];
			a[l - 1] = k2[l - 1] * dx1 - dy1;
			b[l - 1] = (0.0 - k2[l]) * dx1 + dy1;
		}
		DoubleArray result = default(DoubleArray);
		result.a = a;
		result.b = b;
		return result;
	}
}
