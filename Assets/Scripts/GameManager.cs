using UnityEngine;
using System.Collections;
using System;

public class GameManager : MonoBehaviour {

	MoveManager playerManager;
	public enum GameMode {Regular, BlindfoldWithBoard01, BlindfoldWithBoard02, BlindfoldSansBoard01, BlindfoldSansBoard02};
	public static int gameModeIndex;
	public static bool gameModeIndexSet;

	[Header("Game mode:")]
	public GameMode gameMode;

	[Space(15)]
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
		if (gameModeIndexSet) {
			gameMode = (GameMode)gameModeIndex;
		}

		Board.SetPositionFromFen (Definitions.gameStartFen,true);

		ZobristKey.Init ();
		Evaluation.Init ();
		if (regenerateOpeningBook) {
			OpeningBookGenerator.GenerateBook ();
		}
		OpeningBookReader.Init ();

		playerManager = GetComponent<MoveManager> ();

		playerManager.CreatePlayers ();

		Board.SetPositionFromFen (Definitions.gameStartFen,true);

	}


	public void ReturnToMenu() {
		Application.LoadLevel ("Menu");
	}

}
