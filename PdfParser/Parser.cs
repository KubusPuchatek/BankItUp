using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using iTextSharp.text.pdf;
using iTextSharp.text.pdf.parser;
using PdfParser.Extensions;
using PdfParser.Models;

namespace PdfParser
{
    public class Parser
    {
        private readonly IEnumerable<string> _parts = new[] { "Summary", "Experience", "Education", "Honors and Awards" };

        private readonly IEnumerable<string> _managingPositions = new[]
        {
            "owner", "founder", "co-Founder", "co-owner", "dyrektor", "director", "chief", "prezes", "wice-prezes",
            "president", "vice-president", "head", "naczelnik"
        };
        private readonly IEnumerable<string> _excludedEducations = new[] {"lo ", "gymnasium", "gimnazjum", "technikum", "zespół szkół"};

        public string ReadPdfFile(string fileName)
        {
            StringBuilder text = new StringBuilder();

            if (File.Exists(fileName))
            {
                PdfReader pdfReader = new PdfReader(fileName);

                for (var page = 1; page <= pdfReader.NumberOfPages; page++)
                {
                    ITextExtractionStrategy strategy = new SimpleTextExtractionStrategy();
                    var currentText = PdfTextExtractor.GetTextFromPage(pdfReader, page, strategy);

                    currentText = Encoding.UTF8.GetString(Encoding.Convert(Encoding.Default, Encoding.UTF8, Encoding.Default.GetBytes(currentText)));
                    text.Append(currentText);
                }
                pdfReader.Close();
            }
            return text.ToString();
        }

        public bool TryParse(string accountData, out ProfileData profileData)
        {
            try
            {
                profileData = new ProfileData();
                Dictionary<string, List<string>> accountDataDictionary = new Dictionary<string, List<string>>();
                using (StringReader reader = new StringReader(accountData))
                {
                    var userName = "";
                    var section = "";
                    var rewards = "";
                    var sectionParts = new List<string>();
                    string line = string.Empty;
                    string previousLine = string.Empty;
                    int education = 0;
                    int experience = 0;
                    do
                    {
                        line = reader.ReadLine();

                        if (line != null)
                        {
                            var indexOfEnd = line.LastIndexOf("Page", StringComparison.Ordinal);

                            if (indexOfEnd != -1)
                            {
                                line = line.Substring(0, indexOfEnd);
                            }

                            if (_parts.Contains(line))
                            {
                                accountDataDictionary.Add(section, sectionParts);
                                sectionParts = new List<string>();
                                section = line;
                                previousLine = line;
                            }
                            else
                            {
                                switch (section)
                                {
                                    case "Experience":
                                        if (line.TryParseLinkedInEmploymentDuration(out var duration))
                                        {
                                            if (string.IsNullOrEmpty(profileData.Position))
                                            {
                                                profileData.Position = previousLine;
                                            }

                                            //Plus one month because of wrong result during parsing end date - it should be set to last day of the last mont not first
                                            var exp = ((duration[1].Year - duration[0].Year) * 12) + duration[1].Month -
                                                      duration[0].Month + 1;
                                            experience += exp;
                                        }


                                        break;

                                    case "Education":
                                        var res = Regex.Match(line, @"[0-9]{4}\s-\s[0-9]{4}");
                                        if (res.Success)
                                        {
                                            if (_excludedEducations.Any(x => previousLine.ToLower().Contains(x)))
                                            {
                                                break;
                                            }
                                            var ttt = res.Groups[0].Value.Split('-').Select(p => p.Trim()).ToList();

                                            int.TryParse(ttt[0], out var startEducation);
                                            int.TryParse(ttt[1], out var endEducation);

                                            education += endEducation - startEducation;
                                        }

                                        break;

                                    case "Honors and Awards":
                                        if (line.Equals(userName))
                                        {
                                            section = "";
                                            break;
                                        }

                                        rewards += line;
                                        break;

                                    default:
                                        if (line.Contains("Page"))
                                        {
                                            continue;
                                        }
                                        userName = string.IsNullOrEmpty(userName) ? line : userName;
                                        break;
                                }
                            }
                            sectionParts.Add(line);
                            previousLine = line;
                        }


                    } while (line != null);

                    if (education==0 && experience == 0)
                    {
                        profileData = null;
                        return false;
                    }

                    profileData.HonorsAndAwards = string.IsNullOrEmpty(rewards) ? 0 : rewards.Split(',').ToList().Count;
                    profileData.Education = education*12;
                    profileData.ProfessionalExperience = experience;
                    profileData.Id = userName;

                    Console.WriteLine("Name:{0}", profileData.Id);
                    Console.WriteLine("Education:{0}", profileData.Education);
                    Console.WriteLine("Experience:{0}", profileData.ProfessionalExperience);
                    Console.WriteLine("Awards:{0}", profileData.HonorsAndAwards);
                    Console.WriteLine();
                }

                return true;
            }
            catch (Exception)
            {
                profileData = null;
                return false;
            }
        }

        public List<ProfileData> GetManagersProfiles(List<ProfileData> allProfiles)
        {
            var managers = new List<ProfileData>();
            foreach (var profileData in allProfiles)
            {
                if (profileData.Position == null)
                    continue;
                
                if (_managingPositions.Any(x=> profileData.Position.ToLower().Contains(x)))
                {
                    if (profileData.Position.ToLower().Contains("product owner"))
                    {
                        continue;
                    }
                    managers.Add(profileData);
                }
                else
                {
                    var result = Regex.Match(profileData.Position, @"C\w\w\s");
                    if (result.Success)
                    {
                        managers.Add(profileData);
                    }
                }
            }

            return managers.ToList();

        }
    }
}
