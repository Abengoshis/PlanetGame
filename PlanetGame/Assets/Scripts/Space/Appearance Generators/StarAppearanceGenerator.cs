using UnityEngine;
using System.Collections;

public class StarAppearanceGenerator : AppearanceGenerator
{
	[SerializeField]
	private Color colour;

	protected override void GenerateProperties()
	{
		float heat = transform.localScale.x;
	}
	
	protected override IEnumerator Generate()
	{
		if (ShouldYield())
			yield return new WaitForEndOfFrame();
	}
	
	protected override void SetAtmosphereColour()
	{

	}
}
