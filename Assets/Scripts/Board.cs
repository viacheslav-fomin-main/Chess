using System.Collections.Generic;
using System;

public static class Board {
	
	public static bool debugMode;
	
	// Piece type codes:
	// to represent colour, a 1 can be added to make the piece white.
	public const int pawnCode = 2;
	public const int rookCode = 4;
	public const int knightCode = 6;
	public const int bishopCode = 8;
	public const int queenCode = 10;
	public const int kingCode = 12;
	
	public const int whiteCode = 1;
	public const int blackCode = 0;

	public static ulong zobristKey;
	
	public static int[] pieceCodeArray = new int[]{queenCode,rookCode,knightCode, bishopCode, pawnCode, kingCode}; // (this order puts pawn-promotable pieces first)
	
	// Corner indices of chessboard
	const int a1 = 0;
	const int a8 = 112;
	const int h1 = 7;
	const int h8 = 119;
	
	public static Dictionary<int,char> pieceNameDictionary = new Dictionary<int, char> ();
	
	/// Board array of piece codes.
	/// Note that colour information is included in the codes.
	public static int[] boardArray;
	/// Board array representing colour of pieces. (0=black, 1=white, -1=vacant)
	public static int[] boardColourArray;

	public static int blackKingIndex;
	public static int whiteKingIndex;
	
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
	
	public static int Convert64to128(int index) {
		int y = index / 8;
		return index + y * 8;
	}
	
	public static int Convert128to64(int index) {
		int y = index / 16;
		return index - y * 8;
	}

	/// Returns rank (from 1-8) given a 128-based index
	public static int RankFrom128(int index) {
		return index / 16 + 1;
	}

	/// Returns file (from 1-8) given a 128-based index
	public static int FileFrom128(int index) {
		return index % 16 + 1;
	}

	public static bool WhiteHasCastlingRights() {
		return (currentGamestate & 6) != 0;
	}

	public static bool BlackHasCastlingRights() {
		return (currentGamestate & 24) != 0;
	}
	
