MIGRATION_NAME ?= Initial

ef-migration:
	dotnet ef migrations add $(MIGRATION_NAME) \
	--project src/src.csproj \
	--startup-project src/src.csproj \
	--context src.Data.AppDbContext \
	--configuration Debug \
	--output-dir Data/Migrations

ef-update:
	dotnet ef database update \
	--project src/src.csproj \
	--startup-project src/src.csproj \
	--context src.Data.AppDbContext \
	--configuration Debug

ef-script:
	dotnet ef migrations script \
	--project src/src.csproj \
	--startup-project src/src.csproj \
	--context src.Data.AppDbContext \
	--configuration Debug \
	--output migration.sql
