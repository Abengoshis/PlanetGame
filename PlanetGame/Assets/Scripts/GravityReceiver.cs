using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(Rigidbody2D))]
public class GravityReceiver : MonoBehaviour
{
	private Rigidbody2D rigidbody2D;
	private Vector2 gravity;

	void Awake()
	{
		rigidbody2D = GetComponent<Rigidbody2D>();
	}

	void Update()
	{
		// Get the total force of gravity acting on this object.
		gravity = GravitySource.CalculateForceAtPosition(transform.position, rigidbody2D.mass);
	}

	void FixedUpdate()
	{
		// Apply the gravity over time.
		rigidbody2D.AddForce (gravity * Time.fixedDeltaTime);
	}
}