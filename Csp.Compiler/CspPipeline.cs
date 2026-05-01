using Microsoft.CodeAnalysis;
using Csp.Compiler.Rewriter;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Csp.Compiler;

public static class CspPipeline
{
    public static string Transform(string source)
    {
        var tree = CSharpSyntaxTree.ParseText(source);
        var root = tree.GetRoot();

        // 1. collect alias
        var aliases = CollectAliases(root);

        // 2. rewrite
        var rewritten = new AliasRewriter(aliases).Visit(root);
        
        // 3. strip
        rewritten = new AliasStripper().Visit(rewritten);

        return rewritten.ToFullString();
    }

    static Dictionary<string, ExpressionSyntax> CollectAliases(SyntaxNode root)
    {
        var map = new Dictionary<string, ExpressionSyntax>();

        foreach (var node in root.DescendantNodes())
        {
            if (node is not GlobalStatementSyntax gs)
                continue;

            if (gs.Statement is not LocalDeclarationStatementSyntax local)
                continue;

            var decl = local.Declaration;

            if (decl.Type.ToString() != "alias")
                continue;

            foreach (var variable in decl.Variables)
            {
                var name = variable.Identifier.Text;

                if (variable.Initializer == null)
                    continue;

                var expr = variable.Initializer.Value;

                map[name] = expr;
            }
        }

        return map;
    }
}