using System;
using System.Collections.Generic;
using Blizzard.T5.Core.Utils;
using Hearthstone.UI;
using UnityEngine;

public class TransformUtil
{
	public enum PhoneAspectRatio
	{
		Minimum = 0,
		Wide = 1,
		ExtraWide = 2,
		Maximum = 2
	}

	public static Vector3 GetUnitAnchor(Anchor anchor)
	{
		Vector3 unitAnchor = default(Vector3);
		switch (anchor)
		{
		case Anchor.TOP_LEFT:
			unitAnchor.x = 0f;
			unitAnchor.y = 1f;
			unitAnchor.z = 0f;
			break;
		case Anchor.TOP:
			unitAnchor.x = 0.5f;
			unitAnchor.y = 1f;
			unitAnchor.z = 0f;
			break;
		case Anchor.TOP_RIGHT:
			unitAnchor.x = 1f;
			unitAnchor.y = 1f;
			unitAnchor.z = 0f;
			break;
		case Anchor.LEFT:
			unitAnchor.x = 0f;
			unitAnchor.y = 0.5f;
			unitAnchor.z = 0f;
			break;
		case Anchor.CENTER:
			unitAnchor.x = 0.5f;
			unitAnchor.y = 0.5f;
			unitAnchor.z = 0f;
			break;
		case Anchor.RIGHT:
			unitAnchor.x = 1f;
			unitAnchor.y = 0.5f;
			unitAnchor.z = 0f;
			break;
		case Anchor.BOTTOM_LEFT:
			unitAnchor.x = 0f;
			unitAnchor.y = 0f;
			unitAnchor.z = 0f;
			break;
		case Anchor.BOTTOM:
			unitAnchor.x = 0.5f;
			unitAnchor.y = 0f;
			unitAnchor.z = 0f;
			break;
		case Anchor.BOTTOM_RIGHT:
			unitAnchor.x = 1f;
			unitAnchor.y = 0f;
			unitAnchor.z = 0f;
			break;
		case Anchor.FRONT:
			unitAnchor.x = 0.5f;
			unitAnchor.y = 0f;
			unitAnchor.z = 1f;
			break;
		case Anchor.BACK:
			unitAnchor.x = 0.5f;
			unitAnchor.y = 0f;
			unitAnchor.z = 0f;
			break;
		case Anchor.TOP_LEFT_XZ:
			unitAnchor.x = 0f;
			unitAnchor.z = 1f;
			unitAnchor.y = 0f;
			break;
		case Anchor.TOP_XZ:
			unitAnchor.x = 0.5f;
			unitAnchor.z = 1f;
			unitAnchor.y = 0f;
			break;
		case Anchor.TOP_RIGHT_XZ:
			unitAnchor.x = 1f;
			unitAnchor.z = 1f;
			unitAnchor.y = 0f;
			break;
		case Anchor.LEFT_XZ:
			unitAnchor.x = 0f;
			unitAnchor.z = 0.5f;
			unitAnchor.y = 0f;
			break;
		case Anchor.CENTER_XZ:
			unitAnchor.x = 0.5f;
			unitAnchor.z = 0.5f;
			unitAnchor.y = 0f;
			break;
		case Anchor.RIGHT_XZ:
			unitAnchor.x = 1f;
			unitAnchor.z = 0.5f;
			unitAnchor.y = 0f;
			break;
		case Anchor.BOTTOM_LEFT_XZ:
			unitAnchor.x = 0f;
			unitAnchor.z = 0f;
			unitAnchor.y = 0f;
			break;
		case Anchor.BOTTOM_XZ:
			unitAnchor.x = 0.5f;
			unitAnchor.z = 0f;
			unitAnchor.y = 0f;
			break;
		case Anchor.BOTTOM_RIGHT_XZ:
			unitAnchor.x = 1f;
			unitAnchor.z = 0f;
			unitAnchor.y = 0f;
			break;
		case Anchor.FRONT_XZ:
			unitAnchor.x = 0.5f;
			unitAnchor.z = 0f;
			unitAnchor.y = 1f;
			break;
		case Anchor.BACK_XZ:
			unitAnchor.x = 0.5f;
			unitAnchor.z = 0f;
			unitAnchor.y = 0f;
			break;
		}
		return unitAnchor;
	}

	public static Vector3 ComputeWorldPoint(Bounds bounds, Vector3 selfUnitAnchor)
	{
		Vector3 worldPoint = default(Vector3);
		worldPoint.x = Mathf.Lerp(bounds.min.x, bounds.max.x, selfUnitAnchor.x);
		worldPoint.y = Mathf.Lerp(bounds.min.y, bounds.max.y, selfUnitAnchor.y);
		worldPoint.z = Mathf.Lerp(bounds.min.z, bounds.max.z, selfUnitAnchor.z);
		return worldPoint;
	}

