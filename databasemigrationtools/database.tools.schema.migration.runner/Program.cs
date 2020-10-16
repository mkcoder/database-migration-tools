using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DbUp;
using DbUp.Builder;
using DbUp.Engine;

namespace database.tools.schema.migration.runner
{
    class Program
    {
        static void Main(string[] args)
        {
            var jsonFile = new StreamReader(args[0]).ReadToEnd();
            SchemaOrchestrator
                .WithJson(jsonFile)
                // .WithSchemaProviders() // if we want to add more provider
                .PrepareRunner()
                .Run();
        }
    }

    public class SchemaOrchestrator
    {
        public class Runner
        {
            private readonly SchemaOrchestrator schemaOrchestrator;
            private readonly Dictionary<string, string> dictionary = new Dictionary<string, string>();

            internal Runner(SchemaOrchestrator schemaOrchestrator)
            {
                this.schemaOrchestrator = schemaOrchestrator;
            }

            public static Runner Prepare(SchemaOrchestrator schemaOrchestrator)
            {
                return new Runner(schemaOrchestrator).Prepare();
            }

            private Runner Prepare()
            {
                var max = schemaOrchestrator.defaultSchemas.Max(p => p.Order());
                schemaOrchestrator.defaultSchemas.OrderByDescending(p => p.Order());

                foreach (var item in schemaOrchestrator.defaultSchemas)
                {
                    var options = new SqlScriptOptions();

                    if (item.Order() == (int)Order.Next)
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

                    schemaOrchestrator.upgrader
                        .WithScripts(new CustomSqlProvider(item, schemaOrchestrator.store, options));
                }

                return this;
            }

            public Runner Run()
            {
                schemaOrchestrator.upgrader
                    .WithVariablesEnabled()
                    .WithVariables(dictionary);

                return this;
            }
        }

        private readonly JsonFile jsonFile;// = new JsonFile("");
        private readonly SchemaStore store;// = new SchemaStore(jsonFile.CreateConfiguration().SchemaLocation);
        private readonly List<ISchemaProvider> defaultSchemas;// = new InitialSchemaProvider(new SchemaStore(""), new JsonFile(""));
        private readonly UpgradeEngineBuilder upgrader;
        private SchemaOrchestrator(string json)
        {
            jsonFile = new JsonFile(json);
            store = new SchemaStore(jsonFile.Configuration.SchemaLocation);
            defaultSchemas = new List<ISchemaProvider>()
            {
                new InitialSchemaProvider(store, jsonFile),
                new PreSchemaProvider(store, jsonFile),
                new ApplySchemaProvider(store, jsonFile),
                new PostSchemaProvider(store, jsonFile)
            };
            upgrader =
                DeployChanges.To
                    .SqlDatabase(jsonFile.Configuration.ConnectionString);
        }

        public static SchemaOrchestrator WithJson(string json)
        {
            return new SchemaOrchestrator(json);
        }

        public SchemaOrchestrator WithSchemaProviders(params ISchemaProvider[] schemaProviders)
        {
            defaultSchemas.AddRange(schemaProviders);
            return this;           
        }

        public Runner PrepareRunner()
        {
            return Runner.Prepare(this);
        }
    }
}
