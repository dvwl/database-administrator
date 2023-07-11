/* Initialise table with data */
INSERT INTO producttable(idProduct, Name, Price)
VALUES 
(123, 'Hammer', 2.99),
(162, 'Screwdriver', 3.49),
(201, 'Wrench', 4.25);

INSERT INTO lineitemtable(OrderNo, ItemNo, Quantity, ProductId)
VALUES
(1000, 1, 1, 123),
(1000, 2, 2, 201),
(1001, 1, 1, 123);

INSERT INTO ordertable(idOrder, OrderDate, Customer)
VALUES
(1000, "2023-07-11", 1),
(1001, "2023-07-11", 2);

/* DML */
SELECT o.idOrder, o.OrderDate, c.Email, c.Address
FROM ( 
	ordertable AS o
	JOIN customertable AS c
	ON o.Customer = c.idCustomer 
);

/* Creating a view */
CREATE VIEW deliveries
AS
SELECT o.idOrder, o.OrderDate, c.Email, c.Address
FROM ordertable AS o 
JOIN customertable AS c
ON o.Customer = c.idCustomer;

/* Consuming the view */
SELECT idOrder, OrderDate, Address
FROM deliveries;

/* Creating a stored procedure */
DELIMITER //

CREATE PROCEDURE renameproduct (
	IN ProductId INT,
	IN NewName VARCHAR(45)
)
BEGIN
	UPDATE producttable AS p
	SET p.Name = NewName
	WHERE p.idProduct = ProductId;
END; //

DELIMITER ;

/* Consuming the stored procedure */
SELECT * FROM producttable;	/* before */
CALL renameproduct(201, 'Spanner');
SELECT * FROM producttable; /* after */

/* Create an index */
CREATE INDEX idxProductName
ON producttable(Name);
