using UnityEngine;
using System.Collections;

[RequireComponent(typeof(LineRenderer))]
public class OrbitBehaviour : MonoBehaviour
{
	private const int LINE_SEGMENTS = 32;

	private readonly Color UNSELECTED_COLOR = new Color(1f, 1f, 1f, 0.1f);
	private readonly Color HIGHLIGHTED_COLOR = new Color(1f, 1f, 1f, 0.4f);
	private readonly Color SELECTED_COLOR = new Color(1f, 1f, 1f, 0.8f);

	private const float UNSELECTED_WIDTH = 0.5f;
	private const float HIGHLIGHTED_WIDTH = 0.5f;
	private const float SELECTED_WIDTH = 0.5f;

	private static bool selectAvailable = true;
	private enum SelectState { UNSELECTED, HIGHLIGHTED, SELECTED }
	private SelectState selectState = SelectState.UNSELECTED;

	[SerializeField]
	private GameObject objectToOrbit;

	[SerializeField]
	[Range(0f, 1f)]
	private float eccentricity;

	[SerializeField]
	[Range(0f, 360f)]
	private float initialAngle;
	private float currentAngle;
	
	private float majorRadius;
	private Vector2 majorAxis;

	private LineRenderer lineRenderer;

	public float MajorRadius
	{
		get { return majorRadius; }
	}

	public float MinorRadius
	{
		get { return majorRadius * (1f - eccentricity); }
	}
	
	public Vector2 MajorAxis
	{
		get { return majorAxis; }
	}
	
	public Vector2 MinorAxis
	{
		get { return new Vector2(-majorAxis.y, majorAxis.x);}
	}

	public float Offset
	{
		get { return MajorRadius * eccentricity; }
	}

	public Vector2 PositionOrbited
	{
		get
		{
			if (objectToOrbit != null)
				return objectToOrbit.transform.position;
			return Vector2.zero;
		}
	}

	public Vector2 Centre
	{
		get { return PositionOrbited + MajorAxis * Offset; }
	}

	void Start()
	{
		InitializeOrbitValues();
		
		lineRenderer = GetComponent<LineRenderer>();
		lineRenderer.SetColors(UNSELECTED_COLOR, UNSELECTED_COLOR);
		lineRenderer.SetWidth(UNSELECTED_WIDTH, UNSELECTED_WIDTH);
		lineRenderer.SetVertexCount(LINE_SEGMENTS);

		initialAngle += + Mathf.Atan2 (-majorAxis.y, majorAxis.x) * Mathf.Rad2Deg;	// todo: change this so it doesn't need to add the angle of the ellipse.
		currentAngle = initialAngle;
	}

	void Update()
	{
		// Store the current select state to check if it has changed after processing input.
		SelectState lastSelectState = selectState;

		ProcessInput();

		// Keep the position at the correct angle around the position being orbited.
		transform.position = GetPositionAtDegrees(currentAngle);

		// If the select state has changed, update the line renderer.
		if (selectState != lastSelectState)
		{
			switch (selectState)
			{
			case SelectState.UNSELECTED:
				lineRenderer.SetColors(UNSELECTED_COLOR, UNSELECTED_COLOR);
				lineRenderer.SetWidth(UNSELECTED_WIDTH, UNSELECTED_WIDTH);
				break;
			case SelectState.HIGHLIGHTED:
				lineRenderer.SetColors(HIGHLIGHTED_COLOR, HIGHLIGHTED_COLOR);
				lineRenderer.SetWidth(HIGHLIGHTED_WIDTH, HIGHLIGHTED_WIDTH);
				break;
			case SelectState.SELECTED:
				lineRenderer.SetColors(SELECTED_COLOR, SELECTED_COLOR);
				lineRenderer.SetWidth(SELECTED_WIDTH, SELECTED_WIDTH);
				break;
			}
		}

		for (int i = 0; i < LINE_SEGMENTS; ++i)
		{
			float t = (float)i / (LINE_SEGMENTS - 1) * 2 * Mathf.PI;
			Vector2 position = GetPositionAtRadians(t);
			lineRenderer.SetPosition(i, position);
		}
	}

	private void InitializeOrbitValues()
	{
		Vector3 delta = (Vector2)transform.position - PositionOrbited;
		float distance = delta.magnitude;
		majorAxis = delta / distance;
		majorRadius = distance;
	}

