using BlazorServerWithCassandraDB.Backend.Models;
using Cassandra;
using Cassandra.Data.Linq;
using Cassandra.Mapping;
using System.Linq.Dynamic.Core;

namespace BlazorServerWithCassandraDB.Backend.Database
{
    public class CassandraDatabaseInitializer
    {

        public static async Task Initialize(Cassandra.ISession session)
        {
            MappingConfiguration.Global.Define<CustomMappings>();
            //Create tables if not exists
            session.Execute("CREATE TABLE IF NOT EXISTS users (id UUID PRIMARY KEY, first_name VARCHAR,last_name VARCHAR,email VARCHAR,password VARCHAR,email_verified BOOLEAN);");
            //initializer Users
            var users = new Table<User>(session);
            if (users.Select(user => user).Execute().Count() == 0)
            {
                var batch = users.GetSession().CreateBatch(BatchType.Logged);

                var user = new User()
                {
                    FirstName = "Admin",
                    LastName = "Top",
                    Email = "admin@CassandraStuffs.com",
                    Password = "CassandraStuffs*&dS",
                    EmailVerified = true
                };

                batch.Append(users.Insert(user));
                await batch.ExecuteAsync().ConfigureAwait(false);
            }
        }
    }

    public class CustomMappings : Mappings
    {
        public CustomMappings()
        {
            For<User>()
               .KeyspaceName("my_database")
               .TableName("users")
               .PartitionKey(u => u.Id)
               .Column(u => u.Id, cm => cm.WithName("id"))
               .Column(u => u.FirstName, cm => cm.WithName("first_name"))
               .Column(u => u.LastName, cm => cm.WithName("last_name"))
               .Column(u => u.Email, cm => cm.WithName("email"))
               .Column(u => u.Password, cm => cm.WithName("password"))
               .Column(u => u.EmailVerified, cm => cm.WithName("email_verified"))
               .Column(u => u.CreatedAt, cm => cm.WithName("created_at"))
               .Column(u => u.UpdatedAt, cm => cm.WithName("updated_at"));
        }
    }
}
