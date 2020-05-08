﻿using Dumplings.Helpers;
using Dumplings.Scanning;
using NBitcoin;
using NBitcoin.RPC;
using System;
using System.Net;
using System.Threading.Tasks;

namespace Dumplings.Cli
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Logger.InitializeDefaults();

            Logger.LogInfo("Parsing arguments...");
            ParseArgs(args, out Command command, out NetworkCredential rpcCred);

            var rpcConf = new RPCCredentialString
            {
                UserPassword = rpcCred
            };
            var client = new RPCClient(rpcConf, Network.Main);

            Logger.LogInfo("Checking Bitcoin Knots sync status...");

            var bci = await client.GetBlockchainInfoAsync();

            var missingBlocks = bci.Headers - bci.Blocks;
            if (missingBlocks != 0)
            {
                throw new InvalidOperationException($"Knots is not synchronized. Blocks missing: {missingBlocks}.");
            }

            Logger.LogInfo($"Bitcoin Knots is synchronized. Current height: {bci.Blocks}.");

            using (BenchmarkLogger.Measure(operationName: $"{command} Command"))
            {
                if (command == Command.Resync)
                {
                    var scanner = new Scanner(client);
                    await scanner.ScanAsync(rescan: true);
                }
                else if (command == Command.Sync)
                {
                    var scanner = new Scanner(client);
                    await scanner.ScanAsync(rescan: false);
                }
            }

            Console.WriteLine();
            Console.WriteLine("Press a button to exit...");
            Console.ReadKey();
        }

        private static void ParseArgs(string[] args, out Command command, out NetworkCredential cred)
        {
            string rpcUser = null;
            string rpcPassword = null;
            command = (Command)Enum.Parse(typeof(Command), args[0], ignoreCase: true);

            var rpcUserArg = "--rpcuser=";
            var rpcPasswordArg = "--rpcpassword=";
            foreach (var arg in args)
            {
                var idx = arg.IndexOf(rpcUserArg, StringComparison.Ordinal);
                if (idx == 0)
                {
                    rpcUser = arg.Substring(idx + rpcUserArg.Length);
                }

                idx = arg.IndexOf(rpcPasswordArg, StringComparison.Ordinal);
                if (idx == 0)
                {
                    rpcPassword = arg.Substring(idx + rpcPasswordArg.Length);
                }
            }

            cred = new NetworkCredential(rpcUser, rpcPassword);
        }
    }
}