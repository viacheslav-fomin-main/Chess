using System;

public interface IMoveGenerator {
	Move[] GetAllLegalMoves(Position position);

	void PrintTimes();
}

public interface ISearch {
	event Action<Move> OnNewMoveFound;
	//event Action TestEvent;
	void StartSearch(Position position);
	void Update();
}