	public static Bounds ComputeSetPointBounds(Component c)
	{
		return ComputeSetPointBounds(c.gameObject, includeInactive: false);
	}

	public static Bounds ComputeSetPointBounds(GameObject go)
	{
		return ComputeSetPointBounds(go, includeInactive: false);
	}

	public static Bounds ComputeSetPointBounds(Component c, bool includeInactive)
	{
		return ComputeSetPointBounds(c.gameObject, includeInactive);
	}

	public static Bounds ComputeSetPointBounds(GameObject go, bool includeInactive)
	{
		UberText uberText = go.GetComponent<UberText>();
		if (uberText != null)
		{
			return uberText.GetTextWorldSpaceBounds();
		}
		Renderer renderer = go.GetComponent<Renderer>();
		if (renderer != null)
		{
			return renderer.bounds;
		}
		BoundsOverride boundsOverride = go.GetComponent<BoundsOverride>();
		if (boundsOverride != null)
		{
			return boundsOverride.bounds;
		}
		CustomSelectiveMeshBoundsOverride customBoundsOverride = go.GetComponent<CustomSelectiveMeshBoundsOverride>();
		if (customBoundsOverride != null)
		{
			return customBoundsOverride.ComputeBounds(includeInactive);
		}
		Collider collider = go.GetComponent<Collider>();
		if (collider != null)
		{
			Bounds result;
			if (collider.enabled)
			{
				result = collider.bounds;
			}
			else
			{
				collider.enabled = true;
				result = collider.bounds;
				collider.enabled = false;
			}
			MobileHitBox hitBox = go.GetComponent<MobileHitBox>();
			if (hitBox != null && hitBox.HasExecuted())
			{
				result.size = new Vector3(result.size.x / hitBox.m_scaleX, result.size.y / hitBox.m_scaleY, result.size.z / hitBox.m_scaleY);
			}
			return result;
		}
		return GetBoundsOfChildren(go, includeInactive);
	}

	public static OrientedBounds ComputeOrientedWorldBounds(GameObject go, Vector3 minLocalPadding = default(Vector3), Vector3 maxLocalPadding = default(Vector3), List<GameObject> ignoreMeshes = null, bool includeAllChildren = true, bool includeMeshRenderers = true, bool includeWidgetTransformBounds = false, bool includeUberTextSize = false)
	{
		if (go == null || !go.activeSelf)
		{
			return null;
		}
		List<MeshFilter> allMeshes = null;
		if (includeMeshRenderers)
		{
			allMeshes = GetComponentsWithIgnore<MeshFilter>(go, ignoreMeshes, includeAllChildren);
		}
		List<WidgetTransform> allWidgetTransforms = null;
		if (includeWidgetTransformBounds)
		{
			allWidgetTransforms = GetComponentsWithIgnore<WidgetTransform>(go, ignoreMeshes, includeAllChildren);
		}
		List<UberText> allUberTexts = null;
		if (includeUberTextSize)
		{
			allUberTexts = GetComponentsWithIgnore<UberText>(go, ignoreMeshes, includeAllChildren);
		}
		return ComputeOrientedWorldBoundsWithComponents(go, minLocalPadding, maxLocalPadding, allMeshes, allWidgetTransforms, allUberTexts);
	}

