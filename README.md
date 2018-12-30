# Multiplayer Simulator
A sanbox to test the network latency, lag, jitter, etc between the server and multiple clients, that are part of the Client-Server architecture. git@github.com:agadabanka/SimpleFFA.git project demo's the usage of this simulator using a character that runs around in an open world.

## Installation
Requires Mono or .NET 4.7 

## Running on Mac OSX
To run as client use - `mono MultiplayerSim.exe -client`

To run as a server use - `mono MultiplayerSim.exe -server`

## Architecture
Both the Client and Server work off of an inbox and an outbox. When messages arrive through the socket, we push them onto an inbox. Messages in the inbox are not delivered to the process above till a certain time has passed. This time is computed based on the networking delay, which can be part of the policy file.
Once the messages are processed, they are then enqueued into an outbox, which is flushed out based on the Send Rate.

```
CLIENT                              SERVER
----------------            --------------------
INBOX   |                        INBOX  |
--------|                   ------------|  
NETSIM  | OUTBOX                 NETSIM | OUTBOX
--------|                   ------------|
NETINBOX|                       NETINBOX|
----------------            --------------------
    UDP <----------------------------->UDP

```
Note that the network simulation is done only for the Inbox and not for the Outbox. It should be sufficient to do on one or the other, since the actual route taken by packets would be independent of either the source or the destination. So simulating lag only on Inbox should provide a good approximate.
Once the message is in the inbox, it is then moved over to a per frame message queue. The per frame message queue is buffered, i.e input that arrives at frame 10, might be buffered for 2 frames and would be exposed to the Sim 2 frames later.

## NetworkSim
Includes a conduit that simulates the internet lag. The ping is simulated based on a random latency number. 
