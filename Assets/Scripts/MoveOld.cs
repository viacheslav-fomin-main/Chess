
/*
 * Represents a chess move 
 */

public class MoveOld {

	public bool isWhiteMove;
	public bool isPawnPromotion;
	public bool isEnPassantCapture;
	public bool isCastles;

	public int fromIndex;
	public int toIndex;

	public int enPassantPawnIndex;
	public int rookFromIndex;
	public int rookToIndex;

	public int movePieceType;
	public int capturePieceType;
	public int promotionPieceType;


	public Coord from;
	public Coord to;
	

	public Coord enPassantPawnLocation;
	

	public Coord rookFrom;
	public Coord rookTo;
	
	public bool isCapture;

	
	// Defines the game state following this move (castling rights, en passant square etc)
	public GameState gameStateAfterMove;
	
	public MoveOld(Coord _from, Coord _to, GameState newGameState) {
		from = _from;
		to = _to;
		gameStateAfterMove = newGameState;
	}
	
	public MoveOld() {
		
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
