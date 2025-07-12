# Changelog: SimpleRag

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