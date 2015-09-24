using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/*
 * Engine communication protocol:
 * Initial position set from fen string
 * Move format: (start square, end square) + (promotion piece type)
 * 
 * Examples:
 * e2e4 (no extra notation denoting capture, check, etc)
 * e1g1 (castling)
 * e7e8Q (pawn promotion)
 * e5f6 (pawn capture - en passant remains the same)
 * 
 */

public class ChessUI : MonoBehaviour {

	public Color lightColour;
	public Color darkColour;

	public Transform lightSquare;
	public Transform darkSquare;

	public Sprite blackRook, blackKnight, blackBishop, blackQueen, blackKing, blackPawn;
	public Sprite whiteRook, whiteKnight, whiteBishop, whiteQueen, whiteKing, whitePawn;
	public Sprite none;

	// board (0,0) = a1; (7,7) = h8
	char[,] board;
	Dictionary<char,Sprite> spriteDictionary;
	SpriteRenderer[,] pieceSquares;
	Renderer[,] squares;

	static ChessUI myInstance;

	string fileNames = "abcdefgh";
	string rankNames = "12345678";

	// Editor vars:
	[HideInInspector]
	public string editorFen;
	[HideInInspector]
	public string editorMove;

	void Awake() {
		CreateBoardUI ();
		SetPosition (editorFen);
	}

	/// <summary>
	/// Gets the instance.
	/// </summary>
	public static ChessUI instance {
		get {
			if (myInstance == null) {
				myInstance = FindObjectOfType<ChessUI> ();
			}
			return myInstance;
		}
	}

	/// <summary>
	/// Set the board UI from fen string
	/// </summary>
	public void SetPosition (string fen) {
		board = new char[8,8];

		// read fen into board array
		string pieceChars = "rnbqkpRNBQKP";
		string pieceFen = fen.Split (' ') [0];
		int boardX = 0;
		int boardY = 7;

		for (int i = 0; i < pieceFen.Length; i ++) {
			char key = pieceFen[i];
			
			if (pieceChars.Contains(key.ToString())) {
				board[boardX,boardY] = key;
				boardX ++;
			}
			else if (key == '/') {
				boardX = 0;
				boardY --;
			}
			else {
				int skipCount;
				if (int.TryParse(key + "", out skipCount)) {
					for (int skipIndex = 0; skipIndex < skipCount; skipIndex ++) {
						board[boardX,boardY] = ' ';
						boardX ++;
					}
				}
			}
		}

		UpdateBoardUI ();

	}

	/// <summary>
	/// Highlight the given square (algebraic coordinate)
	/// </summary>
	public void HighlightSquare(string squaresToHighlight) {
		int squareX = fileNames.IndexOf (squaresToHighlight [0]);
		int squareY = rankNames.IndexOf (squaresToHighlight [1]);

		squares [squareX, squareY].material.color = Color.green;
	}

	public void ResetHighlights() {
		PaintBoard (lightColour, darkColour);
	}

	/// <summary>
	/// Makes move given in algebraic notation (see comments at top of ChessUI class for details)
	/// Note: all moves are assumed legal and proving non-legal input may result in errors/unexpected behaviour
	/// </summary>
	public void MakeMove(string move) {

		int fromX = fileNames.IndexOf (move [0]);
		int fromY = rankNames.IndexOf (move [1]);
		int toX = fileNames.IndexOf (move [2]);
		int toY = rankNames.IndexOf (move [3]);

		// detect en passant
		if (char.ToUpper (board [fromX, fromY]) == 'P') {
			if (toX != fromX) { // is capture
				if (board[toX, toY] == ' ') { // nothing to capture on this square, thus capture must be en passant
					int pawnMoveDir = (int)Mathf.Sign(toY - fromY);
					board[toX, toY - pawnMoveDir] = ' '; // remove captured piece from board
				}
			}
		}

		// detect castling:
		if (char.ToUpper (board [fromX, fromY]) == 'K' && Mathf.Abs (fromX - toX) > 1) {
			int castleDir = (int)Mathf.Sign(toX-fromX);
			int rookFromX = (castleDir == 1)?7:0;
			int rookToX = (castleDir == 1)?5:3;

			// move rook
			board[rookToX,fromY] = board[rookFromX, fromY];
			board[rookFromX, fromY] = ' ';
		}

		// Make move on board
		board [toX, toY] = board [fromX, fromY];
		board [fromX, fromY] = ' ';

		// detect promotion:
		// (this is done after making move on board so as to prevent overwriting of newly promoted piece)
		if (move.Length > 4) {
			board[toX, toY] = move[4]; // promote piece
		}


		UpdateBoardUI ();
	}