	/// Undo the previous move
	public static void UnmakeMove (ushort move, bool updateUI = false)
	{
		ushort gamestateBeforeUndo = gameStateHistory.Pop (); // return to previous game state. currentGamestate now refers to the state after the undo
		
		int colourToMove = currentGamestate & 1; // the colour of player whose move is being undone
		int moveFromIndex = move & 127; // the square the piece originally moved FROM (now moving TO this square since in undo mode)
		int moveToIndex = (move >> 7) & 127;
		int movePieceCode = boardArray [moveToIndex];
		int movePieceType = movePieceCode & ~1;
		int capturedPieceCode = (gamestateBeforeUndo >> 9) & 15;

		// Update board
		boardArray [moveFromIndex] = movePieceCode;
		boardArray [moveToIndex] = capturedPieceCode;
		// Update colour board
		SetColourBoard(moveFromIndex,moveToIndex, colourToMove); 
		SetColourBoard (moveToIndex, capturedPieceCode);

		// update king positions
		if (movePieceType == kingCode) {
			if (colourToMove == 1) {
				whiteKingIndex = moveFromIndex;
			}
			else {
				blackKingIndex = moveFromIndex;
			}
		}
		else if (capturedPieceCode == kingCode) { // king may be allowed to be captured during alphabeta search
			blackKingIndex = moveToIndex;
		} else if (capturedPieceCode == kingCode + 1) {
			whiteKingIndex = moveToIndex;
		}

		// Update zobrist key
		zobristKey ^= ZobristKey.sideToMove; // toggle side to move
		zobristKey ^= ZobristKey.piecesArray[(movePieceType >> 1) - 1, colourToMove,moveToIndex]; // remove piece from old square
		zobristKey ^= ZobristKey.piecesArray[(movePieceType >> 1) - 1, colourToMove,moveFromIndex]; // place piece at new square
		if (capturedPieceCode != 0) {
			zobristKey ^= ZobristKey.piecesArray [((capturedPieceCode & ~1) >> 1) - 1, 1 - colourToMove, moveToIndex]; // remove captured piece
		}

		// remove old ep square from zobrist key
		int epIndex = (gamestateBeforeUndo >> 5 & 15) -1;
		if (epIndex != -1) {
			zobristKey ^= ZobristKey.enPassantSquare[epIndex + (((currentGamestate & 1) == 0)?80:32)];
		}

		// add new ep square to zobrist key
		epIndex = (currentGamestate >> 5 & 15) -1;
		if (epIndex != -1) {
			zobristKey ^= ZobristKey.enPassantSquare[epIndex + (((currentGamestate & 1) == 1)?80:32)];
		}

		// remove old castling rights from zobrist key
		zobristKey ^= ZobristKey.castlingRightsWhite[(gamestateBeforeUndo >> 1) & 3];
		zobristKey ^= ZobristKey.castlingRightsBlack[(gamestateBeforeUndo >> 3) & 3];
		
		// add new castling rights to zobrist key
		zobristKey ^= ZobristKey.castlingRightsWhite[(currentGamestate >> 1) & 3];
		zobristKey ^= ZobristKey.castlingRightsBlack[(currentGamestate >> 3) & 3];
		
		if ((gamestateBeforeUndo & 1 << 15) != 0) { // move was castles; move rook back to original square
			int rookFromIndex = 0;
			int rookToIndex = 0;
			if (moveToIndex == 2) { // white 0-0-0
				rookFromIndex = 3;
				rookToIndex = 0;
			}
			else if (moveToIndex == 6) { // white 0-0
				rookFromIndex = 5;
				rookToIndex = 7;
			}
			else if (moveToIndex == 114) { // black 0-0-0
				rookFromIndex = 115;
				rookToIndex = 112;
			}
			else if (moveToIndex == 118) { // black 0-0
				rookFromIndex = 117;
				rookToIndex = 119;
			}

			boardArray[rookFromIndex] = 0;
			boardArray[rookToIndex] = rookCode + colourToMove;
			SetColourBoard(rookToIndex,rookFromIndex, colourToMove);

			zobristKey ^= ZobristKey.piecesArray[(rookCode>>1) - 1, colourToMove, rookFromIndex]; // remove rook from old square in zobrist key
			zobristKey ^= ZobristKey.piecesArray[(rookCode>>1) - 1, colourToMove, rookToIndex]; // add rook to new square in zobrist key
		}
		else if ((gamestateBeforeUndo & 1 << 13) != 0) { // pawn captured en passant; put opponent-coloured pawn back on ep capture square.
			int dir = (colourToMove == 1) ? 1 : -1;
			int epCapturedPawnIndex = moveToIndex - 16 * dir; // index of pawn that was captured en passant
			boardArray [epCapturedPawnIndex] = pawnCode + (1 - colourToMove); // add ep captured pawn back onto board
			boardColourArray [epCapturedPawnIndex] = (1 - colourToMove);

			zobristKey ^= ZobristKey.piecesArray[(pawnCode>>1)-1, 1-colourToMove, epCapturedPawnIndex]; // add captured ep pawn to zobrist key
		}
		else if ((gamestateBeforeUndo & 1 << 14) != 0) { // move was promotion; replace promoted piece with pawn
			boardArray [moveFromIndex] = pawnCode + colourToMove;
			zobristKey ^= ZobristKey.piecesArray[(movePieceType >> 1) - 1, colourToMove,moveFromIndex]; // remove promoted piece from zobrist key
			zobristKey ^= ZobristKey.piecesArray[(pawnCode>>1)-1,colourToMove, moveFromIndex]; // add the demoted pawn to zobrist key
		}
		
		if (debugMode) {
			DebugGameState (currentGamestate);
		}
		
		if (updateUI) {
			UpdatePhysicalBoard ();
		}
	}
	
	
	/// Update all boards to reflect latest move
	/// Note that move is assumed to be legal
	public static void MakeMove (ushort move, bool updateUI = false)
	{
		ushort newGamestate = (ushort)(currentGamestate & 31); // only copy side to move and castling rights from current gamestate (0000000000011111)
		
		int colourToMove = currentGamestate & 1;
		int moveFromIndex = move & 127;
		int moveToIndex = (move >> 7) & 127;
		int promotionPieceIndex = (move >> 14) & 3; // 0 = queen, 1 = rook, 2 = knight, 3 = bishop
		
		int movePieceCode = boardArray [moveFromIndex]; // get move piece code
		int movePieceType = movePieceCode & ~1; // get move piece type code (no colour info)
		int capturedPieceCode = boardArray [moveToIndex]; // get capture piece code
		int promotionPieceCode = 0; // this assigned later if promotion occurs


		// Update board with new move:
		boardArray [moveToIndex] = movePieceCode;
		boardArray [moveFromIndex] = 0;
		// Update colour board
		SetColourBoard(moveToIndex,moveFromIndex, colourToMove);

		// Update zobrist key
		zobristKey ^= ZobristKey.sideToMove; // toggle side to move
		zobristKey ^= ZobristKey.piecesArray[(movePieceType >> 1) - 1, colourToMove,moveToIndex]; // place piece at new square
		zobristKey ^= ZobristKey.piecesArray[(movePieceType >> 1) - 1, colourToMove,moveFromIndex]; // remove piece from old square

		if (capturedPieceCode != 0) {
			zobristKey ^= ZobristKey.piecesArray[((capturedPieceCode & ~1) >> 1) -1, 1-colourToMove,moveToIndex]; // remove captured piece
		}

		// update king positions
		if (movePieceType == kingCode) {
			if (colourToMove == 1) {
				whiteKingIndex = moveToIndex;
			} else {
				blackKingIndex = moveToIndex;
			}
		} else if (capturedPieceCode == kingCode) { // king may be allowed to be captured during alphabeta search
			blackKingIndex = -1;
		} else if (capturedPieceCode == kingCode + 1) {
			whiteKingIndex = 1;
		}
		
		// Pawn moves
		if (movePieceType == pawnCode) {
			if (moveToIndex >= 112 || moveToIndex <= 7) { // pawn has reached first/eighth rank
				newGamestate |= 1<<14; // record in game state that pawn promoted this move
				promotionPieceIndex = move >> 14 & 3;
				promotionPieceCode = pieceCodeArray [promotionPieceIndex] + colourToMove;
				boardArray [moveToIndex] = promotionPieceCode; // add promoted piece to the board

				zobristKey ^= ZobristKey.piecesArray[(movePieceType >> 1)-1, colourToMove,moveToIndex]; // remove pawn from zobrist key
				zobristKey ^= ZobristKey.piecesArray[(pieceCodeArray [promotionPieceIndex] >> 1)-1, colourToMove,moveToIndex]; // add promoted piece to zobrist key
			}
			else if (Math.Abs (moveToIndex - moveFromIndex) == 32) { // pawn advances two squares
				int pawnFile = moveToIndex % 8 + 1; // file is stored from 1-8 (because 0 is reserved for no ep square)
				newGamestate += (ushort)(pawnFile << 5); // add ep capture file to game state

			}
			else if (Math.Abs (moveToIndex - moveFromIndex) != 16) { // pawn capture
				if (capturedPieceCode == 0) { // seemingly capturing empty square, thus en passant capture has occurred
					newGamestate |= 1<<13; // record in game state that pawn captured en passant this move
					int dir = (colourToMove == 1) ? 1 : -1;
					int epCapturedPawnIndex = moveToIndex - 16 * dir; // index of pawn being captured en passant
					boardArray [epCapturedPawnIndex] = 0; // remove captured pawn from board
					boardColourArray [epCapturedPawnIndex] = -1;

					zobristKey ^= ZobristKey.piecesArray[(pawnCode >> 1)-1, 1-colourToMove,epCapturedPawnIndex]; // remove ep captured pawn from zobrist key
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
				
				if (Math.Abs(moveToIndex-moveFromIndex) == 2) { // king is castling this move
					newGamestate |= 1<<15; // record in game state that king castled this move
					int rookFromIndex = 0;
					int rookToIndex = 0;

					if (moveToIndex == 2) { // white 0-0-0
						rookToIndex = 3;
						rookFromIndex = 0;
					}
					else if (moveToIndex == 6) { // white 0-0
						rookToIndex = 5;
						rookFromIndex = 7;
					}
					else if (moveToIndex == 114) { // black 0-0-0
						rookToIndex = 115;
						rookFromIndex = 112;
					}
					else if (moveToIndex == 118) { // black 0-0
						rookToIndex = 117;
						rookFromIndex = 119;
					}

					boardArray[rookToIndex] = rookCode + colourToMove;
					boardArray[rookFromIndex] = 0;
					SetColourBoard(rookToIndex,rookFromIndex,colourToMove);

					zobristKey ^= ZobristKey.piecesArray[(rookCode>>1) - 1, colourToMove, rookFromIndex]; // remove rook from old square in zobrist key
					zobristKey ^= ZobristKey.piecesArray[(rookCode>>1) - 1, colourToMove, rookToIndex]; // add rook to new square in zobrist key
				}
			}
			
			// if a rook moves, or is captured, castling rights on that side of the board will be removed
			if (moveFromIndex == h1 || moveToIndex == h1) {
				newGamestate &= 65533; // white kingside castle no longer allowed (1111111111111101)
			}
			else if (moveFromIndex == a1 || moveToIndex == a1) {
				newGamestate &= 65531; // white queenside no longer allowed (1111111111111011)
			}
			else if (moveFromIndex == h8 || moveToIndex == h8) {
				newGamestate &= 65527; // black kingside no longer allowed (1111111111110111)
			}
			else if (moveFromIndex == a8 || moveToIndex == a8) {
				newGamestate &= 65519; // black queenside no longer allowed (1111111111101111)
			}
		}

		// remove old ep square from zobrist key
		int epIndex = (currentGamestate >> 5 & 15) -1;
		if (epIndex != -1) {
			zobristKey ^= ZobristKey.enPassantSquare[epIndex + (((currentGamestate & 1) == 1)?80:32)];
		}

		// add new ep square to zobrist key
		epIndex = (newGamestate >> 5 & 15) -1;
		if (epIndex != -1) {
			zobristKey ^= ZobristKey.enPassantSquare[epIndex + (((currentGamestate & 1) == 0)?80:32)];
		}

		// remove old castling rights from zobrist key
		zobristKey ^= ZobristKey.castlingRightsWhite[(currentGamestate >> 1) & 3];
		zobristKey ^= ZobristKey.castlingRightsBlack[(currentGamestate >> 3) & 3];

		// add new castling rights to zobrist key
		zobristKey ^= ZobristKey.castlingRightsWhite[(newGamestate >> 1) & 3];
		zobristKey ^= ZobristKey.castlingRightsBlack[(newGamestate >> 3) & 3];

		newGamestate ^= 1; // toggle side to move
		newGamestate |= (ushort)(capturedPieceCode << 9); // set last captured piece type
		gameStateHistory.Push (newGamestate);
		
		if (debugMode) {
			DebugGameState (newGamestate);
		}
		
		if (updateUI) {
			UpdatePhysicalBoard(moveFromIndex, moveToIndex);
		}
	}
	
	static void UpdatePhysicalBoard(int moveFromIndex, int moveToIndex) {
		ChessUI.instance.AutoUpdate ();
		ChessUI.instance.HighlightMove (moveFromIndex, moveToIndex);
	}

	static void UpdatePhysicalBoard() {
		ChessUI.instance.AutoUpdate ();
	}
	
	static bool isInitialized;
	static void Init() {
		if (!isInitialized) {
			isInitialized = true;
			pieceNameDictionary.Add (0, ' ');
			
			pieceNameDictionary.Add (pawnCode, 'p');
			pieceNameDictionary.Add (rookCode, 'r');
			pieceNameDictionary.Add (knightCode, 'n');
			pieceNameDictionary.Add (bishopCode, 'b');
			pieceNameDictionary.Add (queenCode, 'q');
			pieceNameDictionary.Add (kingCode, 'k');
			
			pieceNameDictionary.Add (pawnCode + 1, 'P');
			pieceNameDictionary.Add (rookCode + 1, 'R');
			pieceNameDictionary.Add (knightCode + 1, 'N');
			pieceNameDictionary.Add (bishopCode + 1, 'B');
			pieceNameDictionary.Add (queenCode + 1, 'Q');
			pieceNameDictionary.Add (kingCode + 1, 'K');
		}
	}
	
	static void SetColourBoard(int setIndex, int pieceCode) {
		if (pieceCode == 0) {
			boardColourArray [setIndex] = -1;
		} else {
			boardColourArray [setIndex] = pieceCode & 1;
		}
	}
	
	static void SetColourBoard(int setIndex, int clearIndex, int colour) {
		boardColourArray [clearIndex] = -1;
		boardColourArray[setIndex] = colour;
	}
	
	/// Sets the board position from a given fen string
	/// Note that this will clear the board history
	public static void SetPositionFromFen (string fen, bool updateBoardUI = true)
	{
		Init ();
		
		boardArray = new int[128];
		boardColourArray = new int[128];
		for (int i = 0; i <= 127; i ++) { // clear colour array (all values to -1)
			boardColourArray[i] = -1;
		}
		
		ushort initialGameState = 0;
		
		string[] fenSections = fen.Split (' ');
		
		string pieceChars = "rnbqkpRNBQKP";
		string pieceFen = fenSections [0];
		int boardX = 0;
		int boardY = 7;
		
		for (int i = 0; i < pieceFen.Length; i ++) {
			char key = pieceFen [i];
			
			if (pieceChars.Contains (key.ToString ())) {
				int squareIndex64 = boardY*8 + boardX;
				int squareIndex128 = Convert64to128(squareIndex64);
				bool white = char.IsUpper(key);
				int pieceCode = ColourCode(white);
				
				switch (key.ToString().ToUpper().ToCharArray()[0]) {
				case 'R':
					pieceCode |= rookCode;
					break;
				case 'N':
					pieceCode |= knightCode;
					break;
				case 'B':
					pieceCode |= bishopCode;
					break;
				case 'Q':
					pieceCode |= queenCode;
					break;
				case 'K':
					pieceCode |= kingCode;
					if (white) {
						whiteKingIndex = squareIndex128;
					}
					else {
						blackKingIndex = squareIndex128;
					}
					break;
				case 'P':
					pieceCode |= pawnCode;
					break;
				}
				
				boardArray[squareIndex128] = pieceCode;
				SetColourBoard(squareIndex128,squareIndex128, pieceCode & 1);
				
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
		//string halfMoveNumber = fenSections [4];
		//	string fullMoveNumber = fenSections [5];
		
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

		zobristKey = ZobristKey.GetZobristKey ();

		if (updateBoardUI) {
			UpdatePhysicalBoard ();
		}

	}
	
	static void MakeTestMove() {
		return;
		/*
		ushort moveA = 0;
		ushort moveB = 0;
		ushort moveC = 0;
		ushort moveD = 0;
		ushort moveE = 0;
		ushort moveF = 0;

		// 0-0
		moveA = 4 | 6<<7;
		MakeMove (moveA,true);

		// 0-0-0
		moveB = 116 | 114<<7;
		MakeMove (moveB,true);

		// exf8=Q
		moveC = 100 | 117 << 7;
		MakeMove (moveC,true);

		// f4
		moveD = 69 | 53 << 7;
		MakeMove (moveD,true);

		// g4
		moveE = 22 | 54 << 7;
		MakeMove (moveE,true);

		// fxg3
		moveF = 53 | 38 << 7;
		MakeMove (moveF,true);


		UnityEngine.Debug.Log ("## undo fxg3");
		UnmakeMove (moveF, true);
	
		//UnityEngine.Debug.Log ("## undo exf8=q");
		UnmakeMove (moveE, true);
		//UnityEngine.Debug.Log ("## undo black 0-0-0");
		UnmakeMove (moveD, true);
		//UnityEngine.Debug.Log ("## undo white 0-0");
		UnmakeMove (moveC, true);
		UnmakeMove (moveB, true);
		UnmakeMove (moveA, true);
		*/
		
	}
	
	public static bool IsWhiteToPlay() {
		return ((currentGamestate&1) == 1);
	}
	
	public static void PrintBoard(string optionalHeader = "") {
		string boardString = "";
		string colourString = "";
		
		if (optionalHeader != "") {
			optionalHeader += ":\n";
		}
		
		for (int y = 7; y>=0; y--) {
			for (int x = 0; x < 8; x ++) {
				int i = Board.Convert64to128(y*8+x);
				// pieces
				int code = boardArray [i];
				if (code == 0) {
					boardString += " # ";
				}
				else {
					boardString += " " +pieceNameDictionary [code] + " ";
				}
				
				// colour
				string colString = (boardColourArray[i] == 0)?" B ":" W ";
				if (boardColourArray[i] == -1) {
					colString = " # ";
				}
				colourString += colString;
			}
			boardString += "\n";
			colourString += "\n";
		}
		UnityEngine.Debug.Log (optionalHeader + boardString);
	}
	
	public static void DebugGameState(ushort state) {
		UnityEngine.Debug.Log ("state " + state);
		bool whiteToMove = (state & 1) != 0;
		bool w00 = (state & 1<<1) != 0;
		bool w000 = (state & 1<<2) != 0;
		bool b00 = (state & 1<<3 )!= 0;
		bool b000 = (state & 1<<4) != 0;
		int epFile = (state >> 5) & 15;
		int capturedPieceCode = (state >> 9) & 15;
		
		UnityEngine.Debug.Log ("white to move: " + whiteToMove);
		UnityEngine.Debug.Log ("white 0-0 " + w00 + " 0-0-0 " + w000 + " black 0-0 " + b00 + " 0-0-0 " + b000);
		UnityEngine.Debug.Log ("ep file: " + epFile);
		string boardString = "";
		string colourString = "";
		
		if (capturedPieceCode != 0) {
			UnityEngine.Debug.Log ("Captured piece: " + pieceNameDictionary [capturedPieceCode]);
		}
		
		for (int y = 7; y>=0; y--) {
			for (int x = 0; x < 8; x ++) {
				int i = Board.Convert64to128(y*8+x);
				// pieces
				int code = boardArray [i];
				if (code == 0) {
					boardString += " # ";
				}
				else {
					boardString += " " +pieceNameDictionary [code] + " ";
				}
				
				// colour
				string colString = (boardColourArray[i] == 0)?" B ":" W ";
				if (boardColourArray[i] == -1) {
					colString = " # ";
				}
				colourString += colString;
			}
			boardString += "\n";
			colourString += "\n";
		}
		UnityEngine.Debug.Log (boardString);
		UnityEngine.Debug.Log (colourString);
		UnityEngine.Debug.Log ("########################### \n");
	}
	
	
	static int ColourCode(bool white) {
		return (white) ? 1 : 0;
	}
	
	
}