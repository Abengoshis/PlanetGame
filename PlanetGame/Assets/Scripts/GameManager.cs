using UnityEngine;
using System.Collections;

public class GameManager : MonoBehaviour
{
	private static GameManager instance;

	[SerializeField]
	private Canvas canvas;
	[SerializeField]
	private CanvasGroup resultsGroup;

	public enum State { PLANNING, PLAYING, RESULTS }
	private State gameState = State.PLANNING;
	private float unpausedTimeScale = 1f;
	private float timeElapsed = 0f;

	public static Canvas Canvas
	{
		get { return instance.canvas; }
	}

	public static State GameState
	{
		get { return instance.gameState; }
	}

	void Awake()
	{
		instance = this;
	}

	void Update()
	{
		if (gameState == State.PLAYING)
		{
			timeElapsed += Time.deltaTime;
			if (timeElapsed > WhiteHole.MAX_SPAWN_DELAY)
			{
				if (Hazardous.NumHazards == 0)
				{
					EndGame ();
				}
			}
		}
	}

	public void StartGame()
	{
		foreach (WhiteHole wh in FindObjectsOfType<WhiteHole>())
		{
			wh.Spawn();
		}

		timeElapsed = 0f;

		gameState = State.PLAYING;
	}
	
	private void EndGame ()
	{
		canvas.GetComponent<CanvasGroupController>().FadeToGroup(resultsGroup, 4f);

		gameState = State.RESULTS;
	}

	public void PauseGame()
	{
		unpausedTimeScale = Time.timeScale;
		Time.timeScale = 0f;
	}

	public void ResumeGame()
	{
		Time.timeScale = unpausedTimeScale;
	}

	public void ResetGame(bool keepState)
	{
		// Bring back resurrectable objects.
		Resurrectable.ResurrectAll();
		
		// Destroy all explosions.
		foreach (Explosion ex in FindObjectsOfType<Explosion>())
		{
			Destroy (ex.gameObject);
		}
		
		// Reset orbits.
		foreach (Orbit o in FindObjectsOfType<Orbit>())
		{
			o.Reset(keepState);
		}

		ResumeGame();

		gameState = State.PLANNING;
	}

	public void QuitGame()
	{
		Application.LoadLevel(0);
	}
}
	
