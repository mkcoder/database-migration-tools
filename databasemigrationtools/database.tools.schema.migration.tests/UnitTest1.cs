using System.Collections.Generic;
using System.Reflection;
using DbUp;
using DbUp.Engine;
using DbUp.ScriptProviders;
using NUnit.Framework;

namespace database.tools.schema.migration.tests
{
    public class Tests
    {
        private readonly ISchemaProvider schemaProvider = new InitialSchemaProvider(new SchemaStore(""), new JsonFile(""));

        [SetUp]
        public void Setup()
        {
            // get a list of ISchemaProvider
            var listP = new List<ISchemaProvider>();
            var upgrader =
                DeployChanges.To
                    .SqlDatabase("");
            foreach (var item in listP)
            {
                var options = new SqlScriptOptions()
                {
                    RunGroupOrder = item.Order(),
                };
                if(item.RunAlways())
                {
                    options.ScriptType = DbUp.Support.ScriptType.RunAlways;
                }
                else if (item.RunOnce())
                {
                    options.ScriptType = DbUp.Support.ScriptType.RunOnce;
                }

                upgrader.WithScripts(new CustomSqlProvider(item, defaultOption))
            }
                    
        }

        [Test]
        public void Test1()
        {
            Assert.Pass();
        }
    }
}