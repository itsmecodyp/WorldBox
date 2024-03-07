using System;
using SimpleGUI.Submods.SimpleGamba.LargeNumbers;
using UnityEngine;

namespace SimpleGUI.Submods.SimpleGamba {
	public class Mines {
		//need some auto-logic for mines based on x*y total?
		public int mineRowAmountX = 5;
		public int mineRowAmountY = 5;
		public int minesAmount => ((mineRowAmountX * mineRowAmountY) / 8); //3;
		
		public float mult = 1f;

		public int minesLeft;

		public bool hasBet;
		public bool isAbleToPlay;
		public double betAmount;
		public LargeNumber betAmountMines = new LargeNumber(0f);

		public bool[,] minesLayout;
		public bool[,] spacesClicked;

		public void MinesWindow(int windowID)
		{
			Color orig = GUI.backgroundColor;
			//show money
			GUILayout.Button("Money: " + Main.humanPlayer.money);
			//do bet
			GUILayout.BeginHorizontal();
			if (isAbleToPlay == false && GUILayout.Button("Bet"))
			{
				if (Main.humanPlayer.money > betAmount)
				{
					Main.humanPlayer.money -= betAmount;
					Main.SaveMoney();
					hasBet = true;
					isAbleToPlay = true;
					mult = 1f;
					minesLeft = minesAmount;
					//do mines setup stuff
					minesLayout = new bool[mineRowAmountX, mineRowAmountY];
					spacesClicked = new bool[mineRowAmountX, mineRowAmountY];
					for (int i = 0; i < mineRowAmountY; i++)
					{
						for (int j = 0; j < mineRowAmountX; j++)
						{
							minesLayout[j, i] = false;
							if (minesLeft > 0 && Toolbox.randomChance(0.125f))
							{
								minesLayout[j, i] = true;
								minesLeft--;
							}
						}
					}
				}
			}
			betAmount = (float)Convert.ToDouble(GUILayout.TextField(betAmount.ToString()));
			if (isAbleToPlay)
			{
				if (GUILayout.Button("Cashout with " + mult + "x"))
				{
					double amount = betAmount * mult;
					lastCashout = amount;
					if (amount > sessionHighest)
					{
						sessionHighest = amount;
					}
					Main.humanPlayer.money += amount;
					Main.SaveMoney();
					isAbleToPlay = false;
					
				}
			}
			GUILayout.EndHorizontal();
			//show mines
			if (minesLayout != null)
			{
				for (int i = 0; i < mineRowAmountY; i++)
				{
					GUILayout.BeginHorizontal();
					for (int j = 0; j < mineRowAmountX; j++)
					{
						//check if space is already clicked
						if (spacesClicked[j, i] == true)
						{
							if (minesLayout[j, i] == true)
							{
								GUI.backgroundColor = Color.red;
								GUILayout.Button("x");
								GUI.backgroundColor = orig;
							}
							GUI.backgroundColor = Color.green;
							GUILayout.Button("o");
							GUI.backgroundColor = orig;
						}
						else
						{
							if (GUILayout.Button(" ") && isAbleToPlay)
							{
								if (minesLayout[j, i] == true)
								{
									isAbleToPlay = false;
								}
								else
								{
									mult *= 1.25f; // more logic here?
								}
								spacesClicked[j, i] = true;
							}
						}
					}
					GUILayout.EndHorizontal();
				}
			}

			GUILayout.Button("Last win: " + new LargeNumber(lastCashout));
			GUILayout.Button("Session highest: " + new LargeNumber(sessionHighest));

			GUI.DragWindow();
		}

		public double lastCashout;
		public double sessionHighest;

	}
}
