using System;
using System.Data.Common;
using System.Data.SqlClient;
using System.Transactions;
using Moq;
using NUnit.Framework;
using Shuttle.Core.Data;
using Shuttle.Core.Transactions;

namespace Shuttle.ContentStore.Tests.Integration.DataAccess
{
    [TestFixture]
    public class DataAccessFixture
    {
        [SetUp]
        public void DataAccessSetUp()
        {
            DbProviderFactories.RegisterFactory("System.Data.SqlClient", SqlClientFactory.Instance);

            TransactionScopeFactory =
                new DefaultTransactionScopeFactory(true, IsolationLevel.ReadCommitted, TimeSpan.FromSeconds(120));
            DatabaseGateway = new DatabaseGateway();
            DatabaseContextCache = new ThreadStaticDatabaseContextCache();
            QueryMapper = new QueryMapper(DatabaseGateway, new DataRowMapper());

            var connectionConfigurationProvider = new Mock<IConnectionConfigurationProvider>();

            connectionConfigurationProvider.Setup(m => m.Get(It.IsAny<string>())).Returns(
                new ConnectionConfiguration(
                    "Access",
                    "System.Data.SqlClient",
                    "Data Source=.;Initial Catalog=ContentStore;user id=sa;password=Pass!000"));

            DatabaseContextFactory = new DatabaseContextFactory(
                connectionConfigurationProvider.Object,
                new DbConnectionFactory(),
                new DbCommandFactory(),
                new ThreadStaticDatabaseContextCache())
                .ConfigureWith("ContentStore");

        }

        protected static string DefaultConnectionStringName = "ContentStore";

        protected ITransactionScopeFactory TransactionScopeFactory { get; private set; }
        protected IDatabaseContextCache DatabaseContextCache { get; private set; }
        protected IDatabaseGateway DatabaseGateway { get; private set; }
        protected IDatabaseContextFactory DatabaseContextFactory { get; private set; }
        protected IQueryMapper QueryMapper { get; private set; }
    }
}