	private void ProcessInput()
	{
		// Determine select state.
		Vector2 inputPosition = (Vector2)Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, -Camera.main.transform.position.z));
		if (selectAvailable)
		{
			if (Vector2.Distance((Vector2)inputPosition, (Vector2)transform.position) <= transform.localScale.x)
			{
				if (Input.GetMouseButtonDown(0))
				{
					selectState = SelectState.SELECTED;
					selectAvailable = false;
				}
				else
				{
					selectState = SelectState.HIGHLIGHTED;
				}
			}
			else
			{
				selectState = SelectState.UNSELECTED;
			}
		}
		else
		{
			if (selectState == SelectState.SELECTED)
			{
				if (Input.GetMouseButtonUp(0))
				{
					selectState = SelectState.UNSELECTED;
					selectAvailable = true;
				}
			}
			else
			{
				selectState = SelectState.UNSELECTED;
			}
		}

		// If selected, clamp the input position to the orbit and check the angle.
		if (selectState == SelectState.SELECTED)
		{
			Vector2 position = ClampToOrbit(inputPosition);
			currentAngle = GetDegreesAtPosition(position);
		}
	}

	/// <summary>
	/// Clamps a position to a circle defined by the major radius.
	/// </summary>
	public Vector2 ClampToMajorRadius(Vector2 position)
	{
		Vector2 direction = (position - Centre).normalized;
		return Centre + direction * MajorRadius;
	}

	/// <summary>
	/// Clamps a position to the orbit ellipse.
	/// </summary>
	/// <returns>The to orbit.</returns>
	/// <param name="position">Position.</param>
	public Vector2 ClampToOrbit(Vector2 position)
	{
		// Get the direction of the line as the direction to the centre of the ellipse.
		Vector2 direction = (position - Centre).normalized;

		// Get the direction of the line along the ellipse's axes.
		Vector2 axialDirection = new Vector2(Vector2.Dot (direction, MajorAxis), Vector2.Dot (direction, MinorAxis));

		// Is mayonnaise an instrument?
		float length = (MinorRadius * MajorRadius) / Mathf.Sqrt (Mathf.Pow (MajorRadius, 2) * Mathf.Pow (axialDirection.y, 2) +
		                										 Mathf.Pow (MinorRadius, 2) * Mathf.Pow (axialDirection.x, 2));
		// Calculate the intersection of the line with the ellipse.
		Vector2 axialIntersect = axialDirection * length;

		// Convert back to Cartesian axes and return.
		Vector2 intersect = Vector2.Reflect(new Vector2(Vector2.Dot (axialIntersect, MajorAxis), Vector2.Dot (axialIntersect, -MinorAxis)), MinorAxis);

		return Centre + intersect;
	}

	/// <summary>
	/// Gets the position on the circumference of the ellipse at the given angle.
	/// </summary>
	/// <param name="angle">Angle in radians.</param>
	public Vector2 GetPositionAtRadians(float radians)
	{
		// Rotate by 90 degrees to match the planet in the editor.
		radians = radians + Mathf.PI * 0.5f;
		return ClampToOrbit(Centre + new Vector2(Mathf.Sin (radians), Mathf.Cos (radians)));
	}

	/// <summary>
	/// Gets the position on the circumference of the ellipse at the given angle.
	/// </summary>
	/// <param name="angle">Angle in radians.</param>
	public Vector2 GetPositionAtDegrees(float degrees)
	{
		return GetPositionAtRadians(degrees * Mathf.Deg2Rad);
	}

	public float GetDegreesAtPosition(Vector2 position)
	{
		position = ClampToOrbit(position);

		Vector2 worldDirection = (position - Centre).normalized;
		Vector2 localDirection = Vector2.Dot (worldDirection, MajorAxis) * MajorAxis + Vector2.Dot(worldDirection, MinorAxis) * MinorAxis;

		return Mathf.Atan2 (-localDirection.y, localDirection.x) * Mathf.Rad2Deg;
	}

	void OnDrawGizmos()
	{
		if (!Application.isPlaying)
		{
			InitializeOrbitValues ();

			Vector2 centre = Centre;

			Gizmos.color = Color.grey;

			// Draw radial lines.
			Gizmos.DrawLine(centre - MajorAxis * MajorRadius, centre + MajorAxis * MajorRadius);
			Gizmos.DrawLine(centre - MinorAxis * MinorRadius, centre + MinorAxis * MinorRadius);

			Gizmos.color = Color.white;

			// Draw circumference lines.
			Vector2 lastPosition = GetPositionAtRadians(0);
			for (int i = 1; i < LINE_SEGMENTS; ++i)
			{
				float t = (float)i / (LINE_SEGMENTS - 1) * 2 * Mathf.PI;
				Vector2 position = GetPositionAtRadians(t);
				Gizmos.DrawLine((Vector3)lastPosition, (Vector3)position);
				lastPosition = position;
			}
			
			Gizmos.color = Color.yellow;

			// Draw arrow to spawn position.
			Vector2 initialPosition = GetPositionAtDegrees(initialAngle + Mathf.Atan2 (-majorAxis.y, majorAxis.x) * Mathf.Rad2Deg);
			CustomGizmos.DrawArrow(transform.position, initialPosition);

			Gizmos.color = Color.yellow * 0.8f;
			
			Gizmos.DrawSphere(initialPosition, transform.localScale.x * 0.5f);

			Gizmos.color = Color.red;
			if (selectState == SelectState.SELECTED)
			{
				Vector2 inputPosition = (Vector2)Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, -Camera.main.transform.position.z));
				Vector2 intersect = ClampToOrbit(inputPosition);
				Gizmos.DrawLine(centre, inputPosition);
				Vector2 perp = inputPosition - centre;
				perp = new Vector2(-perp.y, perp.x).normalized * 5;
				Gizmos.DrawLine(intersect - perp, intersect + perp);
				Gizmos.DrawWireSphere(intersect, 0.5f);
			}
		}
	}
}

// Perhaps rotate over time by making circle offset*2 centre so one side near to aphelion and other far from perihelion then pull around the circle.
