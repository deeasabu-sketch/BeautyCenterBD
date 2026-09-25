IF COL_LENGTH('dbo.Users','PasswordSetupTokenHash') IS NULL ALTER TABLE dbo.Users ADD PasswordSetupTokenHash NVARCHAR(128) NULL;
IF COL_LENGTH('dbo.Users','PasswordSetupExpiresAt') IS NULL ALTER TABLE dbo.Users ADD PasswordSetupExpiresAt DATETIME NULL;
IF COL_LENGTH('dbo.Users','PasswordSetupCompletedAt') IS NULL ALTER TABLE dbo.Users ADD PasswordSetupCompletedAt DATETIME NULL;
IF COL_LENGTH('dbo.Users','InvitationSentAt') IS NULL ALTER TABLE dbo.Users ADD InvitationSentAt DATETIME NULL;
IF COL_LENGTH('dbo.Users','InvitationReminderSentAt') IS NULL ALTER TABLE dbo.Users ADD InvitationReminderSentAt DATETIME NULL;
