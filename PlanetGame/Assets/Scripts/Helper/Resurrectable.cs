using UnityEngine;
using UnityEngine.Events;
using System.Collections;
using System.Collections.Generic;

public class Resurrectable : MonoBehaviour
{
	private static HashSet<Resurrectable> inactive = new HashSet<Resurrectable>();

	[SerializeField]
	private UnityEvent OnResurrect;

	public static void ResurrectAll()
	{
		HashSet<Resurrectable> temp = new HashSet<Resurrectable>(inactive);
		foreach (Resurrectable r in temp)
		{
			r.OnEnable();
		}
	}

	void OnDisable()
	{
		gameObject.SetActive(false);
		inactive.Add (this);
	}

	void OnEnable()
	{
		gameObject.SetActive(true);
		inactive.Remove(this);
		OnResurrect.Invoke();
	}
}
