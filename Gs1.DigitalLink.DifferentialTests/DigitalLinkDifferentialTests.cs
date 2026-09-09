using Gs1.DigitalLink.TestSupport;
using Solidsoft.Reply.Gs1DigitalLinkLib;
using Xunit.Abstractions;

namespace Gs1.DigitalLink.DifferentialTests;

public sealed class DigitalLinkDifferentialTests(ITestOutputHelper output)
{
    [Fact]
    public void Build_WithSeededInputs_RecordsDifferencesFromReferencePort()
    {
        const int seed = 20260908;
        const int inputCount = 2_000;
        var random = new Random(seed);
        var differences = new List<string>();

        for (int iteration = 0; iteration < inputCount; iteration++)
        {
            Gs1Element[] elements = ValidElementGenerator.Generate(random);
            string elementString = string.Concat(elements.Select(element =>
                $"({element.ApplicationIdentifier}){element.Value}"));
            string ours = Gs1DigitalLinkBuilder.Build(elements);

            try
            {
                string reference = elementString
                    .ToGs1DigitalLink(uriStem: "https://id.gs1.org")
                    .ToString();

                if (!string.Equals(ours, reference, StringComparison.Ordinal))
                {
                    differences.Add(
                        $"Iteration {iteration}: {elementString}{Environment.NewLine}" +
                        $"  Ours:      {ours}{Environment.NewLine}" +
                        $"  Reference: {reference}");
                }
            }
            catch (Exception exception)
            {
                differences.Add(
                    $"Iteration {iteration}: {elementString}{Environment.NewLine}" +
                    $"  Ours:      {ours}{Environment.NewLine}" +
                    $"  Reference error: {exception.GetType().Name}: {exception.Message}");
            }
        }

        output.WriteLine($"Seed: {seed}; inputs: {inputCount}; differences: {differences.Count}");
        foreach (string difference in differences)
            output.WriteLine(difference);

        // A difference is a diagnostic finding, not a regression failure.
    }
}
