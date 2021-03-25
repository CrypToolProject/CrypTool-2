Format-Preserving Encryption
============================

   This package implements the two methods for format-preserving encryption
   specified in NIST Special Publication 800-38G, Recommendation for Block
   Cipher Modes of Operation: Methods for Format-Preserving Encryption.

Distribution
------------

   The source code for this package is available from SourceForge:
   https://sourceforge.net/p/format-preserving-encryption/

   This package uses the AES cipher with keys of 128, 192 or 256 bits. To use
   key sizes larger than 128 bits with the Oracle JDK you must install the
   "Unlimited Strength Jurisdiction Policy Files." The policy files can be found
   at the same place as the JDK download:
   http://www.oracle.com/technetwork/java/javase/downloads/index.html.

Export restrictions
-------------------

   This distribution includes cryptographic software. The country in which you
   currently reside may have restrictions on the import, possession, use, and/or
   re-export to another country, of encryption software. BEFORE using any
   encryption software, please check your country's laws, regulations and
   policies concerning the import, possession, or use, and re-export of
   encryption software, to see if this is permitted. See
   <http://www.wassenaar.org/> for more information.

   The U.S. Government Department of Commerce, Bureau of Industry and Security
   (BIS), has classified this software as Export Commodity Control Number (ECCN)
   5D002.C.1, which includes information security software using or performing
   cryptographic functions with symmetric algorithms employing a key length in
   excess of 56-bits. The form and manner of this software distribution makes it
   eligible for export under the License Exception ENC Technology Software
   Unrestricted (TSU) exception (see the BIS Export Administration Regulations,
   Section 740.13) for both object code and source code.

License
-------

   Copyright (c) 2016 Weydstone LLC dba Sutton Abinger
   
   See the NOTICE file distributed with this work for additional information
   regarding copyright ownership. Sutton Abinger licenses this file to you under
   the Apache License, Version 2.0 (the "License"); you may not use this file
   except in compliance with the License. You may obtain a copy of the License
   at http://www.apache.org/licenses/LICENSE-2.0
   
   Unless required by applicable law or agreed to in writing, software
   distributed under the License is distributed on an "AS IS" BASIS, WITHOUT
   WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the
   License for the specific language governing permissions and limitations under
   the License.

Acknowledgements
----------------

   The author would like to thank Morris Dworkin and the Computer Security
   Division, Information Technology Laboratory at National Institute of
   Standards and Technology for their kind assistance in interpreting the
   specification.

Additional Resources
--------------------

   NIST Special Publication 800-38G, "Recommendation for Block Cipher Modes of
   Operation: Methods for Format-Preserving Encryption" is available free of
   charge from: http://dx.doi.org/10.6028/NIST.SP.800-38G.
   
   Sample data for FF1 and FF3 are available at the examples page on NIST’s
   Computer Security Resource Center website:
   http://csrc.nist.gov/groups/ST/toolkit/examples.html.
