/*                              
   Copyright 2025 Nils Kopal, CrypTool Project

   Licensed under the Apache License, Version 2.0 (the "License");
   you may not use this file except in compliance with the License.
   You may obtain a copy of the License at

       http://www.apache.org/licenses/LICENSE-2.0

   Unless required by applicable law or agreed to in writing, software
   distributed under the License is distributed on an "AS IS" BASIS,
   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
   See the License for the specific language governing permissions and
   limitations under the License.
*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace CrypTool.Plugins.EllipticCurveCryptography
{
    /// <summary>
    /// Implemented types of elliptic curves
    /// </summary>
    public enum CurveType
    {
        Weierstraß,
        Montgomery,
        TwistedEdwards
    }

    /// <summary>
    /// A set of predefined elliptic curves
    /// </summary>
    public static class PredefinedEllipticCurves
    {
        /// <summary>
        /// A definition for an elliptic curve
        /// </summary>
        public struct EllipticCurveDefinition
        {
            public readonly string Name;
            public readonly BigInteger A;
            public readonly BigInteger B;
            public readonly BigInteger D;
            public readonly BigInteger P;
            public readonly BigInteger Gx;
            public readonly BigInteger Gy;
            public readonly CurveType CurveType;

            /// <summary>
            /// Constructor
            /// </summary>
            /// <param name="name"></param>
            /// <param name="aHex"></param>
            /// <param name="bHex"></param>
            /// <param name="dHex"></param>
            /// <param name="pHex"></param>
            /// <param name="gxHex"></param>
            /// <param name="gyHex"></param>
            /// <param name="curveType"></param>
            public EllipticCurveDefinition(string name, string aHex, string bHex, string dHex,
                                   string pHex, string gxHex, string gyHex, CurveType curveType)
            {
                Name = name;
                A = ParseStringToBiginteger(aHex);
                B = ParseStringToBiginteger(bHex);
                D = ParseStringToBiginteger(dHex);
                P = ParseStringToBiginteger(pHex);
                Gx = ParseStringToBiginteger(gxHex);
                Gy = ParseStringToBiginteger(gyHex);
                CurveType = curveType;
            }

            /// <summary>
            /// Parses the given string to an integer
            /// </summary>
            /// <param name="hex"></param>
            /// <returns></returns>
            public static BigInteger ParseStringToBiginteger(string input)
            {
                if (input.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
                {
                    string hex = input.Substring(2);

                    if (hex.Length % 2 != 0)
                    {
                        hex = "0" + hex;
                    }

                    byte[] bigEndianBytes = Enumerable.Range(0, hex.Length / 2)
                        .Select(i => Convert.ToByte(hex.Substring(i * 2, 2), 16))
                        .ToArray();

                    byte[] littleEndianBytes = bigEndianBytes.Reverse().ToArray();

                    if (littleEndianBytes[littleEndianBytes.Length - 1] >= 0x80)
                    {
                        Array.Resize(ref littleEndianBytes, littleEndianBytes.Length + 1);
                    }

                    return new BigInteger(littleEndianBytes);
                }

                return BigInteger.Parse(input);
            }
        }

        /// <summary>
        /// Lookup table for predefined curves
        /// </summary>
        public static readonly Dictionary<string, EllipticCurveDefinition> Curves
            = new Dictionary<string, EllipticCurveDefinition>(StringComparer.OrdinalIgnoreCase)
            {
                // NIST Prime Curves:
                ["secp192r1"] = new EllipticCurveDefinition(
                "secp192r1",
                "0xFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFEFFFFFFFFFFFFFFFC",
                "0x64210519E59C80E70FA7E9AB72243049FEB8DEECC146B9B1",
                "0x0",
                "0xFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFEFFFFFFFFFFFFFFFF",
                "0x188DA80EB03090F67CBF20EB43A18800F4FF0AFD82FF1012",
                "0x07192B95FFC8DA78631011ED6B24CDD573F977A11E794811",
                CurveType.Weierstraß),

                ["secp224r1"] = new EllipticCurveDefinition(
                "secp224r1",
                "0xFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFEFFFFFFFFFFFFFFFEFFFFFFFFFFFFFFFC",
                "0xB4050A850C04B3ABF54132565044B0B7D7BFD8BA270B39432355FFB4",
                "0x0",
                "0xFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFF000000000000000000000001",
                "0xB70E0CBD6BB4BF7F321390B94A03C1D356C21122343280D6115C1D21",
                "0xBD376388B5F723FB4C22DFE6CD4375A05A07476444D5819985007E34",
                CurveType.Weierstraß),

                ["secp256r1"] = new EllipticCurveDefinition(
                "secp256r1",
                "0xFFFFFFFF00000001000000000000000000000000FFFFFFFFFFFFFFFFFFFFFFFC",
                "0x5AC635D8AA3A93E7B3EBBD55769886BC651D06B0CC53B0F63BCE3C3E27D2604B",
                "0x0",
                "0xFFFFFFFF00000001000000000000000000000000FFFFFFFFFFFFFFFFFFFFFFFF",
                "0x6B17D1F2E12C4247F8BCE6E563A440F277037D812DEB33A0F4A13945D898C296",
                "0x4FE342E2FE1A7F9B8EE7EB4A7C0F9E162BCE33576B315ECECBB6406837BF51F5",
                CurveType.Weierstraß),

                ["secp384r1"] = new EllipticCurveDefinition(
                "secp384r1",
                "0xFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFEFFFFFFFF0000000000000000FFFFFFFC",
                "0xB3312FA7E23EE7E4988E056BE3F82D19181D9C6EFE8141120314088F5013875AC656398D8A2ED19D2A85C8EDD3EC2AEF",
                "0x0",
                "0xFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFEFFFFFFFF0000000000000000FFFFFFFF",
                "0xAA87CA22BE8B05378EB1C71EF320AD746E1D3B628BA79B9859F741E082542A385502F25DBF55296C3A545E3872760AB7",
                "0x3617DE4A96262C6F5D9E98BF9292DC29F8F41DBD289A147CE9DA3113B5F0B8C00A60B1CE1D7E819D7A431D7C90EA0E5F",
                CurveType.Weierstraß),

                ["secp521r1"] = new EllipticCurveDefinition(
                "secp521r1",
                "0x01FFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFC",
                "0x0051953EB9618E1C9A1F929A21A0B68540EEA2DA725B99B315F3B8B489918EF109E156193951EC7E937B1652C0BD3BB1BF073573DF883D2C34F1EF451FD46B503F00",
                "0x0",
                "0x01FFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFF",
                "0x00C6858E06B70404E9CD9E3ECB662395B4429C648139053FB521F828AF606B4D3DBAA14B5E77EFE75928FE1DC127A2FFA8DE3348B3C1856A429BF97E7E31C2E5BD66",
                "0x011839296A789A3BC0045C8A5FB42C7D1BD998F54449579B446817AFBD17273E662C97EE72995EF42640C550B9013FAD0761353C7086A272C24088BE94769FD16650",
                CurveType.Weierstraß),

                //SECG Koblitz Curves (incl. Bitcoin):
                ["secp160k1"] = new EllipticCurveDefinition(
                "secp160k1",
                "0x0000000000000000000000000000000000000000",
                "0x0000000000000000000000000000000000000007",
                "0x0",
                "0xFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFEFFFFAC73",
                "0x3B4C382CE37AA192A4019E763036F4F5DD4D7EBB",
                "0x938CF935318FDCED6BC28286531733C3F03C4FEE",
                CurveType.Weierstraß),

                ["secp192k1"] = new EllipticCurveDefinition(
                "secp192k1",
                "0x000000000000000000000000000000000000000000000000",
                "0x000000000000000000000000000000000000000000000003",
                "0x0",
                "0xfffffffffffffffffffffffffffffffffffffffeffffee37",
                "0xdb4ff10ec057e9ae26b07d0280b7f4341da5d1b1eae06c7d",
                "0x9b2f2f6d9c5628a7844163d015be86344082aa88d95e2f9d",
                CurveType.Weierstraß),

                ["secp224k1"] = new EllipticCurveDefinition(
                "secp224k1",
                "0x0",
                "0x5",
                "0x0",
                "0xFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFEFFFFE56D",
                "0xA1455B334DF099DF30FC28A169A467E9E47075A90F7E650EB6B7A45C",
                "0x7E089FED7FBA344282CAFBD623D5A23E0E0FF77500DB995E5C9E483E",
                CurveType.Weierstraß),

                ["secp256k1"] = new EllipticCurveDefinition(
                "secp256k1",
                "0x0",
                "0x7",
                "0x0",
                "0xFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFEFFFFFC2F",
                "0x79BE667EF9DCBBAC55A06295CE870B07029BFCDB2DCE28D959F2815B16F81798",
                "0x483ADA7726A3C4655DA4FBFC0E1108A8FD17B448A68554199C47D08FFB10D4B8",
                CurveType.Weierstraß),

                // Brainpool:
                ["brainpoolP256r1"] = new EllipticCurveDefinition(
                "brainpoolP256r1",
                "0x7D5A0975FC2C3057EEF67530417AFFE7FB8055C126DC5C6CE94A4B44F330B5D9",
                "0x26DC5C6CE94A4B44F330B5DB0E082F1F2F3F3F9686A7B6A53D0B7C4E15D213BE",
                "0x0",
                "0xA9FB57DBA1EEA9BC3E660A909D838D726E3BF623D52620282013481D1F6E5377",
                "0x8BD2AEB9CB7E57CB2C4B482FFC81B7AFB9DE27E1E3BD23C23A4453BD9ACE3262",
                "0x547EF835C3DAC4FD97F8461A14611DC9C27745132DED8E545C1D54C72F046997",
                CurveType.Weierstraß),

                // Montgomery curve (DH)
                ["curve25519"] = new EllipticCurveDefinition(
                "curve25519",
                "0x76D06",
                "0x1",
                "0x0",
                "0x7FFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFED",
                "0x9",
                "0x20ae19a1b8a086b4e01edd2c7748d14c923d4d7e6d7c61b229e9c5a27eced3d9",
                CurveType.Montgomery),

                // Twisted Edwards curve (Signatures)
                ["ed25519"] = new EllipticCurveDefinition(
                "ed25519",
                "0x7fffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffec",
                "0x0",
                "0x52036cee2b6ffe738cc740797779e89800700a4d4141d8ab75eb4dca135978a3",
                "0x7FFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFED",
                "0x216936D3CD6E53FEC0A4E231FDD6DC5C692CC7609525A7B2C9562D608F25D51A",
                "0x6666666666666666666666666666666666666666666666666666666666666658",
                CurveType.TwistedEdwards),

                ["sm2p256v1"] = new EllipticCurveDefinition(
                "sm2p256v1",
                "0xFFFFFFFEFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFF00000000FFFFFFFFFFFFFFFC",
                "0x28E9FA9E9D9F5E344D5A9E4BCF6509A7F39789F515AB8F92DDBCBD414D940E93",
                "0x0",
                "0xFFFFFFFEFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFF00000000FFFFFFFFFFFFFFFF",
                "0x32C4AE2C1F1981195F9904466A39C9948FE30BBFF2660BE1715A4589334C74C7",
                "0xBC3736A2F4F6779C59BDCEE36B692153D0A9877CC62A474002DF32E52139F0A0",
                CurveType.Weierstraß),

                ["gostR3410_2001_CryptoPro_A"] = new EllipticCurveDefinition(
                "gostR3410_2001_CryptoPro_A",
                "0xFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFD94",
                "0xA6",
                "0x0",
                "0xFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFD97",
                "0x8D91E471E0989186AC415A5CA3E6BDC6C8E71DDBEF1DDBD685A9BBAA8A83355C",
                "0x4D940A82462557ACB88B3B2802B6BB9C6CEF5A44A171ABEF1FAFCCA5D20A78C9",
                CurveType.Weierstraß),

                ["gostR3410_2001_CryptoPro_B"] = new EllipticCurveDefinition(
                "gostR3410_2001_CryptoPro_B",
                "0xFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFD94",
                "0xA6",
                "0x0",
                "0xFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFD97",
                "0x01",
                "0x8D91E471E098BC378E1A9272DA46B1D3FEC62C36C1A2A4B7C7FE649CE85820F7",
                CurveType.Weierstraß),

                ["gostR3410_2001_CryptoPro_C"] = new EllipticCurveDefinition(
                "gostR3410_2001_CryptoPro_C",
                "0xFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFD94",
                "0x7A1F0F9F3B8E7C4CF57F4B1B71DA8C6DCE70A7E21D5A9F5CE835C3FE97E2331E",
                "0x0",
                "0xFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFD97",
                "0x01",
                "0xA54829B1A99D07BBFB5ABCB0A0B8B8D195C8B5F5E6A3DA1A73C73092CEC85480",
                CurveType.Weierstraß),

                ["tc26_Gost_3410_12_256_paramSetA"] = new EllipticCurveDefinition(
                "tc26_Gost_3410_12_256_paramSetA",
                "0xFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFD94",
                "0xA6",
                "0x0",
                "0xFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFD97",
                "0x1",
                "0x8D91E471E0989186ACB35F6CF8FF69A40AD92D272207BB34F48F70FD22E70255",
                CurveType.Weierstraß),

                ["tc26_Gost_3410_12_512_paramSetA"] = new EllipticCurveDefinition(
                "tc26_Gost_3410_12_512_paramSetA",
                "0x8E20FAA2348C9C2289F21FF1DFEF65BBE717ACEB835DFB5A5BEA9909B2F6A4DC1E2E40BAE7D8E8F62542A3D2407DF6071D0B17020E96F4320A1C1C4B3FE0F31B",
                "0x33D5AED7FF412C5CE4D712ACF04A0ABA7D751B2EA72DB95D3A91B4B9C4F4DF0308FDC6AF254F5AC9E86BCEB06CA8ECCF2AEC9F5B9EFB1E4C3DF1A56EEC99B324",
                "0x0",
                "0xFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFD97",
                "0x3",
                "0xC6858E06B70404E9CD9E3ECB662395B4429C648139053FB521F828AF606B4D3DBAA14B5E77EFE75928FE1DC127A2FFA8DE3348B3C1856A429BF97E7E31C2E5BD66",
                CurveType.Weierstraß),

                ["FRP256v1"] = new EllipticCurveDefinition(
                "FRP256v1",
                "0x7",
                "0x5FBFF498AA938CE739B8E022FBAFEF9FE13D2BD4A10A880F152E6B7EB0F6FB71",
                "0x0",
                "0xFFFFFFFF00000001000000000000000000000000FFFFFFFFFFFFFFFFFFFFFFFF",
                "0x79A7ECAF1B9DCCE24A73AFA6AD84CCF8A87FD16F7CA1C6D5E7BF5B50FD55A0A",
                "0x1C232F1B3B5A3F9A8C568E6EA4111E13D40F352AE433FBAD1B3D5EBE7F3F904F",
                CurveType.Weierstraß),
            };
    }
}