/*
   Copyright 2020 George Lasry
   Converted in 2020 from Java to C# by Nils Kopal

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
using System.Text;

namespace EnigmaAnalyzerLib.Common
{
    public class CommandLine
    {
        /**
         * Command line argument. Encapsulates both the specification of the argument, as well as the values parse
         * from the actual command line arguments.
         */
        public class Argument
        {
            public enum Type
            {
                NUMERIC, STRING, BOOL
            }

            // Specifications.

            // Relevant to all types.
            public Flag flag;
            public Type type;
            public string shortDesc;
            public string longDesc;

            // Relevant to string and NUMERIC
            public bool required;
            public bool multiple;

            // Relevant to NUMERIC.
            public int minIntValue;
            public int maxIntValue;
            public int defaultIntValue;

            // Relevant to string
            public string defaultstringValue = "";
            public string[] validstringValues = null;
            public string validstringValuesstring = null;

            // end of specifications.

            // Values - parsed from the program command line.
            public bool boolValue = false;
            public List<int> integerArrayList = new List<int>();
            public List<string> stringArrayList = new List<string>();
            public bool set = false; // Was the argument set.

            public Argument(Flag flag,
                            string shortDesc,
                            string longDesc)
            {
                this.flag = flag;
                type = Type.BOOL;
                this.longDesc = longDesc;
                this.shortDesc = shortDesc;

                required = false;
                multiple = false;

            }

            public Argument(Flag flag,
                            string shortDesc,
                            string longDesc,

                            bool required,

                            int minIntValue,
                            int maxIntValue,
                            int defaultIntValue)
            {
                this.flag = flag;
                type = Type.NUMERIC;
                this.longDesc = longDesc;
                this.shortDesc = shortDesc;

                this.required = required;
                multiple = false;

                this.minIntValue = minIntValue;
                this.maxIntValue = maxIntValue;
                this.defaultIntValue = defaultIntValue;

            }

            public Argument(Flag flag,
                            string shortDesc,
                            string longDesc,

                            bool required,

                            int minIntValue,
                            int maxIntValue)
            {
                this.flag = flag;
                type = Type.NUMERIC;
                this.longDesc = longDesc;
                this.shortDesc = shortDesc;

                this.required = required;
                multiple = true;

                this.minIntValue = minIntValue;
                this.maxIntValue = maxIntValue;
            }

            public Argument(Flag flag,
                            string shortDesc,
                            string longDesc,

                            bool required,

                            string defaultstringValue)
            {
                this.flag = flag;
                type = Type.STRING;
                this.longDesc = longDesc;
                this.shortDesc = shortDesc;

                this.required = required;
                multiple = false;

                this.defaultstringValue = defaultstringValue;
                validstringValues = null;
            }
            public Argument(Flag flag,
                            string shortDesc,
                            string longDesc,

                            bool required,

                            string defaultstringValue,
                            string[] validstringValues)
            {
                this.flag = flag;
                type = Type.STRING;
                this.longDesc = longDesc;
                this.shortDesc = shortDesc;

                this.required = required;
                multiple = false;

                this.defaultstringValue = defaultstringValue;
                this.validstringValues = validstringValues;

                validstringValuesstring = "";
                foreach (string validstringValue in validstringValues)
                {
                    if (validstringValuesstring.Length > 0)
                    {
                        validstringValuesstring += ", ";
                    }
                    validstringValuesstring += validstringValue;
                }
            }
        }

        private static readonly List<Argument> arguments = new List<Argument>();

        public static void createCommonArguments()
        {
            add(new Argument(
                    Flag.CIPHERTEXT,
                    "Ciphertext or ciphertext file",
                    "Ciphertext string, or full path for the file with the cipher, ending with .txt.",
                    false,
                    ""));

            add(new Argument(
                    Flag.CRIB,
                    "Crib (known-plaintext)",
                    "Known plaintext (crib) at the beginning of the message.",
                    false,
                    ""));

            add(new Argument(
                    Flag.RESOURCE_PATH,
                    "Resource directory",
                    "Full path of directory for resources (e.g. stats files).",
                    false,
                    "."));

            add(new Argument(
                    Flag.THREADS,
                    "Number of processing threads",
                    "Number of threads, for multithreading. 1 for no multithreading.",
                    false,
                    1, 20, 7));

            add(new Argument(
                    Flag.CYCLES,
                    "Number of cycles",
                    "Number of cycles for key search. 0 for infinite.",
                    false,
                    0, 1000, 0));
        }
        public static bool parseAndPrintCommandLineArgs(string[] args)
        {
            //Dictionary<int, string> inputValuesMap = CtAPI.getInputValuesMap();
            /*if (!(inputValuesMap.Count == 0)) {
                string[] ctArgs = parseRemoteCommandLineArguments(inputValuesMap);
                parseArguments(ctArgs, false);
            }*/
            if (!parseArguments(args, true))
            {
                return false;
            }
            printArguments();
            return true;
        }

        /*private static string[] parseRemoteCommandLineArguments(Dictionary<int, string> inputValuesMap) {

            string args = inputValuesMap.Get(CtAPI.INPUT_VALUE_ARGS);
            if (args != null && args.Trim().Length > 0) {
                Console.WriteLineln("Received remote args:" + args);
            } else {
                args = "";
            }
            string ciphertext = inputValuesMap.Get(CtAPI.INPUT_VALUE_CIPHERTEXT);
            if (ciphertext != null && ciphertext.Trim().Length > 0) {
                args += " -i " + ciphertext;
                Console.WriteLineln("Received remote ciphertext: " + ciphertext);
            }
            string crib = inputValuesMap.Get(CtAPI.INPUT_VALUE_CRIB);
            if (crib != null && crib.Trim().Length > 0) {
                args += " -" + Flag.CRIB + " " + crib;
                Console.WriteLineln("Received remote crib: " + crib);
            }

            string resourcePath = inputValuesMap.Get(CtAPI.INPUT_VALUE_RESOURCES);
            if (resourcePath != null && resourcePath.Trim().Length > 0) {
                args += " -" + Flag.RESOURCE_PATH + " " + resourcePath;
                Console.WriteLineln("Received resource path: " + resourcePath);
            }

            string threads = inputValuesMap.Get(CtAPI.INPUT_VALUE_THREADS);
            if (threads != null && threads.Trim().Length > 0) {
                args += " -" + Flag.THREADS + " " + threads;
                Console.WriteLineln("Received threads: " + threads);
            }

            string cycles = inputValuesMap.Get(CtAPI.INPUT_VALUE_CYCLES);
            if (cycles != null && cycles.Trim().Length > 0) {
                args += " -" + Flag.CYCLES + " " + cycles;
                Console.WriteLineln("Received cycles: " + cycles);
            }


            if (args.Count == 0) {
                return new string[]{};
            }

            args = args.Replace("[\\\r]+", " ");
            args = args.Replace(" +", " ");
            args = args.Trim();
            Console.WriteLineln("Summary of remote args: " + args);

            return args.split(" ");
        }*/

        public static void add(Argument argument)
        {
            arguments.Add(argument);
        }
        public static int getintValue(Flag flag)
        {
            foreach (Argument argument in arguments)
            {
                if (argument.flag == flag)
                {
                    /*if (argument.type != Argument.Type.NUMERIC) {
                        Console.WriteLine("Not a numeric flag " + flag.ToString());
                    }
                    if (argument.multiple) {
                        Console.WriteLine("Multiple value numeric flag " + flag.ToString());
                    }
                    if (argument.integerArrayList.Count == 0) {
                        Console.WriteLine("No value for numeric flag " + flag.ToString());
                    }*/
                    if (argument.integerArrayList.Count > 0)
                    {
                        return argument.integerArrayList[0];
                    }
                }
            }
            //Console.WriteLine("No such flag " + flag.ToString());
            return -1;
        }
        public static List<int> getintValues(Flag flag)
        {
            foreach (Argument argument in arguments)
            {
                if (argument.flag == flag)
                {
                    /*if (argument.type != Argument.Type.NUMERIC) {
                        Console.WriteLine("Not a numeric flag " + flag.ToString());
                    }
                    if (!argument.multiple) {
                        Console.WriteLine("Single value numeric flag " + flag.ToString());
                    }*/
                    return argument.integerArrayList;
                }
            }
            //Console.WriteLine("No such flag " + flag.ToString());
            return null;
        }
        public static string getstringValue(Flag flag)
        {
            foreach (Argument argument in arguments)
            {
                if (argument.flag == flag)
                {
                    /*if (argument.type != Argument.Type.STRING) {
                        Console.WriteLine("Not a string flag " + flag.ToString());
                    }
                    if (argument.multiple) {
                        Console.WriteLine("Multiple value string flag " + flag.ToString());
                    }
                    if (argument.stringArrayList.Count == 0) {
                        Console.WriteLine("No value for string flag " + flag.ToString());
                    }*/
                    if (argument.stringArrayList.Count > 0)
                    {
                        return argument.stringArrayList[0];
                    }
                }
            }
            //Console.WriteLine("No such flag " + flag.ToString());
            return string.Empty;
        }
        public static List<string> getstringValues(Flag flag)
        {
            foreach (Argument argument in arguments)
            {
                if (argument.flag == flag)
                {
                    /*if (argument.type != Argument.Type.STRING) {
                        Console.WriteLine("Not a string flag " + flag.ToString());
                    }
                    if (!argument.multiple) {
                        Console.WriteLine("Single value string flag " + flag.ToString());
                    }*/
                    return argument.stringArrayList;
                }
            }
            //Console.WriteLine("No such flag " + flag.ToString());
            return new List<string>();
        }
        public static bool getboolValue(Flag flag)
        {
            foreach (Argument argument in arguments)
            {
                if (argument.flag == flag)
                {
                    /*if (argument.type != Argument.Type.BOOL) {
                        Console.WriteLine("Not a bool flag " + flag.ToString());
                    }*/
                    return argument.boolValue;
                }
            }
            //Console.WriteLine("No such flag " + flag.ToString());
            return false;
        }
        public static bool isSet(Flag flag)
        {
            foreach (Argument argument in arguments)
            {
                if (argument.flag == flag)
                {
                    return argument.set;
                }
            }
            //Console.WriteLine("No such flag " + flag.ToString());
            return false;
        }
        public static string getShortDesc(Flag flag)
        {
            foreach (Argument mainMenuArgument in arguments)
            {
                if (mainMenuArgument.flag == flag)
                {
                    return mainMenuArgument.shortDesc;
                }
            }
            return string.Empty;
        }

        private static bool parseArguments(string[] args, bool setDefaults)
        {
            string error = parseArgumentsAndReturnError(args, setDefaults);
            if (error != null)
            {
                //printUsage();
                Console.WriteLine(error);
                return false;
            }
            return true;
        }

        private static string parseArgumentsAndReturnError(string[] args, bool setDefaults)
        {
            Argument currentArgument = null;

            foreach (string arg in args)
            {
                if (arg.ToUpper().StartsWith("-V"))
                {
                    printUsage();
                    return null;
                }
                if (arg.StartsWith("-") && currentArgument != null)
                {
                    return string.Format("Invalid argument >{0}<. Parameter missing for -{1} ({2})",
                            arg, currentArgument.flag.ToString(), currentArgument.shortDesc);
                }
                if (!arg.StartsWith("-") && currentArgument == null)
                {
                    return string.Format("Invalid argument >{0}<.", arg);
                }
                if (arg.StartsWith("-"))
                {
                    currentArgument = getMainArgument(arg);
                    if (currentArgument == null)
                    {
                        return string.Format("Invalid argument >{0}<.", arg);
                    }
                    if (currentArgument.type == Argument.Type.BOOL)
                    {
                        currentArgument.boolValue = true;
                        currentArgument.set = true;
                        currentArgument = null;
                    }
                    continue;
                }

                // Handle string or numeric values.
                if (currentArgument.type == Argument.Type.NUMERIC)
                {
                    int value = 0;
                    try
                    {
                        value = int.Parse(arg);
                        if (value >= currentArgument.minIntValue && value <= currentArgument.maxIntValue)
                        {
                            if (!currentArgument.multiple && currentArgument.integerArrayList.Count > 0)
                            {
                                return string.Format("Duplicate value >{0}< for -{1} ({2}).Previous value >{3}<.",
                                        arg, currentArgument.flag.ToString(), currentArgument.shortDesc, currentArgument.integerArrayList[0]);
                            }
                            currentArgument.integerArrayList.Add(value);
                            currentArgument.set = true;
                            currentArgument = null;
                            continue;
                        }
                    }
                    catch (Exception)
                    {
                        return string.Format("Invalid value >{0}< for -{1} ({2}). " +
                                      "Should be between {3} and {4} (default is {5}).{6}",
                              arg,
                              currentArgument.flag.ToString(),
                              currentArgument.shortDesc,
                              currentArgument.minIntValue,
                              currentArgument.maxIntValue,
                              currentArgument.defaultIntValue,
                              currentArgument.longDesc);
                    }
                }

                if (currentArgument.type == Argument.Type.STRING)
                {
                    if (currentArgument.stringArrayList.Count > 0 && !currentArgument.multiple)
                    {
                        return string.Format("Duplicate value >{0}< for -{1} ({2}).Previous value {3}.",
                                arg, currentArgument.flag.ToString(), currentArgument.shortDesc, currentArgument.stringArrayList[0]);
                    }
                    if (currentArgument.validstringValues != null)
                    {
                        bool valid = false;
                        foreach (string validstringValue in currentArgument.validstringValues)
                        {
                            if (arg.Equals(validstringValue))
                            {
                                valid = true;
                                break;
                            }

                        }
                        if (!valid)
                        {
                            return string.Format("Invalid value >{0}< for -{1} ({2}). " +
                                            "Should be one of {3} (default is {4}).{5}",
                                    arg, currentArgument.flag.ToString(), currentArgument.shortDesc,
                                    currentArgument.validstringValuesstring, currentArgument.defaultstringValue,
                                    currentArgument.longDesc);
                        }
                    }
                    currentArgument.stringArrayList.Add(arg);
                    currentArgument.set = true;
                    currentArgument = null;
                }
            }

            if (currentArgument != null)
            {
                return string.Format("Parameter missing for -{0} ({1})", currentArgument.flag.ToString(), currentArgument.shortDesc);
            }

            if (setDefaults)
            {
                foreach (Argument arguments in arguments)
                {
                    if (arguments.type == Argument.Type.NUMERIC && arguments.integerArrayList.Count == 0 && !arguments.multiple)
                    {
                        if (arguments.required)
                        {
                            return string.Format("Flag -{0} is mandatory but missing ({1})" +
                                            "Should speficiy a value between {2} and {3} (default is {4}).{5}",
                                    arguments.flag.ToString(), arguments.shortDesc, arguments.minIntValue,
                                    arguments.maxIntValue, arguments.defaultIntValue, arguments.longDesc);
                        }
                        else
                        {
                            arguments.integerArrayList.Add(arguments.defaultIntValue);
                        }
                    }
                    else if (arguments.type == Argument.Type.STRING && arguments.stringArrayList.Count == 0 && !arguments.multiple)
                    {
                        if (arguments.required)
                        {
                            return string.Format("Flag -{0} is mandatory but missing ({1}).{2}",
                                    arguments.flag.ToString(), arguments.shortDesc, arguments.longDesc);
                        }
                        else
                        {
                            arguments.stringArrayList.Add(arguments.defaultstringValue);
                        }
                    }
                }
            }
            return null;
        }
        private static void printArguments()
        {

            //Console.WriteLineln("Input Parameters");

            foreach (Argument arguments in arguments)
            {
                StringBuilder s = new StringBuilder(string.Format("{0} (-{1}): \t", arguments.shortDesc, arguments.flag.ToString()));

                switch (arguments.type)
                {
                    case Argument.Type.BOOL:
                        s.Append(" ").Append(arguments.boolValue);
                        break;
                    case Argument.Type.NUMERIC:
                        bool first = true;
                        foreach (int numericValue in arguments.integerArrayList)
                        {
                            if (first)
                            {
                                first = false;
                                s.Append(" ").Append(numericValue);
                            }
                            else
                            {
                                s.Append(", ").Append(numericValue);
                            }
                        }
                        break;
                    case Argument.Type.STRING:
                        first = true;
                        foreach (string stringValue in arguments.stringArrayList)
                        {
                            if (first)
                            {
                                first = false;
                                s.Append(" ").Append(stringValue);
                            }
                            else
                            {
                                s.Append(", ").Append(stringValue);
                            }
                        }
                        break;
                    default:
                        break;
                }
                //Console.WriteLineln(s.ToString());

            }
            //Console.WriteLineln("");
        }

        private static Argument getMainArgument(string arg)
        {
            Argument currentArgument = null;
            foreach (Argument mainMenuArgument in arguments)
            {
                if (mainMenuArgument.flag.ToString("g").EqualsIgnoreCase(arg.Substring(1)))
                {
                    currentArgument = mainMenuArgument;
                    break;
                }

            }
            return currentArgument;
        }
        private static void printUsage()
        {

            Console.WriteLine("Usage: EnigmaAnalyzer [arguments]Arguments:");

            foreach (Argument currentArgument in arguments)
            {
                string prefix = string.Format("\t-{0} \t{1}", currentArgument.flag.ToString(), currentArgument.shortDesc);

                switch (currentArgument.type)
                {
                    case Argument.Type.BOOL:
                        if (currentArgument.required && currentArgument.multiple)
                        {
                            Console.WriteLine("{0}  (required, one or more).\t\t{1}", prefix, currentArgument.longDesc);
                        }
                        else if (currentArgument.required)
                        {
                            Console.WriteLine("{0}  (required).\t\t{1}", prefix, currentArgument.longDesc);
                        }
                        else if (currentArgument.multiple)
                        {
                            Console.WriteLine("{0}  (optional, one or more).\t\t{1}", prefix, currentArgument.longDesc);
                        }
                        else
                        {
                            Console.WriteLine("{0}  (optional).\t\t{1}", prefix, currentArgument.longDesc);
                        }
                        break;
                    case Argument.Type.NUMERIC:
                        if (currentArgument.required && currentArgument.multiple)
                        {
                            Console.WriteLine("{0} \t\tShould specify a value between {1} and {2} (required, one or more).\t\t{3}",
                                    prefix,
                                    currentArgument.minIntValue,
                                    currentArgument.maxIntValue,
                                    currentArgument.longDesc);
                        }
                        else if (currentArgument.required)
                        {
                            Console.WriteLine("{0} \t\tShould specify a value between {1} and {2} (required).\t\t{3}",
                                    prefix,
                                    currentArgument.minIntValue,
                                    currentArgument.maxIntValue,
                                    currentArgument.longDesc);
                        }
                        else if (currentArgument.multiple)
                        {
                            Console.WriteLine("{0} \t\tShould specify a value between {1} and {2} (optional, one or more).\t\t{3}",
                                    prefix,
                                    currentArgument.minIntValue, currentArgument.maxIntValue,
                                    currentArgument.longDesc);
                        }
                        else
                        {
                            Console.WriteLine("{0} \t\tShould specify a value between {1} and {2} (optional, default is {3}).\t\t{4}",
                                    prefix,
                                    currentArgument.minIntValue, currentArgument.maxIntValue,
                                    currentArgument.defaultIntValue,
                                    currentArgument.longDesc);
                        }
                        break;
                    case Argument.Type.STRING:
                        string validValuesAddition = "";
                        if (currentArgument.validstringValuesstring != null && !(currentArgument.validstringValuesstring.Length == 0))
                        {
                            validValuesAddition = " \t\tShould specify one of " + currentArgument.validstringValuesstring;
                        }
                        if (currentArgument.required && currentArgument.multiple)
                        {
                            Console.WriteLine("{0} {1} (required, one or more).\t\t{2}", prefix, validValuesAddition, currentArgument.longDesc);
                        }
                        else if (currentArgument.required)
                        {
                            Console.WriteLine("{0} {1} (required).\t\t{2}", prefix, validValuesAddition, currentArgument.longDesc);
                        }
                        else if (currentArgument.multiple)
                        {
                            Console.WriteLine("{0} {1} (optional, one or more).\t\t{2}", prefix, validValuesAddition, currentArgument.longDesc);
                        }
                        else
                        {
                            Console.WriteLine("{0} {1} (optional, default is \"{2}\").\t\t{3}",
                                    prefix,
                                    validValuesAddition,
                                    currentArgument.defaultstringValue,
                                    currentArgument.longDesc);
                        }


                        break;
                    default:
                        break;
                }
            }
        }
    }
}