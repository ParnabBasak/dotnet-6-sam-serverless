using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Net;
using System.Net.Http;
using System.Text.Json;

using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace HelloWorld
{
    public class Function
    {
        private readonly AmazonDynamoDBClient client;
        private readonly DynamoDBContext dbContext;
        private static string TABLE_NAME = Environment.GetEnvironmentVariable("TABLE_NAME");
        
        public Function()
        {
            this.client = new AmazonDynamoDBClient();
            this.dbContext = new DynamoDBContext(client);
        }
        
        public async Task<APIGatewayHttpApiV2ProxyResponse?> FunctionHandler(APIGatewayHttpApiV2ProxyRequest request, ILambdaContext context)
        {
            try
            {
                //AmazonDynamoDBClient client = new AmazonDynamoDBClient();
                //DynamoDBContext dbContext = new DynamoDBContext(client);
                LambdaLogger.Log("Table Name: " + TABLE_NAME);
                LambdaLogger.Log("request: " + JsonConvert.SerializeObject(request));
                string idfrompath = request.PathParameters["id"];
                Guid id = new Guid(idfrompath);
                
                LambdaLogger.Log("id: " + id);
                
                var booking = await dbContext.LoadAsync<BookingDto>(id, id);
               
                if (booking == null)
                {
                    LambdaLogger.Log("No booking");
                    
                    var nullresponse = new APIGatewayHttpApiV2ProxyResponse{
                    StatusCode = (int)HttpStatusCode.OK,
                    Body = "No Object found",
                    Headers = new Dictionary<string, string> { { "Content-Type","text/plain" } }};
                    return nullresponse;
                }
                
                var response = new APIGatewayHttpApiV2ProxyResponse{
                    StatusCode = (int)HttpStatusCode.OK,
                    Body = JsonConvert.SerializeObject(booking),
                    Headers = new Dictionary<string, string> { { "Content-Type","text/plain" } }};
            
                return response;
            }
            catch (Exception ex)
            {
                LambdaLogger.Log(ex.Message);
                return new APIGatewayHttpApiV2ProxyResponse
                {
                    Body = "Error Found",
                    StatusCode = (int)HttpStatusCode.InternalServerError,
                };
            }
        }
    }
}