using UnityEngine;
using System.Collections;

[RequireComponent(typeof(GravityReceiver))]
public class Asteroid : MonoBehaviour
{
	public float initialSpeed = 0;
	private Rigidbody2D rigidbody2D;

	void Start ()
	{
		rigidbody2D = GetComponent<Rigidbody2D>();
		rigidbody2D.AddRelativeForce(new Vector2(0f, initialSpeed), ForceMode2D.Impulse);
	}

	void OnCollisionEnter(Collision collision)
	{
		// Check if colliding with a planet/star/moon.
		if (collision.gameObject.GetComponent<CelestialObject>())
		{
			Destroy (gameObject);
		}
	}

	void OnDrawGizmos()
	{
		Gizmos.color = Color.red;

		Vector2 end;
		if (Application.isPlaying)
			end = (Vector2)transform.position + rigidbody2D.velocity;
		else
			end = (Vector2)(transform.position + transform.up * initialSpeed);

		CustomGizmos.DrawArrow(transform.position, end);
	}
}
