using Golem.Recipes.Nant;
using NUnit.Framework;

namespace Golem.Test
{
    [TestFixture]
    public class NantRecipeFixure
    {
        [Test]
        public void XmlPeek()
        {
            var r = new NantRecipe();
            r.XmlPeek("HelloWorld.xml", "//Item[2]/@name","name");
           
        }

        [Test]
        public void Regex()
        {
            var r = new NantRecipe();
            r.Regex("Hello World",".+(Wor).+");
        }
    }
}