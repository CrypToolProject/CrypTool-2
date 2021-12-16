using System;
using System.Collections.Generic;

namespace common
{
    public class TranspositionTransformations
    {

        // get a string representing the forwardKey
        // if long KEY_PRINTABLE_ALPHABET_SIZE then only the first 50 symbols (represented by comma separated numbers)
        // is returned
        private static string getKeyString(int[] key, int keylen)
        {

            AlphabetVector stream = new AlphabetVector(keylen, false);
            stream.copy(key);
            return key.ToString();
        }

        // check if forwardKey is valid
        private static bool isKeyValid(int[] key, int keylen)
        {
            int[] count = new int[keylen];

            for (int i = 0; i < keylen; i++)
            {
                if (++count[key[i]] == 2)
                {
                    return false;
                }
            }

            return true;

        }

        // rotate a forwardKey
        //private static void rotateKey(int[] fromKey, int toKey[], int keylen, int shift)
        private static void rotateKey(int[] fromKey, int[] toKey, int keylen, int shift)
        {

            int from, to, len;
            from = 0;
            to = shift;
            len = keylen - shift;
            Array.Copy(fromKey, from, toKey, to, len);
            //System.arraycopy(fromKey, from, toKey, to, len);

            from = keylen - shift;
            to = 0;
            len = shift;
            Array.Copy(fromKey, from, toKey, to, len);
            //System.arraycopy(fromKey, from, toKey, to, len);


        }

        // slide a segment to the right by "shift" steps.
        //private static void slideKeySegment(int[] fromKey, int toKey[], int keylen, int startPosition, int len, int shift)
        private static void slideKeySegment(int[] fromKey, int[] toKey, int keylen, int startPosition, int len, int shift)
        {
            Array.Copy(fromKey, 0, toKey, 0, keylen);
            //System.arraycopy(fromKey, 0, toKey, 0, keylen);

            int affectedLen = len + shift;

            if (affectedLen > keylen)
            {
                return;
            }

            for (int i = 0; i < affectedLen; i++)
            {
                int fromOffset = i;
                int toOffset = (i + shift) % affectedLen;
                toKey[(startPosition + toOffset) % keylen] = fromKey[(startPosition + fromOffset) % keylen];
            }

            //System.out.printf ("p1=%d, Len = %d, Shift %d, AFTER  - TransformationKey = %s BEFORE %s \n",p1,len,shift,getKeyString(toKey,length),getKeyString(fromKey,length));

            if (!isKeyValid(toKey, keylen))
            {
                //Console.WriteLine("BAD KEY SHIFT3\n");
                //Console.WriteLine("p1=%d, Len = %d, Shift %d, AFTER: \n%s BEFORE: \n%s\n", startPosition, len, shift, getKeyString(toKey, keylen), getKeyString(fromKey, keylen));
            }
        }

        // invert a segment .
        //private static void invertKeySegment(int[] fromKey, int toKey[], int keylen, int startPosition, int len)
        private static void invertKeySegment(int[] fromKey, int[] toKey, int keylen, int startPosition, int len)
        {
            Array.Copy(fromKey, 0, toKey, 0, keylen);
            //System.arraycopy(fromKey, 0, toKey, 0, keylen);

            len %= (keylen + 1);
            for (int i = 0; i < len; i++)
            {
                int p1 = (startPosition + i) % keylen;
                int p2 = (startPosition + len - i - 1 + keylen) % keylen;
                toKey[p1] = fromKey[p2];
            }
            if (!isKeyValid(toKey, keylen))
            {
                //Console.WriteLine("BAD KEY INVERT\n");
                //Console.WriteLine("p1=%d, Len = %d, AFTER: \n%s BEFORE: \n%s\n", startPosition, len, getKeyString(toKey, keylen), getKeyString(fromKey, keylen));
            }
        }
        //private static void invertAdjacent(int[] fromKey, int toKey[], int keylen, bool skipFirst)
        private static void invertAdjacent(int[] fromKey, int[] toKey, int keylen, bool skipFirst)
        {
            Array.Copy(fromKey, 0, toKey, 0, keylen);
            //System.arraycopy(fromKey, 0, toKey, 0, keylen);

            for (int i = skipFirst ? 1 : 0; i < keylen - 1; i += 2)
            {
                int p1 = i;
                int p2 = i + 1;
                toKey[p1] = fromKey[p2];
                toKey[p2] = fromKey[p1];
            }
            if (!isKeyValid(toKey, keylen))
            {
                //Console.WriteLine("BAD KEY invertAdjacent\n");
                //Console.WriteLine("skip=%s, AFTER: \n%s BEFORE: \n%s\n", skipFirst, getKeyString(toKey, keylen), getKeyString(fromKey, keylen));
            }
        }

