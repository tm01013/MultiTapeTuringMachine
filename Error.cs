namespace MTTM
{
    public class Error
    {
        public Error(string customMessage)
        {
            Console.WriteLine();
            Console.WriteLine("----------FATAL-ERROR-----------");
            Console.WriteLine(customMessage);
            Environment.Exit(1);
        }

        public Error(ErrorCodes code, int line, string wrongObject)
        {
            Console.WriteLine();
            Console.WriteLine("----------FATAL-ERROR-----------");
            switch (code)
            {
                case ErrorCodes.InvalidLineStructure:
                    Console.WriteLine("Line " + line + " has an invalid line structure!");
                    break;
                case ErrorCodes.TapeNotFound:
                    Console.WriteLine(wrongObject + " tape not found! Line " + line);
                    Console.WriteLine("Tapes must be in the range of T1...T13]");
                    break;
                case ErrorCodes.InvalidCommand:
                    Console.WriteLine(wrongObject + " is an invalid command! Line " + line);
                    break;
                case ErrorCodes.ArgumentsExcepted:
                    Console.WriteLine("The command at line " + line + " expects arguments!");
                    break;
                case ErrorCodes.InvalidArgument:
                    Console.WriteLine("The argument specified for the command at line " + line + " is invalid!");
                    break;
                case ErrorCodes.LabelNotFound:
                    Console.WriteLine(wrongObject + " label not found! Line " + line);
                    break;
                case ErrorCodes.LabelAlreadyExist:
                    Console.WriteLine(wrongObject + " label is already exist! Line " + line);
                    break;
            }
            Environment.Exit(1);
        }
    }

    public enum ErrorCodes
    {
        InvalidLineStructure,
        TapeNotFound,
        InvalidCommand,
        ArgumentsExcepted,
        InvalidArgument,
        LabelNotFound,
        LabelAlreadyExist,
        /*MatchingSSTagExcepted,

        NoSutchTapeFile,
        NoSutchTapeLibary,
        NoSutchTapeInTheSpecifiedLibary*/
    }
}