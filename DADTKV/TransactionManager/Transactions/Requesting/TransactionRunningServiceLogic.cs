using DADTKV;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TransactionManager.Store;

namespace TransactionManager.Transactions
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
                // Create Transaction
                Transaction transaction = new Transaction(
                    clientId,
                    keysToRead,
                    keysToWrite
                );

                // We create a lock so we only reply to the client once: we READ AND we are allowed to reply (because WRITE has propagated)
                TransactionReplyLock replyLock = new TransactionReplyLock();
                lock (replyLock)
                {
                    // Add it so it's available to the other threads
                    this.transactionManager.TransactionReplyLocks.Add(transaction, replyLock);

                    // Add transaction to be worked on by worker thread
                    // Worker thread will Pulse replyLock when READ is completed as well as when WRITE is completed
                    this.transactionManager.TransactionsBuffer.Add(transaction);

                    // Wait until we're allowed to reply
                    while (!replyLock.CanReply || replyLock.ReplyValue == null)
                    {
                        // When transaction READ is concluded, it will write to the ReplyValue (and pulse it)
                        // When transaction WRITE is concluded (after URB), it will write CanReply as true (and pulse it)
                        Monitor.Wait(replyLock);
                    }

                    // TODO should garbage collect replyLock?
                    //  can have issues if other threads still want to use it lol

                    // Return read values
                    return replyLock.ReplyValue!;
                }
            }
        }
    }
}
