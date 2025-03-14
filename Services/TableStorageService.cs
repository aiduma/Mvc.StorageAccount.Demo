using Azure;
using Azure.Data.Tables;
using Mvc.StorageAccount.Demo.Data;

namespace Mvc.StorageAccount.Demo.Services
{
    public class TableStorageService : ITableStorageService
    {
        //private const string TableName = "Attendees";
        private readonly IConfiguration _configuration;
        private readonly TableServiceClient _tableServiceClient;
        private readonly TableClient _tableClient;

        public TableStorageService(IConfiguration configuration, TableServiceClient tableServiceClient, TableClient tableClient)
        {
            _configuration = configuration;
            _tableServiceClient = tableServiceClient;
            _tableClient = tableClient;
        }

        public async Task<AttendeeEntity> GetAttendee(string industry, string id)
        {
            //var tableClient = _tableServiceClient.GetTableClient(TableName);
            return await _tableClient.GetEntityAsync<AttendeeEntity>(industry, id);
        }

        public IEnumerable<AttendeeEntity> GetAttendees()
        {
            //var tableClient = _tableServiceClient.GetTableClient(TableName);
            Pageable<AttendeeEntity> attendeeEntities = _tableClient.Query<AttendeeEntity>();
            return attendeeEntities.ToList();
        }

        public async Task UpsertAttendee(AttendeeEntity attendeeEntity)
        {
            //var tableClient = _tableServiceClient.GetTableClient(TableName);
            await _tableClient.UpsertEntityAsync(attendeeEntity);
        }

        public async Task DeleteAttendee(string industry, string id)
        {
            //var tableClient = _tableServiceClient.GetTableClient(TableName);
            await _tableClient.DeleteEntityAsync(industry, id);
        }

        //private async Task<TableClient> GetTableClient()
        //{
        //    var serviceClient = new TableServiceClient(_configuration["StorageConnectionString"]);
        //    var tableClient = serviceClient.GetTableClient(TableName);
        //    await tableClient.CreateIfNotExistsAsync();

        //    return tableClient;
        //}
    }
}
