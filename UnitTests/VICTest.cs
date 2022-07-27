using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Text.RegularExpressions;

namespace UnitTests
{
    [TestClass]
    public class VICTest
    {
        public VICTest()
        {
        }
        //The tests themselves are not yet implemented. I'm just trying to initializie the component correctly and run it with a testvector, but that keeps failing.
        [TestMethod]
        public void VICTestMethod()
        {
            CrypTool.PluginBase.ICrypComponent pluginInstance = TestHelpers.GetPluginInstance("VIC");
            PluginTestScenario scenario = new PluginTestScenario(pluginInstance, new[] { "Input", "Phrase", "Date", "InitializingString", "Number", "Password", ".Action", ".Alphabet" }, new[] { "Output" });            

            object[] output;
            object[] secondOutput;

            foreach (TestVector vector in testvectors)
            {
                string input = vector.input;
                input = FormatString(input, vector.alphabetType);
                output = scenario.GetOutputs(new object[] { input, vector.phrase, vector.date, vector.initializingString, vector.number, vector.password, vector.actionType, vector.alphabetType });
                secondOutput = scenario.GetOutputs(new object[] { (string)output[0], vector.phrase, vector.date, vector.initializingString, vector.number, vector.password, 1, vector.alphabetType });
                Assert.AreEqual(true, CheckStringEquality(input, (string)secondOutput[0]), "Unexpected value in test #" + vector.n + ".");
            }

        }

        public bool CheckStringEquality(string first, string second)
        {
            int firstiterator = 0, seconditerator = 0;
            char[] firstAr = first.ToCharArray();
            char[] secondAr = second.ToCharArray();

            int differentChars = 0;

            while (firstiterator < firstAr.Length)
            {
                if (firstAr.GetValue(firstiterator).ToString() == secondAr.GetValue(seconditerator).ToString())
                {
                    firstiterator++;
                    seconditerator++;
                }
                else
                {
                    while (firstAr.GetValue(firstiterator).ToString() != secondAr.GetValue(seconditerator).ToString())
                    {
                        seconditerator++;
                        differentChars++;
                        if (differentChars > 5)
                        {
                            return false;
                        }
                    }
                }
            }


            return true;
        }

        public string FormatString(string input, int usedAlphabet)
        {
            string alphabet;
            if (usedAlphabet == 0)
            {
                alphabet = "abcdefghijklmnopqrstuvwxyz".ToUpper();
            }
            else
            {
                alphabet = "абвгдежзиклмнопрстуфхцчшщыьэюя".ToUpper();
            }
            input = input.ToUpper();
            input = (Regex.Replace(input, $"[^{alphabet}.,;?!]", ""));
            input = (Regex.Replace(input, @"\s+", ""));
            return input;
        }

        private struct TestVector
        {
            public int n;
            public string input, output, phrase, date, initializingString, number, password;
            public int alphabetType, actionType;
        }

        private readonly TestVector[] testvectors = new TestVector[]
        {
            new TestVector () {n=0,
                input="Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat.",
                phrase="Met defective are allowance two perceived listening consulted contained",
                date="391945",
                initializingString="208103",
                number="10",
                password="snowfall",
                output="Some output value",
                alphabetType=0,
                actionType=0
            },
            new TestVector () {n=1,
                input="Having an increased awareness of the possible differences in expectations and behaviour can help us avoid cases of miscommunication, but it is vital that we also remember that cultural stereotypes can be detrimental",
                phrase="Four loko air plant unicorn copper mug",
                date="161982",
                initializingString="263614",
                number="13",
                password="helicopter",
                output="Some output value",
                alphabetType=0,
                actionType=0
            },
            new TestVector () {n=2,
                input="Каждый человек должен обладать всеми правами и всеми свободами, провозглашенными настоящей Декларацией, без какого бы то ни было различия, как-то в отношении расы, цвета кожи, пола, языка, религии",
                phrase="Все люди рождаются свободными и равными в своем достоинстве и правах",
                date="721981",
                initializingString="12894",
                number="7",
                password="автомобиль",
                output="Some output value",
                alphabetType=1,
                actionType=0
            },
            new TestVector () {n=3,
                input="Ни завтрак, ни его приготовление в России обычно не занимает много времени. В России вообще не принято много есть утром. Обычный завтрак включает омлет, бутерброды, кукурузные хлопья или что-то в этом роде.",
                phrase="В России обычно принимают пищу три раза в день - на завтрак, обед и ужин.",
                date="491384",
                initializingString="123763",
                number="4",
                password="снегопад",
                output="Some output value",
                alphabetType=1,
                actionType=0
            }
        };

    }
}