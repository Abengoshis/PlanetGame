using UnityEngine;
using System.Collections;

[RequireComponent(typeof(CircleCollider2D))]
public class Atmosphere : MonoBehaviour
{
	[SerializeField]
	[Range(0f, 1f)]
	private float thickness;

	void OnTrigger2DEnter(Collider2D other)
	{
		other.attachedRigidbody.velocity = other.attachedRigidbody.velocity * (1f - thickness);
	}
}
