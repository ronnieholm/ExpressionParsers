# TODO

- Add AST node
- Add AST tree-walking evaluators implementing visitor pattern: Value, prefix, postfix, parens, graphics files
- Add line and column to token
- Create large file of expressions and profile evaluating it
- Add mathematical simplification steps
- Add visitor pattern. Pattern isn't about tree traversal but double dispatch,
  ensuring every AST node has associated code to evaluate it during travel. We
  could do without the visitor pattern. FP languages don't use it at as they can
  exhaustively match on AST types
- Add parser which doesn't use Pratt but doesn't repeat methods either.
