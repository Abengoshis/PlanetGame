using UnityEngine;
using System.Collections;

public class GasAppearanceGenerator : AppearanceGenerator
{
	[System.Serializable]
	private struct Band
	{
		public int weight;
		[Range(0f, 1f)]
		public float border;
		[Range(0f, 1f)]
		public float roughness;
		[Range(0f, 1f)]
		public float flux;
		public Color colour;
	}

	[System.Serializable]
	private struct Storm
	{
		[Range(0f, 1f)]
		public float latitude;
		[Range(0f, 0.5f)]
		public float size;
		[Range(0f, 1f)]
		public float twist;
		public Color colour;
	}

	[SerializeField]
	private Color baseColour = Color.white;

	[SerializeField]
	private Band[] bands;

	[SerializeField]
	private Storm storm;

	protected override void GenerateProperties()
	{
		float hue = Random.Range (0f, 360f);
		baseColour = Helper.CreateRandomColour(hue, hue, valMin: 0.1f, satMax: 0.5f);

		int numBands = Random.Range (5, 10);
		if (numBands > 0)
		{
			bands = new Band[numBands];
			for (int i = 0; i < bands.Length; ++i)
			{
				bands[i].weight = Random.Range (1, Mathf.RoundToInt(bands.Length / 2));
				bands[i].border = Random.Range (0f, 1f);
				bands[i].roughness = Random.Range (0.1f, 1f);
				bands[i].flux = Random.Range (0.5f, 1f);
				bands[i].colour = Helper.CreateRandomColour(hue - 30, hue + 30, valMin: 0.1f, satMax: 0.5f);
			}
		}
	}
	
	protected override IEnumerator Generate()
	{
		if (TextureWidth <= 0 || TextureHeight <= 0)
			yield break;

		Color[] pixels = new Color[TextureWidth * TextureHeight];

		int halfWidth = TextureWidth / 2;
		int halfHeight = TextureHeight / 2;
		
		#region Bands
		if (bands != null)
		{
			// Get the total weight of the bands.
			int totalWeight = 0;
			foreach (Band b in bands)
				totalWeight += b.weight;

			// Set all pixels to the base colour.
			for (int x = 0; x < TextureWidth; ++x)
			{
				for (int y = 0; y < TextureHeight; ++y)
				{
					pixels[Helper.Get1DIndex(x, y, TextureWidth)] = baseColour;
				}
			}

			if (totalWeight > 0)
			{
				int top = 0;
				int bottom = 0;
				for (int i = bands.Length - 1; i >= 0; --i)
				{
					// Get the weight scaled between 0 and 1 by the total weight.
					float weight = (float)bands[i].weight / totalWeight;

					// Get the bottom pixel row of the texture.
					bottom = top + Mathf.RoundToInt(weight * TextureHeight);

					// Loop half way across the texture.
					for (int x = 0; x <= halfWidth; ++x)
					{
						// Only loop through the rows of the band.
						for (int y = top; y <= bottom && y < TextureHeight; ++y)
						{
							int index = Helper.Get1DIndex(x, y, TextureWidth);

							int thickness = bottom - top;
							float delta = Mathf.Sin ((float)(y - top) / thickness * Mathf.PI);

							if (delta >= bands[i].border)
							{
								int borderThickness = (int)(bands[i].border * thickness * 0.5f);
								int bandThickness = thickness - borderThickness * 2;
								float bandDelta = Mathf.Sin ((float)(y - (top + borderThickness)) / bandThickness * Mathf.PI);

								Color colour = bands[i].colour;

								float perl = Mathf.PerlinNoise(x * bands[i].roughness + transform.position.x, y * bands[i].roughness + transform.position.y);
								float fade = (perl * bands[i].flux + bandDelta);
								pixels[index] = Color.Lerp (pixels[index], colour, fade * 0.5f);
							}

							if (ShouldYield())
								yield return new WaitForEndOfFrame();
						}
					}

					// The top of the new band will be the bottom of the current band.
					top = bottom;
				}
			}
		}
		#endregion

		#region distortion

		#endregion

		// Mirror the first half.
		for (int x = halfWidth; x < TextureWidth; ++x)
		{
			for (int y = 0; y < TextureHeight; ++y)
			{
				int index = Helper.Get1DIndex(x, y, TextureWidth);
				int indexReverse = Helper.Get1DIndex(halfWidth - (x - halfWidth), y, TextureWidth);
				pixels[index] = pixels[indexReverse];

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
		GetComponent<MeshRenderer>().material.SetColor("_GlowColour", baseColour);
		Transform atmosphere = transform.Find ("atmosphere");
		if (atmosphere != null)
		{
			atmosphere.transform.localScale = Vector3.one * 1.4f;
			atmosphere.GetComponent<Renderer>().material.SetColor("_Color", baseColour);
		}
	}
}
