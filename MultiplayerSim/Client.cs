using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Runtime.Remoting.Messaging;
using System.Threading;
using System.Threading.Tasks;
using Timer = System.Threading.Timer;

namespace MultiplayerSim
{
    enum ClientState
    {
        CONNECTING,
        CONNECTED,
        DISCONNECTING,
        DISCONNECTED,                        
    }
    
    public class Client : INetworkedFrameGenerator, INetworkEngineSupport
    {
        private const int SIM_RATE = 16;
        private const int SEND_RATE = 8;

        private int frame = 0;
        
        public UdpUser UdpUserClient;

        private ConcurrentQueue<UdpMessage> inbox;                       
        ConcurrentQueue<UdpMessage> outbox = new ConcurrentQueue<UdpMessage>();
        
        List<Payload> payloads = new List<Payload>();
        
        ClientState clientState = ClientState.DISCONNECTED;
        
        private NetworkSim NetworkSim;
               
        private void ProcessGameCode(List<Payload> payloads)
        {
            // call into the game code here
        }
        
        private void SendMessages()
        {
            Payload payload = new Payload();
            string clientFrame = $"CL-SIM: {frame}";
            
            switch (clientState)
            {
                case ClientState.CONNECTED:
                    payload.CommandType = "INPUT";
                    payload.ClientState = "CONNECTED";
                    if (payload.CommandType == "LEAVE")
                    {
                        clientState = ClientState.DISCONNECTING;
                    }
                    break;
                case ClientState.DISCONNECTED:
                    payload.CommandType = "JOIN";
                    payload.ClientState = "CONNECTING";
                    clientState = ClientState.CONNECTING;
                    break;
                case ClientState.DISCONNECTING:                    
                    break;
                case ClientState.CONNECTING:                    
                    break;                    
            }
            UdpMessage outgoing;
            
            payload.FrameInfo = clientFrame;            
            outgoing.Message = Utils.Serialize(payload);
            outgoing.Sender = null;            
            outbox.Enqueue(outgoing);  
        }

        private void ReceiveMessages()
        {
            Payload payload;
            string clientFrame = $"CL-SIM: {frame}";
            
            if (NetworkSim.NetInbox.TryRemove(frame, out inbox))
            {                                          
                while (inbox.TryDequeue(out var received))
                {
                    payload = Utils.DeSerialize(received.Message);
                    switch (clientState)
                    {
                        case ClientState.CONNECTED:                            
                            break;
                        case ClientState.DISCONNECTED:                            
                            break;
                        case ClientState.DISCONNECTING:                    
                            if (payload.ClientState == "DISCONNECTED")
                            {
                                clientState = ClientState.DISCONNECTED;
                            }
                            break;
                        case ClientState.CONNECTING:
                            if (payload.ClientState == "CONNECTED")
                            {
                                clientState = ClientState.CONNECTED;
                            }        
                            break;                    
                    }
                    payloads.Add(payload);
                    Console.WriteLine($"{clientFrame}:[{payload.FrameInfo}, {payload.CommandType}]");
                }       
            }

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
            return UdpUserClient.Receive();
        }

        public int GetFrameRate()
        {
            return SIM_RATE;
        }
        
        #endregion

        #region INetworkEngineSupport

        public void Init()
        {
            string ip = "127.0.0.1";           
            Console.WriteLine("Client: Starting Client, connected to "+ ip + " - " + DateTime.Now.ToShortTimeString());                        
            //create a new client
            UdpUserClient = UdpUser.ConnectTo(ip, 32123);
        }

        public void InitNetwork()
        {
            NetworkSim = NetworkSim.GetInsance().SetNetworkFrameGenerator(this);            
            NetworkSim.Latency = new Random().Next(50, 55);;
            NetworkSim.Start();
        }

        public void SimCallback(Object o)
        {
            frame++;            
            ReceiveMessages();
            ProcessGameCode(payloads);
            SendMessages();                      
        }                        
        

        public void SendCallback(Object o)
        {
            UdpMessage outgoing;
            
            while (outbox.TryDequeue(out outgoing))
            {
                Payload payload = Utils.DeSerialize(outgoing.Message);                
                payload.FrameInfo = $"CL-SEND {frame}:[{payload.FrameInfo}]";                                
                string serializedMessage = Utils.Serialize(payload);                 
                Console.WriteLine(payload.FrameInfo);                
                UdpUserClient.Send(serializedMessage);
            }
        }

        public int GetSimRate()
        {
            return  SIM_RATE;
        }

        public int GetSendRate()
        {
            return SEND_RATE;
        }        

        #endregion
    }
}