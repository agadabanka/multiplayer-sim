using System;

namespace MultiplayerSim
{
    public interface INetworkEngineSupport
    {
        void Init();
        void InitNetwork();
        void SimCallback(Object o);
        void SendCallback(Object o);
        int GetSimRate();
        int GetSendRate();
    }
}