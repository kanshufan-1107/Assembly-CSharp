using System;
using System.Collections;
using UnityEngine;

public class MockGIServer
{
	private string[] personalizedmessageIDs;

	public MockGIServer()
	{
		personalizedmessageIDs = new string[3] { "12234455", "23123332", "12320948" };
	}

	public IEnumerator GetMessages(Action<string[]> OnDone)
	{
		yield return new WaitForSeconds(0.1f);
		OnDone?.Invoke(personalizedmessageIDs);
	}
}
