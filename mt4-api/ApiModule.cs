using DataModels;
using System.Linq;
using System.Collections.Generic;
using System;
using System.IO;
using System.Threading;
using Nancy;
using System.Globalization;

namespace mt4_api
{
    public class ApiModule : Nancy.NancyModule
    {
        private static List<Mt4Users> mt4users;
        private static List<Mt4Trades> mt4trades;
        private static Dictionary<int, string> nicks = new Dictionary<int, string>();
        

        public static List<Trader> Traders { get; private set; }
        private DateTime beginEpoch = DateTime.Now.AddYears(-1);
        private DateTime begin = DateTime.ParseExact(
    "7 Nov 2017 12:00:00 UTC",
    "d MMM yyyy HH:mm:ss UTC",
    CultureInfo.InvariantCulture);
        private static Thread thread;

        public ApiModule()
        {
            After.AddItemToEndOfPipeline((ctx) => ctx.Response
            .WithHeader("Access-Control-Allow-Origin", "*"));

            Get("/", _ => "");
            Get("/GetTraders", _ =>
            {
                return Response.AsJson(new
                {
                    code = 200,
                    status = "success",
                    result = GetTraders()
                });
            });


            var nicksList = File.ReadAllLines("traders.txt")
                .ToList();
            for(var i = 0; i < nicksList.Count(); i++)
            {
                nicks[i + 1400060] = nicksList[i];
            }
            if(thread == null)
            {
                thread = new Thread(() =>
                {
                    while(true)
                    {
                        Update();
                        Thread.Sleep(30000);
                    }
                });
                thread.Start();
            }
        }
            
        public void Update()
        {
            var diff = DateTime.Now.AddHours(3) - begin;
            Console.WriteLine(diff);
            using (var db = new GcmtsrvDemoDB())
            {
                mt4users = db.Mt4Users.ToList();
                mt4trades = db.Mt4Trades.Where(tt => tt.CLOSETIME > begin).ToList();
            }
            var newTraders = new List<Trader>();
            foreach(var u in mt4users)
            {
                if (!nicks.ContainsKey(u.LOGIN))
                {
                    continue;
                }
                var trades = mt4trades
                    .Where(tr => tr.CLOSETIME > beginEpoch)
                    .Where(tr => tr.LOGIN == u.LOGIN).ToList();

                var deposit = trades.Sum(tr => tr.PROFIT) + trades.Sum(tr => tr.COMMISSION) + 5000;
                var trader = new Trader
                {
                    Id = u.LOGIN,
                    Avatar = "https://gv-api.azurewebsites.net/avatars/" + (u.LOGIN - 1400000) + ".png",
                    Country = "en",
                    Currency = "USD",
                    DaysLeft = 1,
                    Level = 1,
                    Weeks = 1,
                    Trades = trades.Where(tr => tr.CMD != 6).Count(),
                    Deposit = (int)deposit,
                    Name = nicks[u.LOGIN],
                    Fund = 0,
                    Profit = (int)(((deposit - 5000) / 5000.0) * 100),
                    ChartEntries = new List<int> ()
                };
                newTraders.Add(trader);
            }

            foreach (var trader in newTraders)
            {
                trader.ChartEntries.Add(0);
                var lastProfit = 0.0;
                Console.WriteLine(diff);
                for (var i = 0; i <= diff.TotalHours; i++)
                {
                    var fromTime = begin.AddHours(i);
                    var ToTime = fromTime.AddHours(1);
                    var tt = mt4trades
                        .Where(tr => tr.CLOSETIME > beginEpoch)
                        .Where(t => t.LOGIN == trader.Id)
                        .Where(tr => tr.CMD != 6)
                        .ToList();
                    if(trader.Id == 1400064)
                    {
                        var tttt = mt4trades
                        .Where(t => t.LOGIN == trader.Id)
                        .Where(tr => tr.CMD != 6)
                        .ToList();

                    }
                    var trdsQuery = 
                    mt4trades
                        .Where(t => t.LOGIN == trader.Id)
                        .Where(tr => tr.CMD != 6)
                        .Where(tr => tr.CLOSETIME > beginEpoch)
                        .Where(tr => tr.CLOSETIME >= fromTime && tr.CLOSETIME <= ToTime);

                    var ProfitInPeriod = trdsQuery.Sum(tr => tr.PROFIT) + trdsQuery.Sum(tr => tr.COMMISSION);

                    lastProfit += ProfitInPeriod;
                    trader.ChartEntries.Add((int)lastProfit);
                }
            }

            Traders = newTraders.Where(p => p.Trades > 0).OrderByDescending(tr => tr.Deposit).ToList();
        }

        private List<Trader> GetTraders()
        {
            return Traders;
        }
    }
}
