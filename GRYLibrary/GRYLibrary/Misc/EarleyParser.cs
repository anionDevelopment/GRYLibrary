using System;
using System.Collections.Generic;
using System.Linq;

namespace GRYLibrary.Core.Misc
{
    internal/*Internal because implementation not finished yet*/ class EarleyParser
    {
        private SortedSet<State>[] S;

        private void Initialize(object[] words)
        {
            this.S = new SortedSet<State>[words.Length + 1];
            for (int k = 0; k <= words.Length; k++)
            {
                this.S[k] = [];
            }
        }
        public SortedSet<State>[] Parse(object[] words, Grammar grammar)
        {
            this.Initialize(words);
            // S[0].Add(new State() { Rule = new ProductionRuleWithExpection() { Identifier = new NonTerminalSymbol() { Value = string.Empty }, Substituted = new List<Symbol>(), Expected  = S }, J = 0 });
            for (int k = 0; k <= words.Length; k++)
            {
                foreach (State state in this.S[k]) // S[k] can expand during this loop
                {
                    if (this.Finished(state))
                    {
                        if (this.NextElementOfState(state) is TerminalSymbol)
                        {
                            this.Scanner(state, k, words);
                        }
                        else
                        {
                            this.Predictor(state, k, grammar);
                        }
                    }
                    else
                    {
                        this.Completer(state, k);
                    }
                }
            }
            return this.S;
        }

        private Symbol NextElementOfState(State state)
        {
            throw new NotImplementedException();
        }

        private bool Finished(State state)
        {
            throw new NotImplementedException();
        }

        private void Predictor(State state, int k, Grammar grammar)
        {
            foreach (ProductionRule rule in this.GrammarRules(state.Rule.Expected, grammar))
            {
                this.S[k].Add(new State()
                {
                    Rule = new ProductionRuleWithExpection()
                    {
                        Identifier = (NonTerminalSymbol)state.Rule.Expected.First(),
                        Substituted = [],
                        Expected = rule.Subsitution,
                    },
                    J = k,
                });
            }
        }

        private IEnumerable<ProductionRule> GrammarRules(List<Symbol> B, Grammar grammar)
        {
            throw new NotImplementedException();
        }

        private void Scanner(State state, int k, object[] words)
        {
            if (this.PartsOfSpeech(words[k]).Contains(state.Rule.Expected.First()))
            {
                this.S[k + 1].Add(new State()
                {
                    Rule = new ProductionRuleWithExpection()
                    {
                        Identifier = state.Rule.Identifier,
                        Substituted = [.. state.Rule.Substituted, .. new List<Symbol>() { state.Rule.Expected.First() }],
                        Expected = [.. state.Rule.Expected.Skip(1)]
                    },
                    J = state.J
                });
            }
        }

        private IEnumerable<Symbol> PartsOfSpeech(object word)
        {
            throw new NotImplementedException();
        }

        private void Completer(State state, int k)
        {
            foreach (State state2 in this.S[state.J])
            {
                this.S[k].Add(new State()
                {
                    Rule = new ProductionRuleWithExpection()
                    {
                        Identifier = state2.Rule.Identifier,
                        Substituted = [.. state2.Rule.Substituted, .. new List<Symbol>() { state2.Rule.Expected.First() }],
                        Expected = [.. state2.Rule.Expected.Skip(1)]
                    },
                    J = state2.J
                });
            }
        }
        public class Grammar
        {
            public List<ProductionRule> Rules;
        }
        public abstract class Symbol
        {
            public string Value;
        }
        public class TerminalSymbol : Symbol
        {

        }
        public class NonTerminalSymbol : Symbol
        {

        }
        public class State
        {
            public ProductionRuleWithExpection Rule;
            public int J;
        }
        public class ProductionRuleWithExpection
        {
            public NonTerminalSymbol Identifier;
            public List<Symbol> Substituted;
            public List<Symbol> Expected;
        }
        public class ProductionRule
        {
            public NonTerminalSymbol Identifier;
            public List<Symbol> Subsitution;
        }
    }
}