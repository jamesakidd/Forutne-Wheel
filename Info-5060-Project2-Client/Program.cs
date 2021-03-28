﻿using FortuneWheelLibrary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            //LinkedList<int> intLL = new LinkedList<int>();

            //intLL.AddLast(1);
            //intLL.AddLast(2);
            //intLL.AddLast(3);
            //intLL.AddLast(4);
            //intLL.AddLast(5);

            //IEnumerator<int> e = intLL.GetEnumerator();
            //for (int i = 0; i < 20; i++)
            //{
            //    if (e.MoveNext())
            //    {
            //        Console.WriteLine(e.Current);
            //    }
            //    else
            //    {
            //        e.Reset();
            //        e.MoveNext();
            //        Console.WriteLine(e.Current);
            //    }
            //}



            //e.Dispose();

            Wheel wheel = new Wheel();

            wheel.AddPlayer(new Player("James"));

            Console.WriteLine($"The current phrase is: \"{wheel.CurrentPhrase}\", from the category: \"{wheel.CurrentCategory}\"");

            Console.WriteLine($"\nCurrent available letters and if they're available: ");
            foreach (KeyValuePair<char, bool> pair in wheel.Letters)
            {
                Console.WriteLine($"{pair.Key}, {pair.Value}");
            }

            Console.WriteLine("\n\nThe longest phrase is: ");

            string longestPhrase = "";

            foreach (var s in from puzzle in wheel.Puzzles from s in puzzle.Value where s.Length > longestPhrase.Length select s)
            {
                longestPhrase = s;
            }

            Console.WriteLine($"\"{longestPhrase}\" at {longestPhrase.Length} chars."); //"What's Good For The Goose Is Good For The Gander" at 48 chars.



            Console.ReadKey();
        }
    }
}
