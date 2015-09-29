using System.Collections.Generic;
using System;

public static class Board {


	// Piece type codes:
	// to represent colour, a 1 can be added to make the piece white.
	public const int pawnCode = 2;
	public const int rookCode = 4;
	public const int knightCode = 6;
	public const int bishopCode = 8;
	public const int queenCode = 10;
	public const int kingCode = 12;

	static int[] pieceCodeArray = new int[]{queenCode,rookCode,knightCode, bishopCode, pawnCode, kingCode}; // (this order puts pawn-promotable pieces first)

	// Corner indices of chessboard
	const int a1 = 0;
	const int a8 = 56;
	const int h1 = 7;
	const int h8 = 63;

	/// Board array of piece codes.
	/// Note that colour information is included in the codes.
	public static int[] boardArray;

	static Stack<ushort> gameStateHistory = new Stack<ushort> ();

	public static ushort currentGamestate {
		get {
			return gameStateHistory.Peek ();
		}
	}

	public static int GetPiece(int squareIndex) {
		return boardArray [squareIndex];
	}

	public static int GetPiece(int x, int y) {
		return boardArray[y * 8 + x];
	}
	
	/*

	/// Undo the previous move
	public static void UnmakeMove (Move move)
	{
		gameStateHistory.Pop (); // return to previous game state


		SetBoardArraySquare(move.fromIndex, move.movePieceType, move.isWhiteMove);
		ClearBoardArraySquare(move.toIndex);

		if (move.isCapture) {
			if (move.isEnPassantCapture) {
				SetBoardArraySquare(move.enPassantPawnIndex,pawnCode, !move.isWhiteMove);
			}
			else {
				SetBoardArraySquare(move.toIndex,move.capturePieceType, !move.isWhiteMove);
			}
		}

		if (move.isPawnPromotion) {
			SetBoardArraySquare(move.fromIndex,pawnCode, move.isWhiteMove); // replace the promoted piece with pawn (board array)
		}

		if (move.isCastles) {
			SetBoardArraySquare(move.rookFromIndex, rookCode, move.isWhiteMove);
			ClearBoardArraySquare(move.rookToIndex);
		}

	}
*/

	/// Update all boards to reflect latest move
	public static void MakeMove (ushort move)
	{
		ushort newGamestate = currentGamestate;
		newGamestate ^= 1; // toggle side to move
		newGamestate &= 65055; // clear en passant file (1111111000011111)

		int colourToMove = currentGamestate & 1;
		int moveFromIndex = move & 63;
		int moveToIndex = (move >> 6) & 63;
		int promotionPieceIndex = (move >> 12) & 3; // 0 = queen, 1 = rook, 2 = knight, 3 = bishop

		int movePieceType = boardArray [moveFromIndex] & ~1; // get piece type code
		int capturePieceType = boardArray [moveToIndex] & ~1; // get capture piece type code


		// Update board with new move:
		boardArray [moveToIndex] = boardArray [moveToIndex];
		boardArray [moveFromIndex] = 0;

		// Pawn moves
		if (movePieceType == pawnCode) {
			if (moveToIndex >= 56 || moveToIndex <= 7) { // pawn has reached first/eighth rank
				boardArray [moveToIndex] = pieceCodeArray [promotionPieceIndex] + colourToMove; // add promoted piece to the board
			} else if (Math.Abs (moveToIndex - moveFromIndex) == 16) { // pawn advances two squares
				int pawnFile = moveToIndex / 8 + 1;
				newGamestate += (ushort)(pawnFile << 5); // add ep capture file to game state
			} else if (Math.Abs (moveToIndex - moveFromIndex) != 8) { // pawn capture
				if (boardArray [moveToIndex] == 0) { // seemingly capturing empty square, thus en passant capture has occurred
					int dir = (colourToMove == 1) ? 1 : -1;
					int epCapturedPawnIndex = moveToIndex - 8 * dir; // index of pawn being captured en passant
					boardArray [epCapturedPawnIndex] = 0; // remove captured pawn from board
				}
			}
		} 
		// Castling
		if ((currentGamestate & 30) != 0) { // if castling options still exist for either side (0000000000011110)
			if (movePieceType == kingCode) { // moving king immediately removes all castling rights
				if (colourToMove == 1) {
					newGamestate &= 65529; // remove white castling privileges (1111111111111001)
				} else {
					newGamestate &= 65511; // remove black castling privileges (1111111111100111)
				}
			}

			// if a rook moves, or is captured, castling rights on that side of the board will be removed
			if (moveFromIndex == h1 || moveToIndex == h1) {
				newGamestate &= 65533; // white kingside (1111111111111101)
			}
			else if (moveFromIndex == a1 || moveToIndex == a1) {
				newGamestate &= 65531; // white queenside (1111111111111011)
			}
			else if (moveFromIndex == h8 || moveToIndex == h8) {
				newGamestate &= 65527; // black kingside (1111111111110111)
			}
			else if (moveFromIndex == a8 || moveToIndex == a8) {
				newGamestate &= 65519; // black queenside (1111111111101111)
			}
		}

		gameStateHistory.Push (newGamestate);
	}
	
