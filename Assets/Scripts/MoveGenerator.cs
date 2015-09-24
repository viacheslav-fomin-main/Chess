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

		//int[,] attackBoard = GetHostileAttackBoard (position, host);

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

	BitBoard HostileAttackBoard(BitBoard allPieces, BitBoard currentPieceTypeBoard, Definitions.PieceName piece) {
		BitBoard attackBoard = new BitBoard ();
		for (int x = 0; x < 8; x ++) {
			for (int y = 0; y < 8; y ++) {

			}
		}

		return new BitBoard();
	
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
		BitBoard allPieces = BitBoard.Combine (friendlyPieces, hostilePieces);

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
				pawnCaptureMask.SetSquare(new Coord(rankIndex,position.gameState.enPassantFileIndex));
			}
			pieceMovementBoard.And(pawnCaptureMask.board); // pawn can only capture if capture square contains hostile piece

			// pawn movement:
			if (!allPieces.ContainsPieceAtSquare (advanceSquare)) { // pawn is blocked from advancing by any piece
				pieceMovementBoard.TrySetSquare (advanceSquare);
			
				if (origin.y == 1 || origin.y == 6) { // advance two squares on first move
					Coord doubleAdvanceSquare = new Coord (origin.x, origin.y + advanceDir * 2);
					if (!allPieces.ContainsPieceAtSquare (advanceSquare)) {
						pieceMovementBoard.TrySetSquare (doubleAdvanceSquare);
					}
				}
			}
		} 
		// Rook, Bishop, Queen, King, Knight
		else {
			if (piece == Definitions.PieceName.King) {
				// king is on starting square so castling is a possibility
				// Note: no consideration is given to colour, so white king will think he is able to castle when on e8.
				// This will obviously be picked up during legal move checking.
				if (origin.x == 4 && (origin.y == 0 || origin.y == 7)) {
					pieceMovementBoard.TrySetSquare (new Coord (origin.x + 2, origin.y));
					pieceMovementBoard.TrySetSquare (new Coord (origin.x - 2, origin.y));
				}
			}

			pieceMovementBoard.And(~friendlyPieces.board); // piece movement is blocked by friendly pieces
		}

		return pieceMovementBoard;
	}

	
}
