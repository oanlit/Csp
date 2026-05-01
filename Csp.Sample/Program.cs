using Csp.Compiler;

var input = """
            alias hButton = Components.HButton.Create;

            hButton(new(content:"Add"));
            """;

var output = CspPipeline.Transform(input);

Console.WriteLine(output);