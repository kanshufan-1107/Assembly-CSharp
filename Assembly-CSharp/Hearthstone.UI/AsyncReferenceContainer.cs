using System.Collections.Generic;
using UnityEngine;

namespace Hearthstone.UI;

public class AsyncReferenceContainer<T> : AsyncReferenceContainerBase where T : Object
{
	public bool AddReferencesAndResolve(List<AsyncReference> referencesToAdd)
	{
		if (referencesToAdd == null)
		{
			return false;
		}
		return AddReferencesAndResolve(referencesToAdd.ToArray());
	}

	public bool AddReferencesAndResolve(AsyncReference[] referencesToAdd)
	{
		if (m_waitingOnReferences || referencesToAdd == null || referencesToAdd.Length == 0)
		{
			return false;
		}
		bool allSuccess = true;
		foreach (AsyncReference reference in referencesToAdd)
		{
			if (!AddReferenceAndResolve(reference))
			{
				allSuccess = false;
			}
		}
		if (!allSuccess)
		{
			Log.UIFramework.PrintWarning("Some async references were null when trying to resolve a array/list of them.");
		}
		return allSuccess;
	}

	public bool AddReferenceAndResolve(AsyncReference referenceToAdd)
	{
		if (referenceToAdd == null)
		{
			return false;
		}
		m_asyncReferenceSet.Add(referenceToAdd);
		referenceToAdd.RegisterSelfReadyListener<T>(base.OnReferenceReady);
		return true;
	}

	public T GetObject(AsyncReference referenceToGetObjectFor)
	{
		if (referenceToGetObjectFor == null || !referenceToGetObjectFor.IsReady)
		{
			return null;
		}
		return referenceToGetObjectFor.Object as T;
	}

	public List<T> GetObjects()
	{
		List<T> asyncObjects = new List<T>();
		if (GetIsLoaded())
		{
			foreach (AsyncReference reference in m_asyncReferenceSet)
			{
				asyncObjects.Add(GetObject(reference));
			}
		}
		return asyncObjects;
	}
}
public class AsyncReferenceContainer : AsyncReferenceContainerBase
{
	public bool AddReferenceAndResolve<T>(AsyncReference referenceToAdd) where T : Object
	{
		if (m_asyncReferenceSet.Contains(referenceToAdd) || m_waitingOnReferences)
		{
			return false;
		}
		m_asyncReferenceSet.Add(referenceToAdd);
		referenceToAdd.RegisterSelfReadyListener<T>(base.OnReferenceReady);
		return true;
	}

	public T GetObject<T>(AsyncReference referenceToGetObjectFor) where T : Object
	{
		if (referenceToGetObjectFor == null || !referenceToGetObjectFor.IsReady)
		{
			return null;
		}
		return referenceToGetObjectFor.Object as T;
	}
}
