using Common;
using DADTKV;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TransactionManager.Store;

namespace TransactionManager.Transactions.Replication
{
    internal class TransactionRunningServiceLogic
    {
        public TransactionManager transactionManager;
        public TransactionRunningServiceLogic(TransactionManager transactionManager)
        {
            this.transactionManager = transactionManager;
        }

        public List<DadInt> RunTransaction(string clientId, List<ReadOperation> keysToRead, List<WriteOperation> keysToWrite)
        {
            // Only one running transaction at a time
            lock (this)
            {
                Logger.GetInstance().Log($"TransactionRunningService.{clientId}", $"Client wants to read={string.Join(",", keysToRead)} and write={string.Join(",", keysToWrite)}");

                // Create Transaction
                Transaction transaction = new Transaction(
                    this.transactionManager.ManagerId,
                    Guid.NewGuid().ToString(),
                    keysToRead,
                    keysToWrite
                );

                // We create a lock so we only reply to the client once: we READ AND we are allowed to reply (because WRITE has propagated)
                TransactionReplyLock replyLock = new TransactionReplyLock();
                lock (replyLock)
                {
                    // Add it so it's available to the other threads
                    this.transactionManager.TransactionReplyLocks.Add(transaction.Guid, new KeyValuePair<TransactionReplyLock, Transaction>(replyLock, transaction));

                    // Add transaction to be worked on by worker thread
                    // Worker thread will Pulse replyLock when READ is completed as well as when WRITE is completed
                    lock (this.transactionManager.TransactionsBuffer)
                    {
                        this.transactionManager.TransactionsBuffer.Add(transaction);
                        Monitor.Pulse(this.transactionManager.TransactionsBuffer); // To alert the worker thread
                        Logger.GetInstance().Log($"TransactionRunningService.{clientId}", $"TransactionBuffer worker pulsed");
                    }

                    // Wait until we're allowed to reply
                    while (!replyLock.CanReply || replyLock.ReplyValue == null)
                    {
                        Logger.GetInstance().Log($"TransactionRunningService.{clientId}", $"Waiting to reply...");
                        // When transaction READ is concluded, it will write to the ReplyValue (and pulse it)
                        // When transaction WRITE is concluded (after URB), it will write CanReply as true (and pulse it)
                        Monitor.Wait(replyLock);
                    }

                    // TODO should garbage collect replyLock?
                    //  can have issues if other threads still want to use it lol

                    // Return read values
                    Logger.GetInstance().Log($"TransactionRunningService.{clientId}", $"Replied.");
                    return replyLock.ReplyValue!;
                }
            }
        }
    }
}
