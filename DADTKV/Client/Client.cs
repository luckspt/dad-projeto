using Common;
using DADTKV;
using DADTKV.Transactions;
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
        private Peer CurrentTransactionManager
        {
            get => this.transactionManagers[this.currentTMIdx];
            set => this.currentTMIdx = this.transactionManagers.IndexOf(value);
        }

        public Client(string id, int currentTMIdx, List<ClientCommandConfigLine> commands, List<Peer> transactionManagersAddresses)
        {
            this.id = id;
            this.commands = commands;
            this.transactionManagers = transactionManagersAddresses;
            this.currentTMIdx = currentTMIdx;
            this.transactions = new Transactions();
        }

        public void Start()
        {
            while (true)
            {
                this.ProcessCommands();
            }
        }

        private void ProcessCommands()
        {
            Logger.GetInstance().Log("ProcessCommands", $"Starting to process {this.commands.Count} commands...");

            foreach (ClientCommandConfigLine command in this.commands)
            {
                switch (command.Type)
                {
                    case ClientCommandConfigType.Wait:
                        var waitCommand = (WaitConfigLine)command;

                        Logger.GetInstance().Log("ProcessCommands", $"Waiting {waitCommand.Time}ms...");
                        Thread.Sleep(waitCommand.Time);
                        break;
                    case ClientCommandConfigType.ReadWriteSet:
                        Logger.GetInstance().Log("ProcessCommands", $"Sending transaction...");

                        var rwCommand = (ReadWriteSetConfigLine)command;

                        List<DadInt> readValues = this.transactions.TxSubmit(this.id, rwCommand.ReadSet, rwCommand.WriteSet);
                        string readValuesStr = string.Join(", ", readValues.Select(v => v.ToString()));

                        System.Console.WriteLine($"Values read: {readValuesStr}");
                        break;
                }
            }

            Logger.GetInstance().Log("ProcessCommands", $"Finished processing commands!");
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
