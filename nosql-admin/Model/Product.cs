using System;

namespace NoSQLAdmin.Model
{
	// C# record type for items in the container
	public record Product(
		string id,
		string category,
		string name,
		int quantity,
		bool sale
	);
}