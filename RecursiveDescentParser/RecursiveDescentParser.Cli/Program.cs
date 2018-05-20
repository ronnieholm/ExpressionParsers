using System;
using RecursiveDescentParser.Core;

namespace RecursiveDescentParser.Cli
{
    class Program
    {
        static void Main(string[] args)
        {         
            Console.WriteLine("Enter expression. Ctrl-c for exit.");
            while (true)
            {
                Console.Write("> ");
                var input = Console.ReadLine();
                var lexer = new Lexer(input);
                var parser = new Parser(lexer);
                var result = parser.Parse();
                Console.WriteLine(result);
            }
        }
    }
}
