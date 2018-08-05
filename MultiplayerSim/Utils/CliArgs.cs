using System;

namespace MultiplayerSim
{
    public class CliArgs
    {
        public Boolean IsServer { get; set; }
        public Boolean IsClient { get; set; }

        public CliArgs ParseArgs(string [] args)
        {
            IsServer = false;
            IsClient = false;

            if (args.Length <= 0)
            {
                return this;
            }

            if (args[0].Equals("-server"))
            {
                IsServer = true;
            }
            else if (args[0].Equals("-client"))
            {
                IsClient = true;
            }

            return this;
        }
    }
}