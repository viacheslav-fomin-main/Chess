using System;

public interface IMoveGenerator {
//	MoveOld[] GetAllLegalMoves(Position position);

	void PrintTimes();
}

public interface ISearch {
//	event Action<MoveOld> OnNewMoveFound;
	//event Action TestEvent;
	//void StartSearch(Position position);
	void Update();
}