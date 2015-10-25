using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class Clock : MonoBehaviour {

	PlayerManager playerManager;

	public float clockTimeSeconds;
	public float incrementSeconds;
	
	float secondsRemainingWhite;
	float secondsRemainingBlack;

	public Text clockUIWhite;
	public Text clockUIBlack;

	void Start () {
		playerManager = GetComponent<PlayerManager> ();
		
		playerManager.OnMoveMade += OnMoveMade;
		secondsRemainingWhite = clockTimeSeconds;
		secondsRemainingBlack = clockTimeSeconds;
	}

	void Update() {
		if (playerManager.whiteToPlay) {
			secondsRemainingWhite -= Time.deltaTime;
		} else {
			secondsRemainingBlack -= Time.deltaTime;
	
		}

		int clockMinutesRemainingWhite =  (int)(secondsRemainingWhite/60);
		int clockSecondsRemainingWhite = (int)(secondsRemainingWhite - clockMinutesRemainingWhite * 60);
		clockUIWhite.text = string.Format("{0:0}:{1:00}",clockMinutesRemainingWhite, clockSecondsRemainingWhite);

		int clockMinutesRemainingBlack = (int)(secondsRemainingBlack/60);
		int clockDecondsRemainingBlack = (int)(secondsRemainingBlack  - clockMinutesRemainingBlack * 60);
		clockUIBlack.text = string.Format("{0:0}:{1:00}",clockMinutesRemainingBlack, clockDecondsRemainingBlack);
	}
	
	void OnMoveMade(bool moverIsWhite) {
		if (moverIsWhite) {
			secondsRemainingWhite += incrementSeconds;
		} else {
			secondsRemainingBlack += incrementSeconds;
		}
	}
}
