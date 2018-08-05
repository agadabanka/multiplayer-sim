using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace MultiplayerSim
{
    public struct UdpMessage
    {
        public IPEndPoint Sender;
        public string Message;        
    }
    
    public abstract class UdpBase
    {
        protected UdpClient Client;

        protected UdpBase()
        {
            Client = new UdpClient();
        }

        public async Task<UdpMessage> Receive()
        {
            var result = await Client.ReceiveAsync();
            return new UdpMessage()
            {
                Message = Encoding.ASCII.GetString(result.Buffer, 0, result.Buffer.Length),
                Sender = result.RemoteEndPoint
            };
        }        
    }
}