using System;

namespace IronCore
{
    class Program
    {
        static void Main(string[] args)
        {
            using (var game = new MainGame())
            {
                game.Run();
            }
        }
    }
}
