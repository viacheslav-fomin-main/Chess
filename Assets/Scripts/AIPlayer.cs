using System.Collections;
using System;

public class AIPlayer : Player {

	Search searcher;
	bool moveRequested;

	public override void Init (bool white)
	{
		base.Init (white);
		searcher = new Search ();
	}

	public override void RequestMove ()
	{

		base.RequestMove ();
		moveRequested = true;
		searcher.StartSearch ();
	}
	

	public override void Update() {
		if (searcher.finishedSearch && isMyMove && moveRequested) {
			MakeMove(searcher.bestMoveSoFar);
			moveRequested = false;
		}
	}



}
