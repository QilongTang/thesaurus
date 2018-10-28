﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ParseJSON
{
    public class ParseDYF
    {
        public StringBuilder csvcontent;
        public Regex CleanupRegex;

        public ParseDYF()
        {
            csvcontent = InitializeCsvContent();
            CleanupRegex = CreateCleanupRegex();
        }

        public List<string> GetDyfsInDir(string path)
        {
            List<string> files = new List<string>();
            try
            {
                foreach (var d in Directory.GetDirectories(path))
                {
                    Console.WriteLine(d);
                    foreach (var f in Directory.GetFiles(d))
                    {
                        files.Add(f);
                        //Console.WriteLine(f);

                        // Filter out all the DYF files
                        if (f.EndsWith("dyf", StringComparison.OrdinalIgnoreCase))
                            files.Add(f);

                        string[] fullText = System.IO.File.ReadAllLines(f);
                        string lines = fullText[0];
                        List<string> matches = GetMatches(lines);

                        try
                        {
                            //string matchId = CleanupRegex.Replace(matches[0], "");
                            string matchId = matches[0];
                            matchId = matchId.Replace("\"", "");
                            matchId = matchId.Replace("ID=", "");

                            //MatchCollection matches = CleanupRegex.Matches((string) matches[0]);
                            Console.WriteLine(matchId);
                            string csvLine = matchId + "," + f;
                            csvcontent.AppendLine(csvLine);
                        }
                        catch { }
                    }
                    GetDyfsInDir(d);
                }
            }
            catch (Exception)
            {
                // ignore 
            }

            Console.WriteLine("FINISHED PARSING");

            return files;
        }

        public StringBuilder InitializeCsvContent()
        {
            StringBuilder csvcontent = new StringBuilder();
            //csvcontent.AppendLine("A,B");
            return csvcontent;
        }

        public void ExportCSV()
        {
            string dirPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            File.WriteAllText(dirPath + "\\packageData.csv", csvcontent.ToString());
        }
        

        public static Regex CreateIDRegex()
        {
            string sample = "ID = 6c712c67-329c-4499-a399-d1cedd2b45bf";


            string dash = "(-)"; // Dash
            string id = "(ID=)"; // LETTERS
            string chars = "([0-9a-fA-F]+)";

            Regex reg = new Regex(@"ID=.[0-9a-fA-F]+-[0-9a-fA-F]+-[0-9a-fA-F]+-[0-9a-fA-F]+-[0-9a-fA-F]+.");
            Regex reg2 = new Regex(id);


            return reg;
        }

        public Regex CreateCleanupRegex()
        {
            string patternString = @"""([^""]|"""")*""";
            //string quote = @"(")";

            Regex pattern = new Regex(patternString);

            return pattern;
        }

        public static List<string> GetMatches(string fileString)
        {
            List<string> matchStrings = new List<string>(); //Create empty container

            Regex reg = CreateIDRegex();
            MatchCollection matches = reg.Matches(fileString); //Get all match occurances in a document

            string resultString = "EMPTY"; //Set default value for the string to placate MSVS

            foreach (Match match in matches) //Loop over all regex matches found
            {
                if (match.Success) //If the match is successful
                {
                    resultString = match.Value; //Get the text of the matched KTMS code
                    //Console.WriteLine(match.Value);
                    matchStrings.Add(resultString); //Add the text to the list of matched KTMS codes
                }
            }

            return matchStrings;

        }

    }
}
