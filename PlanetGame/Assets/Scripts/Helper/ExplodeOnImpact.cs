using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(Collider2D))]
public class ExplodeOnImpact : MonoBehaviour
{
	[SerializeField]
	private GameObject explosionPrefab;

	[SerializeField]
	private float energyNeeded;

	void OnCollisionEnter2D(Collision2D collision)
	{
		float mass;
		if (collision.rigidbody != null)
			mass = collision.rigidbody.mass;
		else if (collision.gameObject.GetComponent<GravitySource>() != null)
			mass = collision.gameObject.GetComponent<GravitySource>().Mass;
		else
			mass = float.MaxValue;

		float kineticEnergy = 0.5f * mass * collision.relativeVelocity.sqrMagnitude;
		if (kineticEnergy > energyNeeded)
		{
			// Create the explosion and set its position, rotation and scale to this object's position, rotation and scale.
			GameObject spawned = Instantiate<GameObject>(explosionPrefab);
			spawned.transform.SetParent(transform.parent);
			spawned.transform.localPosition = transform.localPosition;
			spawned.transform.localRotation = transform.localRotation;
			spawned.transform.localScale = transform.localScale;

			Explosion explosion = spawned.GetComponent<Explosion>();

			if (explosion.GetType() == typeof(DebrisExplosion))
				((DebrisExplosion)explosion).SetMaterial(GetComponent<Renderer>().sharedMaterial);

			if (GetComponent<Resurrectable>() != null)
			{
				// Hide this object.
				gameObject.SetActive(false);
			}
			else
			{
				// Destroy this object.
				Destroy (gameObject);
			}
		}
	}
}
