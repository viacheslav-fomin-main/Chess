using System.Collections.Generic;
using System.Collections;

public class MoveGenerator {
	
	public Move[] GetAllLegalMoves(Position position) {
		List<Move> movesFound = new List<Move> ();

		bool isWhite = position.gameState.whiteToMove;
		BitBoard friendlyPieces = (isWhite) ? position.allPiecesW : position.allPiecesB;
		BitBoard hostilePieces = (!isWhite) ? position.allPiecesW : position.allPiecesB;

		List<BitBoard> bitboards = new List<BitBoard> ();
		List<Coord> origins = new List<Coord> ();

		//PseudoLegalMovesOnPopulatedBoard (Definitions.PieceName.Knight, new Coord (1, 7), position).PrintBoardToConsole ("Knight b8");

		for (int i =0; i < 64; i ++) {
			if (friendlyPieces.ContainsPieceAtSquare(i)) {
				Coord square = new Coord(i);
				BitBoard pseudoLegalMoveBoard = new BitBoard();

				if (position.pawnsB.ContainsPieceAtSquare(i) || position.pawnsW.ContainsPieceAtSquare(i)) {
					pseudoLegalMoveBoard = PseudoLegalMovesOnPopulatedBoard(Definitions.PieceName.Pawn, square, position);
				}
				else if (position.rooksB.ContainsPieceAtSquare(i) || position.rooksW.ContainsPieceAtSquare(i)) {
					pseudoLegalMoveBoard = PseudoLegalMovesOnPopulatedBoard(Definitions.PieceName.Rook, square, position);
				}
				else if (position.knightsB.ContainsPieceAtSquare(i) || position.knightsW.ContainsPieceAtSquare(i)) {
					pseudoLegalMoveBoard = PseudoLegalMovesOnPopulatedBoard(Definitions.PieceName.Knight, square, position);
				}
				else if (position.bishopsB.ContainsPieceAtSquare(i) || position.bishopsW.ContainsPieceAtSquare(i)) {
					pseudoLegalMoveBoard = PseudoLegalMovesOnPopulatedBoard(Definitions.PieceName.Bishop, square, position);
				}
				else if (position.queensB.ContainsPieceAtSquare(i) || position.queensW.ContainsPieceAtSquare(i)) {
					pseudoLegalMoveBoard = PseudoLegalMovesOnPopulatedBoard(Definitions.PieceName.Queen, square, position);
				}
				else if (position.kingB.ContainsPieceAtSquare(i) || position.kingW.ContainsPieceAtSquare(i)) {
					pseudoLegalMoveBoard = PseudoLegalMovesOnPopulatedBoard(Definitions.PieceName.King, square, position);
				}

				bitboards.Add(pseudoLegalMoveBoard);
				origins.Add(square);
			}
		}

		// convert bitboards to moves
		for (int i =0; i < bitboards.Count; i ++) {
			for (int j =0; j < 64; j ++) {
				if (bitboards[i].ContainsPieceAtSquare(j)) {
					GameState newGameState = position.gameState;
					newGameState.whiteToMove = !newGameState.whiteToMove;
					movesFound.Add(new Move(origins[i],new Coord(j), newGameState));
				}
			}
		}


		return movesFound.ToArray();
	}


	
	/// Returns a bitboard of all moves possible for given piece in given position, not considering checks
	BitBoard PseudoLegalMovesOnPopulatedBoard(Definitions.PieceName piece, Coord square, Position position) {
		BitBoard pieceMovementBoard = new BitBoard ();

		// if white to play, white pieces are friendly; black pieces are hostile
		bool isWhite = position.gameState.whiteToMove;
		BitBoard friendlyPieces = (isWhite) ? position.allPiecesW : position.allPiecesB;
		BitBoard hostilePieces = (!isWhite) ? position.allPiecesW : position.allPiecesB;
		BitBoard allPieces = BitBoard.Combine (friendlyPieces, hostilePieces);

		if (piece == Definitions.PieceName.Knight || piece == Definitions.PieceName.King) {
			if (piece == Definitions.PieceName.Knight) {
				pieceMovementBoard.TrySetSquare (new Coord (square.x + 1, square.y + 2));
				pieceMovementBoard.TrySetSquare (new Coord (square.x - 1, square.y + 2));
				pieceMovementBoard.TrySetSquare (new Coord (square.x + 1, square.y - 2));
				pieceMovementBoard.TrySetSquare (new Coord (square.x - 1, square.y - 2));
				pieceMovementBoard.TrySetSquare (new Coord (square.x + 2, square.y + 1));
				pieceMovementBoard.TrySetSquare (new Coord (square.x + 2, square.y - 1));
				pieceMovementBoard.TrySetSquare (new Coord (square.x - 2, square.y + 1));
				pieceMovementBoard.TrySetSquare (new Coord (square.x - 2, square.y - 1));
			} else if (piece == Definitions.PieceName.King) {
				pieceMovementBoard.TrySetSquare (new Coord (square.x + 1, square.y + 1));
				pieceMovementBoard.TrySetSquare (new Coord (square.x + 1, square.y - 1));
				pieceMovementBoard.TrySetSquare (new Coord (square.x + 1, square.y));
				pieceMovementBoard.TrySetSquare (new Coord (square.x - 1, square.y - 1));
				pieceMovementBoard.TrySetSquare (new Coord (square.x - 1, square.y + 1));
				pieceMovementBoard.TrySetSquare (new Coord (square.x - 1, square.y));
				pieceMovementBoard.TrySetSquare (new Coord (square.x, square.y + 1));
				pieceMovementBoard.TrySetSquare (new Coord (square.x, square.y - 1));

				// king is on starting square so castling is a possibility
				// Note: no consideration is given to colour, so white king will think he is able to castle when on e8.
				// This will obviously be picked up during legal move checking.
				if (square.x == 4 && (square.y == 0 || square.y == 7)) {
					pieceMovementBoard.TrySetSquare (new Coord (square.x + 2, square.y));
					pieceMovementBoard.TrySetSquare (new Coord (square.x - 2, square.y));
				}
			}
			pieceMovementBoard.And (~friendlyPieces.board); // Both knight and king can only be blocked by friendly pieces
		} else if (piece == Definitions.PieceName.Pawn) {
			int advanceDir = (isWhite) ? 1 : -1;
			Coord advanceSquare = new Coord (square.x, square.y + advanceDir);

			if (!allPieces.ContainsPieceAtSquare (advanceSquare)) { // pawn is blocked from advancing by any piece
				pieceMovementBoard.TrySetSquare (advanceSquare);
			
				if (square.y == 1 || square.y == 6) { // advance two squares on first move
					Coord doubleAdvanceSquare = new Coord (square.x, square.y + advanceDir * 2);
					if (!allPieces.ContainsPieceAtSquare (advanceSquare)) {
						pieceMovementBoard.TrySetSquare (doubleAdvanceSquare);
					}
				}
			}
			// pawn is blocked from capturing by friendly piece or no piece (unless en passant)
			Coord captureLeft = new Coord (square.x + 1, square.y + advanceDir);
			Coord captureRight = new Coord (square.x - 1, square.y + advanceDir);
			bool onEnPassantRank = (square.y == 3 || square.y == 4);

			if (hostilePieces.ContainsPieceAtSquare (captureLeft) || (onEnPassantRank && captureLeft.x == position.gameState.enPassantFileIndex)) {
				pieceMovementBoard.TrySetSquare (captureLeft);
			}
			if (hostilePieces.ContainsPieceAtSquare (captureRight) || (onEnPassantRank && captureRight.x == position.gameState.enPassantFileIndex)) {
				pieceMovementBoard.TrySetSquare (captureRight);
			}
		} else {
			for (int dir = -1; dir <= 1; dir += 2) {
				bool horizontalOpen = true;
				bool verticalOpen = true;
				bool leftDiagonalOpen = true;
				bool rightDiagonalOpen = true;
				
				for (int i = 1; i < 8; i ++) {
					Coord horizontal = new Coord(square.x + i * dir, square.y);
					Coord vertical = new Coord(square.x, square.y + i * dir);
					Coord leftDiagonal = new Coord(square.x - i, square.y + i * dir);
					Coord rightDiagonal = new Coord(square.x + i, square.y + i * dir);


					if (piece == Definitions.PieceName.Rook || piece == Definitions.PieceName.Queen) {
						// Horizontal
						if (horizontalOpen) {
							if (friendlyPieces.ContainsPieceAtSquare(horizontal)) { // friendly piece blocks square, movement ends immediately
								horizontalOpen = false;
							} else {
								pieceMovementBoard.TrySetSquare(horizontal);
								if (hostilePieces.ContainsPieceAtSquare(horizontal)) { // hostile piece blocks square, movement ends after capture
									horizontalOpen = false;
								}
							}
						}
						// Vertical
						if (verticalOpen) {
							if (friendlyPieces.ContainsPieceAtSquare(vertical)) { // friendly piece blocks square, movement ends immediately
								verticalOpen = false;
							} else {
								pieceMovementBoard.TrySetSquare(vertical);
								if (hostilePieces.ContainsPieceAtSquare(vertical)) { // hostile piece blocks square, movement ends after capture
									verticalOpen = false;
								}
							}
						}
					}
					if (piece == Definitions.PieceName.Bishop || piece == Definitions.PieceName.Queen) {
						// Left Diagonal
						if (leftDiagonalOpen) {
							if (friendlyPieces.ContainsPieceAtSquare(leftDiagonal)) { // friendly piece blocks square, movement ends immediately
								leftDiagonalOpen = false;
							} else {
								pieceMovementBoard.TrySetSquare(leftDiagonal);
								if (hostilePieces.ContainsPieceAtSquare(leftDiagonal)) { // hostile piece blocks square, movement ends after capture
									leftDiagonalOpen = false;
								}
							}
						}
						// Right Diagonal
						if (rightDiagonalOpen) {
							if (friendlyPieces.ContainsPieceAtSquare(rightDiagonal)) { // friendly piece blocks square, movement ends immediately
								rightDiagonalOpen = false;
							} else {
								pieceMovementBoard.TrySetSquare(rightDiagonal);
								if (hostilePieces.ContainsPieceAtSquare(rightDiagonal)) { // hostile piece blocks square, movement ends after capture
									rightDiagonalOpen = false;
								}
							}
						}
					}
				}
			}
		}

		return pieceMovementBoard;
	}

	
}
