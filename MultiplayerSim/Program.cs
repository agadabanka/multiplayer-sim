using System;
using System.IO;
using System.Threading;


namespace MultiplayerSim
{    
    internal class Program
    {
        public static void Main(string[] args)
        {
            CliArgs cliArgs = new CliArgs().ParseArgs(args);

            if (cliArgs.IsServer)
            {
                NetworkEngine.GetInstance().NetworkEngineSupport = new Server();                
            }
            else if (cliArgs.IsClient)
            {
                NetworkEngine.GetInstance().NetworkEngineSupport = new Client();                                
            }
            else
            {
                Utils.WriteError("Not Server or Client: Please specify -server or -client.");
                return;                
            }   
            
            NetworkEngine.GetInstance().Start();
        }
    }
}