using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace MultiplayerSim
{
    public interface INetworkedFrameGenerator
    {
        int GetFrame();
        void SetFrame(int frame);
        int GetFrameRate();        
        Task<UdpMessage> GetReceiver();        
    }
}