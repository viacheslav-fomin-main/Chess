using System.Collections.Generic;

public class MoveGenerator {

	static int[] knightOverlay = new int[]{14, 31, 33, 18, -14, -31, -33, -18};
	static int[] kingOverlay = new int[]{16, 1, -16, -1, 15, 17, -15, -17};
	static int[] orthogonalOverlay = new int[]{16, 1, -16, -1};
	static int[] diagonalOverlay = new int[]{15, 17, -15, -17};

	/// If true, move generator will not worry about checks when generating moves (ignores pins etc)
	/// This can be used for faster move gen if king captures are going to be rejected in search
	bool psuedolegalMode;

	List<ushort> moves;

	public List<ushort> GetMoves(bool capturesOnly, bool psuedolegal) {
		psuedolegalMode = psuedolegal;
		moves = new List<ushort> (128); // I imagine that most positions will yield less than 128 psuedolegal moves. (The greatest known number of legal moves available in a position is 218)

		if (capturesOnly) {
			GenerateCaptureMoves();
		} else {
			GenerateAllMoves();
		}

		return moves;
	}

	/// Generate all moves
	void GenerateAllMoves() {

		int colourToMove = Board.currentGamestate & 1;
		int opponentColour = 1 - colourToMove;
		int moveToIndex;

		for (int moveFromIndex =0; moveFromIndex <= 127; moveFromIndex ++) {
			if ((moveFromIndex & 8) != 0) { // don't look at indices which are not on the real board
				continue;
			}
			if (Board.boardColourArray[moveFromIndex] == colourToMove) { // only find moves for piece of correct colour
				int movePieceType = Board.boardArray [moveFromIndex] & ~1; // piece type code

				// Moving the king:
				if (movePieceType == Board.kingCode) {
					for (int overlayIndex = 0; overlayIndex < kingOverlay.Length; overlayIndex ++) {
						moveToIndex = moveFromIndex + kingOverlay[overlayIndex];
						if (IndexOnBoard(moveToIndex)) {
							if (Board.boardColourArray[moveToIndex] != colourToMove) { // can't move to square occupied by friendly piece
								CreateKingMove(moveFromIndex,moveToIndex,0,false);
							}
						}
					}

					// Castling:
					if (colourToMove == 1 && moveFromIndex == 4) { // white king still on starting square
						if ((Board.currentGamestate >> 1 & 1) == 1) { // has 0-0 right
							if (Board.boardArray[5] == 0 && Board.boardArray[6] == 0) { // no pieces blocking castling
								CreateKingMove(4,6,5,true);
							}
						}
						if ((Board.currentGamestate >> 2 & 1) == 1) { // has 0-0-0 right
							if (Board.boardArray[3] == 0 && Board.boardArray[2] == 0 && Board.boardArray[1] == 0) { // no pieces blocking castling
								CreateKingMove(4,2,3,true);
							}
						}
					}
					else if (colourToMove == 0 && moveFromIndex == 116) { // black king still on starting square
						if ((Board.currentGamestate >> 3 & 1) == 1) { // has 0-0 right
							if (Board.boardArray[117] == 0 && Board.boardArray[118] == 0) { // no pieces blocking castling
								CreateKingMove(116,118,117,true);
							}
						}
						if ((Board.currentGamestate >> 4 & 1) == 1) { // has 0-0-0 right
							if (Board.boardArray[115] == 0 && Board.boardArray[114] == 0 && Board.boardArray[113] == 0) { // no pieces blocking castling
								CreateKingMove(116,114,115,true);
							}
						}
					}
				}

				// Moving the knight:
				else if (movePieceType == Board.knightCode) {
					for (int overlayIndex = 0; overlayIndex < knightOverlay.Length; overlayIndex ++) {
						moveToIndex = moveFromIndex + knightOverlay[overlayIndex];
						if (IndexOnBoard(moveToIndex)) {
							if (Board.boardColourArray[moveToIndex] != colourToMove) { // can't move to square occupied by friendly piece
								CreateMove(moveFromIndex,moveToIndex);
							}
						}
					}
				}
				// Moving a pawn:
				else if (movePieceType == Board.pawnCode) {
					int pawnDirection = (colourToMove == 1)?1:-1;
					moveToIndex = moveFromIndex + pawnDirection*16;
					if (Board.boardArray[moveToIndex] == 0) { // square in front of pawn is unnocupied
						if (moveToIndex >= 112 || moveToIndex <= 7) { // pawn is promoting
							CreateMove(moveFromIndex,moveToIndex, 0); // promote to queen
							CreateMove(moveFromIndex,moveToIndex, 1); // promote to rook
							CreateMove(moveFromIndex,moveToIndex, 2); // promote to knight
							CreateMove(moveFromIndex,moveToIndex, 3); // promote to bishop
						}
						else {
							CreateMove(moveFromIndex,moveToIndex); // regular pawn move

							if ((moveFromIndex <= 23 && colourToMove == 1) || (moveFromIndex >= 96 && colourToMove == 0)) { // pawn on starting rank
								moveToIndex = moveFromIndex + pawnDirection * 32; 
								if (Board.boardArray[moveToIndex] == 0) { // if no pieces blocking double pawn push
									CreateMove(moveFromIndex,moveToIndex); // move two squares
								}
							}
						}
					}
					// pawn captures
					moveToIndex = moveFromIndex + (16-pawnDirection) * pawnDirection; // capture left (from white's pov)
					if (IndexOnBoard(moveToIndex)) {
						if (Board.boardColourArray[moveToIndex] == opponentColour) { // if capture square contains opponent piece
							CreateMove(moveFromIndex,moveToIndex);
						}
					}
					moveToIndex = moveFromIndex + (16+pawnDirection) * pawnDirection; // capture right (from white's pov)
					if (IndexOnBoard(moveToIndex)) {
						if (Board.boardColourArray[moveToIndex] == opponentColour) { // if capture square contains opponent piece
							CreateMove(moveFromIndex,moveToIndex);
						}
					}
				}
				// Queen, rook and bishop
				else {
					int startIndex = 0;
					int endIndex = 7;

					if (movePieceType == Board.bishopCode) {
						startIndex = 4; // skip horizontal overlays
					}
					else if (movePieceType == Board.rookCode) {
						endIndex = 3; // skip diagonal overlays
					}

					for (int overlayIndex = startIndex; overlayIndex <= endIndex; overlayIndex ++) {
						for (int i =1; i <= 8; i ++) {
							moveToIndex = moveFromIndex + kingOverlay[overlayIndex] * i;
							bool lineOpen = IndexOnBoard(moveToIndex);
							if (lineOpen) {
								if (Board.boardArray[moveToIndex] != 0) { // something is obstructing movement
									lineOpen = false;
								}
								if (Board.boardColourArray[moveToIndex] != colourToMove) { // if square is not friendly, i.e contains enemy or no piece, square can be moves to
									CreateMove(moveFromIndex, moveToIndex);
								}
							}
							if (!lineOpen) {
								break; // stop searching this line once it has reached obstruction/end of board
							}
						}
					}
				}
			}
		}
	}

