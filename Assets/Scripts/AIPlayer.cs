using System.Collections;
using System.Collections.Generic;
using System;

public class AIPlayer : Player {

	Search searcher;
	bool moveRequested;

	public override void Init (bool white)
	{
		base.Init (white);
		searcher = new Search ();
	}

	public override void RequestMove ()
	{

		base.RequestMove ();

		if (OpeningBookReader.IsInBook ()) {
			Random prng = new Random();

			List<ushort> bookMoves = OpeningBookReader.GetBookMoves();
			ushort randomBookMove = bookMoves[prng.Next(0,bookMoves.Count)];
			if (moveGenerator.GetMoves(false,false).Contains(randomBookMove)) { // ensure book move is legal (possible for zobrist keys to have hashed wrong position)
				MakeMove(randomBookMove);
			}

		} else {
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
