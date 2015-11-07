using UnityEngine;
using System.Collections;
using System;

public class MoveManager : MonoBehaviour {

	public enum PlayerType {Human, AI};
	public PlayerType whitePlayerType;
	public PlayerType blackPlayerType;

	Player whitePlayer;
	Player blackPlayer;

	[HideInInspector]
	public bool whiteToPlay;

	ChessInput boardInput;
	public event Action<bool, ushort> OnMoveMade;
	public event Action<int> OnGameOver; // result (-1;0;1) (black wins; draw; white wins)
	MoveGenerator moveGenerator = new MoveGenerator();

	public void CreatePlayers() {
		boardInput = GetComponent<ChessInput> ();

		HumanPlayer whiteHuman = null;
		HumanPlayer blackHuman = null;	
		AIPlayer whiteAI = null;
		AIPlayer blackAI = null;

		if (whitePlayerType == PlayerType.Human) {
			ChessUI.instance.SetBoardOrientation(true);
			whiteHuman = new HumanPlayer ();
			boardInput.AddPlayer (whiteHuman);
			FindObjectOfType<NotationInput>().SetPlayer(whiteHuman);
		} else {
			whiteAI = new AIPlayer ();
		}
		if (blackPlayerType == PlayerType.Human) {
			ChessUI.instance.SetBoardOrientation(false);
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

		whiteToPlay = Board.IsWhiteToPlay ();
		RequestMove ();
	}

	void GameOver(int result) {
		if (OnGameOver != null) {
			OnGameOver(result);
		}
		whitePlayer.Deactivate ();
		blackPlayer.Deactivate ();
	}

	public void Resign() {
		GameOver (-1);
	}
	
	public void Draw() {
	
	}

	public void TimeOut(bool white) {
		GameOver((white)?-1:1);
	}

	void OnMove(ushort move) {
		if (OnMoveMade != null) {
			OnMoveMade(whiteToPlay, move);
		}

		whiteToPlay = !whiteToPlay;


		// detect mate/stalemate
		if (moveGenerator.PositionIsMate ()) {
			GameOver(((whiteToPlay)?-1:1)); // player is mated
			
		} else if (moveGenerator.PositionIsStaleMate ()) {
			GameOver(0); // player is mated
		} else {

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
