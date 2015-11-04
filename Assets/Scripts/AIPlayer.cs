using System.Collections;
using System.Collections.Generic;
using System;

public class AIPlayer : Player {

	Search searcher;
	bool moveRequested;

	ushort sic = 8546;
	bool first = true;

	public override void Init (bool white)
	{
		base.Init (white);
		searcher = new Search ();
	}

	public override void RequestMove ()
	{

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
				MakeMove(randomBookMove);
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
	

	public override void Update() {
		if (searcher.finishedSearch && isMyMove && moveRequested) {
			MakeMove(searcher.bestMoveSoFar);
			moveRequested = false;
		}
	}



}
