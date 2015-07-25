using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections;

[RequireComponent(typeof(LineRenderer))]
public class Orbit : MonoBehaviour
{
	private const int LINE_SEGMENTS = 32;

	private readonly Color UNSELECTABLE_COLOR = new Color(1f, 0f, 0f, 0.1f);
	private readonly Color UNSELECTED_COLOR = new Color(1f, 1f, 1f, 0.1f);
	private readonly Color SELECTED_COLOR = new Color(1f, 1f, 1f, 0.8f);

	private const float UNSELECTED_WIDTH = 0.5f;
	private const float SELECTED_WIDTH = 0.5f;

	// Whether orbits can be selected (false if any orbit is currently selected).
	private static bool selectAvailable = true;
	private bool selected;

	// The object around which this object orbits.
	[SerializeField]
	private GameObject objectToOrbit;

	// Major axis of the orbit ellipse and its radius. (Semi-major axis). 
	private float majorRadius;
	private Vector2 majorAxis;

	// Flatness of the orbit, controls the Minor axis as a factor of the Major axis.
	[SerializeField]
	[Range(0f, 1f)]
	private float eccentricity;

	// The initial angle of the object around its orbit when it spawns, in degrees.
	[SerializeField]
	[Range(0f, 360f)]
	private float initialAngle;

	// The current angle of the object around its orbit.
	private float currentAngle;

	// The hacky offset that is added to the initial angle at the start to account for a rotated orbit.
	private float offsetAngle;
	
	// Represents the difference from the initialAngle, up to 180 degrees, while planning.
	private float plannedAngle;

	// Represents the cumulative angle the player has moved the object while playing.
	private float deltaAngle;

	// Whether object can be moved around its orbit.
	[SerializeField]
	private bool interactable = true;// remove and just dont add this script or the line renderer?

	// The line renderer to draw the orbit.
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

	public float TotalAngle
	{
		get { return Mathf.Abs (plannedAngle) + deltaAngle; }
	}

	void Start()
	{
		InitializeOrbitValues();
		
		lineRenderer = GetComponent<LineRenderer>();

		if (interactable)
		{
			lineRenderer.SetColors(UNSELECTED_COLOR, UNSELECTED_COLOR);
			lineRenderer.SetWidth(UNSELECTED_WIDTH, UNSELECTED_WIDTH);
		}
		else
		{
			lineRenderer.SetColors(UNSELECTABLE_COLOR, UNSELECTABLE_COLOR);
			lineRenderer.SetWidth(UNSELECTED_WIDTH, UNSELECTED_WIDTH);
		}

		lineRenderer.SetVertexCount(LINE_SEGMENTS);

		offsetAngle = Mathf.Atan2 (-majorAxis.y, majorAxis.x) * Mathf.Rad2Deg;	// todo: change this so it doesn't need to add the angle of the ellipse.
		initialAngle += offsetAngle;
		currentAngle = initialAngle;
		deltaAngle = 0;

		UpdateLineRenderer();
	}

	void Update()
	{
		if (interactable && Time.timeScale != 0f)
		{
			ProcessInput();
		}

		// Keep the position at the correct angle around the position being orbited.
		Vector2 desiredPosition = GetPositionAtDegrees(currentAngle);
		if (transform.position.x != desiredPosition.x &&
		    transform.position.y != desiredPosition.y)
		{
			transform.position = desiredPosition;
			UpdateLineRenderer();
		}
	}

	private void UpdateLineRenderer()
	{
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
				if (Input.GetMouseButtonDown(0) && !Helper.IsUIBeingTouched())
				{
					selected = true;
					selectAvailable = false;

					Camera.main.GetComponent<CameraController>().enabled = false;

					lineRenderer.SetColors(SELECTED_COLOR, SELECTED_COLOR);
					lineRenderer.SetWidth(SELECTED_WIDTH, SELECTED_WIDTH);
				}
			}
		}
		else
		{
			if (selected)
			{
				if (Input.GetMouseButtonUp(0))
				{
					selected = false;
					selectAvailable = true;

					Camera.main.GetComponent<CameraController>().enabled = true;

					lineRenderer.SetColors(UNSELECTED_COLOR, UNSELECTED_COLOR);
					lineRenderer.SetWidth(UNSELECTED_WIDTH, UNSELECTED_WIDTH);
				}
			}
		}

		// If selected, clamp the input position to the orbit and check the angle.
		if (selected)
		{
			Vector2 position = ClampToOrbit(inputPosition);
			float angle = GetDegreesAtPosition(position);

			// When playing, accumulate movements.
			if (GameManager.GameState == GameManager.State.PLAYING)
			{
				deltaAngle += Mathf.Abs (Mathf.DeltaAngle(currentAngle, angle));
			}
			else
			{
				plannedAngle = Mathf.DeltaAngle(initialAngle, angle);
			}

			// Update the current angle.
			currentAngle = angle;
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
			if (selected)
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

	public void Reset(bool keepState)
	{
		currentAngle = initialAngle;
		deltaAngle = 0f;
		
		if (keepState)
			currentAngle += plannedAngle;
		else
			plannedAngle = 0f;
	}
}