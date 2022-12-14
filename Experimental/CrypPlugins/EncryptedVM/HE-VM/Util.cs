using System;
using System.Collections.Generic;
using System.IO;

using Microsoft.Research.SEAL;

namespace CrypTool.Plugins.EncryptedVM
{
    class Util
    {
        private EncryptionParameters encparms;
        private BinaryEncoder balencoder;
        private Encryptor encryptor;
        private Evaluator evaluator;
        private Decryptor decryptor;
        private BigPolyArray pubkey;
        private BigPoly seckey;
        private EvaluationKeys evalkeys;
        public int numofreencrypts { get; private set; } = 0;

        public Util(EncryptionParameters p_encparms = null, BigPolyArray p_pubkey = null, EvaluationKeys p_evalkeys = null, BigPoly p_seckey = null)
        {
            if (p_encparms == null)
            {
                encparms = new EncryptionParameters();
                encparms.PolyModulus.Set("1x^1024 + 1");
                encparms.CoeffModulus.Set(ChooserEvaluator.DefaultParameterOptions[1024]);
                encparms.PlainModulus.Set(2);
                encparms.DecompositionBitCount = 24;
            }
            else
                encparms = p_encparms;

            if (p_pubkey == null && p_evalkeys == null && p_seckey == null)
            {
                KeyGenerator keygen = new KeyGenerator(encparms);
                keygen.Generate(10);
                pubkey = keygen.PublicKey;
                seckey = keygen.SecretKey;
                evalkeys = keygen.EvaluationKeys;
            }

            if (p_pubkey != null)
                pubkey = p_pubkey;
            if (p_evalkeys != null)
                evalkeys = p_evalkeys;
            if (p_seckey != null)
                seckey = p_seckey;

            balencoder = new BinaryEncoder(encparms.PlainModulus);

            if (pubkey != null)
                encryptor = new Encryptor(encparms, pubkey);
            if (evalkeys != null)
                evaluator = new Evaluator(encparms, evalkeys);
            if (seckey != null)
                decryptor = new Decryptor(encparms, seckey);
        }

        public Util(Stream p_encparms = null, Stream p_pubkey = null, Stream p_evalkeys = null, Stream p_seckey = null)
        {
            encparms = new EncryptionParameters();
            if (p_encparms == null)
            {
                encparms.PolyModulus.Set("1x^1024 + 1");
                encparms.CoeffModulus.Set(ChooserEvaluator.DefaultParameterOptions[1024]);
                encparms.PlainModulus.Set(2);
                encparms.DecompositionBitCount = 24;
            }
            else
                encparms.Load(p_encparms);

            if (p_pubkey == null && p_evalkeys == null && p_seckey == null)
            {
                KeyGenerator keygen = new KeyGenerator(encparms);
                keygen.Generate(10);
                pubkey = keygen.PublicKey;
                seckey = keygen.SecretKey;
                evalkeys = keygen.EvaluationKeys;
            }

            if (p_pubkey != null)
            {
                pubkey = new BigPolyArray();
                pubkey.Load(p_pubkey);
            }
            if (p_evalkeys != null)
            {
                evalkeys = new EvaluationKeys();
                evalkeys.Load(p_evalkeys);
            }
            if (p_seckey != null)
            {
                seckey = new BigPoly();
                seckey.Load(p_seckey);
            }

            balencoder = new BinaryEncoder(encparms.PlainModulus);

            if (pubkey != null)
                encryptor = new Encryptor(encparms, pubkey);
            if (evalkeys != null)
                evaluator = new Evaluator(encparms, evalkeys);
            if (seckey != null)
                decryptor = new Decryptor(encparms, seckey);
        }

        ~Util()
        { }

        public BigPoly encode(int value)
        {
            return balencoder.Encode(value);
        }

        public BigPoly[] encode(int[] values)
        {
            BigPoly[] polys = new BigPoly[values.Length];

            for (int i = 0; i < values.Length; ++i)
                balencoder.Encode(values[i], polys[i]);

            return polys;
        }

        public int decode(BigPoly poly)
        {
            return Math.Abs(balencoder.DecodeInt32(poly));
        }

        public int[] decode(BigPoly[] polys)
        {
            int[] values = new int[polys.Length];

            for (int i = 0; i < polys.Length; ++i)
                values[i] = Math.Abs(balencoder.DecodeInt32(polys[i]));

            return values;
        }

