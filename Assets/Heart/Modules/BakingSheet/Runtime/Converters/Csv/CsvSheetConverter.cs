using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Pancake.Csv;
using Pancake.BakingSheet.Internal;
using Pancake.BakingSheet.Raw;

namespace Pancake.BakingSheet
{
    public class CsvSheetConverter : RawSheetConverter
    {
        private readonly IFileSystem _fileSystem;
        private readonly string _loadPath;
        private readonly string _extension;

        private readonly Dictionary<string, List<Page>> _pages = new Dictionary<string, List<Page>>();

        private class CsvTable : List<List<string>>
        {
            public List<string> AddRow()
            {
                var row = new List<string>();
                Add(row);
                return row;
            }
        }

        public CsvSheetConverter(
            string loadPath,
            TimeZoneInfo timeZoneInfo = null,
            string extension = "csv",
            IFileSystem fileSystem = null,
            bool splitHeader = false,
            IFormatProvider formatProvider = null)
            : base(timeZoneInfo, formatProvider, splitHeader)
        {
            _loadPath = loadPath;
            _extension = extension;
            _fileSystem = fileSystem ?? new FileSystem();
        }

        private class Page : IRawSheetImporterPage, IRawSheetExporterPage
        {
            public string SubName { get; }

            public CsvTable Table { get; }

            public Page(CsvTable table, string subName)
            {
                Table = table;
                SubName = subName;
            }

            public string GetCell(int col, int row)
            {
                if (row >= Table.Count) return null;

                if (col >= Table[row].Count) return null;

                return Table[row][col];
            }

            public void SetCell(int col, int row, string data)
            {
                for (int i = Table.Count; i <= row; ++i)
                    Table.AddRow();

                for (int i = Table[row].Count; i <= col; ++i)
                    Table[row].Add(null);

                Table[row][col] = data;
            }
        }

        public override void Reset()
        {
            base.Reset();
            _pages.Clear();
        }

        protected override IEnumerable<IRawSheetImporterPage> GetPages(string sheetName)
        {
            if (_pages.TryGetValue(sheetName, out var pages)) return pages;
            return Enumerable.Empty<IRawSheetImporterPage>();
        }

        protected override IRawSheetExporterPage CreatePage(string sheetName)
        {
            var page = new Page(new CsvTable(), null);
            _pages[sheetName] = new List<Page> {page};
            return page;
        }

        protected override Task<bool> LoadData()
        {
            var files = _fileSystem.GetFiles(_loadPath, _extension);

            _pages.Clear();

            foreach (string file in files)
            {
                using (var stream = _fileSystem.OpenRead(file))
                using (var reader = new StreamReader(stream))
                {
                    var csv = new CsvReader(reader);
                    var table = new CsvTable();

                    while (csv.Read())
                    {
                        var row = table.AddRow();
                        for (var i = 0; i < csv.FieldsCount; ++i) row.Add(csv[i]);
                    }

                    string fileName = Path.GetFileNameWithoutExtension(file);
                    var (sheetName, subName) = Config.ParseSheetName(fileName);

                    if (!_pages.TryGetValue(sheetName, out var sheetList))
                    {
                        sheetList = new List<Page>();
                        _pages.Add(sheetName, sheetList);
                    }

                    sheetList.Add(new Page(table, subName));
                }
            }

            return Task.FromResult(true);
        }

        protected override Task<bool> SaveData()
        {
            _fileSystem.CreateDirectory(_loadPath);

            foreach (var pageItem in _pages)
            {
                string file = Path.Combine(_loadPath, $"{pageItem.Key}.{_extension}");

                using (var stream = _fileSystem.OpenWrite(file))
                using (var writer = new StreamWriter(stream))
                {
                    var csv = new CsvWriter(writer);

                    foreach (var page in pageItem.Value)
                    {
                        foreach (var row in page.Table)
                        {
                            foreach (string cell in row) csv.WriteField(cell);
                            csv.NextRecord();
                        }
                    }
                }
            }

            return Task.FromResult(true);
        }
    }
}