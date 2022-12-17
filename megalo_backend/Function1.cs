using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using Azure;
using Azure.Core;
using Azure.Data.Tables;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json;
using TableAttribute = Microsoft.Azure.WebJobs.TableAttribute;

namespace megalo_backend
{
    public class Function1
    {
        private readonly ILogger<Function1> _logger;

        public Function1(ILogger<Function1> log)
        {
            _logger = log;
        }

        [FunctionName("Function1")]
        [OpenApiOperation(operationId: "Run", tags: new[] { "name" })]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "code", In = OpenApiSecurityLocationType.Query)]
        [OpenApiParameter(name: "name", In = ParameterLocation.Query, Required = true, Type = typeof(string), Description = "The **Name** parameter")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "text/plain", bodyType: typeof(string), Description = "The OK response")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req)
        {
            _logger.LogInformation("C# HTTP trigger function processed a request.");

            string name = req.Query["name"];

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            dynamic data = JsonConvert.DeserializeObject(requestBody);
            name = name ?? data?.name;

            string responseMessage = string.IsNullOrEmpty(name)
                ? "This HTTP triggered function executed successfully. Pass a name in the query string or in the request body for a personalized response."
                : $"Hello, {name}. This HTTP triggered function executed successfully.";

            return new OkObjectResult(responseMessage);
        }


        [OpenApiOperation(operationId: "Run", tags: new[] { "samples" })]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "code", In = OpenApiSecurityLocationType.Query)]
        [OpenApiRequestBody(contentType: "application/json", bodyType: typeof(string), Description = "The **Json** parameter")]
        //[OpenApiRequestBody(contentType: "application/json", bodyType: typeof(DiaryPayload), Description = "The **Json** parameter")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(string), Description = "The OK response")]
        //[OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(DiaryEntity), Description = "The OK response")]
        [FunctionName("SamplePost")]
        public static async Task<IActionResult> SamplePosttRun(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequest req,
            [Table("sample"/*, Connection = "MyTableService"*/)] TableClient tableClient,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            string payload = requestBody;
            Console.WriteLine(payload);
            Console.WriteLine(payload.ToCharArray());

            //string[] words = payload.DiaryDate.Split('-');
            var sampleEntity = new SampleEntity()
            {
                PartitionKey = "123456789",
                RowKey = "123456789",
                Content = payload 
            };

            await tableClient.AddEntityAsync(sampleEntity);
            return new OkObjectResult(sampleEntity);
        }

        [OpenApiOperation(operationId: "Run", tags: new[] { "samples" })]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "code", In = OpenApiSecurityLocationType.Query)]
        //[OpenApiRequestBody(contentType: "application/json", bodyType: typeof(string), Description = "The **Json** parameter")]
        //[OpenApiRequestBody(contentType: "application/json", bodyType: typeof(DiaryPayload), Description = "The **Json** parameter")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(string), Description = "The OK response")]
        //[OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(DiaryEntity), Description = "The OK response")]
        [FunctionName("SampleGet")]
        public static async Task<IActionResult> SampleGetRun(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = null)] HttpRequest req,
            [Table("sample"/*, Connection = "MyTableService"*/)] TableClient tableClient,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");
            AsyncPageable<SampleEntity> queryResults = tableClient.QueryAsync<SampleEntity>(filter: "PartitionKey eq '123456789'");
            Console.WriteLine(queryResults);
            await foreach (SampleEntity entity in queryResults)
            {
                log.LogInformation($"{entity.PartitionKey}\t{entity.RowKey}\t{entity.Content}\t");
            }

            return new OkObjectResult(queryResults);
        }

        [OpenApiOperation(operationId: "Run", tags: new[] { "task" })]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "code", In = OpenApiSecurityLocationType.Query)]
        [OpenApiRequestBody(contentType: "application/json", bodyType: typeof(TaskPayload), Description = "The **Json** parameter")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(string), Description = "The OK response")]
        [FunctionName("TaskPost")]
        public static async Task<IActionResult> TaskPosttRun(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequest req,
            [Table("tasks", Connection = "MyTableService")] TableClient tableClient,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            TaskPayload payload = TaskPayload.ConvertTo(requestBody);
            Console.WriteLine(payload);

            //string[] words = payload.DiaryDate.Split('-');
            var taskEntity = new TaskEntity()
            {
                PartitionKey = payload.UserId,
                RowKey = payload.Date + "-" + payload.TaskNumber,
                Title = payload.Title,
                Content = payload.Content,
                Date = payload.Date,
                Tag = payload.Tag,
                Place = payload.Place,
                Url = payload.Url,
                TaskNumber = payload.TaskNumber,
            };

            await tableClient.AddEntityAsync(taskEntity);
            return new OkObjectResult(taskEntity);
        }

        [OpenApiOperation(operationId: "Run", tags: new[] { "task" })]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "code", In = OpenApiSecurityLocationType.Query)]
        [OpenApiParameter(name: "userId", In = ParameterLocation.Query, Required = true, Type = typeof(string), Description = "The **userId** parameter")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(string), Description = "The OK response")]
        [FunctionName("TaskGet")]
        public static async Task<IActionResult> TaskGetRun(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = null)] HttpRequest req,
            [Table("tasks", Connection = "MyTableService")] TableClient tableClient,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");
            string key = req.Query["userId"];
            Console.WriteLine(key);
            AsyncPageable<TaskEntity> queryResults = tableClient.QueryAsync<TaskEntity>(filter: $"PartitionKey eq '{key}'");
            Console.WriteLine(queryResults);
            var tasks = new List<TaskEntity>();
            await foreach (TaskEntity entity in queryResults)
            {
                log.LogInformation($"{entity.PartitionKey}\t{entity.RowKey}\t{entity.Title}\t");
                tasks.Add(entity);
            }


            return new OkObjectResult(tasks);
        }

        [OpenApiOperation(operationId: "Run", tags: new[] { "task" })]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "code", In = OpenApiSecurityLocationType.Query)]
        [OpenApiRequestBody(contentType: "application/json", bodyType: typeof(TaskPayload), Description = "The **Json** parameter")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(string), Description = "The OK response")]
        [FunctionName("TaskUpdate")]
        public static async Task<IActionResult> TableClientUpdate(
           [HttpTrigger(AuthorizationLevel.Function, "put", Route = null)] HttpRequest req,
           [Table("tasks", Connection = "MyTableService")] TableClient tableClient,
           ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            TaskPayload payload = TaskPayload.ConvertTo(requestBody);

            //string[] words = payload.DiaryDate.Split('-');
            Console.WriteLine($"{payload.UserId} + - + {payload.Date}");
            TaskEntity entity = tableClient.GetEntity<TaskEntity>(payload.UserId, $"{payload.Date}-{payload.TaskNumber}");

            entity.Title = payload.Title;
            entity.Content = payload.Content;
            entity.Date = payload.Date;
            entity.Tag = payload.Tag;
            entity.Place = payload.Place;
            entity.Url = payload.Url;
            entity.TaskNumber = payload.TaskNumber;

            await tableClient.UpdateEntityAsync(entity, ETag.All, TableUpdateMode.Replace);
            return new OkObjectResult(entity);
        }
    }
}

