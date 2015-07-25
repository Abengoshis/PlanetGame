using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public static class Helper
{
	public static bool IsUIBeingTouched()
	{
		UnityEngine.EventSystems.EventSystem eventSystem = UnityEngine.EventSystems.EventSystem.current;
		
		if (eventSystem.currentSelectedGameObject == null ||
		    eventSystem.currentSelectedGameObject.GetComponent<Button>() == null)
			return false;
		
		return true;
	}

	public static int Get1DIndex(int x, int y, int width)
	{
		return y * width + x;
	}

	public static float FindMass(GameObject gameObject)
	{
		float mass;
		if (gameObject.GetComponent<GravitySource>() != null)
			mass = gameObject.GetComponent<GravitySource>().Mass;
		else if (gameObject.GetComponent<Rigidbody2D>())
			mass = gameObject.GetComponent<Rigidbody2D>().mass;
		else
			mass = gameObject.transform.localScale.x;
		return mass;
	}

	public static Color CreateRandomColour(float hueMin = 0f, float hueMax = 360f, float satMin = 0f, float satMax = 1f, float valMin = 0f, float valMax = 1f)
	{
		return CreateColourFromHSV(Random.Range (hueMin, hueMax), Random.Range (satMin, satMax), Random.Range (valMin, valMax));
	}

	// http://www.rapidtables.com/convert/color/hsv-to-rgb.htm
	public static Color CreateColourFromHSV(float hue, float sat, float val)
	{
		hue %= 360f;
		sat = Mathf.Clamp (sat, 0f, 1f);
		val = Mathf.Clamp (val, 0f, 1f);

		float c = sat * val;
		float x = c * (1 - Mathf.Abs((hue / 60.0f) % 2 - 1));
		float m = val - c;

		Color colour;

		if (hue < 60)
			colour = new Color(c, x, 0);
		else if (hue < 120)
			colour = new Color(x, c, 0);
		else if (hue < 180)
			colour = new Color(0, c, x);
		else if (hue < 240)
			colour = new Color(0, x, c);
		else if (hue < 300)
			colour = new Color(x, 0, c);
		else
			colour = new Color(c, 0, x);

		colour.r += m;
		colour.g += m;
		colour.b += m;
		return colour;
	}
}
