using UnityEngine;
using System.Collections;
using System.Collections.Generic;

// Can anyone reading this think of any reason why tracking components like this (and Resurrectable) is a bad idea?
public class Hazardous : MonoBehaviour
{
	private static HashSet<Hazardous> hazards = new HashSet<Hazardous>();

	public static int NumHazards
	{
		get { return hazards.Count; }
	}

	void OnDisable()
	{
		hazards.Remove (this);
	}
	
	void OnEnable()
	{
		hazards.Add (this);
	}
}
