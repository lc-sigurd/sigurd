using System;
using System.Collections.Generic;
using System.Text;

namespace Sigurd.Terminal.Parsing;

internal class RawCommandSplitter
{
    public static IEnumerable<string> Split(string rawCommand)
    {
        return new SplitParser(rawCommand).Parse();
    }

    private class SplitParser
    {
        private const char SingleQuote = '\'';
        private const char DoubleQuote = '\"';
        private const char Escape = '\\';

        private static bool IsDelimiter(char c) => Char.IsWhiteSpace(c);
        private static bool IsSingleQuote(char c) => c == SingleQuote;
        private static bool IsDoubleQuote(char c) => c == DoubleQuote;
        private static bool IsEscape(char c) => c == Escape;

        private readonly string _rawCommand;
        private int _position = 0;
        private readonly StringBuilder _tokenBuilder;
        private State _currentState = State.Default;

        public SplitParser(string rawCommand)
        {
            _rawCommand = rawCommand;
            _tokenBuilder = new StringBuilder(rawCommand.Length);
        }

        public IEnumerable<string> Parse()
        {
            while (_position < _rawCommand.Length) {
                if (InState(State.Escaped & State.WithinSingleQuotes)) {
                    throw new InvalidStateException("Characters cannot be escaped within single quotes.");
                }

                if (InState(State.Escaped & State.WithinDoubleQuotes)) {
                    if (IsDoubleQuote(Current) | IsEscape(Current)) {
                        _tokenBuilder.Append(Current);
                        _currentState &= ~State.Escaped;
                        Advance();
                        continue;
                    }

                    _tokenBuilder.Append(Escape);
                    _currentState &= ~State.Escaped;
                    continue;
                }

                if (InState(State.Escaped)) {
                    if (IsDelimiter(Current) | IsSingleQuote(Current) | IsDoubleQuote(Current) | IsEscape(Current)) {
                        _tokenBuilder.Append(Current);
                        _currentState &= ~State.Escaped;
                        Advance();
                        continue;
                    }

                    _tokenBuilder.Append(Escape);
                    _currentState &= ~State.Escaped;
                    continue;
                }

                if (IsSingleQuote(Current)) {
                    if (InState(State.WithinDoubleQuotes)) {
                        _tokenBuilder.Append(Current);
                        Advance();
                        continue;
                    }

                    _currentState ^= State.WithinSingleQuotes;
                    _currentState |= State.AllowEmptyToken;
                    Advance();
                    continue;
                }

                if (IsDoubleQuote(Current)) {
                    if (InState(State.WithinSingleQuotes)) {
                        _tokenBuilder.Append(Current);
                        Advance();
                        continue;
                    }

                    _currentState ^= State.WithinDoubleQuotes;
                    _currentState |= State.AllowEmptyToken;
                    Advance();
                    continue;
                }

                if (IsEscape(Current) & !InState(State.WithinSingleQuotes)) {
                    _currentState |= State.Escaped;
                    Advance();
                    continue;
                }

                if (InState(State.WithinSingleQuotes) | InState(State.WithinDoubleQuotes)) {
                    _tokenBuilder.Append(Current);
                    Advance();
                    continue;
                }

                if (IsDelimiter(Current)) {
                    if (TokenIsValid()) {
                        yield return _tokenBuilder.ToString();
                    }

                    _tokenBuilder.Clear();
                    _currentState &= ~State.AllowEmptyToken;
                    Advance();
                    continue;
                }

                _tokenBuilder.Append(Current);
                Advance();
            }

            if (InState(State.WithinSingleQuotes)) {
                throw new RawCommandSyntaxException("Mismatched single-quote");
            }

            if (InState(State.WithinDoubleQuotes)) {
                throw new RawCommandSyntaxException("Mismatched double-quote");
            }

            if (InState(State.Escaped)) {
                throw new RawCommandSyntaxException("Cannot escape end-of-command");
            }

            if (TokenIsValid()) {
                yield return _tokenBuilder.ToString();
            }
        }

        private void Advance(int positions = 1) => _position += positions;

        private char Current => _rawCommand[_position];

        private bool InState(State state) => (_currentState & state) > 0;

        private bool TokenIsValid() => InState(State.AllowEmptyToken) || _tokenBuilder.Length > 0;

        [Flags]
        enum State {
            Default = 0,
            WithinSingleQuotes = 1 << 0,
            WithinDoubleQuotes = 1 << 1,
            Escaped = 1 << 2,
            AllowEmptyToken = 1 << 3,
        }

        public class InvalidStateException : Exception
        {
            public InvalidStateException() { }

            public InvalidStateException(string message) : base(message) { }

            public InvalidStateException(string message, Exception inner) : base(message, inner) { }
        }

        public class RawCommandSyntaxException : Exception
        {
            public RawCommandSyntaxException() { }

            public RawCommandSyntaxException(string message) : base(message) { }

            public RawCommandSyntaxException(string message, Exception inner) : base(message, inner) { }
        }
    }
}
