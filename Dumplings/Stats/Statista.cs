﻿using Dumplings.Helpers;
using Dumplings.Rpc;
using Dumplings.Scanning;
using NBitcoin;
using NBitcoin.RPC;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Dumplings.Stats
{
    public class Statista
    {
        public Statista(ScannerFiles scannerFiles, RPCClient rpc)
        {
            ScannerFiles = scannerFiles;
            Rpc = rpc;
        }

        public ScannerFiles ScannerFiles { get; }
        public RPCClient Rpc { get; }

        public void CalculateMonthlyVolumes()
        {
            using (BenchmarkLogger.Measure())
            {
                Dictionary<YearMonth, Money> otheri = CalculateMonthlyVolumes(ScannerFiles.OtherCoinJoins);
                Dictionary<YearMonth, Money> wasabi = CalculateMonthlyVolumes(ScannerFiles.WasabiCoinJoins);
                Dictionary<YearMonth, Money> samuri = CalculateMonthlyVolumes(ScannerFiles.SamouraiCoinJoins);
                DisplayOtheriWasabiSamuriResults(otheri, wasabi, samuri);
            }
        }

        public void CalculateMonthlyEqualVolumes()
        {
            using (BenchmarkLogger.Measure())
            {
                Dictionary<YearMonth, Money> otheri = CalculateMonthlyEqualVolumes(ScannerFiles.OtherCoinJoins);
                Dictionary<YearMonth, Money> wasabi = CalculateMonthlyEqualVolumes(ScannerFiles.WasabiCoinJoins);
                Dictionary<YearMonth, Money> samuri = CalculateMonthlyEqualVolumes(ScannerFiles.SamouraiCoinJoins);
                DisplayOtheriWasabiSamuriResults(otheri, wasabi, samuri);
            }
        }

        public void CalculateNeverMixed()
        {
            using (BenchmarkLogger.Measure())
            {
                Dictionary<YearMonth, Money> otheri = CalculateNeverMixed(ScannerFiles.OtherCoinJoins);
                Dictionary<YearMonth, Money> wasabi = CalculateNeverMixed(ScannerFiles.WasabiCoinJoins);
                Dictionary<YearMonth, Money> samuri = CalculateNeverMixedFromTx0s(ScannerFiles.SamouraiCoinJoins, ScannerFiles.SamouraiTx0s);
                DisplayOtheriWasabiSamuriResults(otheri, wasabi, samuri);
            }
        }

        public void CalculateEquality()
        {
            using (BenchmarkLogger.Measure())
            {
                Dictionary<YearMonth, ulong> otheri = CalculateEquality(ScannerFiles.OtherCoinJoins);
                Dictionary<YearMonth, ulong> wasabi = CalculateEquality(ScannerFiles.WasabiCoinJoins);
                Dictionary<YearMonth, ulong> samuri = CalculateEquality(ScannerFiles.SamouraiCoinJoins);
                DisplayOtheriWasabiSamuriResults(otheri, wasabi, samuri);
            }
        }

        public void CalculatePostMixConsolidation()
        {
            using (BenchmarkLogger.Measure())
            {
                Dictionary<YearMonth, decimal> otheri = CalculateAveragePostMixInputs(ScannerFiles.OtherCoinJoinPostMixTxs);
                Dictionary<YearMonth, decimal> wasabi = CalculateAveragePostMixInputs(ScannerFiles.WasabiPostMixTxs);
                Dictionary<YearMonth, decimal> samuri = CalculateAveragePostMixInputs(ScannerFiles.SamouraiPostMixTxs);
                DisplayOtheriWasabiSamuriResults(otheri, wasabi, samuri);
            }
        }

        public void CalculateSmallerThanMinimumWasabiInputs()
        {
            using (BenchmarkLogger.Measure())
            {
                Dictionary<YearMonth, decimal> wasabi = CalculateSmallerThanMinimumWasabiInputs(ScannerFiles.WasabiCoinJoins);
                DisplayWasabiResults(wasabi);
            }
        }

        public void CalculateIncome()
        {
            using (BenchmarkLogger.Measure())
            {
                Dictionary<YearMonth, Money> wasabi = CalculateWasabiIncome(ScannerFiles.WasabiCoinJoins);
                Dictionary<YearMonth, Money> samuri = CalculateSamuriIncome(ScannerFiles.SamouraiTx0s);
                DisplayWasabiSamuriResults(wasabi, samuri);
            }
        }

        public void CalculateFreshBitcoins()
        {
            using (BenchmarkLogger.Measure())
            {
                Dictionary<YearMonth, Money> otheri = CalculateFreshBitcoins(ScannerFiles.OtherCoinJoins);
                Dictionary<YearMonth, Money> wasabi = CalculateFreshBitcoins(ScannerFiles.WasabiCoinJoins);
                Dictionary<YearMonth, Money> samuri = CalculateFreshBitcoinsFromTX0s(ScannerFiles.SamouraiTx0s, ScannerFiles.SamouraiCoinJoinHashes);
                DisplayOtheriWasabiSamuriResults(otheri, wasabi, samuri);
            }
        }

        private void DisplayOtheriWasabiSamuriResults(Dictionary<YearMonth, Money> otheriResults, Dictionary<YearMonth, Money> wasabiResults, Dictionary<YearMonth, Money> samuriResults)
        {
            Console.WriteLine($"Month;Otheri;Wasabi;Samuri");

            foreach (var yearMonth in wasabiResults
                .Keys
                .Concat(otheriResults.Keys)
                .Concat(samuriResults.Keys)
                .Distinct()
                .OrderBy(x => x.Year)
                .ThenBy(x => x.Month))
            {
                if (!otheriResults.TryGetValue(yearMonth, out Money otheri))
                {
                    otheri = Money.Zero;
                }
                if (!wasabiResults.TryGetValue(yearMonth, out Money wasabi))
                {
                    wasabi = Money.Zero;
                }
                if (!samuriResults.TryGetValue(yearMonth, out Money samuri))
                {
                    samuri = Money.Zero;
                }

                Console.WriteLine($"{yearMonth};{otheri.ToDecimal(MoneyUnit.BTC):0};{wasabi.ToDecimal(MoneyUnit.BTC):0};{samuri.ToDecimal(MoneyUnit.BTC):0}");
            }
        }

        private void DisplayWasabiResults(Dictionary<YearMonth, decimal> wasabiResults)
        {
            Console.WriteLine($"Month;Wasabi");

            foreach (var yearMonth in wasabiResults
                .Keys
                .Distinct()
                .OrderBy(x => x.Year)
                .ThenBy(x => x.Month))
            {
                if (!wasabiResults.TryGetValue(yearMonth, out decimal wasabi))
                {
                    wasabi = 0;
                }

                Console.WriteLine($"{yearMonth};{wasabi:0.00}");
            }
        }

        private void DisplayWasabiSamuriResults(Dictionary<YearMonth, Money> wasabiResults, Dictionary<YearMonth, Money> samuriResults)
        {
            Console.WriteLine($"Month;Wasabi;Samuri");

            foreach (var yearMonth in wasabiResults
                .Keys
                .Concat(samuriResults.Keys)
                .Distinct()
                .OrderBy(x => x.Year)
                .ThenBy(x => x.Month))
            {
                if (!wasabiResults.TryGetValue(yearMonth, out Money wasabi))
                {
                    wasabi = Money.Zero;
                }
                if (!samuriResults.TryGetValue(yearMonth, out Money samuri))
                {
                    samuri = Money.Zero;
                }

                Console.WriteLine($"{yearMonth};{wasabi.ToDecimal(MoneyUnit.BTC):0.00};{samuri.ToDecimal(MoneyUnit.BTC):0.00}");
            }
        }

        private void DisplayOtheriWasabiSamuriResults(Dictionary<YearMonth, ulong> otheriResults, Dictionary<YearMonth, ulong> wasabiResults, Dictionary<YearMonth, ulong> samuriResults)
        {
            Console.WriteLine($"Month;Otheri;Wasabi;Samuri");

            foreach (var yearMonth in wasabiResults
                .Keys
                .Concat(otheriResults.Keys)
                .Concat(samuriResults.Keys)
                .Distinct()
                .OrderBy(x => x.Year)
                .ThenBy(x => x.Month))
            {
                if (!otheriResults.TryGetValue(yearMonth, out ulong otheri))
                {
                    otheri = 0;
                }
                if (!wasabiResults.TryGetValue(yearMonth, out ulong wasabi))
                {
                    wasabi = 0;
                }
                if (!samuriResults.TryGetValue(yearMonth, out ulong samuri))
                {
                    samuri = 0;
                }

                Console.WriteLine($"{yearMonth};{otheri:0};{wasabi:0};{samuri:0}");
            }
        }

        private void DisplayOtheriWasabiSamuriResults(Dictionary<YearMonth, decimal> otheriResults, Dictionary<YearMonth, decimal> wasabiResults, Dictionary<YearMonth, decimal> samuriResults)
        {
            Console.WriteLine($"Month;Otheri;Wasabi;Samuri");

            foreach (var yearMonth in wasabiResults
                .Keys
                .Concat(otheriResults.Keys)
                .Concat(samuriResults.Keys)
                .Distinct()
                .OrderBy(x => x.Year)
                .ThenBy(x => x.Month))
            {
                if (!otheriResults.TryGetValue(yearMonth, out decimal otheri))
                {
                    otheri = 0;
                }
                if (!wasabiResults.TryGetValue(yearMonth, out decimal wasabi))
                {
                    wasabi = 0;
                }
                if (!samuriResults.TryGetValue(yearMonth, out decimal samuri))
                {
                    samuri = 0;
                }

                Console.WriteLine($"{yearMonth};{otheri:0.0};{wasabi:0.0};{samuri:0.0}");
            }
        }

        private Dictionary<YearMonth, Money> CalculateMonthlyVolumes(IEnumerable<VerboseTransactionInfo> txs)
        {
            var myDic = new Dictionary<YearMonth, Money>();

            foreach (var tx in txs)
            {
                var blockTime = tx.BlockInfo.BlockTime;
                if (blockTime.HasValue)
                {
                    var blockTimeValue = blockTime.Value;
                    var yearMonth = new YearMonth(blockTimeValue.Year, blockTimeValue.Month);
                    var sum = tx.Outputs.Sum(x => x.Value);
                    if (myDic.TryGetValue(yearMonth, out Money current))
                    {
                        myDic[yearMonth] = current + sum;
                    }
                    else
                    {
                        myDic.Add(yearMonth, sum);
                    }
                }
            }

            return myDic;
        }

        private Dictionary<YearMonth, Money> CalculateMonthlyEqualVolumes(IEnumerable<VerboseTransactionInfo> txs)
        {
            var myDic = new Dictionary<YearMonth, Money>();

            foreach (var tx in txs)
            {
                var blockTime = tx.BlockInfo.BlockTime;
                if (blockTime.HasValue)
                {
                    var blockTimeValue = blockTime.Value;
                    var yearMonth = new YearMonth(blockTimeValue.Year, blockTimeValue.Month);
                    var sum = tx.GetIndistinguishableOutputs(includeSingle: false).Sum(x => x.value * x.count);
                    if (myDic.TryGetValue(yearMonth, out Money current))
                    {
                        myDic[yearMonth] = current + sum;
                    }
                    else
                    {
                        myDic.Add(yearMonth, sum);
                    }
                }
            }

            return myDic;
        }

        private Dictionary<YearMonth, Money> CalculateFreshBitcoins(IEnumerable<VerboseTransactionInfo> txs)
        {
            var myDic = new Dictionary<YearMonth, Money>();
            var txHashes = txs.Select(x => x.Id).ToHashSet();

            foreach (var tx in txs)
            {
                var blockTime = tx.BlockInfo.BlockTime;
                if (blockTime.HasValue)
                {
                    var blockTimeValue = blockTime.Value;
                    var yearMonth = new YearMonth(blockTimeValue.Year, blockTimeValue.Month);

                    var sum = Money.Zero;
                    foreach (var input in tx.Inputs.Where(x => !txHashes.Contains(x.OutPoint.Hash)))
                    {
                        sum += input.PrevOutput.Value;
                    }

                    if (myDic.TryGetValue(yearMonth, out Money current))
                    {
                        myDic[yearMonth] = current + sum;
                    }
                    else
                    {
                        myDic.Add(yearMonth, sum);
                    }
                }
            }

            return myDic;
        }

        private Dictionary<YearMonth, Money> CalculateFreshBitcoinsFromTX0s(IEnumerable<VerboseTransactionInfo> tx0s, IEnumerable<uint256> cjHashes)
        {
            var myDic = new Dictionary<YearMonth, Money>();
            // In Samourai in order to identify fresh bitcoins the tx0 input shouldn't come from other samuri coinjoins, nor tx0s.
            var txHashes = tx0s.Select(x => x.Id).Union(cjHashes).ToHashSet();

            foreach (var tx in tx0s)
            {
                var blockTime = tx.BlockInfo.BlockTime;
                if (blockTime.HasValue)
                {
                    var blockTimeValue = blockTime.Value;
                    var yearMonth = new YearMonth(blockTimeValue.Year, blockTimeValue.Month);

                    var sum = Money.Zero;
                    foreach (var input in tx.Inputs.Where(x => !txHashes.Contains(x.OutPoint.Hash)))
                    {
                        sum += input.PrevOutput.Value;
                    }

                    if (myDic.TryGetValue(yearMonth, out Money current))
                    {
                        myDic[yearMonth] = current + sum;
                    }
                    else
                    {
                        myDic.Add(yearMonth, sum);
                    }
                }
            }

            return myDic;
        }

        private Dictionary<YearMonth, Money> CalculateNeverMixed(IEnumerable<VerboseTransactionInfo> coinJoins)
        {
            // Go through all the coinjoins.
            // If a change output is spent and didn't go to coinjoins, then it didn't get remixed.
            var coinJoinInputs =
               coinJoins
                   .SelectMany(x => x.Inputs)
                   .Select(x => x.OutPoint)
                   .ToHashSet();

            var myDic = new Dictionary<YearMonth, Money>();
            VerboseTransactionInfo[] coinJoinsArray = coinJoins.ToArray();
            for (int i = 0; i < coinJoinsArray.Length; i++)
            {
                var reportProgress = ((i + 1) % 100) == 0;
                if (reportProgress)
                {
                    Logger.LogInfo($"{i + 1}/{coinJoinsArray.Length}");
                }
                VerboseTransactionInfo tx = coinJoinsArray[i];
                var blockTime = tx.BlockInfo.BlockTime;
                if (blockTime.HasValue)
                {
                    var blockTimeValue = blockTime.Value;
                    var yearMonth = new YearMonth(blockTimeValue.Year, blockTimeValue.Month);

                    var sum = Money.Zero;
                    var changeOutputValues = tx.GetIndistinguishableOutputs(includeSingle: true).Where(x => x.count == 1).Select(x => x.value).ToHashSet();
                    VerboseOutputInfo[] outputArray = tx.Outputs.ToArray();
                    for (int j = 0; j < outputArray.Length; j++)
                    {
                        var output = outputArray[j];
                        // If it's a change and it didn't get remixed right away.
                        OutPoint outPoint = new OutPoint(tx.Id, j);
                        if (changeOutputValues.Contains(output.Value) && !coinJoinInputs.Contains(outPoint) && Rpc.GetTxOut(outPoint.Hash, (int)outPoint.N, includeMempool: false) is null)
                        {
                            sum += output.Value;
                        }
                    }

                    if (myDic.TryGetValue(yearMonth, out Money current))
                    {
                        myDic[yearMonth] = current + sum;
                    }
                    else
                    {
                        myDic.Add(yearMonth, sum);
                    }
                }
            }

            return myDic;
        }

        private Dictionary<YearMonth, Money> CalculateNeverMixedFromTx0s(IEnumerable<VerboseTransactionInfo> samuriCjs, IEnumerable<VerboseTransactionInfo> samuriTx0s)
        {
            // Go through all the outputs of TX0 transactions.
            // If an output is spent and didn't go to coinjoins or other TX0s, then it didn't get remixed.
            var samuriTx0CjInputs =
                samuriCjs
                    .SelectMany(x => x.Inputs)
                    .Select(x => x.OutPoint)
                    .Union(
                        samuriTx0s
                            .SelectMany(x => x.Inputs)
                            .Select(x => x.OutPoint))
                            .ToHashSet();

            var myDic = new Dictionary<YearMonth, Money>();
            VerboseTransactionInfo[] samuriTx0sArray = samuriTx0s.ToArray();
            for (int i = 0; i < samuriTx0sArray.Length; i++)
            {
                var reportProgress = ((i + 1) % 100) == 0;
                if (reportProgress)
                {
                    Logger.LogInfo($"{i + 1}/{samuriTx0sArray.Length}");
                }
                VerboseTransactionInfo tx = samuriTx0sArray[i];
                var blockTime = tx.BlockInfo.BlockTime;
                if (blockTime.HasValue)
                {
                    var blockTimeValue = blockTime.Value;
                    var yearMonth = new YearMonth(blockTimeValue.Year, blockTimeValue.Month);

                    var sum = Money.Zero;
                    VerboseOutputInfo[] outputArray = tx.Outputs.ToArray();
                    for (int j = 0; j < outputArray.Length; j++)
                    {
                        var output = outputArray[j];
                        OutPoint outPoint = new OutPoint(tx.Id, j);
                        if (!samuriTx0CjInputs.Contains(outPoint) && Rpc.GetTxOut(outPoint.Hash, (int)outPoint.N, includeMempool: false) is null)
                        {
                            sum += output.Value;
                        }
                    }

                    if (myDic.TryGetValue(yearMonth, out Money current))
                    {
                        myDic[yearMonth] = current + sum;
                    }
                    else
                    {
                        myDic.Add(yearMonth, sum);
                    }
                }
            }

            return myDic;
        }

        private Dictionary<YearMonth, ulong> CalculateEquality(IEnumerable<VerboseTransactionInfo> coinJoins)
        {
            // CoinJoin Equality metric shows how much equality is gained for bitcoins. It is calculated separately to inputs and outputs and the results are added together.
            // For example if 3 people mix 10 bitcoins only on the output side, then CoinJoin Equality will be 3^2 * 10.

            var myDic = new Dictionary<YearMonth, ulong>();
            foreach (var tx in coinJoins)
            {
                var blockTime = tx.BlockInfo.BlockTime;
                if (blockTime.HasValue)
                {
                    var blockTimeValue = blockTime.Value;
                    var yearMonth = new YearMonth(blockTimeValue.Year, blockTimeValue.Month);

                    var equality = tx.CalculateCoinJoinEquality();

                    if (myDic.TryGetValue(yearMonth, out ulong current))
                    {
                        myDic[yearMonth] = current + equality;
                    }
                    else
                    {
                        myDic.Add(yearMonth, equality);
                    }
                }
            }

            return myDic;
        }

        private Dictionary<YearMonth, decimal> CalculateAveragePostMixInputs(IEnumerable<VerboseTransactionInfo> postMixes)
        {
            var myDic = new Dictionary<YearMonth, (int totalTxs, int totalIns, decimal avg)>();
            foreach (var tx in postMixes)
            {
                var blockTime = tx.BlockInfo.BlockTime;
                if (blockTime.HasValue)
                {
                    var blockTimeValue = blockTime.Value;
                    var yearMonth = new YearMonth(blockTimeValue.Year, blockTimeValue.Month);

                    int ttxs = 1;
                    int tins = tx.Inputs.Count();
                    decimal average = (decimal)tins / ttxs;

                    if (myDic.TryGetValue(yearMonth, out (int totalTxs, int totalIns, decimal) current))
                    {
                        ttxs = current.totalTxs + 1;
                        tins = current.totalIns + tins;
                        average = (decimal)tins / ttxs;
                        myDic[yearMonth] = (ttxs, tins, average);
                    }
                    else
                    {
                        myDic.Add(yearMonth, (ttxs, tins, average));
                    }
                }
            }

            var retDic = new Dictionary<YearMonth, decimal>();
            foreach(var kv in myDic)
            {
                retDic.Add(kv.Key, kv.Value.avg);
            }
            return retDic;
        }

        private Dictionary<YearMonth, decimal> CalculateSmallerThanMinimumWasabiInputs(IEnumerable<VerboseTransactionInfo> postMixes)
        {
            var myDic = new Dictionary<YearMonth, (int totalInputs, int totalSmallerInputs)>();
            foreach (var tx in postMixes)
            {
                var blockTime = tx.BlockInfo.BlockTime;
                if (blockTime.HasValue)
                {
                    var blockTimeValue = blockTime.Value;
                    var yearMonth = new YearMonth(blockTimeValue.Year, blockTimeValue.Month);

                    var (value, count) = tx.GetIndistinguishableOutputs(includeSingle: false).OrderByDescending(x => x.count).First();                   

                    int tic = tx.Inputs.Count();
                    int sic = tic - count;

                    if (myDic.TryGetValue(yearMonth, out (int tins, int tsins) current))
                    {
                        tic += current.tins;
                        sic += current.tsins;
                        myDic[yearMonth] = (tic, sic);
                    }
                    else
                    {
                        myDic.Add(yearMonth, (tic, sic));
                    }
                }
            }

            var retDic = new Dictionary<YearMonth, decimal>();
            foreach (var kv in myDic)
            {
                var perc = (decimal)kv.Value.totalSmallerInputs / kv.Value.totalInputs;
                retDic.Add(kv.Key, perc);
            }
            return retDic;
        }

        private Dictionary<YearMonth, Money> CalculateWasabiIncome(IEnumerable<VerboseTransactionInfo> coinJoins)
        {
            var myDic = new Dictionary<YearMonth, Money>();
            foreach (var tx in coinJoins)
            {
                var blockTime = tx.BlockInfo.BlockTime;
                if (blockTime.HasValue)
                {
                    var blockTimeValue = blockTime.Value;
                    var yearMonth = new YearMonth(blockTimeValue.Year, blockTimeValue.Month);

                    var sum = Money.Zero;
                    foreach (var output in tx.Outputs.Where(x => Constants.WasabiCoordScripts.Contains(x.ScriptPubKey)))
                    {
                        sum += output.Value;
                    }

                    if (myDic.TryGetValue(yearMonth, out Money current))
                    {
                        myDic[yearMonth] = current + sum;
                    }
                    else
                    {
                        myDic.Add(yearMonth, sum);
                    }
                }
            }

            return myDic;
        }

        private Dictionary<YearMonth, Money> CalculateSamuriIncome(IEnumerable<VerboseTransactionInfo> tx0s)
        {
            var myDic = new Dictionary<YearMonth, Money>();
            foreach (var tx in tx0s)
            {
                var blockTime = tx.BlockInfo.BlockTime;
                if (blockTime.HasValue)
                {
                    var blockTimeValue = blockTime.Value;
                    var yearMonth = new YearMonth(blockTimeValue.Year, blockTimeValue.Month);

                    var fee = Money.Zero;
                    var equalOutputValues = tx.GetIndistinguishableOutputs(false).Select(x => x.value).ToHashSet();
                    var feeCandidates = tx.Outputs.Where(x => !equalOutputValues.Contains(x.Value) && !TxNullDataTemplate.Instance.CheckScriptPubKey(x.ScriptPubKey));
                    if (feeCandidates.Count() == 1)
                    {
                        fee = feeCandidates.First().Value;
                    }
                    else
                    {
                        if (equalOutputValues.Count == 0)
                        {
                            List<VerboseOutputInfo> closeEnoughs = FindMostLikelyMixOutputs(feeCandidates, 10);

                            if (closeEnoughs.Any()) // There are like 5 tx from old time, I guess just experiemtns where it's not found.
                            {
                                var closeEnough = closeEnoughs.First().Value;
                                var expectedMaxFee = closeEnough.Percentage(6m); // They do some discounts to ruin user privacy.
                                var closest = feeCandidates.Where(x => x.Value < expectedMaxFee && x.Value != closeEnough).OrderByDescending(x => x.Value).FirstOrDefault();
                                if (closest is { }) // There's no else here.
                                {
                                    fee = closest.Value;
                                }
                            }
                        }
                        else if (equalOutputValues.Count == 1)
                        {
                            var poolDenomination = equalOutputValues.First();
                            var expectedMaxFee = poolDenomination.Percentage(6m); // They do some discounts to ruin user privacy.
                            var closest = feeCandidates.Where(x => x.Value < expectedMaxFee).OrderByDescending(x => x.Value).FirstOrDefault();
                            if (closest is { })
                            {
                                fee = closest.Value;
                            }
                        }
                    }

                    if (myDic.TryGetValue(yearMonth, out Money current))
                    {
                        myDic[yearMonth] = current + fee;
                    }
                    else
                    {
                        myDic.Add(yearMonth, fee);
                    }
                }
            }

            return myDic;
        }

        private static List<VerboseOutputInfo> FindMostLikelyMixOutputs(IEnumerable<VerboseOutputInfo> feeCandidates, int percentagePrecision)
        {
            var closeEnoughs = new List<VerboseOutputInfo>();
            foreach (var denom in Constants.SamouraiPools)
            {
                var found = feeCandidates.FirstOrDefault(x => x.Value.Almost(denom, denom.Percentage(percentagePrecision)));
                if (found is { })
                {
                    closeEnoughs.Add(found);
                }
            }

            if (closeEnoughs.Count > 1)
            {
                var newCloseEnoughs = FindMostLikelyMixOutputs(closeEnoughs, percentagePrecision - 1);
                if (newCloseEnoughs.Count == 0)
                {
                    closeEnoughs = closeEnoughs.Take(1).ToList();
                }
                else
                {
                    closeEnoughs = newCloseEnoughs.ToList();
                }
            }

            return closeEnoughs;
        }
    }
}
