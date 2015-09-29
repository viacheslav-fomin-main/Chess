using System.Collections.Generic;

public static class Board {


	// Piece type codes:
	// to represent colour, a 1 can be added to make the piece white.
	public const int pawnCode = 2;
	public const int rookCode = 4;
	public const int knightCode = 6;
	public const int bishopCode = 8;
	public const int queenCode = 10;
	public const int kingCode = 12;

	// Corner indices of chessboard
	const int a1 = 0;
	const int a8 = 56;
	const int h1 = 7;
	const int h8 = 63;

	/// Board array of piece codes.
	/// Note that colour information is included in the codes.
	public static int[,] boardArray { get; private set; }

	static Bitboard pawnsW;
	static Bitboard rooksW;
	static Bitboard knightsW;
	static Bitboard bishopsW;
	static Bitboard queensW;
	static Bitboard kingW;
	static Bitboard allPiecesW;
	static Bitboard pawnsB;
	static Bitboard rooksB;
	static Bitboard knightsB;
	static Bitboard bishopsB;
	static Bitboard queensB;
	static Bitboard kingB;
	static Bitboard allPiecesB;

	static Stack<GameState> gameStateHistory = new Stack<GameState> ();

	public static GameState currentGamestate {
		get {
			return gameStateHistory.Peek ();
		}
	}

	/// Set board array at given index to the given piece type code.
	/// Since it is a TYPE code, it must not contain colour information (this is added automatically)
	static void SetBoardArraySquare(int index, int pieceTypeCode, bool isWhite) {
		int y = index / 8;
		int x = index - y*8;
		boardArray[x,y] = pieceTypeCode + ColourCode(isWhite);
	}

	/// Clears board array at given index
	static void ClearBoardArraySquare(int index) {
		SetBoardArraySquare (index, 0, false);
	}

	static void MakeMoveOnBoard (int pieceType, bool white, int fromIndex, int toIndex)
	{
		DefineSquareOnBoard (pieceType, white, fromIndex, false);
		DefineSquareOnBoard (pieceType, white, fromIndex, true);
	}

	/// sets or clears square on board based on set bool
	static void DefineSquareOnBoard (int pieceType, bool white, int squareIndex, bool set)
	{

		// Automatically set all pieces board along with given piecetype's board
		if (white) {
			allPiecesW.DefineSquare (squareIndex, set);
		} else {
			allPiecesB.DefineSquare (squareIndex, set);
		}

		switch (pieceType) {
		case pawnCode:
			Pawns(white).DefineSquare (squareIndex, set);
			break;
		case rookCode:
			Rooks(white).DefineSquare (squareIndex, set);
			break;
		case knightCode:
			Knights(white).DefineSquare (squareIndex, set);
			break;
		case bishopCode:
			Bishops(white).DefineSquare (squareIndex, set);
			break;
		case queenCode:
			Queens(white).DefineSquare (squareIndex, set);
			break;
		case kingCode:
			King(white).DefineSquare (squareIndex, set);
			break;
		}
	}

	/// Undo the previous move
	public static void UnmakeMove (Move move)
	{
		gameStateHistory.Pop (); // return to previous game state


		MakeMoveOnBoard(move.movePieceType, move.isWhiteMove, move.toIndex, move.fromIndex); // move piece back to where it came from
		SetBoardArraySquare(move.fromIndex, move.movePieceType, move.isWhiteMove);
		ClearBoardArraySquare(move.toIndex);

		if (move.isCapture) {
			if (move.isEnPassantCapture) {
				DefineSquareOnBoard(move.capturePieceType, !move.isWhiteMove, move.enPassantPawnIndex, true); // reinstate opponent's en passant captured pawn
				SetBoardArraySquare(move.enPassantPawnIndex,pawnCode, !move.isWhiteMove);
			}
			else {
				DefineSquareOnBoard(move.capturePieceType, !move.isWhiteMove, move.toIndex, true); // reinstate opponents captured piece
				SetBoardArraySquare(move.toIndex,move.capturePieceType, !move.isWhiteMove);
			}
		}

		if (move.isPawnPromotion) {
			DefineSquareOnBoard(move.promotionPieceType, move.isWhiteMove, move.toIndex, false); // remove promoted piece from board
			SetBoardArraySquare(move.fromIndex,pawnCode, move.isWhiteMove); // replace the promoted piece with pawn (board array)
		}

		if (move.isCastles) {
			MakeMoveOnBoard(rookCode, move.isWhiteMove, move.toIndex, move.fromIndex); // move rook back to original square before castling
			SetBoardArraySquare(move.rookFromIndex, rookCode, move.isWhiteMove);
			ClearBoardArraySquare(move.rookToIndex);
		}

	}

