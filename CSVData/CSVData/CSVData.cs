using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CSVDataNS
{
    using ColsType = List<string>;

    using RowType = List<string>;

    using RowsType = List<List<string>>;

    public class ListEqualityComparer<T> : IEqualityComparer<List<T>>
    {
        private List<int> indicesToCompare;

        public ListEqualityComparer(List<int> idxs)
        {
            indicesToCompare = idxs;
        }

        public bool Equals(List<T> xs, List<T> ys)
        {
            List<T> x = new List<T>();
            List<T> y = new List<T>();
            indicesToCompare.ForEach(idx =>
            {
                x.Add(xs[idx]);
                y.Add(ys[idx]);
            });

            return Object.ReferenceEquals(x, y) || (x != null && y != null && x.SequenceEqual(y));
        }

        public int GetHashCode(List<T> xs)
        {
            // Will not throw an OverflowException
            unchecked
            {
                List<T> obj = new List<T>();
                indicesToCompare.ForEach(idx =>
                {
                    obj.Add(xs[idx]);
                });
                return obj.Where(e => e != null).Select(e => e.GetHashCode()).Aggregate(17, (a, b) => 23 * a + b);
            }
        }
    }

    public class CSVData
    {
        private ColsType _columns;
        private RowsType _rows;
        private Dictionary<string, int> _columnsIdxMap;

        public CSVData(ColsType columns, RowsType rows)
        {
            SetColumnsAndRows(columns, new RowsType());
            ConstructRows(rows);
        }

        private void SetColumnsAndRows(ColsType columns, RowsType rows)
        {
            if (columns.Count != columns.Distinct().Count())
            {
                string message = String.Format("CSVData CSVData: duplicate column names found in input ({0})", String.Join(",", columns));
                throw new ArgumentOutOfRangeException(nameof(columns), message);
            }
            _columns = columns.Select(x => x).ToList();
            _rows = rows.Select(x => x).ToList();
            _columnsIdxMap = new Dictionary<string, int>();
            ConstructColIdxMap();
        }

        public CSVData(CSVData other)
        {
            SetColumnsAndRows(
                other.Cols.Select(x => x).ToList(),
                other._rows.Select(x => x.Select(y => y).ToList()).ToList()
            );
        }

        private void ConstructRows(RowsType rows)
        {
            AddRows(rows);
        }

        private void ConstructColIdxMap()
        {
            _columnsIdxMap.Clear();
            for (var i = 0; i < ColsCount; ++i)
            {
                _columnsIdxMap[Cols[i]] = i;
            }
        }

        public int ColsCount
        {
            get
            {
                return _columns.Count;
            }
        }

        public int RowsCount
        {
            get
            {
                return _rows.Count;
            }
        }

        public ColsType Cols
        {
            get
            {
                return _columns;
            }
        }

        public RowsType GetRowsRangeRaw(int startIdx, int count)
        {
            return _rows.GetRange(startIdx, count);
        }

        public RowType GetRow(int idx)
        {
            return _rows[idx];
        }

        public List<Dictionary<string, string>> ToListOfDicts()
        {
            return _rows.Select(row => ToDict(Cols, row)).ToList();
        }

        public string GetElement(int rowIdx, string columnName)
        {
            return GetRow(rowIdx)[ColIdxMap[columnName]];
        }

        public void SetElement(int rowIdx, string columnName, string value)
        {
            GetRow(rowIdx)[ColIdxMap[columnName]] = value;
        }

        public List<string> GetColumnRaw(string key)
        {
            int idx = ColIdxMap[key];
            return _rows.Select(x => x[idx]).ToList();
        }

        public CSVData GetColumn(string key)
        {
            CSVData ret = new CSVData(new ColsType() { key }, GetColumnRaw(key).Select(r => new RowType() { r }).ToList());
            return ret;
        }

        public void RenameColumn(string keyOld, string keyNew)
        {
            if (keyOld != keyNew)
            {
                if (Cols.Contains(keyNew))
                {
                    string message = String.Format("RenameColumn: new name of key ({0}) already exists in columns ({1})", keyNew, String.Join(",", Cols));
                    throw new ArgumentOutOfRangeException(nameof(keyNew), message);
                }
                int idx = ColIdxMap[keyOld];
                _columns[idx] = keyNew;
                ConstructColIdxMap();
            }
        }

        public CSVData GetColumns(List<string> keys)
        {
            var columns = keys.ToDictionary(
                k => k,
                k => GetColumnRaw(k)
            );

            CSVData ret = null;

            keys.ForEach(key =>
            {
                if (ret == null)
                {
                    ret = new CSVData(new ColsType() { key }, columns[key].Select(r => new RowType() { r }).ToList());
                }
                else
                {
                    ret.AddColumn(
                        key,
                        columns[key]
                    );
                }
            });

            return ret;
        }

        public Dictionary<string, int> ColIdxMap
        {
            get
            {
                return _columnsIdxMap;
            }
        }

        public bool ContainsKey(string key)
        {
            return ColIdxMap.ContainsKey(key);
        }

        public void FilterInPlace(Func<RowType, bool> predicate)
        {
            _rows = _rows.Where(predicate).ToList();
        }

        private static Dictionary<string, string> ToDict(ColsType keys, RowType values)
        {
            return keys.Zip(values, (k, v) => new { k, v }).ToDictionary(x => x.k, x => x.v);
        }

        public void FilterInPlace(Func<Dictionary<string, string>, bool> predicate)
        {
            _rows = _rows.Where(x => predicate(ToDict(Cols, x))).ToList();
        }

        public void ForEachRowInPlace(Action<RowType> action)
        {
            _rows.ForEach(action);
        }

        public List<T> MapRows<T>(Func<RowType, T> func)
        {
            return _rows.Select(func).ToList();
        }

        public List<T> MapColumn<T>(string key, Func<string, T> func)
        {
            return GetColumnRaw(key).Select(func).ToList();
        }

        public void AddRow(RowType row)
        {
            if (row.Count != ColsCount)
            {
                string message = String.Format("AddRow: row to be added count ({0}) does not match the number of columns ({1}) in the CSVData", row.Count, ColsCount);
                throw new ArgumentOutOfRangeException(nameof(row), message);
            }
            else
            {
                _rows.Add(row.Select(x => x).ToList());
            }
        }

        public void AddRows(RowsType rows)
        {
            _rows.Capacity = RowsCount + rows.Count;
            rows.ForEach(x => AddRow(x));
        }

        public void AddColumn(string key, Func<RowType, string> func)
        {
            var toAdd = MapRows(func);
            AddColumn(key, toAdd);
        }

        public void AddColumn(string key, List<string> rows)
        {
            if (ColIdxMap.ContainsKey(key))
            {
                string message = String.Format("AddColumn: key({0}) is already present in the CSVData", key);
                throw new ArgumentOutOfRangeException(nameof(key), message);
            }
            else if (rows.Count != RowsCount)
            {
                string message = String.Format("AddColumn: Dimension mismatch, current({0}), to be added ({1})", RowsCount, rows.Count);
                throw new ArgumentOutOfRangeException(nameof(rows), message);
            }
            else
            {
                _columns.Add(key);
                for (int i = 0; i < RowsCount; ++i)
                {
                    _rows[i].Add(rows[i]);
                }
                ConstructColIdxMap();
            }
        }

        public void AddColumns(List<string> keys, Func<RowType, List<string>> func)
        {
            CSVData toAdd = new CSVData(keys, MapRows(func));
            HConcat(toAdd);
        }

        public List<string> RemoveColumnInPlace(string key)
        {
            if (ColIdxMap.ContainsKey(key))
            {
                List<string> removedColumn = GetColumnRaw(key);
                int keyIdx = ColIdxMap[key];
                _columns.RemoveAt(keyIdx);
                _rows.ForEach(row => row.RemoveAt(keyIdx));
                ConstructColIdxMap();
                return removedColumn;
            }
            else
            {
                string message = String.Format("RemoveColumn: key ({0}) not present in CSVData", key);
                throw new ArgumentOutOfRangeException(nameof(key), message);
            }
        }

        public void RemoveColumns(List<string> keys)
        {
            keys.ForEach(key => RemoveColumnInPlace(key));
        }

        public void SetColumn(string key, List<string> rows)
        {
            if (!ColIdxMap.ContainsKey(key))
            {
                string message = String.Format("SetColumn: key({0}) is not present in the CSVData", key);
                throw new ArgumentOutOfRangeException(nameof(key), message);
            }
            else if (rows.Count != RowsCount)
            {
                string message = String.Format("SetColumn: Dimension mismatch, current({0}), to be added ({1})", RowsCount, rows.Count);
                throw new ArgumentOutOfRangeException(nameof(rows), message);
            }
            else
            {
                int idx = ColIdxMap[key];
                for (int i = 0; i < RowsCount; ++i)
                {
                    _rows[i][idx] = rows[i];
                }
            }
        }

        public void ReorderAllColumns(List<string> keys)
        {
            if (!Cols.All(keys.Contains))
            {
                string message = String.Format("ReorderAllColumns: keys({0}) to reorder are not the same as CSVData ({1})", String.Join(",", keys), String.Join(",", Cols));
                throw new ArgumentOutOfRangeException(nameof(keys), message);
            }
            else
            {
                ReorderColumns(keys);
            }
        }

        public void ReorderColumns(List<string> keys)
        {
            if (!keys.All(Cols.Contains))
            {
                string message = String.Format("ReorderColumns: keys({0}) to reorder are not a subset of CSVData ({1})", String.Join(",", keys), String.Join(",", Cols));
                throw new ArgumentOutOfRangeException(nameof(keys), message);
            }
            else if (keys.Count != keys.Distinct().Count())
            {
                string message = String.Format("ReorderColumns: keys({0}) cannot contain duplicates", String.Join(",", keys));
                throw new ArgumentOutOfRangeException(nameof(keys), message);
            }
            else
            {
                List<int> oldIdxs = keys.Select(x => ColIdxMap[x]).ToList();
                _columns = keys;
                _rows = _rows.Select(row =>
                {
                    var newRow = new List<string>();
                    oldIdxs.ForEach(idx =>
                    {
                        newRow.Add(row[idx]);
                    });
                    return newRow;
                }).ToList();
            }
        }

        public void Sort<T>(Func<RowType, T> comp)
        {
            _rows = _rows.OrderBy(comp).ToList();
        }

        public void SortDescending<T>(Func<RowType, T> comp)
        {
            _rows = _rows.OrderByDescending(comp).ToList();
        }

        public void DropDuplicates(List<string> keys)
        {
            List<int> idxsToCompare = keys.Select(key => ColIdxMap[key]).ToList();
            _rows = _rows.Distinct(new ListEqualityComparer<string>(idxsToCompare)).ToList();
        }

        public void DropDuplicates()
        {
            DropDuplicates(Cols);
        }

        public void GroupBy(List<string> keys, Dictionary<string, Func<List<string>, string>> mergeKeysAndFuncs)
        {
            if (keys.Any(mergeKeysAndFuncs.Keys.Contains))
            {
                string message = String.Format("GroupBy: keys({0}) can not overlap with the mergeKeys({1})", String.Join(",", keys), String.Join(",", mergeKeysAndFuncs.Keys));
                throw new ArgumentOutOfRangeException(nameof(keys), message);
            }
            List<int> keyIdxs = keys.Select(key => ColIdxMap[key]).ToList();
            string dummyColName = String.Format("##{0}", String.Join("##", keys));
            AddColumn(dummyColName, row =>
            {
                var subrow = new List<string>();
                keyIdxs.ForEach(idx => subrow.Add(row[idx]));
                return String.Join("##", subrow);
            });

            List<string> entries = GetColumnRaw(dummyColName).Distinct().ToList();

            Dictionary<string, CSVData> entriesToData = new Dictionary<string, CSVData>();
            int dummyEntryIdx = ColIdxMap[dummyColName];

            entries.ForEach(entry =>
            {
                CSVData data = new CSVData(_columns, _rows);
                data.FilterInPlace(row => row[dummyEntryIdx] == entry);
                entriesToData[entry] = data;
            });

            List<List<string>> newRows = entriesToData.ToList().Select(kvp1 =>
            {
                Dictionary<string, string> newColValues = mergeKeysAndFuncs.ToDictionary(
                    kvp2 => kvp2.Key,
                    kvp2 => kvp2.Value(kvp1.Value.GetColumnRaw(kvp2.Key))
                );
                List<string> ret = new List<string>();
                keyIdxs.ForEach(idx => ret.Add(kvp1.Value._rows[0][idx]));
                newColValues.ToList().ForEach(kvp => ret.Add(kvp.Value));
                return ret;
            }).ToList();

            List<string> newCols = keys;
            newCols.AddRange(mergeKeysAndFuncs.ToList().Select(kvp => kvp.Key));

            SetColumnsAndRows(newCols, newRows);
        }

        public void SetColumn(string key, Func<string, string> func)
        {
            if (!ColIdxMap.ContainsKey(key))
            {
                string message = String.Format("SetColumn: key({0}) is not present in the CSVData", key);
                throw new ArgumentOutOfRangeException(nameof(key), message);
            }
            else
            {
                int idx = ColIdxMap[key];
                for (int i = 0; i < RowsCount; ++i)
                {
                    _rows[i][idx] = func(_rows[i][idx]);
                }
            }
        }

        public void HConcat(CSVData other)
        {
            other._columns.ForEach(key => AddColumn(key, other.GetColumnRaw(key)));
        }

        public void VConcat(CSVData other)
        {
            if (Object.ReferenceEquals(other, this))
            {
                other = new CSVData(other);
            }
            if (Cols.SequenceEqual(other.Cols))
            {
                AddRows(other._rows);
            }
            else
            {
                string message = String.Format("VConcat: cannot be done with CSV data with different columns");
                throw new ArgumentOutOfRangeException(nameof(other), message);
            }
        }

        public string ToEscapedDelimitedString(string delimiter, string escapePrefix, string escapeSuffix, string lineSeperator)
        {
            StringBuilder builder = new StringBuilder();

            Func<string, string> escape = name => name.Contains(delimiter) ? String.Format("{0}{1}{2}", escapePrefix, name, escapeSuffix) : name;

            builder.Append(String.Join(delimiter, Cols.Select(escape)));
            builder.Append(lineSeperator);
            builder.Append(String.Join(lineSeperator, _rows.Select(row => String.Join(delimiter, row.Select(escape)))));

            return builder.ToString();
        }

        public string ToCSVString()
        {
            return ToEscapedDelimitedString(",", "\"", "\"", "\n");
        }

        public CSVData Filter(Func<RowType, bool> predicate)
        {
            return new CSVData(this.Cols, _rows.Where(predicate).ToList());
        }

        public CSVData Filter(Func<Dictionary<string, string>, bool> predicate)
        {
            return new CSVData(this.Cols, _rows.Where(x => predicate(ToDict(Cols, x))).ToList());
        }

        public List<T> MapRows<T>(Func<Dictionary<string, string>, T> func)
        {
            return _rows.Select(row => func(ToDict(this.Cols, row))).ToList();
        }

        public CSVData MapRows(List<string> keys, Func<RowType, List<string>> func)
        {
            return new CSVData(keys, this.MapRows(func));
        }

        public CSVData MapRows(List<string> keys, Func<Dictionary<string, string>, List<string>> func)
        {
            return new CSVData(keys, this.MapRows(func));
        }

        public void Sort<T>(Func<Dictionary<string, string>, T> comp)
        {
            _rows = _rows.OrderBy(row => comp(ToDict(Cols, row))).ToList();
        }

        public void SortDescending<T>(Func<Dictionary<string, string>, T> comp)
        {
            _rows = _rows.OrderByDescending(row => comp(ToDict(Cols, row))).ToList();
        }

        public CSVData GetRowsRange(int startIdx, int count)
        {
            return new CSVData(this.Cols, this.GetRowsRangeRaw(startIdx, count));
        }
    }

    public static class CSVDataFunctional
    {
        public static CSVData Filter(CSVData data, Func<RowType, bool> predicate)
        {
            CSVData ret = new CSVData(data);
            ret.FilterInPlace(predicate);
            return ret;
        }

        public static CSVData Filter(CSVData data, Func<Dictionary<string, string>, bool> predicate)
        {
            CSVData ret = new CSVData(data);
            ret.FilterInPlace(predicate);
            return ret;
        }

        public static CSVData ForEachRow(CSVData data, Action<RowType> action)
        {
            CSVData ret = new CSVData(data);
            ret.ForEachRowInPlace(action);
            return ret;
        }

        public static CSVData AddRow(CSVData data, RowType row)
        {
            CSVData ret = new CSVData(data);
            ret.AddRow(row);
            return ret;
        }

        public static CSVData AddRows(CSVData data, RowsType rows)
        {
            CSVData ret = new CSVData(data);
            ret.AddRows(rows);
            return ret;
        }

        public static CSVData AddColumn(CSVData data, string key, Func<RowType, string> func)
        {
            CSVData ret = new CSVData(data);
            ret.AddColumn(key, func);
            return ret;
        }

        public static CSVData AddColumn(CSVData data, string key, List<string> rows)
        {
            CSVData ret = new CSVData(data);
            ret.AddColumn(key, rows);
            return ret;
        }

        public static CSVData AddColumns(CSVData data, List<string> keys, Func<RowType, List<string>> func)
        {
            CSVData ret = new CSVData(data);
            ret.AddColumns(keys, func);
            return ret;
        }

        public static CSVData RemoveColumn(CSVData data, string key)
        {
            CSVData ret = new CSVData(data);
            ret.RemoveColumnInPlace(key);
            return ret;
        }

        public static CSVData RemoveColumns(CSVData data, List<string> keys)
        {
            CSVData ret = new CSVData(data);
            ret.RemoveColumns(keys);
            return ret;
        }

        public static CSVData HConcat(CSVData data, CSVData other)
        {
            CSVData ret = new CSVData(data);
            ret.HConcat(other);
            return ret;
        }

        public static CSVData VConcat(CSVData data, CSVData other)
        {
            CSVData ret = new CSVData(data);
            ret.VConcat(other);
            return ret;
        }

        public static CSVData SetColumn(CSVData data, string key, List<string> rows)
        {
            CSVData ret = new CSVData(data);
            ret.SetColumn(key, rows);
            return ret;
        }

        public static CSVData ReorderAllColumns(CSVData data, List<string> keys)
        {
            CSVData ret = new CSVData(data);
            ret.ReorderAllColumns(keys);
            return ret;
        }

        public static CSVData ReorderColumns(CSVData data, List<string> keys)
        {
            CSVData ret = new CSVData(data);
            ret.ReorderColumns(keys);
            return ret;
        }

        public static CSVData Sort<T>(CSVData data, Func<RowType, T> comp)
        {
            CSVData ret = new CSVData(data);
            ret.Sort(comp);
            return ret;
        }

        public static CSVData SortDescending<T>(CSVData data, Func<RowType, T> comp)
        {
            CSVData ret = new CSVData(data);
            ret.SortDescending(comp);
            return ret;
        }

        public static CSVData DropDuplicates(CSVData data, List<string> keys)
        {
            CSVData ret = new CSVData(data);
            ret.DropDuplicates(keys);
            return ret;
        }

        public static CSVData DropDuplicates(CSVData data)
        {
            CSVData ret = new CSVData(data);
            ret.DropDuplicates();
            return ret;
        }

        public static CSVData GroupBy(CSVData data, List<string> keys, Dictionary<string, Func<List<string>, string>> mergeKeysAndFuncs)
        {
            CSVData ret = new CSVData(data);
            ret.GroupBy(keys, mergeKeysAndFuncs);
            return ret;
        }
    }
}