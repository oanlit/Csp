using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Csp.Compiler;

public static class CspPipeline
{
    public static string Transform(string source)
    {
        var tree = CSharpSyntaxTree.ParseText(source);
        var root = tree.GetRoot();

        var rewritten = new AliasRewriter().Visit(root);

        return rewritten.NormalizeWhitespace().ToFullString();
    }
}