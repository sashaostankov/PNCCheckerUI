//
//  Ostankov Alexander
//  Higher School of Economics
//  sashaostankov@gmail.com
//  (c) 2015
//

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

using Property = System.Collections.Generic.SortedDictionary<string, string>;
using PetriNet = System.Collections.Generic.SortedDictionary<int, 
                 System.Collections.Generic.List<
                 System.Collections.Generic.SortedDictionary<string, string>>>;

namespace PNTFReader
{
    public static class FileReader
    {
        const string INVALID_DATA = "Неправильный формат входных данных";

        enum Statements
        {
            None,
            ReadNet,
            ReadArc,
            ReadNode
        }

        public static PetriNet PNReadNet(string fileName)
        {
            var StreamIn = new StreamReader(fileName,
                Encoding.ASCII,
                true);
            var state = Statements.None;
            var Net = new PetriNet();

            while (!StreamIn.EndOfStream)
            {
                string line = StreamIn.ReadLine();
                int tabCount = GetTabCount(line);
                line = line.TrimStart();

                if (line.Length == 0)
                    continue;

                switch (tabCount)
                {
                    case 0:
                        if (line.ToLower() != "net")
                            throw new Exception(INVALID_DATA);
                        if (state != Statements.None)
                            throw new Exception("Чтение файлов с несколько сетями Петри недоступно"); // Read a new Petri Net;

                        state = Statements.ReadNet;
                        Net = new PetriNet();

                        break;
                    case 1:
                        switch (state)
                        {
                            case Statements.None:
                                throw new Exception(INVALID_DATA);
                            case Statements.ReadNet:
                                Net.Add(0, new List<Property>());
                                Net.Add(1, new List<Property>());
                                break;
                        }

                        switch (line.ToLower())
                        {
                            case "node":
                                Net[0].Add(new Property());
                                state = Statements.ReadNode;
                                break;
                            case "arc":
                                Net[1].Add(new Property());
                                state = Statements.ReadArc;
                                break;
                            default:
                                throw new Exception(INVALID_DATA);
                        }

                        break;
                    case 2:
                        int index = line.IndexOf("=", StringComparison.Ordinal);

                        if (index == -1)
                            throw new Exception(INVALID_DATA);

                        if (state != Statements.ReadArc &&
                            state != Statements.ReadNode)
                            throw new Exception(INVALID_DATA);

                        string name = line.Substring(0, index).Trim();
                        string value = line.Substring(index + 1);

                        switch (state)
                        {
                            case Statements.ReadNode:
                                Net[0][Net[0].Count - 1].Add(name, value);
                                break;
                            case Statements.ReadArc:
                                Net[1][Net[1].Count - 1].Add(name, value);
                                break;
                        }

                        break;
                    default:
                        throw new Exception(INVALID_DATA);
                }
            }

            StreamIn.Close();

            if (Net.Count == 0)
                throw new Exception(INVALID_DATA);

            return Net;
        }

        public static int GetTabCount(string s)
        {
            return Array.FindIndex(s.ToCharArray(), x => x != '\t');
        }

        public static Dictionary<List<string>, int> PNReadLog(string fileName)
        {
            var StreamIn = new StreamReader(fileName, Encoding.ASCII, true);
            var inputData = StreamIn.ReadToEnd();
            var inputLog = inputData.Split(new []{ '\n' });

            var intermid = new Dictionary<string, int>();

            foreach (var str in inputLog)
            {
                if (intermid.ContainsKey(str))
                    intermid[str]++;
                else
                    intermid.Add(str, 1);
            }

            return ConvertToNormalLog(intermid);
        }

        public static Dictionary<List<string>, int> ConvertToNormalLog(Dictionary<string, int> log)
        {
            var result = new Dictionary<List<string>, int>();

            foreach (var caseLog in log)
            {
                var list = new List<string>(caseLog.Key.Split(new []{ ';' }));

                if (list.FindIndex(x => (string.IsNullOrEmpty(x) || x == "\r")) != -1)
                    throw new Exception("Лог-файл содержит ошибки! Неправильная строка: '" + caseLog.Key + "'");

                for (int i = 0; i < list.Count; i++)
                    if (list[i].EndsWith("\r"))
                        list[i] = list[i].Remove(list[i].Length - 1);

                result.Add(list, caseLog.Value);
            }

            return result;
        }
    }
}