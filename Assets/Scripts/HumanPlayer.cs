using System.Collections;
using System.Collections.Generic;

public class HumanPlayer : Player {

	public static Stack<ushort> movesMade = new Stack<ushort>();
	List<ushort> legalMoves;

	public override void Init (bool white) {
		base.Init (white);
	}

	/// <summary>
	/// Make a move that is known to be legal
	/// </summary>
	protected override void MakeMove (ushort move)
	{
		base.MakeMove (move);
		movesMade.Push (move);
		//legalMovesInPosition = moveGenerator.GetAllLegalMoves (currentPosition);
	}



	/// <summary>
	/// Makes the move after confirming that it is legal
	/// </summary>
	public void TryMakeMove(string algebraicMove) {
		legalMoves = moveGenerator.GetMoves (false, false);

		for (int i = 0; i < legalMoves.Count; i ++) {
			int moveFromIndex = legalMoves[i] & 127;
			int moveToIndex = (legalMoves[i] >> 7) & 127;
			
			int moveFromX = Board.Convert128to64(moveFromIndex) % 8;
			int moveFromY = Board.Convert128to64(moveFromIndex) / 8;
			int moveToX = Board.Convert128to64(moveToIndex) % 8;
			int moveToY = Board.Convert128to64(moveToIndex) / 8;
			
			
			string fromAlgebraic = Definitions.fileNames[moveFromX].ToString() + Definitions.rankNames[moveFromY].ToString();
			string toAlgebraic = Definitions.fileNames[moveToX].ToString() + Definitions.rankNames[moveToY].ToString();


			string moveCoords = fromAlgebraic + toAlgebraic;
			if (moveCoords == algebraicMove) { // move confirmed as legal
				MakeMove(legalMoves[i]);
			}
		}
	}

	public override void RequestMove ()
	{
		base.RequestMove ();
		//legalMovesInPosition = moveGenerator.GetAllLegalMoves (currentPosition);
	}

}
