using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Rakish.Core
{
    public class TaskAttribute : Attribute
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string[] After{get;set;}
        public TaskAttribute()
        {
            After = new string[0];
        }
    }

    
}
