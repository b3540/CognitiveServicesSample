﻿using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using System.Linq;

namespace CognitiveServicesSample.Data
{
    public class CategolizedImageRepository : ICategolizedImageRepository
    {
        private const string DatabaseId = "db";
        private const string CategolizedImageCollection = "CategolizedImageCollection";
        private const string CategoriesCollection = "CategoliesCollection";

        private CosmosDbSetting CosmosDbSetting { get; }

        public CategolizedImageRepository(IOptions<CosmosDbSetting> cosmosDbSetting)
        {
            this.CosmosDbSetting = cosmosDbSetting.Value;
        }

        public async Task InsertAsync(CategolizedImage data)
        {
            var client = await this.CreateClientAsync();
            await client.CreateDocumentAsync(
                UriFactory.CreateDocumentCollectionUri(DatabaseId, CategolizedImageCollection),
                data);
            if (!client.CreateDocumentQuery<Category>(
                UriFactory.CreateDocumentCollectionUri(DatabaseId, CategoriesCollection),
                new FeedOptions { MaxItemCount = -1 })
                .Where(x => x.PartitionKey == Category.PartitionKeyValue && x.Name == data.Category)
                .Any())
            {
                await client.CreateDocumentAsync(
                    UriFactory.CreateDocumentCollectionUri(DatabaseId, CategoriesCollection),
                    new Category { Name = data.Category, JaName = data.JaCategory });
            }
        }

        public async Task<IEnumerable<CategolizedImage>> LoadAsync(int skip, int pageSize, string category)
        {
            var client = await this.CreateClientAsync();
            return client.CreateDocumentQuery<CategolizedImage>(
                UriFactory.CreateDocumentCollectionUri(DatabaseId, CategolizedImageCollection),
                new FeedOptions { MaxItemCount = -1 })
                .Where(x => x.Category == category)
                .OrderBy(x => x.TweetId)
                .Skip(skip)
                .Take(pageSize)
                .ToList();
        }

        public async Task<IEnumerable<Category>> GetCategoriesAsync()
        {
            var client = await this.CreateClientAsync();
            return client.CreateDocumentQuery<Category>(
                UriFactory.CreateDocumentCollectionUri(DatabaseId, CategolizedImageCollection),
                new FeedOptions { EnableCrossPartitionQuery = true, MaxItemCount = -1 })
                .ToList();
        }

        private async Task<DocumentClient> CreateClientAsync()
        {
            var client = new DocumentClient(new Uri(this.CosmosDbSetting.EndpointUri), this.CosmosDbSetting.PrimaryKey);
            await client.CreateDatabaseIfNotExistsAsync(new Database { Id = DatabaseId });

            await client.CreateDocumentCollectionIfNotExistsAsync(
                UriFactory.CreateDatabaseUri(DatabaseId),
                new DocumentCollection
                {
                    Id = CategolizedImageCollection,
                    PartitionKey =
                    {
                        Paths =
                        {
                            "/category",
                        },
                    },
                });
            await client.CreateDocumentCollectionIfNotExistsAsync(
                UriFactory.CreateDatabaseUri(DatabaseId),
                new DocumentCollection
                {
                    Id = CategoriesCollection,
                    PartitionKey =
                    {
                        Paths =
                        {
                            "/partitionKey",
                        },
                    },
                });

            return client;
        }

        public async Task<bool> IsExistTweet(long id)
        {
            var client = await this.CreateClientAsync();
            return client.CreateDocumentQuery<CategolizedImage>(
                UriFactory.CreateDocumentCollectionUri(DatabaseId, CategolizedImageCollection),
                new FeedOptions { EnableCrossPartitionQuery = true })
                .Where(x => x.TweetId == id)
                .Any();
        }
    }
}
