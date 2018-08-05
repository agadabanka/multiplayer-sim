using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Threading.Tasks;

namespace MultiplayerSim
{
    public class NetworkSim
    {               
        private static NetworkSim _instance;

        public INetworkedFrameGenerator NetworkedFrameGenerator { get; set; }
        public ConcurrentDictionary<int, ConcurrentQueue<UdpMessage>> NetInbox = 
            new ConcurrentDictionary<int, ConcurrentQueue<UdpMessage>>();

        public int Latency;
                
        public void Start()
        {
            Task.Factory.StartNew(async () => {
                while (true)
                {
                    try
                    {
                        var received = await NetworkedFrameGenerator.GetReceiver();                                    
                        int frameWithDelay = NetworkedFrameGenerator.GetFrame() + GetFrameDelayWithNetworkLatency();
                        // Console.WriteLine($"{received.Message} delayed to frame {frameWithDelay}");
                        NetInbox.GetOrAdd(frameWithDelay, f => new ConcurrentQueue<UdpMessage>()).Enqueue(received);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex);
                        throw;
                    }                    
                }
            });
        }

        public static NetworkSim GetInsance()
        {
            return _instance ?? (_instance = new NetworkSim());
        }

        public NetworkSim SetNetworkFrameGenerator(INetworkedFrameGenerator networkedFrameGenerator)
        {
            NetworkedFrameGenerator = networkedFrameGenerator;
            return GetInsance();
        }
        
        public int GetFrameDelayWithNetworkLatency()
        {
            int timeBetweenSims = 1000 / NetworkedFrameGenerator.GetFrameRate(); // ms
            return (int) Math.Ceiling(Latency * 1.0 / timeBetweenSims);
        }
    }
}