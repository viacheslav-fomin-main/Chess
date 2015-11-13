using UnityEngine;
using System.Collections;
using System;

public class MoveManager : MonoBehaviour {

	public enum PlayerType {Human, AI};
	public PlayerType whitePlayerType;
	public PlayerType blackPlayerType;
	public AudioClip moveAudio;
	public AudioClip captureAudio;

	Player whitePlayer;
	Player blackPlayer;

	[HideInInspector]
	public bool whiteToPlay;

	ChessInput boardInput;
	public event Action<bool, ushort> OnMoveMade;
	public event Action<int, Definitions.ResultType> OnGameOver; // result (-1;0;1) (black wins; draw; white wins)
	MoveGenerator moveGenerator = new MoveGenerator();
	bool gameOver;

	public void CreatePlayers() {
		boardInput = GetComponent<ChessInput> ();

		HumanPlayer whiteHuman = null;
		HumanPlayer blackHuman = null;	
		AIPlayer whiteAI = null;
		AIPlayer blackAI = null;

		if (blackPlayerType == PlayerType.Human) {
			ChessUI.instance.SetBoardOrientation(false);
			blackHuman = new HumanPlayer ();
			boardInput.AddPlayer (blackHuman);
		} else {
			blackAI = new AIPlayer ();
		}

		if (whitePlayerType == PlayerType.Human) {
			ChessUI.instance.SetBoardOrientation(true);
			whiteHuman = new HumanPlayer ();
			boardInput.AddPlayer (whiteHuman);
			FindObjectOfType<NotationInput>().SetPlayer(whiteHuman);
		} else {
			whiteAI = new AIPlayer ();
		}

		whitePlayer = (Player)whiteHuman ?? (Player)whiteAI;
		blackPlayer = (Player)blackHuman ?? (Player)blackAI;

		whitePlayer.OnMove += OnMove;
		blackPlayer.OnMove += OnMove;

		whitePlayer.Init (true);
		blackPlayer.Init (false);

		whiteToPlay = Board.IsWhiteToPlay ();
		RequestMove ();
	}

	void GameOver(int result, Definitions.ResultType resultType = Definitions.ResultType.NA) {
		if (!gameOver) {
			gameOver = true;
			if (OnGameOver != null) {
				OnGameOver (result, resultType);
			}
			whitePlayer.Deactivate ();
			blackPlayer.Deactivate ();
		}
	}

	public void Resign() {
		GameOver (-1, Definitions.ResultType.Resignation);
	}

	// draw requested by player
	public void Draw() {
		if (Board.ThreefoldRepetition ()) {
			GameOver (0, Definitions.ResultType.Repetition);
		} else if (Board.halfmoveCountSinceLastPawnMoveOrCap >= 100) {
			GameOver(0, Definitions.ResultType.FiftyMoveRule);
		}
	}

	public void TimeOut(bool white) {
		GameOver((white)?-1:1, Definitions.ResultType.Timeout);
	}

	void OnMove(ushort move) {
		int moveToIndex = (move >> 7) & 127;
		int capturedPieceCode = Board.boardArray [moveToIndex]; // get capture piece code
		if (capturedPieceCode == 0) {
			AudioSource.PlayClipAtPoint (moveAudio, Vector3.zero, 1f);
		} else {
			AudioSource.PlayClipAtPoint (captureAudio, Vector3.zero, 1f);
		}

		Board.MakeMove (move, true);

		if (OnMoveMade != null) {
			OnMoveMade(whiteToPlay, move);
		}

		whiteToPlay = !whiteToPlay;

		// detect mate/stalemate
		if (moveGenerator.PositionIsMate ()) {
			GameOver (((whiteToPlay) ? -1 : 1), Definitions.ResultType.Checkmate); // player is mated
			
		} else if (moveGenerator.PositionIsStaleMate ()) {
			GameOver (0, Definitions.ResultType.Stalemate); // player is mated
		} else if (moveGenerator.InsuffientMatingMaterial ()) {
			GameOver(0, Definitions.ResultType.InsufficientMaterial);
		}
		else {

			if (whitePlayerType == PlayerType.AI && blackPlayerType == PlayerType.AI) {
				StartCoroutine (RequestMoveCoroutine (.15f)); // force delay between moves when two AI are playing
			} else {
				RequestMove ();
			}
		}
	}

	void RequestMove() {
		if (whiteToPlay) {
			whitePlayer.RequestMove ();
		} else {
			blackPlayer.RequestMove ();
		}
	}

	
	IEnumerator RequestMoveCoroutine(float delay) {
		yield return new WaitForSeconds (delay);
		if (whiteToPlay) {
			whitePlayer.RequestMove ();
		} else {
			blackPlayer.RequestMove ();
		}
	}

	void Update() {
		whitePlayer.Update ();
		blackPlayer.Update ();
	}


}
