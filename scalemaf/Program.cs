/*
    MAF Scaling utility
    Copyright (C) 2014 Cory Nelson

    This program is free software: you can redistribute it and/or modify
    it under the terms of version 3 of the GNU General Public License as
    published by the Free Software Foundation.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with this program.  If not, see <http://www.gnu.org/licenses/>.
*/

using IdComLog.Data;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace scalemaf
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                Console.Error.WriteLine("Usage: scalemaf <file1> [<file2>, <file3>...]");
                return;
            }

            MafScaler scaler = new MafScaler(MafScaler.StockBins);

            foreach (var path in args)
            {
                UpdateResult res = scaler.ApplyLog(path);

                Console.WriteLine("Read {0:N0} records from '{1}'. Keeping {2:N0} CL and {3:N0} OL records with an IAT between {4:F1} and {5:F1}, averaging {6:F1}.",
                    res.LoadedRecordCount,
                    Path.GetFileNameWithoutExtension(path),
                    res.ClosedLoopCount,
                    res.OpenLoopCount,
                    res.IatMin,
                    res.IatMax,
                    res.IatAvg);
            }

            Console.WriteLine();

            foreach (var a in scaler.AdjustedBins)
            {
                string vol = a.Volume.ToString("G6");

                if (vol.IndexOf('.') == -1)
                {
                    vol += ".";
                }

                vol = vol.PadRight(7, '0');

                Console.WriteLine("{0} ({1:F2} samples over {2:F2} seconds)", vol, a.SampleCount, a.SampleSeconds);
            }
        }
    }
}
