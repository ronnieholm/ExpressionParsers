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
                var input = ReadLine();
                var lexer = new Lexer(input);
                var parser = new Parser(lexer, new Tracer());
                var result = parser.Parse();
                WriteLine(result);
            }
        }
    }
}
