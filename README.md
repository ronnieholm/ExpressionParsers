Shunting Yard parser
====================

Implements a parser that accepts a mathematical expression in infix notation and
outputs the corresponding prefix and postfix notation and the abstract syntax tree
for the expression. The parser adheres to the common rules of associativity and 
precedence.

Here's an example of running the application:

	> 2 + 5 * 7
	Value: 37
	Prefix notation: + 2 * 5 7
	Postfix notation: 2 5 7 * +
	Flat syntax tree: +(2, *(5, 7))
	Hierarchical syntax tree:

	BinaryPlus
		Literal (2)
		BinaryMul
			Literal (5)
			Literal (7)

	> (2 + 5) * 7
	Value: 49
	Prefix notation: * + 2 5 7
	Postfix notation: 2 5 + 7 *
	Flat syntax tree: *(+(2, 5), 7)
	Hierarchical syntax tree:

	BinaryMul
		BinaryPlus
			Literal (2)
			Literal (5)
		Literal (7)
