using System.IO;
using System.Text;

namespace Pancake.Csv
{
    /// <summary>
    /// Fast and efficient implementation of CSV writer.
    /// </summary>
    /// <remarks>API is similar to CSVHelper CsvWriter class</remarks>
    public class CsvWriter
    {
        public string Delimiter { get; private set; }

        public string QuoteString
        {
            get => _quoteString;
            set
            {
                _quoteString = value;
                _doubleQuoteString = value + value;
            }
        }

        public bool QuoteAllFields { get; set; } = false;

        public bool QuoteIfTrimPossible { get; set; } = true;

        public bool Trim { get; set; } = false;

        private readonly char[] _quoteRequiredChars;
        private readonly bool _checkDelimForQuote;
        private string _quoteString = "\"";
        private string _doubleQuoteString = "\"\"";
        private readonly TextWriter _wr;
        private readonly StringBuilder _sb;

        public CsvWriter(TextWriter wr, string delimiter = ",")
        {
            _wr = wr;
            _sb = new StringBuilder();
            Delimiter = delimiter;
            _checkDelimForQuote = delimiter.Length > 1;
            _quoteRequiredChars = _checkDelimForQuote ? new[] {'\r', '\n'} : new[] {'\r', '\n', delimiter[0]};
        }

        private int _recordFieldCount = 0;

        public void WriteField(string field)
        {
            bool shouldQuote = QuoteAllFields;

            field ??= string.Empty;

            if (field.Length > 0 && Trim) field = field.Trim();

            if (field.Length > 0)
            {
                if (QuoteIfTrimPossible && (field[0] == ' ' || field[^1] == ' ')) shouldQuote = true;

                if (shouldQuote // Quote all fields
                    || field.Contains(_quoteString) // Contains quote
                    || field.IndexOfAny(_quoteRequiredChars) > -1 // Contains chars that require quotes
                    || (_checkDelimForQuote && field.Contains(Delimiter)) // Contains delimiter
                   )
                {
                    shouldQuote = true;
                }
            }

            if (shouldQuote)
            {
                _sb.Clear();
                _sb.Append(_quoteString);
                if (field.Length > 0) _sb.Append(field.Replace(_quoteString, _doubleQuoteString));
                _sb.Append(_quoteString);
                field = _sb.ToString();
            }

            if (_recordFieldCount > 0) _wr.Write(Delimiter);

            if (field.Length > 0) _wr.Write(field);
            _recordFieldCount++;
        }

        public void NextRecord()
        {
            _wr.WriteLine();
            _recordFieldCount = 0;
        }
    }
}