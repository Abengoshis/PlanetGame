using UnityEngine;
using System.Collections;

/// <summary>
/// This all needs re-doing, probably. It's not proper code, its a mess.
/// </summary>
public class RockyAppearanceGenerator : AppearanceGenerator
{
	[SerializeField]
	private Color baseColour = Color.white;

	[SerializeField]
	private Color southColour =Color.white;

	[SerializeField]
	private Color capColour = Color.white;

	[SerializeField]
	[Range(0f, 0.5f)]
	private float capSize = 0.1f;

	[SerializeField]
	private int capSeed = 0;

	[SerializeField]
	[Range(0f, 1f)]
	private float capCoarseness = 0.01f;

	[SerializeField]
	[Range(0f, 1f)]
	private float noiseIntensity = 0.5f;

	[SerializeField]
	private int noiseSeed = 0;
	
	[SerializeField]
	[Range(0f, 1f)]
	private float noiseCoarseness = 0.1f;

	[SerializeField]
	private int strataSeed = 0;

	[SerializeField]
	[Range(0f, 1f)]
	private float strataCoarseness = 0.01f;

	[System.Serializable]
	private struct Strata : System.IComparable<Strata>
	{
		[Range(0f, 1f)]
		public float depth;
		public Color colour;

		// Default descending sort by height.
		int System.IComparable<Strata>.CompareTo (Strata other)
		{
			if (other.depth > depth)
				return 1;

			if (other.depth < depth)
				return -1;

			return 0;
		}
	}

	[SerializeField]
	private Strata[] strata;

	/// <summary>
	/// Embarrassingly hodge-podge thrown together property randomisation.
	/// </summary>
	protected override void GenerateProperties()
	{
		float heat = GravitySource.CalculateForceAtPosition(transform.position, Helper.FindMass(gameObject)).sqrMagnitude / 1000000f;

		int originalSeed = Random.seed;
		int newSeed = (int)(transform.position.sqrMagnitude + transform.eulerAngles.sqrMagnitude + transform.localScale.x);
		Random.seed = newSeed;

		baseColour = Helper.CreateRandomColour(satMax: 1f - heat, valMin: 0.1f);

		southColour = Helper.CreateRandomColour(valMin: 0.2f);

		noiseSeed = newSeed + Random.Range (0, 42);
		
		noiseCoarseness = Random.Range (0.1f, 0.7f);
		
		noiseIntensity = Random.Range (0.4f, 0.9f);
		
		if (heat > 0.5)
		{
			capSize = Random.Range (0f, 0.3f / heat);
			capColour = Helper.CreateRandomColour(satMax: 0.1f, valMin: 0.9f);
			noiseIntensity -= heat * 0.1f;
		}
		else
		{
			capSize = Random.Range (0f, 0.8f);
			capColour = Helper.CreateRandomColour(satMax: 0.4f, valMin: 0.5f);
		}

		if (Random.Range (0, 10) < 2)
			capSize = 0f;

		capSeed = newSeed + Random.Range (0, 42);
				
		capCoarseness = Random.Range (0.01f, capSize);
				
		strataSeed = newSeed + Random.Range (0, 42);

		if (Random.Range (0, 10) < 2)
			strataCoarseness = Random.Range (0f, 0.3f);
		else
			strataCoarseness = Random.Range (0f, 0.1f);

		strata = new Strata[Random.Range (1, 8)];
		for (int i = 0; i < strata.Length; ++i)
		{
			strata[i].depth = Random.Range (0f, 1f);

			if (heat > 4f)
				strata[i].colour = Helper.CreateRandomColour(satMax: 0.1f, valMin: 0.05f);
			else if (heat > 2f)
				strata[i].colour = Helper.CreateRandomColour(satMax: 0.4f, valMin: 0.1f);
			else
				strata[i].colour = Helper.CreateRandomColour(valMin: 0.1f);
		}

		Random.seed = originalSeed;
	}
	