        // swap to segments (of given length len, positions p1 and p2) in the forwardKey
        //private static void swapKeySegment(int[] fromKey, int toKey[], int keylen, int p1, int p2, int len)
        private static void swapKeySegment(int[] fromKey, int[] toKey, int keylen, int p1, int p2, int len)
        {
            Array.Copy(fromKey, 0, toKey, 0, keylen);
            Array.Copy(fromKey, p1, toKey, p2, len);
            Array.Copy(fromKey, p2, toKey, p1, len);
            /*System.arraycopy(fromKey, 0, toKey, 0, keylen);
            System.arraycopy(fromKey, p1, toKey, p2, len);
            System.arraycopy(fromKey, p2, toKey, p1, len);*/


            //System.out.printf ("p1=%d, Len = %d, Shift %d, AFTER  - TransformationKey = %s BEFORE %s \n",p1,len,shift,getKeyString(toKey,length),getKeyString(fromKey,length));

            if (!isKeyValid(toKey, keylen))
            {
                //Console.WriteLine("BAD KEY SWAP\n");
                //Console.WriteLine("p1=%d, p2 = %d, Len %d, AFTER: \n%s BEFORE: \n%s \n", p1, p2, len, getKeyString(toKey, keylen), getKeyString(fromKey, keylen));
            }

        }

        private class TransformationKey
        {
            private readonly int[] key;

            public TransformationKey(int[] key, int keylen)
            {
                this.key = new int[keylen];
                for (short i = 0; i < key.Length; i++)
                {
                    this.key[key[i]] = i;
                }

            }

            public int[] getKey()
            {
                return key;
            }

            public int GetSingleKey(int index)
            {
                return key[index];
            }

        }

        public int[,] list;

        public int size()
        {
            return list.GetLength(0);
        }

