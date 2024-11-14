using ShuntingYardParser.CSharp.InfixEvaluator;
using ShuntingYardParser.CSharp.InfixToAbstractSyntaxTree;
using ShuntingYardParser.CSharp.InfixToPostfix;
using ShuntingYardParser.CSharp.InfixToPrefix;
using ShuntingYardParser.CSharp.Lexer;

namespace ShuntingYardParser.CSharp;

public static class Program
{
    public static void Main()
    {
        TestVariousExpressions();
        ExecuteRepl();
    }

    private static void TestVariousExpressions()
    {
        string[][] expressions =
        [
            // infix, value, prefix, postfix, AST
            ["2", "2", "2", "2", "2"],
            ["-2", "-2", "- 2", "2 -", "-(2)"],
            ["-2 + 5", "3", "+ - 2 5", "2 - 5 +", "+(-(2), 5)"],
            ["2 + 5 * 7", "37", "+ 2 * 5 7", "2 5 7 * +", "+(2, *(5, 7))"],
            ["-(2 + 5) * 7", "-49", "* - + 2 5 7", "2 5 + - 7 *", "*(-(+(2, 5)), 7)"],
            ["1 ^ 2 ^ 3", "1", "^ 1 ^ 2 3", "1 2 3 ^ ^", "^(1, ^(2, 3))"]
        ];

        foreach (var e in expressions)
        {
            int value = new InfixEvaluatorParser(new ExpressionLexer()).Parse(e[0]);
            Token prefix = new InfixToPrefixParser(new ExpressionLexer()).Parse(e[0]);
            Token postfix = new InfixToPostfixParser(new ExpressionLexer()).Parse(e[0]);
            Expression expression = new InfixToAbstractSyntaxTreeParser(new ExpressionLexer()).Parse(e[0]);

            if (e[1] != value.ToString() &&
                e[1] != expression.Evaluate().ToString() &&
                e[2] != prefix.Lexeme &&
                e[3] != postfix.Lexeme &&
                e[4] != FlatExpressionPrinter.Print(expression))
            {
                Console.WriteLine($"Error parsing: {e[0]}");
            }
        }
    }

    private static void ExecuteRepl()
    {
        Console.WriteLine("Enter a syntactically valid mathematical expression, such as (2 + 3) * 4.");
        Console.WriteLine("Only integer math, parenthesis, and operators +, -, *, /, ^ supported.");
        while (true)
        {
            Console.Write("> ");
            var infix = Console.ReadLine()!;
            int value = new InfixEvaluatorParser(new ExpressionLexer()).Parse(infix);
            Token prefix = new InfixToPrefixParser(new ExpressionLexer()).Parse(infix);
            Token postfix = new InfixToPostfixParser(new ExpressionLexer()).Parse(infix);
            Expression expression = new InfixToAbstractSyntaxTreeParser(new ExpressionLexer()).Parse(infix);

            var hierarchicalPrinter = new HierarchicalExpressionPrinter();
            Console.WriteLine($"Value: {value}");
            Console.WriteLine($"Prefix notation: {prefix.Lexeme}");
            Console.WriteLine($"Postfix notation: {postfix.Lexeme}");
            Console.WriteLine($"Flat syntax tree: {FlatExpressionPrinter.Print(expression)}");
            Console.WriteLine($"Hierarchical syntax tree: {Environment.NewLine}");
            Console.WriteLine(hierarchicalPrinter.Print(expression));
        }
        // ReSharper disable once FunctionNeverReturns
    }
}