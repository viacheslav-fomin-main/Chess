using UnityEngine;
using System.Collections;

public class GameManager : MonoBehaviour {

	PlayerManager playerManager;

	float clockTimeSeconds;
	float incrementSeconds;

	float secondsRemainingWhite;
	float secondsRemainingBlack;

	public bool regenerateOpeningBook;
	public bool useOpeningBook;
	public bool useTestPosition;

	static GameManager myInstance;

	public static GameManager instance {
		get {
			if (myInstance == null) {
				myInstance = FindObjectOfType<GameManager>();
			}
			return myInstance;
		}
	}
	
	void Start () {
		ZobristKey.Init ();
		Evaluation.Init ();
		if (regenerateOpeningBook) {
			OpeningBookGenerator.GenerateBook ();
		}
		OpeningBookReader.Init ();

		playerManager = GetComponent<PlayerManager> ();

		Board.SetPositionFromFen (Definitions.gameStartFen);

		playerManager.CreatePlayers ();
	}


}