	/// Returns true if the given colour player attacks the given square. This can be used for detecting checks etc.
	bool SquareAttackedByPlayer(int squareIndex, int colour) {
		return false;
	}

	// Generates all moves that are captures. TODO: this should at some point be changed to 'aggressive moves' and include moves that deliver checks.
	void GenerateCaptureMoves() {
		
	}

	/// Creates and adds move to move list. Also checks legality if not in psuedolegal mode
	/// Note: for king moves use separate CreateKingMove method
	void CreateMove(int fromIndex, int toIndex, int promotionPieceIndex = 0) {
		if (!psuedolegalMode) { // if not in psuedolegal mode, elimate moves that leave king in check

		}

		ushort newMove = (ushort)(fromIndex | toIndex << 7 | promotionPieceIndex << 14);
		moves.Add (newMove);
	}

	/// Creates and adds king move to move list. Also checks legality if not in psuedolegal mode.
	/// castleThroughIndex is the square which king passes through during castling (so that can't castle through check)
	void CreateKingMove(int fromIndex, int toIndex, int castleThroughIndex, bool isCastles) {
		if (!psuedolegalMode) { // if not in psuedolegal mode, elimate moves that leave king in check / caslting through check
			if (isCastles) {

			}
		}
		
		ushort newMove = (ushort)(fromIndex | toIndex << 7);
		moves.Add (newMove);
	}

	bool IndexOnBoard(int squareIndex) {
		return (squareIndex & 136) == 0;
	}
}
