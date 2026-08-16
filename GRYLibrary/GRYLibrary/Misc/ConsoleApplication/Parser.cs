using CommandLine.Text;
using CommandLine;
using GRYLibrary.Core.Logging.GeneralPurposeLogger;
using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;

namespace GRYLibrary.Core.Misc.ConsoleApplication
{
    public abstract class ParserBase
    {
        public string[] OriginalArguments { get; internal set; }
        public string OriginalArgumentsAsString { get; internal set; }
        internal IGeneralLogger _Logger;
        internal SentenceBuilder _SentenceBuilder;
        internal GRYConsoleApplicationInitialInformation ApplicationInitialInformation;
        protected abstract int RunImplementation(ParserResult<object> parsed);
        public Type[] GetVerbs()
        {
            Type[] result = [.. this.GetVerbsImplementation(), typeof(Help), typeof(Info)];
            //TODO check if all types inherit from ICommandlineParameter
            return result;
        }
        protected abstract Type[] GetVerbsImplementation();
        public abstract void Accept(IParserBaseVisitor visitor);
        public abstract T Accept<T>(IParserBaseVisitor<T> visitor);
        public int Run(ParserResult<object> parsed)
        {
            return this.RunImplementation(parsed);
        }

        protected Func<IEnumerable<Error>, int> Error(string argumentsAsString)
        {
            return errors =>
            {
                try
                {
                    int amountOfErrors = errors.Count();
                    this._Logger.Log($"Argument '{argumentsAsString}' could not be parsed successfully.", LogLevel.Error);
                    if (0 < amountOfErrors)
                    {
                        this._Logger.Log($"The following error{(amountOfErrors == 1 ? string.Empty : "s")} occurred:", LogLevel.Error);
                        foreach (Error error in errors)
                        {
                            this._Logger.Log($"{error.Tag}: {this._SentenceBuilder.FormatError(error)}", LogLevel.Error);
                        }
                    }
                }
                catch
                {
                    throw;
                }
                return 1;
            };
        }

        protected int ShowInformation(Info options)
        {
            options.ParserBase = this;
            return options.Run(this.ApplicationInitialInformation);
        }

        protected int ShowHelp(Help options)
        {
            options.ParserBase = this;
            return options.Run(this.ApplicationInitialInformation);
        }

    }
    public interface IParserBaseVisitor
    {
        void Handle<Verb01>(VerbParser<Verb01> parserBase);
        void Handle<Verb01, Verb02>(VerbParser<Verb01, Verb02> parserBase);
        void Handle<Verb01, Verb02, Verb03>(VerbParser<Verb01, Verb02, Verb03> parserBase);
    }
    public interface IParserBaseVisitor<T>
    {
        T Handle<Verb01>(VerbParser<Verb01> parserBase);
        T Handle<Verb01, Verb02>(VerbParser<Verb01, Verb02> parserBase);
        T Handle<Verb01, Verb02, Verb03>(VerbParser<Verb01, Verb02, Verb03> parserBase);
    }

    public class VerbParser<Verb01> : ParserBase
    {
        private readonly Func<Verb01, GRYConsoleApplicationInitialInformation, int> _Verb01Runner;
        public VerbParser(Func<Verb01, GRYConsoleApplicationInitialInformation, int> verb01Runner)
        {
            this._Verb01Runner = verb01Runner;
        }

        protected override int RunImplementation(ParserResult<object> parsed)
        {
            return parsed.MapResult((Help options) => this.ShowHelp(options),
                                    (Info options) => this.ShowInformation(options),
                                    (Verb01 options) => this._Verb01Runner(options, this.ApplicationInitialInformation),
                                    this.Error(this.OriginalArgumentsAsString));
        }

        protected override Type[] GetVerbsImplementation()
        {
            return [typeof(Verb01)];
        }

        #region Overhead
        public override void Accept(IParserBaseVisitor visitor)
        {
            visitor.Handle(this);
        }

        public override T Accept<T>(IParserBaseVisitor<T> visitor)
        {
            return visitor.Handle(this);
        }
        #endregion
    }
    public class VerbParser<Verb01, Verb02> : ParserBase
    {
        private readonly Func<Verb01, GRYConsoleApplicationInitialInformation, int> _Verb01Runner;
        private readonly Func<Verb02, GRYConsoleApplicationInitialInformation, int> _Verb02Runner;
        public VerbParser(Func<Verb01, GRYConsoleApplicationInitialInformation, int> verb01Runner, Func<Verb02, GRYConsoleApplicationInitialInformation, int> verb02Runner)
        {
            this._Verb01Runner = verb01Runner;
            this._Verb02Runner = verb02Runner;
        }

        protected override int RunImplementation(ParserResult<object> parsed)
        {
            return parsed.MapResult((Help options) => this.ShowHelp(options),
                                    (Info options) => this.ShowInformation(options),
                                    (Verb01 options) => this._Verb01Runner(options, this.ApplicationInitialInformation),
                                    (Verb02 options) => this._Verb02Runner(options, this.ApplicationInitialInformation),
                                    this.Error(this.OriginalArgumentsAsString));
        }

        protected override Type[] GetVerbsImplementation()
        {
            return [typeof(Verb01), typeof(Verb02)];
        }

        #region Overhead
        public override void Accept(IParserBaseVisitor visitor)
        {
            visitor.Handle(this);
        }

        public override T Accept<T>(IParserBaseVisitor<T> visitor)
        {
            return visitor.Handle(this);
        }
        #endregion
    }
    public class VerbParser<Verb01, Verb02, Verb03> : ParserBase
    {
        private readonly Func<Verb01, GRYConsoleApplicationInitialInformation, int> _Verb01Runner;
        private readonly Func<Verb02, GRYConsoleApplicationInitialInformation, int> _Verb02Runner;
        private readonly Func<Verb03, GRYConsoleApplicationInitialInformation, int> _Verb03Runner;
        public VerbParser(Func<Verb01, GRYConsoleApplicationInitialInformation, int> verb01Runner, Func<Verb02, GRYConsoleApplicationInitialInformation, int> verb02Runner, Func<Verb03, GRYConsoleApplicationInitialInformation, int> verb03Runner)
        {
            this._Verb01Runner = verb01Runner;
            this._Verb02Runner = verb02Runner;
            this._Verb03Runner = verb03Runner;
        }

        protected override int RunImplementation(ParserResult<object> parsed)
        {
            return parsed.MapResult((Help options) => this.ShowHelp(options),
                                    (Info options) => this.ShowInformation(options),
                                    (Verb01 options) => this._Verb01Runner(options, this.ApplicationInitialInformation),
                                    (Verb02 options) => this._Verb02Runner(options, this.ApplicationInitialInformation),
                                    (Verb03 options) => this._Verb03Runner(options, this.ApplicationInitialInformation),
                                    this.Error(this.OriginalArgumentsAsString));
        }

        protected override Type[] GetVerbsImplementation()
        {
            return [typeof(Verb01), typeof(Verb02), typeof(Verb03)];
        }

        #region Overhead
        public override void Accept(IParserBaseVisitor visitor)
        {
            visitor.Handle(this);
        }

        public override T Accept<T>(IParserBaseVisitor<T> visitor)
        {
            return visitor.Handle(this);
        }
        #endregion
    }
}
