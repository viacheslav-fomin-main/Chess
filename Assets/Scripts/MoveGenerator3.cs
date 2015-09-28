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
	int[,] boardArray = new int[8,8];
	static Coord[] knightMoves = new Coord[]{
		new Coord (1, 2),
		new Coord (-1, 2),
		new Coord (1, -2),
		new Coord (-1, -2),
		new Coord (2, 1),
		new Coord (2, -1),
		new Coord (-2, 1),
		new Coord (-2, -1)
	};

	static Coord[] lineDirections = new Coord[]{ // first four are orthogonal, last four are diagonal
		new Coord (0, 1),
		new Coord (0, -1),
		new Coord (1, 0),
		new Coord (-1, 0),
		new Coord (1, 1),
		new Coord (1, -1),
		new Coord (-1, 1),
		new Coord (-1, -1)
	};

	const int empty = 0;
	const int pawn = 1;
	const int rook = 2;
	const int knight = 3;
	const int bishop = 4;
	const int queen = 5;
	const int king = 6;
	const int whiteCode = 8; // real value = 8
	
	CheckInfo checkInfo;
	bool isWhite;

	Position currentPosition;
	BitBoardStruct friendlyPieces;
	BitBoardStruct hostilePieces;
	BitBoardStruct allPieces;
	
	List<MoveOld> legalMoves;
	
	public MoveOld[] GetAllLegalMoves(Position position) {
		timeFull.Start ();

		for (int j = 0; j <= 1; j ++) {
			bool white = (j==1);
			SetArray(position.Pawns(white), pawn + ((white)?whiteCode:0)); 
			SetArray(position.Rooks(white), rook + ((white)?whiteCode:0)); 
			SetArray(position.Knights(white), knight + ((white)?whiteCode:0)); 
			SetArray(position.Bishops(white), bishop + ((white)?whiteCode:0)); 
			SetArray(position.Queens(white), queen + ((white)?whiteCode:0)); 
			SetArray(position.King(white), king + ((white)?whiteCode:0)); 
		}
		
		legalMoves = new List<MoveOld> ();
		isWhite = position.gameState.whiteToMove;
		friendlyPieces = position.AllPieces (isWhite);
		hostilePieces = position.AllPieces (!isWhite);
		allPieces = BitBoardStruct.Combination (friendlyPieces, hostilePieces);
		currentPosition = position;

		checkInfo = GetCheckInfo (position); // Generate info relating to checks (pins etc)
		GenerateLegalMovesForSquare (Definitions.PieceName.King, checkInfo.kingSquare);

		if (!checkInfo.inDoubleCheck) { // no pieces besides king can move when in double check
			GetAllMovesFromBoard(currentPosition.Pawns(isWhite), Definitions.PieceName.Pawn);
			GetAllMovesFromBoard(currentPosition.Rooks(isWhite), Definitions.PieceName.Rook);
			GetAllMovesFromBoard(currentPosition.Knights(isWhite), Definitions.PieceName.Knight);
			GetAllMovesFromBoard(currentPosition.Bishops(isWhite), Definitions.PieceName.Bishop);
			GetAllMovesFromBoard(currentPosition.Queens(isWhite), Definitions.PieceName.Queen);
		}
		
		timeFull.Stop ();
		timeAttackMoves.Stop ();
		timePinDiscoverDetection.Stop ();
		timePseudolegalMoves.Stop ();
		timeCheckInfo.Stop ();
		
		return legalMoves.ToArray();
	}

	public void GetAllMovesFromBoard(BitBoardStruct board, Definitions.PieceName piece) {
		int[] indices = board.GetActiveIndices();
		for (int i = 0; i < indices.Length; i ++) {
			GenerateLegalMovesForSquare(piece,new Coord(indices[i]));
		}
	}


	
	public void PrintTimes() {
		UnityEngine.Debug.Log ("hostile attack move generation : " + timeAttackMoves.ElapsedMilliseconds + " ms");
		UnityEngine.Debug.Log ("Pin/Discovery detection : " + timePinDiscoverDetection.ElapsedMilliseconds + " ms");
		UnityEngine.Debug.Log ("Psuedolegal move gen : " + timePseudolegalMoves.ElapsedMilliseconds + " ms");
		UnityEngine.Debug.Log ("Check info gen : " + timeCheckInfo.ElapsedMilliseconds + " ms");
		UnityEngine.Debug.Log ("wildcard : " + wildcard.ElapsedMilliseconds + " ms");
		UnityEngine.Debug.Log ("Total : " + timeFull.ElapsedMilliseconds + " ms");
	}
	
	
	void SetArray(BitBoardStruct board, int code) {
		int[] indices = board.GetActiveIndices();
		for (int i = 0; i < indices.Length; i ++) {
			Coord c =new Coord(indices[i]);
			boardArray[c.x,c.y] = code;
		}
	}
	
	/// Returns a bitboard of all moves possible for given piece origin, not considering checks/pins
	void GenerateLegalMovesForSquare(Definitions.PieceName piece, Coord origin) {
		timePseudolegalMoves.Start ();
		
		GameState nextGameState = currentPosition.gameState;
		nextGameState.whiteToMove = !isWhite; // toggle move
		
		// King movement
		if (piece == Definitions.PieceName.King) {

			BitBoardStruct kingObstructionBoard = BitBoardStruct.Combination(friendlyPieces, checkInfo.hostileControlledSquares); // obstruction board containing friendly pieces and squares controlled by the enemy
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
						MoveOld newMove;
						if (TryCreateMove(out newMove, origin,lineDirections[i] + origin, newGameState, piece)) {
							legalMoves.Add(newMove);
						}
					}
					
				}
			}
				
			// castling
			if ((isWhite && (nextGameState.castleKingsideW || nextGameState.castleQueensideW)) || (!isWhite && (nextGameState.castleKingsideB || nextGameState.castleQueensideB))) { // king still has the right to castle on at least one side
				BitBoardStruct castlingObstructionBoard = BitBoardStruct.Combination(kingObstructionBoard, hostilePieces); // add hostile pieces to obstruction board since the king cannot castle through them
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
						MoveOld castleMove;
						if (TryCreateMove(out castleMove, origin, new Coord("g" + kingRankName), castleGameState, piece)) {
							castleMove.isCastles = true;
							castleMove.rookFrom = new Coord("h" + kingRankName);
							castleMove.rookTo = new Coord("f" + kingRankName);
							legalMoves.Add(castleMove);
						}
					}
				}
				
				if ((isWhite && nextGameState.castleQueensideW) || (!isWhite && nextGameState.castleQueensideB)) { // king still has right to castle queenside
					if (!castlingObstructionBoard.ContainsPieceAtSquare(new Coord("d" + kingRankName)) && !castlingObstructionBoard.ContainsPieceAtSquare(new Coord("c" + kingRankName))) { // no obstructions/checks on d and c files
						if (!kingObstructionBoard.ContainsPieceAtSquare(new Coord("b" + kingRankName))) { // no piece on b file
							MoveOld castleMove;
							if (TryCreateMove(out castleMove, origin, new Coord("c" + kingRankName), castleGameState, piece)) {
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
						MoveOld newMove;
						if (TryCreateMove(out newMove, origin,knightMoves[i] + origin, nextGameState, piece)) {
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
			BitBoardStruct pawnCaptureMask = hostilePieces;
			Coord epCaptureSquare = new Coord(currentPosition.gameState.enPassantFileIndex,(isWhite)?5:2); // the square the pawn will capture to
			Coord epPawnSquare = new Coord(currentPosition.gameState.enPassantFileIndex,(isWhite)?4:3); // the square which the pawn being captured en passant actually occupies
			bool isEpCapture;
			
			Coord[] pawnCaptureSquares = new Coord[]{ // pawn captures on left and right diagonals
				new Coord (origin.x + 1, origin.y + advanceDir),
				new Coord (origin.x - 1, origin.y + advanceDir)
			};
			
			for (int i = 0; i < pawnCaptureSquares.Length; i ++) {
				isEpCapture = pawnCaptureSquares[i] == epCaptureSquare;
				if (pawnCaptureSquares[i].inBoard) {
					if (pawnCaptureMask.ContainsPieceAtSquare(pawnCaptureSquares[i]) || isEpCapture) { // can only capture if there is enemy piece at square (or is ep square)
						MoveOld pawnCaptureMove;
						bool enPassantCaptureIsPinned = false;
						if (TryCreateMove(out pawnCaptureMove,origin, pawnCaptureSquares[i], nextGameState, piece)) {
							if (isEpCapture) {
								pawnCaptureMove.isEnPassantCapture = true;
								pawnCaptureMove.enPassantPawnLocation = epPawnSquare;

								Position checkPos = currentPosition; // check if capture leaves king exposed
								pawnCaptureMove.gameStateAfterMove.whiteToMove = isWhite;
								checkPos.MakeMove(pawnCaptureMove);
								pawnCaptureMove.gameStateAfterMove.whiteToMove = !isWhite;
								enPassantCaptureIsPinned = KingInCheck(checkPos);
							}
							pawnCaptureMove.isPawnPromotion = promotion;
							if (!enPassantCaptureIsPinned) {
								legalMoves.Add(pawnCaptureMove);
							}
						}
					}
				}
			}
			
			// pawn movement
			if (!allPieces.ContainsPieceAtSquare (advanceSquare)) { // pawn is blocked from advancing by any piece
				MoveOld pawnMove;
				if (TryCreateMove(out pawnMove, origin,advanceSquare, nextGameState, piece)) {
					pawnMove.isPawnPromotion = promotion;
					legalMoves.Add(pawnMove);
				}
				
				// advance two squares on first move
				if ((isWhite && origin.y == 1) || (!isWhite && origin.y == 6)) { 
					Coord doubleAdvanceSquare = new Coord (origin.x, origin.y + advanceDir * 2);
					if (!allPieces.ContainsPieceAtSquare (doubleAdvanceSquare)) {
						GameState newGameState = nextGameState;
						newGameState.enPassantFileIndex = origin.x;
						
						MoveOld doubleAdvanceMove;
						if (TryCreateMove(out doubleAdvanceMove, origin, doubleAdvanceSquare, newGameState, piece)) {
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
									MoveOld newMove;
									if (TryCreateMove(out newMove,origin,lineCoord, nextGameState, piece)) {
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
	bool TryCreateMove(out MoveOld move, Coord from, Coord to, GameState gameState, Definitions.PieceName pieceName) {
		move = null;
		if (from != checkInfo.kingSquare) { // if not king piece
			
			if (Coord.Collinear(from,checkInfo.kingSquare)) { // is on line from king
				Coord dirFromKing = Coord.Direction(from, checkInfo.kingSquare);

				int lineDirIndex = 0;
				for (int i = 0; i < 8; i ++) {
					if (lineDirections[i] == dirFromKing) {
						lineDirIndex = i;
						break;
					}
				}
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
		move = new MoveOld (from, to, gameState);
		move.isWhiteMove = isWhite;

		move.movePieceType = boardArray [from.x, from.y];
		int captureVal = boardArray [to.x, to.y];
		if (captureVal != 0) {
			move.isCapture = true;
			move.capturePieceType = captureVal;
		}

		
		return true;
	}


	/// Is king in check (used for ep captures only)
	bool KingInCheck(Position position) {


		int friendlyKingSquareIndex = BitBoardStruct.BitIndex (position.King(isWhite).board); // index of the friendly king
		Coord friendlyKingPosition = new Coord (friendlyKingSquareIndex);
	
		BitBoardStruct hostileControlledSquares = new BitBoardStruct ();
		BitBoardStruct allPiecesSansFriendlyKing = BitBoardStruct.Combination(position.allPiecesB,position.allPiecesW);
		allPiecesSansFriendlyKing.SetSquare(friendlyKingPosition,false); // remove the friendly king from board so that hostile attacks are not blocked by it (king cannot be used to block a check)

		// boards containing positions of all pieces
		BitBoardStruct[] whitePieceBoards = new BitBoardStruct[]{position.pawnsW, position.rooksW, position.knightsW, position.bishopsW, position.queensW, position.kingW};
		BitBoardStruct[] blackPieceBoards = new BitBoardStruct[]{position.pawnsB, position.rooksB, position.knightsB, position.bishopsB, position.queensB, position.kingB};
		BitBoardStruct[] hostilePieceBoards = (isWhite) ? blackPieceBoards : whitePieceBoards;
		
		// order of the piece boards in the array
		Definitions.PieceName[] pieceBoardOrder = new Definitions.PieceName[] {
			Definitions.PieceName.Pawn,
			Definitions.PieceName.Rook,
			Definitions.PieceName.Knight,
			Definitions.PieceName.Bishop,
			Definitions.PieceName.Queen,
			Definitions.PieceName.King
		};
		
		BitBoardStruct hostileOrthogonalPieces = BitBoardStruct.Combination (hostilePieceBoards [1], hostilePieceBoards [4]); // mask of hostile rook and queen
		BitBoardStruct hostileDiagonalPieces = BitBoardStruct.Combination (hostilePieceBoards [3], hostilePieceBoards [4]); // mask of hostile bishop and queen
		BitBoardStruct hostileKnights = hostilePieceBoards [2];
		BitBoardStruct hostilePawns = hostilePieceBoards [0];
		
		// Get attack boards for each hostile piece and combine to form bitboard of all hostile-controlled squares
		for (int pieceBoardIndex = 0; pieceBoardIndex < hostilePieceBoards.Length; pieceBoardIndex ++) {
			for (int squareIndex = 0; squareIndex <= 63; squareIndex ++) {
				bool isPieceOnBoard = hostilePieceBoards[pieceBoardIndex].ContainsPieceAtSquare(squareIndex);
				
				if (isPieceOnBoard) {
					
					BitBoardStruct pieceAttackBoard = HostileAttackBoard(pieceBoardOrder[pieceBoardIndex], new Coord(squareIndex), allPiecesSansFriendlyKing);
					
					if (pieceAttackBoard.ContainsPieceAtSquare(friendlyKingSquareIndex)) { // incrememnt check count if attack square is same as friendly king
						return true;
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
	BitBoardStruct HostileAttackBoard(Definitions.PieceName piece, Coord origin,BitBoardStruct allPiecesSansFriendlyKing) {
		timeAttackMoves.Start ();
		BitBoardStruct attackBoard = new BitBoardStruct ();
		
	
		
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
	
	Stopwatch wildcard = new Stopwatch();

	/// Calculates all necessary information about checks/pins in the position
	CheckInfo GetCheckInfo(Position position) {

		CheckInfo checkInfo = new CheckInfo ();

		int friendlyKingSquareIndex = BitBoardStruct.BitIndex (position.King(isWhite).board); // index of the friendly king
		Coord friendlyKingPosition = new Coord (friendlyKingSquareIndex);

		BitBoardStruct allPiecesSansFriendlyKing = allPieces;
		allPiecesSansFriendlyKing.SetSquare(friendlyKingPosition,false); // remove the friendly king from board so that hostile attacks are not blocked by it (king cannot be used to block a check)

		BitBoardStruct hostileControlledSquares = new BitBoardStruct ();
		
		// boards containing positions of all pieces
		BitBoardStruct[] whitePieceBoards = new BitBoardStruct[]{position.pawnsW, position.rooksW, position.knightsW, position.bishopsW, position.queensW, position.kingW};
		BitBoardStruct[] blackPieceBoards = new BitBoardStruct[]{position.pawnsB, position.rooksB, position.knightsB, position.bishopsB, position.queensB, position.kingB};
		BitBoardStruct[] hostilePieceBoards = (isWhite) ? blackPieceBoards : whitePieceBoards;
		
		// order of the piece boards in the array
		Definitions.PieceName[] pieceBoardOrder = new Definitions.PieceName[] {
			Definitions.PieceName.Pawn,
			Definitions.PieceName.Rook,
			Definitions.PieceName.Knight,
			Definitions.PieceName.Bishop,
			Definitions.PieceName.Queen,
			Definitions.PieceName.King
		};
		
		BitBoardStruct hostileOrthogonalPieces = BitBoardStruct.Combination (hostilePieceBoards [1], hostilePieceBoards [4]); // mask of hostile rook and queen
		BitBoardStruct hostileDiagonalPieces = BitBoardStruct.Combination (hostilePieceBoards [3], hostilePieceBoards [4]); // mask of hostile bishop and queen
		BitBoardStruct hostileKnights = hostilePieceBoards [2];
		BitBoardStruct hostilePawns = hostilePieceBoards [0];

		int checkCount = 0; // number of checks delivered to friendly king
		
		// Get attack boards for each hostile piece and combine to form bitboard of all hostile-controlled squares
		for (int pieceBoardIndex = 0; pieceBoardIndex < hostilePieceBoards.Length; pieceBoardIndex ++) {
			for (int squareIndex = 0; squareIndex <= 63; squareIndex ++) {
				bool isPieceOnBoard = hostilePieceBoards[pieceBoardIndex].ContainsPieceAtSquare(squareIndex);
				
				if (isPieceOnBoard) {

					BitBoardStruct pieceAttackBoard = HostileAttackBoard(pieceBoardOrder[pieceBoardIndex], new Coord(squareIndex), allPiecesSansFriendlyKing);

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

		BitBoardStruct checkBlockBoard = new BitBoardStruct();
		List<BitBoardStruct> pinTemp = new List<BitBoardStruct> (8);

		// Only calculate pin/check block boards if king is not in double check
		// Reason: If in double check the king is the only piece that can move, so info is unecessary.
		if (!checkInfo.inDoubleCheck) {

			// Knight checks
			for (int i = 0; i < knightMoves.Length; i ++) {
				Coord knightAttackCoord = new Coord(friendlyKingPosition.x + knightMoves[i].x, friendlyKingPosition.y + knightMoves[i].y);
				if (hostileKnights.SafeContainsPieceAtSquare(knightAttackCoord)) {
					checkBlockBoard.SetSquare(knightAttackCoord);
					break;
				}
			}
			
			// Pawn checks
			int hostilePawnDir = (isWhite)?-1:1;
			Coord pawnAttackLeft = new Coord(friendlyKingPosition.x -1, friendlyKingPosition.y - hostilePawnDir);
			Coord pawnAttackRight = new Coord(friendlyKingPosition.x +1, friendlyKingPosition.y - hostilePawnDir);
			if (hostilePawns.SafeContainsPieceAtSquare(pawnAttackLeft)) {
				checkBlockBoard.SetSquare(pawnAttackLeft);
			}
			else if (hostilePawns.SafeContainsPieceAtSquare(pawnAttackRight)) {
				checkBlockBoard.SetSquare(pawnAttackRight);
			}
			
			List<List<Coord>> pinLines = new List<List<Coord>>(8);
			for (int i =0; i < 8; i ++) {
				pinLines.Add(new List<Coord>());
			}
			
			// Sliding piece checks
			for (int dirIndex = 0; dirIndex < lineDirections.Length; dirIndex ++) {
				pinTemp.Add(new BitBoardStruct());
				BitBoardStruct hostileLinePieceMask = (dirIndex < 4)?hostileOrthogonalPieces:hostileDiagonalPieces; // first 4 directions are orthog, next four are diag. Only check for pieces with correct movement type

				Coord directionFromKing = lineDirections[dirIndex];
				List<Coord> lineCoords = new List<Coord>();
				bool foundFriendlyPieceAlongLineFromKing = false;

				for (int i = 1; i < 8; i ++) { // iterate through all squares in direction
					Coord nextSquare = new Coord(friendlyKingPosition.x + directionFromKing.x * i, friendlyKingPosition.y + directionFromKing.y * i); // rays going out from friendly king position
					
					if (!nextSquare.inBoard) {
						break;
					}
					lineCoords.Add(nextSquare);
					
					if (friendlyPieces.ContainsPieceAtSquare(nextSquare)) { // friendly piece found
						if (foundFriendlyPieceAlongLineFromKing) { // two friendly pieces in a row eliminates possiblity of pin
							break;
						}
						else {
							foundFriendlyPieceAlongLineFromKing = true;
						}
					}
					else if (hostilePieces.ContainsPieceAtSquare(nextSquare)) { // hostile piece found (note this hostile piece is not necesarilly capable of checking king)
						if (hostileLinePieceMask.ContainsPieceAtSquare(nextSquare)) { // this piece IS capable of checking king
							if (foundFriendlyPieceAlongLineFromKing) { // friendly piece between king and hostile piece - thus piece is pinned
								pinLines[dirIndex] = lineCoords;
								pinTemp[dirIndex].SetSquares(lineCoords);
							}
							else { // no friendly piece between king and hostile piece, thus piece is checking the king
								checkBlockBoard.SetSquares(lineCoords);
							}
						}
						break; // if hostile piece that is incapable of checking king is in the way, then no pins/checks exist on this line
					}
				}
			}

			List<BitBoardStruct> pinBoards = new List<BitBoardStruct>();
			for (int i = 0; i < pinLines.Count; i ++) {
				BitBoardStruct pinLineBoard = new BitBoardStruct();
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
		public BitBoardStruct hostileControlledSquares; // squares attacked by hostile pieces
		public Coord kingSquare;
		
		public List<BitBoardStruct> pinBoards; // each pin board represents the movement options of the pinned piece
		public BitBoardStruct checkBlockBoard; // bitboard of squares which can be moved to in order to block (or capture) the checking piece
		
	}
	
}
