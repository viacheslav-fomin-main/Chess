using System.Collections.Generic;
using System.Collections;
using System;

public class MoveGenerator : IMoveGenerator {
	
	public Move[] GetAllLegalMoves(Position position) {
		List<Move> movesFound = new List<Move> ();

		bool isWhite = position.gameState.whiteToMove;
		BitBoard friendlyPieces = (isWhite) ? position.allPiecesW : position.allPiecesB;
		BitBoard hostilePieces = (!isWhite) ? position.allPiecesW : position.allPiecesB;

		List<BitBoard> pseudolegalMoveBoards = new List<BitBoard> ();
		List<Coord> origins = new List<Coord> ();

		CheckInfo checkInfo = GetCheckInfo (position);

		BitBoard kingMoves = PseudoLegalMovesOnPopulatedBoard (Definitions.PieceName.King, checkInfo.kingSquare, position);
		kingMoves.And(~checkInfo.hostileControlledSquares.board); // king can't move into check
		if ((isWhite && (position.gameState.castleKingsideW || position.gameState.castleQueensideW)) || (!isWhite && (position.gameState.castleKingsideB || position.gameState.castleQueensideB))) { // if able to castle on one or both sides
			if (checkInfo.hostileControlledSquares.ContainsPieceAtSquare(new Coord(checkInfo.kingSquare.x-1, checkInfo.kingSquare.y))) { // can't castle queenside if passes through check
				kingMoves.SetSquare(new Coord(checkInfo.kingSquare.x-2, checkInfo.kingSquare.y), false);
			}
			if (checkInfo.hostileControlledSquares.ContainsPieceAtSquare(new Coord(checkInfo.kingSquare.x+1, checkInfo.kingSquare.y))) { // can't castle kingside if passes through check
				kingMoves.SetSquare(new Coord(checkInfo.kingSquare.x+2, checkInfo.kingSquare.y), false);
			}
		}
		pseudolegalMoveBoards.Add (kingMoves);
		origins.Add (checkInfo.kingSquare);

		if (!checkInfo.inDoubleCheck) { // no pieces besides king can move when in double check
			for (int squareIndex =0; squareIndex < 64; squareIndex ++) {
				if (friendlyPieces.ContainsPieceAtSquare (squareIndex)) {
					Coord square = new Coord (squareIndex);
					BitBoard pseudoLegalMoveBoard = new BitBoard ();

					if (position.pawnsB.ContainsPieceAtSquare (squareIndex) || position.pawnsW.ContainsPieceAtSquare (squareIndex)) {
						pseudoLegalMoveBoard = PseudoLegalMovesOnPopulatedBoard (Definitions.PieceName.Pawn, square, position);
					} else if (position.rooksB.ContainsPieceAtSquare (squareIndex) || position.rooksW.ContainsPieceAtSquare (squareIndex)) {
						pseudoLegalMoveBoard = PseudoLegalMovesOnPopulatedBoard (Definitions.PieceName.Rook, square, position);
					} else if (position.knightsB.ContainsPieceAtSquare (squareIndex) || position.knightsW.ContainsPieceAtSquare (squareIndex)) {
						pseudoLegalMoveBoard = PseudoLegalMovesOnPopulatedBoard (Definitions.PieceName.Knight, square, position);
					} else if (position.bishopsB.ContainsPieceAtSquare (squareIndex) || position.bishopsW.ContainsPieceAtSquare (squareIndex)) {
						pseudoLegalMoveBoard = PseudoLegalMovesOnPopulatedBoard (Definitions.PieceName.Bishop, square, position);
					} else if (position.queensB.ContainsPieceAtSquare (squareIndex) || position.queensW.ContainsPieceAtSquare (squareIndex)) {
						pseudoLegalMoveBoard = PseudoLegalMovesOnPopulatedBoard (Definitions.PieceName.Queen, square, position);
					}

					if (checkInfo.inCheck) {
						pseudoLegalMoveBoard.And(checkInfo.checkBlockBoard.board); // if in check, pieces can only move to squares that will block/capture checking piece.
					}

					for (int lineDirIndex = 0; lineDirIndex < checkInfo.pinBoards.Count; lineDirIndex++) {
						if (checkInfo.pinBoards[lineDirIndex].ContainsPieceAtSquare(squareIndex)) {
							pseudoLegalMoveBoard.And(checkInfo.pinBoards[lineDirIndex].board); // if pinned, piece cannot move out of the pinning line
						}
					}

					pseudolegalMoveBoards.Add (pseudoLegalMoveBoard);
					origins.Add (square);
				}
			}
		}


		// convert bitboards to moves
		for (int i =0; i < pseudolegalMoveBoards.Count; i ++) {
			for (int squareIndex =0; squareIndex < 64; squareIndex ++) {
				if (pseudolegalMoveBoards[i].ContainsPieceAtSquare(squareIndex)) {
					Coord moveFrom = origins[i];
					Coord moveTo = new Coord(squareIndex);

					Move newMove = new Move();
					GameState newGameState = position.gameState;
					newGameState.whiteToMove = !newGameState.whiteToMove;

					// determine castling right after this move (white)
					if (isWhite && (newGameState.castleKingsideW || newGameState.castleQueensideW)) {
						if (moveFrom.algebraic == "e1") { // moving king
							newGameState.castleKingsideW = false;
							newGameState.castleQueensideW = false;
						}
						else if (moveFrom.algebraic == "a1") { // move rook
							newGameState.castleQueensideW = false;
						}
						else if (moveFrom.algebraic == "h1") {
							newGameState.castleKingsideW = false;
						}
					}
					// determine castling right after this move (black)
					if (!isWhite && (newGameState.castleKingsideB || newGameState.castleQueensideB)) {
						if (moveFrom.algebraic == "e8") { // moving king
							newGameState.castleKingsideB = false;
							newGameState.castleQueensideB = false;
						}
						else if (moveFrom.algebraic == "a8") { // move rook
							newGameState.castleQueensideB = false;
						}
						else if (moveFrom.algebraic == "h8") {
							newGameState.castleKingsideB = false;
						}
					}

					// is castling move:
					if (position.kingB.ContainsPieceAtSquare(moveFrom) || position.kingW.ContainsPieceAtSquare(moveFrom)) {
						int deltaMoveX = moveTo.x - moveFrom.x;
						if (Math.Abs(deltaMoveX) == 2) { // castling
							newMove.isCastles = true;
							if (deltaMoveX < 0) { // queenside
								newMove.rookFrom = new Coord(0,moveFrom.y);
								newMove.rookTo = new Coord(3,moveFrom.y);
							}
							else {
								newMove.rookFrom = new Coord(7,moveFrom.y);
								newMove.rookTo = new Coord(5,moveFrom.y);
							}

						}
					}

					// pawn info (en passant and promotion)
					newGameState.enPassantFileIndex = -1; // default ep value indicating no ep
					if (position.pawnsW.ContainsPieceAtSquare(moveFrom) || position.pawnsB.ContainsPieceAtSquare(moveFrom)) {
						// determine en passant square created
						if (Math.Abs(moveFrom.y - moveTo.y) == 2) { // pawn has moved two forward
							newGameState.enPassantFileIndex = moveFrom.x;
						}

						// determine en passant capture
						if (moveTo.x == position.gameState.enPassantFileIndex && moveFrom.x != moveTo.x) { // pawn captures to same file as ep square; therefore is capturing en passant
							newMove.isEnPassantCapture = true;
							newMove.enPassantPawnLocation = new Coord(moveTo.y, (isWhite)?4:3);// location of pawn that is being captured enpassant
						}

						// determine promotion
						if (moveTo.y == 7 || moveTo.y == 0) { // has reached first/last rank
							newMove.isPawnPromotion = true;
						}
					}

					newMove.from = moveFrom;
					newMove.to = moveTo;
					newMove.gameStateAfterMove = newGameState;
					newMove.whitesMove = isWhite;

					movesFound.Add(newMove);
				}
			}
		}


		return movesFound.ToArray();
	}
	
