using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Rakish.Core
{
    public class TaskAttribute : Attribute
    {
        public string Description { get; set; }
    }
}
