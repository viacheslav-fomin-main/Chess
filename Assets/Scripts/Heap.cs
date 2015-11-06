using UnityEngine;
using System.Collections;
using System;

/// <summary>
/// Heap for sorting moves. Move with the highest eval is kept at the top
/// </summary>
public class Heap {

	public ushort[] moves;
	short[] evals;
	ushort currentItemCount;
	
	public Heap(int maxHeapSize) {
		moves = new ushort[maxHeapSize];
		evals = new short[maxHeapSize];
	}
	
	public void Add(ushort move, short eval = 0) {
		moves[currentItemCount] = move;
		evals [currentItemCount] = eval;
		
		SortUp(currentItemCount);
		currentItemCount++;
	}

	public ushort GetMove(int index) {
		return moves [index];
	}

	public bool Contains(ushort move) {
		for (int i = 0; i < moves.Length; i ++) {
			if (moves[i] == move) {
				return true;
			}
		}
		return false;
	}
	
	public ushort RemoveFirst() {
		ushort firstItem = moves[0];
		currentItemCount--;
		moves[0] = moves[currentItemCount];
		evals [0] = evals [currentItemCount];
		
		SortDown(0);
		return firstItem;
	}

	public void UpdateEval(int index, short newEval) {
		short originalEval = evals [index];
		evals [index] = newEval;

		if (originalEval < newEval) {
			SortUp (index);
		} else if (originalEval > newEval) {
			SortDown(index);
		}

	}
	
	
	public int Count {
		get {
			return currentItemCount;
		}
	}
	
	
	void SortDown(int heapIndex) {
		while (true) {
			int childIndexLeft = heapIndex * 2 + 1;
			int childIndexRight = heapIndex * 2 + 2;
			int swapIndex = 0;
			
			if (childIndexLeft < currentItemCount) {
				swapIndex = childIndexLeft;
				
				if (childIndexRight < currentItemCount) {
					if (evals[childIndexRight] > (evals[childIndexLeft])) {
						swapIndex = childIndexRight;
					}
				}
				
				if (evals[heapIndex] < evals[swapIndex]) {
					Swap (heapIndex,swapIndex);
					heapIndex = swapIndex;
				}
				else {
					return;
				}
				
			}
			else {
				return;
			}
			
		}
	}
	
	void SortUp(int heapIndex) {
		int parentIndex = (heapIndex-1)/2;
		
		while (true) {
			if (evals[heapIndex] > evals[parentIndex]) {
				Swap (heapIndex,parentIndex);
			}
			else {
				break;
			}

			heapIndex = parentIndex;
			parentIndex = (heapIndex-1)/2;
		}
	}
	
	void Swap(int indexA, int indexB) {
		ushort tempMoveA = moves [indexA];
		moves [indexA] = moves [indexB];
		moves [indexB] = tempMoveA;
		
		short tempEvalA = evals [indexA];
		evals [indexA] = evals [indexB];
		evals [indexB] = tempEvalA;
	}
	
	
	
}

public interface IHeapItem<T> : IComparable<T> {
	int HeapIndex {
		get;
		set;
	}
}
