/*
   Copyright CrypTool 2 Team josef.matwich@gmail.com

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
using M209AnalyzerLib.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace M209AnalyzerLib.Common
{
    public class CommandLine
    {
        public
            class Argument
        {

            public enum Type { NUMERIC, STRING, BOOLEAN }

            // Specifications.

            // Relevant to all types.
            public Flag flag;
            public Type type;
            public String shortDesc;
            public String longDesc;

            // Relevant to STRING and NUMERIC
            public bool required;
            public bool multiple;

            // Relevant to NUMERIC.
            public int minIntValue;
            public int maxIntValue;
            public int defaultIntValue;

            // Relevant to STRING
            public String defaultStringValue = "";
            public String[] validStringValues = null;
            public String validStringValuesString = null;

            // end of specifications.

            // Values - parsed from the program command line.
            public bool booleanValue = false;
            public List<int> integerArrayList = new List<int>();
            public List<String> stringArrayList = new List<String>();
            public bool set = false; // Was the argument set.


            public Argument(Flag flag,
                            String shortDesc,
                            String longDesc)
            {
                this.flag = flag;
                this.type = Type.BOOLEAN;
                this.longDesc = longDesc;
                this.shortDesc = shortDesc;

                this.required = false;
                this.multiple = false;

            }

            public Argument(Flag flag,
                            String shortDesc,
                            String longDesc,

                            bool required,

                            int minIntValue,
                            int maxIntValue,
                            int defaultIntValue)
            {
                this.flag = flag;
                this.type = Type.NUMERIC;
                this.longDesc = longDesc;
                this.shortDesc = shortDesc;

                this.required = required;
                this.multiple = false;

                this.minIntValue = minIntValue;
                this.maxIntValue = maxIntValue;
                this.defaultIntValue = defaultIntValue;

            }

            public Argument(Flag flag,
                            String shortDesc,
                            String longDesc,

                            bool required,

                            int minIntValue,
                            int maxIntValue)
            {
                this.flag = flag;
                this.type = Type.NUMERIC;
                this.longDesc = longDesc;
                this.shortDesc = shortDesc;

                this.required = required;
                this.multiple = true;

                this.minIntValue = minIntValue;
                this.maxIntValue = maxIntValue;
            }


            public Argument(Flag flag,
                            String shortDesc,
                            String longDesc,

                            bool required,

                            String defaultStringValue)
            {
                this.flag = flag;
                this.type = Type.STRING;
                this.longDesc = longDesc;
                this.shortDesc = shortDesc;

                this.required = required;
                this.multiple = false;

                this.defaultStringValue = defaultStringValue;
                this.validStringValues = null;


            }
            public Argument(Flag flag,
                            String shortDesc,
                            String longDesc,

                            bool required,

                            String defaultStringValue,
                            String[] validStringValues)
            {
                this.flag = flag;
                this.type = Type.STRING;
                this.longDesc = longDesc;
                this.shortDesc = shortDesc;

                this.required = required;
                this.multiple = false;

                this.defaultStringValue = defaultStringValue;
                this.validStringValues = validStringValues;

                validStringValuesString = "";
                StringBuilder sb = new StringBuilder();
                foreach (String validStringValue in validStringValues)
                {
                    if (validStringValuesString.Length > 0)
                    {
                        sb.Append(", ");
                    }
                    sb.Append(validStringValue);
                }
                validStringValuesString = sb.ToString();
            }



        }

        private static List<Argument> arguments = new List<Argument>();

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
                    1, 50, 7));

            add(new Argument(
                    Flag.CYCLES,
                    "Number of cycles",
                    "Number of cycles for key search. 0 for infinite.",
                    false,
                    0, 1000, 0));
        }
        public static void parseAndPrintCommandLineArgs(String[] args)
        {
            //  Map<Integer, String> inputValuesMap = CtAPI.getInputValuesMap();
            //  if (!inputValuesMap.isEmpty()) {
            //      String[] ctArgs = parseRemoteCommandLineArguments(inputValuesMap);
            //      parseArguments(ctArgs, false);
            //   }
            try
            {
                parseArguments(args, true);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                //throw;
            }
            printArguments();
        }

        private static String[] parseRemoteCommandLineArguments(Dictionary<int, String> inputValuesMap)
        {

            //   String args = inputValuesMap.get(CtAPI.INPUT_VALUE_ARGS);
            //      if (args != null && args.trim().length() > 0) {
            //        CtAPI.println("Received remote args:" + args);
            //     } else {
            //          args = "";
            //      }
            //      String ciphertext = inputValuesMap.get(CtAPI.INPUT_VALUE_CIPHERTEXT);
            // //     if (ciphertext != null && ciphertext.trim().length() > 0) {
            //         args += " -i " + ciphertext;
            //         CtAPI.println("Received remote ciphertext: " + ciphertext);
            //     }
            //      String crib = inputValuesMap.get(CtAPI.INPUT_VALUE_CRIB);
            //      if (crib != null && crib.trim().length() > 0) {
            //          args += " -" + Flag.CRIB + " " + crib;
            //         CtAPI.println("Received remote crib: " + crib);
            //     }

            //       String resourcePath = inputValuesMap.get(CtAPI.INPUT_VALUE_RESOURCES);
            //      if (resourcePath != null && resourcePath.trim().length() > 0) {
            //          args += " -" + Flag.RESOURCE_PATH + " " + resourcePath;
            //         CtAPI.println("Received resource path: " + resourcePath);
            //     }

            //     String threads = inputValuesMap.get(CtAPI.INPUT_VALUE_THREADS);
            //     if (threads != null && threads.trim().length() > 0) {
            //         args += " -" + Flag.THREADS + " " + threads;
            ///    }

            //    String cycles = inputValuesMap.get(CtAPI.INPUT_VALUE_CYCLES);
            //    if (cycles != null && cycles.trim().length() > 0) {
            //        args += " -" + Flag.CYCLES + " " + cycles;
            //       CtAPI.println("Received cycles: " + cycles);
            //   }


            //   if (args.isEmpty()) {
            //        return new String[]{};
            //     }

            //      args = args.replaceAll("[\\n\\r]+", " ");
            //     args = args.replaceAll(" +", " ");
            //     args = args.trim();
            Console.WriteLine("Summary of remote args: ");

            return new String[0];

        }

        public static void add(Argument argument)
        {
            arguments.Add(argument);
        }
        public static int getIntegerValue(Flag flag)
        {
            foreach (Argument argument in arguments)
            {
                if (argument.flag == flag)
                {
                    if (argument.type != Argument.Type.NUMERIC)
                    {
                        Console.WriteLine($"Not a numeric flag {flag}");
                        Console.Read();
                        Environment.Exit(-1);
                    }
                    if (argument.multiple)
                    {
                        Console.WriteLine($"Multiple value numeric flag {flag}");
                        Console.Read();
                        Environment.Exit(-1);
                    }
                    if (argument.integerArrayList.Count == 0)
                    {
                        Console.WriteLine($"No value for numeric flag {flag}");
                        Console.Read();
                        Environment.Exit(-1);
                    }
                    return argument.integerArrayList[0];
                }
            }
            Console.WriteLine($"No such flag {flag}");
            Console.Read();
            Environment.Exit(-1);
            return -1;
        }
        public static List<int> getIntegerValues(Flag flag)
        {
            foreach (Argument argument in arguments)
            {
                if (argument.flag == flag)
                {
                    if (argument.type != Argument.Type.NUMERIC)
                    {
                        Console.WriteLine($"Not a numeric flag {flag}");
                        Console.Read();
                        Environment.Exit(-1);
                    }
                    if (!argument.multiple)
                    {
                        Console.WriteLine($"Single value numeric flag {flag}");
                        Console.Read();
                        Environment.Exit(-1);
                    }
                    return argument.integerArrayList;
                }
            }
            Console.WriteLine($"No such flag {flag}");
            Console.Read();
            Environment.Exit(-1);
            return null;
        }
        public static String getStringValue(Flag flag)
        {
            foreach (Argument argument in arguments)
            {
                if (argument.flag == flag)
                {
                    if (argument.type != Argument.Type.STRING)
                    {
                        Console.WriteLine($"Not a string flag {flag}");
                        Console.Read();
                        Environment.Exit(-1);
                    }
                    if (argument.multiple)
                    {
                        Console.WriteLine($"Multiple value string flag {flag}");
                        Console.Read();
                        Environment.Exit(-1);
                    }
                    if (argument.stringArrayList.Count == 0)
                    {
                        Console.WriteLine($"No value for string flag {flag}");
                        Console.Read();
                        Environment.Exit(-1);
                    }
                    return argument.stringArrayList[0];
                }
            }
            Console.WriteLine($"No such flag {flag}");
            Console.Read();
            Environment.Exit(-1);
            return null;
        }
        public static List<String> getStringValues(Flag flag)
        {
            foreach (Argument argument in arguments)
            {
                if (argument.flag == flag)
                {
                    if (argument.type != Argument.Type.STRING)
                    {
                        Console.WriteLine($"Not a string flag {flag}");
                        Console.Read();
                        Environment.Exit(-1);
                    }
                    if (!argument.multiple)
                    {
                        Console.WriteLine($"Single value string flag {flag}");
                        Console.Read();
                        Environment.Exit(-1);
                    }
                    return argument.stringArrayList;
                }
            }
            Console.WriteLine($"No such flag {flag}");
            Console.Read();
            Environment.Exit(-1);
            return null;
        }
        public static bool getBooleanValue(Flag flag)
        {
            foreach (Argument argument in arguments)
            {
                if (argument.flag == flag)
                {
                    if (argument.type != Argument.Type.BOOLEAN)
                    {
                        Console.WriteLine($"Not a boolean flag {flag}");
                        Console.Read();
                        Environment.Exit(-1);
                    }
                    return argument.booleanValue;
                }
            }
            Console.WriteLine($"No such flag {flag}");
            Console.Read();
            Environment.Exit(-1);
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
            Console.WriteLine($"No such flag {flag}");
            Console.Read();
            Environment.Exit(-1);
            return false;
        }
        public static String getShortDesc(Flag flag)
        {
            foreach (Argument mainMenuArgument in arguments)
            {
                if (mainMenuArgument.flag == flag)
                {
                    return mainMenuArgument.shortDesc;
                }
            }
            return null;
        }

        private static void parseArguments(String[] args, bool setDefaults)
        {
            String error = parseArgumentsAndReturnError(args, setDefaults);
            if (error != null)
            {
                printUsage();
                Console.WriteLine(error);
                Console.Read();
                Environment.Exit(-1);
            }
        }
        private static String parseArgumentsAndReturnError(String[] args, bool setDefaults)
        {
            Argument currentArgument = null;

            foreach (String arg in args)
            {
                if (arg.ToUpper().StartsWith("-V"))
                {
                    printUsage();
                    return null;
                }
                if (arg.StartsWith("-") && currentArgument != null)
                {
                    return $"Invalid argument >{arg}<. Parameter missing for - {currentArgument.flag.ToString()} ({currentArgument.shortDesc})";
                }
                if (!arg.StartsWith("-") && currentArgument == null)
                {
                    return $"Invalid argument >{arg}<.\n";
                }
                if (arg.StartsWith("-"))
                {
                    currentArgument = getMainArgument(arg);
                    if (currentArgument == null)
                    {
                        return $"Invalid argument >{arg}<.\n";
                    }
                    if (currentArgument.type == Argument.Type.BOOLEAN)
                    {
                        currentArgument.booleanValue = true;
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
                            if (!currentArgument.multiple && currentArgument.integerArrayList.Count() > 0)
                            {
                                return $"Duplicate value >{arg}< for -{currentArgument.flag.ToString()} " +
                                    $"({currentArgument.shortDesc}).\nPrevious value >{currentArgument.integerArrayList[0]}<.\n";
                            }
                            currentArgument.integerArrayList.Add(value);
                            currentArgument.set = true;
                            currentArgument = null;
                            continue;
                        }
                        else
                        {
                            value = 0;
                        }
                    }
                    catch (FormatException ignored)
                    {
                    }
                    if (value == null)
                    {
                        return $"Invalid value >{arg}< for -{currentArgument.flag.ToString()} ({currentArgument.shortDesc}). \n" +
                                        $"Should be between {currentArgument.minIntValue} and {currentArgument.maxIntValue} " +
                                        $"(default is {currentArgument.defaultIntValue}).\n{currentArgument.longDesc}\n"; ;
                    }
                }

                if (currentArgument.type == Argument.Type.STRING)
                {
                    if (currentArgument.stringArrayList.Count() > 0 && !currentArgument.multiple)
                    {
                        return $"Duplicate value >{arg}< for -{currentArgument.flag.ToString()} ({currentArgument.shortDesc})." +
                            $"\nPrevious value {currentArgument.stringArrayList[0]}.\n";
                    }
                    if (currentArgument.validStringValues != null)
                    {
                        bool valid = false;
                        foreach (String validStringValue in currentArgument.validStringValues)
                        {
                            if (arg.Equals(validStringValue))
                            {
                                valid = true;
                                break;
                            }

                        }
                        if (!valid)
                        {
                            return $"Invalid value >{arg}< for -{currentArgument.flag.ToString()} ({currentArgument.shortDesc}). \n" +
                                            $"Should be one of {currentArgument.validStringValuesString} (default is {currentArgument.defaultStringValue}).\n{currentArgument.longDesc}\n";
                        }
                    }

                    currentArgument.stringArrayList.Add(arg);
                    currentArgument.set = true;
                    currentArgument = null;
                }
            }

            if (currentArgument != null)
            {
                return $"Parameter missing for -{currentArgument.flag.ToString()} ({currentArgument.shortDesc})\n";
            }

            if (setDefaults)
            {
                foreach (Argument arguments in arguments)
                {
                    if (arguments.type == Argument.Type.NUMERIC && arguments.integerArrayList.Count == 0 && !arguments.multiple)
                    {
                        if (arguments.required)
                        {
                            return $"Flag -{arguments.flag.ToString()} is mandatory but missing ({arguments.shortDesc})\n" +
                                            $"Should speficiy a value between {arguments.minIntValue} and {arguments.maxIntValue} " +
                                            $"(default is {arguments.defaultIntValue}).\n{arguments.longDesc}\n";
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
                            return $"Flag -{arguments.flag.ToString()} is mandatory but missing ({arguments.shortDesc}).\n{arguments.longDesc}\n";
                        }
                        else
                        {
                            arguments.stringArrayList.Add(arguments.defaultStringValue);
                        }
                    }
                }
            }
            return null;

        }
        private static void printArguments()
        {

            Console.WriteLine("Input Parameters\n");

            foreach (Argument arguments in arguments)
            {

                StringBuilder s = new StringBuilder($"{arguments.shortDesc} (-{arguments.flag.ToString()}): \t");

                switch (arguments.type)
                {
                    case Argument.Type.BOOLEAN:
                        s.Append(" ").Append(arguments.booleanValue);
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
                        foreach (String stringValue in arguments.stringArrayList)
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
                Console.WriteLine(s.ToString());

            }
            Console.WriteLine("");

        }
        private static Argument getMainArgument(String arg)
        {
            Argument currentArgument = null;
            foreach (Argument mainMenuArgument in arguments)
            {
                var flagString = mainMenuArgument.flag.ToString();
                var argString = arg.Substring(1);
                if (mainMenuArgument.flag.ToString().Equals(arg.Substring(1), StringComparison.OrdinalIgnoreCase))
                {
                    currentArgument = mainMenuArgument;
                    break;
                }
            }
            return currentArgument;
        }
        private static void printUsage()
        {

            Console.WriteLine("\nUsage: java -jar <jarname>.jar [arguments]\nArguments:\n");

            foreach (Argument currentArgument in arguments)
            {

                String prefix = $"\t-{currentArgument.flag.ToString()} \t{currentArgument.shortDesc}";

                switch (currentArgument.type)
                {
                    case Argument.Type.BOOLEAN:
                        if (currentArgument.required && currentArgument.multiple)
                        {
                            Console.WriteLine($"{prefix}  (required, one or more).\n\t\t{currentArgument.longDesc}\n");
                        }
                        else if (currentArgument.required)
                        {
                            Console.WriteLine($"{prefix}  (required).\n\t\t{currentArgument.longDesc}\n");
                        }
                        else if (currentArgument.multiple)
                        {
                            Console.WriteLine($"{prefix}  (optional, one or more).\n\t\t{currentArgument.longDesc}\n");
                        }
                        else
                        {
                            Console.WriteLine($"{prefix}  (optional).\n\t\t{currentArgument.longDesc}\n");
                        }
                        break;
                    case Argument.Type.NUMERIC:
                        if (currentArgument.required && currentArgument.multiple)
                        {
                            Console.WriteLine($"{prefix} \n\t\tShould specify a value between {currentArgument.minIntValue}" +
                                $" and {currentArgument.maxIntValue} (required, one or more).\n\t\t{currentArgument.longDesc}\n");
                        }
                        else if (currentArgument.required)
                        {
                            Console.WriteLine($"{prefix} \n\t\tShould specify a value between {currentArgument.minIntValue}" +
                                $" and {currentArgument.maxIntValue} (required).\n\t\t{currentArgument.longDesc}\n");
                        }
                        else if (currentArgument.multiple)
                        {
                            Console.WriteLine($"{prefix} \n\t\tShould specify a value between {currentArgument.minIntValue}" +
                                $" and {currentArgument.maxIntValue} (optional, one or more).\n\t\t{currentArgument.longDesc}\n");
                        }
                        else
                        {
                            Console.WriteLine($"{prefix} \n\t\tShould specify a value between {currentArgument.minIntValue}" +
                                $" and {currentArgument.maxIntValue} (optional, default is {currentArgument.defaultIntValue}).\n\t\t{currentArgument.longDesc}\n");
                        }
                        break;
                    case Argument.Type.STRING:
                        String validValuesAddition = "";
                        if (currentArgument.validStringValuesString != null && !(currentArgument.validStringValuesString.Length == 0))
                        {
                            validValuesAddition = " \n\t\tShould specify one of " + currentArgument.validStringValuesString;
                        }
                        if (currentArgument.required && currentArgument.multiple)
                        {
                            Console.WriteLine($"{prefix} {validValuesAddition} (required, one or more).\n\t\t{currentArgument.longDesc}\n");
                        }
                        else if (currentArgument.required)
                        {
                            Console.WriteLine($"{prefix} {validValuesAddition} (required).\n\t\t{currentArgument.longDesc}\n");
                        }
                        else if (currentArgument.multiple)
                        {
                            Console.WriteLine($"{prefix} {validValuesAddition} (optional, one or more).\n\t\t{currentArgument.longDesc}\n");
                        }
                        else
                        {
                            Console.WriteLine($"{prefix} {validValuesAddition} (optional, default is \"{currentArgument.defaultStringValue}\").\n\t\t{currentArgument.longDesc}\n");
                        }


                        break;
                    default:
                        break;
                }
            }
        }
    }
}
