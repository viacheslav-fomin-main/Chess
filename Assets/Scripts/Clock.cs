using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class Clock : MonoBehaviour {

	MoveManager playerManager;

	public float clockTimeSeconds;
	public float incrementSeconds;
	
	float secondsRemainingWhite;
	float secondsRemainingBlack;

	public Text clockUIWhite;
	public Text clockUIBlack;
	bool stopClock = true;

	void Start () {
		playerManager = GetComponent<MoveManager> ();
		
		playerManager.OnMoveMade += OnMoveMade;
		secondsRemainingWhite = clockTimeSeconds;
		secondsRemainingBlack = clockTimeSeconds;
	}

	public void StartClock() {
		stopClock = false;
	}

	void Update() {
		if (stopClock) {
			return;
		}

		if (playerManager.whiteToPlay) {
			secondsRemainingWhite -= Time.deltaTime;
			secondsRemainingWhite = Mathf.Clamp(secondsRemainingWhite,0,int.MaxValue);
		} else {
			secondsRemainingBlack -= Time.deltaTime;
			secondsRemainingBlack = Mathf.Clamp(secondsRemainingBlack,0,int.MaxValue);
	
		}

		int clockMinutesRemainingWhite =  (int)(secondsRemainingWhite/60);
		int clockSecondsRemainingWhite = (int)(secondsRemainingWhite - clockMinutesRemainingWhite * 60);
		clockUIWhite.text = string.Format("{0:0}:{1:00}",clockMinutesRemainingWhite, clockSecondsRemainingWhite);

		int clockMinutesRemainingBlack = (int)(secondsRemainingBlack/60);
		int clockDecondsRemainingBlack = (int)(secondsRemainingBlack  - clockMinutesRemainingBlack * 60);
		clockUIBlack.text = string.Format("{0:0}:{1:00}",clockMinutesRemainingBlack, clockDecondsRemainingBlack);

		if (secondsRemainingBlack == 0) {
			stopClock = true;
			FindObjectOfType<MoveManager>().TimeOut(false);
		} else if (secondsRemainingWhite == 0) {
			stopClock = true;
			FindObjectOfType<MoveManager>().TimeOut(true);
		}
	}
	
	void OnMoveMade(bool moverIsWhite, ushort move) {
		if (moverIsWhite) {
			secondsRemainingWhite += incrementSeconds;
		} else {
			secondsRemainingBlack += incrementSeconds;
		}
	}
}
