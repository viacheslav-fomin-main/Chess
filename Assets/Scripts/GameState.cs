
/*
 * Store information about the position:
 * Castling rights
 * En passant
 */

public struct GameState {

	// Castling rights for both sides
	public bool castleKingsideW;
	public bool castleKingsideB;
	public bool castleQueensideW;
	public bool castleQueensideB; 

	// The file index (a=0, h=7) of a pawn that has advanced two squares in the last move
	public int enPassantFileIndex;

	public bool whiteToMove;

	public GameState(bool _castleKingsideW, bool _castleKingsideB, bool _castleQueensideW,bool _castleQueensideB, int _enPassantFileIndex, bool _whiteToMove) {
		castleKingsideW = _castleKingsideW;
		castleKingsideB = _castleKingsideB;
		castleQueensideW = _castleQueensideW;
		castleQueensideB = _castleQueensideB;

		enPassantFileIndex = _enPassantFileIndex;
		whiteToMove = _whiteToMove;
	}
}