using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace fileSignature
{
    class Program
    {
        static void Main(string[] args)
        {
            string _path;
            int _size;

            try
            {
                Console.TreatControlCAsInput = false;
                
                handleParametrs(args, out _path, out _size);

                var blockPool = new BlockPool(_size, _path);
            
                var pool = new ThreadPool();
                pool.ThreadsCompleted += Pool_ThreadsCompleted;
                pool.Start(blockPool);
            }
            catch (Exception exc)
            {
                Console.WriteLine("{0}\n{1}", exc.Message, exc.StackTrace);
                Console.ReadKey();
            }
        }

        private static void Pool_ThreadsCompleted(object sender, ThreadsCompletedEventArgs e)
        {
            if (e.isBreaked)
                Console.WriteLine("\nthreads breaked");
            else
                Console.WriteLine("\nthreads finished");

            Console.ReadLine();
        }

        static void handleParametrs(string[] args, out string path, out int size)
        {
            if (args.Length < 2)
            {
                Console.WriteLine("Введите путь к файлу:");
                path = Console.ReadLine();

                Console.WriteLine("Введите размер блока:");
                while (!int.TryParse(Console.ReadLine(), out size))
                {
                    Console.WriteLine("Введённое число неверно, попробуйте ещё раз.");
                }
            }
            else
            {
                path = args[0];
                if (!int.TryParse(args[1], out size))
                {
                    Console.WriteLine("Параметр 2 задан неверно");
                    return;
                }
            }
        }
    }
}
