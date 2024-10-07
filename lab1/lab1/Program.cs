using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace GeneticsProject
{
    public struct GeneticData
    {
        public string name; // protein name
        public string organism;
        public string formula; // formula
    }

    class Program
    {
        static List<GeneticData> data = new List<GeneticData>();
        static int count = 1;

        static void Main(string[] args)
        {
            ReadGeneticData("sequences.1.txt");
            ReadHandleCommands("commands.1.txt");
        }

        static string GetFormula(string proteinName)
        {
            foreach (GeneticData item in data)
            {
                if (item.name.Equals(proteinName)) return item.formula;
            }
            return null;
        }

        static void ReadGeneticData(string filename)
        {
            StreamReader reader = new StreamReader(filename);
            while (!reader.EndOfStream)
            {
                string line = reader.ReadLine();
                string[] fragments = line.Split('\t');
                GeneticData protein;
                protein.name = fragments[0];
                protein.organism = fragments[1];
                protein.formula = fragments[2];
                data.Add(protein);
                count++;
            }
            reader.Close();
        }

        static void ReadHandleCommands(string filename)
        {
            StreamReader reader = new StreamReader(filename);
            int counter = 0;
            List<string> outputLines = new List<string>(); // Список для хранения выходных строк

            // Записываем заголовок и имя
            outputLines.Add("Matsvei Voitsekhovich"); // Замените на свое имя
            outputLines.Add("Genetic search");

            while (!reader.EndOfStream)
            {
                string line = reader.ReadLine();
                counter++;
                string[] command = line.Split('\t');
                if (command[0].Equals("search"))
                {
                    outputLines.Add($"{counter.ToString("D3")}   {"search"}   {Decoding(command[1])}");
                    int index = Search(command[1]);
                    if (index != -1)
                        outputLines.Add($"{data[index].organism}    {data[index].name}");
                    else
                        outputLines.Add("NOT FOUND");
                    outputLines.Add("================================================");
                }
                else if (command[0].Equals("diff"))
                {
                    outputLines.Add($"{counter.ToString("D3")}   {"diff"}   {command[1]}   {command[2]}");
                    string result = Diff(command[1], command[2]);
                    outputLines.Add(result);
                    outputLines.Add("================================================");
                }
                else if (command[0].Equals("mode"))
                {
                    outputLines.Add($"{counter.ToString("D3")}   {"mode"}   {command[1]}");
                    string result = Mode(command[1]);
                    outputLines.Add(result);
                    outputLines.Add("================================================");
                }
            }
            reader.Close();

            WriteOutputToFile("genedata.txt", outputLines);
        }

        static void WriteOutputToFile(string filename, List<string> outputLines)
        {
            using (StreamWriter writer = new StreamWriter(filename))
            {
                foreach (var line in outputLines)
                {
                    writer.WriteLine(line);
                }
            }
        }

        static bool IsValid(string formula)
        {
            List<char> letters = new List<char>() { 'A', 'C', 'D', 'E', 'F', 'G', 'H', 'I', 'K', 'L', 'M', 'N', 'P', 'Q', 'R', 'S', 'T', 'V', 'W', 'Y' };
            foreach (char ch in formula)
            {
                if (!letters.Contains(ch)) return false;
            }
            return true;
        }

        static string Encoding(string formula)
        {
            string encoded = String.Empty;
            for (int i = 0; i < formula.Length; i++)
            {
                char ch = formula[i];
                int count = 1;
                while (i < formula.Length - 1 && formula[i + 1] == ch)
                {
                    count++;
                    i++;
                }
                if (count > 2) encoded += count + ch;
                if (count == 1) encoded += ch;
                if (count == 2) encoded += ch + ch;
            }
            return encoded;
        }

        static string Decoding(string formula)
        {
            string decoded = String.Empty;
            for (int i = 0; i < formula.Length; i++)
            {
                if (char.IsDigit(formula[i]))
                {
                    char letter = formula[i + 1];
                    int conversion = formula[i] - '0';
                    for (int j = 0; j < conversion - 1; j++) decoded += letter;
                    
                }
                else
                {
                    decoded += formula[i];
                }
            }
            return decoded;
        }

        static int Search(string amino_acid)
        {
            // FKIII -> FK3I
            string decoded = Decoding(amino_acid);
            for (int i = 0; i < data.Count; i++)
            {
                if (data[i].formula.Contains(decoded)) return i;
            }
            return -1;
        }

        static string Diff(string protein1, string protein2)
        {
            var proteinData1 = data.FirstOrDefault(p => p.name.Equals(protein1));
            var proteinData2 = data.FirstOrDefault(p => p.name.Equals(protein2));
            List<string> missing = new List<string>();

            if (string.IsNullOrEmpty(proteinData1.name)) missing.Add(protein1);
            if (string.IsNullOrEmpty(proteinData2.name)) missing.Add(protein2);

            if (missing.Count > 0)
            {
                return $"MISSING: {string.Join(", ", missing)}";
            }

            // Декодируем формулы аминокислот
            string decoded1 = Decoding(proteinData1.formula);
            string decoded2 = Decoding(proteinData2.formula);

            // Подсчитываем различия
            int differences = CountDifferences(decoded1, decoded2);
            return $"amino-acids difference: {differences}"; // Возвращаем количество различий
        }

        static int CountDifferences(string seq1, string seq2)
        {
            int maxLength = Math.Max(seq1.Length, seq2.Length);
            int count = 0;

            for (int i = 0; i < maxLength; i++)
            {
                char a1 = i < seq1.Length ? seq1[i] : '\0'; // Используем '\0' для заполнения
                char a2 = i < seq2.Length ? seq2[i] : '\0';

                if (a1 != a2) count++; // Увеличиваем счетчик различий
            }

            return count; // Возвращаем общее количество различий
        }


        static string Mode(string protein)
        {
            var proteinData = data.FirstOrDefault(p => p.name.Equals(protein));

            if (proteinData.name == null)
            {
                return $"MISSING: {protein}";
            }

            var aminoCount = new Dictionary<char, int>();

            foreach (char amino in proteinData.formula)
            {
                if (aminoCount.ContainsKey(amino))
                    aminoCount[amino]++;
                else
                    aminoCount[amino] = 1;
            }

            var maxAmino = aminoCount.OrderByDescending(kvp => kvp.Value).ThenBy(kvp => kvp.Key).First();
            return $"amino-acid occurs: {maxAmino.Key} {maxAmino.Value}";
        }
    }
}
