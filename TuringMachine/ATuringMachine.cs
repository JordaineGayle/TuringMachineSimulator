using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TuringMachine
{
    public enum TDirection
    {
        S,
        L,
        R
    }


    public enum TStateType
    {
        Initial,
        Accept,
        Reject
    }

    public record TapeEvent(char Input, TDirection TapeDirection, TapeState TapeState, TState FromState, TState ToState);

    public record TapeState(char Symbol, long Position);

    public record TState(string Label, TStateType Type, string? Color = null)
    {
        public bool Accept => Type == TStateType.Accept;
        public bool Initial => Type == TStateType.Initial;
        public bool Reject => Type == TStateType.Reject;
    }

    public record TInput(TState State, char InputFromTapeAlphabet);

    public record TOutput(TState State, char CharacterToBeWrittenOnTape, TDirection DirectionOnTape)
    {
        public bool MoveLeft => DirectionOnTape == TDirection.L;
        public bool MoveRight => DirectionOnTape == TDirection.R;
        public bool Stay => DirectionOnTape == TDirection.S;
    }

    public record ATuringMachine(
        ISet<TState> States, 
        ISet<char> Alphabet, 
        ISet<char> TapeAlphabet, 
        Func<Dictionary<TInput, TOutput>, TInput, TOutput?> TransitionFunction,
        Dictionary<TInput, TOutput> TransitionRules)
    {


        readonly char[] Tape = FillTape();



        string UserInput = "";



        string TapeStream = "";



        TapeState TapeHead = new TapeState('ϵ', 50);


        
        TapeEvent Write(char inputCharacter, TOutput output) => output.DirectionOnTape switch
        {
            TDirection.R => WriteRight(inputCharacter, output),
            TDirection.L => WriteLeft(inputCharacter, output),
            TDirection.S => Stay(inputCharacter, output),
            _ => throw new NotImplementedException(),
        };


        TapeEvent WriteLeft(char character, TOutput output)
        {
            Tape[TapeHead.Position] = output.CharacterToBeWrittenOnTape;
            var next = TapeHead.Position - 1;
            TapeHead = TapeHead with { Symbol = Tape[next], Position = next };
            var e = new TapeEvent(character, output.DirectionOnTape, TapeHead, GetCurrentState(), output.State);
            CurrentState = output.State;
            return e;
        }


        TapeEvent WriteRight(char character, TOutput output)
        {
            Tape[TapeHead.Position] = output.CharacterToBeWrittenOnTape;
            var next = TapeHead.Position + 1;
            TapeHead = TapeHead with { Symbol = Tape[next], Position = next };
            var e = new TapeEvent(character, output.DirectionOnTape, TapeHead, GetCurrentState(), output.State);
            CurrentState = output.State;
            return e;
        }


        TapeEvent Stay(char character, TOutput output)
        {
            Tape[TapeHead.Position] = output.CharacterToBeWrittenOnTape;
            var next = TapeHead.Position + 0;
            TapeHead = TapeHead with { Symbol = Tape[next], Position = next };
            var e = new TapeEvent(character, output.DirectionOnTape, TapeHead, GetCurrentState(), output.State);
            CurrentState = output.State;
            return e;
        }



        static char[] FillTape(long size = 100) 
        {
            var tape = new char[size];
            Array.Fill(tape, 'ϵ');
            return tape;
        } 


        public TState StartState => States.First(x => x.Initial);



        public TState CurrentState;



        public ISet<TState> AcceptStates => States.Where(x => x.Accept is true).ToHashSet();



        public ISet<TState> RejectStates => States.Where(x => x.Accept is not true).ToHashSet();



        public List<TapeEvent> Events = new List<TapeEvent>();



        public TState GetCurrentState() => CurrentState == null ? StartState : CurrentState;



        public bool Write(char character)
        {
            var input = new TInput(GetCurrentState(), character);
            var output = TransitionFunction(TransitionRules, input);

            if (character == (char)13)
            {
                input = new TInput(GetCurrentState(), 'ϵ');
                output = TransitionFunction(TransitionRules, input);
                UserInput = new string(TapeStream.Replace("ϵ", ""));
                var res = Write('ϵ', output);
                Events.Add(res);
            }
            else if (output != null)
            {
                TapeStream += $"{character}";
                var res = Write(character, output);
                Events.Add(res);
                return true;
            }

            return false;
        }



        public void ProcessTape()
        {
            var moreInputToRead = Write(TapeHead.Symbol);
            while (moreInputToRead)
            {
                moreInputToRead = Write(TapeHead.Symbol);
            }
        }


        public void Print(bool exit = true)
        {
            var path = string.Join(" --> ", Events.Select(x => x.ToState.Label.ToUpper()));
            var tapeData = new string(Tape.Where(x => x != 'ϵ').Where(TapeAlphabet.Contains).ToArray());
            Console.WriteLine($"\nINPUT: {UserInput}");
            Console.WriteLine($"OUTPUT: {tapeData}");
            Console.WriteLine($"TAPE HEAD SYMBOL: {TapeHead.Symbol.ToString().ToUpper()}");
            Console.WriteLine($"CURRENT STATE LABEL: {new string(GetCurrentState().Label.ToUpper())}");
            Console.WriteLine($"ACCEPTED: {(GetCurrentState().Accept ? "YES" : "NO")}");
            Console.WriteLine($"*********************************************************************************\n\n");
            //Console.WriteLine($"PATH: {path}\n\n");
            if (exit ) Environment.Exit(0);
        }



        public static ATuringMachine New(ISet<TState> states, ISet<char> alphabet, 
            ISet<char> tapeAlphabet, Dictionary<TInput, TOutput> transitionRules)
        {
            if (states == null || (states?.Count ?? 0) < 2)
                throw new ArgumentException("a turing machine requires at least 2 states.");

            if (alphabet == null || (alphabet?.Count ?? 0) < 1)
                throw new ArgumentException("a turing machine requires at least 1 input alphabet");

            if (alphabet!.Contains('\0'))
                throw new ArgumentException("the input alphabet for a turing machine must not contain the empty string.");

            if (tapeAlphabet == null || (tapeAlphabet?.Count ?? 0) < 1)
                throw new ArgumentException("a turing machine requires at least 1 tape alphabet");

            tapeAlphabet = tapeAlphabet!.Prepend('ϵ').ToHashSet();
            tapeAlphabet = tapeAlphabet!.Union(alphabet).ToHashSet();


            return new(
                States: states,
                Alphabet: alphabet,
                TapeAlphabet: tapeAlphabet,
                TransitionFunction: (d, i) => 
                {
                    if (d == null) throw new ArgumentException("data is required for this operation.");
                    d.TryGetValue(i, out var output);
                    return output;
                },
                TransitionRules: transitionRules
                );
        }


        public static ATuringMachine CreateTuringMachineForAnagramAndOrPalindromeOfRacecar()
        {
            var epsilon = 'ϵ';


            var states = new HashSet<TState>()
            {
                new("q0", TStateType.Initial),
                new("q1", TStateType.Reject),
                new("q2", TStateType.Accept)
            };


            var alphabet = new HashSet<char>() { 'a', 'c', 'e', 'r' };


            var tapeAlphabet = new HashSet<char>() { '$', '#', '1', '2', '3', '4', 'q', 's', 't', 'v', '|', 'x' };



            var transitionRules = new Dictionary<TInput, TOutput>()
            {
                { new(new("q0", TStateType.Initial), 'a'), new(new("q1", TStateType.Reject), 'a', TDirection.R) },
                { new(new("q0", TStateType.Initial), 'c'), new(new("q1", TStateType.Reject), 'c', TDirection.R) },
                { new(new("q0", TStateType.Initial), 'e'), new(new("q1", TStateType.Reject), 'e', TDirection.R) },
                { new(new("q0", TStateType.Initial), 'r'), new(new("q1", TStateType.Reject), 'r', TDirection.R) },

                { new(new("q1", TStateType.Reject), 'a'), new(new("q1", TStateType.Reject), 'a', TDirection.R) },
                { new(new("q1", TStateType.Reject), 'c'), new(new("q1", TStateType.Reject), 'c', TDirection.R) },
                { new(new("q1", TStateType.Reject), 'e'), new(new("q1", TStateType.Reject), 'e', TDirection.R) },
                { new(new("q1", TStateType.Reject), 'r'), new(new("q1", TStateType.Reject), 'r', TDirection.R) },
                { new(new("q1", TStateType.Reject), epsilon), new(new("q2", TStateType.Reject), '#', TDirection.S) },

                { new(new("q2", TStateType.Reject), '#'), new(new("q3", TStateType.Reject), '#', TDirection.L) },

                { new(new("q3", TStateType.Reject), 'a'), new(new("q3", TStateType.Reject), 'a', TDirection.L) },
                { new(new("q3", TStateType.Reject), 'c'), new(new("q3", TStateType.Reject), 'c', TDirection.L) },
                { new(new("q3", TStateType.Reject), 'r'), new(new("q3", TStateType.Reject), 'r', TDirection.L) },
                { new(new("q3", TStateType.Reject), 'e'), new(new("q3", TStateType.Reject), 'e', TDirection.L) },

                { new(new("q3", TStateType.Reject), epsilon), new(new("q4", TStateType.Reject), '$', TDirection.S) },


                { new(new("q4", TStateType.Reject), '$'), new(new("q5", TStateType.Reject), '$', TDirection.R) },


                { new(new("q5", TStateType.Reject), 'a'), new(new("q5", TStateType.Reject), '1', TDirection.R) },
                { new(new("q5", TStateType.Reject), 'e'), new(new("q5", TStateType.Reject), 'e', TDirection.R) },
                { new(new("q5", TStateType.Reject), 'c'), new(new("q5", TStateType.Reject), 'c', TDirection.R) },
                { new(new("q5", TStateType.Reject), 'r'), new(new("q5", TStateType.Reject), 'r', TDirection.R) },
                { new(new("q5", TStateType.Reject), '#'), new(new("q6", TStateType.Reject), '#', TDirection.L) },


                { new(new("q6", TStateType.Reject), 'c'), new(new("q6", TStateType.Reject), 'c', TDirection.L) },
                { new(new("q6", TStateType.Reject), 'q'), new(new("q6", TStateType.Reject), 'q', TDirection.L) },
                { new(new("q6", TStateType.Reject), 'e'), new(new("q6", TStateType.Reject), 'e', TDirection.L) },
                { new(new("q6", TStateType.Reject), 'r'), new(new("q6", TStateType.Reject), 'r', TDirection.L) },
                { new(new("q6", TStateType.Reject), '1'), new(new("q7", TStateType.Reject), 'q', TDirection.R) },
                { new(new("q6", TStateType.Reject), '$'), new(new("q9", TStateType.Reject), '$', TDirection.R) },


                { new(new("q7", TStateType.Reject), 'r'), new(new("q7", TStateType.Reject), 'r', TDirection.R) },
                { new(new("q7", TStateType.Reject), '#'), new(new("q7", TStateType.Reject), '#', TDirection.R) },
                { new(new("q7", TStateType.Reject), 'q'), new(new("q7", TStateType.Reject), 'q', TDirection.R) },
                { new(new("q7", TStateType.Reject), 'e'), new(new("q7", TStateType.Reject), 'e', TDirection.R) },
                { new(new("q7", TStateType.Reject), 'c'), new(new("q7", TStateType.Reject), 'c', TDirection.R) },
                { new(new("q7", TStateType.Reject), 'a'), new(new("q7", TStateType.Reject), 'a', TDirection.R) },
                { new(new("q7", TStateType.Reject), epsilon), new(new("q8", TStateType.Reject), 'a', TDirection.L) },

                { new(new("q8", TStateType.Reject), 'a'), new(new("q8", TStateType.Reject), 'a', TDirection.L) },
                { new(new("q8", TStateType.Reject), '#'), new(new("q6", TStateType.Reject), '#', TDirection.L) },
                { new(new("q8", TStateType.Reject), 'c'), new(new("q6", TStateType.Reject), 'c', TDirection.L) },
                { new(new("q8", TStateType.Reject), 'e'), new(new("q6", TStateType.Reject), 'e', TDirection.L) },
                { new(new("q8", TStateType.Reject), 'r'), new(new("q6", TStateType.Reject), 'r', TDirection.L) },
                { new(new("q8", TStateType.Reject), 'q'), new(new("q6", TStateType.Reject), 'q', TDirection.L) },

                { new(new("q9", TStateType.Reject), 'c'), new(new("q9", TStateType.Reject), '2', TDirection.R) },
                { new(new("q9", TStateType.Reject), 'q'), new(new("q9", TStateType.Reject), 'q', TDirection.R) },
                { new(new("q9", TStateType.Reject), 'r'), new(new("q9", TStateType.Reject), 'r', TDirection.R) },
                { new(new("q9", TStateType.Reject), 'e'), new(new("q9", TStateType.Reject), 'e', TDirection.R) },
                { new(new("q9", TStateType.Reject), '#'), new(new("q10", TStateType.Reject), '#', TDirection.L) },

                { new(new("q10", TStateType.Reject), 'e'), new(new("q10", TStateType.Reject), 'e', TDirection.L) },
                { new(new("q10", TStateType.Reject), 'a'), new(new("q10", TStateType.Reject), 'a', TDirection.L) },
                { new(new("q10", TStateType.Reject), 'r'), new(new("q10", TStateType.Reject), 'r', TDirection.L) },
                { new(new("q10", TStateType.Reject), 's'), new(new("q10", TStateType.Reject), 's', TDirection.L) },
                { new(new("q10", TStateType.Reject), 'q'), new(new("q10", TStateType.Reject), 'q', TDirection.L) },
                { new(new("q10", TStateType.Reject), '2'), new(new("q11", TStateType.Reject), 's', TDirection.R) },
                { new(new("q10", TStateType.Reject), '$'), new(new("q13", TStateType.Reject), '$', TDirection.R) },


                { new(new("q11", TStateType.Reject), 's'), new(new("q11", TStateType.Reject), 's', TDirection.R) },
                { new(new("q11", TStateType.Reject), 'a'), new(new("q11", TStateType.Reject), 'a', TDirection.R) },
                { new(new("q11", TStateType.Reject), '#'), new(new("q11", TStateType.Reject), '#', TDirection.R) },
                { new(new("q11", TStateType.Reject), 'c'), new(new("q11", TStateType.Reject), 'c', TDirection.R) },
                { new(new("q11", TStateType.Reject), 'e'), new(new("q11", TStateType.Reject), 'e', TDirection.R) },
                { new(new("q11", TStateType.Reject), 'r'), new(new("q11", TStateType.Reject), 'r', TDirection.R) },
                { new(new("q11", TStateType.Reject), 'q'), new(new("q11", TStateType.Reject), 'q', TDirection.R) },
                { new(new("q11", TStateType.Reject), epsilon), new(new("q12", TStateType.Reject), 'c', TDirection.L) },


                { new(new("q12", TStateType.Reject), 'a'), new(new("q12", TStateType.Reject), 'a', TDirection.L) },
                { new(new("q12", TStateType.Reject), 'c'), new(new("q12", TStateType.Reject), 'c', TDirection.L) },
                { new(new("q12", TStateType.Reject), 'e'), new(new("q10", TStateType.Reject), 'e', TDirection.L) },
                { new(new("q12", TStateType.Reject), 'r'), new(new("q10", TStateType.Reject), 'r', TDirection.L) },
                { new(new("q12", TStateType.Reject), 's'), new(new("q10", TStateType.Reject), 's', TDirection.L) },
                { new(new("q12", TStateType.Reject), 'q'), new(new("q10", TStateType.Reject), 'q', TDirection.L) },
                { new(new("q12", TStateType.Reject), '#'), new(new("q10", TStateType.Reject), '#', TDirection.L) },


                { new(new("q13", TStateType.Reject), 'e'), new(new("q13", TStateType.Reject), '3', TDirection.R) },
                { new(new("q13", TStateType.Reject), 'r'), new(new("q13", TStateType.Reject), 'r', TDirection.R) },
                { new(new("q13", TStateType.Reject), 'q'), new(new("q13", TStateType.Reject), 'q', TDirection.R) },
                { new(new("q13", TStateType.Reject), 's'), new(new("q13", TStateType.Reject), 's', TDirection.R) },
                { new(new("q13", TStateType.Reject), '#'), new(new("q14", TStateType.Reject), '#', TDirection.L) },

                { new(new("q14", TStateType.Reject), 't'), new(new("q14", TStateType.Reject), 't', TDirection.L) },
                { new(new("q14", TStateType.Reject), 'q'), new(new("q14", TStateType.Reject), 'q', TDirection.L) },
                { new(new("q14", TStateType.Reject), 's'), new(new("q14", TStateType.Reject), 's', TDirection.L) },
                { new(new("q14", TStateType.Reject), 'r'), new(new("q14", TStateType.Reject), 'r', TDirection.L) },
                { new(new("q14", TStateType.Reject), 'a'), new(new("q14", TStateType.Reject), 'a', TDirection.L) },
                { new(new("q14", TStateType.Reject), 'c'), new(new("q14", TStateType.Reject), 'c', TDirection.L) },
                { new(new("q14", TStateType.Reject), '3'), new(new("q15", TStateType.Reject), 't', TDirection.R) },
                { new(new("q14", TStateType.Reject), '$'), new(new("q17", TStateType.Reject), '$', TDirection.R) },

                { new(new("q15", TStateType.Reject), 'r'), new(new("q15", TStateType.Reject), 'r', TDirection.R) },
                { new(new("q15", TStateType.Reject), 't'), new(new("q15", TStateType.Reject), 't', TDirection.R) },
                { new(new("q15", TStateType.Reject), 'q'), new(new("q15", TStateType.Reject), 'q', TDirection.R) },
                { new(new("q15", TStateType.Reject), 'e'), new(new("q15", TStateType.Reject), 'e', TDirection.R) },
                { new(new("q15", TStateType.Reject), 's'), new(new("q15", TStateType.Reject), 's', TDirection.R) },
                { new(new("q15", TStateType.Reject), '#'), new(new("q15", TStateType.Reject), '#', TDirection.R) },
                { new(new("q15", TStateType.Reject), 'a'), new(new("q15", TStateType.Reject), 'a', TDirection.R) },
                { new(new("q15", TStateType.Reject), 'c'), new(new("q15", TStateType.Reject), 'c', TDirection.R) },
                { new(new("q15", TStateType.Reject), epsilon), new(new("q16", TStateType.Reject), 'e', TDirection.L) },

                { new(new("q16", TStateType.Reject), 'c'), new(new("q16", TStateType.Reject), 'c', TDirection.L) },
                { new(new("q16", TStateType.Reject), 'a'), new(new("q16", TStateType.Reject), 'a', TDirection.L) },
                { new(new("q16", TStateType.Reject), 'e'), new(new("q16", TStateType.Reject), 'e', TDirection.L) },
                { new(new("q16", TStateType.Reject), '#'), new(new("q14", TStateType.Reject), '#', TDirection.L) },
                { new(new("q16", TStateType.Reject), 'q'), new(new("q14", TStateType.Reject), 'q', TDirection.L) },
                { new(new("q16", TStateType.Reject), 't'), new(new("q14", TStateType.Reject), 't', TDirection.L) },
                { new(new("q16", TStateType.Reject), 's'), new(new("q14", TStateType.Reject), 's', TDirection.L) },
                { new(new("q16", TStateType.Reject), 'r'), new(new("q14", TStateType.Reject), 'r', TDirection.L) },


                { new(new("q17", TStateType.Reject), 'r'), new(new("q17", TStateType.Reject), '4', TDirection.R) },
                { new(new("q17", TStateType.Reject), 's'), new(new("q17", TStateType.Reject), 's', TDirection.R) },
                { new(new("q17", TStateType.Reject), 't'), new(new("q17", TStateType.Reject), 't', TDirection.R) },
                { new(new("q17", TStateType.Reject), 'q'), new(new("q17", TStateType.Reject), 'q', TDirection.R) },
                { new(new("q17", TStateType.Reject), '#'), new(new("q18", TStateType.Reject), '#', TDirection.L) },

                { new(new("q18", TStateType.Reject), 's'), new(new("q18", TStateType.Reject), 's', TDirection.L) },
                { new(new("q18", TStateType.Reject), 'a'), new(new("q18", TStateType.Reject), 'a', TDirection.L) },
                { new(new("q18", TStateType.Reject), 't'), new(new("q18", TStateType.Reject), 't', TDirection.L) },
                { new(new("q18", TStateType.Reject), 'v'), new(new("q18", TStateType.Reject), 'v', TDirection.L) },
                { new(new("q18", TStateType.Reject), 'q'), new(new("q18", TStateType.Reject), 'q', TDirection.L) },
                { new(new("q18", TStateType.Reject), 'c'), new(new("q18", TStateType.Reject), 'c', TDirection.L) },
                { new(new("q18", TStateType.Reject), '4'), new(new("q19", TStateType.Reject), 'v', TDirection.R) },
                { new(new("q18", TStateType.Reject), '$'), new(new("q21", TStateType.Reject), '$', TDirection.R) },


                { new(new("q19", TStateType.Reject), '#'), new(new("q19", TStateType.Reject), '#', TDirection.R) },
                { new(new("q19", TStateType.Reject), 'r'), new(new("q19", TStateType.Reject), 'r', TDirection.R) },
                { new(new("q19", TStateType.Reject), 't'), new(new("q19", TStateType.Reject), 't', TDirection.R) },
                { new(new("q19", TStateType.Reject), 'q'), new(new("q19", TStateType.Reject), 'q', TDirection.R) },
                { new(new("q19", TStateType.Reject), 'a'), new(new("q19", TStateType.Reject), 'a', TDirection.R) },
                { new(new("q19", TStateType.Reject), 's'), new(new("q19", TStateType.Reject), 's', TDirection.R) },
                { new(new("q19", TStateType.Reject), 'e'), new(new("q19", TStateType.Reject), 'e', TDirection.R) },
                { new(new("q19", TStateType.Reject), 'v'), new(new("q19", TStateType.Reject), 'v', TDirection.R) },
                { new(new("q19", TStateType.Reject), 'c'), new(new("q19", TStateType.Reject), 'c', TDirection.R) },
                { new(new("q19", TStateType.Reject), epsilon), new(new("q20", TStateType.Reject), 'r', TDirection.L) },

                { new(new("q20", TStateType.Reject), 'c'), new(new("q20", TStateType.Reject), 'c', TDirection.L) },
                { new(new("q20", TStateType.Reject), 'e'), new(new("q20", TStateType.Reject), 'e', TDirection.L) },
                { new(new("q20", TStateType.Reject), 'a'), new(new("q20", TStateType.Reject), 'a', TDirection.L) },
                { new(new("q20", TStateType.Reject), 'r'), new(new("q20", TStateType.Reject), 'r', TDirection.L) },

                { new(new("q20", TStateType.Reject), 'v'), new(new("q18", TStateType.Reject), 'v', TDirection.L) },
                { new(new("q20", TStateType.Reject), 's'), new(new("q18", TStateType.Reject), 's', TDirection.L) },
                { new(new("q20", TStateType.Reject), 't'), new(new("q18", TStateType.Reject), 't', TDirection.L) },
                { new(new("q20", TStateType.Reject), 'q'), new(new("q18", TStateType.Reject), 'q', TDirection.L) },
                { new(new("q20", TStateType.Reject), '#'), new(new("q18", TStateType.Reject), '#', TDirection.L) },


                { new(new("q21", TStateType.Reject), 't'), new(new("q21", TStateType.Reject), 't', TDirection.R) },
                { new(new("q21", TStateType.Reject), 'q'), new(new("q21", TStateType.Reject), 'q', TDirection.R) },
                { new(new("q21", TStateType.Reject), 's'), new(new("q21", TStateType.Reject), 's', TDirection.R) },
                { new(new("q21", TStateType.Reject), 'v'), new(new("q21", TStateType.Reject), 'v', TDirection.R) },
                { new(new("q21", TStateType.Reject), '#'), new(new("q22", TStateType.Reject), '#', TDirection.R) },

                { new(new("q22", TStateType.Reject), 'a'), new(new("q23", TStateType.Reject), 'a', TDirection.R) },
                { new(new("q23", TStateType.Reject), 'a'), new(new("q24", TStateType.Reject), 'a', TDirection.R) },
                { new(new("q24", TStateType.Reject), 'c'), new(new("q25", TStateType.Reject), 'c', TDirection.R) },
                { new(new("q25", TStateType.Reject), 'c'), new(new("q26", TStateType.Reject), 'c', TDirection.R) },
                { new(new("q26", TStateType.Reject), 'e'), new(new("q27", TStateType.Reject), 'e', TDirection.R) },
                { new(new("q27", TStateType.Reject), 'r'), new(new("q28", TStateType.Reject), 'r', TDirection.R) },
                { new(new("q28", TStateType.Reject), 'r'), new(new("q29", TStateType.Reject), 'r', TDirection.R) },


                { new(new("q29", TStateType.Reject), epsilon), new(new("q30", TStateType.Reject), epsilon, TDirection.L) },


                { new(new("q30", TStateType.Reject), 't'), new(new("q30", TStateType.Reject), 'e', TDirection.L) },
                { new(new("q30", TStateType.Reject), 's'), new(new("q30", TStateType.Reject), 'c', TDirection.L) },
                { new(new("q30", TStateType.Reject), 'q'), new(new("q30", TStateType.Reject), 'a', TDirection.L) },
                { new(new("q30", TStateType.Reject), 'v'), new(new("q30", TStateType.Reject), 'r', TDirection.L) },
                { new(new("q30", TStateType.Reject), '#'), new(new("q30", TStateType.Reject), '#', TDirection.L) },
                { new(new("q30", TStateType.Reject), 'e'), new(new("q30", TStateType.Reject), 'e', TDirection.L) },
                { new(new("q30", TStateType.Reject), 'r'), new(new("q30", TStateType.Reject), 'r', TDirection.L) },
                { new(new("q30", TStateType.Reject), 'c'), new(new("q30", TStateType.Reject), 'c', TDirection.L) },
                { new(new("q30", TStateType.Reject), 'a'), new(new("q30", TStateType.Reject), 'a', TDirection.L) },
                { new(new("q30", TStateType.Reject), '$'), new(new("q31", TStateType.Reject), '$', TDirection.R) },

                { new(new("q31", TStateType.Reject), 'e'), new(new("ANAGRAM", TStateType.Accept), 'e', TDirection.R) },
                { new(new("q31", TStateType.Reject), 'c'), new(new("q39", TStateType.Reject), 'c', TDirection.S) },
                { new(new("q31", TStateType.Reject), 'a'), new(new("q39", TStateType.Reject), 'a', TDirection.S) },
                { new(new("q31", TStateType.Reject), 'r'), new(new("q32", TStateType.Reject), 'r', TDirection.R) },


                { new(new("q32", TStateType.Reject), 'a'), new(new("q33", TStateType.Reject), 'a', TDirection.R) },
                { new(new("q32", TStateType.Reject), 'e'), new(new("q38", TStateType.Reject), 'e', TDirection.L) },
                { new(new("q32", TStateType.Reject), 'r'), new(new("q38", TStateType.Reject), 'r', TDirection.L) },
                { new(new("q32", TStateType.Reject), 'c'), new(new("q38", TStateType.Reject), 'c', TDirection.L) },


                { new(new("q33", TStateType.Reject), 'c'), new(new("q34", TStateType.Reject), 'c', TDirection.R) },
                { new(new("q33", TStateType.Reject), 'a'), new(new("q38", TStateType.Reject), 'a', TDirection.L) },
                { new(new("q33", TStateType.Reject), 'e'), new(new("q38", TStateType.Reject), 'e', TDirection.L) },
                { new(new("q33", TStateType.Reject), 'r'), new(new("q38", TStateType.Reject), 'r', TDirection.L) },


                { new(new("q34", TStateType.Reject), 'e'), new(new("q35", TStateType.Reject), 'e', TDirection.R) },
                { new(new("q34", TStateType.Reject), 'a'), new(new("q38", TStateType.Reject), 'a', TDirection.L) },
                { new(new("q34", TStateType.Reject), 'c'), new(new("q38", TStateType.Reject), 'c', TDirection.L) },
                { new(new("q34", TStateType.Reject), 'r'), new(new("q38", TStateType.Reject), 'r', TDirection.L) },



                { new(new("q35", TStateType.Reject), 'c'), new(new("q36", TStateType.Reject), 'c', TDirection.R) },
                { new(new("q35", TStateType.Reject), 'a'), new(new("q38", TStateType.Reject), 'a', TDirection.L) },
                { new(new("q35", TStateType.Reject), 'e'), new(new("q38", TStateType.Reject), 'e', TDirection.L) },
                { new(new("q35", TStateType.Reject), 'r'), new(new("q38", TStateType.Reject), 'r', TDirection.L) },

                { new(new("q36", TStateType.Reject), 'a'), new(new("q37", TStateType.Reject), 'a', TDirection.R) },
                { new(new("q36", TStateType.Reject), 'c'), new(new("q38", TStateType.Reject), 'c', TDirection.L) },
                { new(new("q36", TStateType.Reject), 'e'), new(new("q38", TStateType.Reject), 'e', TDirection.L) },
                { new(new("q36", TStateType.Reject), 'r'), new(new("q38", TStateType.Reject), 'r', TDirection.L) },


                { new(new("q37", TStateType.Reject), 'r'), new(new("PALINDROME", TStateType.Accept), 'r', TDirection.R) },
                { new(new("q37", TStateType.Reject), 'c'), new(new("q38", TStateType.Reject), 'c', TDirection.L) },
                { new(new("q37", TStateType.Reject), 'e'), new(new("q38", TStateType.Reject), 'e', TDirection.L) },
                { new(new("q37", TStateType.Reject), 'a'), new(new("q38", TStateType.Reject), 'a', TDirection.L) },



                { new(new("q38", TStateType.Reject), 'e'), new(new("q38", TStateType.Reject), 'e', TDirection.L) },
                { new(new("q38", TStateType.Reject), 'a'), new(new("q38", TStateType.Reject), 'a', TDirection.L) },
                { new(new("q38", TStateType.Reject), 'r'), new(new("q38", TStateType.Reject), 'r', TDirection.L) },
                { new(new("q38", TStateType.Reject), 'c'), new(new("q38", TStateType.Reject), 'c', TDirection.L) },
                { new(new("q38", TStateType.Reject), '$'), new(new("q39", TStateType.Reject), '$', TDirection.R) },


                { new(new("q39", TStateType.Reject), 'e'), new(new("ANAGRAM", TStateType.Accept), 'e', TDirection.R) },
                { new(new("q39", TStateType.Reject), 'a'), new(new("q40", TStateType.Reject), 'a', TDirection.R) },
                { new(new("q39", TStateType.Reject), 'c'), new(new("q40", TStateType.Reject), 'c', TDirection.R) },
                { new(new("q39", TStateType.Reject), 'r'), new(new("q40", TStateType.Reject), 'r', TDirection.R) },

                { new(new("q40", TStateType.Reject), 'r'), new(new("q40", TStateType.Reject), 'r', TDirection.R) },
                { new(new("q40", TStateType.Reject), 'a'), new(new("q40", TStateType.Reject), 'a', TDirection.R) },
                { new(new("q40", TStateType.Reject), 'e'), new(new("q40", TStateType.Reject), 'e', TDirection.R) },
                { new(new("q40", TStateType.Reject), 'c'), new(new("q40", TStateType.Reject), 'c', TDirection.R) },
                { new(new("q40", TStateType.Reject), '#'), new(new("q40", TStateType.Reject), '#', TDirection.R) },
                { new(new("q40", TStateType.Reject), epsilon), new(new("q41", TStateType.Reject), '|', TDirection.L) },

                { new(new("q41", TStateType.Reject), 'a'), new(new("q41", TStateType.Reject), 'a', TDirection.L) },
                { new(new("q41", TStateType.Reject), '#'), new(new("q41", TStateType.Reject), '#', TDirection.L) },
                { new(new("q41", TStateType.Reject), 'c'), new(new("q41", TStateType.Reject), 'c', TDirection.L) },
                { new(new("q41", TStateType.Reject), 'e'), new(new("q41", TStateType.Reject), 'e', TDirection.L) },
                { new(new("q41", TStateType.Reject), 'r'), new(new("q41", TStateType.Reject), 'r', TDirection.L) },
                { new(new("q41", TStateType.Reject), '$'), new(new("q42", TStateType.Reject), '$', TDirection.R) },

                { new(new("q42", TStateType.Reject), 'e'), new(new("q42", TStateType.Reject), 'e', TDirection.R) },
                { new(new("q42", TStateType.Reject), 'r'), new(new("q43", TStateType.Reject), 'x', TDirection.R) },
                { new(new("q42", TStateType.Reject), 'c'), new(new("q46", TStateType.Reject), 'x', TDirection.R) },
                { new(new("q42", TStateType.Reject), 'a'), new(new("q49", TStateType.Reject), 'x', TDirection.R) },
                { new(new("q42", TStateType.Reject), '#'), new(new("q52", TStateType.Reject), '#', TDirection.R) },

                { new(new("q43", TStateType.Reject), 'c'), new(new("q43", TStateType.Reject), 'c', TDirection.R) },
                { new(new("q43", TStateType.Reject), 'a'), new(new("q43", TStateType.Reject), 'a', TDirection.R) },
                { new(new("q43", TStateType.Reject), 'r'), new(new("q43", TStateType.Reject), 'r', TDirection.R) },
                { new(new("q43", TStateType.Reject), 'e'), new(new("q43", TStateType.Reject), 'e', TDirection.R) },
                { new(new("q43", TStateType.Reject), '#'), new(new("q43", TStateType.Reject), '#', TDirection.R) },
                { new(new("q43", TStateType.Reject), '|'), new(new("q44", TStateType.Reject), '|', TDirection.R) },

                { new(new("q44", TStateType.Reject), 'a'), new(new("q44", TStateType.Reject), 'a', TDirection.R) },
                { new(new("q44", TStateType.Reject), 'r'), new(new("q44", TStateType.Reject), 'r', TDirection.R) },
                { new(new("q44", TStateType.Reject), 'c'), new(new("q44", TStateType.Reject), 'c', TDirection.R) },
                { new(new("q44", TStateType.Reject), epsilon), new(new("q45", TStateType.Reject), 'r', TDirection.L) },

                { new(new("q45", TStateType.Reject), 'a'), new(new("q45", TStateType.Reject), 'a', TDirection.L) },
                { new(new("q45", TStateType.Reject), 'e'), new(new("q45", TStateType.Reject), 'e', TDirection.L) },
                { new(new("q45", TStateType.Reject), 'r'), new(new("q45", TStateType.Reject), 'r', TDirection.L) },
                { new(new("q45", TStateType.Reject), '|'), new(new("q45", TStateType.Reject), '|', TDirection.L) },
                { new(new("q45", TStateType.Reject), '#'), new(new("q45", TStateType.Reject), '#', TDirection.L) },
                { new(new("q45", TStateType.Reject), 'c'), new(new("q45", TStateType.Reject), 'c', TDirection.L) },
                { new(new("q45", TStateType.Reject), 'x'), new(new("q42", TStateType.Reject), 'x', TDirection.R) },

                { new(new("q46", TStateType.Reject), 'c'), new(new("q46", TStateType.Reject), 'c', TDirection.R) },
                { new(new("q46", TStateType.Reject), '#'), new(new("q46", TStateType.Reject), '#', TDirection.R) },
                { new(new("q46", TStateType.Reject), 'e'), new(new("q46", TStateType.Reject), 'e', TDirection.R) },
                { new(new("q46", TStateType.Reject), 'a'), new(new("q46", TStateType.Reject), 'a', TDirection.R) },
                { new(new("q46", TStateType.Reject), 'r'), new(new("q46", TStateType.Reject), 'r', TDirection.R) },
                { new(new("q46", TStateType.Reject), '|'), new(new("q47", TStateType.Reject), '|', TDirection.R) },

                { new(new("q47", TStateType.Reject), 'c'), new(new("q47", TStateType.Reject), 'c', TDirection.R) },
                { new(new("q47", TStateType.Reject), 'a'), new(new("q47", TStateType.Reject), 'a', TDirection.R) },
                { new(new("q47", TStateType.Reject), 'r'), new(new("q47", TStateType.Reject), 'r', TDirection.R) },
                { new(new("q47", TStateType.Reject), epsilon), new(new("q48", TStateType.Reject), 'c', TDirection.L) },

                { new(new("q48", TStateType.Reject), 'c'), new(new("q48", TStateType.Reject), 'c', TDirection.L) },
                { new(new("q48", TStateType.Reject), 'r'), new(new("q48", TStateType.Reject), 'r', TDirection.L) },
                { new(new("q48", TStateType.Reject), '|'), new(new("q48", TStateType.Reject), '|', TDirection.L) },
                { new(new("q48", TStateType.Reject), 'e'), new(new("q48", TStateType.Reject), 'e', TDirection.L) },
                { new(new("q48", TStateType.Reject), 'a'), new(new("q48", TStateType.Reject), 'a', TDirection.L) },
                { new(new("q48", TStateType.Reject), '#'), new(new("q48", TStateType.Reject), '#', TDirection.L) },
                { new(new("q48", TStateType.Reject), 'x'), new(new("q42", TStateType.Reject), 'x', TDirection.R) },


                { new(new("q49", TStateType.Reject), 'a'), new(new("q49", TStateType.Reject), 'a', TDirection.R) },
                { new(new("q49", TStateType.Reject), 'c'), new(new("q49", TStateType.Reject), 'c', TDirection.R) },
                { new(new("q49", TStateType.Reject), 'r'), new(new("q49", TStateType.Reject), 'r', TDirection.R) },
                { new(new("q49", TStateType.Reject), 'e'), new(new("q49", TStateType.Reject), 'e', TDirection.R) },
                { new(new("q49", TStateType.Reject), '#'), new(new("q49", TStateType.Reject), '#', TDirection.R) },
                { new(new("q49", TStateType.Reject), '|'), new(new("q50", TStateType.Reject), '|', TDirection.R) },

                { new(new("q50", TStateType.Reject), 'c'), new(new("q50", TStateType.Reject), 'c', TDirection.R) },
                { new(new("q50", TStateType.Reject), 'r'), new(new("q50", TStateType.Reject), 'r', TDirection.R) },
                { new(new("q50", TStateType.Reject), 'a'), new(new("q50", TStateType.Reject), 'a', TDirection.R) },
                { new(new("q50", TStateType.Reject), epsilon), new(new("q51", TStateType.Reject), 'a', TDirection.L) },

                { new(new("q51", TStateType.Reject), 'r'), new(new("q51", TStateType.Reject), 'r', TDirection.L) },
                { new(new("q51", TStateType.Reject), '#'), new(new("q51", TStateType.Reject), '#', TDirection.L) },
                { new(new("q51", TStateType.Reject), 'c'), new(new("q51", TStateType.Reject), 'c', TDirection.L) },
                { new(new("q51", TStateType.Reject), '|'), new(new("q51", TStateType.Reject), '|', TDirection.L) },
                { new(new("q51", TStateType.Reject), 'e'), new(new("q51", TStateType.Reject), 'e', TDirection.L) },
                { new(new("q51", TStateType.Reject), 'a'), new(new("q51", TStateType.Reject), 'a', TDirection.L) },
                { new(new("q51", TStateType.Reject), 'x'), new(new("q42", TStateType.Reject), 'x', TDirection.R) },

                { new(new("q52", TStateType.Reject), 'r'), new(new("q52", TStateType.Reject), 'r', TDirection.R) },
                { new(new("q52", TStateType.Reject), 'a'), new(new("q52", TStateType.Reject), 'a', TDirection.R) },
                { new(new("q52", TStateType.Reject), 'e'), new(new("q52", TStateType.Reject), 'e', TDirection.R) },
                { new(new("q52", TStateType.Reject), 'c'), new(new("q52", TStateType.Reject), 'c', TDirection.R) },
                { new(new("q52", TStateType.Reject), '|'), new(new("q53", TStateType.Reject), '|', TDirection.R) },


                { new(new("q53", TStateType.Reject), 'x'), new(new("BOTH", TStateType.Accept), 'x', TDirection.S) },
                { new(new("q53", TStateType.Reject), 'a'), new(new("q54", TStateType.Reject), 'x', TDirection.R) },
                { new(new("q53", TStateType.Reject), 'c'), new(new("q57", TStateType.Reject), 'x', TDirection.R) },
                { new(new("q53", TStateType.Reject), 'r'), new(new("q60", TStateType.Reject), 'x', TDirection.R) },


                { new(new("q54", TStateType.Reject), 'r'), new(new("q54", TStateType.Reject), 'r', TDirection.R) },
                { new(new("q54", TStateType.Reject), 'a'), new(new("q54", TStateType.Reject), 'a', TDirection.R) },
                { new(new("q54", TStateType.Reject), 'c'), new(new("q54", TStateType.Reject), 'c', TDirection.R) },
                { new(new("q54", TStateType.Reject), 'x'), new(new("q54", TStateType.Reject), 'x', TDirection.R) },
                { new(new("q54", TStateType.Reject), epsilon), new(new("q55", TStateType.Reject), epsilon, TDirection.L) },

                { new(new("q55", TStateType.Reject), 'x'), new(new("q55", TStateType.Reject), 'x', TDirection.L) },
                { new(new("q55", TStateType.Reject), 'r'), new(new("ANAGRAM", TStateType.Accept), 'r', TDirection.S) },
                { new(new("q55", TStateType.Reject), 'c'), new(new("ANAGRAM", TStateType.Accept), 'c', TDirection.S) },
                { new(new("q55", TStateType.Reject), 'a'), new(new("q56", TStateType.Reject), 'x', TDirection.L) },

                { new(new("q56", TStateType.Reject), 'c'), new(new("q56", TStateType.Reject), 'c', TDirection.L) },
                { new(new("q56", TStateType.Reject), 'r'), new(new("q56", TStateType.Reject), 'r', TDirection.L) },
                { new(new("q56", TStateType.Reject), 'a'), new(new("q56", TStateType.Reject), 'a', TDirection.L) },
                { new(new("q56", TStateType.Reject), 'x'), new(new("q53", TStateType.Reject), 'x', TDirection.R) },




                { new(new("q57", TStateType.Reject), 'r'), new(new("q57", TStateType.Reject), 'r', TDirection.R) },
                { new(new("q57", TStateType.Reject), 'a'), new(new("q57", TStateType.Reject), 'a', TDirection.R) },
                { new(new("q57", TStateType.Reject), 'c'), new(new("q57", TStateType.Reject), 'c', TDirection.R) },
                { new(new("q57", TStateType.Reject), 'x'), new(new("q57", TStateType.Reject), 'x', TDirection.R) },
                { new(new("q57", TStateType.Reject), epsilon), new(new("q58", TStateType.Reject), epsilon, TDirection.L) },

                { new(new("q58", TStateType.Reject), 'x'), new(new("q58", TStateType.Reject), 'x', TDirection.L) },
                { new(new("q58", TStateType.Reject), 'r'), new(new("ANAGRAM", TStateType.Accept), 'r', TDirection.S) },
                { new(new("q58", TStateType.Reject), 'a'), new(new("ANAGRAM", TStateType.Accept), 'a', TDirection.S) },
                { new(new("q58", TStateType.Reject), 'c'), new(new("q59", TStateType.Reject), 'x', TDirection.L) },

                { new(new("q59", TStateType.Reject), 'c'), new(new("q59", TStateType.Reject), 'c', TDirection.L) },
                { new(new("q59", TStateType.Reject), 'r'), new(new("q59", TStateType.Reject), 'r', TDirection.L) },
                { new(new("q59", TStateType.Reject), 'a'), new(new("q59", TStateType.Reject), 'a', TDirection.L) },
                { new(new("q59", TStateType.Reject), 'x'), new(new("q53", TStateType.Reject), 'x', TDirection.R) },


                { new(new("q60", TStateType.Reject), 'r'), new(new("q60", TStateType.Reject), 'r', TDirection.R) },
                { new(new("q60", TStateType.Reject), 'a'), new(new("q60", TStateType.Reject), 'a', TDirection.R) },
                { new(new("q60", TStateType.Reject), 'c'), new(new("q60", TStateType.Reject), 'c', TDirection.R) },
                { new(new("q60", TStateType.Reject), 'x'), new(new("q60", TStateType.Reject), 'x', TDirection.R) },
                { new(new("q60", TStateType.Reject), epsilon), new(new("q61", TStateType.Reject), epsilon, TDirection.L) },

                { new(new("q61", TStateType.Reject), 'x'), new(new("q61", TStateType.Reject), 'x', TDirection.L) },
                { new(new("q61", TStateType.Reject), 'c'), new(new("ANAGRAM", TStateType.Accept), 'c', TDirection.S) },
                { new(new("q61", TStateType.Reject), 'a'), new(new("ANAGRAM", TStateType.Accept), 'a', TDirection.S) },
                { new(new("q61", TStateType.Reject), 'r'), new(new("q62", TStateType.Reject), 'x', TDirection.L) },

                { new(new("q62", TStateType.Reject), 'c'), new(new("q62", TStateType.Reject), 'c', TDirection.L) },
                { new(new("q62", TStateType.Reject), 'r'), new(new("q62", TStateType.Reject), 'r', TDirection.L) },
                { new(new("q62", TStateType.Reject), 'a'), new(new("q62", TStateType.Reject), 'a', TDirection.L) },
                { new(new("q62", TStateType.Reject), 'x'), new(new("q53", TStateType.Reject), 'x', TDirection.R) },

            };


            return New(states, alphabet, tapeAlphabet, transitionRules);
        }
    }
}