        public BigPolyArray encrypt(BigPoly poly)
        {
            return encryptor.Encrypt(poly);
        }

        public BigPolyArray[] encrypt(BigPoly[] polys)
        {
            BigPolyArray[] encr = new BigPolyArray[polys.Length];

            for (int i = 0; i < polys.Length; ++i)
                encr[i] = encryptor.Encrypt(polys[i]);

            return encr;
        }

        public BigPoly decrypt(BigPolyArray poly)
        {
            return decryptor.Decrypt(poly);
        }

        public BigPoly[] decrypt(BigPolyArray[] polys)
        {
            BigPoly[] decr = new BigPoly[polys.Length];

            for (int i = 0; i < polys.Length; ++i)
                decr[i] = decryptor.Decrypt(polys[i]);

            return decr;
        }

        public BigPolyArray enc_enc(int value)
        {
            return encryptor.Encrypt(balencoder.Encode(value));
        }

        public BigPolyArray[] enc_enc(int[] values)
        {
            BigPolyArray[] polys = new BigPolyArray[values.Length];

            for (int i = 0; i < values.Length; ++i)
                polys[i] = encryptor.Encrypt(balencoder.Encode(values[i]));

            return polys;
        }

        public int dec_dec(BigPolyArray poly)
        {
            return Math.Abs(balencoder.DecodeInt32(decryptor.Decrypt(poly)));
        }

        public int[] dec_dec(BigPolyArray[] polys)
        {
            int[] values = new int[polys.Length];

            for (int i = 0; i < polys.Length; ++i)
                values[i] = Math.Abs(balencoder.DecodeInt32(decryptor.Decrypt(polys[i])));

            return values;
        }

        public BigPolyArray add(BigPolyArray value1, BigPolyArray value2)
        {
            if (get_noise(evaluator.Add(value1, value2)) >= get_max_noise())
            {
                value1 = recrypt(value1);
                value2 = recrypt(value2);
            }

            return evaluator.Add(value1, value2);

            /*BigPoly[] values = auto_recrypt(Operation.Add, new BigPoly[] { value1, value2 });

            return evaluator.Add(values[0], values[1]);*/
        }

        public BigPolyArray add_plain(BigPolyArray valueEnc, BigPoly value) // recrypt broken
        {
            if (get_noise(evaluator.AddPlain(valueEnc, value)) >= get_max_noise())
                valueEnc = recrypt(valueEnc);

            return evaluator.AddPlain(valueEnc, value);

            /*BigPoly[] values = auto_recrypt(Operation.AddPlain, new BigPoly[] { valueEnc, value });

            return evaluator.AddPlain(values[0], values[1]);*/
        }

        public BigPolyArray add_many(BigPolyArray[] values)
        {
            if (get_noise(evaluator.AddMany(new List<BigPolyArray>(values))) >= get_max_noise())
                values = recrypt(values);

            return evaluator.AddMany(new List<BigPolyArray>(values));

            /*values = auto_recrypt(Operation.AddMany, values);

            return evaluator.AddMany(new List<BigPoly>(values));*/
        }

        public BigPolyArray mul(BigPolyArray value1, BigPolyArray value2)
        {
            if (value1.Size + value2.Size - 1 > 4)
            {
                if (get_noise(evaluator.Relinearize(evaluator.Multiply(value1, value2))) >= get_max_noise())
                {
                    value1 = recrypt(value1);
                    value2 = recrypt(value2);
                }
                return evaluator.Relinearize(evaluator.Multiply(value1, value2));
            }
            else
            {
                if (get_noise(evaluator.Multiply(value1, value2)) >= get_max_noise())
                {
                    value1 = recrypt(value1);
                    value2 = recrypt(value2);
                }
                return evaluator.Multiply(value1, value2);
            }

            /*BigPoly[] values = auto_recrypt(Operation.Mul, new BigPoly[] { value1, value2 });

            return evaluator.Multiply(values[0], values[1]);*/
        }

        public BigPolyArray mul_plain(BigPolyArray valueEnc, BigPoly value)
        {
            if (valueEnc.Size + 1 > 4)
            {
                if (get_noise(evaluator.Relinearize(evaluator.MultiplyPlain(valueEnc, value))) >= get_max_noise())
                    valueEnc = recrypt(valueEnc);

                return evaluator.Relinearize(evaluator.MultiplyPlain(valueEnc, value));
            }
            else
            {
                if (get_noise(evaluator.MultiplyPlain(valueEnc, value)) >= get_max_noise())
                    valueEnc = recrypt(valueEnc);

                return evaluator.MultiplyPlain(valueEnc, value);
            }

            /*BigPoly[] values = auto_recrypt(Operation.MulPlain, new BigPoly[] { valueEnc, value }); // recrypt broken

            return evaluator.MultiplyPlain(values[0], values[1]);*/
        }

