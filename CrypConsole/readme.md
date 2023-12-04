# CrypConsole

The ''CrypConsole'' application allows the execution of CrypTool 2 workspace manager files (cwm) in the Windows console. It is shipped with each CrypTool 2 installation.

## Basic idea

The basic idea is that you can execute your cwm files in the windows console without a need of starting CrypTool 2.
CrypConsole allows to input data (by filling TextInput components and NumberInputs components) and to retrieve outputs (from TextOutput components).
CrypConsole is loaded and executed much faster then CrypTool 2. Also it allows to use CrypTool 2 workspaces in scripts.

### Set Windows PATH variable for CrypConsole

To allow CrypConsole being executed in any windows command prompt, without the need to be in the CrypTool 2 directory, you should add it to the PATH variable of Windows. If your CrypTool 2 is installed in e.g. ''C:\program files\CrypTool 2'' you have to add this to your PATH variable. To edit your PATH go to system settings and search for environment. Then edit your system PATH variable and add the path of CrypTool 2.

### Usage of CrypConsole

When you open a new command prompt and just type "CrypConsole" you should see the following output:

```
 CrypConsole -- a CrypTool 2 console for executing CrypTool 2 workspaces in the Windows console 
 (C) 2023 cryptool.org; author: Nils Kopal, kopal<at>CrypTool.org
 Usage:
 CrypConsole.exe -cwm=path\to\file.cwm -input=type,name,data -output=name
 All arguments:
  -help                               -> shows this help page
  -discover                           -> discovers the given cwm file: returns all possible inputs and outputs
  -cwm=path\to\file.cwm               -> specifies a path to a cwm file that should be executed
  -input=type,name,data               -> specifies an input parameter
                                        type can be number,text,file
  -output=name                        -> specifies an output parameter
  -setting=name.settingname,value     -> specifies a setting of a component (name) to change to defined value
  -timeout=duration                   -> specifies a timeout in seconds. If timeout is reached, the process is killed
  -jsonoutput                         -> enables the json output
  -jsoninputfile=path\to\file.json    -> specifies a path to a json file that should be used as input
  -verbose                            -> writes logs etc to the console; for debugging
  -loglevel=Info/Debug/Warning/Error  -> changes the log level; default is "warning"
```

#### Discover a cwm file

CrypConsole allows to "discover" a cwm (CrypTool WorkspaceManager) file. Just enter
```
CrypConsole -cwm=caesar.cwm -discover
```
Here, for demonstration we use a simple workspace containing a Caesar cipher component, a TextInput component ("plaintext"), a TextOutput component ("ciphertext"), and a NumberInput component ("key"). You can create such a cwm file using CrypTool 2 and save it as "caesar.cwm":

