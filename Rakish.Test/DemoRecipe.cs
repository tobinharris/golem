using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using Rakish.Core;

namespace Rakish.Test
{
    [Recipe(Name="demo2")]
    public class Demo2Recipe
    {
        [Task(Name = "one", After = new[] { "Three", "Two" })]
        public void One()
        {
            Console.WriteLine("One!");
        }

        [Task(Name = "two")]
        public void Two()
        {
            Console.WriteLine("Two!");
        }

        [Task(Name = "three")]
        public void Three()
        {
            Console.WriteLine("Three!");
        }
    }

    [Recipe(Name = "demo")]
    public class DemoRecipe
    {
        [Task(Name = "run")]
        public void Default()
        {
            AppDomain.CurrentDomain.SetData("TEST", "TEST");
        }

        [Task(Name="list", Help = "List all NUnit tests in solution")]
        public void List()
        {
            AppDomain.CurrentDomain.SetData("TEST", "LIST");
        }
        
        [Task(Name="stats", Help="Lists line counts for all types of files")]
        public void Stats()
        {
            string rootDir = Environment.CurrentDirectory;// +"..\\..\\..\\..\\";
            var count = new Dictionary<string, long>() { { "lines", 0 }, { "classes", 0 }, { "files", 0 }, { "enums", 0 }, { "methods", 0 } };
            GetLineCount(rootDir, "*.cs", count);

            
            Console.WriteLine("c# Files:\t\t{0}", count["files"]);
            Console.WriteLine("c# Classes:  \t{0}", count["classes"]);
            Console.WriteLine("c# Methods:\t\t{0}", count["methods"]);
            Console.WriteLine("c# Lines:\t\t{0}", count["lines"]);
            
        }
        

        private static void GetLineCount(string rootDir, string fileFilter, Dictionary<string,long> counts)
        {
            var files = Directory.GetFiles(rootDir, fileFilter, SearchOption.AllDirectories);
            long lineCount = 0;
            foreach(var file in files)
            {
                using(var r = File.OpenText(file))
                {
                    counts["files"] += 1;

                    var line = r.ReadLine();
                    while(line != null)
                    {
                        if (fileFilter == "*.cs" && Regex.Match(line, ".+public|private|internal|protected.+class.+").Length > 0)
                            counts["classes"] += 1;

                        if (fileFilter == "*.cs" && Regex.Match(line, ".+public|private|internal|protected.+enum.+").Length > 0)
                            counts["enums"] += 1;

                        if (fileFilter == "*.cs" && Regex.Match(line, ".+public|private|internal|protected.+\\(.*\\).+").Length > 0)
                            counts["methods"] += 1;

                        counts["lines"] += 1;

                        line = r.ReadLine();
                    }
                }
            }
            
        }
    }

    [Recipe]
    public class Demo3Recipe
    {
        [Task]
        public void Hello()
        {
            Console.WriteLine("Hello");
        }
    }
}