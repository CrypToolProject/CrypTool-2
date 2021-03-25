This Visual Decoder is a fork of the original decoder. It contains an OCR-decoder, which may cause memory leaks.
 Since there are also were some changes in order to embedd the decoder - that may be usefull later - 
 this fork has been comitted into the experimental folder.

 In order to get this fork run, you have to do some changes.

 1. replace the startup tag in CryptWin/app.config with the following to enable legacy-support and legacy-exeptionhandling.

  <startup useLegacyV2RuntimeActivationPolicy="true">
    <legacyCorruptedStateExceptionsPolicy enabled="true"/>
	<supportedRuntime version="v4.0" sku=".NETFramework,Version=v4.0"/>
  </startup>

  2. copy the ./tessdata folder into CryptBild/Lib 