        public BigPolyArray mul_many(BigPolyArray[] values)
        {
            if (get_noise(evaluator.Relinearize(evaluator.MultiplyMany(new List<BigPolyArray>(values)))) >= get_max_noise())
                values = recrypt(values);

            return evaluator.Relinearize(evaluator.MultiplyMany(new List<BigPolyArray>(values)));

            /* values = auto_recrypt(Operation.MulMany, values);

            return evaluator.MultiplyMany(new List<BigPoly>(values));*/
        }

        /*
        protected enum Operation { Add = 0, AddPlain = 1, AddMany = 2, Mul = 10, MulPlain = 11, MulMany = 12 };
        private SimulationEvaluator simevaluator;
        protected BigPoly[] auto_recrypt(Operation operation, BigPoly[] values) // not implemented for add_plain/mul_plain
        {
            if (encparms.Mode == EncryptionMode.Test)
                return values;

            if (mode < Mode.SECKEY) // Mode.EVALKEYS
                throw new NotSupportedException("No Secret Key available!");

            Simulation[] simulations = new Simulation[values.Length];
            for (int i = 0; i < values.Length; i++)
                simulations[i] = new Simulation(encparms, Utilities.InherentNoise(values[i], encparms, seckey));

            switch (operation)
            {
                case Operation.Add:
                    if (!simevaluator.Add(simulations[0], simulations[1]).Decrypts())
                        return recrypt(values);
                    else
                        return values;
                case Operation.AddPlain:
                    if (!simevaluator.AddPlain(simulations[0]).Decrypts())
                        return new BigPoly[] { recrypt(values[0]), values[1] };
                    else
                        return values;
                case Operation.AddMany:
                    if (!simevaluator.AddMany(new List<Simulation>(simulations)).Decrypts())
                        return recrypt(values);
                    else
                        return values;

                case Operation.Mul:
                    if (!simevaluator.Multiply(simulations[0], simulations[1]).Decrypts())
                        return recrypt(values);
                    else
                        return values;
                case Operation.MulPlain:
                    if (!simevaluator.MultiplyPlain(simulations[0], values[1].CoeffCount, 1).Decrypts()) // 1!
                        return new BigPoly[] { recrypt(values[0]), values[1] };
                    else
                        return values;
                case Operation.MulMany:
                    if (!simevaluator.MultiplyMany(new List<Simulation>(simulations)).Decrypts())
                        return recrypt(values);
                    else
                        return values;

                default:
                    return values;
            }
        }*/

        protected BigPolyArray[] recrypt(BigPolyArray[] polys)
        {
            for (int i = 0; i < polys.Length; i++)
                polys[i] = recrypt(polys[i]);

            return polys;
        }

        protected BigPolyArray recrypt(BigPolyArray poly)
        {
            numofreencrypts++;

            return encryptor.Encrypt(decryptor.Decrypt(poly));
        }

        public int get_noise(BigPolyArray poly)
        {
            return Utilities.InherentNoise(poly, encparms, seckey).GetSignificantBitCount();
        }

        public int get_max_noise()
        {
            return Utilities.InherentNoiseMax(encparms).GetSignificantBitCount();
        }

        public void save_encparams(Stream file)
        {
            encparms.Save(file);
        }

        public void save_pubkey(Stream file)
        {
            pubkey.Save(file);
        }

        public void save_evalkeys(Stream file)
        {
            evalkeys.Save(file);
        }

        public void save_seckey(Stream file)
        {
            seckey.Save(file);
        }

        /*public void print_encparms(StreamWriter output)
        {
            output.Write(encparms.ToString());
        }*/

        public void print_pubkey(StreamWriter output)
        {
            output.Write(pubkey.ToString());
        }

        /*public void print_evalkeys(StreamWriter output)
        {
            if (mode < Mode.EVALKEYS)
                throw new NotSupportedException("No Evaluation Keys available!");

            output.Write(evalkeys.ToString());
        }*/

        public void print_seckey(StreamWriter output)
        {
            output.Write(seckey.ToString());
        }
    }
}
