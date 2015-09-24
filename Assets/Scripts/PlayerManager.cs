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

	void Start() {
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

		whitePlayer.RequestMove ();

		//whitePlayer = ((whitePlayerType == PlayerType.Human) ? (Player)new HumanPlayer () : (Player)new AIPlayer ());
		//blackPlayer = ((blackPlayerType == PlayerType.Human) ? (Player)new HumanPlayer () : (Player)new AIPlayer ());


	}

	void OnMove() {
		whiteToPlay = !whiteToPlay;

		Invoke ("RequestAIResponse",.1f);
	}

	void RequestAIResponse() {
		if (whiteToPlay) {
			whitePlayer.RequestMove ();
		} else {
			blackPlayer.RequestMove ();
		}
	}


}
