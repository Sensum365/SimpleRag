# Changelog: SimpleRag

## Unreleased
- Upgraded all nuget packages
- Add first crude version of a PowerPoint Datasource
- Add datasource for Word documents

---

## Version 0.0.2-preview1 (21st of July 2025)
- Renamed `CSharpContentFormatBuilder` to `ContentFormatBuilder` (the property is on a C# source so no need for the prefix)
- `MarkdownChunk` and `PdfChunk` are now classes instead of records
- Changed default content format of markdown
- Added custom content formatters for Markdown and PDF content
- Added options for C# Datasource to include non-public members + to include member bodies

---

## Version 0.0.1-preview12 (19th of July 2025)
- Breaking Change: Removed .AddSimpleRagWithGitHubIntegration (github credentails are now given directly to DataProvider to make it more simple to understand and allow different credentials to different sources)
- Breaking Change: GitHubRepository is now a record
- Breaking Change: Made the GitHubQuery internal
- Added support for connecting to github with a GitHubApp (instead of a PAT Token)
- Added VectorStoreCommand.SyncAsync for Easier custom implementations
- `FileContent` now operates with bytes instead raw Text to support binary files
- Added first crude version of a PDF Datasource
---

## Version 0.0.1-preview11 (15th of July 2025)
**WARNING: Tons of breaking changes to allow for custom implementations [Sorry for all the constant changes, but things are moving in the right direction now]**
- Everything now use interfaces
- Lots of namespace changes
- `CSharpDataSourceCommand` and `MarkdowSourceCommand` are no no more and instead each Datasource contains it implementation so Customer Datasources and Custom DataProviders are possible
- Added DataManagement Service
---

## Version 0.0.1-preview10 (15th of July 2025)
- **WARNING: Quite a few breaking changes in this version to streamline and prepare for first non-preview**
- - Remove `CSharpDataSourceGitHub`, `CSharpDataSourceLocal` and replace it with `CSharpDataSource` that have a Provider property to define where data source be taken from
- Remove `MarkdownDataSourceGitHub`, `MarkdownDataSourceLocal` and replace it with `MarkdownDataSource` that have a Provider property to define where data source be taken from

---

## Version 0.0.1-preview9 (11th of July 2025)
- Moved LastCommitTimestamp back out on the source itself (wrong to have it in the sub-object)

## Version 0.0.1-preview8 (11th of July 2025)
- **WARNING: Quite a few breaking changes in this version to streamline and prepare for first non-preview**
- Reintroduce this changelog for the previews (People need to know what is changing!)
- Introduced CSharpDatasource (layer between the public and base Datasource)
- GitHub Repo-info on Datasources are now their own object to avoid copy/paste code
- Renamed MarkdownSource to MarkdownDatasource
- Moved `CSharpContentFormatBuilder` from `IngestionOptions` to the individual datasources
- Move `OnProgressNotification` into `IngestionOptions` to ensure future backward compatibilty
- Remove `ProgressNotificationBase` (no more use of events internally)
- Enabled Treat Warnings as Errors
- Added option to provide a citation builder for search-results

---

## Version 0.0.1-preview7 (11th of July 2025)
- Fixed that ingestion did not check collectionId when getting existing causing overwrite if two sources had the same Id
- Added guard that throw exception if the ingested datasource do not have unique collectionId/sourceId combinations
- Tweaked how ignored files where reported back from gitHub

---

## Version 0.0.1-preview6 (10th of July 2025)
- Removed this Changelog
- Added XML Summaries
- Fixed that `AddSimpleRagWithGitHubIntegration` was spelled wrong

---

## Version 0.0.1-preview5 (3rd of July 2025)
- CSharp now include Attributes
- CSharp now include Empty Constructors
- Added CancellationToken support
- Removed public Notification via Eventing (Use onProgressNotification Actions instead)

----

## Version 0.0.1-preview4 (2nd of July 2025)
- CSharp include 'empty' classes
- Fixed mistakes in default CSharpContentBuilder

---

## Version 0.0.1-preview3 (29th of June 2025)
- Fixed NuGet Link in ReadMe
- Removed Interfaces folder (not used)
- Moved Notification system into Models folder
- Added TopLevel Ingestion and Search classes for easier usage

---

## Version 0.0.1-preview2 (28th of June 2025)
- Fixed Changelog path in Nuget Package
- Added initial data in ReadMe (more will come later)

---

## Version 0.0.1-preview1 (28th of June 2025)
- Initial Preview Release (WARINING: Everything is subject to Change untill 1.0.0)

---