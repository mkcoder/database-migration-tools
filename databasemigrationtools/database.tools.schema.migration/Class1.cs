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

    public class JsonFile
    {
        private readonly JObject _json;
        private readonly IAmConfiguration result;

        public JsonFile(string json)
        {
            _json = JObject.Parse(json);
            result = IAmConfiguration.IAmConfigurationBuilder.BuildFromJson(_json);
        }

        public IAmConfiguration Configuration { get => result; }

        public JToken Section(string query)
        {
            return _json[query];
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

    public enum Order
    {
        Next = -1,
        Any = -2,
    }

    public class Script
    {
        public string FilePath { get; set; }
        public string Content {
            get
            {
                using var fs = new StreamReader(FilePath);
                return fs.ReadToEnd();
            }
        }

        public static Script CreateFromJObject(JObject obj, string root)
        {
            var script = new Script();
            script.FilePath = $"{root}\\{((string)obj["Filename"])}";
            return script;
        }
    }

    public interface ISchemaProvider
    {
        List<Script> GetScripts();
        int Order();
        bool RunAlways();
        bool RunOnce();
        void GetVariables(Dictionary<string, string> keyValuePairs);
    }

    public abstract class BaseSchemaProvider : ISchemaProvider
    {
        protected readonly SchemaStore store;
        protected readonly JsonFile file;

        public BaseSchemaProvider(SchemaStore store, JsonFile file)
        {
            this.store = store;
            this.file = file;
        }

        public abstract int Order();
        public abstract bool RunAlways();
        public abstract bool RunOnce();
        public abstract JToken GetSection();

        public virtual List<Script> GetScripts()
        {
            var result = new List<Script>();
            var section = GetSection();
            foreach (JObject configuration in section)
            {
                result.Add(Script.CreateFromJObject(configuration, file.Configuration.SchemaLocation));
            }
            return result;
        }

        public virtual void GetVariables(Dictionary<string, string> keyValuePairs)
        {
        }
    }

    public class InitialSchemaProvider : BaseSchemaProvider
    {
        public InitialSchemaProvider(SchemaStore store, JsonFile file) : base(store, file)
        {
        }

        public override int Order() => 1;

        public override bool RunAlways() => false;

        public override bool RunOnce() => true;

        public override JToken GetSection() => file.Section("Inital");
    }

    public class PreSchemaProvider : BaseSchemaProvider
    {
        public PreSchemaProvider(SchemaStore store, JsonFile file) : base(store, file)
        {
        }

        public override int Order() => 2;

        public override bool RunAlways() => true;

        public override bool RunOnce() => false;

        public override JToken GetSection() => file.Section("Pre");

    }

    public class ApplySchemaProvider : BaseSchemaProvider
    {
        public ApplySchemaProvider(SchemaStore store, JsonFile file) : base(store, file)
        {
        }

        public override int Order() => 3;

        public override bool RunAlways() => false;

        public override bool RunOnce() => false;

        public override JToken GetSection() => file.Section("Apply");
    }

    public class PostSchemaProvider : BaseSchemaProvider
    {
        public PostSchemaProvider(SchemaStore store, JsonFile file) : base(store, file)
        {
        }

        public override int Order() => 4;

        public override bool RunAlways() => true;

        public override bool RunOnce() => false;

        public override JToken GetSection() => file.Section("Post");
    }

    public class CustomSqlProvider : IScriptProvider
    {
        private readonly ISchemaProvider schemas;
        private readonly SqlScriptOptions options;

        public CustomSqlProvider(ISchemaProvider schemas, SqlScriptOptions options)
        {
            this.schemas = schemas;
            this.options = options;
        }

        public IEnumerable<SqlScript> GetScripts(IConnectionManager connectionManager)
        {
            var myScripts = new List<MyCustomSqlScript>();
            foreach (var item in schemas.GetScripts())
            {
                myScripts.Add(new MyCustomSqlScript(item.FilePath, item.Content, options));
            }
            return myScripts;
        }
    }

    public class MyCustomSqlScript : SqlScript
    {
        public MyCustomSqlScript(string name, string contents) : base(name, contents)
        {
        }

        public MyCustomSqlScript(string name, string contents, SqlScriptOptions sqlScriptOptions) : base(name, contents, sqlScriptOptions)
        {
        }
    }
}
