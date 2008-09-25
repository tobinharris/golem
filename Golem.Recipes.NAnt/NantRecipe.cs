using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Xml;
using Golem.Core;
using NAnt.Core.Tasks;

namespace Golem.Recipes.Nant
{
    /// <summary>
    /// TODO: NAntRecipe is pretty pretty ugly, but proof of concept at least!
    /// </summary>
    [Recipe]
    public class NantRecipe
    {
        [Task(Description="Peek at a property in an XML file.")]
        public void XmlPeek(string file, string xpath, string property)
        {   
            var task = new GXmlPeekTask();
            task.InitializeTaskConfiguration();
            task.XmlFile = new FileInfo(file);
            task.XPath = xpath;
            task.Property = property;

            var m = typeof(XmlPeekTask).GetMethod("ExecuteTask", BindingFlags.NonPublic|BindingFlags.Instance);
            if(m==null)throw new Exception("Doh!");
            m.Invoke(task, null);

            foreach (DictionaryEntry p in task.Properties)
            {
                Console.WriteLine("{0}={1}", p.Key, p.Value);
            }
            
        }
        [Task(Description="Runs a regular expression match.")]
        public void Regex(string input, string pattern)
        {
            var task = new GRegexTask();
            task.InitializeTaskConfiguration();
            task.Input = input;
            task.Pattern = pattern;
            var m = typeof(RegexTask).GetMethod("ExecuteTask", BindingFlags.NonPublic | BindingFlags.Instance);
            if (m == null) throw new Exception("Doh!");
            m.Invoke(task, null);
            Console.WriteLine("Found {0} matches:", task.Properties.Count);
            foreach(DictionaryEntry p in task.Properties)
            {
                Console.WriteLine("{0}={1}",p.Key, p.Value);
            }
        }
    }
}