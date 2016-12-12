1. Database Setup.

Run the below SQL to create the Database and necessary table. If you decide to change the database name or the table name in this SQL you
must update the App.config with the necessary changes. 

CREATE DATABASE [CSVImportDB];
GO
CREATE TABLE [dbo].[Accounts](
	[Account] [text] NULL,
	[Description] [text] NULL,
	[CurrencyCode] [nchar](3) NULL,
	[Value] [numeric](18, 2) NULL
);
GO

2. Modify the App.config ( CSVImporter.exe.config ) to define the SQL Connection string for your database. 
    This is an optional step depending on whether your database name or connection details are different. 

3. Modify the App.config ( CSVImporter.exe.config ) to define the database table name if different from dbo.Accounts.
    This is an option step depending on whether you chose a different database table name in step 1. 

4. Execute the program and use the Browse button to select the required CSV

5. Press the Import button to begin the import process. 
