using System;
using System.Collections.Generic;
using System.IO;

namespace Pancake.Csv
{
    /// <summary>
    /// Fast and memory efficient implementation of CSV reader (3x times faster than CsvHelper).
    /// </summary>
    /// <remarks>API is similar to CSVHelper CsvReader.</remarks>
    public class CsvReader
    {
        public string Delimiter { get; private set; }
        private readonly TextReader _rdr;
        private readonly int _delimLength;
        private char[] _buffer;
        private int _bufferLength;
        private int _bufferLoadThreshold;
        private int _lineStartPos;
        private int _actualBufferLen;
        private List<Field> _fields;
        private int _linesRead;

        public int FieldsCount { get; private set; }

        /// <summary>
        /// Size of the circular buffer. Buffer size limits max length of the CSV line that can be processed. 
        /// </summary>
        /// <remarks>Default buffer size is 32kb.</remarks>
        public int BufferSize { get; set; } = 32768;

        /// <summary>
        /// If true start/end spaces are excluded from field values (except values in quotes). True by default.
        /// </summary>
        public bool TrimFields { get; set; } = true;

        public CsvReader(TextReader rdr, string delimiter = ",")
        {
            _rdr = rdr;
            Delimiter = delimiter;
            _delimLength = delimiter.Length;

            if (_delimLength == 0)
                throw new ArgumentException("Delimiter cannot be empty.");
        }


        private int ReadBlockAndCheckEof(char[] buffer, int start, int len, ref bool eof)
        {
            if (len == 0) return 0;
            int read = _rdr.ReadBlock(buffer, start, len);
            if (read < len) eof = true;
            return read;
        }

        private bool FillBuffer()
        {
            var eof = false;
            int toRead = _bufferLength - _actualBufferLen;
            if (toRead >= _bufferLoadThreshold)
            {
                int freeStart = (_lineStartPos + _actualBufferLen) % _buffer.Length;
                if (freeStart >= _lineStartPos)
                {
                    _actualBufferLen += ReadBlockAndCheckEof(_buffer, freeStart, _buffer.Length - freeStart, ref eof);
                    if (_lineStartPos > 0)
                        _actualBufferLen += ReadBlockAndCheckEof(_buffer, 0, _lineStartPos, ref eof);
                }
                else
                {
                    _actualBufferLen += ReadBlockAndCheckEof(_buffer, freeStart, toRead, ref eof);
                }
            }

            return eof;
        }

        private string GetLineTooLongMsg() { return string.Format("CSV line #{1} length exceedes buffer size ({0})", BufferSize, _linesRead); }

        private int ReadQuotedFieldToEnd(int start, int maxPos, bool eof, ref int escapedQuotesCount)
        {
            int pos = start;
            for (; pos < maxPos; pos++)
            {
                int chIdx = pos < _bufferLength ? pos : pos % _bufferLength;
                char ch = _buffer[chIdx];
                if (ch == '\"')
                {
                    bool hasNextCh = (pos + 1) < maxPos;
                    if (hasNextCh && _buffer[(pos + 1) % _bufferLength] == '\"')
                    {
                        // double quote inside quote = just a content
                        pos++;
                        escapedQuotesCount++;
                    }
                    else
                    {
                        return pos;
                    }
                }
            }

            if (eof)
            {
                // this is incorrect CSV as quote is not closed
                // but in case of EOF lets ignore that
                return pos - 1;
            }

            throw new InvalidDataException(GetLineTooLongMsg());
        }

        private bool ReadDelimTail(int start, int maxPos, ref int end)
        {
            var offset = 1;
            for (; offset < _delimLength; offset++)
            {
                int pos = start + offset;
                int idx = pos < _bufferLength ? pos : pos % _bufferLength;
                if (pos >= maxPos || _buffer[idx] != Delimiter[offset])
                    return false;
            }

            end = start + offset - 1;
            return true;
        }

        private Field GetOrAddField(int startIdx)
        {
            FieldsCount++;
            while (FieldsCount > _fields.Count) _fields.Add(new Field());
            var f = _fields[FieldsCount - 1];
            f.Reset(startIdx);
            return f;
        }

        public string this[int idx]
        {
            get
            {
                if (idx < FieldsCount)
                {
                    var f = _fields[idx];
                    return _fields[idx].GetValue(_buffer);
                }

                return null;
            }
        }

        public int GetValueLength(int idx)
        {
            if (idx < FieldsCount)
            {
                var f = _fields[idx];
                return f.quoted ? f.Length - f.escapedQuotesCount : f.Length;
            }

            return -1;
        }

        public void ProcessValueInBuffer(int idx, Action<char[], int, int> handler)
        {
            if (idx < FieldsCount)
            {
                var f = _fields[idx];
                if ((f.quoted && f.escapedQuotesCount > 0) || f.end >= _bufferLength)
                {
                    char[] chArr = f.GetValue(_buffer).ToCharArray();
                    handler(chArr, 0, chArr.Length);
                }
                else if (f.quoted)
                {
                    handler(_buffer, f.start + 1, f.Length - 2);
                }
                else
                {
                    handler(_buffer, f.start, f.Length);
                }
            }
        }

