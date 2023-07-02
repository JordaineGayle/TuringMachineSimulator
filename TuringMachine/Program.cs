using System.Text.RegularExpressions;
using TuringMachine;

Console.WriteLine("Welcome");

var userInput = "aceryϵ";
var blank = 'ϵ';

var userInputArray = userInput.ToCharArray();
Array.Resize(ref userInputArray, userInputArray.Length+1);
userInputArray[userInputArray.Length-1] = '\r';

var states = new HashSet<TState>()
{
    new("q0", TStateType.Initial),
    new("q1", TStateType.Reject),
    new("q2", TStateType.Accept)
};


var alphabet = new HashSet<char>(){'a', 'c', 'e', 'r'};


var tapeAlphabet = new HashSet<char>() { '#' };



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
    { new(new("q1", TStateType.Reject), blank), new(new("q2", TStateType.Reject), '#', TDirection.S) },

    { new(new("q2", TStateType.Reject), '#'), new(new("q3", TStateType.Reject), '#', TDirection.L) },
    
    { new(new("q3", TStateType.Reject), 'a'), new(new("q3", TStateType.Reject), 'a', TDirection.L) },
    { new(new("q3", TStateType.Reject), 'c'), new(new("q3", TStateType.Reject), 'c', TDirection.L) },
    { new(new("q3", TStateType.Reject), 'r'), new(new("q3", TStateType.Reject), 'r', TDirection.L) },
    { new(new("q3", TStateType.Reject), 'e'), new(new("q3", TStateType.Reject), 'e', TDirection.L) },

    { new(new("q3", TStateType.Reject), blank), new(new("q4", TStateType.Reject), '$', TDirection.S) },


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
    { new(new("q7", TStateType.Reject), '#'), new(new("q7", TStateType.Reject), 'r', TDirection.R) },
    { new(new("q7", TStateType.Reject), 'q'), new(new("q7", TStateType.Reject), 'r', TDirection.R) },
    { new(new("q7", TStateType.Reject), 'e'), new(new("q7", TStateType.Reject), 'r', TDirection.R) },
    { new(new("q7", TStateType.Reject), 'c'), new(new("q7", TStateType.Reject), 'r', TDirection.R) },
    { new(new("q7", TStateType.Reject), 'a'), new(new("q7", TStateType.Reject), 'r', TDirection.R) },
    { new(new("q7", TStateType.Reject), blank), new(new("q8", TStateType.Reject), 'a', TDirection.L) },

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
    { new(new("q10", TStateType.Reject), 'a'), new(new("q10", TStateType.Reject), 'e', TDirection.L) },
    { new(new("q10", TStateType.Reject), 'r'), new(new("q10", TStateType.Reject), 'e', TDirection.L) },
    { new(new("q10", TStateType.Reject), 's'), new(new("q10", TStateType.Reject), 'e', TDirection.L) },
    { new(new("q10", TStateType.Reject), 'q'), new(new("q10", TStateType.Reject), 'e', TDirection.L) },
    { new(new("q10", TStateType.Reject), '2'), new(new("q11", TStateType.Reject), 's', TDirection.R) },
    { new(new("q10", TStateType.Reject), '$'), new(new("q13", TStateType.Reject), '$', TDirection.R) },


    { new(new("q11", TStateType.Reject), 's'), new(new("q11", TStateType.Reject), 's', TDirection.R) },
    { new(new("q11", TStateType.Reject), 'a'), new(new("q11", TStateType.Reject), 'a', TDirection.R) },
    { new(new("q11", TStateType.Reject), '#'), new(new("q11", TStateType.Reject), '#', TDirection.R) },
    { new(new("q11", TStateType.Reject), 'c'), new(new("q11", TStateType.Reject), 'c', TDirection.R) },
    { new(new("q11", TStateType.Reject), 'e'), new(new("q11", TStateType.Reject), 'e', TDirection.R) },
    { new(new("q11", TStateType.Reject), 'r'), new(new("q11", TStateType.Reject), 'r', TDirection.R) },
    { new(new("q11", TStateType.Reject), 'q'), new(new("q11", TStateType.Reject), 'q', TDirection.R) },
    { new(new("q11", TStateType.Reject), blank), new(new("q12", TStateType.Reject), 'c', TDirection.L) },


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
    { new(new("q15", TStateType.Reject), blank), new(new("q16", TStateType.Reject), 'e', TDirection.L) },

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
    { new(new("q19", TStateType.Reject), blank), new(new("q20", TStateType.Reject), 'r', TDirection.L) },

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


    { new(new("q29", TStateType.Reject), blank), new(new("q30", TStateType.Reject), blank, TDirection.L) },


    { new(new("q30", TStateType.Reject), 't'), new(new("q30", TStateType.Reject), 'e', TDirection.L) },
    { new(new("q30", TStateType.Reject), 's'), new(new("q30", TStateType.Reject), 'c', TDirection.L) },
    { new(new("q30", TStateType.Reject), 'q'), new(new("q30", TStateType.Reject), 'a', TDirection.L) },
    { new(new("q30", TStateType.Reject), 'v'), new(new("q30", TStateType.Reject), 'r', TDirection.L) },
    { new(new("q30", TStateType.Reject), '#'), new(new("q30", TStateType.Reject), '$', TDirection.L) },
    { new(new("q30", TStateType.Reject), 'e'), new(new("q30", TStateType.Reject), 'e', TDirection.L) },
    { new(new("q30", TStateType.Reject), 'r'), new(new("q30", TStateType.Reject), 'r', TDirection.L) },
    { new(new("q30", TStateType.Reject), 'c'), new(new("q30", TStateType.Reject), 'c', TDirection.L) },
    { new(new("q30", TStateType.Reject), 'a'), new(new("q30", TStateType.Reject), 'a', TDirection.L) },
    { new(new("q30", TStateType.Reject), '$'), new(new("q31", TStateType.Reject), '$', TDirection.R) },

    { new(new("q31", TStateType.Reject), 'e'), new(new("ANAGRAM", TStateType.Accept), 'e', TDirection.S) },
    { new(new("q31", TStateType.Reject), 'c'), new(new("q39", TStateType.Reject), 'c', TDirection.S) },
    { new(new("q31", TStateType.Reject), 'a'), new(new("q39", TStateType.Reject), 'c', TDirection.S) },
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


    { new(new("q37", TStateType.Reject), 'r'), new(new("PALINDROME", TStateType.Accept), 'r', TDirection.S) },
    { new(new("q36", TStateType.Reject), 'c'), new(new("q38", TStateType.Reject), 'c', TDirection.L) },
    { new(new("q36", TStateType.Reject), 'e'), new(new("q38", TStateType.Reject), 'e', TDirection.L) },
    { new(new("q36", TStateType.Reject), 'a'), new(new("q38", TStateType.Reject), 'a', TDirection.L) },



    { new(new("q38", TStateType.Reject), 'e'), new(new("q38", TStateType.Reject), 'e', TDirection.L) },
    { new(new("q38", TStateType.Reject), 'a'), new(new("q38", TStateType.Reject), 'a', TDirection.L) },
    { new(new("q38", TStateType.Reject), 'r'), new(new("q38", TStateType.Reject), 'r', TDirection.L) },
    { new(new("q38", TStateType.Reject), 'c'), new(new("q38", TStateType.Reject), 'c', TDirection.L) },
    { new(new("q38", TStateType.Reject), '$'), new(new("q39", TStateType.Reject), '$', TDirection.R) },


    { new(new("q39", TStateType.Reject), 'e'), new(new("ANAGRAM", TStateType.Accept), 'e', TDirection.S) },
    { new(new("q39", TStateType.Reject), 'a'), new(new("q40", TStateType.Reject), 'a', TDirection.R) },
    { new(new("q39", TStateType.Reject), 'c'), new(new("q40", TStateType.Reject), 'c', TDirection.R) },
    { new(new("q39", TStateType.Reject), 'r'), new(new("q40", TStateType.Reject), 'r', TDirection.R) },

    { new(new("q40", TStateType.Reject), 'r'), new(new("q40", TStateType.Reject), 'r', TDirection.R) },
    { new(new("q40", TStateType.Reject), 'a'), new(new("q40", TStateType.Reject), 'a', TDirection.R) },
    { new(new("q40", TStateType.Reject), 'e'), new(new("q40", TStateType.Reject), 'e', TDirection.R) },
    { new(new("q40", TStateType.Reject), 'c'), new(new("q40", TStateType.Reject), 'c', TDirection.R) },
    { new(new("q40", TStateType.Reject), '#'), new(new("q40", TStateType.Reject), '#', TDirection.R) },
    { new(new("q40", TStateType.Reject), blank), new(new("q41", TStateType.Reject), '|', TDirection.L) },

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
    { new(new("q44", TStateType.Reject), 'r'), new(new("q44", TStateType.Reject), 'a', TDirection.R) },
    { new(new("q44", TStateType.Reject), 'c'), new(new("q44", TStateType.Reject), 'a', TDirection.R) },
    { new(new("q44", TStateType.Reject), blank), new(new("q45", TStateType.Reject), 'r', TDirection.L) },

    { new(new("q45", TStateType.Reject), 'a'), new(new("q45", TStateType.Reject), 'a', TDirection.L) },
    { new(new("q45", TStateType.Reject), 'e'), new(new("q45", TStateType.Reject), 'e', TDirection.L) },
    { new(new("q45", TStateType.Reject), 'r'), new(new("q45", TStateType.Reject), 'r', TDirection.L) },
    { new(new("q45", TStateType.Reject), '|'), new(new("q45", TStateType.Reject), '|', TDirection.L) },
    { new(new("q45", TStateType.Reject), '#'), new(new("q45", TStateType.Reject), '#', TDirection.L) },
    { new(new("q45", TStateType.Reject), 'c'), new(new("q45", TStateType.Reject), 'c', TDirection.L) },
    { new(new("q45", TStateType.Reject), 'x'), new(new("q42", TStateType.Reject), 'x', TDirection.R) },

    //TODO 43 onward

};


var machine = ATuringMachine.New(states, alphabet, tapeAlphabet, transitionRules);


foreach(var input in userInputArray)
{
    machine.Read(input);
}