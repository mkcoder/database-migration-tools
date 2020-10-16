using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using DbUp;
using DbUp.Engine;
using DbUp.ScriptProviders;
using NUnit.Framework;

namespace database.tools.schema.migration.tests
{
    public class Tests
    {
        private readonly JsonFile jsonFile;// = new JsonFile("");
        private readonly SchemaStore store;// = new SchemaStore(jsonFile.CreateConfiguration().SchemaLocation);
        private readonly List<ISchemaProvider> defaultSchemas;// = new InitialSchemaProvider(new SchemaStore(""), new JsonFile(""));

        public Tests()
        {
            var json = @"
{
    

}
";
            jsonFile = new JsonFile(json);
            store = new SchemaStore(jsonFile.Configuration.SchemaLocation);
            defaultSchemas = new List<ISchemaProvider>()
            {
                new InitialSchemaProvider(store, jsonFile),
                new PreSchemaProvider(store, jsonFile),
                new ApplySchemaProvider(store, jsonFile),
                new PostSchemaProvider(store, jsonFile)
            };
        }

        [SetUp]
        public void Setup()
        {
            // get a list of ISchemaProvider
            var upgrader =
                DeployChanges.To
                    .SqlDatabase(jsonFile.Configuration.ConnectionString);
            var max = defaultSchemas.Max(p => p.Order());
            var dictionary = new Dictionary<string, string>();

            defaultSchemas.OrderByDescending(p => p.Order());

            foreach (var item in defaultSchemas)
            {
                var options = new SqlScriptOptions();

                if(item.Order() == (int)Order.Next)
                {
                    options.RunGroupOrder = ++max;
                }
                else
                {
                    options.RunGroupOrder = item.Order();
                }
                
                if (item.RunAlways())
                {
                    options.ScriptType = DbUp.Support.ScriptType.RunAlways;
                }
                else if (item.RunOnce())
                {
                    options.ScriptType = DbUp.Support.ScriptType.RunOnce;
                }

                item.UpdateVariable(dictionary);

                upgrader
                    .WithScripts(new CustomSqlProvider(item, store, options));
            }

            upgrader
                .WithVariablesEnabled()
                .WithVariables(dictionary);
        }

        [Test]
        public void Test1()
        {
            Assert.Pass();
        }
    }
}