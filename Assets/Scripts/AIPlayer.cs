using System.Collections;
using System;

public class AIPlayer : Player {

	Search search = new Search();

	public override void Init (bool white)
	{
		base.Init (white);
		search.OnNewMoveFound += OnNewMoveFound;
	}

	public override void RequestMove ()
	{
		base.RequestMove ();
		search.StartSearch (currentPosition);
		nextMoveTime = UnityEngine.Time.time + 5;
		started = true;
		//Move bestMove = search.SearchForBestMove (currentPosition);
		//MakeMove (bestMove);
		//Move[] moves = moveGenerator.GetAllLegalMoves (currentPosition);
		
		//Random random = new Random ();
		//MakeMove (moves [random.Next (0, moves.Length)]);
	}

	float nextMoveTime;
	bool started;

	public override void Update() {
		/*
		if (currentPosition.gameState.whiteToMove == false) {
			if (search.bestMoveSoFar != null) {
				UnityEngine.Debug.Log (search.bestMoveSoFar.algebraicMove);
			}
			if (search.returnReady || UnityEngine.Time.time > nextMoveTime) {
				if (search.returnReady) {
					UnityEngine.Debug.Log ("Move recieved AI Update");
				} else {
					UnityEngine.Debug.Log ("Forcing move");
				}

				search.returnReady = false;
				MakeMove (search.bestMoveSoFar);
			}
		}
		*/
		search.Update ();

		//if (search.bestMoveSoFar != null)
		//UnityEngine.Debug.Log (search.bestMoveSoFar.algebraicMove);

		//search.Update ();
		if (foundMove) {
			foundMove = false;
			MakeMove(m);
		}

	}

	bool foundMove;
	MoveOld m;

	public void OnNewMoveFound(MoveOld move) {
		//UnityEngine.Debug.Log ("Move recieved AI");
		foundMove = true;
		m = move;
		//MakeMove (move);
	}


}
