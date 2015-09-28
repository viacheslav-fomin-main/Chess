using System.Collections.Generic;

public class MoveGenerator {

	static int[] knightOverlay = new int[]{6, 15, 17, 10, -6, -15, -17, -10};
	static int[] orthogonalOverlay = new int[]{8, 1, -8, -1};
	static int[] diagonalOverlay = new int[]{7, 9, -7, -9};

	List<Move> legalMoves;
	GameState currentGameState;


	/// Returns a list of all legal moves in the current position as defined by the Board class
	public List<Move> GetAllLegalMoves() {

		legalMoves = new List<Move> ();
		currentGameState = Board.gamestate;
		/*
		isWhite = position.gameState.whiteToMove;
		friendlyPieces = position.AllPieces (isWhite);
		hostilePieces = position.AllPieces (!isWhite);
		allPieces = BitBoardStruct.Combination (friendlyPieces, hostilePieces);
		currentPosition = position;
		
		checkInfo = GetCheckInfo (position); // Generate info relating to checks (pins etc)
		GenerateLegalMovesForSquare (Definitions.PieceName.King, checkInfo.kingSquare);
		
		if (!checkInfo.inDoubleCheck) { // no pieces besides king can move when in double check
			GetAllMovesFromBoard(currentPosition.Pawns(isWhite), Definitions.PieceName.Pawn);
			GetAllMovesFromBoard(currentPosition.Rooks(isWhite), Definitions.PieceName.Rook);
			GetAllMovesFromBoard(currentPosition.Knights(isWhite), Definitions.PieceName.Knight);
			GetAllMovesFromBoard(currentPosition.Bishops(isWhite), Definitions.PieceName.Bishop);
			GetAllMovesFromBoard(currentPosition.Queens(isWhite), Definitions.PieceName.Queen);
		}
*/
		
		return legalMoves;
	}

}