	public static OrientedBounds ComputeOrientedWorldBoundsWithComponents(GameObject go, Vector3 minLocalPadding = default(Vector3), Vector3 maxLocalPadding = default(Vector3), List<MeshFilter> allMeshes = null, List<WidgetTransform> allWidgetTransforms = null, List<UberText> allUberTexts = null)
	{
		if (go == null || !go.activeSelf)
		{
			return null;
		}
		if ((allMeshes == null || allMeshes.Count == 0) && (allUberTexts == null || allUberTexts.Count == 0) && (allWidgetTransforms == null || allWidgetTransforms.Count == 0))
		{
			return null;
		}
		Matrix4x4 parentLocalMatrix = go.transform.worldToLocalMatrix;
		Vector3 min = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
		Vector3 max = new Vector3(float.MinValue, float.MinValue, float.MinValue);
		if (allMeshes != null)
		{
			foreach (MeshFilter mesh in allMeshes)
			{
				if (mesh.gameObject.activeSelf && !(mesh.sharedMesh == null))
				{
					Matrix4x4 meshTransform = mesh.transform.localToWorldMatrix;
					Bounds meshBounds = mesh.sharedMesh.bounds;
					Matrix4x4 meshLocalMatrix = parentLocalMatrix * meshTransform;
					Vector3[] meshLocalExtents = new Vector3[3]
					{
						meshLocalMatrix * new Vector3(meshBounds.extents.x, 0f, 0f),
						meshLocalMatrix * new Vector3(0f, meshBounds.extents.y, 0f),
						meshLocalMatrix * new Vector3(0f, 0f, meshBounds.extents.z)
					};
					Vector3 centerOffset = meshTransform * mesh.sharedMesh.bounds.center;
					GetBoundsMinMax(parentLocalMatrix * (mesh.transform.position + centerOffset), meshLocalExtents[0], meshLocalExtents[1], meshLocalExtents[2], ref min, ref max);
				}
			}
		}
		if (allWidgetTransforms != null)
		{
			foreach (WidgetTransform widgetTransform in allWidgetTransforms)
			{
				if (widgetTransform.gameObject.activeSelf)
				{
					Matrix4x4 meshTransform2 = widgetTransform.transform.localToWorldMatrix;
					Bounds bounds = WidgetTransform.GetLocalBoundsOfWidgetTransform(widgetTransform.transform);
					Matrix4x4 meshLocalMatrix2 = parentLocalMatrix * meshTransform2;
					Vector3[] meshLocalExtents2 = new Vector3[3]
					{
						meshLocalMatrix2 * new Vector3(bounds.extents.x, 0f, 0f),
						meshLocalMatrix2 * new Vector3(0f, bounds.extents.y, 0f),
						meshLocalMatrix2 * new Vector3(0f, 0f, bounds.extents.z)
					};
					Vector3 centerOffset2 = meshTransform2 * bounds.center;
					GetBoundsMinMax(parentLocalMatrix * (widgetTransform.transform.position + centerOffset2), meshLocalExtents2[0], meshLocalExtents2[1], meshLocalExtents2[2], ref min, ref max);
				}
			}
		}
		if (allUberTexts != null)
		{
			foreach (UberText uberText in allUberTexts)
			{
				if (uberText.gameObject.activeSelf)
				{
					Matrix4x4 meshTransform3 = uberText.transform.localToWorldMatrix;
					Bounds bounds2 = uberText.GetTextWorldSpaceBounds();
					Vector3 meshExtents = go.transform.InverseTransformVector(bounds2.extents);
					Matrix4x4 meshLocalMatrix3 = parentLocalMatrix * meshTransform3;
					Vector3[] meshLocalExtents3 = new Vector3[3]
					{
						meshLocalMatrix3 * new Vector3(meshExtents.x, 0f, 0f),
						meshLocalMatrix3 * new Vector3(0f, meshExtents.y, 0f),
						meshLocalMatrix3 * new Vector3(0f, 0f, meshExtents.z)
					};
					GetBoundsMinMax(parentLocalMatrix * uberText.transform.position, meshLocalExtents3[0], meshLocalExtents3[1], meshLocalExtents3[2], ref min, ref max);
				}
			}
		}
		if (minLocalPadding.sqrMagnitude > 0f)
		{
			min -= minLocalPadding;
		}
		if (maxLocalPadding.sqrMagnitude > 0f)
		{
			max += maxLocalPadding;
		}
		Matrix4x4 transform = go.transform.localToWorldMatrix;
		Matrix4x4 extentScaleRotate = transform;
		extentScaleRotate.SetColumn(3, Vector4.zero);
		Vector3 extents = (max - min) * 0.5f;
		Vector3 origin = (transform * max + transform * min) * 0.5f;
		OrientedBounds orientedBounds = new OrientedBounds();
		orientedBounds.Extents = new Vector3[3]
		{
			extentScaleRotate * new Vector3(extents.x, 0f, 0f),
			extentScaleRotate * new Vector3(0f, extents.y, 0f),
			extentScaleRotate * new Vector3(0f, 0f, extents.z)
		};
		orientedBounds.Origin = origin;
		orientedBounds.CenterOffset = go.transform.position - origin;
		return orientedBounds;
	}

	public static bool CanComputeOrientedWorldBoundsWithComponents(GameObject go, List<GameObject> ignoreObjects = null, List<MeshFilter> meshFilters = null, List<UberText> uberTexts = null, List<WidgetTransform> widgetTransforms = null)
	{
		if (go == null || !go.activeSelf)
		{
			return false;
		}
		if (meshFilters != null)
		{
			foreach (MeshFilter meshFilter in meshFilters)
			{
				if (!(meshFilter == null) && !ignoreObjects.Contains(meshFilter.gameObject))
				{
					return true;
				}
			}
		}
		if (uberTexts != null)
		{
			foreach (UberText uberText in uberTexts)
			{
				if (!(uberText == null) && !ignoreObjects.Contains(uberText.gameObject))
				{
					return true;
				}
			}
		}
		if (widgetTransforms != null)
		{
			foreach (WidgetTransform widgetTransform in widgetTransforms)
			{
				if (!(widgetTransform == null) && !ignoreObjects.Contains(widgetTransform.gameObject))
				{
					return true;
				}
			}
		}
		return false;
	}

