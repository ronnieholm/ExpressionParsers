using System;
using RecursiveDescentParser.Core;
using static System.Console;

namespace RecursiveDescentParser.Cli
{
    class Program
    {
        static void Main(string[] args)
        {         
            WriteLine("Enter expression. Press Ctrl-c to exit.");
            while (true)
            {
                Write("> ");
                var input = Console.ReadLine();
                var lexer = new Lexer(input);
                var parser = new Parser(lexer);
                var result = parser.Parse();
                WriteLine(result);
            }
        }
    }
}
