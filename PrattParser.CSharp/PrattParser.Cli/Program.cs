using PrattParser.Core;
using static System.Console;

namespace PrattParser.Cli;

class Program
{
    static void Main(string[] args)
    {         
        WriteLine("Enter expression. Press Ctrl-C to exit.");
        while (true)
        {
            Write("> ");
            var input = ReadLine();
            if (input == null)
                break;

            var lexer = new Lexer(input);
            var parser = new ExpressionParser(lexer);
            var result = parser.Parse();
            WriteLine($"{result.String}");
        }
    }
}