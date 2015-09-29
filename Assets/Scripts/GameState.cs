
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
	// If there is no en passant square, this is set to -1
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

	/// Sets castling rights for specified colour
	public void SetCastlingRights(bool white, bool kingside, bool queenside) {
		if (white) {
			castleKingsideW = kingside;
			castleQueensideW = queenside;
		} else {
			castleKingsideB = kingside;
			castleQueensideB = queenside;
		}
	}

	public bool CanCastleKingside(bool white) {
		return (white)?castleKingsideW:castleKingsideB;
	}

	public bool CanCastleQueenside(bool white) {
		return (white)?castleQueensideW:castleQueensideB;
	}
	
}