using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Csp.Compiler;

public class DefRewriter : CSharpSyntaxRewriter
{
    private readonly Stack<Dictionary<string, ExpressionSyntax>> _scopes = new();

    public DefRewriter()
    {
        _scopes.Push(new()); // global scope
    }

    // enter block
    public override SyntaxNode? VisitBlock(BlockSyntax node)
    {
        _scopes.Push(new());

        var result = base.VisitBlock(node);

        _scopes.Pop();

        return result;
    }

    // handle def declaration and deletion
    public override SyntaxNode? VisitLocalDeclarationStatement(LocalDeclarationStatementSyntax node)
    {
        var decl = node.Declaration;

        // determine def
        if (decl.Type.ToString() != "def")
            return base.VisitLocalDeclarationStatement(node);

        foreach (var v in decl.Variables)
        {
            if (v.Initializer == null)
                continue;

            var name = v.Identifier.Text;
            var expr = v.Initializer.Value;

            _scopes.Peek()[name] = expr;
        }

        return SyntaxFactory.ParseStatement("//"); // delete def
    }

    // replacement call
    public override SyntaxNode? VisitInvocationExpression(InvocationExpressionSyntax node)
    {
        var visited = (InvocationExpressionSyntax?)base.VisitInvocationExpression(node);
        if (visited?.Expression is not IdentifierNameSyntax id)
            return visited;

        var name = id.Identifier.Text;
        var expr = Resolve(name);

        return expr is null
            ? visited
            : visited.WithExpression((ExpressionSyntax)Visit(expr));
    }

    // Analysis of def (from inside to outside)
    private ExpressionSyntax? Resolve(string name)
    {
        foreach (var scope in _scopes)
        {
            if (scope.TryGetValue(name, out var expr))
                return expr;
        }

        return null;
    }
}