using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Csp.Compiler.Rewriter;

public class AliasRewriter(Dictionary<string, ExpressionSyntax> map) : CSharpSyntaxRewriter
{
    public override SyntaxNode VisitInvocationExpression(InvocationExpressionSyntax node)
    {
        if (node.Expression is not IdentifierNameSyntax id)
            return base.VisitInvocationExpression(node)!;

        var name = id.Identifier.Text;
        if (!map.TryGetValue(name, out var expr))
            return base.VisitInvocationExpression(node)!;

        return node.WithExpression(expr);
    }
}

public class AliasStripper : CSharpSyntaxRewriter
{
    public override SyntaxNode? VisitGlobalStatement(GlobalStatementSyntax node)
    {
        if (node.Statement is LocalDeclarationStatementSyntax local
            && local.Declaration.Type.ToString() == "alias")
            return null;

        return base.VisitGlobalStatement(node);
    }
}