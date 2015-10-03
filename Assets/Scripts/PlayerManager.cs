using UnityEngine;
using System.Collections;

public class PlayerManager : MonoBehaviour {

	public enum PlayerType {Human, AI};
	public PlayerType whitePlayerType;
	public PlayerType blackPlayerType;

	Player whitePlayer;
	Player blackPlayer;

	bool whiteToPlay = true;

	ChessInput boardInput;

	public void CreatePlayers() {
		boardInput = GetComponent<ChessInput> ();

		HumanPlayer whiteHuman = null;
		HumanPlayer blackHuman = null;	
		AIPlayer whiteAI = null;
		AIPlayer blackAI = null;

		if (whitePlayerType == PlayerType.Human) {
			whiteHuman = new HumanPlayer ();
			boardInput.AddPlayer (whiteHuman);
		} else {
			whiteAI = new AIPlayer ();
		}
		if (blackPlayerType == PlayerType.Human) {
			blackHuman = new HumanPlayer ();
			boardInput.AddPlayer (blackHuman);
		} else {
			blackAI = new AIPlayer ();
		}

		whitePlayer = (Player)whiteHuman ?? (Player)whiteAI;
		blackPlayer = (Player)blackHuman ?? (Player)blackAI;

		whitePlayer.OnMove += OnMove;
		blackPlayer.OnMove += OnMove;

		whitePlayer.Init (true);
		blackPlayer.Init (false);

		whiteToPlay = Board.isWhiteToPlay ();
		RequestAIResponse ();
	}

	void OnMove() {
		whiteToPlay = !whiteToPlay;

		if (whitePlayerType == PlayerType.AI && blackPlayerType == PlayerType.AI) {
			Invoke ("RequestAIResponse", .25f); // force delay between moves when two AI are playing
		} else {
			RequestAIResponse();
		}
	}

	void RequestAIResponse() {
		if (whiteToPlay) {
			whitePlayer.RequestMove ();
		} else {
			blackPlayer.RequestMove ();
		}
	}

	void Update() {
		whitePlayer.Update ();
		blackPlayer.Update ();

		if (Input.GetKeyDown(KeyCode.Backspace)) {
			Board.UnmakeMove(HumanPlayer.movesMade.Pop(),true);
		}
	}


}
