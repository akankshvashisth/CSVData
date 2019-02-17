using Microsoft.VisualStudio.TestTools.UnitTesting;
using CSVDataNS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSVDataNS.Tests
{
    [TestClass()]
    public class CSVDataTests
    {
        private static Tuple<List<string>, List<List<string>>> BuildTestCSVData()
        {
            List<string> columns = new List<string>() { "A", "B", "C", "D", "E" };
            List<List<string>> rows = new List<List<string>>();
            rows.Add(new List<string>() { "a", "b", "c", "d", "2.5" });
            rows.Add(new List<string>() { "a", "b", "c", "e", "2.5" });
            rows.Add(new List<string>() { "a", "c", "b", "f", "2.5" });
            rows.Add(new List<string>() { "b", "c", "b", "f", "2.5" });
            rows.Add(new List<string>() { "b", "c", "b", "f", "2.5" });
            rows.Add(new List<string>() { "a", "b", "c", "e", "2.5" });
            rows.Add(new List<string>() { "b", "e", "c", "e", "2.5" });
            return new Tuple<List<string>, List<List<string>>>(columns, rows);
        }

        private static void AssertCSVEqual(CSVData left, CSVData right)
        {
            Assert.IsTrue(left.Cols.SequenceEqual(right.Cols));

            Assert.AreEqual(left.RowsCount, right.RowsCount);

            for (int i = 0; i < left.RowsCount; ++i)
            {
                Assert.IsTrue(left.GetRow(i).SequenceEqual(right.GetRow(i)));
            }
        }

        [TestMethod()]
        public void CSVDataTest()
        {
            var rawData = BuildTestCSVData();
            CSVData data = new CSVData(rawData.Item1, rawData.Item2);
            Assert.IsTrue(data.Cols.SequenceEqual(rawData.Item1));
            Assert.AreEqual(data.RowsCount, rawData.Item2.Count);
        }

        [TestMethod()]
        public void CSVDataCopyCtorTest()
        {
            var rawData = BuildTestCSVData();
            CSVData data = new CSVData(rawData.Item1, rawData.Item2);
            CSVData copy = new CSVData(data);

            Assert.IsTrue(data.Cols.SequenceEqual(rawData.Item1));
            Assert.AreEqual(data.RowsCount, rawData.Item2.Count);

            Assert.IsTrue(copy.Cols.SequenceEqual(rawData.Item1));
            Assert.AreEqual(copy.RowsCount, rawData.Item2.Count);

            AssertCSVEqual(data, copy);

            copy.RenameColumn("C", "F");

            Assert.IsTrue(data.Cols.SequenceEqual(rawData.Item1));
            Assert.IsFalse(copy.Cols.SequenceEqual(rawData.Item1));

            Assert.AreEqual(data.GetElement(2, "B"), copy.GetElement(2, "B"));

            copy.SetElement(2, "B", "Foo");

            Assert.AreNotEqual(data.GetElement(2, "B"), copy.GetElement(2, "B"));
        }

        [TestMethod()]
        public void GetRowsRangeTest()
        {
            var rawData = BuildTestCSVData();
            CSVData data = new CSVData(rawData.Item1, rawData.Item2);
            var rows = data.GetRowsRange(1, 3);

            for (int i = 0; i < 3; ++i)
            {
                Assert.IsTrue(rawData.Item2[i + 1].SequenceEqual(rows[i]));
            }
        }

        [TestMethod()]
        public void GetRowTest()
        {
            var rawData = BuildTestCSVData();
            CSVData data = new CSVData(rawData.Item1, rawData.Item2);

            var row = data.GetRow(3);

            Assert.IsTrue(rawData.Item2[3].SequenceEqual(row));
        }

        [TestMethod()]
        public void GetElementTest()
        {
            var rawData = BuildTestCSVData();
            CSVData data = new CSVData(rawData.Item1, rawData.Item2);

            var element = data.GetElement(3, "B");

            Assert.AreEqual(element, rawData.Item2[3][rawData.Item1.IndexOf("B")]);
        }

        [TestMethod()]
        public void SetElementTest()
        {
            var rawData = BuildTestCSVData();
            CSVData data = new CSVData(rawData.Item1, rawData.Item2);

            data.SetElement(3, "B", "Foo");

            var element = data.GetElement(3, "B");

            Assert.AreEqual(element, "Foo");
        }

        [TestMethod()]
        public void GetColumnRawTest()
        {
            var rawData = BuildTestCSVData();
            CSVData data = new CSVData(rawData.Item1, rawData.Item2);

            var col = data.GetColumnRaw("C");

            int cIdx = rawData.Item1.IndexOf("C");
            Assert.IsTrue(rawData.Item2.Select(r => r[cIdx]).SequenceEqual(col));
        }

        [TestMethod()]
        public void GetColumnTest()
        {
            var rawData = BuildTestCSVData();
            CSVData data = new CSVData(rawData.Item1, rawData.Item2);

            int cIdx = rawData.Item1.IndexOf("C");
            CSVData C_CSVData = new CSVData(new List<string>() { "C" }, rawData.Item2.Select(r => new List<string>() { r[cIdx] }).ToList());

            CSVData col = data.GetColumn("C");

            AssertCSVEqual(col, C_CSVData);
        }

        [TestMethod()]
        public void RenameColumnTest()
        {
            var rawData = BuildTestCSVData();
            CSVData data = new CSVData(rawData.Item1, rawData.Item2);

            data.RenameColumn("B", "Foo");

            rawData.Item1[rawData.Item1.IndexOf("B")] = "Foo";

            Assert.IsTrue(data.Cols.SequenceEqual(rawData.Item1));
        }

        [TestMethod()]
        public void GetColumnsTest()
        {
            var rawData = BuildTestCSVData();
            CSVData data = new CSVData(rawData.Item1, rawData.Item2);

            CSVData cols = data.GetColumns(new List<string>() { "E", "B", "C" });

            int eIdx = rawData.Item1.IndexOf("E");
            int bIdx = rawData.Item1.IndexOf("B");
            int cIdx = rawData.Item1.IndexOf("C");
            CSVData test = new CSVData(
                new List<string>() { "E", "B", "C" },
                rawData.Item2.Select(r => new List<string>() { r[eIdx], r[bIdx], r[cIdx] }).ToList()
            );

            AssertCSVEqual(cols, test);
        }

        [TestMethod()]
        public void ContainsKeyTest()
        {
            var rawData = BuildTestCSVData();
            CSVData data = new CSVData(rawData.Item1, rawData.Item2);

            rawData.Item1.ForEach(key => Assert.IsTrue(data.ContainsKey(key)));
        }

        [TestMethod()]
        public void FilterInPlaceTest()
        {
            var rawData = BuildTestCSVData();
            CSVData data = new CSVData(rawData.Item1, rawData.Item2);

            int bIdx = rawData.Item1.IndexOf("B");
            int cIdx = rawData.Item1.IndexOf("C");

            data.FilterInPlace(row => row[bIdx] == "e" && row[cIdx] == "c");

            Assert.AreEqual(data.RowsCount, 1);

            data.GetRow(0).SequenceEqual(rawData.Item2.Last());
        }

        [TestMethod()]
        public void FilterInPlaceTest1()
        {
            var rawData = BuildTestCSVData();
            CSVData data = new CSVData(rawData.Item1, rawData.Item2);

            data.FilterInPlace(row => row["B"] == "e" && row["C"] == "c");

            Assert.AreEqual(data.RowsCount, 1);

            data.GetRow(0).SequenceEqual(rawData.Item2.Last());
        }

        [TestMethod()]
        public void ForEachRowInPlaceTest()
        {
            var rawData = BuildTestCSVData();
            CSVData data = new CSVData(rawData.Item1, rawData.Item2);

            Assert.AreNotEqual(data.GetColumnRaw("B").Distinct().ToList().Count, 1);

            int bIdx = data.ColIdxMap["B"];
            data.ForEachRowInPlace(row =>
                row[bIdx] = "test"
            );

            Assert.AreEqual(data.GetColumnRaw("B").Distinct().ToList().Count, 1);
            Assert.AreEqual(data.GetColumnRaw("B").Distinct().ToList()[0], "test");
        }

        [TestMethod()]
        public void MapRowsTest()
        {
            var rawData = BuildTestCSVData();
            CSVData data = new CSVData(rawData.Item1, rawData.Item2);

            int bIdx = rawData.Item1.IndexOf("B");
            int cIdx = rawData.Item1.IndexOf("C");

            List<string> ret = data.MapRows(row => row[bIdx] + row[cIdx]);
            Assert.AreEqual("bc,bc,cb,cb,cb,bc,ec", String.Join(",", ret));

            List<int> ret2 = data.MapRows(row => row.Count);

            Assert.AreEqual(ret2.Distinct().ToList().Count, 1);
            Assert.AreEqual(ret2.Distinct().ToList()[0], data.ColsCount);
        }

        [TestMethod()]
        public void MapColumnTest()
        {
            var rawData = BuildTestCSVData();
            CSVData data = new CSVData(rawData.Item1, rawData.Item2);

            List<Double> res = data.MapColumn("E", x => Double.Parse(x));

            Assert.AreEqual(17.5, res.Sum());
        }

        [TestMethod()]
        public void AddRowTest()
        {
            var rawData = BuildTestCSVData();
            CSVData data = new CSVData(rawData.Item1, rawData.Item2);

            var newRow = rawData.Item1.Select(x => "TestRow").ToList();

            data.AddRow(newRow);

            Assert.IsTrue(data.GetRow(data.RowsCount - 1).SequenceEqual(newRow));
        }

        [TestMethod()]
        public void AddRowsTest()
        {
            var rawData = BuildTestCSVData();
            CSVData data = new CSVData(rawData.Item1, rawData.Item2);

            var newRow1 = rawData.Item1.Select(x => "TestRow1").ToList();
            var newRow2 = rawData.Item1.Select(x => "TestRow2").ToList();

            var newRows = new List<List<string>>() { newRow1, newRow2 };

            data.AddRows(newRows);

            var test = data.GetRowsRange(data.RowsCount - 2, 2);

            Assert.IsTrue(test[0].SequenceEqual(newRow1));
            Assert.IsTrue(test[1].SequenceEqual(newRow2));
        }

        [TestMethod()]
        public void AddColumnTest()
        {
            var rawData = BuildTestCSVData();
            CSVData data = new CSVData(rawData.Item1, rawData.Item2);

            var newColEntries = Enumerable.Range(0, data.RowsCount).Select(x => "NewF").ToList();

            data.AddColumn("F", newColEntries);

            Assert.IsTrue(newColEntries.SequenceEqual(data.GetColumnRaw("F")));
        }

        [TestMethod()]
        public void AddColumnTest1()
        {
            var rawData = BuildTestCSVData();
            CSVData data = new CSVData(rawData.Item1, rawData.Item2);

            int bIdx = rawData.Item1.IndexOf("B");
            int cIdx = rawData.Item1.IndexOf("C");

            data.AddColumn("F", row => row[bIdx] + row[cIdx]);

            Assert.AreEqual(String.Join(",", data.GetColumnRaw("F")), "bc,bc,cb,cb,cb,bc,ec");
        }

        [TestMethod()]
        public void AddColumnsTest()
        {
            var rawData = BuildTestCSVData();
            CSVData data = new CSVData(rawData.Item1, rawData.Item2);

            int bIdx = rawData.Item1.IndexOf("B");
            int cIdx = rawData.Item1.IndexOf("C");
            data.AddColumns(new List<string>() { "F", "G" }, row => new List<string>() { row[bIdx] + row[cIdx], row[cIdx] + row[bIdx] });

            Assert.AreEqual(String.Join(",", data.GetColumnRaw("F")), "bc,bc,cb,cb,cb,bc,ec");
            Assert.AreEqual(String.Join(",", data.GetColumnRaw("G")), "cb,cb,bc,bc,bc,cb,ce");
        }

        [TestMethod()]
        public void RemoveColumnInPlaceTest()
        {
            var rawData = BuildTestCSVData();
            CSVData data = new CSVData(rawData.Item1, rawData.Item2);

            Assert.AreEqual(String.Join(",", data.GetRow(0)), "a,b,c,d,2.5");

            data.RemoveColumnInPlace("C");

            Assert.AreEqual(String.Join(",", data.GetRow(0)), "a,b,d,2.5");
            Assert.AreEqual(String.Join("", data.Cols), "ABDE");
        }

        [TestMethod()]
        public void RemoveColumnsTest()
        {
            var rawData = BuildTestCSVData();
            CSVData data = new CSVData(rawData.Item1, rawData.Item2);

            Assert.AreEqual(String.Join(",", data.GetRow(0)), "a,b,c,d,2.5");

            data.RemoveColumns(new List<string>() { "B", "D" });

            Assert.AreEqual(String.Join(",", data.GetRow(0)), "a,c,2.5");
            Assert.AreEqual(String.Join("", data.Cols), "ACE");
        }

        [TestMethod()]
        public void SetColumnTest()
        {
            var rawData = BuildTestCSVData();
            CSVData data = new CSVData(rawData.Item1, rawData.Item2);

            data.SetColumn("B", data.GetColumnRaw("E"));

            Assert.AreEqual(String.Join(",", data.GetColumnRaw("E")), String.Join(",", data.GetColumnRaw("B")));
        }

        [TestMethod()]
        public void SetColumnTest1()
        {
            var rawData = BuildTestCSVData();
            CSVData data = new CSVData(rawData.Item1, rawData.Item2);

            data.SetColumn("B", x => x + "T");

            Assert.AreEqual("bT,bT,cT,cT,cT,bT,eT", String.Join(",", data.GetColumnRaw("B")));
        }

        [TestMethod()]
        public void ReorderAllColumnsTest()
        {
            var rawData = BuildTestCSVData();
            CSVData data = new CSVData(rawData.Item1, rawData.Item2);

            Assert.AreEqual(String.Join(",", data.GetRow(0)), "a,b,c,d,2.5");

            List<string> new_columns = new List<string>() { "E", "C", "D", "A", "B" };
            data.ReorderAllColumns(new_columns);

            Assert.AreEqual(String.Join(",", data.GetRow(0)), "2.5,c,d,a,b");
        }

        [TestMethod()]
        public void ReorderColumnsTest()
        {
            var rawData = BuildTestCSVData();
            CSVData data = new CSVData(rawData.Item1, rawData.Item2);

            Assert.AreEqual(String.Join(",", data.GetRow(0)), "a,b,c,d,2.5");

            List<string> new_columns = new List<string>() { "E", "C", "A" };
            data.ReorderColumns(new_columns);

            Assert.AreEqual(String.Join(",", data.GetRow(0)), "2.5,c,a");
        }

        [TestMethod()]
        public void SortTest()
        {
            var rawData = BuildTestCSVData();
            CSVData data = new CSVData(rawData.Item1, rawData.Item2);

            int aIdx = rawData.Item1.IndexOf("A");
            int dIdx = rawData.Item1.IndexOf("D");
            data.Sort(row => new Tuple<string, string>(row[dIdx], row[aIdx]));

            List<List<string>> rows = new List<List<string>>();
            rows.Add(new List<string>() { "a", "b", "c", "d", "2.5" });
            rows.Add(new List<string>() { "a", "b", "c", "e", "2.5" });
            rows.Add(new List<string>() { "a", "b", "c", "e", "2.5" });
            rows.Add(new List<string>() { "b", "e", "c", "e", "2.5" });
            rows.Add(new List<string>() { "a", "c", "b", "f", "2.5" });
            rows.Add(new List<string>() { "b", "c", "b", "f", "2.5" });
            rows.Add(new List<string>() { "b", "c", "b", "f", "2.5" });

            CSVData test = new CSVData(rawData.Item1, rows);

            AssertCSVEqual(test, data);
        }

        [TestMethod()]
        public void SortDescendingTest()
        {
            var rawData = BuildTestCSVData();
            CSVData data = new CSVData(rawData.Item1, rawData.Item2);

            int aIdx = rawData.Item1.IndexOf("A");
            int dIdx = rawData.Item1.IndexOf("D");
            data.SortDescending(row => new Tuple<string, string>(row[dIdx], row[aIdx]));

            List<List<string>> rows = new List<List<string>>();
            rows.Add(new List<string>() { "a", "b", "c", "d", "2.5" });
            rows.Add(new List<string>() { "a", "b", "c", "e", "2.5" });
            rows.Add(new List<string>() { "a", "b", "c", "e", "2.5" });
            rows.Add(new List<string>() { "b", "e", "c", "e", "2.5" });
            rows.Add(new List<string>() { "a", "c", "b", "f", "2.5" });
            rows.Add(new List<string>() { "b", "c", "b", "f", "2.5" });
            rows.Add(new List<string>() { "b", "c", "b", "f", "2.5" });

            rows.Reverse();

            CSVData test = new CSVData(rawData.Item1, rows);

            AssertCSVEqual(test, data);
        }

        [TestMethod()]
        public void DropDuplicatesTest()
        {
            var rawData = BuildTestCSVData();
            CSVData data = new CSVData(rawData.Item1, rawData.Item2);

            data.DropDuplicates();

            Assert.AreEqual(data.RowsCount, rawData.Item2.Count - 2);

            List<List<string>> rows = new List<List<string>>();
            rows.Add(new List<string>() { "a", "b", "c", "d", "2.5" });
            rows.Add(new List<string>() { "a", "b", "c", "e", "2.5" });
            rows.Add(new List<string>() { "a", "c", "b", "f", "2.5" });
            rows.Add(new List<string>() { "b", "c", "b", "f", "2.5" });
            rows.Add(new List<string>() { "b", "e", "c", "e", "2.5" });

            CSVData test = new CSVData(rawData.Item1, rows);

            AssertCSVEqual(test, data);
        }

        [TestMethod()]
        public void DropDuplicatesTest1()
        {
            var rawData = BuildTestCSVData();
            CSVData data = new CSVData(rawData.Item1, rawData.Item2);

            data.DropDuplicates(new List<string>() { "C", "D" });

            Assert.AreEqual(data.RowsCount, 3);

            List<List<string>> rows = new List<List<string>>();
            rows.Add(new List<string>() { "a", "b", "c", "d", "2.5" });
            rows.Add(new List<string>() { "a", "b", "c", "e", "2.5" });
            rows.Add(new List<string>() { "a", "c", "b", "f", "2.5" });

            CSVData test = new CSVData(rawData.Item1, rows);

            AssertCSVEqual(test, data);
        }

        [TestMethod()]
        public void GroupByTest()
        {
            var rawData = BuildTestCSVData();
            CSVData data = new CSVData(rawData.Item1, rawData.Item2);

            Dictionary<string, Func<List<string>, string>> mapper = new Dictionary<string, Func<List<string>, string>>();

            mapper["E"] = xs => xs.Select(x => Double.Parse(x)).Sum().ToString();

            data.GroupBy(new List<string>() { "D", "A" }, mapper);

            Assert.AreEqual("D|A|E#d|a|2.5#e|a|5#f|a|2.5#f|b|5#e|b|2.5", data.ToEscapedDelimitedString("|", "{", "}", "#"));
        }

        [TestMethod()]
        public void GroupByTest1()
        {
            var rawData = BuildTestCSVData();
            CSVData data = new CSVData(rawData.Item1, rawData.Item2);

            Dictionary<string, Func<List<string>, string>> mapper = new Dictionary<string, Func<List<string>, string>>();

            mapper["E"] = xs => xs.Select(x => Double.Parse(x)).Sum().ToString();
            mapper["C"] = xs => String.Join("$", xs.Distinct().Select(x => xs.Count(y => y == x).ToString() + x));

            data.GroupBy(new List<string>() { "A" }, mapper);

            Assert.AreEqual("A|E|C#a|10|3c$1b#b|7.5|2b$1c", data.ToEscapedDelimitedString("|", "{", "}", "#"));
        }

        [TestMethod()]
        public void HConcatTest()
        {
            var rawData = BuildTestCSVData();
            CSVData data = new CSVData(rawData.Item1, rawData.Item2);
            CSVData data2 = new CSVData(new List<string>() { "P", "Q", "R", "S", "T" }, rawData.Item2);

            data2.RemoveColumnInPlace("R");

            data.HConcat(data2);

            Assert.AreEqual(String.Join("", data.Cols), "ABCDEPQST");
            Assert.AreEqual(String.Join("", data.GetRow(0)), "abcd2.5abd2.5");
        }

        [TestMethod()]
        public void VConcatTest()
        {
            var rawData = BuildTestCSVData();
            CSVData data = new CSVData(rawData.Item1, rawData.Item2);
            CSVData data2 = new CSVData(rawData.Item1, rawData.Item2.GetRange(2, 3));

            data.VConcat(data2);

            Assert.AreEqual(rawData.Item2.Count + 3, data.RowsCount);
            Assert.AreEqual(String.Join("", data.GetColumnRaw("D")), "defffeefff");
        }

        [TestMethod()]
        public void ToEscapedDelimitedStringTest()
        {
            var rawData = BuildTestCSVData();
            CSVData data = new CSVData(rawData.Item1, rawData.Item2);
            data.SetElement(2, "B", "X|Y");
            string expected = "A|B|C|D|E#a|b|c|d|2.5#a|b|c|e|2.5#a|{X|Y}|b|f|2.5#b|c|b|f|2.5#b|c|b|f|2.5#a|b|c|e|2.5#b|e|c|e|2.5";
            Assert.AreEqual(expected, data.ToEscapedDelimitedString("|", "{", "}", "#"));
        }

        [TestMethod()]
        public void ToCSVStringTest()
        {
            var rawData = BuildTestCSVData();
            CSVData data = new CSVData(rawData.Item1, rawData.Item2);
            data.SetElement(2, "B", "X,Y");
            string expected = "A,B,C,D,E\na,b,c,d,2.5\na,b,c,e,2.5\na,\"X,Y\",b,f,2.5\nb,c,b,f,2.5\nb,c,b,f,2.5\na,b,c,e,2.5\nb,e,c,e,2.5";
            Assert.AreEqual(expected, data.ToCSVString());
        }
    }
}