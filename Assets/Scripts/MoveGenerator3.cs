using System.Collections.Generic;
using System.Collections;
using System;
using System.Diagnostics;
using UnityEngine;

public class MoveGenerator3 : IMoveGenerator {

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

	Coord[] lineDirections = new Coord[]{ // first four are orthogonal, last four are diagonal
		new Coord (0, 1),
		new Coord (0, -1),
		new Coord (1, 0),
		new Coord (-1, 0),
		new Coord (1, 1),
		new Coord (1, -1),
		new Coord (-1, 1),
		new Coord (-1, -1)
	};


	CheckInfo checkInfo;
	bool isWhite;

	BitBoard friendlyPieces;
	BitBoard hostilePieces;
	BitBoard allPieces;

	List<Move> legalMoves;
	
	public Move[] GetAllLegalMoves(Position position) {
		timeFull.Start ();

		legalMoves = new List<Move> ();
		isWhite = position.gameState.whiteToMove;
		friendlyPieces = (isWhite) ? position.allPiecesW : position.allPiecesB;
		hostilePieces = (!isWhite) ? position.allPiecesW : position.allPiecesB;
		allPieces = BitBoard.Combination (friendlyPieces, hostilePieces);

		checkInfo = GetCheckInfo (position);

		//Print ("Check: " + checkInfo.inCheck + "  Double check: " + checkInfo.inDoubleCheck);

		GenerateLegalMoves (Definitions.PieceName.King, checkInfo.kingSquare, position);
		if (checkInfo.inCheck) {
		//	checkInfo.checkBlockBoard.PrintBoardToConsole("Check block bpard");
		}
		if (!checkInfo.inDoubleCheck) { // no pieces besides king can move when in double check
			//List<Move> allPsuedolegalMoves = new List<Move> ();
			for (int squareIndex =0; squareIndex < 64; squareIndex ++) {
				if (friendlyPieces.ContainsPieceAtSquare (squareIndex)) {
					Coord square = new Coord (squareIndex);

					if (position.pawnsB.ContainsPieceAtSquare (squareIndex) || position.pawnsW.ContainsPieceAtSquare (squareIndex)) {
						GenerateLegalMoves (Definitions.PieceName.Pawn, square, position);
					} else if (position.rooksB.ContainsPieceAtSquare (squareIndex) || position.rooksW.ContainsPieceAtSquare (squareIndex)) {
						GenerateLegalMoves (Definitions.PieceName.Rook, square, position);
					} else if (position.knightsB.ContainsPieceAtSquare (squareIndex) || position.knightsW.ContainsPieceAtSquare (squareIndex)) {
						GenerateLegalMoves (Definitions.PieceName.Knight, square, position);
					} else if (position.bishopsB.ContainsPieceAtSquare (squareIndex) || position.bishopsW.ContainsPieceAtSquare (squareIndex)) {
						GenerateLegalMoves (Definitions.PieceName.Bishop, square, position);
					} else if (position.queensB.ContainsPieceAtSquare (squareIndex) || position.queensW.ContainsPieceAtSquare (squareIndex)) {
						GenerateLegalMoves (Definitions.PieceName.Queen, square, position);
					}
				}
			}
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



	
	/// Returns a bitboard of all moves possible for given piece origin, not considering checks/pins
	void GenerateLegalMoves(Definitions.PieceName piece, Coord origin, Position position) {
		timePseudolegalMoves.Start ();

		GameState nextGameState = position.gameState;
		nextGameState.whiteToMove = !isWhite; // toggle move

		// King movement
		if (piece == Definitions.PieceName.King) {
			BitBoard kingObstructionBoard = BitBoard.Combination(friendlyPieces, checkInfo.hostileControlledSquares); // obstruction board containing friendly pieces and squares controlled by the enemy
			for (int i =0; i < lineDirections.Length; i ++) {
				Coord moveSquare = lineDirections[i] + origin;

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

						legalMoves.Add(CreateMove(origin,lineDirections[i] + origin, newGameState));
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
							legalMoves.Add(castleMove);
						}
					}
				
					if ((isWhite && nextGameState.castleQueensideW) || (!isWhite && nextGameState.castleQueensideB)) { // king still has right to castle queenside
						if (!castlingObstructionBoard.ContainsPieceAtSquare(new Coord("d" + kingRankName)) && !castlingObstructionBoard.ContainsPieceAtSquare(new Coord("c" + kingRankName))) { // no obstructions/checks on d and c files
							if (!kingObstructionBoard.ContainsPieceAtSquare(new Coord("b" + kingRankName))) { // no piece on b file
								Move castleMove = CreateMove(origin, new Coord("c" + kingRankName), castleGameState);
								castleMove.isCastles = true;
								castleMove.rookFrom = new Coord("a" + kingRankName);
								castleMove.rookTo = new Coord("d" + kingRankName);
								legalMoves.Add(castleMove);
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
						Move newMove;
						if (TryCreateMove(out newMove, origin,knightMoves[i] + origin, nextGameState)) {
							legalMoves.Add(newMove);
						}
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
						Move pawnCaptureMove;
						if (TryCreateMove(out pawnCaptureMove,origin, pawnCaptureSquares[i], nextGameState)) {
							if (isEpCapture) {
								pawnCaptureMove.isEnPassantCapture = true;
								pawnCaptureMove.enPassantPawnLocation = epPawnSquare;
							}
							pawnCaptureMove.isPawnPromotion = promotion;
							legalMoves.Add(pawnCaptureMove);
						}
					}
				}
			}

			// pawn movement
			if (!allPieces.ContainsPieceAtSquare (advanceSquare)) { // pawn is blocked from advancing by any piece
				Move pawnMove;
				if (TryCreateMove(out pawnMove, origin,advanceSquare, nextGameState)) {
					pawnMove.isPawnPromotion = promotion;
					legalMoves.Add(pawnMove);
				}

				// advance two squares on first move
				if ((isWhite && origin.y == 1) || (!isWhite && origin.y == 6)) { 
					Coord doubleAdvanceSquare = new Coord (origin.x, origin.y + advanceDir * 2);
					if (!allPieces.ContainsPieceAtSquare (doubleAdvanceSquare)) {
						GameState newGameState = nextGameState;
						newGameState.enPassantFileIndex = origin.x;

						Move doubleAdvanceMove;
						if (TryCreateMove(out doubleAdvanceMove, origin, doubleAdvanceSquare, newGameState)) {
							legalMoves.Add(doubleAdvanceMove);
						}
					}
				}
			}

		}
		// Rook, Bishop, Queen
		else {
			// index 0,1,2,3 = orthogonal directions; index 4,5,6,7 = diagonal directions
			int startIndex = 0;
			int endIndex = 7;
			bool dontRun = false;
			if (piece == Definitions.PieceName.Bishop) {
				startIndex = 4;
			}
			else if (piece == Definitions.PieceName.Rook) {
				endIndex = 3;
			}


			for (int i = 0; i < 8 ; i ++) {

				if (checkInfo.pinBoards[i].ContainsPieceAtSquare(origin)) { // if piece is pinned
					startIndex = i;
					endIndex = i;
					if (piece == Definitions.PieceName.Bishop && startIndex <4) {
						dontRun = true;
					}
					else if (piece == Definitions.PieceName.Rook && startIndex >3) {
						dontRun = true;
					}
				}
			}

			if (!dontRun) {
				for (int lineDirIndex = startIndex; lineDirIndex <= endIndex; lineDirIndex ++) {
					bool lineOpen = true;
					for (int i = 1; i < 8; i++) {
						Coord lineCoord = new Coord(origin.x + lineDirections[lineDirIndex].x * i, origin.y + lineDirections[lineDirIndex].y * i);
						lineOpen = lineCoord.inBoard;

						if (lineOpen) {
							if (allPieces.ContainsPieceAtSquare(lineCoord)) { // something is obstructing piece
								lineOpen = false;
							}
							if (!friendlyPieces.ContainsPieceAtSquare(lineCoord)) { // enemy piece or empty square, piece can move/capture this square
								bool canAddMove = true;
								if (checkInfo.inCheck) { // if king is in check, can only make moves that will block the check
								
									if (!checkInfo.checkBlockBoard.ContainsPieceAtSquare(lineCoord)) {
										canAddMove = false;
									}
								}


								if (canAddMove) {
									Move newMove;
									if (TryCreateMove(out newMove,origin,lineCoord, nextGameState)) {
										legalMoves.Add(newMove);
									}

								}
							}
						}
					
						if (!lineOpen) {
							break;
						}
					}
				}
			}
		}

		timePseudolegalMoves.Stop ();
	}




	/// Creates a move with given information
	/// Also checks if any rooks have been moved/captured and updates castling rights accordingly
	/// Also sets move colour
	bool TryCreateMove(out Move move, Coord from, Coord to, GameState gameState) {
		move = null;
		if (from != checkInfo.kingSquare) { // if not king piece

			if (Coord.Collinear(from,checkInfo.kingSquare)) { // is on line from king
				Coord dirFromKing = Coord.Direction(from, checkInfo.kingSquare);
				int lineDirIndex= Array.IndexOf(lineDirections,dirFromKing);

				if (checkInfo.pinBoards[lineDirIndex].ContainsPieceAtSquare(from)) { // if piece is pinned
					if (Coord.Collinear(to,checkInfo.kingSquare)) {
						Coord newDirFromKing = Coord.Direction(to, checkInfo.kingSquare);
						if (newDirFromKing != dirFromKing) { // pinned, but no longer on same line = illegal
							return false;
						}
					}
					else {
						return false;
					}
				}
			}

			if (checkInfo.inCheck) { // must block check
				if (!checkInfo.checkBlockBoard.ContainsPieceAtSquare(to)) {
					return false;
				}
			}
		}

	

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
		move = new Move (from, to, gameState);
		move.isWhiteMove = isWhite;

		return true;
	}

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
				attackBoard.SafeSetSquare(lineDirections[i] + origin);
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
	

	/// Calculates all necessary information about checks/pins in the position
	CheckInfo GetCheckInfo(Position position) {
		CheckInfo checkInfo = new CheckInfo ();
		
		bool isWhite = position.gameState.whiteToMove;
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
		
		// Only calculate pin/check block boards if king is not in double check
		// Reason: If in double check the king is the only piece that can move, so info is unecessary.
		if (!checkInfo.inDoubleCheck) {
			
			List<Coord> checkBlockSquares = new List<Coord>();

			
			// Knight checks

			for (int i = 0; i < knightMoves.Length; i ++) {
				Coord knightAttackCoord = new Coord(friendlyKingPosition.x + knightMoves[i].x, friendlyKingPosition.y + knightMoves[i].y);
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

			List<List<Coord>> pinLines = new List<List<Coord>>();
			for (int i =0; i < 8; i ++) {
				pinLines.Add(new List<Coord>());
			}

			// Sliding piece checks
			for (int dirIndex = 0; dirIndex < lineDirections.Length; dirIndex ++) {
				BitBoard hostileLinePieceMask = (dirIndex < 4)?hostileOrthogonalPieces:hostileDiagonalPieces; // first 4 directions are orthog, next four are diag. Only check for pieces with correct movement type
				
				List<Coord> lineCoords = new List<Coord>();
				bool lineHasFriendly = false;
				Coord dir = lineDirections[dirIndex];
				
				for (int i = 1; i < 8; i ++) {
					Coord nextSquare = new Coord(friendlyKingPosition.x + dir.x * i, friendlyKingPosition.y + dir.y * i); // rays going out from friendly king position
					lineCoords.Add(nextSquare);

					if (!nextSquare.inBoard) {
						break;
					}

					if (friendlyPieces.ContainsPieceAtSquare(nextSquare)) { // friendly piece found
						if (lineHasFriendly) { // two friendly pieces in a row eliminates possiblity of pin
							break;
						}
						else {
							lineHasFriendly = true;
						}
						
					}
					else if (hostilePieces.ContainsPieceAtSquare(nextSquare)) { // hostile piece found (note this hostile piece is not necesarilly capable of checking king)
						if (hostileLinePieceMask.ContainsPieceAtSquare(nextSquare)) { // this piece IS capable of checking king
							if (lineHasFriendly) { // friendly piece between king and hostile piece - thus piece is pinned
								pinLines[dirIndex] = lineCoords;
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

}
