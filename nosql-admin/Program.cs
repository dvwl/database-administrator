using System;
using System.Data;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Configuration;
using NoSQLAdmin.Model;

namespace NoSQLAdmin
{
	public sealed class Program
	{
		private static Database database;
		private readonly IConfiguration _configuration;

		public static void Main(string[] args)
			=> new Program().MainAsync().GetAwaiter().GetResult();

		public Program()
		{
			// Tokens should be considered secret data and never hard-coded.
			// We can read from the environment variable to avoid hard coding.
			// Read about it here: https://www.nuget.org/packages/Microsoft.Extensions.Configuration.Json
			_configuration = new ConfigurationBuilder()
				.AddJsonFile($"config.json", true, true)
				.Build();
		}
		
		public async Task MainAsync()
		{	
			string connstring = _configuration["primary_connection_string"]!;
			if (string.IsNullOrWhiteSpace(connstring) ||
				connstring.Contains("<"))
			{
				Console.WriteLine("Configuration not set. Please set the connection string in config.json.");
				return;
			}

			try
			{
				// New instance of CosmosClient class using an endpoint and key string
				using CosmosClient client = new
				(
					accountEndpoint: _configuration["uri"],
					authKeyOrResourceToken: _configuration["primary_key"]
				);

				Console.WriteLine("NoSQL using C#");
				Console.WriteLine("Commands available:");
				Console.WriteLine("-------------------------------------------");
				Console.WriteLine("n	=> To create a new product");
				Console.WriteLine("r <id>	=> To read a product by id");
				Console.WriteLine("u <id>	=> To update a product name by id");
				Console.WriteLine("d <id>	=> To delete a product by id");
				Console.WriteLine("c	=> To exit program");
				Console.WriteLine("-------------------------------------------");

				await CreateNoSQLDatabaseAsync(client);

				string input = Console.ReadLine();

				while (input != "c")
				{
					if (input.StartsWith("n"))
					{
						Product newItem = new(
							id: "389",
							category: "gear-surf-surfboards",
							name: "Yamba Surfboard",
							quantity: 10,
							sale: true
						);

						await CreateOrReplaceItemAsync(newItem);
					}
					else if (input.StartsWith("r"))
					{
						string[] substring = input.Split(' ');
						string id = substring[1];

						await ReadItemByIdAsync(id);
					}
					else if (input.StartsWith("u"))
					{
						string[] substring = input.Split(' ');
						string id = substring[1];
						
						Console.WriteLine("Enter new product name:");
						string name = Console.ReadLine();
						
						Console.WriteLine("Enter new product quantity:");
						string quantity = Console.ReadLine();
						
						Product item = new(
							id: $"{id}",
							category: "gear-surf-surfboards",
							name: $"{name}",
							quantity: int.Parse(quantity),
							sale: true
						);
						
						await CreateOrReplaceItemAsync(item);
						Console.WriteLine($"{id} updated!");
					}
					else if (input.StartsWith("d"))
					{
						string[] substring = input.Split(' ');
						string id = substring[1];

						await ReadItemByIdAsync(id);
					}

					input = Console.ReadLine();
				}

			}
			catch (Exception e)
			{
				Console.WriteLine("Error: {0}", e.ToString());
			}
		}

		private async Task CreateNoSQLDatabaseAsync(CosmosClient client)
		{
			// New instance of Database class referencing the server-side database
			database = await client.CreateDatabaseIfNotExistsAsync(
				id: "adventureworks-1"
			);

			// New instance of Container class referencing the server-side container
			Container container = await database.CreateContainerIfNotExistsAsync(
				id: "products-1",
				partitionKeyPath: "/category",
				throughput: 400
			);

			// Create new item and add to container
			Product item = new(
				id: "388",
				category: "gear-surf-surfboards",
				name: "Sunnox Surfboard",
				quantity: 8,
				sale: true
			);

			// Create or replace an item
			Product createdItem = await container.UpsertItemAsync<Product>(
				item: item,
				partitionKey: new PartitionKey(item.category)
			);
		}

		private async Task CreateOrReplaceItemAsync(Product product)
		{
			Container container = database.GetContainer("products-1");

			// Create or replace an item
			Product upsertedItem = await container.UpsertItemAsync<Product>(
				item: product,
				partitionKey: new PartitionKey(product.category)
			);
		}

		private async Task ReadItemByIdAsync(string id)
		{
			Container container = database.GetContainer("products-1");

			// Read an item
			Product readItem = await container.ReadItemAsync<Product>(
				id: id,
				partitionKey: new PartitionKey("gear-surf-surfboards")
			);

			// normally, we'll return the Product
			// however, in this demo, we'll just print it to console
			Console.WriteLine($"Id = {readItem.id}, Category: {readItem.category}, Name = {readItem.name}, Quantity = {readItem.quantity}, Sale = {readItem.sale}");
		}

		private async Task DeleteItemByIdAsync(string id)
		{
			Container container = database.GetContainer("products-1");

			// Delete an item
			Product deleteItem = await container.DeleteItemAsync<Product>(
				id: id,
				partitionKey: new PartitionKey("gear-surf-surfboards")
			);

			Console.WriteLine($"Deleted.");
		}

		// Other examples includes Querying items using LINQ asynchronously
	}
}