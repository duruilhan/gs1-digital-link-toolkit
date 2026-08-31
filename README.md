# GS1 Digital Link Toolkit

[![CI](https://github.com/duruilhan/gs1-digital-link-toolkit/actions/workflows/ci.yml/badge.svg)](https://github.com/duruilhan/gs1-digital-link-toolkit/actions/workflows/ci.yml)

GS1 Digital Link connects GS1 identifiers, such as GTINs and GLNs, to web-accessible information and services. It allows standardized identifiers carried by barcodes to be used in digital applications without changing their meaning.

This .NET library provides the foundations for working with those identifiers. It calculates and validates GS1 check digits, identifies possible GS1 key types by length, loads Application Identifier (AI) definitions from a JSON catalog, and validates AI values against their length, character-set, and check-digit rules.

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

### Why validation returns `false`

`IsValid` methods return `false` for malformed input instead of throwing exceptions. Invalid barcode data is an expected validation result, especially when processing external input, so callers should not need `try`/`catch` for normal control flow.

### Why unknown AI codes return `false`

The validator accepts only Application Identifiers defined in the catalog. An unknown code has no trusted format or validation rules, so treating it as invalid avoids silently accepting data whose meaning is not known.

### Why the AI catalog is stored as JSON

Application Identifier definitions are data rather than validation logic. Keeping them in `Data/application-identifiers.json` separates the catalog from the code, makes definitions easier to review and extend, and allows the same validation implementation to work with additional AIs.