	/// Update all boards to reflect latest move
	public static void MakeMove (Move move)
	{
		// Update boards with new move:
		MakeMoveOnBoard (move.movePieceType, move.isWhiteMove, move.fromIndex, move.toIndex);
		SetBoardArraySquare(move.toIndex, move.movePieceType, move.isWhiteMove);
		ClearBoardArraySquare(move.fromIndex);

		if (move.isCapture) { // if capture, remove opponents piece from board
			DefineSquareOnBoard (move.capturePieceType, !move.isWhiteMove, move.toIndex, false);
		}

		// Pawn promotion
		if (move.isPawnPromotion) {
			DefineSquareOnBoard (pawnCode, move.isWhiteMove, move.toIndex, false); // remove now-promoted pawn from board
			DefineSquareOnBoard (move.promotionPieceType, move.isWhiteMove, move.toIndex, true); // add promoted piece to promoting player's board
			SetBoardArraySquare(move.toIndex, move.promotionPieceType, move.isWhiteMove);
		}
		
		// En passant capture
		if (move.isEnPassantCapture) {
			DefineSquareOnBoard(pawnCode, !move.isWhiteMove, move.enPassantPawnIndex, false); // remove opponent's pawn that was captured en passant
			ClearBoardArraySquare(move.enPassantPawnIndex);
		}
		
		// Castling
		if (move.isCastles) {
			MakeMoveOnBoard(rookCode, move.isWhiteMove, move.rookFromIndex, move.rookToIndex); // move the castling rook to its new position
			SetBoardArraySquare(move.rookToIndex, rookCode, move.isWhiteMove);
			ClearBoardArraySquare(move.rookFromIndex);
		}


		// Define new current game state:
		GameState newGamestate = currentGamestate;
		newGamestate.whiteToMove = !newGamestate.whiteToMove; // toggle colour to move

		// Update castling rights:
		if ((newGamestate.castleKingsideW || newGamestate.castleQueensideW) || (newGamestate.castleKingsideB || newGamestate.castleQueensideB)) { // If any colour still has the right to castle on any side
			// king move (including castling) immediately removes right to castle in future
			if (move.movePieceType == kingCode) {
				newGamestate.SetCastlingRights(move.isWhiteMove, false, false);
			}
			// If rook moves from original square, right to castle on that side is removed
			else if (move.movePieceType == rookCode) {
				if (move.isWhiteMove) {
					if (move.fromIndex == a1) {
						newGamestate.castleQueensideW = false;
					}
					else if (move.fromIndex == h1) {
						newGamestate.castleKingsideW = false;
					}
				}
				else {
					if (move.fromIndex == a8) {
						newGamestate.castleQueensideB = false;
					}
					else if (move.fromIndex == h8) {
						newGamestate.castleKingsideB = false;
					}
				}
			}
			// If opponent piece captures rook, right to castle on that side is removed
			if (move.isCapture && move.capturePieceType == rookCode) {
				if (move.toIndex == a1) {
					newGamestate.castleQueensideW = false;
				}
				else if (move.toIndex == h1) {
					newGamestate.castleKingsideW = false;
				}
				else if (move.toIndex == a8) {
					newGamestate.castleQueensideB = false;
				}
				else if (move.toIndex == h8) {
					newGamestate.castleKingsideB = false;
				}
			}
		}

		// Update en passant file index:
		if (move.movePieceType == pawnCode) {
			int deltaIndex = move.toIndex - move.fromIndex;
			newGamestate.enPassantFileIndex = -1; // ep turned off after each move

			if (deltaIndex == 16 || deltaIndex == -16) { // pawn has advanced two squares
				newGamestate.enPassantFileIndex = move.toIndex % 8; // set ep index to file along which pawn has advanced
			}
		}

		// Add new game state as current
		gameStateHistory.Push (newGamestate);
	}
	
