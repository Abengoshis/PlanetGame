using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GravitySource : MonoBehaviour
{
	private const float GRAVITY_SCALE = 200f;
	private static HashSet<GravitySource> gravitySources = new HashSet<GravitySource>();

	[SerializeField]
	private float mass;
	public float Mass
	{
		get { return mass; }
	}

	void Awake()
	{
		Rigidbody2D rigidbody2D = GetComponent<Rigidbody2D>();
		if (rigidbody2D != null)
			rigidbody2D.mass = mass;
	}

	void OnEnable()
	{
		gravitySources.Add (this);
	}
	
	void OnDisable()
	{
		gravitySources.Remove(this);
	}

	/// <summary>
	/// Calculates the total force of gravity at the given position for the given mass.
	/// </summary>
	public static Vector2 CalculateForceAtPosition(Vector2 position, float mass)
	{
		Vector2 force = Vector2.zero;
		foreach (GravitySource source in gravitySources)
		{
			Vector2 delta = (Vector2)source.transform.position - position;
			if (delta.sqrMagnitude != 0)
			{
				float magnitude = GRAVITY_SCALE * mass * source.mass / delta.sqrMagnitude;
				force += delta * magnitude;
			}
		}
		return force;
	}
}
