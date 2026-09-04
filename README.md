# GS1 Digital Link Toolkit

[![CI](https://github.com/duruilhan/gs1-digital-link-toolkit/actions/workflows/ci.yml/badge.svg)](https://github.com/duruilhan/gs1-digital-link-toolkit/actions/workflows/ci.yml)

GS1 Digital Link connects GS1 identifiers, such as GTINs and GLNs, to web-accessible information and services. It allows standardized identifiers carried by barcodes to be used in digital applications without changing their meaning.

This .NET library provides the foundations for working with those identifiers. It calculates and validates GS1 check digits, identifies possible GS1 key types by length, loads Application Identifier (AI) definitions from a JSON catalog, validates AI values against their length, character-set, and check-digit rules, and parses both parenthesized and raw GS1 element strings into ordered AI/value pairs.

It also builds uncompressed Digital Link URLs from validated AI/value pairs for the supported catalog. It is not a complete implementation of every GS1 AI or association rule.

## Build and test

Requirements: [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0) and Git.

```bash
git clone https://github.com/duruilhan/gs1-digital-link-toolkit.git
cd gs1-digital-link-toolkit
dotnet restore
dotnet build --no-restore
dotnet test --no-build
```

Every push and pull request also runs the build and test suite through GitHub Actions.

## Design decisions

### Building a Digital Link

```csharp
using Gs1.DigitalLink;

string url = Gs1DigitalLinkBuilder.Build([
    new Gs1Element("01", "08690504080008"),
    new Gs1Element("10", "LOT123"),
    new Gs1Element("17", "261231")
]);
// https://id.gs1.org/01/08690504080008/10/LOT123?17=261231

string raw = "010869050408000810LOT123\u001D17261231";
string fromBarcode = Gs1DigitalLinkBuilder.Build(Gs1RawElementStringParser.Parse(raw));

IReadOnlyList<Gs1Element> elements = Gs1DigitalLinkParser.Parse(url);
bool parsed = Gs1DigitalLinkParser.TryParse(url, out var safeElements);
```

`Build` optionally accepts a custom absolute HTTP(S) base address, including a path prefix. Credentials, query strings and fragments are rejected in the base address. Invalid elements or combinations throw `ArgumentException`; a null element collection throws `ArgumentNullException`.

The JSON catalog records each AI's `role`. Qualifiers also specify `qualifierFor` and `qualifierOrder`. Exactly one primary key is required. GTIN qualifiers are emitted in the order **22, 10, 21**, regardless of input order; missing qualifiers are skipped. Data attributes are emitted in ordinal AI-code order in the query string for deterministic output. The original input collection is not modified.

Each value is encoded separately with `Uri.EscapeDataString`, so `A/B` becomes `A%2FB` and `50%` becomes `50%25`. The URL's structural separators are not encoded.

### Why a GTIN qualifier cannot accompany a GLN

For example, `414=8690123456789` with `10=LOT123` is rejected. AI 10 qualifies GTIN (01), not GLN (414). Moving it into the query string would silently change its catalog role; adding it to the path would imply an unsupported relationship. The same check applies to all catalog qualifiers and primary keys.

Duplicate AI codes are rejected by the builder, even when the values match, to avoid ambiguous URLs. This is deliberately stricter than the parsers, which preserve repeated elements from their input.

### Parsing a Digital Link

`Gs1DigitalLinkParser` provides `TryParse` for expected invalid input and `Parse` for callers that need a positional `Gs1ParseException`. It finds the first primary-key AI path segment, so deployment prefixes such as `https://example.com/dl/` are allowed. It decodes percent-encoded values and rejects unknown AIs, invalid values, misplaced data attributes, GTIN qualifiers used with another primary key, duplicates, and non-canonical qualifier order.

The exact property `Parse(Build(x)) == x` does not hold when `x` is in a non-canonical order: the builder intentionally reorders GTIN qualifiers and data attributes. The round-trip guarantees are instead:

- `Build(Parse(Build(x))) == Build(x)`
- `Parse(Build(x)) == Canonicalize(x)`

These properties are checked with 1,000 reproducible randomly generated valid element lists using a fixed seed. The generator deliberately varies both element presence and input order. Exact examples and invalid URLs remain as ordinary regression tests.

### Why validation returns `false`

`IsValid` methods return `false` for malformed input instead of throwing exceptions. Invalid barcode data is an expected validation result, especially when processing external input, so callers should not need `try`/`catch` for normal control flow.

### Why unknown AI codes return `false`

The validator accepts only Application Identifiers defined in the catalog. An unknown code has no trusted format or validation rules, so treating it as invalid avoids silently accepting data whose meaning is not known.

### Why the AI catalog is stored as JSON

Application Identifier definitions are data rather than validation logic. Keeping them in `Data/application-identifiers.json` separates the catalog from the code, makes definitions easier to review and extend, and allows the same validation implementation to work with additional AIs.
