using System;
using System.IO;
using NUnit.Framework;

namespace database.tools.schema.migration.tests
{
    public class JsonFileTests
    {
        private readonly JsonFile jsonFile;
        private const string SAMPLE_JSON = @"
{
  ""Name"": ""My simple schema migration file"",
  ""Version"": ""Guid"",
  ""ConnectionString"": """",
  ""SchemaStore"": """",
  ""TimeoutAfter"":  ""3"",
  ""InitalFiles"": [
    {
      ""Filename"": ""MyDatabaseToday.sql"",
      ""Options"": {
        ""OutputSuccess"": ""true"",
        ""Params"": {
          ""@key"": ""@value""
        }
      }
    }
  ]
}
";

        public JsonFileTests()
        {
            jsonFile = new JsonFile(SAMPLE_JSON);
        }

        [Test]
        public void EnsureJsonFileIsLoaded()
        {
            Assert.IsNotNull(jsonFile.Configuration);
            Assert.IsNotNull(jsonFile.Configuration.Version);
        }

        [Test]
        public void EnsureFileSectionIsRead()
        {
            Assert.IsNotNull(jsonFile.Section("InitalFiles"));
        }

        [Test]
        public void FileHashWorks()
        {
            Assert.IsNotNull(jsonFile.GetJsonHash());
        }

        [Test]
        public void JsonFileWriteToStream()
        {
            var ms = new MemoryStream();
            jsonFile.Save(ms);
            ms.Seek(0, SeekOrigin.Begin);
            var sw = new StreamReader(ms);
            Assert.IsNotEmpty(sw.ReadToEnd());
        }
    }
}
