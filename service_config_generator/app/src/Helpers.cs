using System;
namespace ServiceConfigGenerator.src;

public class Helpers
{
    public static string ReadConsoleInput(string input = "", string prompt = "")
    {
        if (String.IsNullOrEmpty(input))
        {
            Console.WriteLine(prompt);
            bool userInputProcess = true;
            int loopHardLimit = 100;
            int count = 0;
            while (userInputProcess == true && count < loopHardLimit)
            {
                input = Console.ReadLine() ?? "";
                if (String.IsNullOrEmpty(input))
                {
                    Console.WriteLine("Please input a string here.");
                }
                else
                {
                    userInputProcess = false;
                }
                count++;
            }
            if (count == 100)
            {
                Console.WriteLine("Helper: Looped to many times trying to read console input.");
                Environment.Exit(1);
            }
        }
        return input;
    }

    public static string CheckReport(List<string> errors, bool shouldThrow)
    {
        if (errors.Count == 0) return "";
        string report = string.Join(Environment.NewLine, errors);
        string preface = "The following error(s) were found:\n";
        report = preface + report;
        if (shouldThrow)
        {
            throw new Exception(report);
        }
        else
        {
            return report;
        }
    }
}