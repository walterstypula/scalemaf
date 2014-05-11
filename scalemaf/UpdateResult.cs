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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace scalemaf
{
    sealed class UpdateResult
    {
        public int LoadedRecordCount { get; set; }
        public int ClosedLoopCount { get; set; }
        public int OpenLoopCount { get; set; }
        public double IatMin { get; set; }
        public double IatMax { get; set; }
        public double IatAvg { get; set; }
    }
}
