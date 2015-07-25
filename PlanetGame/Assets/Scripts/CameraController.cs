using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Camera))]
public class CameraController : MonoBehaviour
{
	[SerializeField]
	private float panSpeed = 1f;
	[SerializeField]
	private float panDistanceMax = 50f;
	[SerializeField]
	private float zoomSpeed = 10f;
	[SerializeField]
	private float zoomMax = 90f;
	[SerializeField]
	private float zoomMin = -20f;
	[SerializeField]
	private float resetDuration = 1f;

	new private Camera camera;
	private Vector3 startPosition;
	private Vector3 desiredPosition;

	#if UNITY_EDITOR || UNITY_STANDALONE
	private Vector3 lastMousePosition;
	#else
	private float lastTouchDistance;
	#endif
	
	void Awake()
	{
		startPosition = transform.position;
		desiredPosition = startPosition;
	}
	
	void Update()
	{
		Vector3 pan = Vector3.zero;
		float zoom = 0f;

		#if UNITY_EDITOR || UNITY_STANDALONE
		if (Input.GetMouseButtonDown(0))
		{
			lastMousePosition = Input.mousePosition;
		}
		else if (Input.GetMouseButtonDown(1))
		{
			Reset();
		}
		
		if (Input.GetMouseButton(0))
		{
			pan = Input.mousePosition - lastMousePosition;
			lastMousePosition = Input.mousePosition;
		}
		
		zoom = Input.mouseScrollDelta.y;
		#else
		if (Input.touchCount == 2)
		{
			float touchDistance = Vector3.Distance(Input.GetTouch(0).position, Input.GetTouch (1).position);
			if (lastTouchDistance == 0f)
			{
				lastTouchDistance = touchDistance;
			}
			else
			{
				zoom = touchDistance - lastTouchDistance;
			}
		}
		else
		{
			if (Input.touchCount == 1)
			{
				pan = Input.GetTouch(0).deltaPosition;
			}
			
			lastTouchDistance = 0f;
		}
		#endif

		if ((pan != Vector3.zero || zoom != 0f) && !Helper.IsUIBeingTouched())
		{
			StopAllCoroutines();

			desiredPosition = transform.position + new Vector3(pan.x * panSpeed, pan.y * panSpeed, zoom * zoomSpeed);

			if (desiredPosition.x < -panDistanceMax)
					desiredPosition.x = -panDistanceMax;
			else if (desiredPosition.x > panDistanceMax)
				desiredPosition.x = panDistanceMax;
			
			if (desiredPosition.y < -panDistanceMax)
				desiredPosition.y = -panDistanceMax;
			else if (desiredPosition.y > panDistanceMax)
				desiredPosition.y = panDistanceMax;
			
			if (desiredPosition.z - startPosition.z > zoomMax)
				desiredPosition.z = startPosition.z + zoomMax;
			else if (desiredPosition.z < startPosition.z + zoomMin)
				desiredPosition.z = startPosition.z + zoomMin;
		}

		transform.position = Vector3.Lerp (transform.position, desiredPosition, 0.2f);
	}

	void OnDisable()
	{
		StopAllCoroutines();
		desiredPosition = transform.position;
	}
	
	public void Reset()
	{
		StopAllCoroutines();
		StartCoroutine(MoveOverTime(startPosition, resetDuration));
		StartCoroutine(ZoomOverTime(0f, resetDuration));
	}
	
	private IEnumerator MoveOverTime(Vector2 targetPosition, float duration)
	{
		Vector2 initialPosition = transform.position;
		float time = 0f;
		while (time < duration)
		{
			time += Time.deltaTime;
			Vector2 nextPosition = Vector2.Lerp(initialPosition, targetPosition, Mathf.SmoothStep(0f, 1f, time / duration));
			desiredPosition = new Vector3(nextPosition.x, nextPosition.y, desiredPosition.z);
			yield return new WaitForEndOfFrame();
		}
	}
	
	private IEnumerator ZoomOverTime(float targetZoom, float duration)
	{
		float initialZoom = transform.position.z - startPosition.z;
		float time = 0f;
		while (time < duration)
		{
			time += Time.deltaTime;
			float nextZoom = Mathf.SmoothStep(initialZoom, targetZoom, time / duration);
			desiredPosition = new Vector3(desiredPosition.x, desiredPosition.y, startPosition.z + nextZoom);
			yield return new WaitForEndOfFrame();
		}
	}
}
