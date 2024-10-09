using System.Diagnostics.Metrics;
using System.Globalization;
using static System.Net.Mime.MediaTypeNames;

namespace Number_Guessing_Game
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            await NumberGuessingGame.Start();
        }
    }


    public class NumberGuessingGame
    {
        private static readonly ThreadLocal<Random> _rnd = new ThreadLocal<Random>(() => new Random());
        private const int MinNumber = 1;
        private const int MaxNumber = 100;

        private static int _gameDifficulity = 1;
        private static int _gameChances = 0;
        private static int _numberToGuess;
        private static Dictionary<int,int> _chancesByDifficulity = new Dictionary<int,int>()
        {
            {1,10},
            {2,5},
            {3,3}
        };
        private static Dictionary<int, string> _difficulityNames = new Dictionary<int, string>()
        {
            {1,"Easy"},
            {2,"Medium"},
            {3,"Hard"}
        };

        private static async Task PrintGreeting()
        {
            PrintToConsole($"Welcome to the Number Guessing Game!\r\n" +
                                 $"I'm thinking of a number between {MinNumber} and {MaxNumber}.\r\n" +
                                 $"You have 5 chances to guess the correct number.\r\n");


            PrintToConsole("Continue?(Press Enter) ", 5);
            var key = Console.ReadKey();
            if(key.Key != ConsoleKey.Enter)
                Environment.Exit(0);
        }
        private static void PrintToConsole(string text, int printSpeed = 10)
        {
            for (int i = 0; i < text.Length; i++)
            {
                if (Console.KeyAvailable && Console.ReadKey(true).Key == ConsoleKey.Enter)
                {
                    Console.Write(text.Substring(i));
                    break;
                }
                Console.Write(text[i]);
                Thread.Sleep(printSpeed);
            }
        }

        private static void ClearConsole()
        {
            Console.Clear();
        }

        private static async Task<int> SelectGameDifficulty()
        {
            bool isSelected = false;
            int selection = 0;

            PrintToConsole($"Great, now please select the difficulty level:\r\n" +
                                 "1.Easy(10 chances) \r\n" +
                                 "2.Medium(5 chances)\r\n" +
                                 "3.Hard(3 chances)\r\n");
            PrintToConsole("Enter your choice(1-3):", 5);
            while (!isSelected)
            {
                if (int.TryParse(Console.ReadLine(), out selection))
                {
                    if (selection >= 1 && selection <= 3)
                        isSelected = true;
                }
                else
                {
                    PrintToConsole("Enter correct choice(1-3):", 1);
                }
            }

            return selection;
        }

        public static async Task Start()
        {
            await PrintGreeting();
            ClearConsole();

            _gameDifficulity = await SelectGameDifficulty();
            ClearConsole();

            _difficulityNames.TryGetValue(_gameDifficulity, out string diffName);
            PrintToConsole($"Great! You have selected the {diffName} difficulty level.\r\n" +
                                 $"Let's start the game!\n");
            await StartGame();
        }

        public static async Task StartGame()
        {
            _chancesByDifficulity.TryGetValue(_gameDifficulity, out _gameChances);

            if (_gameChances == 0)
                return;
            _numberToGuess = _rnd.Value!.Next(MinNumber, MaxNumber + 1);

            for (int i = 0; i < _gameChances; i++)
            {
                PrintToConsole("Enter your guess:");
                var guess = Console.ReadLine();
                if (int.TryParse(guess, out int guessNumber))
                {
                    if (!IsValidGuess(guessNumber))
                    {
                        PrintToConsole($"Incorrect! {guess} is out of range, I guess the number is only from {MinNumber} to {MaxNumber}.\n");
                        continue;
                    }

                    if (guessNumber == _numberToGuess)
                    {
                        ClearConsole();
                        await GameWin(i + 1);
                        return;
                    }
                    else
                    {
                        var hint = _numberToGuess < guessNumber ? "less" : "greater";
                        PrintToConsole($"Incorrect! The number is {hint} than {guess}.\n");
                    }
                }
                else
                {
                    PrintToConsole($"Incorrect! {guess} is not a number\n");
                }
                Console.WriteLine();
            }

            ClearConsole();
            await GameLost();
        }
        private static bool IsValidGuess(int guessNumber)
        {
            return guessNumber >= MinNumber && guessNumber <= MaxNumber;
        }


        private static async Task GameLost()
        {
            PrintToConsole($"You've lost? the number was: {_numberToGuess}\n");
            await OfferReplay();
        }

        private static async Task GameWin(int attempts)
        {
            PrintToConsole($"Congratulations! You guessed the correct number in {attempts.ToString()} attempts.\n");
            await OfferReplay();
        }

        private static async Task OfferReplay()
        {
            PrintToConsole($"Do you want to replay:(y/n) ");
            bool isCorrectKey = false;

            while (!isCorrectKey)
            {
                var key = Console.ReadKey();
                if (key.Key == ConsoleKey.Y)
                {
                    isCorrectKey = true;
                    ClearConsole();
                    await Start();
                }
                else if (key.Key == ConsoleKey.N)
                {
                    Environment.Exit(0);
                }
                Console.WriteLine();
            }
        }
    }
}
