using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TuringMachine
{
    public record Driver()
    {
        public void Run()
        {
            var machine = ATuringMachine.CreateTuringMachineForAnagramAndOrPalindromeOfRacecar();
            AcceptUserInput(machine);
            machine.ProcessTape();
            machine.Print(exit: false);
        }

        public void Test()
        {
            List<string> userInputs = new();
            userInputs.Add("acrerca\r");
            userInputs.Add("carerac\r");
            userInputs.Add("craearc\r");
            userInputs.Add("arcecra\r");
            userInputs.Add("racecar\r");
            userInputs.Add("rcaeacr\r");
            userInputs.Add("racerca\r");
            userInputs.Add("erraacc\r");
            userInputs.Add("rraacce\r");
            userInputs.Add("eerraacc\r");
            userInputs.Add("ere\r");
            userInputs.Add("re\r");
            userInputs.Add("a\r");
            userInputs.Add("c\r");
            userInputs.Add("e\r");
            userInputs.Add("r\r");

            foreach(var input in userInputs)
            {
                var machine = ATuringMachine.CreateTuringMachineForAnagramAndOrPalindromeOfRacecar();
                var inputArray = input.ToCharArray();
                foreach(var symbol in inputArray)
                {
                    machine.Write(symbol);
                }
                machine.ProcessTape();
                machine.Print(exit: false);
            }
        }


        private void AcceptUserInput(ATuringMachine machine)
        {
            Console.Write("Enter input: ");
            while (machine.Write(Console.ReadKey().KeyChar)) { }
        }
    }
}
