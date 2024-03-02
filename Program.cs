using System.Text;

namespace MTTM
{
    public class Program
    {

        public static void Main(string[] args)
        {
            string program = "";
            string filePath = "";

            if (args.Length < 1) new Error("The program expects a '.mttm' code file as an argument!");
            else if (args.Length > 1) new Error("The program only expects a '.mttm' code file as an argument!");
            else filePath = args[0];

            if (!filePath.EndsWith(".mttm")) new Error("A Multi Tape Turing Machine script must have the '.mttm' file extension!");

            StreamReader sr = new StreamReader(filePath);
            program = sr.ReadToEnd();

            sr.Close();

            List<Token> tokens = Tokenizer.Tokenize(program);

            Dictionary<string, int[]> tapes = new Dictionary<string, int[]>();
            Dictionary<string, int> pointers = new Dictionary<string, int>();
            Dictionary<string, int> maxIndexes = new Dictionary<string, int>();
            Dictionary<string, int> labelMap = new Dictionary<string, int>();   //KEY: name,  line id
            Dictionary<int, int> ssMap = new Dictionary<int, int>();
            Dictionary<string, int> prevIndexes = new Dictionary<string, int>();

            ssMap = BuildSSMap();
            int status = 0;
            bool owerflow = true;
            int maxValue = 255;

            // initialize tapes;
            for (int i = 1; i <= 13; i++)
            {
                tapes.Add("T" + i, new int[1001]);
                for (int j = 0; j < 1001; j++)
                {
                    tapes["T" + i][j] = 0;
                }

                pointers.Add("T" + i, 0);

                maxIndexes.Add("T" + i, 1000);

                prevIndexes.Add("T" + i, 0);
            }

            BuildLabelMap();

            int start = 0;

        mainFlow:

            for (int i = start; i < tokens.Count; i++)
            {
                Token token = tokens[i];
                string key = token.tape.ToString();

                switch (tokens[i].opcode)
                {
                    case Opcode.None:
                        if (token.secondaryOpcode == SecondaryOpcode.None && token.args.Count < 1 && token.tape == Tape.None) continue;
                        new Error("A line " + (i + 1) + " must contain a valid command!");
                        break;
                    case Opcode.GO:
                        if (token.tape == Tape.None) new Error("The command 'GO' requires a tape to operate on ! Line " + (i + 1));

                        if (token.secondaryOpcode == SecondaryOpcode.None)
                        {
                            int newIndex = pointers[key];
                            if (newIndex == maxIndexes[key] && !owerflow) newIndex = maxIndexes[key];
                            else if (newIndex == maxIndexes[key] && owerflow) newIndex = 0;
                            else newIndex++;

                            prevIndexes[key] = pointers[key];
                            pointers[key] = newIndex;

                            foreach (string arg in token.args)
                            {
                                if (arg != "GO") new Error("The command 'GO' only accepts other 'GO' statements as arguments! Line " + (i + 1));
                                newIndex = pointers[key];
                                if (newIndex == maxIndexes[key] && !owerflow) newIndex = maxIndexes[key];
                                else if (newIndex == maxIndexes[key] && owerflow) newIndex = 0;
                                else newIndex++;

                                prevIndexes[key] = pointers[key];
                                pointers[key] = newIndex;
                            }
                        }
                        else if (token.secondaryOpcode == SecondaryOpcode.UNTIL)
                        {
                            if (token.args.Count > 1) new Error("The command 'GO.UNTIL' only requires one argument! Line " + (i + 1));
                            if (token.args.Count < 1) new Error("The command 'GO.UNTIL' requires one argument! Line " + (i + 1));

                            int numberWhenStop = ParseArgument(token.args[0], i + 1);


                            int newIndex = pointers[key];

                            while (tapes[key][newIndex] != numberWhenStop)
                            {
                                if (newIndex == maxIndexes[key])
                                {
                                    newIndex = pointers[key];
                                    break;
                                }
                                newIndex++;
                            }

                            prevIndexes[key] = pointers[key];
                            pointers[key] = newIndex;
                        }
                        else new Error("The command 'GO' only accepts 'UNTIL' as an optional secondery opcode! Line " + (i + 1));

                        break;
                    case Opcode.BACK:
                        if (token.tape == Tape.None) new Error("The command 'BACK' requires a tape to operate on ! Line " + (i + 1));

                        if (token.secondaryOpcode == SecondaryOpcode.None)
                        {
                            int newIndex = pointers[key] - 1;
                            if (newIndex == -1 && !owerflow) newIndex = 0;
                            else if (newIndex == -1 && owerflow) newIndex = maxIndexes[key];

                            prevIndexes[key] = pointers[key];
                            pointers[key] = newIndex;

                            foreach (string arg in token.args)
                            {
                                if (arg != "BACK") new Error("The command 'BACK' only accepts other 'BACK' statements as arguments! Line " + (i + 1));
                                newIndex = pointers[key] - 1;
                                if (newIndex == -1 && !owerflow) newIndex = 0;
                                else if (newIndex == -1 && owerflow) newIndex = maxIndexes[key];

                                prevIndexes[key] = pointers[key];
                                pointers[key] = newIndex;
                            }
                        }
                        else if (token.secondaryOpcode == SecondaryOpcode.UNTIL)
                        {
                            if (token.args.Count > 1) new Error("The command 'BACK.UNTIL' only requires one argument! Line " + (i + 1));
                            if (token.args.Count < 1) new Error("The command 'BACK.UNTIL' requires one argument! Line " + (i + 1));

                            int numberWhenStop = ParseArgument(token.args[0], i + 1);

                            int newIndex = pointers[key];

                            while (tapes[key][newIndex] != numberWhenStop)
                            {
                                newIndex--;
                                if (newIndex == -1)
                                {
                                    newIndex = pointers[key];
                                    break;
                                }
                            }

                            prevIndexes[key] = pointers[key];
                            pointers[key] = newIndex;
                        }
                        else new Error("The command 'BACK' only accepts 'UNTIL' as an optional secondery opcode! Line " + (i + 1));

                        break;
                    case Opcode.INC:
                        if (token.tape == Tape.None) new Error("The command 'INC' requires a tape to operate on ! Line " + (i + 1));

                        if (token.secondaryOpcode == SecondaryOpcode.None)
                        {

                            int newValue = tapes[key][pointers[key]] + 1;
                            if (newValue == maxValue + 1 && !owerflow) newValue = maxValue;
                            else if (newValue == maxValue + 1 && owerflow) newValue = 0 + (newValue - maxValue);

                            tapes[key][pointers[key]] = newValue;

                            foreach (string arg in token.args)
                            {
                                if (arg != "INC") new Error("The command 'INC' only accepts other 'INC' statements as arguments! Line " + (i + 1));

                                newValue = tapes[key][pointers[key]] + 1;
                                if (newValue == maxValue + 1 && !owerflow) newValue = maxValue;
                                else if (newValue == maxValue + 1 && owerflow) newValue = 0 + (newValue - maxValue);

                                tapes[key][pointers[key]] = newValue;
                            }
                        }
                        else new Error("The command 'INC' doesn't accept any secondery opcode! Line " + (i + 1));

                        break;
                    case Opcode.DEC:
                        if (token.tape == Tape.None) new Error("The command 'DEC' requires a tape to operate on ! Line " + (i + 1));

                        if (token.secondaryOpcode == SecondaryOpcode.None)
                        {

                            int newValue = tapes[key][pointers[key]] - 1;
                            if (newValue == -1 && !owerflow) newValue = 0;
                            else if (newValue == -1 && owerflow) newValue = maxValue;

                            tapes[key][pointers[key]] = newValue;

                            foreach (string arg in token.args)
                            {
                                if (arg != "DEC") new Error("The command 'DEC' only accepts other 'DEC' statements as arguments! Line " + (i + 1));

                                newValue = tapes[key][pointers[key]] - 1;
                                if (newValue == -1 && !owerflow) newValue = 0;
                                else if (newValue == -1 && owerflow) newValue = maxValue;

                                tapes[key][pointers[key]] = newValue;
                            }
                        }
                        else new Error("The command 'DEC' doesn't accept any secondery opcode! Line " + (i + 1));

                        break;
                    case Opcode.CP:
                        if (token.tape == Tape.None) new Error("The command 'CP' requires a tape to operate on ! Line " + (i + 1));
                        if (token.args.Count > 0) new Error("The commnad 'CP' doesn't accept any argument! Line " + (i + 1));
                        if (!(token.secondaryOpcode == SecondaryOpcode.GO || token.secondaryOpcode == SecondaryOpcode.BACK))
                        {
                            new Error("The command 'CP' only can used with the 'GO' or 'BACK' secondary opcode ! Line " + (i + 1));
                        }

                        if (token.secondaryOpcode == SecondaryOpcode.GO)
                        {
                            int intToCp = tapes[key][pointers[key]];

                            int nextIndex = pointers[key];
                            if (nextIndex == maxIndexes[key] && !owerflow) nextIndex = maxIndexes[key];
                            else if (nextIndex == maxIndexes[key] && owerflow) nextIndex = 0;
                            else nextIndex++;

                            tapes[key][nextIndex] = intToCp;
                        }
                        else if (token.secondaryOpcode == SecondaryOpcode.BACK)
                        {
                            int intToCp = tapes[key][pointers[key]];

                            int nextIndex = pointers[key] - 1;
                            if (nextIndex == -1 && !owerflow) nextIndex = 0;
                            else if (nextIndex == -1 && owerflow) nextIndex = maxIndexes[key];

                            tapes[key][nextIndex] = intToCp;
                        }
                        break;
                    case Opcode.LABEL:
                        /*if (token.secondaryOpcode != SecondaryOpcode.None)
                        {
                            new Error("A label only can contains letters and digits! Line " + (i + 1));
                        }
                        if (token.args.Count > 1) new Error("A label only can defined like this: 'someLabel123:'! Line " + (i + 1));
                        if (token.args.Count < 1) new Error("A label only can defined like this: 'someLabel123:'! Line " + (i + 1));

                        if (labelMap.ContainsKey(token.args[0]))
                        {
                            if (labelMap[token.args[0]] != i)
                            {
                                new Error("A label with the name: '" + token.args[0] + "' already exist! Error at line " + (i + 1));
                            }
                        }
                        else
                        {
                            labelMap.Add(token.args[0], i);
                        }*/
                        break;
                    case Opcode.CUT:    // ! cut the tape AFTER it!
                        if (token.tape == Tape.None) new Error("The command 'CUT' requires a tape to operate on ! Line " + (i + 1));
                        if (token.secondaryOpcode != SecondaryOpcode.None) new Error("The command 'CUT' cannot used with a secondary opcode! Line " + (i + 1));
                        if (token.args.Count > 0) new Error("The command 'CUT' doesn't require any arguments ! Line " + (i + 1));

                        if (pointers[key] != maxIndexes[key])
                        {
                            maxIndexes[key] = pointers[key];
                        }
                        break;
                    case Opcode.HALT:
                        if (token.secondaryOpcode != SecondaryOpcode.None) new Error("The command 'HALT' cannot used with a secondary opcode! Line " + (i + 1));
                        if (token.args.Count > 0) new Error("The command 'HALT' doesn't require any arguments ! Line " + (i + 1));

                        Environment.Exit(0);
                        break;
                    case Opcode.IF:
                        if (token.tape == Tape.None) new Error("The command 'IF' requires a tape to operate on ! Line " + (i + 1));
                        if (token.args.Count > 2) new Error("The command 'IF' only requires two arguments ! Line " + (i + 1));
                        if (token.args.Count < 2) new Error("The command 'IF' requires two arguments ! Line " + (i + 1));

                        if (token.secondaryOpcode == SecondaryOpcode.IS)
                        {
                            // Actual statement:
                            if (tapes[key][pointers[key]] == ParseArgument(token.args[0], i + 1))
                            {
                                if (labelMap.ContainsKey(token.args[1]))
                                {
                                    if (tokens.Count - 1 < labelMap[token.args[1]] + 1) Environment.Exit(0);
                                    start = labelMap[token.args[1]] + 1;
                                    goto mainFlow;
                                }
                                else
                                {
                                    new Error("Label: '" + token.args[1] + "' is not found! Error at line " + (i + 1));
                                }
                            }
                        }
                        else if (token.secondaryOpcode == SecondaryOpcode.NOT)
                        {
                            // Actual statement:
                            if (tapes[key][pointers[key]] != ParseArgument(token.args[0], i + 1))
                            {
                                if (labelMap.ContainsKey(token.args[1]))
                                {
                                    if (tokens.Count - 1 < labelMap[token.args[1]] + 1) Environment.Exit(0);
                                    start = labelMap[token.args[1]] + 1;
                                    goto mainFlow;
                                }
                                else
                                {
                                    new Error("Label: '" + token.args[1] + "' is not found! Error at line " + (i + 1));
                                }
                            }
                        }
                        else if (token.secondaryOpcode == SecondaryOpcode.LESS)
                        {
                            // Actual statement:
                            if (tapes[key][pointers[key]] < ParseArgument(token.args[0], i + 1))
                            {
                                if (labelMap.ContainsKey(token.args[1]))
                                {
                                    if (tokens.Count - 1 < labelMap[token.args[1]] + 1) Environment.Exit(0);
                                    start = labelMap[token.args[1]] + 1;
                                    goto mainFlow;
                                }
                                else
                                {
                                    new Error("Label: '" + token.args[1] + "' is not found! Error at line " + (i + 1));
                                }
                            }
                        }
                        else if (token.secondaryOpcode == SecondaryOpcode.BIGGER)
                        {
                            // Actual statement:
                            if (tapes[key][pointers[key]] > ParseArgument(token.args[0], i + 1))
                            {
                                if (labelMap.ContainsKey(token.args[1]))
                                {
                                    if (tokens.Count - 1 < labelMap[token.args[1]] + 1) Environment.Exit(0);
                                    start = labelMap[token.args[1]] + 1;
                                    goto mainFlow;
                                }
                                else
                                {
                                    new Error("Label: '" + token.args[1] + "' is not found! Error at line " + (i + 1));
                                }
                            }
                        }
                        else
                        {
                            new Error("The command 'IF' is only can used with the 'IS', 'NOT', 'LESS' or 'BIGGER' secondery opcode! Line " + (i + 1));
                        }
                        break;
                    case Opcode.GOTO:
                        if (token.secondaryOpcode != SecondaryOpcode.None) new Error("The command 'GOTO' cannot used with a secondary opcode! Line " + (i + 1));
                        if (token.args.Count > 1) new Error("The command 'GOTO' only requires one argument ! Line " + (i + 1));
                        if (token.args.Count < 1) new Error("The command 'GOTO' requires one argument ! Line " + (i + 1));

                        if (labelMap.ContainsKey(token.args[0]))
                        {
                            if (tokens.Count - 1 < labelMap[token.args[0]] + 1) Environment.Exit(0);
                            start = labelMap[token.args[0]] + 1;
                            goto mainFlow;
                        }
                        else
                        {
                            new Error("Label: '" + token.args[0] + "' is not found! Error at line " + (i + 1));
                        }
                        break;
                    case Opcode.IN:
                        if (token.tape == Tape.None) new Error("The command 'IN' requires a tape to operate on ! Line " + (i + 1));
                        if (token.secondaryOpcode != SecondaryOpcode.None) new Error("The command 'IN' cannot used with a secondary opcode! Line " + (i + 1));
                        if (token.args.Count > 0) new Error("The command 'IN' doesn't require any argument ! Line " + (i + 1));

                        string input = Console.ReadLine();
                        if (int.TryParse(input, out _))
                        {
                            int intIn = int.Parse(input);
                            if (intIn < 0) Console.WriteLine("Error! Your input is invalid!");
                            if (intIn > maxValue && !owerflow) intIn = maxValue;
                            else if (intIn > maxValue && owerflow) intIn = 0 + (intIn - maxValue);

                            tapes[key][pointers[key]] = intIn;
                        }
                        else if (Char.IsAscii(input[0]))
                        {
                            tapes[key][pointers[key]] = (int)input[0];
                        }
                        else Console.WriteLine("Error! Your input is invalid!");
                        break;
                    case Opcode.OUT:
                        if (token.tape == Tape.None) new Error("The command 'OUT' requires a tape to operate on ! Line " + (i + 1));
                        if (token.args.Count > 0) new Error("The command 'OUT' doesn't require any argument ! Line " + (i + 1));

                        if (token.secondaryOpcode == SecondaryOpcode.ASCII) Console.Write((char)tapes[key][pointers[key]]);
                        else if (token.secondaryOpcode == SecondaryOpcode.None) Console.Write(tapes[key][pointers[key]]);
                        else new Error("The command 'OUT' cannot used with a secondary opcode! Line " + (i + 1));
                        break;
                    case Opcode.ADD:
                        if (token.tape == Tape.None) new Error("The command 'ADD' requires a tape to operate on ! Line " + (i + 1));

                        if (token.secondaryOpcode == SecondaryOpcode.GO)
                        {
                            if (token.args.Count > 0) new Error("The commnad 'ADD.GO' doesn't accept any argument! Line " + (i + 1));
                            if (pointers[key] == maxIndexes[key]) new Error("There is no next cell! Error at line " + (i + 1));

                            int newIndex = pointers[key];
                            if (newIndex == maxIndexes[key] && !owerflow) newIndex = maxIndexes[key];
                            else if (newIndex == maxIndexes[key] && owerflow) newIndex = 0;
                            else newIndex++;

                            int newValue = tapes[key][pointers[key]] + tapes[key][newIndex];
                            if (newValue > maxValue && !owerflow) newValue = maxValue;
                            else if (newValue > maxValue && owerflow) newValue = 0 + (newValue - maxValue);

                            tapes[key][pointers[key]] = newValue;
                        }
                        else if (token.secondaryOpcode == SecondaryOpcode.BACK)
                        {
                            if (token.args.Count > 0) new Error("The commnad 'ADD.BACK' doesn't accept any argument! Line " + (i + 1));
                            if (pointers[key] == 0) new Error("There is no previous cell! Error at line " + (i + 1));

                            int newIndex = pointers[key] - 1;
                            if (newIndex == -1 && !owerflow) newIndex = 0;
                            else if (newIndex == -1 && owerflow) newIndex = maxIndexes[key];

                            int newValue = tapes[key][pointers[key]] + tapes[key][newIndex];
                            if (newValue > maxValue && !owerflow) newValue = maxValue;
                            else if (newValue > maxValue && owerflow) newValue = 0 + (newValue - maxValue);

                            tapes[key][pointers[key]] = newValue;
                        }
                        else if (token.secondaryOpcode == SecondaryOpcode.None)
                        {
                            if (token.args.Count > 1) new Error("The commnad 'ADD' only require one argument! Line " + (i + 1));
                            if (token.args.Count < 1) new Error("The commnad 'ADD' requires one argument! Line " + (i + 1));

                            int newValue = tapes[key][pointers[key]] + ParseArgument(token.args[0], i + 1);
                            if (newValue > maxValue && !owerflow) newValue = maxValue;
                            else if (newValue > maxValue && owerflow) newValue = 0 + (newValue - maxValue);

                            tapes[key][pointers[key]] = newValue;
                        }
                        else
                        {
                            new Error("The command 'ADD' cannot used with the '" + token.secondaryOpcode.ToString() + "' secondary opcode! Line " + (i + 1));
                        }
                        break;
                    case Opcode.SUB:
                        if (token.tape == Tape.None) new Error("The command 'SUB' requires a tape to operate on ! Line " + (i + 1));

                        if (token.secondaryOpcode == SecondaryOpcode.GO)
                        {
                            if (token.args.Count > 0) new Error("The commnad 'SUB.GO' doesn't accept any argument! Line " + (i + 1));
                            if (pointers[key] == maxIndexes[key]) new Error("There is no next cell! Error at line " + (i + 1));

                            int newIndex = pointers[key];
                            if (newIndex == maxIndexes[key] && !owerflow) newIndex = maxIndexes[key];
                            else if (newIndex == maxIndexes[key] && owerflow) newIndex = 0;
                            else newIndex++;

                            int newValue = tapes[key][pointers[key]] - tapes[key][newIndex];
                            if (newValue < 0 && !owerflow) newValue = 0;
                            else if (newValue < 0 && owerflow) newValue = maxValue + newValue + 1; // new value is negative so it's a substraction actually.

                            tapes[key][pointers[key]] = newValue;
                        }
                        else if (token.secondaryOpcode == SecondaryOpcode.BACK)
                        {
                            if (token.args.Count > 0) new Error("The commnad 'SUB.BACK' doesn't accept any argument! Line " + (i + 1));
                            if (pointers[key] == 0) new Error("There is no previous cell! Error at line " + (i + 1));

                            int newIndex = pointers[key] - 1;
                            if (newIndex == -1 && !owerflow) newIndex = 0;
                            else if (newIndex == -1 && owerflow) newIndex = maxIndexes[key];

                            int newValue = tapes[key][pointers[key]] - tapes[key][newIndex];
                            if (newValue < 0 && !owerflow) newValue = 0;
                            else if (newValue < 0 && owerflow) newValue = maxValue + newValue + 1; // new value is negative so it's a substraction actually. 0-1 shoud be maxvalue

                            tapes[key][pointers[key]] = newValue;
                        }
                        else if (token.secondaryOpcode == SecondaryOpcode.None)
                        {
                            if (token.args.Count > 1) new Error("The commnad 'SUB' only require one argument! Line " + (i + 1));
                            if (token.args.Count < 1) new Error("The commnad 'SUB' requires one argument! Line " + (i + 1));

                            int newValue = tapes[key][pointers[key]] - ParseArgument(token.args[0], i + 1);
                            if (newValue < 0 && !owerflow) newValue = 0;
                            else if (newValue < 0 && owerflow) newValue = maxValue + newValue + 1; // new value is negative so it's a substraction actually.

                            tapes[key][pointers[key]] = newValue;
                        }
                        else
                        {
                            new Error("The command 'SUB' cannot used with the '" + token.secondaryOpcode.ToString() + "' secondary opcode! Line " + (i + 1));
                        }
                        break;
                    case Opcode.MUL:
                        if (token.tape == Tape.None) new Error("The command 'MUL' requires a tape to operate on ! Line " + (i + 1));

                        if (token.secondaryOpcode == SecondaryOpcode.GO)
                        {
                            if (token.args.Count > 0) new Error("The commnad 'MUL.GO' doesn't accept any argument! Line " + (i + 1));
                            if (pointers[key] == maxIndexes[key]) new Error("There is no next cell! Error at line " + (i + 1));

                            int newIndex = pointers[key];
                            if (newIndex == maxIndexes[key] && !owerflow) newIndex = maxIndexes[key];
                            else if (newIndex == maxIndexes[key] && owerflow) newIndex = 0;
                            else newIndex++;

                            int newValue = tapes[key][pointers[key]] * tapes[key][newIndex];
                            if (newValue > maxValue && !owerflow) newValue = maxValue;
                            else if (newValue > maxValue && owerflow) newValue = 0 + (newValue - maxValue);

                            tapes[key][pointers[key]] = newValue;
                        }
                        else if (token.secondaryOpcode == SecondaryOpcode.BACK)
                        {
                            if (token.args.Count > 0) new Error("The commnad 'MUL.BACK' doesn't accept any argument! Line " + (i + 1));
                            if (pointers[key] == 0) new Error("There is no previous cell! Error at line " + (i + 1));

                            int newValue = tapes[key][pointers[key]] * tapes[key][pointers[key] - 1];
                            if (newValue > maxValue && !owerflow) newValue = maxValue;
                            else if (newValue > maxValue && owerflow) newValue = 0 + (newValue - maxValue);

                            tapes[key][pointers[key]] = newValue;
                        }
                        else if (token.secondaryOpcode == SecondaryOpcode.None)
                        {
                            if (token.args.Count > 1) new Error("The commnad 'MUL' only require one argument! Line " + (i + 1));
                            if (token.args.Count < 1) new Error("The commnad 'MUL' requires one argument! Line " + (i + 1));

                            int newValue = tapes[key][pointers[key]] * ParseArgument(token.args[0], i + 1);
                            if (newValue > maxValue && !owerflow) newValue = maxValue;
                            else if (newValue > maxValue && owerflow) newValue = 0 + (newValue - maxValue);

                            tapes[key][pointers[key]] = newValue;
                        }
                        else
                        {
                            new Error("The command 'MUL' cannot used with the '" + token.secondaryOpcode.ToString() + "' secondary opcode! Line " + (i + 1));
                        }
                        break;
                    case Opcode.DIV:
                        if (token.tape == Tape.None) new Error("The command 'DIV' requires a tape to operate on ! Line " + (i + 1));

                        if (token.secondaryOpcode == SecondaryOpcode.GO)
                        {
                            if (token.args.Count > 0) new Error("The commnad 'DIV.GO' doesn't accept any argument! Line " + (i + 1));
                            if (pointers[key] == maxIndexes[key]) new Error("There is no next cell! Error at line " + (i + 1));

                            int newIndex = pointers[key];
                            if (newIndex == maxIndexes[key] && !owerflow) newIndex = maxIndexes[key];
                            else if (newIndex == maxIndexes[key] && owerflow) newIndex = 0;
                            else newIndex++;

                            if (tapes[key][newIndex] == 0) new Error("You cannot devide by zero! Error at line " + (i + 1));

                            int newValue = (int)(tapes[key][pointers[key]] / tapes[key][newIndex]);

                            tapes[key][pointers[key]] = newValue;
                        }
                        else if (token.secondaryOpcode == SecondaryOpcode.BACK)
                        {
                            if (token.args.Count > 0) new Error("The commnad 'DIV.BACK' doesn't accept any argument! Line " + (i + 1));
                            if (pointers[key] == 0) new Error("There is no previous cell! Error at line " + (i + 1));

                            int newIndex = pointers[key] - 1;
                            if (newIndex == -1 && !owerflow) newIndex = 0;
                            else if (newIndex == -1 && owerflow) newIndex = maxIndexes[key];

                            if (tapes[key][newIndex] == 0) new Error("You cannot devide by zero! Error at line " + (i + 1));

                            int newValue = (int)(tapes[key][pointers[key]] / tapes[key][newIndex]);

                            tapes[key][pointers[key]] = newValue;
                        }
                        else if (token.secondaryOpcode == SecondaryOpcode.None)
                        {
                            if (token.args.Count > 1) new Error("The commnad 'DIV' only require one argument! Line " + (i + 1));
                            if (token.args.Count < 1) new Error("The commnad 'DIV' requires one argument! Line " + (i + 1));
                            if (ParseArgument(token.args[0], i + 1) == 0) new Error("You cannot devide by zero! Error at line " + (i + 1));

                            int newValue = (int)(tapes[key][pointers[key]] / ParseArgument(token.args[0], i + 1));

                            tapes[key][pointers[key]] = newValue;
                        }
                        else
                        {
                            new Error("The command 'DIV' cannot used with the '" + token.secondaryOpcode.ToString() + "' secondary opcode! Line " + (i + 1));
                        }
                        break;
                    case Opcode.MOD:
                        if (token.tape == Tape.None) new Error("The command 'MOD' requires a tape to operate on ! Line " + (i + 1));

                        if (token.secondaryOpcode == SecondaryOpcode.GO)
                        {
                            if (token.args.Count > 0) new Error("The commnad 'MOD.GO' doesn't accept any argument! Line " + (i + 1));
                            if (pointers[key] == maxIndexes[key]) new Error("There is no next cell! Error at line " + (i + 1));

                            int newIndex = pointers[key];
                            if (newIndex == maxIndexes[key] && !owerflow) newIndex = maxIndexes[key];
                            else if (newIndex == maxIndexes[key] && owerflow) newIndex = 0;
                            else newIndex++;

                            if (tapes[key][newIndex] == 0) new Error("You cannot devide by zero! Error at line " + (i + 1));

                            int newValue = tapes[key][pointers[key]] % tapes[key][newIndex];

                            tapes[key][pointers[key]] = newValue;
                        }
                        else if (token.secondaryOpcode == SecondaryOpcode.BACK)
                        {
                            if (token.args.Count > 0) new Error("The commnad 'MOD.BACK' doesn't accept any argument! Line " + (i + 1));
                            if (pointers[key] == 0) new Error("There is no previous cell! Error at line " + (i + 1));

                            int newIndex = pointers[key] - 1;
                            if (newIndex == -1 && !owerflow) newIndex = 0;
                            else if (newIndex == -1 && owerflow) newIndex = maxIndexes[key];

                            if (tapes[key][newIndex] == 0) new Error("You cannot devide by zero! Error at line " + (i + 1));

                            int newValue = tapes[key][pointers[key]] % tapes[key][newIndex];

                            tapes[key][pointers[key]] = newValue;
                        }
                        else if (token.secondaryOpcode == SecondaryOpcode.None)
                        {
                            if (token.args.Count > 1) new Error("The commnad 'MOD' only require one argument! Line " + (i + 1));
                            if (token.args.Count < 1) new Error("The commnad 'MOD' requires one argument! Line " + (i + 1));
                            if (ParseArgument(token.args[0], i + 1) == 0) new Error("You cannot devide by zero! Error at line " + (i + 1));

                            int newValue = tapes[key][pointers[key]] % ParseArgument(token.args[0], i + 1);

                            tapes[key][pointers[key]] = newValue;
                        }
                        else
                        {
                            new Error("The command 'MOD' cannot used with the '" + token.secondaryOpcode.ToString() + "' secondary opcode! Line " + (i + 1));
                        }
                        break;
                    case Opcode.RND:
                        if (token.tape == Tape.None) new Error("The command 'RND' requires a tape to operate on! Line " + (i + 1));
                        if (token.args.Count > 0) new Error("The commnad 'RND' doesn't accept any argument! Line " + (i + 1));

                        tapes[key][pointers[key]] = new Random().Next(0, maxValue);
                        break;
                    case Opcode.SS:
                        if (token.secondaryOpcode == SecondaryOpcode.START)
                        {
                            if (token.args.Count > 0) new Error("The command 'SS.START' doesn't accept any arguments! Line " + (i + 1));
                            if (!ssMap.ContainsKey(i)) new Error("The matching 'SS.END' is missing! Line " + (i + 1));
                            start = ssMap[i] + 1;
                            goto mainFlow;
                        }
                        else if (token.secondaryOpcode == SecondaryOpcode.END)
                        {
                            continue;
                        }
                        else new Error("The command 'SS." + token.secondaryOpcode.ToString() + "' is not found! Line " + (i + 1));
                        break;
                    case Opcode.STATUS:
                        if (token.secondaryOpcode != SecondaryOpcode.None) new Error("The command 'STATUS' cannot used with a secondery opcode! Line " + (i + 1));
                        if (token.args.Count > 1) new Error("The command 'STATUS' only requires one argument! Line " + (i + 1));
                        if (token.args.Count < 1) new Error("The command 'STATUS' requires one argument! Line " + (i + 1));

                        status = ParseArgument(token.args[0], i + 1);
                        break;
                    case Opcode.JUMP:
                        if (token.tape == Tape.None) new Error("The command 'JUMP' requires a tape to operate on! Line " + (i + 1));
                        if (token.args.Count > 0) new Error("The command 'JUMP' doesn't accept any arguments! Line " + (i + 1));

                        int numberToJump = tapes[key][pointers[key]];
                        int numbersJumped = 0;

                        if (token.secondaryOpcode == SecondaryOpcode.GO)
                        {
                            int IndexWhereStarted = pointers[key];
                            while (numbersJumped != numberToJump)
                            {
                                int newIndex = pointers[key];
                                if (newIndex == maxIndexes[key] && !owerflow) newIndex = maxIndexes[key];
                                else if (newIndex == maxIndexes[key] && owerflow) newIndex = 0;
                                else newIndex++;


                                pointers[key] = newIndex;

                                numbersJumped++;
                            }
                            if (numberToJump != 0) prevIndexes[key] = IndexWhereStarted;
                        }
                        else if (token.secondaryOpcode == SecondaryOpcode.BACK)
                        {
                            int IndexWhereStarted = pointers[key];
                            while (numbersJumped != numberToJump)
                            {
                                int newIndex = pointers[key] - 1;
                                if (newIndex == -1 && !owerflow) newIndex = 0;
                                else if (newIndex == -1 && owerflow) newIndex = maxIndexes[key];

                                pointers[key] = newIndex;

                                numbersJumped++;
                            }
                            if (numberToJump != 0) prevIndexes[key] = IndexWhereStarted;
                        }
                        else new Error("The command 'JUMP' only can used with the 'GO' or the 'BACK' secondary opcode! Line " + (i + 1));
                        break;
                    case Opcode.PREV:
                        if (token.tape == Tape.None) new Error("The command 'PREV' requires a tape to operate on ! Line " + (i + 1));
                        if (token.secondaryOpcode != SecondaryOpcode.None) new Error("The command 'PREV' cannot used with a secondary opcode! Line " + (i + 1));
                        if (token.args.Count > 0) new Error("The command 'PREV' doesn't require any arguments ! Line " + (i + 1));

                        int currentId = pointers[key];
                        pointers[key] = prevIndexes[key];
                        prevIndexes[key] = currentId;
                        break;
                    case Opcode.RESET:
                        if (token.tape == Tape.None) new Error("The command 'RESET' requires a tape to operate on! Line " + (i + 1));
                        if (token.args.Count > 0) new Error("The commnad 'RESET' doesn't accept any argument! Line " + (i + 1));

                        tapes[key][pointers[key]] = 0;
                        break;
                    case Opcode.SAVE:
                        if (token.secondaryOpcode != SecondaryOpcode.None) new Error("The command 'SAVE' cannot used with a secondery opcode! Line " + (i + 1));
                        if (token.args.Count > 1) new Error("The command 'SAVE' only requires one argument! Line " + (i + 1));
                        if (token.args.Count < 1) new Error("The command 'SAVE' requires one argument! Line " + (i + 1));
                        if (token.tape == Tape.None) new Error("The command 'SAVE' requires a tape to operate on! Line " + (i + 1));

                        SaveHandler.SaveTape(token.args[0], tapes[key], filePath);
                        break;
                    case Opcode.LOAD:
                        if (token.secondaryOpcode != SecondaryOpcode.None) new Error("The command 'LOAD' cannot used with a secondery opcode! Line " + (i + 1));
                        if (token.args.Count > 1) new Error("The command 'LOAD' only requires one argument! Line " + (i + 1));
                        if (token.args.Count < 1) new Error("The command 'LOAD' requires one argument! Line " + (i + 1));
                        if (token.tape == Tape.None) new Error("The command 'SAVE' requires a tape to operate on! Line " + (i + 1));

                        int newTapeLenght = maxIndexes[key];
                        tapes[key] = SaveHandler.LoadFromTapefile(token.args[0], maxIndexes[key] + 1, out newTapeLenght, filePath);
                        maxIndexes[key] = newTapeLenght - 1;
                        break;
                    case Opcode.OVERFLOW:
                        if (token.secondaryOpcode != SecondaryOpcode.None) new Error("The command 'OWERFLOW' cannot used with a secondery opcode! Line " + (i + 1));
                        if (token.args.Count > 1) new Error("The command 'OWERFLOW' only requires one argument! Line " + (i + 1));
                        if (token.args.Count < 1) new Error("The command 'OWERFLOW' requires one argument! Line " + (i + 1));

                        if (token.args[0] == "ON")
                        {
                            owerflow = true;
                        }
                        else if (token.args[0] == "OFF")
                        {
                            owerflow = false;
                        }
                        else
                        {
                            new Error("The command 'OWERFLOW' only accepts 'ON' or 'OFF' as arguments! Line " + (i + 1));
                        }
                        break;
                }
            }

            Dictionary<int, int> BuildSSMap()
            {
                List<int> startIdStack = new List<int>();

                Dictionary<int, int> returnMap = new Dictionary<int, int>();

                for (int i = 0; i < tokens.Count; i++)
                {
                    if (tokens[i].opcode != Opcode.SS) continue;

                    if (tokens[i].secondaryOpcode == SecondaryOpcode.START) startIdStack.Add(i);
                    else if (tokens[i].secondaryOpcode == SecondaryOpcode.END)
                    {
                        returnMap.Add(startIdStack.Last(), i);
                        startIdStack.RemoveAt(startIdStack.Count - 1);
                    }
                }
                return returnMap;
            }

            void BuildLabelMap()
            {
                for (int i = 0; i < tokens.Count; i++)
                {
                    Token token = tokens[i];

                    if (token.opcode != Opcode.LABEL) continue;

                    if (token.secondaryOpcode != SecondaryOpcode.None)
                    {
                        new Error("A label only can contains letters and digits! Line " + (i + 1));
                    }
                    if (token.args.Count > 1) new Error("A label only can defined like this: 'someLabel123:'! Line " + (i + 1));
                    if (token.args.Count < 1) new Error("A label only can defined like this: 'someLabel123:'! Line " + (i + 1));

                    if (labelMap.ContainsKey(token.args[0]))
                    {
                        if (labelMap[token.args[0]] != i)
                        {
                            new Error("A label with the name: '" + token.args[0] + "' already exist! Error at line " + (i + 1));
                        }
                    }
                    else
                    {
                        labelMap.Add(token.args[0], i);
                    }
                }
            }

            int ParseArgument(string arg, int line)
            {
                if (arg.Where(c => c == '$').ToList().Count > 1)
                {
                    new Error("The given argument is invalid! Line " + line);
                    return 0; // never get called actually...
                }

                if (arg.StartsWith('$'))
                {
                    if (arg == "$STATUS") return status;
                    else if (arg[1] == 'T')
                    {
                        string key = arg.Split('$')[1];
                        return tapes[key][pointers[key]];
                    }
                    else
                    {
                        new Error("The given argument is invalid! Line " + line);
                        return 0;   // never get called actually...
                    }
                }
                else if (int.TryParse(arg, out _))
                {
                    return int.Parse(arg);
                }
                else
                {
                    new Error("The given argument is invalid! Line " + line);
                    return 0;   // never get called actually...
                }
            }
        }
    }
}
