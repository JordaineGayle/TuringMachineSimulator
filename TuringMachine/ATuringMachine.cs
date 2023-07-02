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

    public record TapeEvent(TDirection TapeDirection, TapeState TapeState, TState FromState, TState ToState);

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



        TapeState TapeHead = new TapeState('ϵ', 50);



        void ProcessOutput(TState state, TOutput? output)
        {
            if (output == null && (GetCurrentState().Reject || GetCurrentState().Initial))
                Console.WriteLine("\n\nREJECTED");
            else if (output == null && GetCurrentState().Accept)
                Console.WriteLine("\n\nACCEPTED");
            else if (output != null && output.State.Accept)
                Console.WriteLine("\n\nACCEPTED");
            else if (output != null && (output.State.Reject || output.State.Initial))
                Console.WriteLine("\n\nREJECTED");
            Environment.Exit(0);
        }



        TapeEvent Write(char inputCharacter, TOutput output) => output.DirectionOnTape switch
        {
            TDirection.R => WriteRight(inputCharacter, output),
            TDirection.L => WriteLeft(inputCharacter, output),
            TDirection.S => Stay(inputCharacter, output),
            _ => throw new NotImplementedException(),
        };


        TapeEvent WriteLeft(char character, TOutput output)
        {
            Tape[TapeHead.Position] = character;
            var next = TapeHead.Position - 1;
            TapeHead = TapeHead with { Symbol = Tape[next], Position = next };
            var e = new TapeEvent(output.DirectionOnTape, TapeHead, GetCurrentState(), output.State);
            CurrentState = output.State;
            return e;
        }


        TapeEvent WriteRight(char character, TOutput output)
        {
            Tape[TapeHead.Position] = character;
            var next = TapeHead.Position + 1;
            TapeHead = TapeHead with { Symbol = Tape[next], Position = next };
            var e = new TapeEvent(output.DirectionOnTape, TapeHead, GetCurrentState(), output.State);
            CurrentState = output.State;
            return e;
        }


        TapeEvent Stay(char character, TOutput output)
        {
            Tape[TapeHead.Position] = character;
            var next = TapeHead.Position + 0;
            TapeHead = TapeHead with { Symbol = Tape[next], Position = next };
            var e = new TapeEvent(output.DirectionOnTape, TapeHead, GetCurrentState(), output.State);
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


        public void Read(char character)
        {
            var input = new TInput(GetCurrentState(), character);
            var output = TransitionFunction(TransitionRules, input);

            if (character == (char)13)
            {
                ProcessOutput(GetCurrentState(), output);
            }
            if (output == null)
            {
                ProcessOutput(GetCurrentState(), output);
            }
            else
            {
                var res = Write(character, output);
                Events.Add(res);
            }
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
    }
}
