BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261011182257_AddPhoneAuthentication'
)
BEGIN
    ALTER TABLE [Users] ADD [PhoneVerifiedAt] datetime2 NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261011182257_AddPhoneAuthentication'
)
BEGIN
    CREATE TABLE [PhoneChallenges] (
        [PhoneNumber] nvarchar(11) NOT NULL,
        [ChallengeId] uniqueidentifier NOT NULL,
        [Purpose] nvarchar(16) NOT NULL,
        [CodeHash] nvarchar(64) NOT NULL,
        [Salt] nvarchar(32) NOT NULL,
        [UserId] int NULL,
        [AuthenticationVersion] uniqueidentifier NOT NULL,
        [ExpiresAt] datetime2 NOT NULL,
        [LastSentAt] datetime2 NOT NULL,
        [WindowStartedAt] datetime2 NOT NULL,
        [SendCount] int NOT NULL,
        [AttemptCount] int NOT NULL,
        [Consumed] bit NOT NULL,
        CONSTRAINT [PK_PhoneChallenges] PRIMARY KEY ([PhoneNumber])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261011182257_AddPhoneAuthentication'
)
BEGIN
    CREATE INDEX [IX_PhoneChallenges_WindowStartedAt] ON [PhoneChallenges] ([WindowStartedAt]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261011182257_AddPhoneAuthentication'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20261011182257_AddPhoneAuthentication', N'8.0.26');
END;
GO

COMMIT;
GO

