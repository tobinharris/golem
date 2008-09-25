using System;
using NAnt.Core;
using NAnt.Core.Tasks;

namespace Golem.Recipes.Nant
{
    public class GRegexTask : RegexTask
    {
        private PropertyDictionary _dict = new PropertyDictionary(null);
        public override void Log(Level messageLevel, string message)
        {
            if (messageLevel == Level.Error)
                Console.WriteLine("NANT:" + message);
        }
        public override PropertyDictionary Properties
        {
            get
            {
                return _dict;
            }
        }
    }
}