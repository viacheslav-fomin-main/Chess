using System.Collections.Generic;
using System;

public class MoveGenerator {
	
	static int[] knightOverlay = new int[]{14, 31, 33, 18, -14, -31, -33, -18};
	static int[] kingOverlay = new int[]{16, 1, -16, -1, 15, 17, -15, -17};
	static int[] orthogonalOverlay = new int[]{16, 1, -16, -1};
	static int[] diagonalOverlay = new int[]{15, 17, -15, -17};
	
	public static bool trackStats;
	public static int captures;
	public static int castles;
	public static int promotions;
	
	/// If true, move generator will not worry about checks when generating moves (ignores pins etc)
	/// This can be used for faster move gen if king captures are going to be rejected in search
	bool pseudolegalMode;
	Heap moves;
	
	int friendlyKingIndex;
	int moveColour;
	int opponentColour;

	bool inCheck;

	public static void Debug() {
		UnityEngine.Debug.Log ("hash calls: " + hashCalls);
	}
	
	public void SetMoveColour(int c) {
		moveColour = c;
	}
	
	public Heap GetMoves(bool capturesOnly, bool pseudolegal, bool autoSetMoveColour = true) { // autoset move colour means move colour will be taken from current position. Otherwise can be custom set using SetMoveColour method
		pseudolegalMode = pseudolegal;
		moves = new Heap(128); // I imagine that most positions will yield less than 128 psuedolegal moves. (The greatest known number of legal moves available in a position is 218)
		
		if (autoSetMoveColour) {
			moveColour = Board.currentGamestate & 1;
		}
		
		opponentColour = 1-moveColour;
		friendlyKingIndex = (moveColour == 1)?Board.whiteKingIndex:Board.blackKingIndex;
		inCheck = SquareAttackedByEnemy (friendlyKingIndex);

		
		if (capturesOnly) {
			GenerateCaptureMoves ();
		} else {
			GenerateAllMoves ();
		}
		
		return moves;
	}

	/// Not optimised; only intended for use by UI etc
	public bool PositionIsCheck() {
		moveColour = Board.currentGamestate & 1;
		
		opponentColour = 1-moveColour;
		friendlyKingIndex = (moveColour == 1)?Board.whiteKingIndex:Board.blackKingIndex;
		return SquareAttackedByEnemy (friendlyKingIndex);
	}

	/// Not optimised; only intended for use by UI etc
	public bool PositionIsMate() {
		return (GetMoves (false, false).Count == 0 && PositionIsCheck());
	}

	public bool PositionIsStaleMate() {
		return (GetMoves (false, false).Count == 0 && !PositionIsCheck());
	}

	public bool InsuffientMatingMaterial() {
		// can mate (unless some sort of blockade, but not considering that for the moment) if there are any pawns, rooks or queens left on the board.
		if (Board.blackPawnCount + Board.whitePawnCount != 0 || Board.blackQueenCount + Board.whiteQueenCount != 0 || Board.blackRookCount + Board.whiteRookCount != 0) {
			return false;
		}

		// if either side has two bishops mate is still possible (taken for granted that bishops are of diff colour, though of course through promotion it is possible that they are not)
		if (Board.blackBishopCount >= 2 || Board.whiteBishopCount >= 2) {
			return false;
		}


		return true;
	}

