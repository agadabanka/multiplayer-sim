using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Timer = System.Threading.Timer;

namespace MultiplayerSim
{
    public class Server : INetworkedFrameGenerator, INetworkEngineSupport
    {
        private const int SIM_RATE = 4;
        private const int SEND_RATE = 1;
        private const int SIM_FRAME_HISTORY = 10;    // Required for Server Rewind.
        private const int SIM_FRAME_BUFFER = 2;      // Larger buffer allows more Clients to send their inputs.
        private const int SEND_FRAME_BUFFER = 1;     // Ensure that current processing frame for Send is behind this buffer, else the packets may not be sent out.
        private const int SEND_FRAME_HISTORY = 2;    // There might not be much value in store SEND_FRAME_HISTORY 
        
        private UdpListener server;

        private int frame = 0;
        
        private const string alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        private int clientIdx = 0;        
        private ConcurrentDictionary<IPEndPoint, string> clientMap = new ConcurrentDictionary<IPEndPoint, string>();
        
        private ConcurrentQueue<UdpMessage> inbox;   // Stores all the received messages (Frame independent)
        private ConcurrentQueue<UdpMessage> outbox = new ConcurrentQueue<UdpMessage>(); // Stores all the messages to be sent (Frame Independent)
                
        private ConcurrentDictionary<int, List<Payload>> payloadsForSimFrame = new ConcurrentDictionary<int, List<Payload>>();
        private ConcurrentDictionary<int, List<Payload>> payloadsForSendFrame = new ConcurrentDictionary<int, List<Payload>>();

        private NetworkSim NetworkSim;
             
        private void ProcessGameCode(List<Payload> payloads)
        {
            // call into the game code here
            foreach (var payload in payloads)
            {                
                payloadsForSendFrame.GetOrAdd(frame + SEND_FRAME_BUFFER, _ => new List<Payload>()).Add(payload);
                Console.WriteLine($"Adding payload to SEND frame {frame + SEND_FRAME_BUFFER}");
            }
        }
                
        private void ReceiveMessages()
        {
            if (!NetworkSim.NetInbox.TryRemove(frame, out inbox))
            {                
                return;
            }
            
            UdpMessage udpMessage;
            while (inbox.TryDequeue(out udpMessage))
            {
                // process all the received messages here.

                string sender = clientMap.GetOrAdd(udpMessage.Sender, ip => getNextClient());
                Payload payload = Utils.DeSerialize(udpMessage.Message);                
                payload.FrameInfo = $"SR-SIM {frame}: {payload.FrameInfo}, {sender}";
                if (payload.CommandType == "JOIN")
                {
                    payload.ClientState = "CONNECTED";                    
                }
                
                // we add the messages FRAME_BUFFER ahead for the Server sim to consume it 
                payloadsForSimFrame.GetOrAdd(GetNextBufferedSimFrame(), _ => new List<Payload>()).Add(payload);
                Console.WriteLine($"Frame {frame} - Adding payload to SIM frame {GetNextBufferedSimFrame()}");
            }
            
            // free all messages for the kth frame of inputs, where k is the buffer of inputs length
            // note that k is an important factor here for Server rewind
            if (frame >= SIM_FRAME_HISTORY)
            {
                payloadsForSimFrame[frame - SIM_FRAME_HISTORY] = null;    
            }
        }

        private void SendMessages()
        {
            List<Payload> payloads =
                payloadsForSendFrame.TryGetValue(frame, out payloads) ? payloads : new List<Payload>();
            
            foreach (var payload in payloads)
            {
                // relay the input to all the registered clients
                foreach (KeyValuePair<IPEndPoint, string> pair in clientMap)
                {
                    UdpMessage outgoing;                
                    outgoing.Message = Utils.Serialize(payload);
                    outgoing.Sender = pair.Key;                
                    outbox.Enqueue(outgoing);                    
                }    
            }

            if (frame >= SEND_FRAME_HISTORY)
            {
                payloadsForSendFrame[SEND_FRAME_HISTORY] = null;
            }
        }

        private int GetNextBufferedSimFrame()
        {
            var mod = frame % SIM_FRAME_BUFFER; 
            if (mod == 0)
            {
                return frame + SIM_FRAME_BUFFER; // this frame is delayed one full buffer length
            }
            
            return frame + (SIM_FRAME_BUFFER - mod); // delayed between (SIM_FRAME_BUFFER - 1 and 1) frame
        }
        
        private string getNextClient()
        {
            if (clientIdx >= alphabet.Length)
            {
                return "Client-ExceededLimit";
            }

            return $"Client-{alphabet[clientIdx++]}";
        }
        
        #region INetworkedFrameGenerator

        public int GetFrame()
        {
            return frame;
        }

        public void SetFrame(int frame)
        {
            this.frame = frame;
        }

        public Task<UdpMessage> GetReceiver()
        {
            return server.Receive();
        }

        public int GetFrameRate()
        {
            return SIM_RATE;
        }
        
        #endregion

        #region IEngineSupport
        
        public int GetSimRate()
        {
            return  SIM_RATE;
        }

        public int GetSendRate()
        {
            return SEND_RATE;
        }

        public void Init()
        {
            Console.Write("Server: Start - " + DateTime.Now.ToShortTimeString());                        
            server = new UdpListener();
        }

        public void InitNetwork()
        {
            NetworkSim = NetworkSim.GetInsance().SetNetworkFrameGenerator(this);            
            NetworkSim.Latency = new Random().Next(50, 55);
            NetworkSim.Start();
        }
                
        public void SimCallback(Object o)
        {
                        
            frame++;            
            ReceiveMessages();
            List<Payload> payloads = payloadsForSimFrame.TryGetValue(frame, out payloads) ? payloads : new List<Payload>();
            ProcessGameCode(payloads);
            SendMessages();
        }

        public void SendCallback(Object o)
        {
            UdpMessage outgoing;
            
            while (outbox.TryDequeue(out outgoing))
            {
                Payload payload = Utils.DeSerialize(outgoing.Message);                
                payload.FrameInfo = $"SR-SEND {frame}:[{payload.FrameInfo}]";
                server.Reply(Utils.Serialize(payload), outgoing.Sender);
            }
        }       
        #endregion

    }
}