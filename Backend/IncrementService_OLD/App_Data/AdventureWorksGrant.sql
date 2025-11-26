USE Increment;   

CREATE LOGIN [IIS APPPOOL\IncrementServiceAppPool]
  FROM WINDOWS WITH DEFAULT_DATABASE=Increment, 
  DEFAULT_LANGUAGE=[us_english]
GO
CREATE USER IncrementServiceUser 
  FOR LOGIN [IIS APPPOOL\IncrementServiceAppPool]
GO
EXEC sp_addrolemember 'db_datareader', 'IncrementServiceUser'
GO

GRANT EXECUTE ON OBJECT::dbo.IncrementKey
    TO IncrementServiceUser;  
GO

GRANT INSERT ON dbo.Keys TO IncrementServiceUser
GO
