using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSVDataNS
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            List<string> columns = new List<string>() { "A", "B", "C", "D" };
            List<List<string>> rows = new List<List<string>>();
            rows.Add(new List<string>() { "a", "b", "c", "d" });
            rows.Add(new List<string>() { "a", "b", "c", "e" });
            rows.Add(new List<string>() { "a", "c", "b", "f" });
            rows.Add(new List<string>() { "b", "c", "b", "f" });
            rows.Add(new List<string>() { "b", "c", "b", "f" });
            CSVDataNS.CSVData data = new CSVDataNS.CSVData(columns, rows);
            Console.WriteLine(data.ToCSVString());
            Func<List<string>, string> joiner = xs => String.Join("", xs);
            Dictionary<string, Func<List<string>, string>> dict = new Dictionary<string, Func<List<string>, string>>();
            dict["C"] = joiner;
            dict["D"] = joiner;
            List<string> keys = new List<string>() { "A", "B" };
            data.GroupBy(keys, dict);
        }
    }
}