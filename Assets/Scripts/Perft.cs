using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System;

public class Perft : MonoBehaviour {

	public bool usePerft;
	public bool printMoves;
	public int searchDepth;
	public string fen;

	int nodeSearchCount;
	MoveGenerator moveGenerator;


	void Start() {
		if (usePerft) {
			moveGenerator = new MoveGenerator();
			Board.SetPositionFromFen(fen);
			Timer.Start("Perft");
			PerfTest(searchDepth);
			Timer.Stop("Perft");
			Timer.Print("Perft");
			print("Leaf nodes at depth " + searchDepth + ": " + nodeSearchCount);

		}
	}

	void PerfTest(int depth) {
		if (depth == 0) {
			nodeSearchCount ++;
			return;
		}
		List<ushort> moves = moveGenerator.GetMoves (false, false);
		for (int i =0; i < moves.Count; i ++) {
			Board.MakeMove(moves[i]);
			PerfTest(depth-1);
			Board.UnmakeMove(moves[i]);
		}
	}


}
