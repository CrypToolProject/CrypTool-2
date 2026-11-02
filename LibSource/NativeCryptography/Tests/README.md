# Native CBC regression tests

Build `NativeCryptography.vcxproj` in Release/x64, then run:

```powershell
powershell -ExecutionPolicy Bypass -File .\LibSource\NativeCryptography\Tests\Run-CbcRegression.ps1
```

`-NativeDll` accepts an alternative DLL for checking an existing installation.
The runner requires the 64-bit .NET Framework 4 C# compiler and returns an error
if any case fails. It does not modify the supplied DLL.

The 48 cases compare native CBC decryption with plaintext encrypted independently
by .NET's AES, DES, TripleDES and RC2 providers. They cover one, two and seven blocks,
zero and nonzero IVs, and null IVs where supported by the native API. They also
check that input, key and IV buffers remain unchanged and can be reused.

AES tests deliberately call the software entry points rather than selecting a
backend based on CPU capabilities. This catches the incomplete XOR regression
even on a machine that supports AES-NI.
