using Csp.Compiler;

if (args.Length == 0)
{
    Console.WriteLine("Usage: csp <input.csp> [-o output]");
    return;
}

string? inputPath = null;
string? outputPath = null;

for (var i = 0; i < args.Length; i++)
{
    if (args[i] == "-o" && i + 1 < args.Length)
    {
        outputPath = args[i + 1];
        i++;
    }
    else
    {
        inputPath = args[i];
    }
}

if (inputPath == null || !File.Exists(inputPath))
{
    Console.WriteLine($"File not found: {inputPath}");
    return;
}

var source = File.ReadAllText(inputPath);

var output = CspPipeline.Transform(source);

if (outputPath == null)
{
    outputPath = Path.ChangeExtension(inputPath, ".cs");
}
else if (Directory.Exists(outputPath))
{
    var fileName = Path.GetFileNameWithoutExtension(inputPath) + ".cs";
    outputPath = Path.Combine(outputPath, fileName);
}

File.WriteAllText(outputPath, output);

Console.WriteLine($"Generated: {outputPath}");
