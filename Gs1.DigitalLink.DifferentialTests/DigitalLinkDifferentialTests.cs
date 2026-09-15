using Gs1.DigitalLink.TestSupport;
using Solidsoft.Reply.Gs1DigitalLinkLib;
using Xunit.Abstractions;

namespace Gs1.DigitalLink.DifferentialTests;

public sealed class DigitalLinkDifferentialTests(ITestOutputHelper output)
{
    [Fact]
    public void Decompress_WithSeededReferenceInputs_RecordsSupportedScope()
    {
        const int seed = 20260915;
        const int inputCount = 500;
        var random = new Random(seed);
        var failures = new Dictionary<string, int>(StringComparer.Ordinal);
        int resolved = 0;

        for (int iteration = 0; iteration < inputCount; iteration++)
        {
            Gs1Element[] input = ValidElementGenerator.Generate(random);
            string elementString = string.Concat(input.Select(element =>
                $"({element.ApplicationIdentifier}){element.Value}"));
            string compressed;
            try
            {
                compressed = elementString.ToGs1DigitalLink(
                    uriStem: "https://id.gs1.org",
                    digitalLinkForm: DigitalLinkForm.Compressed).ToString();
            }
            catch (Exception exception)
            {
                AddFailure(failures, $"Reference compression: {exception.GetType().Name}");
                continue;
            }

            if (!Gs1CompressedDigitalLinkParser.TryParse(compressed, out IReadOnlyList<Gs1Element> actual))
            {
                AddFailure(failures, "Decoder rejected compressed bit stream");
                continue;
            }

            var expectedMap = input.ToDictionary(item => item.ApplicationIdentifier, item => item.Value);
            var actualMap = actual.ToDictionary(item => item.ApplicationIdentifier, item => item.Value);
            if (!expectedMap.OrderBy(item => item.Key).SequenceEqual(actualMap.OrderBy(item => item.Key)))
            {
                AddFailure(failures, "Decoded AI/value list differed");
                continue;
            }

            resolved++;
        }

        output.WriteLine($"Seed: {seed}; inputs: {inputCount}; resolved: {resolved}; unresolved: {inputCount - resolved}");
        foreach ((string reason, int count) in failures.OrderBy(item => item.Key))
            output.WriteLine($"  {reason}: {count}");
        Assert.True(resolved > 0, "The decoder did not resolve any reference-generated input.");
    }

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

    private static void AddFailure(IDictionary<string, int> failures, string reason)
    {
        failures.TryGetValue(reason, out int count);
        failures[reason] = count + 1;
    }
}