	/// Calculates all necessary information about checks/pins in the position
	CheckInfo GetCheckInfo(Position position) {
		CheckInfo checkInfo = new CheckInfo ();

		bool isWhite = position.gameState.whiteToMove;
		int friendlyKingSquareIndex = BitBoard.BitIndex (((isWhite) ? position.kingW : position.kingB).board);
		Coord friendlyKingPosition = new Coord (friendlyKingSquareIndex);

		BitBoard hostileControlledSquares = new BitBoard ();
		BitBoard allHostilePieces = (isWhite) ? position.allPiecesB : position.allPiecesW;
		BitBoard allFriendlyPieces = (!isWhite) ? position.allPiecesB : position.allPiecesW;

		BitBoard allPieces = BitBoard.Combination (position.allPiecesB, position.allPiecesW);
		allPieces.SetSquare (friendlyKingPosition,false); // remove the friendly king from board so that hostile attacks are not blocked by it (king cannot be used to block a check)
	
		// boards containing positions of all pieces
		BitBoard[] whitePieceBoards = new BitBoard[]{position.pawnsW, position.rooksW, position.knightsW, position.bishopsW, position.queensW, position.kingW};
		BitBoard[] blackPieceBoards = new BitBoard[]{position.pawnsB, position.rooksB, position.knightsB, position.bishopsB, position.queensB, position.kingB};
		BitBoard[] hostilePieceBoards = (isWhite) ? blackPieceBoards : whitePieceBoards;

		// order of the piece boards in the array
		Definitions.PieceName[] pieceBoardOrder = new Definitions.PieceName[] {
			Definitions.PieceName.Pawn,
			Definitions.PieceName.Rook,
			Definitions.PieceName.Knight,
			Definitions.PieceName.Bishop,
			Definitions.PieceName.Queen,
			Definitions.PieceName.King
		};

		BitBoard hostileOrthogonalPieces = BitBoard.Combination (hostilePieceBoards [1], hostilePieceBoards [4]); // mask of hostile rook and queen
		BitBoard hostileDiagonalPieces = BitBoard.Combination (hostilePieceBoards [3], hostilePieceBoards [4]); // mask of hostile bishop and queen
		BitBoard hostileKnights = hostilePieceBoards [2];
		BitBoard hostilePawns = hostilePieceBoards [0];

		int checkCount = 0; // number of checks delivered to friendly king

		// Get attack boards for each hostile piece and combine to form bitboard of all hostile-controlled squares
		for (int pieceBoardIndex = 0; pieceBoardIndex < hostilePieceBoards.Length; pieceBoardIndex ++) {
			for (int squareIndex = 0; squareIndex <= 63; squareIndex ++) {
				bool isPieceOnBoard = hostilePieceBoards[pieceBoardIndex].ContainsPieceAtSquare(squareIndex);

				if (isPieceOnBoard) {

					BitBoard pieceAttackBoard = AttackBitBoard(pieceBoardOrder[pieceBoardIndex], new Coord(squareIndex), allPieces, !isWhite);
					if (pieceAttackBoard.ContainsPieceAtSquare(friendlyKingSquareIndex)) { // incrememnt check count if attack square is same as friendly king
						checkCount ++;
					}

					hostileControlledSquares.Combine(pieceAttackBoard);
				}
			}
		}

		checkInfo.inCheck = (checkCount > 0);
		checkInfo.inDoubleCheck = (checkCount > 1);
		checkInfo.hostileControlledSquares = hostileControlledSquares;
		checkInfo.kingSquare = friendlyKingPosition;

		// Only calculate pin/check block boards if king is not in double check
		// Reason: If in double check the king is the only piece that can move, so info is unecessary.
		if (!checkInfo.inDoubleCheck) {
		
			List<Coord> checkBlockSquares = new List<Coord>();
			List<List<Coord>> pinLines = new List<List<Coord>>();

			// Knight checks
			Coord[] knightMovementPattern = new Coord[]{new Coord (-1,2), new Coord(-1,-2), new Coord(1,2), new Coord(1,-2), new Coord(2,-1), new Coord(2,-1), new Coord(-2,1), new Coord(-2,-1)};
			for (int i = 0; i < knightMovementPattern.Length; i ++) {
				Coord knightAttackCoord = new Coord(friendlyKingPosition.x + knightMovementPattern[i].x, friendlyKingPosition.y + knightMovementPattern[i].y);
				if (hostileKnights.ContainsPieceAtSquare(knightAttackCoord)) {
					checkBlockSquares.Add(knightAttackCoord);
					break;
				}
			}

			// Pawn checks
			int hostilePawnDir = (isWhite)?-1:1;
			Coord pawnAttackLeft = new Coord(friendlyKingPosition.x -1, friendlyKingPosition.y - hostilePawnDir);
			Coord pawnAttackRight = new Coord(friendlyKingPosition.x +1, friendlyKingPosition.y - hostilePawnDir);
			if (hostilePawns.ContainsPieceAtSquare(pawnAttackLeft)) {
				checkBlockSquares.Add(pawnAttackLeft);
			}
			else if (hostilePawns.ContainsPieceAtSquare(pawnAttackRight)) {
				checkBlockSquares.Add(pawnAttackRight);
			}

			// Sliding piece checks
			Coord[] lineDirections = new Coord[]{new Coord(-1,0), new Coord(1,0), new Coord(0,-1), new Coord(0,1), new Coord(1,1), new Coord(1,-1), new Coord(-1,-1), new Coord(-1,1)};
			for (int dirIndex = 0; dirIndex < lineDirections.Length; dirIndex ++) {
				BitBoard hostileLinePieceMask = (dirIndex < 4)?hostileOrthogonalPieces:hostileDiagonalPieces; // first 4 directions are orthog, next four are diag. Only check for pieces with correct movement type

				List<Coord> lineCoords = new List<Coord>();
				bool lineHasFriendly = false;
				Coord dir = lineDirections[dirIndex];

				for (int i = 1; i < 8; i ++) {
					Coord nextSquare = new Coord(friendlyKingPosition.x + dir.x * i, friendlyKingPosition.y + dir.y * i);
					lineCoords.Add(nextSquare);
					if (allFriendlyPieces.ContainsPieceAtSquare(nextSquare)) { // friendly piece found
						if (lineHasFriendly) { // two friendly pieces in a row eliminates possiblity of pin
							break;
						}
						else {
							lineHasFriendly = true;
						}

					}
					else if (allHostilePieces.ContainsPieceAtSquare(nextSquare)) { // hostile piece found (note this hostile piece is not necesarilly capable of checking king)
						if (hostileLinePieceMask.ContainsPieceAtSquare(nextSquare)) { // this piece IS capable of checking king
							if (lineHasFriendly) { // friendly piece between king and hostile piece - thus piece is pinned
								pinLines.Add(lineCoords);
							}
							else { // no friendly piece between king and hostile piece, thus piece is checking the king
								checkBlockSquares.AddRange(lineCoords);
							}
						}
						break; // if hostile piece that is incapable of checking king is in the way, then no pins/checks exist on this line
					
					}
				}
			}

			BitBoard checkBlockBoard = new BitBoard();
			for (int i = 0; i < checkBlockSquares.Count; i ++) {
				checkBlockBoard.SetSquare(checkBlockSquares[i]);
			}

			List<BitBoard> pinBoards = new List<BitBoard>();
			for (int i = 0; i < pinLines.Count; i ++) {
				BitBoard pinLineBoard = new BitBoard();
				for (int j = 0; j < pinLines[i].Count; j ++) {
					pinLineBoard.SetSquare(pinLines[i][j]);
				}
				pinBoards.Add(pinLineBoard);
			}

			checkInfo.pinBoards = pinBoards;
			checkInfo.checkBlockBoard = checkBlockBoard;
		}

		return checkInfo;
	
	}

