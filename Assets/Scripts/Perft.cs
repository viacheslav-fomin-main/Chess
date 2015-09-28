using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System;

public class Perft : MonoBehaviour {

	public bool usePerft;
	public bool printMoves;
	public int depth;
	public string fen;

		
	static int nodesSearched;
	string movesFound;

	Position position;

	public static IMoveGenerator moveGenerator;

	void Start() {
		if (usePerft) {
			moveGenerator = new MoveGenerator3();
		
			ChessUI.instance.SetPosition(fen);
			position = new Position();
			position.SetPositionFromFen(fen);

			Thread t = new Thread (SearchForBestMove);
			t.Start ();
		}
	}

		
	void SearchForBestMove() {
		nodesSearched = 0;

		System.Diagnostics.Stopwatch watch = new System.Diagnostics.Stopwatch ();
		watch.Start ();
		
		SearchNode origin = new SearchNode (position);
		AlphaBetaSearch (origin, depth, int.MinValue, int.MaxValue, position.gameState.whiteToMove);

		watch.Stop ();
		UnityEngine.Debug.Log (nodesSearched + " nodes searched in " + watch.ElapsedMilliseconds + " ms");
		if (printMoves) {
			UnityEngine.Debug.Log("Moves found: \n" + movesFound);
		}
	}



	int AlphaBetaSearch(SearchNode node, int depth, int alpha, int beta, bool isWhite) {


		movesFound += node.algebraic + "\n";
		if (depth == 0 || node.NoMoves()) {
			nodesSearched++;
			return node.Evaluate();
		}

		node.Init ();

		List<SearchNode> children = node.GetChildren ();
		int value = int.MinValue;
		
		if (isWhite) { // white is trying to attain the highest evaluation possible
			foreach (SearchNode child in children) {
				value = Math.Max (value, AlphaBetaSearch (child, depth - 1, alpha, beta, false));
				alpha = Math.Max (alpha, value);
			}
			return value;
		} else {
			// black is trying to obtain the lowest evaluation possible
			value = int.MaxValue;
			foreach (SearchNode child in children) {
				value = Math.Min (value, AlphaBetaSearch (child, depth - 1, alpha, beta, true));
				beta = Math.Min (beta, value);
			}
			return value;
		}
		return 0;
	}
		/*
		
	int AlphaBetaSearch(SearchNode node, int depth, int alpha, int beta, bool isWhite) {
		Debug.Log("R3 " + node.GetChildren().Count);
		node.Init ();
		if (printMoves) {
			Debug.Log("Move: " + node.move.algebraicMove);
		}
		//UnityEngine.Debug.Log ("searching: ");
		if (depth == 0 || node.NoMoves()) {
			nodesSearched ++;
			return node.Evaluate();
		}
		List<SearchNode> children = node.GetChildren ();
		int value = int.MinValue;
		
		if (isWhite) { // white is trying to attain the highest evaluation possible
			
			foreach (SearchNode child in children) {
				value = Mathf.Max(value,AlphaBetaSearch(child, depth-1, alpha, beta, false));
				alpha = Mathf.Max(alpha, value);
			}
			return value;
		}
		
		// black is trying to obtain the lowest evaluation possible
		value = int.MaxValue;
		foreach (SearchNode child in children) {
			value = Mathf.Min(value,AlphaBetaSearch(child, depth-1, alpha, beta, true));
			beta = Mathf.Min(beta, value);
		}
		
		return value;
	}
	*/
		
	public class SearchNode {
		
		List<SearchNode> children;
		Position currentPosition;
		public Move move { get; private set; } // The move the got to this position

		public string algebraic {
			get {
				if (move != null) {
					return move.algebraicMove;
				}
				return "";
			}
		}
		
		
		public SearchNode(Position p) {
			currentPosition = p;
		}
		
		public SearchNode(Position p, Move m) {
			currentPosition = p;
			move = m;
		}
		
		public void Init() {
			Move[] allMoves = moveGenerator.GetAllLegalMoves (currentPosition);
			// TODO: prune and sort moves
			children = new List<SearchNode> ();
			
			for (int i =0; i <allMoves.Length; i ++) {
				Position childPosition = currentPosition;
				childPosition.MakeMove(allMoves[i]);
				children.Add(new SearchNode(childPosition, allMoves[i]));
			}
		}
		
		public List<SearchNode> GetChildren() {
			return children;
		}
		
		public bool NoMoves() {
			return false;
		}
		
		public int Evaluate() {
			
			return 0;

		}
		
		
	}

}
