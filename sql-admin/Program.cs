using System;
using System.Data;
using MySql.Data.MySqlClient;
using Microsoft.Extensions.Configuration;

namespace SQLAdmin
{
	public sealed class Program
	{
		public static void Main(string[] args)
		{
			// Tokens should be considered secret data and never hard-coded.
			// We can read from the environment variable to avoid hard coding.
			// Read about it here: https://www.nuget.org/packages/Microsoft.Extensions.Configuration.Json
			IConfiguration _configuration = new ConfigurationBuilder()
				.AddJsonFile($"config.json", true, true)
				.Build();

			string connstring = _configuration["connection_string"]!;
			if (string.IsNullOrWhiteSpace(connstring) ||
				connstring.Contains("example.com"))
			{
				Console.WriteLine("Configuration not set. Please set the connection string in config.json.");
				return;
			}

			MySqlConnection conn = null;

			try
			{
				conn = new MySqlConnection(connstring);
				conn.Open();

				Console.WriteLine("Database administrator sample");
				Console.WriteLine("Commands available:");
				Console.WriteLine("--------------------------------");
				Console.WriteLine("rc	=> To read customer table");
				Console.WriteLine("rp	=> To read product table");
				Console.WriteLine("ro	=> To read order table");
				Console.WriteLine("ic	=> To insert customer data");
				Console.WriteLine("ip	=> To insert product data");
				Console.WriteLine("up	=> To update product data");
				Console.WriteLine("dp	=> To delete product data");
				Console.WriteLine("c	=> To exit program");
				Console.WriteLine("--------------------------------");
				
				string input = Console.ReadLine();
				string value;

				while (input != "c")
				{
					switch (input)
					{
						case "rc":
							ReadData(conn, "customertable");
							break;

						case "rp":
							ReadData(conn, "producttable");
							break;

						case "ro":
							ReadData(conn, "ordertable");
							break;

						case "ic":
							Console.WriteLine("Inserting new customer data in the this format '('Email', 'Address')'.");
							value = Console.ReadLine();
							InsertData(conn, "customertable(Email, Address)", value);
							Console.WriteLine($"Inserted {value}");
							break;

						case "ip":
							Console.WriteLine("Inserting new product data in the this format '(Id, 'Name', Price)'.");
							value = Console.ReadLine();
							InsertData(conn, "producttable(idProduct, Name, Price)", value);
							Console.WriteLine($"Inserted {value}");
							break;

						case "up":
						{
							Console.WriteLine("Which customer id to update?");
							string id = Console.ReadLine();
							string newEmail, newAddress;
							Console.WriteLine("New email: ");
							newEmail = Console.ReadLine();
							Console.WriteLine("New address: ");
							newAddress = Console.ReadLine();
							UpdateCustomerData(conn, id, newEmail, newAddress);
							Console.WriteLine($"Updated customer with ID: {id}");
						}	break;

						case "dp":
						{
							Console.WriteLine("Which product id to delete?");
							string id = Console.ReadLine();
							DeleteProductData(conn, id);
							Console.WriteLine($"Product deleted.");
						}	break;
					}

					input = Console.ReadLine();
				}
			}
			catch (Exception e)
			{
				Console.WriteLine("Error: {0}", e.ToString());
			}
			finally
			{
				if (conn != null)
				{
					conn.Close();
				}
			}
		}

		private static void ReadData(MySqlConnection conn, string tablename)
		{
			string query = $"SELECT * FROM {tablename};";
			MySqlDataAdapter da = new MySqlDataAdapter(query, conn);
			DataSet ds = new DataSet();
			da.Fill(ds, $"{tablename}");
			DataTable dt = ds.Tables[$"{tablename}"]!;

			foreach (DataRow row in dt.Rows)
			{
				foreach (DataColumn col in dt.Columns)
				{
					Console.Write(row[col] + "\t");
				}

				Console.Write("\n");
			}
		}

		private static void InsertData(MySqlConnection conn, string tablename, string values)
		{
			string query = $"INSERT INTO {tablename} VALUES {values};";
			MySqlCommand cmd = new MySqlCommand(query, conn);
			cmd.ExecuteNonQuery();
		}
		
		private static void UpdateCustomerData(MySqlConnection conn, string id, string newEmail, string newAddress)
		{
			string query = $"UPDATE customertable SET Email = {newEmail}, Address = {newAddress} WHERE idCustomer = {id};";
			MySqlCommand cmd = new MySqlCommand(query, conn);
			cmd.ExecuteNonQuery();
		}

		private static void DeleteProductData(MySqlConnection conn, string id)
		{
			string query = $"DELETE FROM producttable WHERE idProduct = {id};";
			MySqlCommand cmd = new MySqlCommand(query, conn);
			cmd.ExecuteNonQuery();
		}
	}
}