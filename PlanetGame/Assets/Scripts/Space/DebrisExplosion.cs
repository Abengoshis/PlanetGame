using UnityEngine;
using System.Collections;

public class DebrisExplosion : Explosion
{
	private Renderer[] pieces;
	private Vector3[] directions;

	void Awake ()
	{
		pieces = GetComponentsInChildren<Renderer>();
		directions = new Vector3[pieces.Length];

		for (int i = 0; i < pieces.Length; ++i)
		{
			directions[i] = (pieces[i].bounds.center - transform.position).normalized;
		}

		Explode();
	}

	public void SetMaterial(Material material)
	{
		foreach (Renderer r in pieces)
			r.material = material;
	}

	protected override void Explode ()
	{
		StartCoroutine(Animate());
	}

	IEnumerator Animate()
	{
		float distance = 0f;
		float duration = 2f + Random.Range (0f, 1f);
		float finalDistance = transform.localScale.x;
		for (float time = 0f, t = 0f; time < duration; time += Time.deltaTime, t = time / duration)
		{
			for (int i = 0; i < pieces.Length; ++i)
			{
				pieces[i].transform.localPosition = directions[i] * distance;
			}

			distance = Mathf.Lerp (0f, finalDistance, Mathf.Sin (t * Mathf.PI * 0.5f));
			yield return new WaitForEndOfFrame();
		}
	}
}
