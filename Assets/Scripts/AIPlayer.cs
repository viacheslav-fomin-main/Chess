using System.Collections;
using System;

public class AIPlayer : Player {

	public override void RequestMove ()
	{
		base.RequestMove ();

		Search search = new Search ();
		Move bestMove = search.SearchForBestMove (currentPosition);
		MakeMove (bestMove);
		//Move[] moves = moveGenerator.GetAllLegalMoves (currentPosition);
		
		//Random random = new Random ();
		//MakeMove (moves [random.Next (0, moves.Length)]);
	}



}
