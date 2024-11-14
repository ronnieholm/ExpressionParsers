namespace ShuntingYardParser.CSharp.InfixToAbstractSyntaxTree;

public class HierarchicalExpressionPrinter
{
    private const int DefaultIndentation = 4;
    private string _result = "";

    public string Print(Expression e)
    {
        Print(e, 0);
        return _result;
    }

    private void AddLine(string s) =>
        _result += s + Environment.NewLine;

    private void Print(Expression e, int indentation)
    {
        switch (e)
        {
            case UnaryMinus minus:
                AddLine(new string(' ', indentation) + minus.GetType().Name);
                Print(minus.Operand, indentation + DefaultIndentation);
                break;
            case BinaryPlus plus:
                AddLine(new string(' ', indentation) + plus.GetType().Name);
                Print(plus.LeftOperand, indentation + DefaultIndentation);
                Print(plus.RightOperand, indentation + DefaultIndentation);
                break;
            case BinaryMinus binaryMinus:
                AddLine(new string(' ', indentation) + binaryMinus.GetType().Name);
                Print(binaryMinus.LeftOperand, indentation + DefaultIndentation);
                Print(binaryMinus.RightOperand, indentation + DefaultIndentation);
                break;
            case BinaryMul mul:
                AddLine(new string(' ', indentation) + mul.GetType().Name);
                Print(mul.LeftOperand, indentation + DefaultIndentation);
                Print(mul.RightOperand, indentation + DefaultIndentation);
                break;
            case BinaryDiv div:
                AddLine(new string(' ', indentation) + div.GetType().Name);
                Print(div.LeftOperand, indentation + DefaultIndentation);
                Print(div.RightOperand, indentation + DefaultIndentation);
                break;
            case BinaryExp exp:
                AddLine(new string(' ', indentation) + exp.GetType().Name);
                Print(exp.LeftOperand, indentation + DefaultIndentation);
                Print(exp.RightOperand, indentation + DefaultIndentation);
                break;
            case Literal literal:
                AddLine($"{new string(' ', indentation)}{literal.GetType().Name} ({literal.Value})");
                break;
            default:
                throw new ArgumentException($"Unsupported type: {e}");
        }
    }
}