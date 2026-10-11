BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261011165117_AddPersistentAuthentication'
)
BEGIN
    ALTER TABLE [Users] ADD [AuthenticationVersion] uniqueidentifier NOT NULL DEFAULT '00000000-0000-0000-0000-000000000000';
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261011165117_AddPersistentAuthentication'
)
BEGIN
    ALTER TABLE [Users] ADD [FailedLoginCount] int NOT NULL DEFAULT 0;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261011165117_AddPersistentAuthentication'
)
BEGIN
    ALTER TABLE [Users] ADD [FailureWindowStartedAt] datetime2 NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261011165117_AddPersistentAuthentication'
)
BEGIN
    ALTER TABLE [Users] ADD [LockoutLevel] int NOT NULL DEFAULT 0;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261011165117_AddPersistentAuthentication'
)
BEGIN
    ALTER TABLE [Users] ADD [LockoutUntil] datetime2 NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261011165117_AddPersistentAuthentication'
)
BEGIN
    CREATE TABLE [AuthenticationSessions] (
        [KeyHash] nvarchar(64) NOT NULL,
        [UserId] int NOT NULL,
        [AuthenticationVersion] uniqueidentifier NOT NULL,
        [Role] nvarchar(32) NOT NULL,
        [ProtectedTicket] varbinary(max) NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [LastSeenAt] datetime2 NOT NULL,
        [ExpiresAt] datetime2 NOT NULL,
        CONSTRAINT [PK_AuthenticationSessions] PRIMARY KEY ([KeyHash]),
        CONSTRAINT [FK_AuthenticationSessions_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [Users] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261011165117_AddPersistentAuthentication'
)
BEGIN
    CREATE INDEX [IX_AuthenticationSessions_ExpiresAt] ON [AuthenticationSessions] ([ExpiresAt]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261011165117_AddPersistentAuthentication'
)
BEGIN
    CREATE INDEX [IX_AuthenticationSessions_LastSeenAt] ON [AuthenticationSessions] ([LastSeenAt]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261011165117_AddPersistentAuthentication'
)
BEGIN
    CREATE INDEX [IX_AuthenticationSessions_UserId] ON [AuthenticationSessions] ([UserId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261011165117_AddPersistentAuthentication'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20261011165117_AddPersistentAuthentication', N'8.0.26');
END;
GO

COMMIT;
GO

