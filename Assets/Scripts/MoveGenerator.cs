using System.Collections.Generic;

public class MoveGenerator {

	static int[] knightOverlay = new int[]{6, 15, 17, 10, -6, -15, -17, -10};
	static int[] orthogonalOverlay = new int[]{8, 1, -8, -1};
	static int[] diagonalOverlay = new int[]{7, 9, -7, -9};
	
	GameState currentGameState;

	/// If true, move generator will not worry about checks when generating moves (ignores pins etc)
	/// This can be used for faster move gen if king captures are going to be rejected in search
	bool generatePsuedolegalMoves; 

	/// Returns a list of all moves in the current position
	public List<Move> AllMoves() {

		currentGameState = Board.gamestate;

		List<Move> allMoves = new List<Move> ();

		
		return allMoves;
	}

	/// Returns all quiet (i.e non-capture) moves in the position.
	/// Currently checks are considered quiet moves *if they do not capture a piece*
	public List<Move> AllQuietMoves() {
		List<Move> quietMoves = new List<Move> ();
		
		
		return quietMoves;
	}

	/// Returns all moves that capture a piece
	public List<Move> AllCaptures() {
		List<Move> captureMoves = new List<Move> ();
		
		
		return captureMoves;
	}
}
