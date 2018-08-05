using System;
using System.IO;
using System.IO.Compression;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;

namespace MultiplayerSim
{
    public class Utils
    {     
        public static void WriteError(string message)
        {            
            Console.ForegroundColor = ConsoleColor.DarkRed;                   
            Console.Write(message);
            Console.ResetColor();
        }
               
        public static string Serialize(Payload payload)
        {
            using (MemoryStream input = new MemoryStream())
            {
                BinaryFormatter bformatter = new BinaryFormatter();
                bformatter.Serialize(input, payload);
                input.Seek(0, SeekOrigin.Begin);

                using (MemoryStream output = new MemoryStream())                
                {
                    input.CopyTo(output);                    
                    return Convert.ToBase64String(output.ToArray());
                }
            }
        }

        public static Payload DeSerialize(string message)
        {
            using (MemoryStream input = new MemoryStream(Convert.FromBase64String(message)))            
            using (MemoryStream output = new MemoryStream())
            {
                input.CopyTo(output);                
                output.Seek(0, SeekOrigin.Begin);

                BinaryFormatter bformatter = new BinaryFormatter();
                Payload payload = (Payload)bformatter.Deserialize(output);
                return payload;
            }
        }
    }
}