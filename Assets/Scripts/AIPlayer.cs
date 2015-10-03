using System.Collections;
using System;

public class AIPlayer : Player {

	Search searcher;

	public override void Init (bool white)
	{
		base.Init (white);
		searcher = new Search ();
	}

	public override void RequestMove ()
	{
		base.RequestMove ();
		searcher.StartSearch ();
	}
	

	public override void Update() {
		if (searcher.finishedSearch && isMyMove) {
			MakeMove(searcher.bestMoveSoFar);
			UnityEngine.Debug.Log("Searcher: " + searcher.nodesSearched + " nodes searched; " + searcher.breakCount + " cutoffs.");
		}
	}



}