	public static List<T> GetComponentsWithIgnore<T>(GameObject obj, List<GameObject> ignoreObjects, bool includeAllChildren = true) where T : Component
	{
		List<T> allComps = new List<T>();
		if (obj == null)
		{
			return allComps;
		}
		if (includeAllChildren)
		{
			obj.GetComponentsInChildren(allComps);
		}
		T thisObj = obj.GetComponent<T>();
		if (thisObj != null && !includeAllChildren)
		{
			allComps.Add(thisObj);
		}
		if (ignoreObjects != null && ignoreObjects.Count > 0)
		{
			T[] array = allComps.ToArray();
			allComps.Clear();
			T[] array2 = array;
			foreach (T comp in array2)
			{
				bool addComp = true;
				foreach (GameObject ignore in ignoreObjects)
				{
					if (ignore == null || comp.transform == ignore.transform || comp.transform.IsChildOf(ignore.transform))
					{
						addComp = false;
						break;
					}
				}
				if (addComp)
				{
					allComps.Add(comp);
				}
			}
		}
		return allComps;
	}

	public static Vector3[] GetBoundCorners(Vector3 origin, Vector3 xExtent, Vector3 yExtent, Vector3 zExtent)
	{
		Vector3 xmax = origin + xExtent;
		Vector3 xmin = origin - xExtent;
		Vector3 ymaxzmax = yExtent + zExtent;
		Vector3 ymaxzmin = yExtent - zExtent;
		Vector3 yminzmax = -yExtent + zExtent;
		Vector3 yminzmin = -yExtent - zExtent;
		return new Vector3[8]
		{
			xmax + ymaxzmax,
			xmax + ymaxzmin,
			xmax + yminzmax,
			xmax + yminzmin,
			xmin - ymaxzmax,
			xmin - ymaxzmin,
			xmin - yminzmax,
			xmin - yminzmin
		};
	}

	public static void GetBoundsMinMax(Vector3 origin, Vector3 xExtent, Vector3 yExtent, Vector3 zExtent, ref Vector3 min, ref Vector3 max)
	{
		Vector3[] corners = GetBoundCorners(origin, xExtent, yExtent, zExtent);
		for (int i = 0; i < corners.Length; i++)
		{
			min.x = Mathf.Min(corners[i].x, min.x);
			min.y = Mathf.Min(corners[i].y, min.y);
			min.z = Mathf.Min(corners[i].z, min.z);
			max.x = Mathf.Max(corners[i].x, max.x);
			max.y = Mathf.Max(corners[i].y, max.y);
			max.z = Mathf.Max(corners[i].z, max.z);
		}
	}

	public static void SetLocalScaleToWorldDimension(GameObject obj, params WorldDimensionIndex[] dimensions)
	{
		SetLocalScaleToWorldDimension(obj, null, dimensions);
	}

	public static void SetLocalScaleToWorldDimension(GameObject obj, List<GameObject> ignoreMeshes, params WorldDimensionIndex[] dimensions)
	{
		Vector3 newScale = obj.transform.localScale;
		OrientedBounds bounds = ComputeOrientedWorldBounds(obj, default(Vector3), default(Vector3), ignoreMeshes);
		for (int i = 0; i < dimensions.Length; i++)
		{
			float dims = bounds.Extents[dimensions[i].Index].magnitude * 2f;
			newScale[dimensions[i].Index] *= ((dims <= Mathf.Epsilon) ? 0.001f : (dimensions[i].Dimension / dims));
			if (Mathf.Abs(newScale[dimensions[i].Index]) < 0.001f)
			{
				newScale[dimensions[i].Index] = 0.001f;
			}
		}
		obj.transform.localScale = newScale;
	}

	public static void SetPoint(Component src, Anchor srcAnchor, Component dst, Anchor dstAnchor)
	{
		SetPoint(src.gameObject, GetUnitAnchor(srcAnchor), dst.gameObject, GetUnitAnchor(dstAnchor), Vector3.zero, includeInactive: false);
	}

	public static void SetPoint(Component src, Anchor srcAnchor, GameObject dst, Anchor dstAnchor)
	{
		SetPoint(src.gameObject, GetUnitAnchor(srcAnchor), dst, GetUnitAnchor(dstAnchor), Vector3.zero, includeInactive: false);
	}

	public static void SetPoint(GameObject src, Anchor srcAnchor, Component dst, Anchor dstAnchor)
	{
		SetPoint(src, GetUnitAnchor(srcAnchor), dst.gameObject, GetUnitAnchor(dstAnchor), Vector3.zero, includeInactive: false);
	}

	public static void SetPoint(GameObject src, Anchor srcAnchor, GameObject dst, Anchor dstAnchor)
	{
		SetPoint(src, GetUnitAnchor(srcAnchor), dst, GetUnitAnchor(dstAnchor), Vector3.zero, includeInactive: false);
	}

