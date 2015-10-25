using UnityEngine;
using System.Collections;

public class GameManager : MonoBehaviour {

	PlayerManager playerManager;

	float clockTimeSeconds;
	float incrementSeconds;

	float secondsRemainingWhite;
	float secondsRemainingBlack;
	
	void Start () {
		ZobristKey.Init ();
		Evaluation.Init ();
		OpeningBookGenerator.GenerateBook ();
		OpeningBookReader.Init ();

		playerManager = GetComponent<PlayerManager> ();

		Board.SetPositionFromFen (Definitions.gameStartFen);

		playerManager.CreatePlayers ();
	}


}
