using UnityEngine;
using System.Collections;

public class Spin : MonoBehaviour
{
	[SerializeField]
	private Vector3 axis;

	[SerializeField]
	private float spin;
	
	void Update ()
	{
		transform.Rotate(axis, spin * Time.deltaTime);
	}
}