	public static void SetPoint(Component src, Anchor srcAnchor, Component dst, Anchor dstAnchor, Vector3 offset)
	{
		SetPoint(src.gameObject, GetUnitAnchor(srcAnchor), dst.gameObject, GetUnitAnchor(dstAnchor), offset, includeInactive: false);
	}

	public static void SetPoint(Component src, Anchor srcAnchor, GameObject dst, Anchor dstAnchor, Vector3 offset)
	{
		SetPoint(src.gameObject, GetUnitAnchor(srcAnchor), dst, GetUnitAnchor(dstAnchor), offset, includeInactive: false);
	}

	public static void SetPoint(GameObject src, Anchor srcAnchor, Component dst, Anchor dstAnchor, Vector3 offset)
	{
		SetPoint(src, GetUnitAnchor(srcAnchor), dst.gameObject, GetUnitAnchor(dstAnchor), offset, includeInactive: false);
	}

	public static void SetPoint(GameObject src, Anchor srcAnchor, GameObject dst, Anchor dstAnchor, Vector3 offset)
	{
		SetPoint(src, GetUnitAnchor(srcAnchor), dst, GetUnitAnchor(dstAnchor), offset, includeInactive: false);
	}

	public static void SetPoint(GameObject src, Anchor srcAnchor, GameObject dst, Anchor dstAnchor, Vector3 offset, bool includeInactive)
	{
		SetPoint(src, GetUnitAnchor(srcAnchor), dst, GetUnitAnchor(dstAnchor), offset, includeInactive);
	}

	public static void SetPoint(Component self, Vector3 selfUnitAnchor, GameObject relative, Vector3 relativeUnitAnchor)
	{
		SetPoint(self.gameObject, selfUnitAnchor, relative, relativeUnitAnchor, Vector3.zero, includeInactive: false);
	}

	public static void SetPoint(GameObject self, Vector3 selfUnitAnchor, GameObject relative, Vector3 relativeUnitAnchor)
	{
		SetPoint(self, selfUnitAnchor, relative, relativeUnitAnchor, Vector3.zero, includeInactive: false);
	}

	public static void SetPoint(Component self, Vector3 selfUnitAnchor, Component relative, Vector3 relativeUnitAnchor, Vector3 offset)
	{
		SetPoint(self.gameObject, selfUnitAnchor, relative.gameObject, relativeUnitAnchor, offset, includeInactive: false);
	}

	public static void SetPoint(Component self, Vector3 selfUnitAnchor, GameObject relative, Vector3 relativeUnitAnchor, Vector3 offset)
	{
		SetPoint(self.gameObject, selfUnitAnchor, relative, relativeUnitAnchor, offset, includeInactive: false);
	}

	public static void SetPoint(GameObject self, Vector3 selfUnitAnchor, GameObject relative, Vector3 relativeUnitAnchor, Vector3 offset)
	{
		SetPoint(self, selfUnitAnchor, relative, relativeUnitAnchor, offset, includeInactive: false);
	}

	public static void SetPoint(GameObject self, Vector3 selfUnitAnchor, GameObject relative, Vector3 relativeUnitAnchor, Vector3 offset, bool includeInactive)
	{
		if ((bool)self && (bool)relative)
		{
			Bounds bounds = ComputeSetPointBounds(self, includeInactive);
			Bounds relativeBounds = ComputeSetPointBounds(relative, includeInactive);
			Vector3 selfPoint = ComputeWorldPoint(bounds, selfUnitAnchor);
			Vector3 relativePoint = ComputeWorldPoint(relativeBounds, relativeUnitAnchor);
			Vector3 relativeDelta = new Vector3(relativePoint.x - selfPoint.x + offset.x, relativePoint.y - selfPoint.y + offset.y, relativePoint.z - selfPoint.z + offset.z);
			self.transform.Translate(relativeDelta, Space.World);
		}
	}

	public static Bounds GetBoundsOfChildren(Component c)
	{
		return GetBoundsOfChildren(c.gameObject, includeInactive: false);
	}

	public static Bounds GetBoundsOfChildren(GameObject go)
	{
		return GetBoundsOfChildren(go, includeInactive: false);
	}

	public static Bounds GetBoundsOfChildren(GameObject go, bool includeInactive)
	{
		Renderer[] renderers = go.GetComponentsInChildren<Renderer>(includeInactive);
		if (renderers.Length == 0)
		{
			return new Bounds(go.transform.position, Vector3.zero);
		}
		Bounds compositeBounds = renderers[0].bounds;
		for (int i = 1; i < renderers.Length; i++)
		{
			Bounds bounds = renderers[i].bounds;
			Vector3 maxPoint = Vector3.Max(bounds.max, compositeBounds.max);
			Vector3 minPoint = Vector3.Min(bounds.min, compositeBounds.min);
			compositeBounds.SetMinMax(minPoint, maxPoint);
		}
		return compositeBounds;
	}