	/// Sets the board position from a given fen string
	/// Note that this will clear the board history
	public static void SetPositionFromFen (string fen)
	{
		boardArray = new int[8,8];
		string[] fenSections = fen.Split (' ');

		string pieceChars = "rnbqkpRNBQKP";
		string pieceFen = fenSections [0];
		int boardX = 0;
		int boardY = 7;
		
		for (int i = 0; i < pieceFen.Length; i ++) {
			char key = pieceFen [i];
			
			if (pieceChars.Contains (key.ToString ())) {
				int squareIndex = Coord.CoordToIndex (boardX, boardY);
				bool white = char.IsUpper(key);
				int pieceCode = ColourCode(white);

				switch (key.ToString().ToUpper()) {
				case "R":
					pieceCode |= rookCode;
					break;
				case "N":
					pieceCode |= knightCode;
					break;
				case "B":
					pieceCode |= bishopCode;
					break;
				case "Q":
					pieceCode |= queenCode;
					break;
				case "K":
					pieceCode |= kingCode;
					break;
				case "P":
					pieceCode |= pawnCode;
					break;
				}

				DefineSquareOnBoard(pieceCode,white,squareIndex,true);
				boardArray[boardX, boardY] = pieceCode;

				boardX ++;
			} else if (key == '/') {
				boardX = 0;
				boardY --;
			} else {
				int skipCount;
				if (int.TryParse (key + "", out skipCount)) {
					boardX += skipCount;
				}
			}
		}

		allPiecesW = Bitboard.Combination (rooksW, knightsW, bishopsW, queensW, kingW, pawnsW);
		allPiecesB = Bitboard.Combination (rooksB, knightsB, bishopsB, queensB, kingB, pawnsB);

		// Game state
		string sideToMove = fenSections [1];
		string castlingRights = fenSections [2];
		string enPassantCaptureSquare = fenSections [3];
		string halfMoveNumber = fenSections [4];
		string fullMoveNumber = fenSections [5];

		bool whiteToMove = sideToMove == "w";

		bool whiteKingside = false;
		bool whiteQueenside = false;
		bool blackKingside = false;
		bool blackQueenside = false;

		for (int i = 0; i < castlingRights.Length; i ++) {
			switch (castlingRights [i]) {
			case 'K':
				whiteKingside = true;
				break;
			case 'Q':
				whiteQueenside = true;
				break;
			case 'k':
				blackKingside = true;
				break;
			case 'q':
				blackQueenside = true;
				break;
			}
		}

		int enPassantFile = Definitions.fileNames.IndexOf (enPassantCaptureSquare [0]);

		gameStateHistory.Clear ();
		gameStateHistory.Push (new GameState (whiteKingside, blackKingside, whiteQueenside, blackQueenside, enPassantFile, whiteToMove));
	}

	static int ColourCode(bool white) {
		return (white) ? 1 : 0;
	}

	// Methods for retrieving a particular bitboard
	public static Bitboard Rooks (bool white)
	{
		return (white) ? rooksW : rooksB;
	}

	public static Bitboard Knights (bool white)
	{
		return (white) ? knightsW : knightsB;
	}

	public static Bitboard Bishops (bool white)
	{
		return (white) ? bishopsW : bishopsB;
	}

	public static Bitboard Queens (bool white)
	{
		return (white) ? queensW : queensB;
	}

	public static Bitboard King (bool white)
	{
		return (white) ? kingW : kingB;
	}

	public static Bitboard Pawns (bool white)
	{
		return (white) ? pawnsW : pawnsB;
	}
	
	public static Bitboard AllPieces (bool white)
	{
		return (white) ? allPiecesW : allPiecesB;
	}

}