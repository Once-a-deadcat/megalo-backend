using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Abstractions;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Resolvers;
using Microsoft.WindowsAzure.Storage.Blob.Protocol;
using Newtonsoft.Json.Serialization;

namespace megalo_backend
{
    [OpenApiExample(typeof(TaskPayloadExample))]
    public class TaskPayload
    {
        [JsonPropertyName("title")]
        public string Title { get; set; }

        [JsonPropertyName("content")]
        public string Content { get; set; }

        [JsonPropertyName("date")]
        public string Date { get; set; }

        [JsonPropertyName("tag")]
        public string Tag { get; set; }

        [JsonPropertyName("place")]
        public string Place { get; set; }

        [JsonPropertyName("url")]
        public string Url { get; set; }
        
        [JsonPropertyName("userId")]
        public string UserId { get; set; }

        [JsonPropertyName("taskNumber")]
        public int TaskNumber { get; set; }

        public static TaskPayload ConvertTo(string value) => JsonSerializer.Deserialize<TaskPayload>(value);

        public override string ToString()
        {
            var options = new JsonSerializerOptions { WriteIndented = false };
            return JsonSerializer.Serialize(this, options);
        }
    }

    public class TaskPayloadExample : OpenApiExample<TaskPayload>
    {
        public override IOpenApiExample<TaskPayload> Build(NamingStrategy namingStrategy = null)
        {
            this.Examples.Add(
                 OpenApiExampleResolver.Resolve(
                     "BookRequestExample",
                     new TaskPayload()
                     {
                         Title = "眠れると思うな",
                         Content = "終わりはない",
                         Date = "2022-12-17",
                         Tag = "hackathon",
                         Place = "ぐーぐるまっぷ",
                         Url = "ゆーあーるえる",
                         UserId = "123456789",
                         TaskNumber = 1,
                     },
                     namingStrategy
                 ));
            return this;
        }
    }
}