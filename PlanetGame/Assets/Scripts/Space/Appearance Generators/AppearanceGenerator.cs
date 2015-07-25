using UnityEngine;
using System.Collections;

public abstract class AppearanceGenerator : MonoBehaviour
{
	private const int SHOULD_YIELD_TARGET = 20096;
	private int shouldYieldCounter = 0;

	[SerializeField]
	private bool automaticallyGenerate;
	
	[SerializeField]
	private int textureWidth = 512;
	public int TextureWidth
	{
		get { return textureWidth; }
	}
	
	[SerializeField]
	private int textureHeight = 256;
	public int TextureHeight
	{
		get { return textureHeight; }
	}
	
	protected Texture2D mainTexture;

	void OnEnable()
	{
		transform.localScale = Random.Range (1f, 6f) * Vector3.one;

		if (automaticallyGenerate)
		{
			GenerateProperties();
		}
		
		StartCoroutine(Generate());
		
		SetAtmosphereColour();
		
		//Destroy (this);
	}

	protected abstract void GenerateProperties();
	protected abstract IEnumerator Generate();
	protected abstract void SetAtmosphereColour();

	protected bool ShouldYield()
	{
		++shouldYieldCounter;
		if (shouldYieldCounter == SHOULD_YIELD_TARGET)
		{
			shouldYieldCounter = 0;
			return true;
		}

		return false;
	}
}