	public static void SetLocalPosX(GameObject go, float x)
	{
		Transform t = go.transform;
		t.localPosition = new Vector3(x, t.localPosition.y, t.localPosition.z);
	}

	public static void SetLocalPosX(Component component, float x)
	{
		Transform t = component.transform;
		t.localPosition = new Vector3(x, t.localPosition.y, t.localPosition.z);
	}

	public static void SetLocalPosY(GameObject go, float y)
	{
		Transform t = go.transform;
		t.localPosition = new Vector3(t.localPosition.x, y, t.localPosition.z);
	}

	public static void SetLocalPosY(Component component, float y)
	{
		Transform t = component.transform;
		t.localPosition = new Vector3(t.localPosition.x, y, t.localPosition.z);
	}

	public static void SetLocalPosZ(GameObject go, float z)
	{
		Transform t = go.transform;
		t.localPosition = new Vector3(t.localPosition.x, t.localPosition.y, z);
	}

	public static void SetLocalPosZ(Component component, float z)
	{
		Transform t = component.transform;
		t.localPosition = new Vector3(t.localPosition.x, t.localPosition.y, z);
	}

	public static void SetPosX(GameObject go, float x)
	{
		Transform t = go.transform;
		t.position = new Vector3(x, t.position.y, t.position.z);
	}

	public static void SetPosX(Component component, float x)
	{
		Transform t = component.transform;
		t.position = new Vector3(x, t.position.y, t.position.z);
	}

	public static void SetPosY(GameObject go, float y)
	{
		Transform t = go.transform;
		t.position = new Vector3(t.position.x, y, t.position.z);
	}

	public static void SetPosY(Component component, float y)
	{
		Transform t = component.transform;
		t.position = new Vector3(t.position.x, y, t.position.z);
	}

	public static void SetPosZ(GameObject go, float z)
	{
		Transform t = go.transform;
		t.position = new Vector3(t.position.x, t.position.y, z);
	}

	public static void SetPosZ(Component component, float z)
	{
		Transform t = component.transform;
		t.position = new Vector3(t.position.x, t.position.y, z);
	}

	public static void SetLocalEulerAngleX(GameObject go, float x)
	{
		Transform t = go.transform;
		t.localEulerAngles = new Vector3(x, t.localEulerAngles.y, t.localEulerAngles.z);
	}

	public static void SetLocalEulerAngleY(GameObject go, float y)
	{
		Transform t = go.transform;
		t.localEulerAngles = new Vector3(t.localEulerAngles.x, y, t.localEulerAngles.z);
	}

	public static void SetEulerAngleX(GameObject go, float x)
	{
		Transform t = go.transform;
		t.eulerAngles = new Vector3(x, t.eulerAngles.y, t.eulerAngles.z);
	}

	public static void SetEulerAngleY(GameObject go, float y)
	{
		Transform t = go.transform;
		t.eulerAngles = new Vector3(t.eulerAngles.x, y, t.eulerAngles.z);
	}

	public static void SetEulerAngleZ(GameObject go, float z)
	{
		Transform t = go.transform;
		t.eulerAngles = new Vector3(t.eulerAngles.x, t.eulerAngles.y, z);
	}

	public static void SetLocalScaleX(Component component, float x)
	{
		Transform t = component.transform;
		t.localScale = new Vector3(x, t.localScale.y, t.localScale.z);
	}

	public static void SetLocalScaleX(GameObject go, float x)
	{
		Transform t = go.transform;
		t.localScale = new Vector3(x, t.localScale.y, t.localScale.z);
	}

	public static void SetLocalScaleY(GameObject go, float y)
	{
		Transform t = go.transform;
		t.localScale = new Vector3(t.localScale.x, y, t.localScale.z);
	}

	public static void SetLocalScaleZ(Component component, float z)
	{
		Transform t = component.transform;
		t.localScale = new Vector3(t.localScale.x, t.localScale.y, z);
	}

	public static void SetLocalScaleZ(GameObject go, float z)
	{
		Transform t = go.transform;
		t.localScale = new Vector3(t.localScale.x, t.localScale.y, z);
	}

	public static void SetLocalScaleXY(GameObject go, float x, float y)
	{
		Transform t = go.transform;
		t.localScale = new Vector3(x, y, t.localScale.z);
	}

	public static void SetLocalScaleXY(Component component, Vector2 v)
	{
		Transform t = component.transform;
		t.localScale = new Vector3(v.x, v.y, t.localScale.z);
	}

	public static void SetLocalScaleXZ(GameObject go, Vector2 v)
	{
		Transform t = go.transform;
		t.localScale = new Vector3(v.x, t.localScale.y, v.y);
	}