![Caesar workspace used for demonstration](https://github.com/CrypToolProject/CrypTool-2/blob/main/documentation/images/Caesar_CrypConsole.png)

Here, you can download a "caesar.cwm" for testing: ![caesar.cwm](https://github.com/CrypToolProject/CrypTool-2/blob/main/documentation/images/caesar.cwm?raw=true)

Then, when you discover the caesar.cwm file, your output should look like:
```
Discovery of cwm_file "caesar.cwm"

"plaintext" ("CrypTool.TextInput.TextInput")
- Output connectors:
-- "TextOutput" ("System.String")
- Settings:
-- "ShowChars" ("System.Boolean")
-- "ShowLines" ("System.Boolean")
-- "ManualFontSettings" ("System.Boolean")
-- "Font" ("System.Int32")
-- "FontSize" ("System.Double")

"ciphertext" ("TextOutput.TextOutput")
...
```

The discover option allows to analyze a cwm file in the console. It shows the components and their input connectors, output connectors, and settings.

### Execute a cwm file

When you don't discover a file, it is automatically executed by CrypConsole. CrypConsole terminates when the workspace reaches 100%, so cwm files that do not reach 100% will cause CrypConsole to stay in an infinite execution loop. To avoid that you can (and should) always specify a timeout (defined in seconds):

```
CrypConsole -cwm=caesar.cwm -timeout=1
```

### Defining inputs to and obaining outputs from CrypConsole

When you execute a cwm file without specifying any input and output, the program will execute and terminate. There will be no result at all displayed. To define inputs, you can use the -input option:

```
CrypConsole -cwm=caesar.cwm -timeout=1 -input="text,plaintext, Hello world"
```
CrypConsole will now try to change the content of a TextInput component with name "plaintext". Here, the first argument defines the type of input ("text"), the second argument the component's name ("plaintext"), and the third argument the text. When you enclose everything in quotation marks, you can also enter spaces in your text.
Instead of providing text, you can also input a text file. To do so, use "file,plaintext,FILENAME", where FILENAME is the name of the file that should be used.

Still, we won't receive any output, since we did not define what output we expect. To do so, the -output option has to be used:
```
CrypConsole -cwm=caesar.cwm -timeout=1 -input="text,plaintext, Hello world" -output=ciphertext
```

Here, and in the rest of this documentation, we defined that we want to obtain the output of a TextOutput component named "ciphertext". When we execute CrypConsole using the above shown command, we should get this:
```
ciphertext=URYYB JBEYQ
```

The cwm file we used in this example contained four components: (1) a TextInput component named "plaintext", connected to a (2) Caesar component, connected to a (3) TextOutput component named "ciphertext", and (4) a NumberInput component named "key".

### Changing component settings

With -setting=ComponentName.SettingName,value you can change the value of a component's setting. If the setting is an enum, you have to provide the exact enum value as defined in the C# code. To get the exact name you can use -discover. For example, with -discover on CT2's Caesar template you find the following enum values for the Action setting:

```
- Settings:
-- "Action" ("CrypTool.Caesar.CaesarSettings+CaesarMode")
--- Possible values:
---> "Encrypt"
---> "Decrypt"
```

So with 

```
 -setting=Caesar.Action,Decrypt
```
you change the action of the Caesar component to "decrypt".

### Json input file

You may define the complete settings in a json file and provide it using the 
```
-jsoninputfile=path\to\file.json
```
command line argument. If you do so, all other command line arguments are ignored and only the json input file is used.

An example input json file looks like:

```
{
   "verbose":false,
   "timeout":30,
   "loglevel":"error",
   "jsonoutput":true,
   "cwmfile":"Path\\to\\Caesar.cwm",
   "inputs":[
      {
         "type":"text",
         "name":"Plaintext",
         "value":"rovvy gybvn"
      }
   ],
   "outputs":[
      {
         "name":"Ciphertext"
      }
   ],
   "settings":[
      {
         "component":"Caesar",
         "setting":"Action",
         "value":"Decrypt"
      }
   ]
}
```

### Some more options

With -verbose, CrypConsole shows debug messages. 

```
Input parameter given: Text,plaintext,Hello world
Output parameter given: Text,plaintext,Hello world
Loaded assembly: C:\Program Files\CrypTool 2\CrypPlugins\TextInput.dll
Loaded assembly: C:\Program Files\CrypTool 2\CrypPlugins\TextOutput.dll
Loaded assembly: C:\Program Files\CrypTool 2\CrypPlugins\Numbers.dll
Loaded assembly: C:\Program Files\CrypTool 2\CrypPlugins\Caesar.dll
Global progress change: 25%
Global progress change: 50%
Global progress change: 55%
Global progress change: 60%
Global progress change: 65%
Global progress change: 67%
Global progress change: 70%
Global progress change: 72%
Global progress change: 75%
Global progress reached 100%, stop execution engine now
Global progress change: 100%
Output: ciphertext=URYYB JBEYQ
ciphertext=URYYB JBEYQ
Execution engine stopped. Terminate now
Total execution took: 00:00:00.3133994
```

With -jsonoutput, CrypConsole outputs the results in a json structure. Also, the current workspace execution progress is shown:

```
{"progress":{"value":"25"}}
{"progress":{"value":"50"}}
{"progress":{"value":"52"}}
{"progress":{"value":"62"}}
{"progress":{"value":"67"}}
{"progress":{"value":"70"}}
{"progress":{"value":"72"}}
{"progress":{"value":"75"}}
{"progress":{"value":"100"}}
{"output":{"name":"ciphertext","value":"URYYB JBEYQ"}}
```
