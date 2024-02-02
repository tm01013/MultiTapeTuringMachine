
using System.IO;
using System.Text.RegularExpressions;

namespace MTTM
{
    public static class SaveHandler
    {
        public static int[] LoadFromTapefile(string path, int maxTapeLenght, out int tapeLenght, string masterPath)
        {
            string fileContents = "";

            StreamReader sr = new StreamReader(Path.Combine(Directory.GetParent(Directory.GetParent(masterPath).FullName).FullName, path));
            fileContents = sr.ReadToEnd();
            sr.Close();

            List<string> lines = fileContents.Split('\n').ToList();

            foreach (string line in lines.ToArray())
            {
                if (String.IsNullOrWhiteSpace(line)) lines.Remove(line);
            }

            if (lines[0].Trim() != "# MTTM tapefile #") new Error("The file at path '" + Path.Combine(Directory.GetParent(masterPath).FullName, path) + "' isn't contains a valid tapefile!");
            if (!Path.Combine(Directory.GetParent(masterPath).FullName, path).EndsWith(".tmt")) new Error("The file at path '" + Path.Combine(Directory.GetParent(masterPath).FullName, path) + "' is not a tapefile [it must have the '.tmt' file extension]!");

            List<int> tape = new List<int>();

            foreach (string cell in lines[1].Trim().Split(',', '[', ']', ';', '-'))
            {
                if (tape.Count == maxTapeLenght) break;
                if (String.IsNullOrWhiteSpace(cell)) continue;
                if (!int.TryParse(cell, out _)) new Error("The file at path '" + Path.Combine(Directory.GetParent(masterPath).FullName, path) + "' isn't contains a valid tapefile!");
                tape.Add(int.Parse(cell));
            }

            tapeLenght = tape.ToArray().Length;
            return tape.ToArray();
        }

        public static void SaveTape(string path, int[] tape, string masterPath)
        {
            if (!path.EndsWith(".tmt")) path += ".tmt";
            FileStream stream = new FileStream(Path.Combine(Directory.GetParent(masterPath).FullName, path), FileMode.OpenOrCreate);

            using (StreamWriter writer = new StreamWriter(stream))
            {
                writer.WriteLine("# MTTM tapefile #");
                writer.Write("[");

                for (int i = 0; i < tape.Length; i++)
                {
                    if (i != 0) writer.Write(",");
                    writer.Write(tape[i].ToString());
                }
                writer.Write("]");
            }
        }
    }
}