	void GenerateMove(int moveFromIndex) {
		int moveToIndex;

		int movePieceType = Board.boardArray [moveFromIndex] & ~1; // piece type code
		
		// Moving the king:
		if (movePieceType == Board.kingCode) {
			for (int overlayIndex = 0; overlayIndex < kingOverlay.Length; overlayIndex ++) {
				moveToIndex = moveFromIndex + kingOverlay[overlayIndex];
				if (IndexOnBoard(moveToIndex)) {
					if (Board.boardColourArray[moveToIndex] != moveColour) { // can't move to square occupied by friendly piece
						CreateKingMove(moveFromIndex,moveToIndex,0,false);
					}
				}
			}
			
			// Castling:
			if (moveColour == 1 && moveFromIndex == 4) { // white king still on starting square
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
			else if (moveColour == 0 && moveFromIndex == 116) { // black king still on starting square
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
					if (Board.boardColourArray[moveToIndex] != moveColour) { // can't move to square occupied by friendly piece
						CreateMove(moveFromIndex,moveToIndex);
					}
				}
			}
		}
		// Moving a pawn:
		else if (movePieceType == Board.pawnCode) {
			int pawnDirection = (moveColour == 1)?1:-1;
			moveToIndex = moveFromIndex + pawnDirection*16;
			if (moveToIndex<0 || moveToIndex >= Board.boardArray.Length) {
				Board.DebugGameState(Board.currentGamestate);
				UnityEngine.Debug.Log("Pawn error: move to: " + moveToIndex + "  from: " + moveFromIndex + "  dir: " + pawnDirection);
			}
			if (Board.boardArray[moveToIndex] == 0) { // square in front of pawn is unnocupied
				CreatePawnMove(moveFromIndex,moveToIndex); // regular pawn move
				
				if ((moveFromIndex <= 23 && moveColour == 1) || (moveFromIndex >= 96 && moveColour == 0)) { // pawn on starting rank
					moveToIndex = moveFromIndex + pawnDirection * 32; 
					if (Board.boardArray[moveToIndex] == 0) { // if no pieces blocking double pawn push
						CreatePawnMove(moveFromIndex,moveToIndex); // move two squares
					}
				}
			}
			int epCaptureIndex = (Board.currentGamestate >> 5 & 15) -1 + ((opponentColour == 0)?80:32);
			// pawn captures
			moveToIndex = moveFromIndex + (16-pawnDirection) * pawnDirection; // capture left (from white's pov)
			if (IndexOnBoard(moveToIndex)) {
				if (Board.boardColourArray[moveToIndex] == opponentColour || moveToIndex == epCaptureIndex) { // if capture square contains opponent piece or is ep capture square
					CreatePawnMove(moveFromIndex,moveToIndex,epCaptureIndex); // TODO: ep capture index parameter is only for stat counting. Remove.
				}
			}
			moveToIndex = moveFromIndex + (16+pawnDirection) * pawnDirection; // capture right (from white's pov)
			if (IndexOnBoard(moveToIndex)) {
				if (Board.boardColourArray[moveToIndex] == opponentColour || moveToIndex == epCaptureIndex) { // if capture square contains opponent piece or is ep capture square
					CreatePawnMove(moveFromIndex,moveToIndex,epCaptureIndex); // TODO: ep capture index parameter is only for stat counting. Remove.
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
						if (Board.boardColourArray[moveToIndex] != moveColour) { // if square is not friendly, i.e contains enemy or no piece, square can be moves to
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
	
	/// Generate all moves
	void GenerateAllMoves() {
		GenerateMove (friendlyKingIndex); // generate king moves first to minimize recalculation of attack squares

		for (int i =0; i <= 127; i ++) {
			if ((i & 8) != 0) { // don't look at indices which are not on the real board
				continue;
			}

			if (Board.boardColourArray[i] == moveColour) { // only find moves for piece of correct colour
				int movePieceType = Board.boardArray [i] & ~1; // piece type code
				if (movePieceType != Board.kingCode) { // don't calculate king moves again
					GenerateMove(i);
				}
			}
		}

	}

	// Generates all moves that are captures. TODO: this should at some point be changed to 'aggressive moves' and include moves that deliver checks.
	void GenerateCaptureMoves() {
		int opponentColour = 1 - moveColour;
		int moveToIndex;
		
		for (int moveFromIndex =0; moveFromIndex <= 127; moveFromIndex ++) {
			if ((moveFromIndex & 8) != 0) { // don't look at indices which are not on the real board
				continue;
			}
			if (Board.boardColourArray[moveFromIndex] == moveColour) { // only find moves for piece of correct colour
				int movePieceType = Board.boardArray [moveFromIndex] & ~1; // piece type code
				
				// Moving the king:
				if (movePieceType == Board.kingCode) {
					for (int overlayIndex = 0; overlayIndex < kingOverlay.Length; overlayIndex ++) {
						moveToIndex = moveFromIndex + kingOverlay[overlayIndex];
						if (IndexOnBoard(moveToIndex)) {
							if (Board.boardColourArray[moveToIndex] == opponentColour) { 
								CreateKingMove(moveFromIndex,moveToIndex,0,false);
							}
						}
					}
				}
				
				// Moving the knight:
				else if (movePieceType == Board.knightCode) {
					for (int overlayIndex = 0; overlayIndex < knightOverlay.Length; overlayIndex ++) {
						moveToIndex = moveFromIndex + knightOverlay[overlayIndex];
						if (IndexOnBoard(moveToIndex)) {
							if (Board.boardColourArray[moveToIndex] == opponentColour) { // can't move to square occupied by friendly piece
								CreateMove(moveFromIndex,moveToIndex);
							}
						}
					}
				}
				// Moving a pawn:
				else if (movePieceType == Board.pawnCode) {
					int pawnDirection = (moveColour == 1)?1:-1;
					int epCaptureIndex = (Board.currentGamestate >> 5 & 15) -1 + ((opponentColour == 0)?80:32);
					// pawn captures
					moveToIndex = moveFromIndex + (16-pawnDirection) * pawnDirection; // capture left (from white's pov)
					if (IndexOnBoard(moveToIndex)) {
						if (Board.boardColourArray[moveToIndex] == opponentColour || moveToIndex == epCaptureIndex) { // if capture square contains opponent piece or is ep capture square
							CreatePawnMove(moveFromIndex,moveToIndex,epCaptureIndex); // TODO: ep capture index parameter is only for stat counting. Remove.
						}
					}
					moveToIndex = moveFromIndex + (16+pawnDirection) * pawnDirection; // capture right (from white's pov)
					if (IndexOnBoard(moveToIndex)) {
						if (Board.boardColourArray[moveToIndex] == opponentColour || moveToIndex == epCaptureIndex) { // if capture square contains opponent piece or is ep capture square
							CreatePawnMove(moveFromIndex,moveToIndex,epCaptureIndex); // TODO: ep capture index parameter is only for stat counting. Remove.
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
								if (Board.boardColourArray[moveToIndex] == opponentColour) { // capture piece
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
	

	
	// Returns true if the square is under attack. Move from and to indices are required so that enemy attack table is only updated if the move will affect it
	bool InCheckAfterMove(int moveFromIndex, int moveToIndex, int capturePieceType = 0) {
		bool inCheckAfterMove = inCheck;

		// Figure out if new move could have affected if in check:

		if (inCheck) { // in check before this move was made, thus it is important to consider the squares this new move will block
			int offsetTo = moveToIndex - friendlyKingIndex;
			if (offsetTo % 15 == 0 || offsetTo % 16 == 0 || offsetTo % 17 == 0 || Math.Abs (offsetTo) <= 7) {
				inCheckAfterMove = SquareAttackedByEnemy (friendlyKingIndex); 
			} else if (capturePieceType == Board.knightCode) { // knights are only pieces which are not in line with the square they attack
				for (int i = 0; i < knightOverlay.Length; i ++) { 
					if (moveToIndex + knightOverlay [i] == friendlyKingIndex) { // if knight that has been captured was attacking the examined square then hash must be updated
						inCheckAfterMove = SquareAttackedByEnemy (friendlyKingIndex);
						break;
					}
				}
			}
		}
		else { // if not in check before move, then check if move could have uncovered an enemy attacker
			int offsetFrom = moveFromIndex - friendlyKingIndex;
			if (offsetFrom % 15 == 0 || offsetFrom % 16 == 0 || offsetFrom % 17 == 0 || Math.Abs (offsetFrom) <= 7) { 
				inCheckAfterMove = SquareAttackedByEnemy (friendlyKingIndex); 
			}
		}

		return inCheckAfterMove;
	}

	bool SquareAttackedByEnemy(int squareIndexToCheck) {
		int moveToIndex;
		
		// Knight attacks
		for (int overlayIndex = 0; overlayIndex < knightOverlay.Length; overlayIndex ++) {
			moveToIndex = squareIndexToCheck + knightOverlay[overlayIndex];
			if (IndexOnBoard(moveToIndex)) {
				if (Board.boardArray[moveToIndex] == Board.knightCode + opponentColour) {
					return true; // square is attacked by knight
				}
			}
		}
		
		int pawnDir = (opponentColour == 1) ? 1 : -1;
		// Diagonal attacks
		for (int overlayIndex = 0; overlayIndex < diagonalOverlay.Length; overlayIndex ++) {
			for (int i =1; i <= 8; i ++) {
				moveToIndex = squareIndexToCheck + diagonalOverlay[overlayIndex] * i;
				if (IndexOnBoard(moveToIndex)) {
					if (Board.boardColourArray[moveToIndex] != -1) { // if square is not empty
						if (i == 1) { // could be attacked by a pawn/king
							if (Board.boardArray[moveToIndex] == Board.kingCode + opponentColour) {
								return true; // target square attacked by king
							}
							if (Math.Sign(squareIndexToCheck-moveToIndex) == pawnDir) { // in correct direction to be attacked by pawn
								if (Board.boardArray[moveToIndex] == Board.pawnCode + opponentColour) {
									return true; // target square attacked by pawn
								}
							}
						}
						if (Board.boardArray[moveToIndex] == Board.bishopCode + opponentColour || Board.boardArray[moveToIndex] == Board.queenCode + opponentColour) { // piece is bishop or queen
							return true; // target square attacked by bishop/queen
						}
						else {
							break; // piece is obstructing further attacks on this line
						}
					}
				}
				else {
					break;
				}
				
			}
		}
		
		// Orthogonal attacks
		for (int overlayIndex = 0; overlayIndex < orthogonalOverlay.Length; overlayIndex ++) {
			for (int i =1; i <= 8; i ++) {
				moveToIndex = squareIndexToCheck + orthogonalOverlay[overlayIndex] * i;
				if (IndexOnBoard(moveToIndex)) {
					if (Board.boardColourArray[moveToIndex] != -1) { // if square is not empty
						
						if (i == 1) { // could be attacked by king
							if (Board.boardArray[moveToIndex] == Board.kingCode + opponentColour) {
								return true; // target square attacked by king
							}
						}
						if (Board.boardArray[moveToIndex] == Board.rookCode + opponentColour || Board.boardArray[moveToIndex] == Board.queenCode + opponentColour) { // piece is rook or queen
							return true; // target square attacked by rook/queen
						}
						else {
							break; // piece is obstructing further attacks on this line
						}
					}
				}
				else {
					break;
				}
				
			}
		}
		
		return false;
	}


	
	static int hashCalls;
	
	/// Creates and adds move to move list. Also checks legality if not in psuedolegal mode
	/// Note: for king moves use separate CreateKingMove method
	void CreateMove(int fromIndex, int toIndex) {
		ushort newMove = (ushort)(fromIndex | toIndex << 7);
		int capturedPieceType = Board.boardArray[toIndex] &~1;

		
		if (!pseudolegalMode) { // if not in psuedolegal mode, elimate moves that leave king in check
			Board.MakeMove(newMove);
			bool inCheckAfterMove = InCheckAfterMove(fromIndex, toIndex, capturedPieceType);
			Board.UnmakeMove(newMove);
			if (inCheckAfterMove) {
				return;
			}
		}
		
		if (trackStats) {
			if (Board.boardColourArray[toIndex] == (1-moveColour)) {
				captures ++;
			}
		}

		int movePieceType = Board.boardArray [fromIndex] & ~1;
		int moveOrderEval = 0;
		if (capturedPieceType != 0) { // capture move
			moveOrderEval = PieceEvalFromType(capturedPieceType) + 100 - PieceEvalFromType(movePieceType); // prefer capturing high-value enemy piece with low-value friendly piece

		} else { // non capture move
			int toRank = Board.RankFrom128(toIndex);
			int direction = (moveColour==1)?1:-1;
			if (Math.Sign(toRank - Board.RankFrom128(fromIndex)) != direction) { // penalise retreating moves
				moveOrderEval = - 10;
			}

		}
		
		moves.Add (newMove,(short)moveOrderEval);
	}
	
	/// Creates and adds move to move list. Also checks legality if not in psuedolegal mode
	/// Note: for king moves use separate CreateKingMove method
	void CreatePawnMove(int fromIndex, int toIndex, int epCaptureIndex = -1) {
		ushort newMove = (ushort)(fromIndex | toIndex << 7);
		int capturedPieceType = Board.boardArray[toIndex] &~1;

		if (!pseudolegalMode) { // if not in psuedolegal mode, elimate moves that leave king in check

			Board.MakeMove(newMove);
			bool inCheckAfterMove = InCheckAfterMove(fromIndex, toIndex, capturedPieceType);
			//bool inCheck = SquareAttackedByPlayer(friendlyKingIndex,(1-moveColour));
			Board.UnmakeMove(newMove);
			if (inCheckAfterMove) {
				return;
			}
		}
		
		if (trackStats) {
			if (Board.boardColourArray[toIndex] == (1-moveColour) || toIndex == epCaptureIndex) {
				captures ++;
			}
		}


		
		if (toIndex >= 112 || toIndex <= 7) { // pawn is promoting
			if (trackStats) {
				promotions += 4;
			}
			moves.Add (newMove, 100); //promote to queen 
			moves.Add ((ushort)(newMove | 1 << 14), -80); // rook
			moves.Add ((ushort)(newMove | 2 << 14), -70); // knight
			moves.Add ((ushort)(newMove | 3 << 14), - 100); // bishop
		} else {
			int movePieceType = Board.boardArray [fromIndex] & ~1;
			int moveOrderEval = 0;
			int toRank = Board.RankFrom128(toIndex);

			if (movePieceType == Board.pawnCode) { // prefer to move pawn the further advanced it is
				if (moveColour == 1) {
					if (toRank > 5) {
						moveOrderEval = 10 * (toRank-5);
					}
				}
				else {
					if (toRank < 4) {
						moveOrderEval = 10 * (4-toRank);
					}
				}
			}
			moves.Add (newMove, (short)moveOrderEval); // regular pawn move
		}
		
	}
	
	/// Creates and adds king move to move list. Also checks legality if not in psuedolegal mode.
	/// castleThroughIndex is the square which king passes through during castling (so that can't castle through check)
	void CreateKingMove(int fromIndex, int toIndex, int castleThroughIndex, bool isCastles) {
		ushort newMove = (ushort)(fromIndex | toIndex << 7);
		int capturedPieceType = Board.boardArray[toIndex] &~1;
		int moveOrderEval = 0;
		if (!pseudolegalMode) { // if not in psuedolegal mode, elimate moves that leave king in check / castling through check

			Board.MakeMove(newMove);
			bool inCheckAfterMove = SquareAttackedByEnemy(toIndex);
			Board.UnmakeMove(newMove);
			if (inCheckAfterMove) {
				return;
			}
		}

		if (isCastles) {
			if (inCheck) {
				return;
			}

			if (SquareAttackedByEnemy(castleThroughIndex)) { // cannot castle if castling through check
				return;
			}
		
			if (trackStats) {
				castles ++;
			}
		}
		
		if (trackStats) {
			if (Board.boardColourArray[toIndex] == (1-moveColour)) {
				captures ++;
			}
		}
		
		
		moves.Add (newMove, (short)moveOrderEval);
	}

	/// Value between 0 and 100 representing piece worth
	int PieceEvalFromType(int type) {
		switch (type) {
		case Board.pawnCode:
			return 10;
			break;
		case Board.queenCode:
			return 90;
			break;
		case Board.rookCode:
			return 50;
			break;
		case Board.knightCode:
			return 30;
			break;
		case Board.bishopCode:
			return 35;
			break;
		}
		return 0;
	}
	
	bool IndexOnBoard(int squareIndex) {
		return (squareIndex & 136) == 0;
	}
}
