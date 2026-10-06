# CT2 unit-test coverage inventory

Checked on 2026-10-06. This inventory compares the 207 top-level `CrypPlugins`
directories with dedicated test classes in `UnitTests`. It is a directory-level
inventory, not a claim that every component, mode, or setting is covered. The
cross-plugin regression classes also test selected behaviors without giving each
plugin its own fixture.

Before this pass, 158 plugin directories had no same-named dedicated test file.
Known-answer tests were added for AffineCipher, BaconCipher, Blowfish, Hill,
RSA, SM4, and TEA. The existing ChaCha vector file was moved into and compiled by the test
project; it previously belonged to no project. The Scytale fixture already
existed but had no active vectors; it now tests both directions. That leaves
150 directories without a dedicated fixture by
this simple file-name or explicit-new-test measure. The count is a triage aid,
not a measure of line or behavior coverage.

## Cipher-related directories still without a dedicated fixture

The list below is limited to directories whose source declares a
`ComponentCategory.Ciphers*` category. It excludes the new and newly activated
tests above.
Some entries are analyzers, visualizations, or cryptographic protocols, so the
appropriate test oracle differs by component. Existing audit regressions may
already cover a narrow behavior of some entries.

| | | |
|---|---|---|
| AESVisualization | BazeriesCipher | BifidCipher |
| BlindSignatureGenerator | BookCipher | |
| Chaocipher | CramerShoup | CryptoBoxCipher |
| CylinderCipher | DGK | ElsieFourCipher |
| FEAL | FormatPreservingEncryption | FourSquareCipher |
| GRANIT160Cipher | GrandpreCipher | HIGHT |
| HagelinMachine | JosseCipher | |
| LorenzSZ42 | M138 | M209Analyzer |
| Magma | MexicanArmyCipherDisk | Paillier |
| SAES | SDES | |
| Solitaire | SpanishStripCipher | StraddlingCheckerboard |
| Substitution | SyllabaryCipher | SymbolCipher |
| T310 | TrifidCipher | Typex |
| Ubchi | VisualCryptography | XOR |

Prioritize independent published or independently calculated known-answer vectors
for the actual algorithms. For analyzers and visualizations, use small fixed
inputs and assert meaningful results or state. A load-only smoke test does not
establish cipher correctness.

## Existing template coverage gate

`EveryConcreteComponentHasATemplate` still reports six concrete components
without a workspace template: `Alphabet`, `BooleanNot`,
`ByteArrayOperations`, `CLK`, `FileOutputClass`, and `SymbolCipher`.
The test remains a failing coverage gate until usable templates are added
for those components. Abstract component types are excluded from the audit.

## Running the current tests

Build the `UnitTests/UnitTests.csproj` project with `Debug|x64` or `Release|x64`
using Visual Studio MSBuild, then run `CrypBuild/<configuration>/UnitTests.dll`
with Visual Studio Test Platform. Build the plugin projects under test first:
stale assemblies in `CrypBuild` can give misleading failures. The full solution
currently also requires restoring the NuGet packages listed by CrypLLM.