	/// <summary>
	/// Updates the board UI to match the board array
	/// </summary>
	void UpdateBoardUI() {
		for (int y = 0; y < 8; y ++) {
			for (int x = 0; x < 8; x ++) {
				pieceSquares[x,y].sprite = spriteDictionary[board[x,y]];
			}
		}
	}

	public void CreateBoardUI () {
		// Create new holder objects for easy organisation/deletion of board UI elements
		string holderName = "UI Holder";

		if (transform.FindChild (holderName)) {
			GameObject holderOld = transform.FindChild(holderName).gameObject;
			DestroyImmediate(holderOld);
		}

		Transform uiHolder = new GameObject (holderName).transform;
		uiHolder.parent = transform;

		Transform boardHolder = new GameObject ("Board Holder").transform;
		boardHolder.parent = uiHolder;

		Transform pieceHolder = new GameObject ("Piece Holder").transform;
		pieceHolder.parent = uiHolder;


		// Generate board and piece squares
		squares = new Renderer[8,8];
		pieceSquares = new SpriteRenderer[8,8];


		for (int y = 0; y < 8; y ++) {
			for (int x = 0; x < 8; x ++) {
				Vector2 position = new Vector2(-4.5f + x+1, -4.5f + y+1);
				string algebraicCoordinate = fileNames[x].ToString() + rankNames[y].ToString();
				bool isLightSquare = ((y+x)%2) != 0;

				// squares
				Transform newSquare = Instantiate((isLightSquare)?lightSquare:darkSquare, position, Quaternion.identity) as Transform;
				newSquare.parent = boardHolder;
				newSquare.name =  algebraicCoordinate;
				squares[x,y] = newSquare.GetComponent<Renderer>();

				// pieces
				PieceUI pieceUI = new GameObject(algebraicCoordinate).AddComponent<PieceUI>();
				pieceUI.Init(algebraicCoordinate, position);
				SpriteRenderer sprite = pieceUI.gameObject.AddComponent<SpriteRenderer>();
				sprite.transform.position = position;
				sprite.transform.parent = pieceHolder;
				pieceSquares[x,y] = sprite;
			}
		}
		PaintBoard (lightColour, darkColour);
		InitializeSpriteDictionary ();
	}

	void PaintBoard(Color light, Color dark) {
		for (int y = 0; y < 8; y ++) {
			for (int x = 0; x < 8; x ++) {
				bool isLightSquare = ((x+y)%2) == 0;
				squares[x,y].sharedMaterial.color = (isLightSquare)?light:dark;
			}
		}
	}

	void InitializeSpriteDictionary() {
		spriteDictionary = new Dictionary<char, Sprite> ();
		spriteDictionary.Add (' ', none);

		spriteDictionary.Add ('r', blackRook);
		spriteDictionary.Add ('n', blackKnight);
		spriteDictionary.Add ('b', blackBishop);
		spriteDictionary.Add ('q', blackQueen);
		spriteDictionary.Add ('k', blackKing);
		spriteDictionary.Add ('p', blackPawn);

		spriteDictionary.Add ('R', whiteRook);
		spriteDictionary.Add ('N', whiteKnight);
		spriteDictionary.Add ('B', whiteBishop);
		spriteDictionary.Add ('Q', whiteQueen);
		spriteDictionary.Add ('K', whiteKing);
		spriteDictionary.Add ('P', whitePawn);
	}


}
