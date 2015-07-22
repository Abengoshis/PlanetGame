using UnityEngine;
using System.Collections;

public class WhiteHole : MonoBehaviour 
{
	public const float MAX_SPAWN_DELAY = 10f;

	[System.Serializable]
	private struct Ejecta
	{
		[Range(0f, MAX_SPAWN_DELAY)]
		public float time;
		public GameObject prefab;
		public Vector2 relativeVelocity;
	}

	[SerializeField]
	private Ejecta[] ejecta;

	void Awake()
	{
		// Sort ejecta by time.
		for (int i = 0; i < ejecta.Length; ++i)
		{
			for (int j = i + 1; j < ejecta.Length; ++j)
			{
				if (ejecta[j].time < ejecta[i].time)
				{
					Ejecta temp = ejecta[j];
					ejecta[j] = ejecta[i];
					ejecta[i] = temp;
				}
			}
		}
	}

	public void Spawn()
	{
		StartCoroutine(SpawnCoroutine());
	}

	private IEnumerator SpawnCoroutine()
	{
		float time = 0f;
		foreach (Ejecta e in ejecta)
		{
			// Wait for the required time before the ejecta is spawned.
			float timeToWait = e.time - time;
			if (timeToWait > 0)
			{
				yield return new WaitForSeconds(timeToWait);
			}
			time += timeToWait;

			// Spawn the ejecta.
			GameObject spawned = Instantiate<GameObject>(e.prefab);
			spawned.transform.position = transform.position;

			// Add velocity if possible.
			Rigidbody2D rigidbody = spawned.GetComponent<Rigidbody2D>();
			if (rigidbody != null)
			{
				rigidbody.AddForce(transform.TransformVector(e.relativeVelocity) * rigidbody.mass, ForceMode2D.Impulse);
			}
		}
	}

	void OnDrawGizmos()
	{
		Gizmos.color = Color.red;

		foreach (Ejecta e in ejecta)
		{
			Vector2 end = (Vector2)(transform.position + transform.TransformVector(e.relativeVelocity));
			CustomGizmos.DrawArrow(transform.position, end);
		}
	}
}