	/// Stores information about checks and pins
	public class CheckInfo {
		public bool inCheck;
		public bool inDoubleCheck; // king is checked by two piece at once
		public BitBoard hostileControlledSquares; // squares attacked by hostile pieces
		public Coord kingSquare;

		public List<BitBoard> pinBoards; // each pin board represents the movement options of the pinned piece
		public BitBoard checkBlockBoard; // bitboard of squares which can be moved to in order to block (or capture) the checking piece

	}
	
	/// Returns a bitboard of all pseudolegal ATTACK moves for given piece at origin on a populated board.
	/// 'Attack move' means that the piece exerts control over that square.
	/// For example: a white pawn on e4 attacks d5 and f5, but does NOT attack e5.
	/// Note, however, that it 'attacks' d5 and f5 regardless of whether the square is occupied by a friendly or hostile piece (or no piece at all).
	BitBoard AttackBitBoard(Definitions.PieceName piece, Coord origin, BitBoard allPieces, bool isWhite) {
		BitBoard attackBoard = new BitBoard ();

		// Knight
		if (piece == Definitions.PieceName.Knight) {
			attackBoard.TrySetSquare (new Coord (origin.x + 1, origin.y + 2));
			attackBoard.TrySetSquare (new Coord (origin.x - 1, origin.y + 2));
			attackBoard.TrySetSquare (new Coord (origin.x + 1, origin.y - 2));
			attackBoard.TrySetSquare (new Coord (origin.x - 1, origin.y - 2));
			attackBoard.TrySetSquare (new Coord (origin.x + 2, origin.y + 1));
			attackBoard.TrySetSquare (new Coord (origin.x + 2, origin.y - 1));
			attackBoard.TrySetSquare (new Coord (origin.x - 2, origin.y + 1));
			attackBoard.TrySetSquare (new Coord (origin.x - 2, origin.y - 1));
		}
		// King
		else if (piece == Definitions.PieceName.King) { 
			attackBoard.TrySetSquare (new Coord (origin.x + 1, origin.y + 1));
			attackBoard.TrySetSquare (new Coord (origin.x + 1, origin.y - 1));
			attackBoard.TrySetSquare (new Coord (origin.x + 1, origin.y));
			attackBoard.TrySetSquare (new Coord (origin.x - 1, origin.y - 1));
			attackBoard.TrySetSquare (new Coord (origin.x - 1, origin.y + 1));
			attackBoard.TrySetSquare (new Coord (origin.x - 1, origin.y));
			attackBoard.TrySetSquare (new Coord (origin.x, origin.y + 1));
			attackBoard.TrySetSquare (new Coord (origin.x, origin.y - 1));
		}
		// Pawns
		else if (piece == Definitions.PieceName.Pawn) {
			int advanceDir = (isWhite) ? 1 : -1;
			attackBoard.TrySetSquare (new Coord (origin.x + 1, origin.y + advanceDir)); // attack diagonal right
			attackBoard.TrySetSquare (new Coord (origin.x - 1, origin.y + advanceDir)); // attack diagonal left
		}
		// Rook, Bishop, Queen
		else {
			for (int dir = -1; dir <= 1; dir += 2) {
				bool horizontalOpen = true;
				bool verticalOpen = true;
				bool leftDiagonalOpen = true;
				bool rightDiagonalOpen = true;
				
				for (int i = 1; i < 8; i ++) {
					Coord horizontal = new Coord(origin.x + i * dir, origin.y);
					Coord vertical = new Coord(origin.x, origin.y + i * dir);
					Coord leftDiagonal = new Coord(origin.x - i, origin.y + i * dir);
					Coord rightDiagonal = new Coord(origin.x + i, origin.y + i * dir);
					
					
					if (piece == Definitions.PieceName.Rook || piece == Definitions.PieceName.Queen) {
						// Horizontal
						if (horizontalOpen) {
							attackBoard.TrySetSquare(horizontal);
							if (allPieces.ContainsPieceAtSquare(horizontal)) {
								horizontalOpen = false;
							}
						}
						// Vertical
						if (verticalOpen) {
							attackBoard.TrySetSquare(vertical);
							if (allPieces.ContainsPieceAtSquare(vertical)) {
								verticalOpen = false;
							}
						}
					}
					if (piece == Definitions.PieceName.Bishop || piece == Definitions.PieceName.Queen) {
						// Left Diagonal
						if (leftDiagonalOpen) {
							attackBoard.TrySetSquare(leftDiagonal);
							if (allPieces.ContainsPieceAtSquare(leftDiagonal)) {
								leftDiagonalOpen = false;
							}
						}
						// Right Diagonal
						if (rightDiagonalOpen) {
							attackBoard.TrySetSquare(rightDiagonal);
							if (allPieces.ContainsPieceAtSquare(rightDiagonal)) {
								rightDiagonalOpen = false;
							}
						}
					}
				}
			}
		}
		return attackBoard;
	}
	
