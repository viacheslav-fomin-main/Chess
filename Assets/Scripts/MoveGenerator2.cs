using System.Collections.Generic;
using System.Collections;
using System;
using System.Diagnostics;

public class MoveGenerator2 : IMoveGenerator {

	static Stopwatch timeFull = new Stopwatch();
	static Stopwatch timePseudolegalMoves = new Stopwatch();
	static Stopwatch timeAttackMoves = new Stopwatch();
	static Stopwatch timeCheckInfo = new Stopwatch();
	static Stopwatch timePinDiscoverDetection = new Stopwatch();

	Coord[] knightMoves = new Coord[]{
		new Coord (1, 2),
		new Coord (-1, 2),
		new Coord (1, -2),
		new Coord (-1, -2),
		new Coord (2, 1),
		new Coord (2, -1),
		new Coord (-2, 1),
		new Coord (-2, -1)
	};

	Coord[] kingMoves = new Coord[]{
		new Coord (1, 1),
		new Coord ( 1, -1),
		new Coord (1, 0),
		new Coord (-1, 1),
		new Coord (-1, -1),
		new Coord (-1, 0),
		new Coord (0, 1),
		new Coord (0, -1)
	};

	CheckInfo checkInfo;
	bool isWhite;

	BitBoard friendlyPieces;
	BitBoard hostilePieces;
	BitBoard allPieces;
	
	public Move[] GetAllLegalMoves(Position position) {
		timeFull.Start ();

		List<Move> legalMoves = new List<Move> ();
		isWhite = position.gameState.whiteToMove;
		friendlyPieces = (isWhite) ? position.allPiecesW : position.allPiecesB;
		hostilePieces = (!isWhite) ? position.allPiecesW : position.allPiecesB;
		allPieces = BitBoard.Combination (friendlyPieces, hostilePieces);

		checkInfo = GetCheckInfo (position);

		//Print ("Check: " + checkInfo.inCheck + "  Double check: " + checkInfo.inDoubleCheck);

		legalMoves = PseudoLegalMovesOnPopulatedBoard (Definitions.PieceName.King, checkInfo.kingSquare, position);

		if (!checkInfo.inDoubleCheck) { // no pieces besides king can move when in double check
			List<Move> allPsuedolegalMoves = new List<Move> ();
			for (int squareIndex =0; squareIndex < 64; squareIndex ++) {
				if (friendlyPieces.ContainsPieceAtSquare (squareIndex)) {
					Coord square = new Coord (squareIndex);

					if (position.pawnsB.ContainsPieceAtSquare (squareIndex) || position.pawnsW.ContainsPieceAtSquare (squareIndex)) {
						allPsuedolegalMoves.AddRange(PseudoLegalMovesOnPopulatedBoard (Definitions.PieceName.Pawn, square, position));
					} else if (position.rooksB.ContainsPieceAtSquare (squareIndex) || position.rooksW.ContainsPieceAtSquare (squareIndex)) {
						allPsuedolegalMoves.AddRange(PseudoLegalMovesOnPopulatedBoard (Definitions.PieceName.Rook, square, position));
					} else if (position.knightsB.ContainsPieceAtSquare (squareIndex) || position.knightsW.ContainsPieceAtSquare (squareIndex)) {
						allPsuedolegalMoves.AddRange(PseudoLegalMovesOnPopulatedBoard (Definitions.PieceName.Knight, square, position));
					} else if (position.bishopsB.ContainsPieceAtSquare (squareIndex) || position.bishopsW.ContainsPieceAtSquare (squareIndex)) {
						allPsuedolegalMoves.AddRange(PseudoLegalMovesOnPopulatedBoard (Definitions.PieceName.Bishop, square, position));
					} else if (position.queensB.ContainsPieceAtSquare (squareIndex) || position.queensW.ContainsPieceAtSquare (squareIndex)) {
						allPsuedolegalMoves.AddRange(PseudoLegalMovesOnPopulatedBoard (Definitions.PieceName.Queen, square, position));
					}
				}
			}

			timePinDiscoverDetection.Start();
			// calculate pins/checks
			for (int i =0; i < allPsuedolegalMoves.Count; i ++) {
				Position positionAfterMove = position;
				positionAfterMove.MakeMove(allPsuedolegalMoves[i]);
				//legalMoves.Add(allPsuedolegalMoves[i]);
				if (!KingInCheckAfterMove(checkInfo.kingSquare, positionAfterMove)) {
					legalMoves.Add(allPsuedolegalMoves[i]);
				}
				else {
					//Print ("Would be in check: " + allPsuedolegalMoves[i].algebraicMove);
				}
			}
			timePinDiscoverDetection.Stop();
		}

		timeFull.Stop ();
		timeAttackMoves.Stop ();
		timePinDiscoverDetection.Stop ();
		timePseudolegalMoves.Stop ();
		timeCheckInfo.Stop ();

		return legalMoves.ToArray();
	}

