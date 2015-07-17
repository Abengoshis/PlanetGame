using UnityEngine;
using System.Collections;

public class Atmosphere : MonoBehaviour
{
	[SerializeField]
	private Color colour = Color.white;

	[SerializeField]
	[Range(0f, 1f)]
	private float thickness;

	void Start()
	{
		GetComponent<Renderer>().material.color = colour;
	}

	void OnTrigger2DEnter(Collider2D other)
	{
		other.attachedRigidbody.velocity = other.attachedRigidbody.velocity * (1f - thickness);
	}
}
