using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class CanvasGroupController : MonoBehaviour
{
	private const float FADE_DURATION = 1f;

	[SerializeField]
	private CanvasGroup[] groups;
	private CanvasGroup currentGroup;
	private CanvasGroup previousGroup;

	void Start()
	{
		previousGroup = groups[0];
		currentGroup = previousGroup;
		currentGroup.gameObject.SetActive(true);
	}

	public void FadeToPreviousGroup()
	{
		FadeToGroup(previousGroup);
	}

	public void FadeToGroup(CanvasGroup group)
	{
		FadeToGroup(group, FADE_DURATION);
	}

	public void FadeToGroup(CanvasGroup group, float duration)
	{
		StartCoroutine(FadeToGroupCoroutine(group, duration));
	}


	private IEnumerator FadeToGroupCoroutine(CanvasGroup group, float duration)
	{
		// Prevent interaction with the first group.
		currentGroup.interactable = false;

		// Activate the next Group.
		group.gameObject.SetActive(true);

		float t = 0f;
		while (t < duration)
		{
			// Increase t in real time.
			t += Time.unscaledDeltaTime;

			// Fade current group out.
			currentGroup.alpha = Mathf.SmoothStep (1f, 0f, t);

			// Fade new group in.
			group.alpha = Mathf.SmoothStep (0f, 1f, t);

			yield return new WaitForEndOfFrame();
		}

		// Update previous and current Group tracking variables.
		previousGroup = currentGroup;
		currentGroup = group;

		// Deactivate the last Group.
		previousGroup.gameObject.SetActive(false);

		//Enable interaction.
		currentGroup.interactable = true;
	}


}