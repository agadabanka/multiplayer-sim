using System;

namespace MultiplayerSim
{
    [Serializable()]
    public class Payload
    {
        public string FrameInfo;
        public string CommandType;
        public int ControlInput;    // Joystick
        public string Position;     // Can be other objects
        public string ClientState;    // DISCONNECTED, CONNECTED
    }
}