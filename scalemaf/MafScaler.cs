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
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace scalemaf
{
    sealed class MafScaler
    {
        static readonly double[] frsVolts = new[] { 0.8984375, 0.9375, 0.9765625, 1.015625, 1.0546875, 1.09375, 1.1328125, 1.171875, 1.2109375, 1.25, 1.2890625, 1.328125, 1.3671875, 1.40625, 1.4453125, 1.484375, 1.5234375, 1.5625, 1.6015625, 1.640625, 1.6796875, 1.71875, 1.7578125, 1.796875, 1.8359375, 1.875, 1.9140625, 1.953125, 1.9921875, 2.03125, 2.0703125, 2.109375, 2.1484375, 2.1875, 2.2265625, 2.265625, 2.3046875, 2.34375, 2.3828125, 2.421875, 2.4609375, 2.5, 2.578125, 2.7734375, 2.96875, 3.203125, 3.4375, 3.7109375, 3.90625, 4.0625, 4.296875, 4.4921875, 4.7265625, 5 };
        static readonly double[] frsVolume = new[] { 0.85, 1, 1.16, 1.43, 1.59, 1.82, 2.02, 2.29, 2.57, 2.87, 3.12, 3.43, 3.79, 4.22, 4.62, 5.02, 5.48, 6.02, 6.57, 7.21, 7.76, 8.51, 9.26, 9.97, 10.73, 11.53, 12.62, 13.48, 14.31, 15.27, 16.28, 17.33, 18.41, 19.51, 20.66, 21.85, 23.09, 24.51, 26.09, 27.62, 29.09, 30.95, 34.56, 43.67, 53.83, 70.21, 87.74, 112.57, 134.75, 153.59, 183.68, 213.71, 254.12, 314.87 };
        static readonly double[] perrinSmallVolume = new[] { 0.850342095, 0.996084631, 1.157039523, 1.328069568, 1.51230371, 1.715627432, 1.940551281, 2.183871508, 2.443892956, 2.71843648, 3.009068012, 3.324135065, 3.678965569, 4.083361626, 4.497094631, 4.941109657, 5.422593594, 5.909182072, 6.465194225, 7.056956768, 7.66237402, 8.33317852, 9.030407906, 9.752721787, 10.50780869, 11.30331898, 12.1346674, 12.99447823, 13.88854027, 14.82530975, 15.80683517, 16.82523537, 17.86863327, 18.94257545, 20.05467606, 21.21268654, 22.41743088, 23.66580391, 24.95509529, 26.28763771, 27.66790962, 29.0982666, 32.12263489, 40.76900482, 50.85312271, 63.4164505, 77.59732819, 97.07341003, 115.995491, 131.9366608, 161.8807068, 187.7812805, 223.0933533, 269.73349 };
        static readonly double[] perrinLargeVolume = new[] { 1.04, 1.22, 1.41, 1.62, 1.85, 1.99, 2.13, 2.58, 2.94, 3.43, 3.92, 4.25, 4.73, 5.38, 5.98, 6.70, 7.53, 8.46, 8.93, 9.91, 10.26, 11.40, 12.30, 13.69, 14.38, 15.75, 16.89, 17.80, 19.08, 20.18, 21.67, 23.15, 24.33, 25.43, 26.75, 28.07, 29.55, 31.03, 31.45, 33.69, 35.07, 36.46, 39.84, 49.46, 60.21, 73.50, 90.64, 119.91, 140.62, 157.50, 189.56, 219.89, 261.25, 315.86 };

        // the original MAF bins we're scaling.
        readonly double[] origVolts, origVolume;

        // these are all accumulations of weighted values, used to calculate a weighted average at the end.
        readonly double[] adjustments; //  bin adjustments.
        readonly double[] adjustmentTimes; // seconds (in time) of adjustments that went into this bin.
        readonly double[] adjustmentWeights; // sample count that went into this bin.
        private readonly Table _targetAfrTable;

        /// <summary>
        /// Bins for stock FR-S.
        /// </summary>
        public static IEnumerable<MafBin> StockBins
        {
            get { return BuildBins(frsVolts, frsVolume); }
        }
        
        /// <summary>
        /// Bins for Perrin's 2.75" CAI.
        /// </summary>
        public static IEnumerable<MafBin> PerrinSmallBins
        {
            get { return BuildBins(frsVolts, perrinSmallVolume); }
        }

        /// <summary>
        /// Bins for Perrin's 3" CAI.
        /// </summary>
        public static IEnumerable<MafBin> PerrinLargeBins
        {
            get { return BuildBins(frsVolts, perrinLargeVolume); }
        }

        /// <summary>
        /// Merges two arrays into a series of bins.
        /// </summary>
        public static IEnumerable<MafBin> BuildBins(double[] volts, double[] volume)
        {
            Debug.Assert(volts != null);
            Debug.Assert(volume != null);
            Debug.Assert(volts.Length == volume.Length);

            for (int i = 0; i < volts.Length; ++i)
            {
                yield return new MafBin
                {
                    Volts = volts[i],
                    Volume = volume[i]
                };
            }
        }

        /// <summary>
        /// Bins after scaling.
        /// </summary>
        public IEnumerable<AdjustedMafBin> AdjustedBins
        {
            get
            {
                for (int i = 0; i < adjustments.Length; ++i)
                {
                    double weight, adj, volume, time;

                    if (adjustmentTimes[i] >= 20.0)
                    {
                        weight = adjustmentWeights[i];
                        adj = adjustments[i] / weight;
                        volume = origVolume[i] * (1.0 + adj);
                        time = adjustmentTimes[i];
                    }
                    else
                    {
                        // don't bother providing an adjustment with fewer than 100 samples (weighted).

                        weight = 0.0;
                        adj = 0.0;
                        volume = origVolume[i];
                        time = 0.0;
                    }

                    yield return new AdjustedMafBin
                    {
                        Volts = origVolts[i],
                        Volume = volume,
                        SampleCount = weight,
                        SampleSeconds = time
                    };
                }
            }
        }

        public MafScaler(IEnumerable<MafBin> bins, Table targetAfrTable)
        {
            if (bins == null) throw new ArgumentNullException("bins");

            _targetAfrTable = targetAfrTable;

            bins = bins.OrderBy(x => x.Volts).ToArray();

            if (!bins.Any()) throw new ArgumentException("There must be at least one bin to scale.");

            origVolts = bins.Select(x => x.Volts).ToArray();
            origVolume = bins.Select(x => x.Volume).ToArray();

            adjustments = new double[origVolts.Length];
            adjustmentTimes = new double[origVolts.Length];
            adjustmentWeights = new double[origVolts.Length];
        }

        /// <summary>
        /// Reads a log file and applys scaling.
        /// </summary>
        public UpdateResult ApplyLog(string filePath, bool closedLoop = true, bool openLoop = true)
        {
            List<OftRecord> srcRecords = OftRecord.FromFile(filePath);

            // determine a good cutoff point for IAT to avoid useless heat-soaked records.
            // an engine load of ~0.25 appears to be idle, where IATs will rise. grab all the temps for loads above
            // 0.3, find the top quartile, and use that as a cutoff for all records. ultimately this may be something
            // the user needs to eyeball, but automating it this way seems to work pretty well so far.

            double[] loadedTemps = srcRecords
                .Where(r => r.EngineLoad > 0.3)
                .Select(r => r.IntakeAirTemp)
                .OrderBy(iat => iat)
                .ToArray();

            int topQuartile = loadedTemps.Length * 3 / 4;

            double iatMin = loadedTemps[0];
            double iatMax = loadedTemps[topQuartile];
            double iatAvg = loadedTemps.Take(topQuartile).Average();

            // ensure records are sorted by time so we can get proper change rate.

            srcRecords.Sort((x, y) => x.Time.CompareTo(y.Time));

            // run adjustments.

            int openKept = 0, closedKept = 0;

            for (int i = 1; i < srcRecords.Count; ++i)
            {
                OftRecord prev = srcRecords[i - 1], cur = srcRecords[i];

                // the time (in seconds) from the previous record to this record.
                double time = cur.Time - prev.Time;

                // The MAFv change rate (in volts/sec) from the previous record to this record.
                // Records with too high of a change rate are thrown out as inaccurate.
                double mafChangeRate = Math.Abs((cur.MafVoltage - prev.MafVoltage) / time);
                if (mafChangeRate > 0.2) continue;

                // The volume adjustment to make.
                // If the record is missing data or columns, it will be null.
                double? adjustment = cur.GetVolumeAdjustment(_targetAfrTable);
                if (adjustment == null) continue;

                // Filter out heat-soaked data, as temp fluctuations will throw things off.
                if (cur.IntakeAirTemp > iatMax) continue;

                if (cur.ClosedLoop == 1)
                {
                    if (!closedLoop) continue;
                    ++closedKept;
                }
                else if (cur.ClosedLoop == 0)
                {
                    if (!openLoop) continue;
                    ++openKept;
                }
                else
                {
                    // unknown fuel system status.
                    continue;
                }

                Adjust(cur.MafVoltage, time, adjustment.Value);
            }

            return new UpdateResult
            {
                LoadedRecordCount = srcRecords.Count,
                ClosedLoopCount = closedKept,
                OpenLoopCount = openKept,
                IatMin = iatMin,
                IatMax = iatMax,
                IatAvg = iatAvg
            };
        }

        /// <summary>
        /// Applies the adjustment, blending between two bins depending on MAFv.
        /// </summary>
        void Adjust(double mafVoltage, double timeXY, double adj)
        {
            if (mafVoltage <= origVolts[0])
            {
                // put it all into the first bin.

                adjustments[0] += adj;
                adjustmentTimes[0] += timeXY;
                adjustmentWeights[0] += 1.0;
            }
            else if (mafVoltage >= origVolts[origVolts.Length - 1])
            {
                // put it all into the last bin.

                int idx = adjustments.Length - 1;

                adjustments[idx] += adj;
                adjustmentTimes[idx] += timeXY;
                adjustmentWeights[idx] += 1.0;
            }
            else
            {
                // blend the adjustment between two bins. binA is the lower voltage, binB is the higher.

                int binA, binB;

                binA = Array.BinarySearch(origVolts, mafVoltage);

                if (binA < 0)
                {
                    binB = ~binA;
                    binA = binB - 1;
                }
                else
                {
                    binB = binA + 1;
                }

                double voltsA = origVolts[binA];
                double dist = (mafVoltage - voltsA) / (origVolts[binB] - voltsA);
                double weightA = 1.0 - dist;
                double weightB = dist;

                adjustments[binA] += adj * weightA;
                adjustmentTimes[binA] += timeXY * weightA;
                adjustmentWeights[binA] += weightA;

                adjustments[binB] += adj * weightB;
                adjustmentTimes[binB] += timeXY * weightB;
                adjustmentWeights[binB] += weightB;
            }
        }
    }
}