        public bool Read()
        {
            Start:
            if (_fields == null)
            {
                _fields = new List<Field>();
                FieldsCount = 0;
            }

            if (_buffer == null)
            {
                _bufferLoadThreshold = Math.Min(BufferSize, 8192);
                _bufferLength = BufferSize + _bufferLoadThreshold;
                _buffer = new char[_bufferLength];
                _lineStartPos = 0;
                _actualBufferLen = 0;
            }

            bool eof = FillBuffer();

            FieldsCount = 0;
            if (_actualBufferLen <= 0)
            {
                return false; // no more data
            }

            _linesRead++;

            int maxPos = _lineStartPos + _actualBufferLen;
            int charPos = _lineStartPos;

            var currentField = GetOrAddField(charPos);
            var ignoreQuote = false;
            char delimFirstChar = Delimiter[0];
            bool trimFields = TrimFields;

            for (; charPos < maxPos; charPos++)
            {
                int charBufIdx = charPos < _bufferLength ? charPos : charPos % _bufferLength;
                char ch = _buffer[charBufIdx];
                switch (ch)
                {
                    case '\"':
                        if (ignoreQuote)
                        {
                            currentField.end = charPos;
                        }
                        else if (currentField.quoted || currentField.Length > 0)
                        {
                            // current field already is quoted = lets treat quotes as usual chars
                            currentField.end = charPos;
                            currentField.quoted = false;
                            ignoreQuote = true;
                        }
                        else
                        {
                            int endQuotePos = ReadQuotedFieldToEnd(charPos + 1, maxPos, eof, ref currentField.escapedQuotesCount);
                            currentField.start = charPos;
                            currentField.end = endQuotePos;
                            currentField.quoted = true;
                            charPos = endQuotePos;
                        }

                        break;
                    case '\r':
                        if ((charPos + 1) < maxPos && _buffer[(charPos + 1) % _bufferLength] == '\n')
                        {
                            // \r\n handling
                            charPos++;
                        }

                        // in some files only \r used as line separator - lets allow that
                        charPos++;
                        goto LineEnded;
                    case '\n':
                        charPos++;
                        goto LineEnded;
                    default:
                        if (ch == delimFirstChar && (_delimLength == 1 || ReadDelimTail(charPos, maxPos, ref charPos)))
                        {
                            currentField = GetOrAddField(charPos + 1);
                            ignoreQuote = false;
                            continue;
                        }

                        // space
                        if (ch == ' ' && trimFields)
                        {
                            continue; // do nothing
                        }

                        // content char
                        if (currentField.Length == 0) currentField.start = charPos;

                        if (currentField.quoted)
                        {
                            // non-space content after quote = treat quotes as part of content
                            currentField.quoted = false;
                            ignoreQuote = true;
                        }

                        currentField.end = charPos;
                        break;
                }
            }

            if (!eof)
            {
                // line is not finished, but whole buffer was processed and not EOF
                throw new InvalidDataException(GetLineTooLongMsg());
            }

            LineEnded:
            _actualBufferLen -= charPos - _lineStartPos;
            _lineStartPos = charPos % _bufferLength;

            if (FieldsCount == 1 && _fields[0].Length == 0)
            {
                // skip empty lines
                goto Start;
            }

            return true;
        }


        internal sealed class Field
        {
            internal int start;
            internal int end;
            internal int Length => end - start + 1;
            internal bool quoted;
            internal int escapedQuotesCount;
            private string _cachedValue;

            internal Field Reset(int start)
            {
                this.start = start;
                end = start - 1;
                quoted = false;
                escapedQuotesCount = 0;
                _cachedValue = null;
                return this;
            }

            internal string GetValue(char[] buf) { return _cachedValue ??= GetValueInternal(buf); }

            private string GetValueInternal(char[] buf)
            {
                if (quoted)
                {
                    int s = start + 1;
                    int lenWithoutQuotes = Length - 2;
                    string val = lenWithoutQuotes > 0 ? GetString(buf, s, lenWithoutQuotes) : string.Empty;
                    if (escapedQuotesCount > 0)
                        val = val.Replace("\"\"", "\"");
                    return val;
                }

                int len = Length;
                return len > 0 ? GetString(buf, start, len) : string.Empty;
            }

            private string GetString(char[] buf, int start, int len)
            {
                int bufLen = buf.Length;
                start = start < bufLen ? start : start % bufLen;
                int endIdx = start + len - 1;
                if (endIdx >= bufLen)
                {
                    int prefixLen = buf.Length - start;
                    var prefix = new string(buf, start, prefixLen);
                    var suffix = new string(buf, 0, len - prefixLen);
                    return prefix + suffix;
                }

                return new string(buf, start, len);
            }
        }
    }
}