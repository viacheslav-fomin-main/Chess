using System.Collections.Generic;

public class MoveGenerator {

	static int[] knightOverlay = new int[]{6, 15, 17, 10, -6, -15, -17, -10};
	static int[] kingOverlay = new int[]{8, 1, -8, -1, 7, 9, -7, -9};
	static int[] orthogonalOverlay = new int[]{8, 1, -8, -1};
	static int[] diagonalOverlay = new int[]{7, 9, -7, -9};

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

		for (int moveFromIndex =0; moveFromIndex <= 63; moveFromIndex ++) {
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
					else if (colourToMove == 0 && moveFromIndex == 60) { // black king still on starting square
						if ((Board.currentGamestate >> 3 & 1) == 1) { // has 0-0 right
							if (Board.boardArray[61] == 0 && Board.boardArray[62] == 0) { // no pieces blocking castling
								CreateKingMove(60,62,61,true);
							}
						}
						if ((Board.currentGamestate >> 4 & 1) == 1) { // has 0-0-0 right
							if (Board.boardArray[59] == 0 && Board.boardArray[58] == 0 && Board.boardArray[57] == 0) { // no pieces blocking castling
								CreateKingMove(60,58,59,true);
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
					moveToIndex = moveFromIndex + pawnDirection*8;
					if (Board.boardArray[moveToIndex] == 0) { // pawn can only move forward to unoccupied square
						if (moveToIndex >= 56 || moveToIndex <= 7) { // pawn is promoting
							CreateMove(moveFromIndex,moveToIndex, 0); // promote to queen
							CreateMove(moveFromIndex,moveToIndex, 1); // promote to rook
							CreateMove(moveFromIndex,moveToIndex, 2); // promote to knight
							CreateMove(moveFromIndex,moveToIndex, 3); // promote to bishop
						}
						else {
							CreateMove(moveFromIndex,moveToIndex); // regular pawn move

							if ((moveFromIndex <= 15 && colourToMove == 1) || (moveFromIndex >= 48 && colourToMove == 0)) { // pawn on starting rank
								moveToIndex = moveFromIndex + pawnDirection * 16; 
								if (Board.boardArray[moveToIndex] == 0) { // if no pieces blocking double pawn push
									CreateMove(moveFromIndex,moveToIndex); // move two squares
								}
							}

							// pawn captures
							if (moveFromIndex %8 != 0) { // if not on left edge of board
								moveToIndex = moveFromIndex + (8-pawnDirection) * pawnDirection; // capture left (from white's pov)
								if (Board.boardColourArray[moveToIndex] == opponentColour) { // if capture square contains opponent piece
									CreateMove(moveFromIndex,moveToIndex);
								}
							}
							if ((moveFromIndex+1) %8 != 0) { // if not on right edge of board
								moveToIndex = moveFromIndex + (8+pawnDirection) * pawnDirection; // capture right (from white's pov)
								if (Board.boardColourArray[moveToIndex] == opponentColour) { // if capture square contains opponent piece
									CreateMove(moveFromIndex,moveToIndex);
								}
							}
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
						for (int i =0; i <= 63; i ++) {
							moveToIndex = startIndex + kingOverlay[overlayIndex] * i;
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

		ushort newMove = (ushort)(fromIndex | toIndex << 6 | promotionPieceIndex << 12);
		moves.Add (newMove);
	}

	/// Creates and adds king move to move list. Also checks legality if not in psuedolegal mode.
	/// castleThroughIndex is the square which king passes through during castling (so that can't castle through check)
	void CreateKingMove(int fromIndex, int toIndex, int castleThroughIndex, bool isCastles) {
		if (!psuedolegalMode) { // if not in psuedolegal mode, elimate moves that leave king in check / caslting through check
			if (isCastles) {

			}
		}
		
		ushort newMove = (ushort)(fromIndex | toIndex << 6);
		moves.Add (newMove);
	}

	bool IndexOnBoard(int squareIndex) {
		return squareIndex >= 0 && squareIndex <= 63;
	}
	/*

	/// Returns a list of all moves in the current position
	public List<Move> AllMoves() {


		List<Move> allMoves = new List<Move> ();

		
		return allMoves;
	}

	/// Returns all quiet (i.e non-capture) moves in the position.
	/// Currently checks are considered quiet moves *if they do not capture a piece*
	public List<Move> AllQuietMoves() {
		List<Move> quietMoves = new List<Move> ();

		bool whiteToMove = Board.currentGamestate.whiteToMove;

		// Iterate through all squares on board
		for (int x = 0; x < 7; x ++) {
			for (int y = 0; y < 7; y ++) {
				int squareIndex = y*8 + x;
				int pieceTypeCode = Board.boardArray[x,y] & ~1;
				bool whitePiece = (Board.boardArray[x,y] & 1) == 1;

				// Look at all pieces of side to move
				if (Board.currentGamestate.whiteToMove == whitePiece) {
					if (pieceTypeCode == Board.kingCode) { // king moves
						for (int i = 0; i < kingOverlay.Length; i ++) {
							Move newMove;
							if (TryCreateMove(out newMove, squareIndex, squareIndex + kingOverlay[i], true)) {
								quietMoves.Add(newMove);
							}
						}
						if (Board.currentGamestate.CanCastleKingside(whitePiece)) { // kingside castling

						}
						if (Board.currentGamestate.CanCastleQueenside(whitePiece)) { // queenside castling
							
						}
					}
					else if (pieceTypeCode == Board.knightCode) { // knight moves
						
					}
					else if (pieceTypeCode == Board.pawnCode) { // pawns moves
						
					}
					else { // queen rook and bishop moves

					}
				}

			}
		}

		
		return quietMoves;
	}

	/// Returns all moves that capture a piece
	public List<Move> AllCaptures() {
		List<Move> captureMoves = new List<Move> ();
		
		
		return captureMoves;
	}

	bool IndexOnBoard(int squareIndex) {
		return squareIndex >= 0 && squareIndex <= 63;
	}

	bool TryCreateMove(out Move move, int fromIndex, int toIndex, bool isKing = false) {
		move = new Move (fromIndex, toIndex);

		// If given indices are out of bounds, reject move
		if (!IndexOnBoard(fromIndex) || !IndexOnBoard(toIndex)) {
			return false;
		}

		// If move gen is not in psuedolegal mode, ensure move is fully legal (i.e look for pins, check blocks)
		if (!generatePsuedolegalMoves) {
			if (!isKing) {

			}
		}

		return true;
	}
	*/
}
