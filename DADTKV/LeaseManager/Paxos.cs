using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

internal class Paxos
{
    private String id;
    private bool isLeader;
    private int clock;
    private List<PaxosMiddleware> peers;
    private int writeTimestamp;
    private int readTimestamp;
    private int value;

    public Paxos(String id)
    {
        this.id = id;
        this.isLeader = false;
        this.clock = 0;
        this.peers = new List<PaxosMiddleware>();
    }

    public void SendPrepare()
    {
        // Send prepare(timestamp) to all peers
    }

    public void ReceivePrepare(PaxosMiddleware proposer, int timestamp)
    {
        // IF readTimestamp < timestamp
        // - Send promise(writeTimestamp, value) to proposer
        // - Update readTimestamp to timestamp
        // ELSE (silent or negative ack)
    }

    public void ReceivePromise()
    {
        // Receive promise(writeTimestamp, value) from majority of nodes.
        // Choose value with highest writeTimestamp. If value is null, then propose a new value.
    }

    public void ReceiveAccept(PaxosMiddleware proposer, int timestamp, int value)
    {
        // IF readTimestamp == timestamp
        // - Send accepted(timestamp, value) to all peers
        /// - Update writeTimestamp to timestamp
        // ELSE (silent or negative ack)
    }  

    public void ReceiveAccepted()
    {
        // Receive accepted(idProposer, value) from majority of nodes.
        // If majority, then consensus reached.
    }

    public void AddPeer(PaxosMiddleware peer)
    {
        this.peers.Add(peer);
    }

    public void RemovePeer(PaxosMiddleware peer)
    {
        this.peers.Remove(peer);
    }
}

class PaxosMiddleware
{

}


