using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class _Tests : MonoBehaviour {

	public string pgn;

	void Start () {

	//	StartCoroutine ("A");

		//GetComponent<PlayerManager>().re
	//	ZobristKey.Init ();
		//Board.SetPositionFromFen (Definitions.startFen,false);


		/*
		int moveFromIndex = 4;
		int moveToIndex = 19;
		int moveFromX = Board.Convert128to64(moveFromIndex) % 8;
		int moveFromY = Board.Convert128to64(moveFromIndex) / 8;
		int moveToX = Board.Convert128to64(moveToIndex) % 8;
		int moveToY = Board.Convert128to64(moveToIndex) / 8;
		
		
		string fromAlgebraic = Definitions.fileNames[moveFromX].ToString() + Definitions.rankNames[moveFromY].ToString();
		string toAlgebraic = Definitions.fileNames[moveToX].ToString() + Definitions.rankNames[moveToY].ToString();
		print ("Move: " + fromAlgebraic + toAlgebraic);
*/
	/*
		Position p = new Position ();
		p.SetPositionFromFen (Definitions.startFen);
		int[] j =  p.AllPieces(false).GetIndices();
		print ("hamming: " +p.AllPieces (false).ActiveBitCount () + "  " + p.AllPieces(false).board);
		for (int i =0; i < j.Length; i ++) {
			print ("S: " + j[i]);
		}

*/
		/*
		Position p = new Position ();
		p.SetPositionFromFen (Definitions.startFen);
		MoveGenerator moveG = new MoveGenerator ();
		Move[] moves = moveG.GetAllLegalMoves (p);

		for (int i = 0; i < moves.Length; i ++) {
			string moveS = moves[i].from.algebraic + moves[i].to.algebraic;
			print (i + ". " + moveS);
		}
		*/
		/*
		long a = System.GC.GetTotalMemory (true);
		print (a + " ");
		MoveGenerator moveG = new MoveGenerator ();
		List<short> rs = new List<short> (13000);
		List<ulong> rs2 = new List<ulong> (13000);
		moveG.GetAllLegalMoves (new Position());
		long b = System.GC.GetTotalMemory (true);
		print (b + " ");

		print ((b-a) + " ");
		*/



	}

	public static void ZobristCheck() {
		ulong z1 = ZobristKey.GetZobristKey();
		
		if (z1 != Board.zobristKey) {
			Debug.Log ("Conflict: " + z1 + "\n" + "board v: " + Board.zobristKey);
			Debug.Break();
		}
	}
	/*
	IEnumerator A() {
		yield return new WaitForSeconds (.5f);
		List<string> moveStrings = PGNReader.MoveStringsFromPGN (pgn);
		List<ushort> moves = PGNReader.MovesFromMoveStrings (moveStrings);
		
		print ("move count: " + moves.Count);
		print ("string count: " + moveStrings.Count);
		Board.SetPositionFromFen (Definitions.startFen);

		for (int i =0; i < moves.Count; i ++) {
			//Board.MakeMove(moves[i],true);
			print(moveStrings[i] + ":  " + moves[i]);
			yield return new WaitForSeconds(.3f);
		}

	}
*/
	void Update() {

		
	

		/*
		if (Input.GetKeyDown (KeyCode.Space)) {

			ulong z1 = ZobristKey.GetZobristKey();

			if (z1 == Board.zobristKey) {
				print ("Match: " + z1);
			}
			else {
				print ("Conflict: " + z1 + "\n" + "board v: " + Board.zobristKey);
			}
		}
		*/
	}
	

}
