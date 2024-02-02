

using System.Text.RegularExpressions;

namespace MTTM
{
    public static class Tokenizer
    {
        public static List<Token> Tokenize(string code)
        {
            code = Regex.Replace(code, "//.*", "");
            code = Regex.Replace(code, "\n\n", "\n");
            code = Regex.Replace(code, "\t", "");
            string[] lines = code.Split('\n', '\r', ';');

            List<Token> tokens = new List<Token>();
            for (int i = 0; i < lines.Length; i++)
            {
                if (TokenizeLine(lines[i], i + 1) != null) tokens.Add(TokenizeLine(lines[i], i + 1));
            }

            return tokens;
        }

        static Token TokenizeLine(string line, int lineNumber)
        {
            if (String.IsNullOrEmpty(line)) return null;

            line = line.Trim();

            Token token = new Token();

            string[] words = line.Split(' ');
            if (words[0] == "//") return null;

            foreach (string word in words)
            {
                if (String.IsNullOrEmpty(word)) continue;

                else if (token.tape == Tape.None)
                {
                    if (word.StartsWith('T') && word.EndsWith('>'))
                    {
                        if (token.opcode != Opcode.None) new Error(ErrorCodes.InvalidLineStructure, lineNumber, null);
                        token.tape = (Tape)int.Parse(word.Split('T', '>')[1].ToString());
                        continue;
                    }

                }
                if (token.opcode == Opcode.None)
                {
                    if (word.ToCharArray().Last() == ':')
                    {
                        token.opcode = Opcode.LABEL;
                        token.args.Add(word.Split(':')[0]);
                        return token;
                    }

                    switch (word.Split('.').First())
                    {
                        case "GO":
                            token.opcode = Opcode.GO;
                            goto seconderyOpcode;

                        case "BACK":
                            token.opcode = Opcode.BACK;
                            goto seconderyOpcode;

                        case "INC":
                            token.opcode = Opcode.INC;
                            goto seconderyOpcode;

                        case "DEC":
                            token.opcode = Opcode.DEC;
                            goto seconderyOpcode;

                        case "CP":
                            token.opcode = Opcode.CP;
                            goto seconderyOpcode;

                        case "CUT":
                            token.opcode = Opcode.CUT;
                            goto seconderyOpcode;

                        case "HALT":
                            token.opcode = Opcode.HALT;
                            goto seconderyOpcode;

                        case "IF":
                            token.opcode = Opcode.IF;
                            goto seconderyOpcode;

                        case "GOTO":
                            token.opcode = Opcode.GOTO;
                            goto seconderyOpcode;

                        case "IN":
                            token.opcode = Opcode.IN;
                            goto seconderyOpcode;

                        case "OUT":
                            token.opcode = Opcode.OUT;
                            goto seconderyOpcode;

                        case "ADD":
                            token.opcode = Opcode.ADD;
                            goto seconderyOpcode;

                        case "SUB":
                            token.opcode = Opcode.SUB;
                            goto seconderyOpcode;

                        case "MUL":
                            token.opcode = Opcode.MUL;
                            goto seconderyOpcode;

                        case "DIV":
                            token.opcode = Opcode.DIV;
                            goto seconderyOpcode;

                        case "MOD":
                            token.opcode = Opcode.MOD;
                            goto seconderyOpcode;

                        case "RND":
                            token.opcode = Opcode.RND;
                            goto seconderyOpcode;

                        case "SS":
                            token.opcode = Opcode.SS;
                            goto seconderyOpcode;

                        case "STATUS":
                            token.opcode = Opcode.STATUS;
                            goto seconderyOpcode;

                        case "JUMP":
                            token.opcode = Opcode.JUMP;
                            goto seconderyOpcode;

                        case "PREV":
                            token.opcode = Opcode.PREV;
                            goto seconderyOpcode;

                        case "RESET":
                            token.opcode = Opcode.RESET;
                            goto seconderyOpcode;

                        case "OVERFLOW":
                            token.opcode = Opcode.OVERFLOW;
                            goto seconderyOpcode;

                        case "SAVE":
                            token.opcode = Opcode.SAVE;
                            goto seconderyOpcode;

                        case "LOAD":
                            token.opcode = Opcode.LOAD;
                            goto seconderyOpcode;

                        default:
                            new Error(ErrorCodes.InvalidCommand, lineNumber, word);
                            break;
                    }

                seconderyOpcode:
                    if (token.secondaryOpcode == SecondaryOpcode.None && word.Split('.').Length >= 2 && word.Contains('.'))
                    {
                        switch (word.Split('.')[1])
                        {
                            case "GO":
                                token.secondaryOpcode = SecondaryOpcode.GO;
                                continue;

                            case "BACK":
                                token.secondaryOpcode = SecondaryOpcode.BACK;
                                continue;

                            case "UNTIL":
                                token.secondaryOpcode = SecondaryOpcode.UNTIL;
                                continue;

                            case "IS":
                                token.secondaryOpcode = SecondaryOpcode.IS;
                                continue;

                            case "NOT":
                                token.secondaryOpcode = SecondaryOpcode.NOT;
                                continue;

                            case "LESS":
                                token.secondaryOpcode = SecondaryOpcode.LESS;
                                continue;

                            case "BIGGER":
                                token.secondaryOpcode = SecondaryOpcode.BIGGER;
                                continue;

                            case "START":
                                token.secondaryOpcode = SecondaryOpcode.START;
                                continue;

                            case "END":
                                token.secondaryOpcode = SecondaryOpcode.END;
                                continue;

                            case "ASCII":
                                token.secondaryOpcode = SecondaryOpcode.ASCII;
                                continue;

                            /*case "LIBARY":
                                token.secondaryOpcode = SecondaryOpcode.LIBARY;
                                continue;*/

                            default:
                                new Error(ErrorCodes.InvalidCommand, lineNumber, word);
                                break;
                        }
                    }
                }
                else token.args.Add(word);
            }

            return token;
        }
    }

    public class Token
    {
        public Opcode opcode;
        public SecondaryOpcode secondaryOpcode;
        public Tape tape;
        public List<string> args;

        public Token()
        {
            opcode = Opcode.None;
            secondaryOpcode = SecondaryOpcode.None;
            tape = Tape.None;
            args = new List<string>();
        }
    }

    public enum Opcode
    {
        None,

        GO,
        BACK,
        INC,
        DEC,
        CP,

        LABEL,  // actually there is no LABEL command
        CUT,
        HALT,
        IF,
        GOTO,

        IN,
        OUT,

        ADD,
        SUB,
        MUL,
        DIV,
        MOD,
        RND,

        SS,

        STATUS,

        JUMP,
        PREV,

        RESET,
        OVERFLOW,

        SAVE,
        LOAD
    }
    public enum SecondaryOpcode
    {
        None,

        GO,     // For add sub
        BACK,

        UNTIL,  //for go, back

        IS,     // For ifs
        NOT,
        LESS,
        BIGGER,

        START,  // SS
        END,

        ASCII,  //out

        //LIBARY  //save, load
    }
    public enum Tape
    {
        None,
        T1,
        T2,
        T3,
        T4,
        T5,
        T6,
        T7,
        T8,
        T9,
        T10,
        T11,
        T12,
        T13
    }
}