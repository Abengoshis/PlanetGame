using UnityEngine;
using System.Collections;

public static class CustomGizmos
{
	public static void DrawArrow(Vector2 start, Vector2 end)
	{
		Vector2 spoke = Vector2.Lerp (start, end, 0.8f);
		Vector2 perp = (end - start);
		float temp = perp.x;
		perp.x = -perp.y;
		perp.y = temp;
		
		Gizmos.DrawLine(start, end);
		Gizmos.DrawLine(spoke + perp * 0.1f, end);
		Gizmos.DrawLine(spoke - perp * 0.1f, end);
	}
}
