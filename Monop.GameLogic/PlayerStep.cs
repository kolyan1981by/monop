using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GameLogic
{

    public class PlayerStep
    {

        public static void MakeStep(Game g, int roll = 3)
        {
            //g.logp("rolls {0} {1}", g.LastRoll);
            if (g.State != GameState.BeginStep) return;

            var p = g.Curr;

            if (p.IsBot && BotBrain.BeforeRollMakeBotAction(g)) return;

            g.CleanTimeOut();

            GameManager.MakeRoll(g);

            bool CanGo = true;


            if (p.Pos == 10 && p.Police > 0)
            {
                CanGo = CanOutFromPolice(g);
                if (p.Police == 4) return;
            }

            if (CanGo)
            {
                var NotTrippleRoll = g.Curr.Step();

                if (NotTrippleRoll)
                {
                    ProcessPosition(g);
                }
                else
                {
                    g.Tlogp("step.TripleRoll", "вы выкинули тройной дубль, вас задержала милиция", "you roll triple, go to POLICE");
                    g.FinishStep();
                }
            }
            else
            {
                g.FinishStep();
            }


        }

        public static bool CanOutFromPolice(Game g)
        {
            var p = g.Curr;
            bool rollDouble = p.LastRoll[0] == p.LastRoll[1];

            if (rollDouble) return true;

            if (p.IsBot && BotBrain.ShouldGoFromPolice(g))
            {
                g.Tlogp("step.PoliceOut", "заплатил 500 k$, чтобы выйти", "you paid 500 k$ to out");

                PlayerAction.Pay(g, 500000);

                return true;
            }
            else
            {
                p.Police++;
                if (p.Police == 4)
                {
                    g.Tlogp("ProcessPolice.PoliceOut", "заплатите 500 k$, чтобы выйти", "you need pay 500 k$ to out");
                    g.AmountOfPay = 500000;
                    g.ToPay(false);
                    return true;
                }
                else
                {
                    g.Tlogp("ProcessPolice.PoliceCatch", "вы можете заплатить и выйти ", "you can pay 500 k$ and out");
                    return false;
                }

            }
            return false;
        }

        public static void ProcessPosition(Game g)
        {
            var p = g.Curr;

            //general land
            var cell = g.Cells[p.Pos];

            if (cell.IsLand)
            {
                ProcessLand(g, p, cell);

            }

            //interpol
            if (p.Pos == 30)
            {
                PlayerStep.MoveFrom30(g);
                //g.MoveToCell();

            }

            // tax
            if (cell.Type == 6)
            {
                //g.logp(g.Text("ProcessPosition.PayTax", "заплатите налог", "you need pay tax "));
                g.AmountOfPay = cell.Rent;
                g.ToPay();
            }

            //var offPos = new[] { 2, 7, 17, 22, 33, 36 };

            if (cell.Type == 4)
            {
                g.Map.TakeRandomCard();
                g.FixAction("random");
                ProcessRandom(g, p);

            }
            else if (p.Pos == 20 || p.Pos == 0 || (p.Pos == 10 && p.Police == 0))
                g.FinishStep("cell_" + p.Pos);

        }

        public static void ProcessLand(Game g, Player p, CellInf cell)
        {
            if (cell.Owner == null)
            {
                g.ToCanBuy();

            }
            else if (cell.Owner != p.Id)
            {
                if (cell.IsMortgage)
                {
                    g.Tlogp("ProcessLand.CellIsMortgaged", "земля {0} заложена", "land {0} is mortgaged ", cell.Name);
                    g.FinishStep("cell_mortgaged");
                }
                else
                {
                    //pay rent
                    g.AmountOfPay = cell.Rent;
                    var toPlayer = g.GetPlayer(cell.Owner.Value);
                    g.ToPay(to: toPlayer);

                }

            }
            else if (cell.Owner == p.Id)
            {
                g.Tlogp("ProcessLand.MyCell", "ваше поле {0}", "your cell {0} ", cell.Name);
                g.FinishStep("mycell");
            }
        }

        public static void ProcessRandom(Game g, Player p)
        {
            var c = g.LastRandomCard;
            //get money
            if (c.RandomGroup == 1)
            {
                p.Money += c.Money;
                g.Tlogp("ProcessRandom.Random1.GetMoney", "получите {0}", "get money = {0}", c.Money.PrintMoney());

                g.ToRandomCell();
            }

            //go to cell
            if (c.RandomGroup == 2 || c.RandomGroup == 3)
            {
                g.MoveToCell();
            }

            //pay each player
            if (c.RandomGroup == 4)
            {
                g.Tlogp("ProcessRandom.PayEachPlayer", "заплатите каждому игроку 500K", "pay each player 500K");
                g.AmountOfPay = c.Money * (g.pcount - 1);
                g.Players.Where(x => x.Id != p.Id).ToList().ForEach(x => x.Money += c.Money);

                g.ToPay();

            }
            //key to out from police
            if (c.RandomGroup == 5)
            {
                p.FreePoliceKey++;
                g.ToRandomCell();
            }
            if (c.RandomGroup == -1)
            {
                g.Tlogp("ProcessRandom.PayBank", "заплатите банку", "pay to bank");
                g.AmountOfPay = c.Money;
                g.ToPay();

            }
            //pay for each house and hotel
            if (c.RandomGroup == 15)
            {
                g.Tlogp("ProcessRandom.FixHouses",
                   "Отремонтируйте ваши здания – $100K за дом, $400K за отель",
                   "You are assessed for street repairs – $100K per house, $400K per hotel");

                var hh = g.Map.GetHotelsAndHousesCount(p.Id);
                g.AmountOfPay = hh[0] * 400000 + hh[1] * 100000;
                g.ToPay();

            }
        }

        public static void MoveToCell(Game g)
        {

            var c = g.LastRandomCard;
            var p = g.Curr;

            if (c.RandomGroup == 2 || c.RandomGroup == 3)
            {
                PlayerStep.MoveAfterRandom(g);
            }
        }

        public static void MoveFrom30(Game g)
        {
            var p = g.Curr;

            if (p.Pos == 30)
            {
                p.Pos = 10;
                p.Police = 1;
                g.Tlogp("ProcessPosition.Police30", "вас задержал интерпол", "go to POLICE");
                g.FinishStep();
            }
        }

        public static void MoveAfterRandom(Game g)
        {
            var c = g.LastRandomCard;
            var p = g.Curr;

            if (c.RandomGroup == 2 && c.Pos == 10)
            {
                p.Pos = 10;
                p.Police = 1;
                g.Tlogp("MoveAfterRandom.GoToPolice", "вы попали в тюрьму", "go To Police");
                g.FinishStep();
            }
            else
            {
                if (c.RandomGroup == 3)
                {
                    g.Tlogp("MoveAfterRandom.Go3Back", "на три хода назад", "go 3 step back");
                    if (p.Pos > 3) p.Pos -= 3;

                }
                else
                {
                    g.Tlogp("MoveAfterRandom.goToCell", "вам нужно на клетку {0}", "go To Cell {0}", c.Pos);

                    if (p.Pos > c.Pos)
                    {
                        p.Money += 2000000;
                        g.Tlogp("MoveAfterRandom.GoPass", "вы прошли через старт и получили 2M$", "get 2M$ ");
                    }
                    p.Pos = c.Pos;

                }
                PlayerStep.ProcessPosition(g);
            }
        }
    }

}