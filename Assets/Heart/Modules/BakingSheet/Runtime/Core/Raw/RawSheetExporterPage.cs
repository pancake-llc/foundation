using System;
using System.Collections.Generic;
using Pancake.BakingSheet.Internal;

namespace Pancake.BakingSheet.Raw
{
    /// <summary>
    /// Single page of a Spreadsheet workbook for exporting.
    /// </summary>
    public interface IRawSheetExporterPage
    {
        void SetCell(int col, int row, string data);
    }
}