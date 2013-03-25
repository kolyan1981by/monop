using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GameLogic
{
    public class CellInf : ICloneable
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int? Owner { get; set; }

        public int Cost { get; set; }

        public string ArendaInfo { get; set; }
        public int HousesCount { get; set; }
        public bool IsMortgage { get; set; }

        public int Type { get; set; }
        public int Group { get; set; }
        public int OwGrCount { get; set; }

        public int NeedPay(int index)
        {
            if (ArendaInfo == null || index < 0) return 0;
            return int.Parse(ArendaInfo.Split(';')[index]) * 1000;
        }

        public bool IsLand
        {
            get
            {
                return Type == 1 || Type == 2 || Type == 3;
            }
        }
        public bool IsMonopoly
        {
            get
            {
                if (Owner == null) return false;
                if (Group >= 2 && Group <= 7) return OwGrCount == 3;
                if (Group == 1 || Group == 8) return OwGrCount == 2;
                //if (Group == 11) return OwGrCount == 4;
                //if (Group == 33) return OwGrCount == 2;
                return false;
            }
        }
        public int Rent
        {
            get
            {
                //tax cells
                if (Id == 38) return 1000000;
                if (Id == 4) return 2000000;

                //trans cells - type2
                if (Group == 11) return NeedPay(OwGrCount - 1);

                //power cells - type3
                if (Group == 33) return OwGrCount == 2 ? 100000 : NeedPay(0);



                //cells - type1
                if (IsMonopoly && HousesCount == 0) return NeedPay(0) * 2;
                else return NeedPay(HousesCount);

                return NeedPay(0);
            }

        }

        public string PrintTextOnCell
        {
            get
            {
                if (Type == 1 || Type == 2 || Type == 3)
                {
                    if (Owner != null && IsMortgage)
                        return "MORTG";
                    else
                    {
                        var text = Rent > 1000000 ? Rent / 1000000d + "M" : Rent / 1000d + "K";
                        return string.Format("{0} <br/>{1}", text, PrintHouses);
                    }

                }
                if (Type == 0 || Type == 6) return Name;

                return "";

            }
        }
        public string PrintHouses
        {
            get
            {
                var count = HousesCount;

                if (Group == 11)
                {
                    count = OwGrCount == 0 ? 0 : HousesCount + 1;
                   
                }
                if (count == 5) return "H";

                return new String('*', count);
            }
        }

        public int HouseCost
        {
            get
            {
                switch (Group)
                {
                    case 1:
                    case 2:
                        return 500000;

                    case 3:
                    case 4:

                        return 1000000;

                    case 5:
                    case 6:

                        return 1500000;

                    case 7:
                    case 8:

                        return 2000000;

                    default:
                        return 0;

                }
            }
        }
        public int HouseCostWhenSell
        {
            get
            {
                return HouseCost / 2;
            }
        }
        public int MortgageAmount
        {
            get
            {
                return Cost / 2;
            }
        }
        public int UnMortgageAmount
        {
            get
            {
                return (int)(Cost / 2 * 1.1);
            }
        }

        public object Clone()
        {
            return base.MemberwiseClone();
        }
    }
}