	public void PrintTimes() {
		UnityEngine.Debug.Log ("hostile attack move generation : " + timeAttackMoves.ElapsedMilliseconds + " ms");
		UnityEngine.Debug.Log ("Pin/Discovery detection : " + timePinDiscoverDetection.ElapsedMilliseconds + " ms");
		UnityEngine.Debug.Log ("Psuedolegal move gen : " + timePseudolegalMoves.ElapsedMilliseconds + " ms");
		UnityEngine.Debug.Log ("Check info gen : " + timeCheckInfo.ElapsedMilliseconds + " ms");
		UnityEngine.Debug.Log ("Total : " + timeFull.ElapsedMilliseconds + " ms");
	}
	
	/// Returns whether or not king is in check given position
	/// This should only be used to remove illegal moves from move list
	/// Does not consider pawn/knight checks as these can't occur as a result of illegal move (pawns can't pin/deliver discovered check) !!!!! TODO: currently ignoring this. Fix!!
	bool KingInCheckAfterMove (Coord kingSquare, Position positionAfterMove) {

		BitBoard hostileKnights = positionAfterMove.Knights (!isWhite);
		BitBoard hostileOrthogonal = BitBoard.Combination (positionAfterMove.Rooks (!isWhite), positionAfterMove.Queens (!isWhite));
		BitBoard hostileDiagonal = BitBoard.Combination (positionAfterMove.Bishops (!isWhite), positionAfterMove.Queens (!isWhite));

		BitBoard orthogonalBlockingPieces = BitBoard.Combination (positionAfterMove.AllPieces(isWhite), positionAfterMove.King (!isWhite), positionAfterMove.Pawns (!isWhite), hostileKnights, positionAfterMove.Bishops(!isWhite)); // all pieces (friendly and hostile) that can block vertical/horizontal checks
		BitBoard diagonalBlockingPieces = BitBoard.Combination (positionAfterMove.AllPieces(isWhite), positionAfterMove.King (!isWhite), positionAfterMove.Pawns (!isWhite), hostileKnights, positionAfterMove.Rooks(!isWhite)); // all pieces (friendly and hostile) that can block diagonal checks

		for (int i =0; i < knightMoves.Length; i ++) {
			if (hostileKnights.SafeContainsPieceAtSquare(kingSquare + knightMoves[i])) {
				return true;
			}
		}

		int pawnAdvanceDir = (isWhite) ? 1 : -1;
		Coord pawnAttackLeft = new Coord (kingSquare.x - 1, kingSquare.y + pawnAdvanceDir);
		Coord pawnAttackRight = new Coord (kingSquare.x + 1, kingSquare.y + pawnAdvanceDir);

		if (positionAfterMove.Pawns (!isWhite).SafeContainsPieceAtSquare (pawnAttackLeft) || positionAfterMove.Pawns (!isWhite).SafeContainsPieceAtSquare (pawnAttackRight)) {
			return true;
		}

		for (int dir = -1; dir <= 1; dir += 2) {
			bool horizontalOpen = true;
			bool verticalOpen = true;
			bool leftDiagonalOpen = true;
			bool rightDiagonalOpen = true;
			
			for (int i = 1; i < 8; i ++) {
				Coord horizontal = new Coord (kingSquare.x + i * dir, kingSquare.y);
				Coord vertical = new Coord (kingSquare.x, kingSquare.y + i * dir);
				Coord leftDiagonal = new Coord (kingSquare.x - i, kingSquare.y + i * dir);
				Coord rightDiagonal = new Coord (kingSquare.x + i, kingSquare.y + i * dir);

				// Horizontal
				if (horizontalOpen) {
					if (hostileOrthogonal.SafeContainsPieceAtSquare(horizontal)) {
						return true;
					}
					if (orthogonalBlockingPieces.SafeContainsPieceAtSquare (horizontal)) {
						horizontalOpen = false;
					}
				}
				// Vertical
				if (verticalOpen) {
					if (hostileOrthogonal.SafeContainsPieceAtSquare(vertical)) {
						return true;
					}
					if (orthogonalBlockingPieces.SafeContainsPieceAtSquare (vertical)) {
						verticalOpen = false;
					}
				}

				// Left Diagonal
				if (leftDiagonalOpen) {
					if (hostileDiagonal.SafeContainsPieceAtSquare(leftDiagonal)) {
						return true;
					}
					if (diagonalBlockingPieces.SafeContainsPieceAtSquare (leftDiagonal)) {
						leftDiagonalOpen = false;
					}
				}
				// Right Diagonal
				if (rightDiagonalOpen) {
					if (hostileDiagonal.SafeContainsPieceAtSquare(rightDiagonal)) {
						return true;
					}
					if (diagonalBlockingPieces.SafeContainsPieceAtSquare (rightDiagonal)) {
						rightDiagonalOpen = false;
					}
				}

			}
		}
		return false;
	}

