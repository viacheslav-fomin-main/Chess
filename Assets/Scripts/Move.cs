
/*
 * Represents a chess move 
 */

public struct Move {

	public Coord from;
	public Coord to;

	// Defines the game state following this move (castling rights, en passant square etc)
	public GameState gameStateAfterMove;

	public Move(Coord _from, Coord _to, GameState _newGameState) {
		from = _from;
		to = _to;
		gameStateAfterMove = _newGameState;
	}

	public string algebraicMove {
		get {
			return from.algebraic + to.algebraic;
		}
	}
}
