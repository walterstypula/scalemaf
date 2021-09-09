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
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace scalemaf
{
    sealed class OftRecord
    {
        [Column("Time (msec)"), Required]
        public double Time { get; set; }

        [Column("RPM"), Required]
        public double EngineSpeed { get; set; }

        [Column("Load (g/rev)"), Required]
        public double EngineLoad { get; set; }

        [Column("MAF (V)"), Required]
        public double MafVoltage { get; set; }

        [Column("*AFR")]
        public double? CurrentAfr { get; set; }

        [Column("CommandedAfr")]
        public double? CommandedAfr { get; set; }

        [Column("Fuel correct (%)")]
        public double? StFuelTrim { get; set; }

        [Column("Fuel learn (%)"), Required]
        public double LtFuelTrim { get; set; }

        [Column("Closed loop"), Required]
        public int ClosedLoop { get; set; }

        [Column("Intake (degF)")]
        public double IntakeAirTemp { get; set; }

        public double? GetVolumeAdjustment(Table targetAfrTable)
        {
            if (ClosedLoop == 1)
            {
                return (StFuelTrim + LtFuelTrim) / 100.0;
            }
            else if (ClosedLoop == 0)
            {
                var afr = CommandedAfr ?? targetAfrTable.GetData(EngineLoad, EngineSpeed);
                return (CurrentAfr - afr) / afr;
            }

            return null;
        }

        public static List<OftRecord> FromFile(string filePath)
        {
            using (var reader = new StreamReader(filePath))
            {
                // finally read in objects.
                return Formats.Csv.ReadObjects<OftRecord>(reader, validate: true).ToList();
            }
        }
    }
}