using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Csp.Compiler;

public class DefRewriter : CSharpSyntaxRewriter
{
    private readonly Stack<Dictionary<string, ExpressionSyntax>> _scopes = new();
    private readonly HashSet<string> _resolving = new();

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
            if (ContainsUnresolvedAlias(expr))
                throw CreateError($"Circular alias detected: '{name}'", node);

            _scopes.Peek()[name] = expr;
        }

        return SyntaxFactory.ParseStatement("//"); // delete def
    }

    public override SyntaxNode? VisitIdentifierName(IdentifierNameSyntax node)
    {
        var name = node.Identifier.Text;
        var expr = Resolve(name);

        if (expr is null)
            return base.VisitIdentifierName(node);

        if (_resolving.Contains(name))
            throw CreateError($"Circular alias detected: '{name}'", node);

        _resolving.Add(name);

        var result = Visit(expr);

        _resolving.Remove(name);

        return result;
    }

    public override SyntaxNode? VisitInvocationExpression(InvocationExpressionSyntax node)
    {
        var visited = (InvocationExpressionSyntax?)base.VisitInvocationExpression(node);

        if (visited is null)
            return null;

        // Only handle cases where the expression is a lambda and is not wrapped.
        if (visited.Expression is not LambdaExpressionSyntax lambda
            || lambda.Parent is ParenthesizedExpressionSyntax)
            return visited;

        return visited.WithExpression(
            SyntaxFactory.ParenthesizedExpression(lambda)
        );
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

    private bool IsAliasName(string name)
    {
        foreach (var scope in _scopes)
        {
            if (scope.TryGetValue(name, out _))
                return true;
        }

        return false;
    }

    private bool ContainsUnresolvedAlias(ExpressionSyntax expr)
    {
        var identifiers = expr.DescendantNodesAndSelf()
            .OfType<IdentifierNameSyntax>();

        foreach (var id in identifiers)
        {
            if (Resolve(id.Identifier.Text) is not null)
                continue;

            if (IsAliasName(id.Identifier.Text))
                return true;
        }

        return false;
    }

    private Exception CreateError(string message, SyntaxNode node)
    {
        var span = node.GetLocation().GetLineSpan();
        var line = span.StartLinePosition.Line + 1;
        var col = span.StartLinePosition.Character + 1;

        return new Exception($"{message} (Line {line}, Column {col})");
    }
}