        public TranspositionTransformations(int keylen, bool slides, bool swaps, bool inversions)
        {
            HashSet<TransformationKey> set = new HashSet<TransformationKey>();

            int[] key = new int[keylen];

            for (int i = 0; i < keylen; i++)
            {
                key[i] = i;
            }
            int[] tempk = new int[keylen];

            if (slides)
            {
                for (int len = keylen; len > 0; len--)
                {
                    for (int shift = 1; shift < keylen; shift++)
                    {
                        for (int p1 = 0; p1 < keylen; p1++)
                        {

                            slideKeySegment(key, tempk, keylen, p1, len, shift);
                            addTransformation(set, tempk, keylen);
                        }
                    }
                }
                for (int shift = 0; shift < keylen; shift++)
                {
                    rotateKey(key, tempk, keylen, shift);
                    addTransformation(set, tempk, keylen);
                }
            }
            if (inversions)
            {
                for (int len = keylen; len > 0; len--)
                {
                    for (int p1 = 0; p1 < keylen; p1++)
                    {
                        invertKeySegment(key, tempk, keylen, p1, len);
                        addTransformation(set, tempk, keylen);
                    }
                }
                invertAdjacent(key, tempk, keylen, false);
                addTransformation(set, tempk, keylen);
                invertAdjacent(key, tempk, keylen, true);
                addTransformation(set, tempk, keylen);
            }

            if (swaps || !slides)
            {
                int maxLen;
                if (!swaps)
                {
                    maxLen = 1;
                }
                else
                {
                    maxLen = keylen / 4;
                }
                for (int len = maxLen; len > 0; len--)
                {

                    for (int p1 = 0; p1 < keylen; p1++)
                    {
                        for (int p2 = p1 + len; p2 < (keylen - len); p2++)
                        {
                            swapKeySegment(key, tempk, keylen, p1, p2, len);
                            addTransformation(set, tempk, keylen);
                        }
                    }
                }

                for (int len = 1; len < 4; len++)
                {
                    for (int p1 = 0; p1 < (keylen - 3 * len); p1++)
                    {
                        for (int p2 = p1 + len; p2 < (keylen - 2 * len); p2++)
                        {
                            for (int p3 = p2 + len; p3 < (keylen - len); p3++)
                            {
                                Array.Copy(key, 0, tempk, 0, keylen);
                                Array.Copy(key, p1, tempk, p2, len);
                                Array.Copy(key, p2, tempk, p3, len);
                                Array.Copy(key, p3, tempk, p1, len);

                                addTransformation(set, tempk, keylen);

                                Array.Copy(key, 0, tempk, 0, keylen);
                                Array.Copy(key, p1, tempk, p3, len);
                                Array.Copy(key, p2, tempk, p1, len);
                                Array.Copy(key, p3, tempk, p2, len);



                                addTransformation(set, tempk, keylen);

                            }
                        }
                    }
                }

                for (int p1 = 0; p1 < keylen; p1++)
                {
                    for (int p2 = 0; p2 < keylen; p2++)
                    {
                        for (int p3 = 0; p3 < keylen; p3++)
                        {
                            if (p1 == p2 || p1 == p3 || p2 == p3)
                            {
                                continue;
                            }
                            Array.Copy(key, 0, tempk, 0, keylen);
                            tempk[p1] = key[p2];
                            tempk[p2] = key[p3];
                            tempk[p3] = key[p1];
                            addTransformation(set, tempk, keylen);
                        }
                    }
                }


            }
            list = new int[set.Count, keylen];
            int count = 0;
            foreach (TransformationKey k in set)
            {
                for (int j = 0; j < list.GetLength(1); j++)
                {
                    list[count, j] = k.GetSingleKey(j);
                }
                count = count + 1;
            }
        }

        private void addTransformation(HashSet<TransformationKey> set, int[] tempk, int keylen)
        {
            if (!isKeyValid(tempk, keylen))
            {
                //Console.WriteLine("Invalid transformation");
                return;
            }

            try
            {
                TransformationKey key = new TransformationKey(tempk, keylen);
                bool found = false;
                foreach (TransformationKey k in set)
                {
                    found = true;
                    int[] transformation1 = k.getKey();
                    int[] transformation2 = key.getKey();
                    for (int i = 0; i < transformation1.Length; i++)
                    {
                        if (transformation1[i] != transformation2[i])
                        {
                            found = false;
                            break;
                        }

                    }
                    if (found)
                    {
                        break;
                    }
                }
                //found = false; //original lasry
                if (!found)
                {
                    set.Add(key);
                }

            }
            catch (System.OutOfMemoryException)
            {
                Environment.Exit(-1);
            }
        }

        public void randomize()
        {
            Random r = new Random();

            for (int i = 0; i < list.GetLength(0); i++)
            {
                int j = r.Next(0, list.GetLength(0));

                int[] temp = { };
                Array.Resize(ref temp, list.GetLength(1));

                for (int k = 0; k < list.GetLength(1); k++)
                {
                    temp[k] = list[i, k];
                    list[i, k] = list[j, k];
                    list[j, k] = temp[k];
                }

            }
        }

        public void transform(int[] from, int[] to, int length, int transformationIndex)
        {
            for (int i = 0; i < length; i++)
            {
                to[list[transformationIndex, i]] = from[i];
            }
        }

    }
}