	/// Returns a bitboard of all moves possible for given piece origin, not considering checks/pins
	BitBoard PseudoLegalMovesOnPopulatedBoard(Definitions.PieceName piece, Coord origin, Position position) {

		// if white to play, white pieces are friendly; black pieces are hostile
		bool isWhite = position.gameState.whiteToMove;
		BitBoard friendlyPieces = (isWhite) ? position.allPiecesW : position.allPiecesB;
		BitBoard hostilePieces = (!isWhite) ? position.allPiecesW : position.allPiecesB;
		BitBoard allPieces = BitBoard.Combination (friendlyPieces, hostilePieces);

		// Piece movement board initially set equal to piece attack board
		BitBoard pieceMovementBoard = AttackBitBoard (piece, origin, allPieces, isWhite);

		if (piece == Definitions.PieceName.Pawn) {
			int advanceDir = (isWhite) ? 1 : -1;
			Coord advanceSquare = new Coord (origin.x, origin.y + advanceDir);

			// pawn captures:
			BitBoard pawnCaptureMask = hostilePieces;
			// add en passant square to capture mask
			if (position.gameState.enPassantFileIndex != -1) {
				int rankIndex = (isWhite)?5:2; // rank of ep capture square
				pawnCaptureMask.SetSquare(new Coord(position.gameState.enPassantFileIndex,rankIndex));
			}
			pieceMovementBoard.And(pawnCaptureMask.board); // pawn can only capture if capture square contains hostile piece

			// pawn movement:
			if (!allPieces.ContainsPieceAtSquare (advanceSquare)) { // pawn is blocked from advancing by any piece
				pieceMovementBoard.TrySetSquare (advanceSquare);
			
				if (origin.y == 1 || origin.y == 6) { // advance two squares on first move
					Coord doubleAdvanceSquare = new Coord (origin.x, origin.y + advanceDir * 2);
					if (!allPieces.ContainsPieceAtSquare (doubleAdvanceSquare)) {
						pieceMovementBoard.TrySetSquare (doubleAdvanceSquare);
					}
				}
			}
		} 
		// Rook, Bishop, Queen, King, Knight
		else {
			if (piece == Definitions.PieceName.King) {
				// king is on starting square so castling is a possibility
				if ((isWhite && position.gameState.castleKingsideW) || (!isWhite && position.gameState.castleKingsideB)) {
					if (!allPieces.ContainsPieceAtSquare(new Coord(5,origin.y))) { // can't castle kingside if piece on f1/f8
						if ((isWhite && position.rooksW.ContainsPieceAtSquare(new Coord(7,0))) || (!isWhite && position.rooksB.ContainsPieceAtSquare(new Coord(7,7)))) { // must be rook on a8/h8
							pieceMovementBoard.TrySetSquare (new Coord (origin.x + 2, origin.y));
						}
					}
				}
				if (isWhite && position.gameState.castleQueensideW || !isWhite && position.gameState.castleQueensideB) {
					if (!allPieces.ContainsPieceAtSquare(new Coord(1,origin.y)) && !allPieces.ContainsPieceAtSquare(new Coord(3,origin.y))) { // can't castle queenside if piece on b1/b8
						if ((isWhite && position.rooksW.ContainsPieceAtSquare(new Coord(0,0))) || (!isWhite && position.rooksB.ContainsPieceAtSquare(new Coord(0,7)))) { // must be rook on a1/h1
							pieceMovementBoard.TrySetSquare (new Coord (origin.x - 2, origin.y));
						}
					}
				}
			}

			pieceMovementBoard.And(~friendlyPieces.board); // piece movement is blocked by friendly pieces
		}

		return pieceMovementBoard;
	}

	
}
