using UnityEngine;
using System.Collections.Generic;
using System.Diagnostics;

public static class Timer {

	static Dictionary<string,Stopwatch> timers = new Dictionary<string, Stopwatch>();

	public static void Start(string name) {
		if (!timers.ContainsKey (name)) {
			timers.Add(name,new Stopwatch());
		}
		timers [name].Start ();
	}

	public static void Stop(string name) {
		if (timers.ContainsKey(name)) {
			timers[name].Stop();
		}
	}

	public static void Reset(string name) {
		if (timers.ContainsKey(name)) {
			timers[name].Reset();
		}
	}


	public static void Print(string name) {
		if (timers.ContainsKey(name)) {
			UnityEngine.Debug.Log(name + ": " + timers[name].ElapsedMilliseconds + " ms.");
		}
	}

}
