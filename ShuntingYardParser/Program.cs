using ShuntingYardParser.Lexer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ShuntingYardParser.InfixEvaluator;
using ShuntingYardParser.InfixToPrefix;
using ShuntingYardParser.InfixToPostfix;
using ShuntingYardParser.InfixToAbstractSyntaxTree;

namespace ShuntingYardParser {
    public class Program {
        public static void Main(String[] args) {
            TestVariousExpression();
            ExecuteREPL();
        }

        private static void TestVariousExpression() {
            string[][] expressions = {
                // infix, value, prefix, postfix, AST
                new[] { "2", "2", "2", "2", "2" },
                new[] { "-2", "-2", "- 2", "2 -", "-(2)" },
                new[] { "-2 + 5", "3", "+ - 2 5", "2 - 5 +", "+(-(2), 5)" },
                new[] { "2 + 5 * 7", "37", "+ 2 * 5 7", "2 5 7 * +", "+(2, *(5, 7))" },
                new[] { "-(2 + 5) * 7", "-49", "* - + 2 5 7", "2 5 + - 7 *", "*(-(+(2, 5)), 7)" },
                new[] { "1 ^ 2 ^ 3", "1", "^ 1 ^ 2 3", "1 2 3 ^ ^", "^(1, ^(2, 3))" },
            };

            foreach (string[] e in expressions) {
                int value = new InfixEvaluatorParser(new ExpressionLexer()).Parse(e[0]);
                Token prefix = new InfixToPrefixParser(new ExpressionLexer()).Parse(e[0]);
                Token postfix = new InfixToPostfixParser(new ExpressionLexer()).Parse(e[0]);
                Expression expression = new InfixToAbstractSyntaxTreeParser(new ExpressionLexer()).Parse(e[0]);

                FlatExpressionPrinter printer = new FlatExpressionPrinter();
                if (e[1] != value.ToString() &&
                    e[1] != expression.Evaluate().ToString() &&
                    e[2] != prefix.Lexeme &&
                    e[3] != postfix.Lexeme &&
                    e[4] != printer.Print(expression)) {
                    Console.WriteLine("Error parsing: " + e[0]);
                }
            }
        }

        private static void ExecuteREPL() {
            string infix = "";

            Console.WriteLine("Enter a syntactically valid mathematical expression, such as (2 + 3) * 4.");
            Console.WriteLine("Only integer math, parenthesis, and operators +, -, *, /, ^ supported.");
            while (true) {
                Console.Write("> ");
                infix = Console.ReadLine();

                int value = new InfixEvaluatorParser(new ExpressionLexer()).Parse(infix);
                Token prefix = new InfixToPrefixParser(new ExpressionLexer()).Parse(infix);
                Token postfix = new InfixToPostfixParser(new ExpressionLexer()).Parse(infix);
                Expression expression = new InfixToAbstractSyntaxTreeParser(new ExpressionLexer()).Parse(infix);

                FlatExpressionPrinter flatPrinter = new FlatExpressionPrinter();
                HierarchicalExpressionPrinter HierarchicalPrinter = new HierarchicalExpressionPrinter();

                Console.WriteLine("Value: " + value);
                Console.WriteLine("Prefix notation: " + prefix.Lexeme);
                Console.WriteLine("Postfix notation: " + postfix.Lexeme);
                Console.WriteLine("Flat syntax tree: " + flatPrinter.Print(expression));
                Console.WriteLine("Hierarchical syntax tree: " + Environment.NewLine);
                Console.WriteLine(HierarchicalPrinter.Print(expression));
            }
        }
    }
}
