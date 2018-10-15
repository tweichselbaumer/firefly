using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace FireFly.Data.Storage
{
    public static class YamlTranslator
    {
        public static string ConvertToYaml<T>(T obj)
        {
            ISerializer serializer = new SerializerBuilder().WithNamingConvention(new CamelCaseNamingConvention()).Build();
            return serializer.Serialize(obj);
        }

        public static T ConvertFromYaml<T>(string content)
        {
            StringReader input = new StringReader(content);

            var deserializer = new DeserializerBuilder()
                .WithNamingConvention(new CamelCaseNamingConvention())
                .Build();

            return deserializer.Deserialize<T>(input);
        }
    }
}
