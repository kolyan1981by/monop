using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Text;
using System.Threading;

namespace GameLogic
{

	public class Game
	{

		#region props

		public Guid Id { get; set; }

		public List<string> Logs { get; set; }

		public List<ChestCard> CommunityChest { get; set; }
		public List<ChestCard> ChanceChest { get; set; }

		public ChestCard LastRandomCard { get; set; }

		public int PayAmount { get; set; }
		public int? PayToUserId { get; set; }

		public int RoundNumber { get; set; }

		public List<Player> Players { get; set; }

		public int pcount;

		public Player Winner { get; set; }

		public int Selected { get; set; }

		public DateTime StartRollTime { get; set; }

		public bool IsManualMode { get { return conf.cnfRollMode == 0; } }

		public bool RunningGame { get; set; }

		public Dictionary<string, string[]> Texts = new Dictionary<string, string[]>();

		public int CurrTimeOut
		{
			get
			{
				var now = DateTime.Now;
				return conf.cnfTimeOut - now.Subtract(StartRollTime).Seconds;
			}
		}

		public string ShowTimeOut
		{
			get
			{
				var now = DateTime.Now;
				return string.Format(HTML.TIMEOUT_TEXT, CurrTimeOut);
			}
		}

		public Player Curr
		{
			get
			{
				if (Selected < pcount)
				{
					return Players[Selected];
				}
				else return pcount == 0 ? new Player { Name = "Finish" } : Players[0];
			}
		}

		public bool IsCurrPlayer(string userName)
		{
			var pl = Players.SingleOrDefault(x => x.Name == userName);
			if (pl != null)
			{
				return pl.Id == Curr.Id;
			}
			else { return false; }
		}

		public CellInf CurrCell
		{
			get
			{
				return Cells[Curr.Pos];
			}
		}

		public bool IsGameCreator(string uname)
		{
			return Players.Count > 0 && Players[0].Name == uname;
		}

		public int[] LastRoll { get; set; }

		public CellInf[] Cells { get; set; }

		public GameConfig conf { get; set; }

		public string GameName { get { return string.Join(",", Players.Select(x => x.Name)); } }


		#endregion

		public MapManager Map;


		public Game()
		{
			Id = Guid.NewGuid();
			Players = new List<Player>();
			Logs = new List<string>();
			ToBeginRound();
			conf = new GameConfig();

			//init map cells
			this.Map = new MapManager(this);
			this.Map.InitMap();

		}



		Timer LifeTimer;

		public void StartGame()
		{
			if (!conf.DebugMode)
				LifeTimer = new Timer(GameManager.LifeTimerJob, this, 0, conf.LifeTimerPeriod);
			StartRollTime = DateTime.Now;
			RunningGame = true;
			pcount = Players.Count();

		}

		public void StopTimer()
		{
			LifeTimer.Change(Timeout.Infinite, 0);
			Tlog("LifeTimerJob.FinishGame", "FinishGame", "FinishGame");
			//RunningGame = true;

		}

		#region Trades

		Dictionary<int, List<TradeRule>> PlayersTradeRules = new Dictionary<int, List<TradeRule>>();

		public List<AucRule> AucRules { get; set; }


		public void SetPlayerTradeRules(List<TradeRule> rules, int pid)
		{
			PlayersTradeRules[pid] = rules;
		}

		public List<TradeRule> GetTradeRules(int pid)
		{
			List<TradeRule> rules;
			if (PlayersTradeRules.TryGetValue(pid, out rules))
			{
				return rules;
			}
			else return PlayersTradeRules[-1];
		}

		public List<Trade> RejectedTrades = new List<Trade>();

		public List<Trade> CompletedTrades = new List<Trade>();

		#endregion


		#region logging



		public void Tlog(string key, string ru_text, string en_text, params object[] args)
		{
			var text = Text(key, ru_text, en_text);

			if (conf.DebugMode)
				log(string.Format("[{0}] {1}", key, text), args);
			else
				log(text, args);
		}

		public void Tlogp(string key, string ru_text, string en_text, params object[] args)
		{
			var text = Text(key, ru_text, en_text);

			if (conf.DebugMode)
				logp(string.Format("[{0}] {1}", key, text), args);
			else
				logp(text, args);

		}

		void logp(string format, params object[] args)
		{
			var rec = string.Format("{0}:@p{1}[{2}:{3}]:", RoundNumber, Curr.Id, LastRoll[0], LastRoll[1])
				+ string.Format(format, args);
			if (conf.EnableLog)
				Logs.Add(ReplaceIdWithName(rec));
		}

		void log(string format, params object[] args)
		{
			var rec = RoundNumber + ":" + string.Format(format, args);
			if (conf.EnableLog)
				Logs.Add(ReplaceIdWithName(rec));
		}

		private string ReplaceIdWithName(string x)
		{
			for (int i = 0; i < 4; i++)
			{
				if (x.Contains("@p" + i)) x = x.Replace("@p" + i, this.GetPlayer(i).Name);
			}
			return x;
		}

