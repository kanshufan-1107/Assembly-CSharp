using UnityEngine;

public class PlayOnce : MonoBehaviour
{
	public string notes;

	public string notes2;

	public GameObject tester;

	public string testerAnim;

	public GameObject tester2;

	public string tester2Anim;

	public GameObject tester3;

	public string tester3Anim;

	private void Start()
	{
		if (tester != null)
		{
			tester.SetActive(value: false);
		}
		if (tester2 != null)
		{
			tester2.SetActive(value: false);
		}
		if (tester3 != null)
		{
			tester3.SetActive(value: false);
		}
	}

	private void OnGUI()
	{
		if (Event.current.isKey)
		{
			if (tester != null)
			{
				tester.SetActive(value: true);
				Animation component = tester.GetComponent<Animation>();
				component.Stop(testerAnim);
				component.Play(testerAnim);
			}
			else
			{
				Debug.Log("NO 'tester' object.");
			}
			if (tester2 != null)
			{
				tester2.SetActive(value: true);
				Animation component2 = tester2.GetComponent<Animation>();
				component2.Stop(tester2Anim);
				component2.Play(tester2Anim);
			}
			else
			{
				Debug.Log("NO 'tester2' object.");
			}
			if (tester3 != null)
			{
				tester3.SetActive(value: true);
				Animation component3 = tester3.GetComponent<Animation>();
				component3.Stop(tester3Anim);
				component3.Play(tester3Anim);
			}
			else
			{
				Debug.Log("NO 'tester3' object.");
			}
		}
	}
}
