using UnityEngine;
using System.Collections;

/// <summary>
/// Star/Planet/Moon
/// </summary>
[RequireComponent(typeof(GravitySource))]
public class CelestialObject : MonoBehaviour
{
	private const float SHATTER_ENERGY_THRESHOLD = 2f;
	private const float DESTROY_ENERGY_THRESHOLD = 1000f;

	[SerializeField]
	private float spin;

	[SerializeField]
	private bool destructible;

	private GravitySource gravitySource;
	private bool shattered;

	void OnCollision2DEnter(Collision2D collision)
	{
		if (destructible)
		{
			float relativeMass = collision.rigidbody.mass / gravitySource.Mass;
			float relativeSpeedSquared = collision.relativeVelocity.sqrMagnitude;
			float kineticEnergy = 0.5f * relativeMass * relativeSpeedSquared;

			if (kineticEnergy > DESTROY_ENERGY_THRESHOLD)
			{

			}
			else if (!shattered && kineticEnergy > SHATTER_ENERGY_THRESHOLD)
			{
				StartCoroutine(RunShatterAnimation(kineticEnergy));
			}
		}
	}

	void Update()
	{
		if (spin != 0)
			transform.Rotate (0f, spin * Time.deltaTime, 0f, Space.Self);
	}

	private IEnumerator RunShatterAnimation(float energy)
	{
		// Prevent further shattering.
		shattered = true;

		yield return new WaitForEndOfFrame();
	}
}
