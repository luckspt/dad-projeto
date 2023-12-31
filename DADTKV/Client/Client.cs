﻿using Common;
using DADTKV;
using DADTKV.Transactions;
using Parser;
using Parser.Parsers.ClientCommand;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Client
{
    internal class Client
    {
        private string id;
        private List<ClientCommandConfigLine> commands;
        private List<Peer> transactionManagers;
        private int currentTMIdx;
        private Transactions transactions;
        private StatusServiceClient statusServiceClient;
        private Peer CurrentTransactionManager
        {
            get => this.transactionManagers[this.currentTMIdx];
            set => this.currentTMIdx = this.transactionManagers.IndexOf(value);
        }

        public Client(string id, string managerAddress, int currentTMIdx, List<ClientCommandConfigLine> commands, List<Peer> transactionManagersAddresses)
        {
            this.id = id;
            this.commands = commands;
            this.transactionManagers = transactionManagersAddresses;
            this.currentTMIdx = currentTMIdx;
            this.transactions = new Transactions(CurrentTransactionManager);
            this.statusServiceClient = new StatusServiceClient(managerAddress);
        }

        public void Start()
        {
            while (true)
            {
                // this.ProcessCommands();
                this.ProcessCLICommands();
            }
        }

        private void ProcessCLICommands()
        {
            Parser.Parser parser = new ReadWriteSetParser();
            do
            {
                try
                {
                    string rawCommand = System.Console.ReadLine()!;
                    Tuple<ConfigType, ConfigLine> parsedCommand = parser.Result(rawCommand);
                    ClientCommandConfigLine command = (ClientCommandConfigLine)parsedCommand.Item2;

                    this.ExecuteCommand(command);
                }
                catch (Exception ex)
                {
                    System.Console.WriteLine(ex.Message);
                }
            } while (true);
        }

        private void ProcessCommands()
        {
            Logger.GetInstance().Log("ProcessCommands", $"Starting to process {this.commands.Count} commands...");

            foreach (ClientCommandConfigLine command in this.commands)
            {
                this.ExecuteCommand(command);
            }

            Logger.GetInstance().Log("ProcessCommands", $"Finished processing commands!");
        }

        private void ExecuteCommand(ClientCommandConfigLine command)
        {
            switch (command.Type)
            {
                case ClientCommandConfigType.Wait:
                    var waitCommand = (WaitConfigLine)command;

                    Console.WriteLine($"Waiting {waitCommand.Time}ms...");
                    Thread.Sleep(waitCommand.Time);
                    break;
                case ClientCommandConfigType.ReadWriteSet:
                    var rwCommand = (ReadWriteSetConfigLine)command;

                    Console.WriteLine($"Sending transaction: reads={string.Join(",", rwCommand.ReadSet)}, writes={string.Join(",", rwCommand.WriteSet)}");

                    List<DadInt> readValues = this.transactions.TxSubmit(this.id, rwCommand.ReadSet, rwCommand.WriteSet);
                    if (readValues.Count == 0)
                        System.Console.WriteLine($"No values read.");
                    else
                    {
                        string readValuesStr = string.Join(", ", readValues.Select(v => v.ToString()));
                        System.Console.WriteLine($"Values read: {readValuesStr}");
                    }
                    break;
                case ClientCommandConfigType.Status:
                    Console.WriteLine($"Asking manager to request all processes to display their status...");
                    this.statusServiceClient.Status();
                    break;
            }
        }

        private void SetNewTransactionManager()
        {
            // Select a random transaction manager that's not the current one
            Random random = new Random();
            int newTMIdx = random.Next(this.transactionManagers.Count);

            if (newTMIdx == this.currentTMIdx)
                newTMIdx++;

            if (newTMIdx >= this.transactionManagers.Count)
                newTMIdx = newTMIdx % this.transactionManagers.Count;

            this.currentTMIdx = newTMIdx;
        }
    }
}
