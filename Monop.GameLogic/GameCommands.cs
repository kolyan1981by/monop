using GameLogic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GameLogic
{
	//format: commname;player;id
	//set-pos
	//set-money;pl;amount
	//set-policeround
	//set-gamestate
	//make-trade;p0-21-23;p1-16-18
	//set-cellowner;pl;cell_Id
	public class GameCommands
	{

		public static string[] List()
		{
			string[] comms = { 
								 "set-pos;0;pos", 
								 "set-money;0;amount", 
								 "set-policeround", 
								 "set-gamestate;BeginStep", 
								 "make-trade;p0-21-23;p1-16-18", 
								 "set-cellowner;0;11", 
							 };
			return comms;
		}

		public static bool Run(Game g, string comm)
		{
			var arr = comm.Split(';');
			if (comm.StartsWith("set-pos"))
			{
				g.Players[int.Parse(arr[1])].Pos = int.Parse(arr[2]);
			}
			if (comm.StartsWith("set-money"))
			{
				g.Players[int.Parse(arr[1])].Money = int.Parse(arr[2]);
			}
			if (comm.StartsWith("set-cellowner"))
			{
				var cell = g.Cells[int.Parse(arr[2])];
				cell.Owner = int.Parse(arr[1]);
				g.Map.UpdateMap();
			}
			if (comm.StartsWith("set-gamestate"))
			{
				g.State = GameState.BeginStep;
			}
			if (comm.StartsWith("make-trade"))
			{
				var p0 = arr[1].Split('-');
				int from = int.Parse(p0[0].Replace("p", ""));

				GameManager.InitTrade(g, arr[1] + ";" + arr[2], from);
				GameManager.MakeTrade(g);
			}
			return true;
		}
	}
}
