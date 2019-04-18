IF NOT EXISTS (SELECT name FROM sys.server_principals WHERE name = 'IIS APPPOOL\DefaultAppPool')
BEGIN
    CREATE LOGIN [IIS APPPOOL\DefaultAppPool] 
      FROM WINDOWS WITH DEFAULT_DATABASE=[OnlineExam], 
      DEFAULT_LANGUAGE=[us_english]
END
GO

