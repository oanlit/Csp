using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

public class AliasRewriter : CSharpSyntaxRewriter
{
    private readonly Stack<Dictionary<string, ExpressionSyntax>> _scopes = new();

    public AliasRewriter()
    {
        _scopes.Push(new()); // global scope
    }

    // ✔ 进入 block
    public override SyntaxNode? VisitBlock(BlockSyntax node)
    {
        _scopes.Push(new());

        var result = base.VisitBlock(node);

        _scopes.Pop();

        return result;
    }

    // handle alias declaration and deletion
    public override SyntaxNode? VisitLocalDeclarationStatement(LocalDeclarationStatementSyntax node)
    {
        var decl = node.Declaration;

        // determine alias
        if (decl.Type.ToString() != "alias")
            return base.VisitLocalDeclarationStatement(node);

        foreach (var v in decl.Variables)
        {
            if (v.Initializer == null)
                continue;

            var name = v.Identifier.Text;
            var expr = v.Initializer.Value;

            _scopes.Peek()[name] = expr;
        }
        return SyntaxFactory.ParseStatement("//"); // delete alias
    }

    // replacement call
    public override SyntaxNode? VisitInvocationExpression(InvocationExpressionSyntax node)
    {
        if (node.Expression is not IdentifierNameSyntax id)
            return base.VisitInvocationExpression(node);

        var name = id.Identifier.Text;
        var expr = Resolve(name);

        return expr is null
            ? base.VisitInvocationExpression(node)
            : node.WithExpression(expr);
    }

    // Analysis of alias (from inside to outside)
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