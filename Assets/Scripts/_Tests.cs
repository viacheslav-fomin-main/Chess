using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class _Tests : MonoBehaviour {
	

	void Start () {

		Heap heap = new Heap (10);

		heap.Add (1, -5);
		heap.Add (2, 1);
		heap.Add (3, 4);
		heap.Add (4, 10);
		heap.Add (5, 99);
		heap.Add (6, 123);
		int movec = heap.Count;
		for (int i = 0; i < movec; i ++) {
			//print(heap.evals[i]);
			print(heap.RemoveFirst());
		}

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
	

}