	protected override IEnumerator Generate()
	{
		if (TextureWidth <= 0 || TextureHeight <= 0)
			yield break;

		System.Array.Sort(strata);

		Color[] pixels = new Color[TextureWidth * TextureHeight];
		for (int x = 0; x < TextureWidth; ++x)
		{
			for (int y = 0; y < TextureHeight; ++y)
			{
				int index = Helper.Get1DIndex(x, y, TextureWidth);

				// Get the height of this point on the terrain.
				float terrainHeight = Mathf.PerlinNoise(x * strataCoarseness + strataSeed, y * strataCoarseness);

				// Get the highest strata that this height is above.
				bool tooLow = true;
				float lastHeight = 1f;
				for (int i = 0; i < strata.Length; ++i)
				{
					if (terrainHeight >= strata[i].depth)
					{
						pixels[index] = strata[i].colour * (0.7f + (terrainHeight - strata[i].depth) / lastHeight * 0.3f);
						tooLow = false;
						break;
					}
					lastHeight = strata[i].depth;
				}

				// If the height was too low, apply the base colour.
				if (tooLow)
				{
					pixels[index] = baseColour;
				}

				// Add the southwards tint.
				float dx = ((float)x / TextureWidth - 0.5f) * 2;
				float dy = (float)y / TextureHeight - 0.5f;
				float distance = Mathf.Sqrt (dx * dx + dy * dy);
				pixels[index] = Color.Lerp (pixels[index], southColour, distance);

				if (ShouldYield())
					yield return new WaitForEndOfFrame();
			}
		}

		if (capSize > 0f)
		{
			// Apply the cap on top of the base texture.
			for (int x = 0; x < TextureWidth; ++x)
			{
				for (int y = 0; y < TextureHeight; ++y)
				{
					int index = Helper.Get1DIndex(x, y, TextureWidth);

					// Check if the polar cap should be applied.
					float dx = ((float)x / TextureWidth - 0.5f) * 2;
					float dy = (float)y / TextureHeight - 0.5f;
					float distance = Mathf.Sqrt (dx * dx + dy * dy);
					distance /= capSize;	// Normalize the distance.
					float distanceSqr = distance * distance;
					if (distance < 1f)
					{
						float capEdge = (1f - distanceSqr) * 0.5f;
						float capHeight = Mathf.PerlinNoise(0.1f * distanceSqr * x * capCoarseness + capSeed, 0.1f * distanceSqr * y * capCoarseness);

						pixels[index] = Color.Lerp(pixels[index], capColour, (int)((1f - distanceSqr) * 3) / 3f);
						if (capHeight > 0.4f)
							pixels[index] = Color.Lerp(pixels[index], Color.Lerp(Color.clear, capColour * 2, capHeight), capEdge * 2);
					}

					if (ShouldYield())
						yield return new WaitForEndOfFrame();
				}
			}
		}
	
		// Apply noise.
		for (int x = 0; x < TextureWidth; ++x)
		{
			for (int y = 0; y < TextureHeight; ++y)
			{
				int index = Helper.Get1DIndex(x, y, TextureWidth);
				float noise = Mathf.PerlinNoise(x * noiseCoarseness + noiseSeed, y * noiseCoarseness + noiseSeed);
				pixels[index] *= noiseIntensity + noise * (1f - noiseIntensity);

				if (ShouldYield())
					yield return new WaitForEndOfFrame();
			}
		}

		mainTexture = new Texture2D(TextureWidth, TextureHeight);
		mainTexture.filterMode = FilterMode.Bilinear;
		mainTexture.SetPixels(pixels);
		mainTexture.Apply();

		GetComponent<MeshRenderer>().material.SetTexture("_MainTex", mainTexture);
	}

	protected override void SetAtmosphereColour()
	{
		Color bestColour = baseColour;
		float bestDeltaDepth = 1f - strata[0].depth;
		for (int i = 1; i < strata.Length; ++i)
		{
			float deltaDepth = strata[i - 1].depth - strata[i].depth;
			if (deltaDepth > bestDeltaDepth)
			{
				bestDeltaDepth = deltaDepth;
				bestColour = strata[i].colour;
			}
		}

		bestColour = Color.Lerp (bestColour, Color.gray, 0.5f);

		GetComponent<MeshRenderer>().material.SetColor("_GlowColour", bestColour);

		Transform atmosphere = transform.Find ("atmosphere");
		if (atmosphere != null)
		{
			atmosphere.transform.localScale = Vector3.one * Random.Range (1.4f, 2.0f);
			atmosphere.GetComponent<Renderer>().material.SetColor("_Color", bestColour);
		}
	}
}
