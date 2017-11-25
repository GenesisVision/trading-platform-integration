using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mt4_api
{
    public class Trader
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Country { get; set; }
        public string Currency { get; set; }
        public List<int> ChartEntries { get; set; }
        public int Deposit { get; set; }
        public int Trades { get; set; }
        public int Weeks { get; set; }
        public int Profit { get; set; }
        public int Level { get; set; }
        public string Avatar { get; set; }
        public int DaysLeft { get; set; }
        public int Fund { get; set; }
    }
}
