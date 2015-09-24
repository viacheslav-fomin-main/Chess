
/*
 * Represents a chess move 
 */

public class Move {

	public Coord from;
	public Coord to;
	
	public bool isPawnPromotion;
	public bool whitesMove;
	public bool isEnPassantCapture;
	public Coord enPassantPawnLocation;

	public bool isCastles;
	public Coord rookFrom;
	public Coord rookTo;

	// Defines the game state following this move (castling rights, en passant square etc)
	public GameState gameStateAfterMove;

	/*
	public Move(Coord _from, Coord _to, GameState _newGameState, bool isWhite, bool promote, bool isEnPassant, Coord capturedEnPassantPawn) {
		from = _from;
		to = _to;
		gameStateAfterMove = _newGameState;

		whitesMove = isWhite;
		isPawnPromotion = promote;
		isEnPassantCapture = isEnPassant;
		enPassantPawnLocation = capturedEnPassantPawn;
	}
*/
	public string algebraicMove {
		get {
			string promotionKey = "";
			if (isPawnPromotion) {
				promotionKey = (whitesMove)?"Q":"q";
			}
			return from.algebraic + to.algebraic + promotionKey;
		}
	}
}
