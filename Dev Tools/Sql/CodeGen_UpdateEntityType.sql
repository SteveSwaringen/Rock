-- Find the EntityTypes that are in the current database, but not in a 'clean' database (created but never had a Rock instance run on it)
SELECT CONCAT(
	  'RockMigrationHelper.UpdateEntityType("'
	, [Name]
	, '","'
	,  [FriendlyName]
	, '","'
	, [AssemblyName]
	, '",'
	, case [IsEntity] when 1 then 'true' else 'false' end
	, ','
	, case [IsSecured] when 1 then 'true' else 'false' end
	, ',"'
	, [Guid]
	, '");') [Up]
FROM [dbo].[EntityType]
WHERE [Guid] NOT IN (SELECT [Guid] FROM [EntityTypeBaseline].[dbo].[EntityType])
--and Name like 'Rock.Reporting.%'
ORDER BY NAME

SELECT CONCAT(
	  'RockMigrationHelper.DeleteEntityType("'
	, [Guid]
	, '");') [Down]
FROM [EntityTypeBaseline].[dbo].[EntityType]
WHERE [Guid] NOT IN (SELECT [Guid] FROM [dbo].[EntityType])
--and Name like 'Rock.Reporting.%'
ORDER BY NAME