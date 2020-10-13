using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Reflection;
using DbUp;
using DbUp.Engine;
using DbUp.Engine.Transactions;
using Newtonsoft.Json.Linq;

namespace database.tools.schema.migration
{
    public sealed class SchemaStore
    {
        private readonly DirectoryInfo _store;

        public SchemaStore(string path)
        {
            _store = Directory.CreateDirectory(path);
        }

        public StreamReader RetrieveSchema(string schemaName)
        {
            var options = new EnumerationOptions();
            options.RecurseSubdirectories = false;
            options.MatchType = MatchType.Simple;
            options.IgnoreInaccessible = true;
            var firstFile = _store.GetFiles(schemaName, options).Single();
            return firstFile.OpenText();
        }
    }

    public class JsonConfigurationReader
    {
        private readonly JObject _json;

        public JsonConfigurationReader(string json)
        {
            _json = JObject.Parse(json);
        }

        public IAmConfiguration CreateConfiguration()
        {
            var result = IAmConfiguration.IAmConfigurationBuilder.BuildFromJson(_json);
            return result;
        }
    }

    public class IAmConfiguration
    {

        public string SchemaLocation { get; private set; }
        public string Name { get; private set; }
        public string FileHash { get; private set; }
        public string Version { get; private set; }
        public string ConnectionString { get; private set; }
        public long TimeoutAfter { get; private set; }

        public class IAmConfigurationBuilder
        {
            public static IAmConfiguration BuildFromJson(JObject json)
            {
                IAmConfiguration configuration = new IAmConfiguration();
                configuration.Name = (string)json["Name"];
                configuration.Version = (string)json["Version"];
                configuration.ConnectionString = (string)json["ConnectionString"];
                configuration.SchemaLocation = (string)json["SchemaStore"];
                configuration.TimeoutAfter = (long)json["TimeoutAfter"];
                return configuration;
            }
        }        
    }

    public class CustomSqlProvider : IScriptProvider
    {
        public IEnumerable<SqlScript> GetScripts(IConnectionManager connectionManager)
        {
            var script = new SqlScript("", "");
            script.SqlScriptOptions.ScriptType = DbUp.Support.ScriptType.RunAlways
            throw new NotImplementedException();
        }
    }

    public class MyCustomSqlScript : IScript
    {
        public string ProvideScript(Func<IDbCommand> dbCommandFactory)
        {
            dbCommandFactory.Invoke();
            throw new NotImplementedException();
        }
    }
}