	public static void Identity(Component c)
	{
		c.transform.localScale = Vector3.one;
		c.transform.localRotation = Quaternion.identity;
		c.transform.localPosition = Vector3.zero;
	}

	public static void Identity(GameObject go)
	{
		go.transform.localScale = Vector3.one;
		go.transform.localRotation = Quaternion.identity;
		go.transform.localPosition = Vector3.zero;
	}

	public static void CopyLocal(Component destination, Component source)
	{
		CopyLocal(destination.gameObject, source.gameObject);
	}

	public static void CopyLocal(GameObject destination, Component source)
	{
		CopyLocal(destination, source.gameObject);
	}

	public static void CopyLocal(GameObject destination, GameObject source)
	{
		destination.transform.localScale = source.transform.localScale;
		destination.transform.localRotation = source.transform.localRotation;
		destination.transform.localPosition = source.transform.localPosition;
	}

	public static void CopyLocal(Component destination, TransformProps source)
	{
		CopyLocal(destination.gameObject, source);
	}

	public static void CopyLocal(GameObject destination, TransformProps source)
	{
		destination.transform.localScale = source.scale;
		destination.transform.localRotation = source.rotation;
		destination.transform.localPosition = source.position;
	}

	public static TransformProps GetLocalTransformProps(Component source)
	{
		return GetLocalTransformProps(source.gameObject);
	}

	public static TransformProps GetLocalTransformProps(GameObject source)
	{
		TransformProps result = default(TransformProps);
		result.scale = source.transform.localScale;
		result.rotation = source.transform.localRotation;
		result.position = source.transform.localPosition;
		return result;
	}

	public static void CopyWorld(Component destination, Component source)
	{
		if (destination != null)
		{
			CopyWorld(destination.gameObject, source);
		}
	}

	public static void CopyWorld(Component destination, GameObject source)
	{
		if (destination != null)
		{
			CopyWorld(destination.gameObject, source);
		}
	}

	public static void CopyWorld(GameObject destination, Component source)
	{
		if (source != null)
		{
			CopyWorld(destination, source.gameObject);
		}
	}

	public static void CopyWorld(GameObject destination, GameObject source)
	{
		CopyWorldScale(destination, source);
		destination.transform.rotation = source.transform.rotation;
		destination.transform.position = source.transform.position;
	}

	public static void CopyWorld(Component destination, TransformProps source)
	{
		CopyWorld(destination.gameObject, source);
	}

	public static void CopyWorld(GameObject destination, TransformProps source)
	{
		SetWorldScale(destination, source.scale);
		destination.transform.rotation = source.rotation;
		destination.transform.position = source.position;
	}

	public static TransformProps GetWorldTransformProps(Component source)
	{
		return GetWorldTransformProps(source.gameObject);
	}

	public static TransformProps GetWorldTransformProps(GameObject source)
	{
		TransformProps result = default(TransformProps);
		result.scale = ComputeWorldScale(source);
		result.rotation = source.transform.rotation;
		result.position = source.transform.position;
		return result;
	}

	public static void CopyWorldScale(Component destination, Component source)
	{
		CopyWorldScale(destination.gameObject, source.gameObject);
	}

	public static void CopyWorldScale(GameObject destination, GameObject source)
	{
		Vector3 scale = ComputeWorldScale(source);
		SetWorldScale(destination, scale);
	}

	public static void SetWorldScale(Component destination, Vector3 scale)
	{
		SetWorldScale(destination.gameObject, scale);
	}

	public static void SetWorldScale(GameObject destination, Vector3 scale)
	{
		if (destination.transform.parent != null)
		{
			Transform t = destination.transform.parent;
			while (t != null)
			{
				scale.Scale(Vector3Reciprocal(t.localScale));
				t = t.parent;
			}
		}
		destination.transform.localScale = scale;
	}

	public static Vector3 ComputeWorldScale(Component c)
	{
		return ComputeWorldScale(c.gameObject);
	}

	public static Vector3 ComputeWorldScale(GameObject go)
	{
		Vector3 scale = go.transform.localScale;
		if (go.transform.parent != null)
		{
			Transform currTransform = go.transform.parent;
			while (currTransform != null)
			{
				scale.Scale(currTransform.localScale);
				currTransform = currTransform.parent;
			}
		}
		return scale;
	}

	public static Vector3 Vector3Reciprocal(Vector3 source)
	{
		Vector3 destination = source;
		if (destination.x != 0f)
		{
			destination.x = 1f / destination.x;
		}
		if (destination.y != 0f)
		{
			destination.y = 1f / destination.y;
		}
		if (destination.z != 0f)
		{
			destination.z = 1f / destination.z;
		}
		return destination;
	}

