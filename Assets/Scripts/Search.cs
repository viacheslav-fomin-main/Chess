using System.Collections;
using System.Collections.Generic;
using System;
using System.Threading;

public class Search {

	public static IMoveGenerator moveGenerator;

	static int nodesSearched;
	static int breakCount;
	/*
	int searchDepth;
	int bestScoreSoFar;
	public volatile MoveOld bestMoveSoFar;
	public volatile bool returnReady;

	public event Action<MoveOld> OnNewMoveFound;

	//Thread t;
	//bool init;

	Position position;

	public void StartSearch(Position p) {
		position = p;
		//SearchForBestMove(position);
		returnReady = false;
		//ThreadStart starter = delegate {
			//SearchForBestMove (position);
		////};
		Thread t = new Thread (SearchForBestMove);
		t.Start ();
		//SearchForBestMove ();

		//OnNewMoveFound(new Move(new Coord("e2"),new Coord("e4"),new GameState()));
	}

	volatile int rem;

	public void Update() {
		//UnityEngine.Debug.Log ("Rec: " + rem);
	}



	void SearchForBestMove() {
		//while (true) {
		//	if (findMove) {

				int startTime = System.DateTime.Now.Second;
				int runTime = 10;

				//while (System.DateTime.Now.Second < startTime + runTime) {

				//}
				//UnityEngine.Debug.Log ("Thread start search: " + System.DateTime.Now.Hour + ":" + System.DateTime.Now.Minute + ":" + System.DateTime.Now.Second);
				nodesSearched = 0;
				breakCount = 0;
				moveGenerator = new MoveGenerator3 ();

				System.Diagnostics.Stopwatch watch = new System.Diagnostics.Stopwatch ();
				watch.Start ();

				SearchNode origin = new SearchNode (position);
				searchDepth = 1;
				bool isMaximising = position.gameState.whiteToMove;
				bestScoreSoFar = (isMaximising) ? int.MinValue : int.MaxValue;

				//ThreadStart starter = delegate {
				//	AlphaBetaSearch (origin, searchDepth, int.MinValue, int.MaxValue, position.gameState.whiteToMove);
				//	};

				//Thread t = new Thread (starter);
				//t.Start ();
				//UnityEngine.Debug.Log ("STARTED");
				AlphaBetaSearch (origin, searchDepth, int.MinValue, int.MaxValue, position.gameState.whiteToMove);
				//UnityEngine.Debug.Log ("FINISHED");
				watch.Stop ();
				UnityEngine.Debug.Log (nodesSearched + " nodes searched in " + watch.ElapsedMilliseconds + " ms; breakcount: " + breakCount);
				//UnityEngine.Debug.Log ("Pos move time: " + Position.sw.ElapsedMilliseconds + " ms");
				//	moveGenerator.PrintTimes ();
				//UnityEngine.Debug.Log ("Thread end search: " + System.DateTime.Now.Hour + ":" +System.DateTime.Now.Minute + ":" + System.DateTime.Now.Second );
				//return bestMoveSoFar;

				OnNewMoveFound (bestMoveSoFar);
		returnReady = true;
				//returnReady = true;
		//	}
			//UnityEngine.Debug.Log ("Thread: " + bestMoveSoFar.algebraicMove);

	}


	int AlphaBetaSearch(SearchNode node, int depth, int alpha, int beta, bool isWhite) {
		node.Init ();

		UnityEngine.Debug.Log ("searching: ");
		if (depth == 0 || node.NoMoves()) {

			return node.Evaluate();
		}
		List<SearchNode> children = node.GetChildren ();
		int value = int.MinValue;

		if (isWhite) { // white is trying to attain the highest evaluation possible

			foreach (SearchNode child in children) {
				value = Math.Max(value,AlphaBetaSearch(child, depth-1, alpha, beta, false));
				alpha = Math.Max(alpha, value);
				//UnityEngine.Debug.Log ("in loop");
				if(depth == searchDepth ) { // has searched full depth and is now looking at top layer of moves to select the best
					if (value>bestScoreSoFar) {
						bestScoreSoFar = value;
						bestMoveSoFar = child.move;
					}
				}

				if (beta <= alpha) { // break
					breakCount++;
					//break;
				}
			}
			return value;
		}

		// black is trying to obtain the lowest evaluation possible
		value = int.MaxValue;
		foreach (SearchNode child in children) {
			value = Math.Min(value,AlphaBetaSearch(child, depth-1, alpha, beta, true));
			beta = Math.Min(beta, value);
			//UnityEngine.Debug.Log ("in loop");
			if(depth == searchDepth ) { // has searched full depth and is now looking at top layer of moves to select the best
				if (value<bestScoreSoFar) {
					bestScoreSoFar = value;
					bestMoveSoFar = child.move;
				}
			}
			
			if (beta <= alpha) { // break
				breakCount++;
				//break;
			}
		}

		return value;
	}

	void Print(object s) {
		//UnityEngine.Debug.Log (s);
	}

	public class SearchNode {
		
		List<SearchNode> children;
		Position currentPosition;
		public MoveOld move { get; private set; } // The move the got to this position
	

		public SearchNode(Position p) {
			currentPosition = p;
		}
		
		public SearchNode(Position p, MoveOld m) {
			currentPosition = p;
			move = m;
		}

		public void Init() {
			Search.nodesSearched ++;
			MoveOld[] allMoves = moveGenerator.GetAllLegalMoves (currentPosition);
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

			//Random r = new Random ();
			//int eval = r.Next (-5000, 5000);

			//UnityEngine.Debug.Log ("Eval: " + eval);
			int eval = 0;
			for (int i = 0; i < 64; i ++) {
				if (currentPosition.allPiecesB.ContainsPieceAtSquare(i)) {
					eval -= new Coord(64-i).y;
				}
				if (currentPosition.allPiecesW.ContainsPieceAtSquare(i)) {
					eval += new Coord(63-i).y+1;
				}
			}
			//eval = UnityEngine.Random.Range (-5000, 5000);
			return eval;
		}
		
		
	}
*/
}