	/// Returns a bitboard of all pseudolegal ATTACK moves for given piece at origin on a populated board.
	/// 'Attack move' means that the piece exerts control over that square.
	/// For example: a white pawn on e4 attacks d5 and f5, but does NOT attack e5.
	/// Note, however, that it 'attacks' d5 and f5 regardless of whether the square is occupied by a friendly or hostile piece (or no piece at all).
	BitBoard HostileAttackBoard(Definitions.PieceName piece, Coord origin, Coord friendlyKingPosition) {
		timeAttackMoves.Start ();
		BitBoard attackBoard = new BitBoard ();

		BitBoard hostileControlledSquares = new BitBoard ();
		BitBoard allPiecesSansFriendlyKing = allPieces;
		allPiecesSansFriendlyKing.SetSquare(friendlyKingPosition,false); // remove the friendly king from board so that hostile attacks are not blocked by it (king cannot be used to block a check)


		// Knight
		if (piece == Definitions.PieceName.Knight) {
			for (int i =0; i < knightMoves.Length; i ++) {
				attackBoard.SafeSetSquare(knightMoves[i] + origin);
			}
		}
		// King
		else if (piece == Definitions.PieceName.King) { 
			for (int i =0; i < knightMoves.Length; i ++) {
				attackBoard.SafeSetSquare(kingMoves[i] + origin);
			}
		}
		// Pawns
		else if (piece == Definitions.PieceName.Pawn) {
			int advanceDir = (!isWhite) ? 1 : -1; // move in opposite direction to colour since this is for opponent
			attackBoard.SafeSetSquare (new Coord (origin.x + 1, origin.y + advanceDir)); // attack diagonal right
			attackBoard.SafeSetSquare (new Coord (origin.x - 1, origin.y + advanceDir)); // attack diagonal left
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
							attackBoard.SafeSetSquare(horizontal);
							if (allPiecesSansFriendlyKing.ContainsPieceAtSquare(horizontal)) {
								horizontalOpen = false;
							}

						}
						// Vertical
						if (verticalOpen) {
							attackBoard.SafeSetSquare(vertical);
							if (allPiecesSansFriendlyKing.ContainsPieceAtSquare(vertical)) {
								verticalOpen = false;
							}
						}
					}
					if (piece == Definitions.PieceName.Bishop || piece == Definitions.PieceName.Queen) {
						// Left Diagonal
						if (leftDiagonalOpen) {
							attackBoard.SafeSetSquare(leftDiagonal);
							if (allPiecesSansFriendlyKing.ContainsPieceAtSquare(leftDiagonal)) {
								leftDiagonalOpen = false;
							}
						}
						// Right Diagonal
						if (rightDiagonalOpen) {
							attackBoard.SafeSetSquare(rightDiagonal);
							if (allPiecesSansFriendlyKing.ContainsPieceAtSquare(rightDiagonal)) {
								rightDiagonalOpen = false;
							}
						}
					}
				}
			}
		}
		timeAttackMoves.Stop ();
		return attackBoard;
	}
	
	/// Returns a bitboard of all moves possible for given piece origin, not considering checks/pins
	List<Move> PseudoLegalMovesOnPopulatedBoard(Definitions.PieceName piece, Coord origin, Position position) {
		timePseudolegalMoves.Start ();

		GameState nextGameState = position.gameState;
		nextGameState.whiteToMove = !isWhite; // toggle move

		List<Move> moves = new List<Move> ();


		// King movement
		if (piece == Definitions.PieceName.King) {
			BitBoard kingObstructionBoard = BitBoard.Combination(friendlyPieces, checkInfo.hostileControlledSquares); // obstruction board containing friendly pieces and squares controlled by the enemy
			for (int i =0; i < kingMoves.Length; i ++) {
				Coord moveSquare = kingMoves[i] + origin;

				if (moveSquare.inBoard) {
					if (!kingObstructionBoard.ContainsPieceAtSquare(moveSquare)) {
						GameState newGameState = nextGameState;
						// Remove castling rights if king moves
						if (isWhite) {
							newGameState.castleKingsideW = false;
							newGameState.castleQueensideW = false;
						}
						else {
							newGameState.castleKingsideB = false;
							newGameState.castleQueensideB = false;
						}
						moves.Add(CreateMove(origin,kingMoves[i] + origin, newGameState));
					}

				}

				// castling
				if ((isWhite && (nextGameState.castleKingsideW || nextGameState.castleQueensideW)) || (!isWhite && (nextGameState.castleKingsideB || nextGameState.castleQueensideB))) { // king still has the right to castle on at least one side
					BitBoard castlingObstructionBoard = BitBoard.Combination(kingObstructionBoard, hostilePieces); // add hostile pieces to obstruction board since the king cannot castle through them
					GameState castleGameState = nextGameState;
					string kingRankName = "";
					if (isWhite) { // castling white
						kingRankName = "1";
						castleGameState.castleKingsideW = false;
						castleGameState.castleQueensideW = false;
					}
					else {
						kingRankName = "8";
						castleGameState.castleKingsideB = false;
						castleGameState.castleQueensideB = false;
					}

					if ((isWhite && nextGameState.castleKingsideW) || (!isWhite && nextGameState.castleKingsideB)) { // king still has right to castle kingside
						if (!castlingObstructionBoard.ContainsPieceAtSquare(new Coord("f" + kingRankName)) && !castlingObstructionBoard.ContainsPieceAtSquare(new Coord("g" + kingRankName))) { // no obstructions/checks on f and g files
							Move castleMove = CreateMove(origin, new Coord("g" + kingRankName), castleGameState);
							castleMove.isCastles = true;
							castleMove.rookFrom = new Coord("h" + kingRankName);
							castleMove.rookTo = new Coord("f" + kingRankName);
							moves.Add(castleMove);
						}
					}
				
					if ((isWhite && nextGameState.castleQueensideW) || (!isWhite && nextGameState.castleQueensideB)) { // king still has right to castle queenside
						if (!castlingObstructionBoard.ContainsPieceAtSquare(new Coord("d" + kingRankName)) && !castlingObstructionBoard.ContainsPieceAtSquare(new Coord("c" + kingRankName))) { // no obstructions/checks on d and c files
							if (!kingObstructionBoard.ContainsPieceAtSquare(new Coord("b" + kingRankName))) { // no piece on b file
								Move castleMove = CreateMove(origin, new Coord("c" + kingRankName), castleGameState);
								castleMove.isCastles = true;
								castleMove.rookFrom = new Coord("a" + kingRankName);
								castleMove.rookTo = new Coord("d" + kingRankName);
								moves.Add(castleMove);
							}
						}
					}
				}

			}

		}
		// Knights
		else if (piece == Definitions.PieceName.Knight) {
			for (int i =0; i < knightMoves.Length; i ++) {
				Coord knightMove = knightMoves[i] + origin;
				if (knightMove.inBoard) {
					if (!friendlyPieces.ContainsPieceAtSquare(knightMove)) {
						moves.Add(CreateMove(origin,knightMoves[i] + origin, nextGameState));
					}
				}
			}
		} 
		// Pawns
		else if (piece == Definitions.PieceName.Pawn) {
	
			int advanceDir = (isWhite) ? 1 : -1;
			Coord advanceSquare = new Coord (origin.x, origin.y + advanceDir);
			bool promotion = false;

			// determine promotion
			if (advanceSquare.y == 7 || advanceSquare.y == 0) { // has reached first/last rank
				promotion = true;
			}

			// pawn captures:
			BitBoard pawnCaptureMask = hostilePieces;
			Coord epCaptureSquare = new Coord(position.gameState.enPassantFileIndex,(isWhite)?5:2); // the square the pawn will capture to
			Coord epPawnSquare = new Coord(position.gameState.enPassantFileIndex,(isWhite)?4:3); // the square which the pawn being captured en passant actually occupies
			bool isEpCapture;

			Coord[] pawnCaptureSquares = new Coord[]{ // pawn captures on left and right diagonals
				new Coord (origin.x + 1, origin.y + advanceDir),
				new Coord (origin.x - 1, origin.y + advanceDir)
			};

			for (int i = 0; i < pawnCaptureSquares.Length; i ++) {
				isEpCapture = pawnCaptureSquares[i] == epCaptureSquare;
				if (pawnCaptureSquares[i].inBoard) {
					if (pawnCaptureMask.ContainsPieceAtSquare(pawnCaptureSquares[i]) || isEpCapture) { // can only capture if there is enemy piece at square (or is ep square)
						Move pawnCaptureMove = CreateMove(origin, pawnCaptureSquares[i], nextGameState);
						if (isEpCapture) {
							pawnCaptureMove.isEnPassantCapture = true;
							pawnCaptureMove.enPassantPawnLocation = epPawnSquare;
						}
						pawnCaptureMove.isPawnPromotion = promotion;
						moves.Add(pawnCaptureMove);
					}
				}
			}

			// pawn movement
			if (!allPieces.ContainsPieceAtSquare (advanceSquare)) { // pawn is blocked from advancing by any piece
				Move pawnMove = CreateMove(origin,advanceSquare, nextGameState);
				pawnMove.isPawnPromotion = promotion;
				moves.Add(pawnMove);

				// advance two squares on first move
				if ((isWhite && origin.y == 1) || (!isWhite && origin.y == 6)) { 
					Coord doubleAdvanceSquare = new Coord (origin.x, origin.y + advanceDir * 2);
					if (!allPieces.ContainsPieceAtSquare (doubleAdvanceSquare)) {
						GameState newGameState = nextGameState;
						newGameState.enPassantFileIndex = origin.x;

						moves.Add(CreateMove(origin, doubleAdvanceSquare, newGameState));
					}
				}
			}

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

							if (allPieces.ContainsPieceAtSquare(horizontal)) {
								horizontalOpen = false;
							}
							if (horizontal.inBoard) {
								if (!friendlyPieces.ContainsPieceAtSquare(horizontal)) {
									moves.Add(CreateMove(origin,horizontal, nextGameState));
								}
							}
						}
						// Vertical
						if (verticalOpen) {

							if (allPieces.ContainsPieceAtSquare(vertical)) {
								verticalOpen = false;
							}
							if (vertical.inBoard) {
								if (!friendlyPieces.ContainsPieceAtSquare(vertical)) {
									moves.Add(CreateMove(origin,vertical, nextGameState));
								}
							}
						}
					}
					if (piece == Definitions.PieceName.Bishop || piece == Definitions.PieceName.Queen) {
						// Left Diagonal
						if (leftDiagonalOpen) {

							if (allPieces.ContainsPieceAtSquare(leftDiagonal)) {
								leftDiagonalOpen = false;
							}
							if (leftDiagonal.inBoard) {
								if (!friendlyPieces.ContainsPieceAtSquare(leftDiagonal)) {
									moves.Add(CreateMove(origin,leftDiagonal, nextGameState));
								}
							}
						}
						// Right Diagonal
						if (rightDiagonalOpen) {

							if (allPieces.ContainsPieceAtSquare(rightDiagonal)) {
								rightDiagonalOpen = false;
							}
							if (rightDiagonal.inBoard) {
								if (!friendlyPieces.ContainsPieceAtSquare(rightDiagonal)) {
									moves.Add(CreateMove(origin,rightDiagonal, nextGameState));
								}
							}
						}
					}
				}
			}
		}

		timePseudolegalMoves.Stop ();
		return moves;
	}

	/// Creates a move with given information
	/// Also checks if any rooks have been moved/captured and updates castling rights accordingly
	/// Also sets move colour
	Move CreateMove(Coord from, Coord to, GameState gameState) {

		// remove castling rights if piece moves to/from rook square
		if (gameState.castleQueensideW) {
			if ((to.x == 0 && to.y == 0) || (from.x == 0 && from.y == 0)) { // a1
				gameState.castleQueensideW = false;
			}
		}
		if (gameState.castleKingsideW) {
			if ((to.x == 7 && to.y == 0) || (from.x == 7 && from.y == 0)) { // h1
				gameState.castleKingsideW = false;
			}
		}

		if (gameState.castleQueensideB) {
			if ((to.x == 0 && to.y == 7) || (from.x == 0 && from.y == 7)) { // a8
				gameState.castleQueensideB = false;
			}
		}
		if (gameState.castleKingsideB) {
			if ((to.x == 7 && to.y == 7) || (from.x == 7 && from.y == 7)) { // h8
				gameState.castleKingsideB = false;
			}
		}

		// set move colour
		Move move = new Move (from, to, gameState);
		move.isWhiteMove = isWhite;

		return move;
	}

	CheckInfo GetCheckInfo(Position position) {
		timeCheckInfo.Start ();
		CheckInfo checkInfo = new CheckInfo ();

		int friendlyKingSquareIndex = BitBoard.BitIndex (((isWhite) ? position.kingW : position.kingB).board);
		Coord friendlyKingPosition = new Coord (friendlyKingSquareIndex);
		
		BitBoard hostileControlledSquares = new BitBoard ();

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
		
		int checkCount = 0; // number of checks delivered to friendly king
		
		// Get attack boards for each hostile piece and combine to form bitboard of all hostile-controlled squares
		for (int pieceBoardIndex = 0; pieceBoardIndex < hostilePieceBoards.Length; pieceBoardIndex ++) {
			for (int squareIndex = 0; squareIndex <= 63; squareIndex ++) {
				bool isPieceOnBoard = hostilePieceBoards[pieceBoardIndex].ContainsPieceAtSquare(squareIndex);
				
				if (isPieceOnBoard) {
					
					BitBoard pieceAttackBoard = HostileAttackBoard(pieceBoardOrder[pieceBoardIndex], new Coord(squareIndex), friendlyKingPosition);
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
		timeCheckInfo.Stop ();
		return checkInfo;
	}

	/// Stores information about checks and pins
	public class CheckInfo {
		public bool inCheck;
		public bool inDoubleCheck; // king is checked by two piece at once
		public BitBoard hostileControlledSquares; // squares attacked by hostile pieces
		public Coord kingSquare;
	}

	public void Print(object m) {
		UnityEngine.Debug.Log (m);
	}
	
}
