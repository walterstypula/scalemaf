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
        [Column("Time[s]"), Required]
        public double Time { get; set; }

        [Column("Engine load"), Required]
        public double EngineLoad { get; set; }

        [Column("MAF Voltage"), Required]
        public double MafVoltage { get; set; }

        [Column("AFR")]
        public double? Afr { get; set; }

        [Column("Commanded AFR")]
        public double? CommandedAfr { get; set; }

        [Column("ST Fuel Trim")]
        public double? StFuelTrim { get; set; }

        [Column("LT Fuel Trim"), Required]
        public double LtFuelTrim { get; set; }

        [Column("Fuel Sys Status"), Required]
        public int FuelSysStatus { get; set; }

        [Column("Intake Air T."), Required]
        public double IntakeAirTemp { get; set; }

        /// <summary>
        /// The volume adjustment needed to make this MAFv accurate.
        /// </summary>
        [NotMapped]
        public double? VolumeAdjustment
        {
            get
            {
                return FuelSysStatus == 2 ? (StFuelTrim + LtFuelTrim) / 100.0 :
                       FuelSysStatus == 4 ? CommandedAfr / (Afr * (LtFuelTrim + 100.0)) * 100.0 :
                       (double?)null;
            }
        }

        public static List<OftRecord> FromFile(string filePath)
        {
            using (var reader = new StreamReader(filePath))
            {
                string line = reader.ReadLine();

                if (!string.Equals(line, "Procede Data Log", StringComparison.OrdinalIgnoreCase))
                {
                    throw new Exception("Expected an OFT log.");
                }

                // I guess "OpenFlash Data File 1" is a version number.

                line = reader.ReadLine();

                if (!string.Equals(line, "OpenFlash Data File 1", StringComparison.OrdinalIgnoreCase))
                {
                    throw new Exception("Expected an OFTv1 log.");
                }

                // this appears to be the number of columns in the file, or more specifically the 0-based index of the final column.
                // i.e. for 5 columns, this will be "6".
                reader.ReadLine();

                // finally read in objects.
                return Formats.Csv.ReadObjects<OftRecord>(reader, validate: true).ToList();
            }
        }
    }
}