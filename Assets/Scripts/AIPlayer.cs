using System.Collections;
using System.Collections.Generic;
using System;

public class AIPlayer : Player {

	Search searcher;
	bool moveRequested;

	bool useBuiltinMoveDelay = true;
	float defaultDelayBetweenMoves = .75f;
	float moveSearchStartTime;
	ushort pendingMove;
	bool moveFound;

	ushort sic = 8546;
	bool first = true;

	public override void Init (bool white)
	{
		base.Init (white);
		searcher = new Search ();
		if (GameManager.instance.gameMode == GameManager.GameMode.Regular) {
			useBuiltinMoveDelay = false;
		}
	}

	public override void RequestMove () {
		moveSearchStartTime = UnityEngine.Time.time;

		base.RequestMove ();

		bool bookMovePlayed = false;

		if (OpeningBookReader.IsInBook () && GameManager.instance.useOpeningBook) {
			UnityEngine.Debug.Log("In Book");
			Random prng = new Random();

			List<ushort> bookMoves = OpeningBookReader.GetBookMoves();
			ushort randomBookMove = bookMoves[prng.Next(0,bookMoves.Count)];
			if (first) {
				first = false;
				//randomBookMove = sic;
			}
			if (moveGenerator.GetMoves(false,false).Contains(randomBookMove)) { // ensure book move is legal (possible for zobrist keys to have hashed wrong position)
				UnityEngine.Debug.Log("Book move");
				HandleAIMove(randomBookMove);
				bookMovePlayed = true;
			}
			else {
				UnityEngine.Debug.LogError("Book error");
			}
		}

		if (!bookMovePlayed) {
			moveRequested = true;
			searcher.StartSearch ();
		}
	}

	void HandleAIMove(ushort move) {
		pendingMove = move;
		moveFound = true;
	}
	

	public override void Update() {
		if (searcher.finishedSearch && isMyMove && moveRequested) {
			HandleAIMove(searcher.bestMoveSoFar);
			moveRequested = false;
		}
		if (moveFound && (useBuiltinMoveDelay || UnityEngine.Time.time > moveSearchStartTime + defaultDelayBetweenMoves)) {
			MakeMove(pendingMove);
			moveFound = false;
			pendingMove = 0;
		}
	}



}
