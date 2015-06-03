using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using PHPAnalysis.Data;

namespace PHPAnalysis.Tests.Data
{
    [TestFixture]
    public class AstConstantsTests
    {
        [TestCase(typeof(AstConstants.Attributes))]
        [TestCase(typeof(AstConstants.Subnodes))]
        [TestCase(typeof(AstConstants.Scalars))]
        [TestCase(typeof(AstConstants.Nodes))]
        public static void AttributeConstantsHaveSameNameAsValue_CaseIgnored(Type type)
        {
            IEnumerable<FieldInfo> constants = type.GetConstants();

            foreach (var fieldInfo in constants)
            {
                Assert.That(fieldInfo.Name, Is.EqualTo(fieldInfo.GetValue(null)).IgnoreCase);
            }
        }
    }
}
