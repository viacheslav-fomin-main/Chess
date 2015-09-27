
/*
 * Represents a chess move 
 */

public class Move {

	public Coord from;
	public Coord to;
	
	public bool isPawnPromotion;
	public bool isWhiteMove;
	public bool isEnPassantCapture;
	public Coord enPassantPawnLocation;

	public bool isCastles;
	public Coord rookFrom;
	public Coord rookTo;

	public bool isCapture;
	public int capturePiece;
	public int pieceType;

	public Definitions.PieceName pieceName;

	// Defines the game state following this move (castling rights, en passant square etc)
	public GameState gameStateAfterMove;

	public Move(Coord _from, Coord _to, GameState newGameState) {
		from = _from;
		to = _to;
		gameStateAfterMove = newGameState;
	}

	public Move() {

	}

	public string algebraicMove {
		get {
			string promotionKey = "";
			if (isPawnPromotion) {
				promotionKey = (isWhiteMove)?"Q":"q";
			}
			return from.algebraic + to.algebraic + promotionKey;
		}
	}
}