		public string Text(string key, string ru_text, string en_text)
		{
			//this.TextResources[key + "ru-RU"] = ru_text;
			//this.TextResources[key + "en-US"] = en_text;

			//"en-US", "ru-RU"
			if (conf.cnfGameLang == "ru-RU")
				return ru_text;
			return en_text;

		}

		public string[] LogInfo
		{
			get
			{

				return Logs.AsEnumerable().Reverse().Take(15).ToArray();
			}
		}



		#endregion


		#region States

		public GameState State { get; set; }

		public void ToBeginRound()
		{
			SetState(GameState.BeginStep);
		}

		public void FinishStep(string act = "")
		{
			if (act != "")
			{
				this.FixAction(act);
			}
			SetState(GameState.EndStep);
			//FinishRound();
		}
		public void ToCanBuy()
		{
			SetState(GameState.CanBuy);
			if (Curr.IsBot) PlayerAction.Buy(this);
				
		}

		public void ToAuction()
		{
			SetState(GameState.Auction);
			GameManager.InitAuction(this);

			//if (Curr.IsBot) {  }
			//else
			//{
			//    Tlogp("PlayerAction.NoMoneyWhenBuy", "недостаточно денег для {0}", "not enough money to buy {0}", CurrCell.Name);
			//    SetState(GameState.Auction);
			//}
		}


		public void ToCantPay()
		{
			SetState(GameState.CantPay);
		}

		public void ToPay(int amount, bool needFinish = true)
		{
			PayAmount = amount;
			ToPay(needFinish);
		}

		public void ToPay(bool needFinish = true)
		{
			SetState(GameState.NeedPay);
			if (Curr.IsBot)PlayerAction.Pay(this, needFinish);
			
		}

		public void ToTrade()
		{
			SetState(GameState.Trade);
		}

		public void MoveToCell()
		{
			if (Curr.IsBot)
			{
				PlayerStep.MoveToCell(this);
				FinishStep();
			}
			else
				SetState(GameState.MoveToCell);
		}
		public void ToRandomCell()
		{
			if (Curr.IsBot) FinishStep();
			else
				SetState(GameState.RandomCell);
		}
		public void SetState(GameState gameState)
		{

			if (!IsFinished) State = gameState;
		}

		public void CleanTimeOut()
		{
			StartRollTime = DateTime.Now;
		}

		#endregion


		#region Player

		public bool IsFinished
		{
			get
			{
				return (State == GameState.FinishGame);
			}

		}

		public void FinishRound()
		{
			if (conf.IsExtendedLog)
				Tlogp("Game.FinishRound", "FinishRound pos={0}", "FinishRound pos={0}", Curr.Pos);

			var p = Curr;
			if (this.GetPlayerCash(p.Id) < 0)
			{
				GameManager.LeaveGame(this, p.Name);
			}

			//start 
			RoundNumber++;
			ToBeginRound();

			if (IsManualMode) Players.ForEach(x => x.ManRoll = 0);

			var goNext = false;

			if (Selected < pcount)
			{
				if (LastRoll != null)
				{
					var police = (Curr.Police > 0 && Curr.Police < 3);

					if ((LastRoll[0] != LastRoll[1]) || police)
					{
						goNext = true;
					}

				}
				else
					goNext = true;

				if (goNext) Selected = (Selected + 1) % pcount;
			}
			else
			{
				Selected = 0;
			}

			//need check double roll
			//p.EnableDoubleRoll = false;
			CleanTimeOut();
		}

		public IEnumerable<Player> Bots
		{
			get
			{
				return Players.Where(x => x.IsBot);
			}
		}


		//leave game


		#endregion

		public Trade CurrTrade = new Trade();

		public AuctionState currAuction;

		#region Leave Game handler

		Action<string, bool> LeaveGameAction;

		public void SetLeaveGameAction(Action<string, bool> act)
		{
			LeaveGameAction += act;
		}

		public void OnLeave(string uname, bool b)
		{
			//if (OnLeavedGame != null) OnLeavedGame(uname, b);
			if (LeaveGameAction != null) LeaveGameAction(uname, b);
		}

		#endregion

		public List<GameAction> RoundActions = new List<GameAction>();

		public void FixAction(string act)
		{
			var gameAction = new GameAction();

			gameAction.round = this.RoundNumber;
			gameAction.curr = this.Curr.Id;
			gameAction.cpos = this.Curr.Pos;
			gameAction.croll = this.LastRoll;
			gameAction.action = act;
			gameAction.Players = (
				from x in this.Players
				select (Player)x.Clone()).ToList<Player>();
			gameAction.Cells = (
				from x in this.Cells
				select (CellInf)x.Clone()).ToArray<CellInf>();
			gameAction.RandomCard = ((act == "random") ? this.LastRandomCard : null);

			RoundActions.Add(gameAction);
			//this.AddToLog(act);
		}
	}

	public class GameAction
	{
		public int round;
		public int curr;
		public int cpos;
		public int[] croll;
		public List<Player> Players;
		public CellInf[] Cells;
		public ChestCard RandomCard;
		public string action;
	}


}