	public static Vector3 RandomVector3(Vector3 min, Vector3 max)
	{
		Vector3 v = default(Vector3);
		v.x = UnityEngine.Random.Range(min.x, max.x);
		v.y = UnityEngine.Random.Range(min.y, max.y);
		v.z = UnityEngine.Random.Range(min.z, max.z);
		return v;
	}

	public static void AttachAndPreserveLocalTransform(Transform child, Transform parent)
	{
		TransformProps copy = GetLocalTransformProps(child);
		child.parent = parent;
		CopyLocal(child, copy);
	}

	public static float GetAspectRatioValue(PhoneAspectRatio aspectRatio)
	{
		return aspectRatio switch
		{
			PhoneAspectRatio.Minimum => 1.5f, 
			PhoneAspectRatio.Wide => 1.7777778f, 
			PhoneAspectRatio.ExtraWide => 2.04f, 
			_ => 0f, 
		};
	}

	public static Vector3 GetAspectRatioDependentPosition(Vector3 aspectSmall, Vector3 aspectWide, Vector3 aspectExtraWide)
	{
		return GetAspectRatioDependentValue(Vector3.Lerp, aspectSmall, aspectWide, aspectExtraWide);
	}

	public static float GetAspectRatioDependentValue(float aspectSmall, float aspectWide, float aspectExtraWide)
	{
		return GetAspectRatioDependentValue(Mathf.Lerp, aspectSmall, aspectWide, aspectExtraWide);
	}

	private static T GetAspectRatioDependentValue<T>(Func<T, T, float, T> interpolator, T small, T wide, T extraWide)
	{
		Dictionary<PhoneAspectRatio, T> vals = new Dictionary<PhoneAspectRatio, T>
		{
			{
				PhoneAspectRatio.Minimum,
				small
			},
			{
				PhoneAspectRatio.Wide,
				wide
			},
			{
				PhoneAspectRatio.ExtraWide,
				extraWide
			}
		};
		PhoneAspectRatio lower;
		PhoneAspectRatio upper;
		float r = PhoneAspectRatioScale(out lower, out upper);
		return interpolator(vals[lower], vals[upper], r);
	}

	public static bool IsExtraWideAspectRatio()
	{
		return GetAspectRatioDependentValue(0f, 1f, 2f) > 1.2f;
	}

	private static float PhoneAspectRatioScale(out PhoneAspectRatio lowerRatio, out PhoneAspectRatio upperRatio)
	{
		float aspectRatio = (float)Screen.width / (float)Screen.height;
		lowerRatio = PhoneAspectRatio.Minimum;
		upperRatio = PhoneAspectRatio.ExtraWide;
		int numSupportedAspectRatios = EnumUtils.Length<PhoneAspectRatio>();
		for (int i = 0; i < numSupportedAspectRatios; i++)
		{
			PhoneAspectRatio ratio = (PhoneAspectRatio)i;
			if (GetAspectRatioValue(ratio) > aspectRatio)
			{
				lowerRatio = ((i > 0) ? ((PhoneAspectRatio)(i - 1)) : PhoneAspectRatio.Minimum);
				upperRatio = ((i == 0) ? ((PhoneAspectRatio)(i + 1)) : ratio);
				break;
			}
		}
		float lowerRatioValue = GetAspectRatioValue(lowerRatio);
		float upperRatioValue = GetAspectRatioValue(upperRatio);
		float aspectRange = upperRatioValue - lowerRatioValue;
		aspectRatio = Mathf.Clamp(aspectRatio, lowerRatioValue, upperRatioValue);
		return (aspectRatio - lowerRatioValue) / aspectRange;
	}

	public static void ConstrainToScreen(GameObject go, int layer)
	{
		Camera camera = CameraUtils.FindFirstByLayer(layer);
		if (camera == null)
		{
			Log.All.PrintError("TransformUtil.ConstrainToScreen - No camera found for indicated layer.");
			return;
		}
		Bounds goBounds = ComputeSetPointBounds(go);
		Vector3[] screenCorners = new Vector3[4];
		camera.CalculateFrustumCorners(new Rect(0f, 0f, 1f, 1f), camera.farClipPlane, Camera.MonoOrStereoscopicEye.Mono, screenCorners);
		Bounds safeBounds = new Bounds(camera.transform.TransformPoint(screenCorners[0]), default(Vector3));
		for (int i = 1; i < 4; i++)
		{
			Vector3 worldCorner = camera.transform.TransformPoint(screenCorners[i]);
			safeBounds.Encapsulate(worldCorner);
		}
		Vector3 goPos = go.transform.position;
		safeBounds.SetMinMax(safeBounds.min - (goBounds.min - goPos), safeBounds.max - (goBounds.max - goPos));
		Vector3 correction = safeBounds.ClosestPoint(goPos) - goPos;
		correction -= camera.transform.forward * Vector3.Dot(camera.transform.forward, correction);
		go.transform.position += correction;
	}
}
