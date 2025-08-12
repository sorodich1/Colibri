using System;
using System.Net.Sockets;

namespace Colibri.Cons
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var sender = new MavlinkSender("127.0.0.1", 14550);
            sender.SendArmCommand();
            Console.WriteLine("Команда на армирование отправлена");
        }
    }
}