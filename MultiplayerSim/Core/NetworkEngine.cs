using System.Threading;

namespace MultiplayerSim
{
    public class NetworkEngine
    {
        public INetworkEngineSupport NetworkEngineSupport { get; set;  }
        public const int ONE_SECOND_MS = 1000; // milliseconds
        private static NetworkEngine _instance;
        
        public static NetworkEngine GetInstance()
        {
            return _instance ?? (_instance = new NetworkEngine());
        }
        
        public void Start()
        {
            // Initialize anything that required to Send/Receive
            NetworkEngineSupport.Init();            
            // Initialize the Network Simulation itself
            NetworkEngineSupport.InitNetwork();

            // Setup Simulation for the networked entities at regular intervals
            new Timer(NetworkEngineSupport.SimCallback, null, 0, ONE_SECOND_MS / NetworkEngineSupport.GetSimRate());
            
            // Setup Sending of the batched messages at regular intervals
            new Timer(NetworkEngineSupport.SendCallback, null, 0, ONE_SECOND_MS / NetworkEngineSupport.GetSendRate());

            while (true) ;
        }

    }
}