using System.IO;
using System.Collections;
using System.Collections.Generic;
//using System;
using UnityEngine;

public static class OpeningBookReader {
	
	static Dictionary<ulong, List<ushort>> book;
	static bool initialized;
	
	public static void Init() {
		initialized = true;
		book = new Dictionary<ulong, List<ushort>> ();
		TextAsset bookFile = (TextAsset)Resources.Load ("Book", typeof(TextAsset));
		string[] lines = bookFile.text.Split ('\n');

		for (int lineIndex = 0; lineIndex < lines.Length; lineIndex ++) {
			string line = lines[lineIndex];
			string[] sections = line.Split(' ');
			
			if (sections.Length > 1) {
				ulong zobristKey = System.Convert.ToUInt64(sections[0]);
				List<ushort> moves = new List<ushort>();
				
				for (int i = 1; i < sections.Length; i ++) {
					moves.Add(System.Convert.ToUInt16(sections[i]));
				}
				
				if (!book.ContainsKey(zobristKey)) {
					book.Add(zobristKey, moves);
				}
			}
		}
	}
	
	public static List<ushort> GetBookMoves() {
		return book [Board.zobristKey];
	}
	
	public static bool IsInBook() {
		if (!initialized) {
			return false;
		}
		return book.ContainsKey(Board.zobristKey);
	}
	
}