	/// Sets the board position from a given fen string
	/// Note that this will clear the board history
	public static void SetPositionFromFen (string fen)
	{
		boardArray = new int[64];
		ushort initialGameState = 0;

		string[] fenSections = fen.Split (' ');

		string pieceChars = "rnbqkpRNBQKP";
		string pieceFen = fenSections [0];
		int boardX = 0;
		int boardY = 7;
		
		for (int i = 0; i < pieceFen.Length; i ++) {
			char key = pieceFen [i];
			
			if (pieceChars.Contains (key.ToString ())) {
				int squareIndex = boardY*8 + boardX;
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

				boardArray[squareIndex] = pieceCode;

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

		// Game state
		string sideToMove = fenSections [1];
		string castlingRights = fenSections [2];
		string enPassantCaptureSquare = fenSections [3];
		string halfMoveNumber = fenSections [4];
		string fullMoveNumber = fenSections [5];

		// Set side to move (bit 1)
		if (sideToMove == "w") {
			initialGameState += 1;
		}

		// Set castling rights (bits 2,3,4,5)
		for (int i = 0; i < castlingRights.Length; i ++) {
			switch (castlingRights [i]) {
			case 'K':
				initialGameState += 1 << 1;
				break;
			case 'Q':
				initialGameState += 1 << 2;
				break;
			case 'k':
				initialGameState += 1 << 3;
				break;
			case 'q':
				initialGameState += 1 << 4;
				break;
			}
		}

		// En passant capture file (bits 6,7,8,9)
		if (enPassantCaptureSquare [0] != '-') {
			initialGameState += (ushort)(Definitions.fileNames.IndexOf (enPassantCaptureSquare [0]) << 5);
		}

		gameStateHistory.Clear ();
		gameStateHistory.Push (initialGameState);

		UnityEngine.Debug.Log ("state " + initialGameState);
		bool whiteToMove = (initialGameState & 1) != 0;
		bool w00 = (initialGameState & 1<<1) != 0;
		bool w000 = (initialGameState & 1<<2) != 0;
		bool b00 = (initialGameState & 1<<3 )!= 0;
		bool b000 = (initialGameState & 1<<4) != 0;
		int epFile = (initialGameState & 480) >> 5;

		UnityEngine.Debug.Log ("white to move: " + whiteToMove);
		UnityEngine.Debug.Log ("white 0-0 " + w00 + " 0-0-0 " + w000 + " black 0-0 " + b00 + " 0-0-0 " + b000);
		UnityEngine.Debug.Log ("ep file: " + epFile);


	}

	static int ColourCode(bool white) {
		return (white) ? 1 : 0;
	}
	

}