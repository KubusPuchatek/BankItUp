using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PdfParser.Models;

namespace PdfParser
{
    class Program
    {
        static void Main(string[] args)
        {
            ShowMenu();
        }

        private static void ShowMenu()
        {
            do
            {
                Console.WriteLine("Welcome to Human Factor!");
                Console.WriteLine();
                Console.WriteLine("1. Brand24");
                Console.WriteLine("2. SentiOne");
                Console.WriteLine();
                Console.WriteLine("q. Quit");

                Console.WriteLine();
                Console.Write("Youre choice: ");
                var userChioce = Console.ReadLine();

                switch (userChioce)
                {
                    case "1":
                        Execute("brand24");
                        Console.WriteLine();
                        Console.WriteLine("Press any butto to return to menu...");
                        Console.ReadKey();
                        Console.Clear();
                        ShowMenu();
                        break;

                    case "2":
                        Execute("sentione");
                        Console.WriteLine();
                        Console.WriteLine("Press any butto to return to menu...");
                        Console.ReadKey();
                        Console.Clear();
                        ShowMenu();
                        break;

                    case "q":
                        Environment.Exit(0);
                        break;

                    default:
                        Console.WriteLine("Unknown choice...");
                        Console.ReadKey();
                        Console.Clear();
                        ShowMenu();
                        break;
                }

            } while (true);
        }

        private static void Execute(string companyName)
        {
            Console.WriteLine();
            Console.WriteLine("Loading data, please wait...");

            var parser = new Parser();
            var users = new List<ProfileData>();

            var directory = new DirectoryInfo(Environment.CurrentDirectory + @"\Files\"+companyName+@"\");

            foreach (var file in directory.GetFiles())
            {
                var pdfContent = parser.ReadPdfFile(file.FullName);
                if (parser.TryParse(pdfContent, out var result))
                {
                    users.Add(result);
                }
            }

            var managers = parser.GetManagersProfiles(users);
            var restOfCrew = users.Except(managers).ToList();
            

            var excelParser = new FundsParser();
            var grants = excelParser.Parse(companyName);

            var managersExperience = 0;
            var managersEducation = 0;

            managers.ForEach(x => managersEducation += x.Education);
            managers.ForEach(x => managersExperience += x.ProfessionalExperience);
            Console.WriteLine();
            Console.WriteLine("Managers: {0}", managers.Count);
            Console.WriteLine("Managers Experience: {0}", managersExperience);
            Console.WriteLine("Managers Education: {0}", managersEducation);
            Console.WriteLine();

            
            var experience = 0;
            var education = 0;

            restOfCrew.ForEach(x => education += x.Education);
            restOfCrew.ForEach(x => experience += x.ProfessionalExperience);
            Console.WriteLine();

            Console.WriteLine("Other Users: {0}", restOfCrew.Count);
            Console.WriteLine("Experience: {0}", experience);
            Console.WriteLine("Education: {0}", education);

            Console.WriteLine();

            var totalExperience = 0;
            var totalEducation = 0;

            users.ForEach(x => totalEducation += x.Education);
            users.ForEach(x => totalExperience += x.ProfessionalExperience);
            Console.WriteLine();
            Console.WriteLine("Total Users: {0}", users.Count);
            Console.WriteLine("Total Experience: {0}", totalExperience);
            Console.WriteLine("Total Education: {0}", totalEducation);
            Console.WriteLine();

            var userWithTheMostAwards = users.Aggregate((i1, i2) => i1.HonorsAndAwards > i2.HonorsAndAwards ? i1 : i2);
            Console.WriteLine();
            Console.WriteLine("User with the most awards: {0}", userWithTheMostAwards.Id);
            Console.WriteLine("Sum of points: {0}", userWithTheMostAwards.Education + userWithTheMostAwards.ProfessionalExperience);
            Console.WriteLine("Number of awards: {0}", userWithTheMostAwards.HonorsAndAwards);
            Console.WriteLine();

            Console.WriteLine("Grants amount: {0}", grants.Count);
            decimal grantsValue = 0;
            grants.ForEach(x =>
            {
                decimal.TryParse(x.Amount, out var value);
                grantsValue += value;
            });
            Console.WriteLine("Grants value: {0}", grantsValue);
        }
    }
}
