# Configuration-file-migrations

An api-server persists its configuration in a xml-file (`<configuration-folder>/Configuration.xml`).
When a property of a persisted configuration-class is renamed, moved or removed, an existing configuration-file still has the old structure: the deserializer does not know the old names, so the value of the property is lost and the property is `null` (or has its default-value) after loading.
Such a problem does not show up while developing with a freshly created configuration-file; it shows up when the new version is started with the configuration-file of an existing installation.

Configuration-file-migrations solve this: they transform an existing configuration-file from its old structure to its new structure **before** it is loaded.

## How it works

- **A migration is always a xslt-transformation.** It gets the whole content of the configuration-file as input and its output becomes the new content of the configuration-file.
- **GRYLibrary brings its own migrations** for the parts of the configuration-file which are defined by GRYLibrary (for example `ServerConfiguration` or `DRequestLoggingConfiguration`). They are embedded xslt-resources of GRYLibrary and are always executed (see `APIServerConfigurationFileMigrations.GetInternalMigrations()`).
- **An application brings its custom migrations** for the parts of the configuration-file which are defined by the application (the properties of its `PersistedApplicationSpecificConfiguration`). They are embedded xslt-resources of the application.
- **Order:** First all internal migrations of GRYLibrary, then all custom migrations of the application. Within each group the migrations are ordered by their resource-name. So a custom migration can rely on the result of all internal migrations.
- **Every migration is executed only once for a configuration-file.** The names of the executed migrations are stored next to the configuration-file in `Configuration.xml.Migrations.xml` (name and timestamp of every executed migration).
- **A new configuration-file is never migrated.** When the configuration-file does not exist, it is created from the initial configuration (which already has the current structure) and all migrations are marked as executed.
- **Backup:** Before the first migration of a start is applied, a copy `Configuration.xml.bak-<yyyyMMddHHmmss>` of the configuration-file is created.
- **Only xml:** Migrations are only supported for xml-configuration-files.

The migrations run during the start of the api-server, after `SetInitializationInformationAction` and before the configuration is loaded.
If a migration fails, the start fails; the configuration-file then still has the state after the last successful migration and the backup contains the state before the first migration.

## How to add a custom migration to an application

### 1. Write the xslt-file

Put the migration as xslt-file into the project of the application, for example into `Resources/ConfigurationFileMigrations`.
The name follows the same scheme as the database-migrations: `Migration000001.xslt`, `Migration000002.xslt`, ...
Describe in a comment in the file what the migration does and why.

Example: the property `WheatherServiceConfiguration` was renamed to `WeatherServiceConfiguration`:

```xml
<?xml version="1.0" encoding="utf-8"?>
<!--
Renames WheatherServiceConfiguration to WeatherServiceConfiguration (typo fixed in v3.0.34).
-->
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
  <xsl:output method="xml" indent="yes" encoding="utf-8"/>

  <!-- copy everything which is not matched by another template -->
  <xsl:template match="@*|node()">
    <xsl:copy>
      <xsl:apply-templates select="@*|node()"/>
    </xsl:copy>
  </xsl:template>

  <xsl:template match="*[local-name()='WheatherServiceConfiguration']">
    <xsl:element name="WeatherServiceConfiguration" namespace="{namespace-uri()}">
      <xsl:apply-templates select="@*|node()"/>
    </xsl:element>
  </xsl:template>
</xsl:stylesheet>
```

### 2. Embed the xslt-file

Add the xslt-files as embedded resources to the project-file of the application.
Use a `LogicalName`, so that the resource-names do not depend on the folder-structure:

```xml
<ItemGroup>
  <EmbeddedResource Include="Resources\ConfigurationFileMigrations\*.xslt" LogicalName="MyApplication.ConfigurationFileMigrations.%(Filename)%(Extension)" />
</ItemGroup>
```

### 3. Register the migrations

Load the migrations in `SetInitializationInformationAction`:

```csharp
apiServerConfiguration.SetInitializationInformationAction = (initializationInformation) =>
{
    initializationInformation.ApplicationConstants.CustomConfigurationFileMigrations = ConfigurationFileMigration.LoadFromEmbeddedResources(typeof(Program).Assembly, "MyApplication.ConfigurationFileMigrations.");
    // ...
};
```

`LoadFromEmbeddedResources` loads all resources of the given assembly whose name starts with the given prefix and ends with `.xslt`, ordered by their name.

## Rules for writing a migration

- **Never change a released migration and never rename it.** The name of a migration is its resource-name without the extension `.xslt` (for example `MyApplication.ConfigurationFileMigrations.Migration000001`). This name is stored in `Configuration.xml.Migrations.xml`; a renamed migration is executed again and a changed migration is not executed again on installations which already executed it. Correct a mistake by adding a new migration.
- **Every migration must be idempotent.** A configuration-file which was created before migrations were supported has no `Configuration.xml.Migrations.xml`, so all migrations are executed on it, including the ones whose change it already has. Applied to a configuration-file which already has the new structure, a migration must not change anything. Renaming only the elements with the old name (as in the example above) is idempotent by itself; adding an element must check first that it does not exist yet (`<xsl:if test="not(...)">`).
- **Compare names with `local-name()`.** The serializer writes namespaces (for example `xmlns="clr-namespace:..."`), so matching `WheatherServiceConfiguration` without namespace matches nothing. Keep the namespace of a renamed element (`namespace="{namespace-uri()}"`).
- **A simple property can be an attribute.** The serializer writes simple values (numbers, strings, booleans) as attributes of the element of their object and complex values as elements. A migration for a simple property has to handle the attribute (`@*[local-name()='...']`), a migration for a complex property the element.
- **Do not add a property only to give it a value.** A property which does not exist in the file gets the default-value of its class when it is loaded. A migration is required when that default-value is not sufficient (for example `null` for a property whose type is an interface or a class without a default-value in the declaration).
- **Test the migration** with a configuration-file which has the old structure: load it with the migrations and check that the loaded configuration has the expected values. Check also that the migration does not change a configuration-file which already has the new structure.

## Internal migrations of GRYLibrary

| Migration         | Change                                                                                                                                                        |
| ----------------- | ------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| `Migration000001` | `DRequestLoggingConfiguration`: `MaximalLengthofRequestBodies` renamed to `MaximalLengthOfRequestBodies`, `LoggedHTTPRequeustHeader` renamed to `LoggedHTTPRequestHeader` (v2.1.7) |

A developer of GRYLibrary who renames, moves or removes a persisted property adds an internal migration to `GRYLibrary/APIServer/Settings/Configuration/ConfigurationFileMigrations` and to this table.
