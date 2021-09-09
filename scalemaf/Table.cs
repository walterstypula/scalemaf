using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace scalemaf
{
    class Table
    {

        public List<double> ColumnReference { get; set; } = new List<double>();

        public List<double> RowReference { get; set; } = new List<double>();

        public List<List<double>> DataCells { get; set; } = new List<List<double>>();

        public double GetData(double xValue, double yValue)
        {
            var xHeaders = ColumnReference;
            var yHeaders = RowReference;
            var values = DataCells;

            //Find Index of Target Columns
            var x1 = FindIndex(xValue, xHeaders);
            var y1 = FindIndex(yValue, yHeaders);

            var x2 = x1 + 1;
            var y2 = y1 + 1;

            x2 = xHeaders.Count > x2 ? x2 : x1;
            y2 = yHeaders.Count > y2 ? y2 : y1;

            var q11 = values[y1][x1];
            var q21 = values[y1][x2];
            var q12 = values[y2][x1];
            var q22 = values[y2][x2];

            var x1Header = xHeaders[x1];
            var x2Header = xHeaders[x2];
            var y1Header = yHeaders[y1];
            var y2Header = yHeaders[y2];


            if (x2Header != x1Header && y2Header != y1Header)
            {
                var r1 = ((x2Header - xValue) / (x2Header - x1Header)) * q11 + ((xValue - x1Header) / (x2Header - x1Header)) * q21;
                var r2 = ((x2Header - xValue) / (x2Header - x1Header)) * q12 + ((xValue - x1Header) / (x2Header - x1Header)) * q22;

                var p = ((y2Header - yValue) / (y2Header - y1Header)) * r1 + ((yValue - y1Header) / (y2Header - y1Header)) * r2;

                return p;
            }
            else if (y1Header != y2Header && q12 != q11)
            {
                var p = y1Header + (y2Header - y1Header) / (q12 - q11) * (yValue - q11);
                return p;
            }
            else if (x1Header != x2Header && q21 != q11)
            {
                var p = x1Header + (x2Header - x1Header) / (q21 - q11) * (xValue - q11);
                return p;
            }
            else
            {
                return q11;
            }
        }

        private static int FindIndex(double value, List<double> headers)
        {
            var index = 0;
            for (int i = 0; i < headers.Count; i++)
            {
                if (value >= headers[i])
                {
                    index = i;
                    continue;
                }
                break;
            }

            return index;
        }

        public static Table LoadTableData(string path)
        {
            var table = new Table();

            using (var file = new FileStream(path, FileMode.Open))
            {
                var reader = new StreamReader(file);
                reader.ReadLine();
                var line = reader.ReadLine();

                table.ColumnReference = GetValues(line);

                var tableData = new List<List<double>>();
                while (true)
                {
                    line = reader.ReadLine();
                    if (string.IsNullOrEmpty(line))
                    {
                        break;
                    }

                    var columnValues = GetValues(line);

                    double rowInfoRef = columnValues[0];
                    int skip = 1;
                    
                    if(columnValues.Count == table.ColumnReference.Count)
                    {
                        skip = 0;
                        rowInfoRef = 0;
                    }
                
                    table.RowReference.Add(rowInfoRef);

                    var rowData = columnValues.Skip(skip).ToList();
                    table.DataCells.Add(rowData);
                }
            }

            return table;
        }

        private static List<double> GetValues(string line)
        {
            var splitArray = line.Split('\t');
            return GetValues(splitArray);
        }

        private static List<double> GetValues(string[] valueStrings)
        {
            var result = new double[valueStrings.Length];

            for (var i = 0; i < result.Length; i++)
            {
                result[i] = double.Parse(valueStrings[i]);
            }

            return result.ToList();
        }
    }
}
