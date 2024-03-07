using SimplerGUI.Submods.SimpleGamba.LargeNumbers;
using UnityEngine;

namespace SimplerGUI.Submods.SimpleGamba {
	public class Crash {
		public bool hasBoughtIn;
		public bool hasRoundStarted;
		public bool hasCrashed = true;
		public float currentCrashValue = 1.0f;
		public float currentCrash = 1f;
		public float lastTimeAtRoundEnd;

		public LargeNumber currentBuyIn = new LargeNumber(1f);
		public LargeNumber lastWinAmount = new LargeNumber(1f);
		public float lastWinCrashAmount = 1f;
		public LargeNumber lastLossAmount = new LargeNumber(1f);
		public string betAmount;

		public void CrashWindow(int windowID)
		{
			//put currentCrash into largenumber only for display pretty-ing
			GUILayout.Button("Current crash: " + new LargeNumber(currentCrashValue));
			if(hasBoughtIn) {
				if(hasRoundStarted) {
					//user clicks payout button to.. payout
					//round continues after user is done
					if(GUILayout.Button("Payout: " + (new LargeNumber(currentBuyIn.Standard() * currentCrashValue)))) {
						double winAsDouble = currentBuyIn.Standard() * currentCrashValue;
						LargeNumber winAmount = new LargeNumber(winAsDouble);
						lastWinAmount = winAmount;
						lastWinCrashAmount = currentCrashValue;
						Debug.Log("Won " + lastWinAmount + "in crash");
						Main.humanPlayer.money += winAmount;
						Main.SaveMoney();
						if(winAmount > Main.crashLargestWin) {
							Main.crashLargestWinC.Value = winAmount.coefficient;
							Main.crashLargestWinM.Value = winAmount.magnitude;
							Main.crashLargestWinMult.Value = currentCrashValue;
							Debug.Log("New record win in crash! C/M: " + winAmount.coefficient + "/" + winAmount.magnitude);
						}
						hasBoughtIn = false;
					}
				}
			}

			if(!hasCrashed && hasRoundStarted) {
				currentCrashValue += 0.05f;
				if(currentCrashValue >= currentCrash) {
					//crash has happened, round has ended
					hasCrashed = true;
					hasRoundStarted = false;
					if(hasBoughtIn) {
						lastLossAmount = currentBuyIn;
						Debug.Log("Lost " + lastLossAmount + "to crash");
						currentBuyIn = new LargeNumber(0f);
						hasBoughtIn = false;
					}
					lastTimeAtRoundEnd = Time.realtimeSinceStartup;
				}
			}

			if(hasCrashed) {
				if(Time.realtimeSinceStartup > lastTimeAtRoundEnd + 5f) {
					// round is starting
					currentCrashValue = 1f;
					currentCrash = Random.Range(1f, 1001f);
					Debug.Log("Next crash happen on " + currentCrash);
					hasCrashed = false;
					hasRoundStarted = true;
				}
			}

			if(!hasBoughtIn) {
				if(GUILayout.Button("Bet") && !hasRoundStarted) {
					if(!double.TryParse(betAmount, out double bet)) {
						return;
					}
					currentBuyIn = new LargeNumber(bet);
					Main.humanPlayer.money -= currentBuyIn;
					Main.SaveMoney();
					hasBoughtIn = true;
				}
				betAmount = GUILayout.TextField(betAmount);
			}
			GUILayout.Button("Last win: " + lastWinAmount + " at " + lastWinCrashAmount +"x");
			GUILayout.Button("Last loss: " + lastLossAmount);
			if(GUILayout.Button("Record win: " + Main.crashLargestWin + " at " + Main.crashLargestWinMult.Value + "x")) {
				//reset if button is clicked
				Main.crashLargestWinM.Value = 0;
				Main.crashLargestWinC.Value = 0d;
				Main.crashLargestWinMult.Value = 0f;
			}

			if(GUILayout.Button("force crash")) {
				lastTimeAtRoundEnd = Time.realtimeSinceStartup;
				hasRoundStarted = false;
				hasCrashed = true;
			}
			GUI.DragWindow();
		}
	}
}
