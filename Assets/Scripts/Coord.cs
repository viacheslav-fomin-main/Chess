
/*
 * Represents a coordinate of the chessboard
 * 0,0 = a1; 7,7 = h8
 */

public struct Coord {

	public int x;
	public int y;

	public Coord(int xIndex, int yIndex) {
		x = xIndex;
		y = yIndex;
	}

	public Coord(int index) {
		y = index / 8;
		x = index - y * 8;
	}

	public Coord(string algebraic) {
		x = Definitions.fileNames.IndexOf (algebraic [0]);
		y = Definitions.rankNames.IndexOf (algebraic [1]);
	}

	/// Does coord lie within dimensions of board
	public bool inBoard {
		get {
			return x >= 0 && x <8 && y >= 0 && y < 8;
		}
	}

	/// Flattens x;y coordinates into index with 0 reprenting a1, and 63 representing h8
	public int flatIndex {
		get {
			return y*8 + x;
		}
	}

	public string algebraic {
		get {
			return Definitions.fileNames[x].ToString() + Definitions.rankNames[y].ToString();
		}
	}

	public static Coord operator +(Coord a, Coord b) {
		return new Coord(a.x + b.x, a.y + b.y);
	}

	public static bool operator ==(Coord a, Coord b) {
		return (a.x == b.x && a.y == b.y);
	}

	public static bool operator !=(Coord a, Coord b) {
		return !(a == b);
	}

	// corners:
	public static Coord bottomLeft { 
		get {
			return new Coord(0,0);
		}
	}

	public static Coord topLeft { 
		get {
			return new Coord(0,7);
		}
	}

	public static Coord bottomRight { 
		get {
			return new Coord(0,7);
		}
	}

	public static Coord topRight { 
		get {
			return new Coord(7,7);
		}
	}

}