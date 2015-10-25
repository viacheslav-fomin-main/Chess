using System.IO;
using System.Collections;
using System.Collections.Generic;
using System;

public static class OpeningBookReader {

	static Dictionary<ulong, List<ushort>> book;

	public static void Init() {
		book = new Dictionary<ulong, List<ushort>> ();
		StreamReader reader = new StreamReader ("Assets/Opening Book/Openings.txt");
	
		while (reader.Peek() != -1) {
			string line = reader.ReadLine();
			string[] sections = line.Split(' ');

			if (sections.Length > 0) {
				ulong zobristKey = Convert.ToUInt64(sections[0]);
				List<ushort> moves = new List<ushort>();

				for (int i = 1; i < sections.Length; i ++) {
					moves.Add(Convert.ToUInt16(sections[i]));
				}

				if (!book.ContainsKey(zobristKey)) {
					book.Add(zobristKey, moves);
				}
			}
		}
		reader.Close ();
	}

	public static List<ushort> GetBookMoves() {
		return book [Board.zobristKey];
	}

	public static bool IsInBook() {
		return book.ContainsKey(Board.zobristKey);
	}

}
