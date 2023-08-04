using RecursiveDescentParser.Core;
using static System.Console;

namespace RecursiveDescentParser.Cli;

static class Program
{
    static void Main()
    {         
        WriteLine("Enter expression. Press Ctrl-c to exit.");
        while (true)
        {
            Write("> ");
            var input = ReadLine();
            if (input == null)
                break;
            
            var lexer = new Lexer(input);
            var parser = new Parser(lexer, new Tracer());
            var result = parser.Parse();
            WriteLine(result);
        }
    }
}