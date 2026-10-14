# Detailed Migration Spec: Arte

**Legacy:** `ct.arte` → **Target:** `Ct.Arte`  
**HTTP actions to implement:** 91  
**Notes:** Artefact metadata, publish, ETL/Github stubs

## Target platform (mandatory)
- **.NET 10**, **C# 14**, nullable enable
- **EF Core 10** + **Npgsql** PostgreSQL only (no RavenDB, no Marten)
- Module project: `Ct.{Name}/` with Domain, Infrastructure (`*DbContext`), Application (`*Service`, `*Cmd`, `*Qry`), Controllers
- Register `Add{Name}Module` + `AddApplicationPart` in `Ct.Host`
- Tenant headers: `X-Owner-Id`, `X-User-Id`, `X-Inst-Id` → always filter by OwnerId
- Nested collections → `jsonb` + ValueComparer
- Dual routes: `api/{module}/...` and legacy `api/{ControllerName}`

## Implementation order for agent
1. Create domain entities + enums from Domain resources section.
2. Create DbContext with tables + jsonb conversions + OwnerId indexes.
3. Implement Application services covering every Cmd + Qry method.
4. Implement Controllers with **exact** action method names and routes from the table.
5. Wire module DI; register in Host.
6. Verify method-name coverage 100% against the endpoint table.

## Endpoints (complete inventory from legacy controllers)

### AnonController.cs
- Route prefix: `api/arte/[controller]`
| HTTP | Route | Method | Parameters |
|---|---|---|---|
| Get | `/getFiles` | `GetFilesFromUrl` | [FromQuery] string url |
| Get | `/contents` | `GetFiles` | [FromQuery] string url |
| Get | `/contentTags` | `GetContentTags` |  |
| Get | `/download` | `Download` | [FromQuery] string id |

### ArtefactController.cs
- Route prefix: `api/arte/[Controller]`
| HTTP | Route | Method | Parameters |
|---|---|---|---|
| Get | `/` | `Get` | [FromQuery] string id |
| Post | `/updateTags` | `UpdateTags` | [FromBody] List<UpdateTagsDto> json |
| Get | `/dataTemplates` | `GetDataTemplates` |  |
| Get | `/dataTemplatesLive` | `GetDataTemplatesLive` | [FromQuery] string id |
| Get | `/dataTemplatesWorking` | `GetDataTemplatesWorking` | [FromQuery] string id |
| Get | `/getDataTemplatesByName` | `GetDataTemplatesByName` | [FromQuery] string name |
| Get | `/getOnQry` | `GetOnQry` | [FromQuery] string qry |
| Get | `/reports` | `GetReports` | [FromQuery] string id |
| Get | `/getTemplate` | `GetTemplate` | [FromQuery] string id |
| Get | `/getModel` | `GetModel` | [FromQuery] string id |
| Get | `/getUrl` | `GetUrl` | [FromQuery] string id |
| Get | `/marksCardTemplate` | `MarksCardTemplate` |  |
| Get | `/sysAdminPolicy` | `SysAdminPolicy` |  |
| Post | `/dataTemplatesSave` | `DataTemplatesSave` | [FromBody] ArtefactDto json |
| Post | `/disable` | `Disable` | [FromQuery] string id |
| Post | `/publishArtefact` | `PublishArte` | [FromQuery] string id, [FromQuery] string topicId |
| Post | `/publishArtefactToPublic` | `PublishArteToPublic` | [FromQuery] string id, [FromQuery] string topicId |
| Post | `/unpublishArtefact` | `UnpublishArte` | [FromQuery] string id |
| Post | `/unpublishPublicArtefact` | `UnpublishPublicArte` | [FromQuery] string id |
| Get | `/download` | `Download` | [FromQuery] string id |
| Get | `/pdfFileContent` | `PDFFileContent` | [FromQuery] string id |
| Get | `/getFileUrl` | `GetFileUrl` | [FromQuery] string path |
| Get | `/downloadWithTimestamp` | `DownloadWithTimestamp` | [FromQuery] string id |
| Post | `/enable` | `Enable` | [FromQuery] string id |
| Post | `/extract` | `Extract` | [FromQuery] string id |
| Post | `/studentAttendanceTemplate` | `StudentAttendanceTemplate` | [FromBody] ArtefactDto json |
| Post | `/template` | `Template` | [FromBody] ArtefactDto json |
| Post | `/update` | `Update` | [FromBody] ArtefactDto json |
| Post | `/updateReport` | `UpdateReport` | [FromBody] ArtefactDto json |
| Post | `/upsert` | `Upsert` | List<IFormFile> files, string acadYear, string path |
| Post | `/uploadAndExtract` | `UploadAndExtract` | [FromBody] ArtefactDto json |
| Post | `/view` | `view` | [FromQuery] string id |
| Post | `/removeFile` | `RemoveFile` | [FromQuery] string FileName |
| Get | `/fileNames` | `GetFileNames` |  |
| Get | `/etlFileNames` | `GetEtlFileNames` | [FromQuery] string path |
| Get | `/file` | `GetFileContent` | [FromQuery] string name |
| Get | `/emailTemplate` | `GetEmailTemplate` |  |
| Get | `/emailTemplateWithoutHF` | `GetEmailTemplateWithoutHF` |  |
| Post | `/viewMarksCard` | `ViewMarksCard` | [FromBody] ViewMarkscardDto json |
| Post | `/publish` | `Publish` | List<IFormFile> files, string acadYear |
| Get | `/generateEtl` | `GenerateEtl` | [FromQuery] string id |
| Post | `/runEtl` | `RunEtl` | [FromQuery] string id |
| Post | `/executeAllEtlsInDir` | `ExecuteEtl` | [FromQuery] string id |
| Get | `/fileContent` | `FileContent` | [FromQuery] string id |
| Get | `/etlFileContent` | `EtlFileContent` | [FromQuery] string path |
| Post | `/applicantPhoto` | `UploadApplicantPhoto` | List<IFormFile> files, string path, List<string> tags = null, string provider = "aws" |
| Post | `/applicantAadhar` | `UploadApplicantAadhar` | List<IFormFile> files, string path, List<string> tags = null, string provider = "aws" |
| Post | `/uploadApplicantDocument` | `UploadApplicantDocument` | List<IFormFile> files, string path, List<string> tags = null, string provider = "aws" |
| Post | `/applicantHSCMarksCard` | `UploadApplicantHSCMarksCard` | List<IFormFile> files, string path, List<string> tags = null, string provider = "aws" |
| Post | `/applicantSSCMarksCard` | `UploadApplicantSSCMarksCard` | List<IFormFile> files, string path, List<string> tags = null, string provider = "aws" |
| Post | `/viewDocument` | `ViewDocument` | [FromBody] ApplicantDto json |
| Post | `/tag` | `CreateTag` | [FromBody] CreateTagDto dto |
| Post | `/contentTag` | `CreateContentTag` | [FromBody] CreateTagDto dto |
| Post | `/comment` | `AddComment` | [FromBody] CommentDto dto |
| Post | `/attribute` | `AddAttribute` | [FromBody] UpdateAttributesDto dto |
| Post | `/videoLink` | `VideoLink` | [FromBody] VideoLinkDto dto |
| Get | `/tags` | `GetTags` |  |
| Get | `/contentTags` | `GetContentTags` |  |
| Post | `/updateTag` | `UpdateTag` | [FromBody] UpdateTagDto dto |
| Post | `/updateContentTag` | `UpdateContentTag` | [FromBody] UpdateTagDto dto |
| Post | `/disableTag` | `DisableTag` | [FromQuery] string id |
| Post | `/enableTag` | `EnableTag` | [FromQuery] string id |
| Post | `/disableContentTag` | `DisableContentTag` | [FromQuery] string id |
| Post | `/enableContentTag` | `EnableContentTag` | [FromQuery] string id |
| Get | `/widget/artefactBandwidth` | `GetArtefactBandwidthWidgetData` |  |
| Get | `/widget/artefactCount` | `GetArtefactCountWidgetData` |  |
| Get | `/widget/sizeBuckets` | `GetWidgetSizeBucketsData` |  |
| Get | `/widget/artefactAuthorCount` | `GetWidgetArtefactAuthorCountData` |  |
| Get | `/widget/fileCount` | `GetWidgetFileCount` |  |
| Post | `/updatePublicUrl` | `UpdatePublicUrl` | [FromBody] UpdatePublicUrlDto dto |
| Post | `/removePublicUrl` | `RemovePublicUrl` | [FromBody] UpdatePublicUrlDto dto |
| Post | `/resetPublicUrl` | `ResetPublicUrl` | [FromBody] ResetPublicUrlDto dto |
| Post | `/upsertStudentDocument` | `UpsertStudentDocument` | List<IFormFile> files, string student_id |

### EtlController.cs
- Route prefix: `api/arte/[controller]`
| HTTP | Route | Method | Parameters |
|---|---|---|---|
| Post | `/generateInstEtl` | `CreateInstituteEtl` | [FromQuery] string path |
| Post | `/generateCourseEtl` | `CreateCourseEtl` | [FromQuery] string path |
| Post | `/generateStudentEtl` | `CreateStudentEtl` | [FromQuery] string path |
| Post | `/generateFeesEtl` | `CreateFeesEtl` | [FromQuery] string path |

### GithubController.cs
- Route prefix: `api/arte/[Controller]`
| HTTP | Route | Method | Parameters |
|---|---|---|---|
| Post | `/upsert` | `Upsert` | List<IFormFile> files, string path, string id = "" |
| Get | `/` | `Get` | [FromQuery] string id |
| Post | `/disable` | `Disable` | [FromQuery] string id |
| Post | `/enable` | `Enable` | [FromQuery] string id |
| Get | `/download` | `Download` | [FromQuery] string id |
| Get | `/getFiles` | `GetFiles` | [FromQuery] string url |
| Post | `/updateDescription` | `UpdateDescription` | [FromBody] DescriptionDto dto |
| Get | `/filesWithContent` | `GetFilesWithContent` | [FromQuery] string url |
| Get | `/specsFile` | `GetSpecsFile` | [FromQuery] string filename |

### ReportController.cs
- Route prefix: `api/arte/[Controller]`
| HTTP | Route | Method | Parameters |
|---|---|---|---|
| Post | `/artefactList` | `GetArtefactsReport` | [FromBody] JObject jObj |

## Command layer (*Cmd) — implement all
### ArtefactCmd.cs
- `Artefact Create(string Id, string Title, string Description, Dictionary<string, string> MetaData, string MimeType, string FileName, double FileSize, string Url, string SHA1, string Model, List<string> Tags)`
- `Artefact CreateEtlArtefact(string Id, string Title, string Description, Dictionary<string, string> MetaData, string MimeType, string FileName, double FileSize, string Url, string SHA1, string Model)`
- `Artefact CreateWithStatusUpload(string Id, string Title, string Description, Dictionary<string, string> MetaData, string MimeType, string FileName, double FileSize, string Url, string SHA1, string Model, List<string> Tags)`
- `Artefact PublishArtefact(string Id, string Title, string Description, Dictionary<string, string> MetaData, string MimeType, string FileName, double FileSize, string Url, string SHA1, string Model)`
- `Artefact Update(string Id, string Title, string Description, Dictionary<string, string> MetaData, List<string> Tags)`
- `Artefact UpdateReport(string Id, string Title, string Description, Dictionary<string, string> MetaData, string Model)`
- `void Disable(string id)`
- `void Publish(string id, string topicId = "")`
- `void PublishToPublic(string id, string topicId = "")`
- `void Unpublish(string id)`
- `void UnpublishPublicArtefact(string id)`
- `string SetFileDownloadName(string id, long timestamp)`
- `void Enable(string id)`
- `void RunEtl(string url)`
- `void ExecuteEtls(string url)`
- `EtlData GenerateInstituteEtl(string file_name, string file_content)`
- `EtlData GenerateCoursesEtl(string file_name, string file_content)`
- `EtlData GenerateStudentsEtl(string file_name, string file_content)`
- `Artefact UpsertArtefact(string id, string title, string description, Dictionary<string, string> metaData, string mimeType, string fileName, double fileSize, string url, string sha1, string model, List<string> tags)`
- `void UpdateArtefactTags(List<UpdateTagsDto> artefacts)`
- `void UpdateDescription(string id, string description)`
- `ArtefactTag CreateTag(string name, bool predefined = false, Dictionary<string, string> meta = null, string instId = "")`
- `ContentTag CreateContentTag(string name, bool predefined = false, Dictionary<string, string> meta = null, string instId = "")`
- `ArtefactTag UpdateTag(string id, string name, bool predefined, Dictionary<string, string> meta)`
- `ContentTag UpdateContentTag(string id, string name, bool predefined, Dictionary<string, string> meta)`
- `void DisableTag(string id)`
- `void EnableTag(string id)`
- `void DisableContentTag(string id)`
- `void EnableContentTag(string id)`
- `void UpdateComment(string id, string by, string text, string html, long itemId)`
- `void UpdateAttribute(string id, List<AttributeDto> attributes)`
- `void UpdateVideoLink(string id, List<VLinkDto> links)`
- `void UpdatePublicUrl(string id, string publicUrl)`
- `void RemovePublicUrl(string id, string publicUrl)`
- `void ResetPublicUrl(string id)`

## Query layer (*Qry) — implement all
### ArtefactQry.cs
- `PagedResultDto<ArteDto> Get(string qual = "all")`
- `Artefact GetById(string id)`
- `IEnumerable<Artefact> GetReports()`
- `IEnumerable<Artefact> GetDataTemplates()`
- `IEnumerable<Artefact> GetByMimeType()`
- `IEnumerable<Artefact> GetByMetaData()`
- `bool FilenameExists(string filename)`
- `PagedResultDto<ArteDto> GetFilesFromUrl(string url, string searchText = "", List<string> tags = null, List<string> statuses = null, string qual = "all")`
- `List<ArteDto> GetAllFiles(string url, string searchText = "")`
- `Artefact GetByFilename(string filename)`
- `List<WidgetDto> GetBandwidthCountData()`
- `List<SizeBucketsDto> GetSizeBucketsData()`
- `List<FileCountDto> GetFileCountData()`

### TemplateQry.cs
- `string Render(string template_string, string model)`
- `static Template GetTemplateInstance(string template_string, string instanceName = "test")`

## Service layer — implement or fold into *Service
### ArtefactService.cs
- `PagedResultDto<ArteDto> GetFilesFromUrl(string url, string searchText = "", List<string> tags = null, List<string> statuses = null, string qual = "all")`
- `PagedResultDto<AnonArteDto> GetFiles(string url, string searchText = "", List<string> statuses = null, string qual = "any")`
- `Artefact GetById(string id)`
- `CreateInstituteDto GetInstitute(string id)`
- `byte[] DownloadFromPath(string url, string shortName)`
- `string GetFileContent(string url)`

### ReportService.cs
- `string GetArtefactsReport(PrintOptions options = null)`
- `List<ArtefactRptDto> GetArtefactsModel()`

## Domain resources (from legacy Resource/Aggregate)
Implement as EF entities. Nested lists/dicts → jsonb unless volume requires tables.
### Artefact.cs
**Enums:**
- `TagStatusEnum`: Unknown=0, Active=1, Disabled=99
- `ArtefactStatusEnum`: Unknown=0, Active=1, Disabled=99, Downloaded=90, Uploaded=80, Published=70, PublishedToPublic=75, Etl=60
- `VideoTypeEnum`: Record=10, Drive=20, Stream=30
**Properties:**
- `string Name`
- `bool Predefined`
- `string CSN`
- `Dictionary<string, string> Meta`
- `TagStatusEnum Status`
- `string Name`
- `bool Predefined`
- `string CSN`
- `Dictionary<string, string> Meta`
- `TagStatusEnum Status`
- `string Key`
- `dynamic Value`
- `long ItemId`
- `List<KeyValueAttr> Attributes`
- `VideoTypeEnum Type`
- `string Url`
- `long ItemId`
- `string By`
- `DateTime TimeStamp`
- `long ItemId`
- `string Text`
- `string Html`
- `string By`
- `DateTime On`
- `string Ref`
- `string Url`
- `string Title`
- `string Description`
- `Dictionary<string, string> MetaData`
- `List<string> Tags`
- `string MimeType`
- `string FileName`
- `double FileSize`
- `ArtefactStatusEnum Status`
- `string SHA1`
- `string Model`
- `string Template`
- `string Csv`
- `List<ChangeRef> ChangeSet`
- `List<Comment> Comments`
- `List<VideoLink> VideoLinks`
- `List<DataAttribute> DataAttributes`
- `DateTime PublishedOn`
- `List<string> PublicUrls`
- `List<string> Thumbnails`

## EF mapping rules for this module
- Primary key: `Guid Id` (generate on create).
- Always set `OwnerId` from user context on insert.
- Soft status enums rather than physical delete when legacy used Disable.
- Report endpoints: return structured JSON; optional simple HTML wrapper.
- External SMS/email: stub or publish to Connect/Rabbit; do not hard-require gateways.

## Acceptance checklist
- [ ] All **91** HTTP actions present (method names match table)
- [ ] All listed Cmd/Qry methods available on services/facades
- [ ] No Raven/Marten package references
- [ ] Host registration complete
- [ ] OwnerId isolation on every query


---

## Business rules (authoritative)

Derived from legacy Command/Handler method bodies. **Implement these constraints in EF services.**


### Cross-cutting Arte rules
- Publish/Unpublish toggle visibility; public/anon endpoints only return published artefacts.
- Tags/attributes replace or merge as per each Cmd (prefer replace when legacy Set*).
- ETL/Github routes may stub with same response shape if external tools absent.


### Per-method signals from legacy code


### Source: `ArtefactCmd.cs`

#### `CreateTag(string name, bool predefined = false, Dictionary<string, string> meta = null, string instId = "")`
**Guards / branches (from legacy):**
- if (string.IsNullOrEmpty(name)
- if (name.Length >= 20)
- if (tagExists)
**Throws:** nameof(name; "Length of tag name exceeds 20 characters"; nameof(name
**Status assignments:** Status = TagStatusEnum.Active
**Legacy body summary (adapt to EF):**
```
 var inst_id = string.IsNullOrEmpty(instId) ? _uc.InstId : instId; if (string.IsNullOrEmpty(name)) throw new ArgumentException(nameof(name)); if (name.Length >= 20) throw new ArgumentException("Length of tag name exceeds 20 characters"); name = name.Trim().ToLower().Replace(" ", "_"); var tagExists = _session.Query<ArtefactTag>() .Where(o => o.Name == name) .Where(o => o.OwnerId == inst_id) .FirstOrDefault() != null; if (tagExists) throw new DuplicateObjectException(nameof(name)); var _tag = new
```

#### `CreateContentTag(string name, bool predefined = false, Dictionary<string, string> meta = null, string instId = "")`
**Guards / branches (from legacy):**
- if (string.IsNullOrEmpty(name)
- if (name.Length >= 20)
- if (tagExists)
**Throws:** nameof(name; "Length of tag name exceeds 20 characters"; nameof(name
**Status assignments:** Status = TagStatusEnum.Active
**Legacy body summary (adapt to EF):**
```
 var inst_id = string.IsNullOrEmpty(instId) ? _uc.InstId : instId; if (string.IsNullOrEmpty(name)) throw new ArgumentException(nameof(name)); if (name.Length >= 20) throw new ArgumentException("Length of tag name exceeds 20 characters"); name = name.Trim().ToLower().Replace(" ", "_"); var tagExists = _session.Query<ContentTag>() .Where(o => o.Name == name) .Where(o => o.OwnerId == inst_id) .FirstOrDefault() != null; if (tagExists) throw new DuplicateObjectException(nameof(name)); var _tag = new 
```

#### `Publish(string id, string topicId = "")`
**Guards / branches (from legacy):**
- if (artefact == null || string.IsNullOrEmpty(artefact.Id)
- if (!string.IsNullOrEmpty(topicId)
**Throws:** nameof(artefact
**Status assignments:** Status = ArtefactStatusEnum.Published
**Side effects / messaging:**
- `EnquePublishToPublicJob(job)`
**Legacy body summary (adapt to EF):**
```
 var artefact = _session.Load<Artefact>(id); if (artefact == null || string.IsNullOrEmpty(artefact.Id)) { throw new ArgumentException(nameof(artefact)); } artefact.Status = ArtefactStatusEnum.Published; artefact.PublishedOn = DateTime.Now; _session.Store(artefact); _session.SaveChanges(); if (!string.IsNullOrEmpty(topicId)) { var payload = new { artefact_id = artefact.Id, topic_id = topicId, topic = _uc.Topic.ToLower() }; var job_id = Guid.NewGuid().ToString(); var job = new Job() { id = job_id,
```

#### `PublishToPublic(string id, string topicId = "")`
**Guards / branches (from legacy):**
- if (artefact == null || string.IsNullOrEmpty(artefact.Id)
- if (!string.IsNullOrEmpty(topicId)
**Throws:** nameof(artefact
**Status assignments:** Status = ArtefactStatusEnum.PublishedToPublic
**Side effects / messaging:**
- `EnquePublishToPublicJob(job)`
**Legacy body summary (adapt to EF):**
```
 var artefact = _session.Load<Artefact>(id); if (artefact == null || string.IsNullOrEmpty(artefact.Id)) { throw new ArgumentException(nameof(artefact)); } artefact.Status = ArtefactStatusEnum.PublishedToPublic; artefact.PublishedOn = DateTime.Now; _session.Store(artefact); _session.SaveChanges(); if (!string.IsNullOrEmpty(topicId)) { var payload = new { artefact_id = artefact.Id, topic_id = topicId, topic = _uc.Topic.ToLower() }; var job_id = Guid.NewGuid().ToString(); var job = new Job() { id =
```

#### `UnpublishPublicArtefact(string id)`
**Guards / branches (from legacy):**
- if (artefact == null || string.IsNullOrEmpty(artefact.Id)
**Throws:** nameof(artefact
**Status assignments:** Status = ArtefactStatusEnum.Active
**Side effects / messaging:**
- `EnquePublishToPublicJob(job)`
**Legacy body summary (adapt to EF):**
```
 var artefact = _session.Load<Artefact>(id); if (artefact == null || string.IsNullOrEmpty(artefact.Id)) { throw new ArgumentException(nameof(artefact)); } artefact.Status = ArtefactStatusEnum.Active; _session.Store(artefact); _session.SaveChanges(); var payload = new { artefact_id = artefact.Id, topic_id = string.Empty, topic = string.Empty }; var job_id = Guid.NewGuid().ToString(); var job = new Job() { id = job_id, title = "unpublish", topic = string.Empty, payload = payload, user_id = "", cli
```

#### `UpdateAttribute(string id, List<AttributeDto> attributes)`
**Guards / branches (from legacy):**
- if (_artefact == null)
- if (arteAttribute != null)
- if (attr != null)
**Throws:** nameof(id
**Legacy body summary (adapt to EF):**
```
 var _artefact = _session.Load<Artefact>(id); if (_artefact == null) throw new ArgumentException(nameof(id)); foreach (var att in attributes) { var arteAttribute = _artefact.DataAttributes.Where(o => o.ItemId == att.ItemId).FirstOrDefault(); if (arteAttribute != null) { var attr = arteAttribute.Attributes.Where(o => o.Key == att.Key).FirstOrDefault(); if (attr != null) attr.Value = att.Value; } else { var attr = new DataAttribute(); attr.ItemId = att.ItemId; var attrDto = new KeyValueAttr(); att
```

#### `Create(string Id, string Title, string Description, Dictionary<string, string> MetaData, string MimeType, string FileName, double FileSize, string Url, string SHA1, string Model, List<string> Tags)`
**Guards / branches (from legacy):**
- if (string.IsNullOrEmpty(_artefact.Id)
- if (MetaData.ContainsKey("meeting_id")
**Status assignments:** Status = ArtefactStatusEnum.Active
**Side effects / messaging:**
- `EnqueUploadNotesJob(job)`
**Legacy body summary (adapt to EF):**
```
 var _artefact = new Artefact() { Id = Id, Title = Title, Description = Description, MetaData = MetaData, MimeType = MimeType, FileName = FileName, FileSize = FileSize, Url = Url, SHA1 = SHA1, Model = Model, Tags = Tags, Status = ArtefactStatusEnum.Active }; if (string.IsNullOrEmpty(_artefact.Id)) _artefact.SetId(); _artefact.SetOwner(_uc.InstId); _session.Store(_artefact); _session.SaveChanges(); if (MetaData.ContainsKey("meeting_id")) { var payload = new { meeting_id = MetaData["meeting_id"] }
```

#### `Update(string Id, string Title, string Description, Dictionary<string, string> MetaData, List<string> Tags)`
**Guards / branches (from legacy):**
- if (_artefact == null || string.IsNullOrEmpty(_artefact.Id)
- if (_uc.Persist)
**Throws:** nameof(_artefact
**Legacy body summary (adapt to EF):**
```
 var _artefact = _session.Load<Artefact>(Id); if (_artefact == null || string.IsNullOrEmpty(_artefact.Id)) { throw new ArgumentException(nameof(_artefact)); } _artefact.Title = Title; _artefact.Description = Description; _artefact.MetaData = MetaData; _artefact.Tags = Tags; //persist if (_uc.Persist) { _session.Store(_artefact); _session.SaveChanges(); } return _artefact; 
```

#### `UpdatePublicUrl(string id, string publicUrl)`
**Guards / branches (from legacy):**
- if (_artefact == null)
- if (!string.IsNullOrEmpty(publicUrl)
**Throws:** nameof(id
**Legacy body summary (adapt to EF):**
```
 var _artefact = _session.Load<Artefact>(id); if (_artefact == null) throw new ArgumentException(nameof(id)); if (!string.IsNullOrEmpty(publicUrl)) _artefact.PublicUrls.Add(publicUrl); _artefact.PublicUrls = _artefact.PublicUrls.Distinct().ToList(); _session.Store(_artefact); _session.SaveChanges(); 
```

#### `RemovePublicUrl(string id, string publicUrl)`
**Guards / branches (from legacy):**
- if (_artefact == null)
- if (!string.IsNullOrEmpty(publicUrl)
**Throws:** nameof(id
**Legacy body summary (adapt to EF):**
```
 var _artefact = _session.Load<Artefact>(id); if (_artefact == null) throw new ArgumentException(nameof(id)); if (!string.IsNullOrEmpty(publicUrl)) _artefact.PublicUrls.Remove(publicUrl); _artefact.PublicUrls = _artefact.PublicUrls.Distinct().ToList(); _session.Store(_artefact); _session.SaveChanges(); 
```

#### `UpdateReport(string Id, string Title, string Description, Dictionary<string, string> MetaData, string Model)`
**Guards / branches (from legacy):**
- if (_artefact == null || string.IsNullOrEmpty(_artefact.Id)
**Throws:** nameof(_artefact
**Legacy body summary (adapt to EF):**
```
 var _artefact = _session.Load<Artefact>(Id); if (_artefact == null || string.IsNullOrEmpty(_artefact.Id)) { throw new ArgumentException(nameof(_artefact)); } _artefact.Title = Title; _artefact.Description = Description; _artefact.MetaData = MetaData; _artefact.Model = Model; _session.Store(_artefact); _session.SaveChanges(); return _artefact; 
```

#### `Disable(string id)`
**Guards / branches (from legacy):**
- if (artefact == null || string.IsNullOrEmpty(artefact.Id)
**Throws:** nameof(artefact
**Status assignments:** Status = ArtefactStatusEnum.Disabled
**Legacy body summary (adapt to EF):**
```
 var artefact = _session.Load<Artefact>(id); if (artefact == null || string.IsNullOrEmpty(artefact.Id)) { throw new ArgumentException(nameof(artefact)); } artefact.Status = ArtefactStatusEnum.Disabled; _session.Store(artefact); _session.SaveChanges(); 
```

#### `Unpublish(string id)`
**Guards / branches (from legacy):**
- if (artefact == null || string.IsNullOrEmpty(artefact.Id)
**Throws:** nameof(artefact
**Status assignments:** Status = ArtefactStatusEnum.Active
**Legacy body summary (adapt to EF):**
```
 var artefact = _session.Load<Artefact>(id); if (artefact == null || string.IsNullOrEmpty(artefact.Id)) { throw new ArgumentException(nameof(artefact)); } artefact.Status = ArtefactStatusEnum.Active; _session.Store(artefact); _session.SaveChanges(); 
```

#### `Enable(string id)`
**Guards / branches (from legacy):**
- if (artefact == null || string.IsNullOrEmpty(artefact.Id)
**Throws:** nameof(artefact
**Status assignments:** Status = ArtefactStatusEnum.Active
**Legacy body summary (adapt to EF):**
```
 var artefact = _session.Load<Artefact>(id); if (artefact == null || string.IsNullOrEmpty(artefact.Id)) { throw new ArgumentException(nameof(artefact)); } artefact.Status = ArtefactStatusEnum.Active; _session.Store(artefact); _session.SaveChanges(); 
```

#### `GenerateInstituteEtl(string file_name, string file_content)`
**Guards / branches (from legacy):**
- if (string.IsNullOrEmpty(line)
**Side effects / messaging:**
- `EnqueEtlScript(job)`
**Legacy body summary (adapt to EF):**
```
 string[] lines = file_content.Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries); var institute = Helper.CsvToList<InstituteDto>(lines); var json_string = JsonConvert.SerializeObject(institute); var json_inst = JsonConvert.DeserializeObject<List<InstituteDto>>(json_string); var etl_inst_lines = _etlService.GetApiScript_CreateInstitute(json_inst); var sb = new StringBuilder(); foreach (var line in etl_inst_lines ?? new List<string>()) { if (string.IsNullOrEmpty(li
```

#### `GenerateCoursesEtl(string file_name, string file_content)`
**Guards / branches (from legacy):**
- if (string.IsNullOrEmpty(line)
**Side effects / messaging:**
- `EnqueEtlScript(job)`
**Legacy body summary (adapt to EF):**
```
 string[] lines = file_content.Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries); var courses = Helper.CsvToList<EtlCourseDto>(lines); var courses_string = JsonConvert.SerializeObject(courses); var course_list = JsonConvert.DeserializeObject<List<EtlCourseDto>>(courses_string); var etl_lines = _etlService.GetApiScript_CreateCourses(course_list); var sb = new StringBuilder(); foreach (var line in etl_lines ?? new List<string>()) { if (string.IsNullOrEmpty(line)) c
```

#### `GenerateStudentsEtl(string file_name, string file_content)`
**Guards / branches (from legacy):**
- if (string.IsNullOrEmpty(line)
**Side effects / messaging:**
- `EnqueEtlScript(job)`
**Legacy body summary (adapt to EF):**
```
 string[] lines = file_content.Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries); var students = Helper.CsvToList<EtlStudentDto>(lines); var students_string = JsonConvert.SerializeObject(students); var student_list = JsonConvert.DeserializeObject<List<EtlStudentDto>>(students_string); var etl_lines = _etlService.GetApiScript_CreateStudents(student_list); var sb = new StringBuilder(); foreach (var line in etl_lines ?? new List<string>()) { if (string.IsNullOrEmpty
```

#### `UpdateArtefactTags(List<UpdateTagsDto> artefacts)`
**Guards / branches (from legacy):**
- if (_artefact == null)
**Throws:** nameof(arte.Id
**Legacy body summary (adapt to EF):**
```
 foreach (var arte in artefacts) { var _artefact = _session.Load<Artefact>(arte.Id); if (_artefact == null) throw new ArgumentException(nameof(arte.Id)); _artefact.Tags = arte.Tags; _session.Store(_artefact); } _session.SaveChanges(); 
```

#### `UpdateDescription(string id, string description)`
**Guards / branches (from legacy):**
- if (artefact == null)
**Throws:** "Artefact not found"
**Legacy body summary (adapt to EF):**
```
 var artefact = _session.Load<Artefact>(id); if (artefact == null) throw new ArgumentNullException("Artefact not found"); artefact.Description = description; _session.Store(artefact); _session.SaveChanges(); 
```

#### `UpdateTag(string id, string name, bool predefined, Dictionary<string, string> meta)`
**Guards / branches (from legacy):**
- if (_tag == null)
**Throws:** nameof(id
**Legacy body summary (adapt to EF):**
```
 var _tag = _session.Load<ArtefactTag>(id); if (_tag == null) throw new ArgumentException(nameof(id)); _tag.Name = name; _tag.Predefined = predefined; _tag.Meta = meta; _session.Store(_tag); _session.SaveChanges(); return _tag; 
```

#### `UpdateContentTag(string id, string name, bool predefined, Dictionary<string, string> meta)`
**Guards / branches (from legacy):**
- if (_tag == null)
**Throws:** nameof(id
**Legacy body summary (adapt to EF):**
```
 var _tag = _session.Load<ContentTag>(id); if (_tag == null) throw new ArgumentException(nameof(id)); _tag.Name = name; _tag.Predefined = predefined; _tag.Meta = meta; _session.Store(_tag); _session.SaveChanges(); return _tag; 
```

#### `DisableTag(string id)`
**Guards / branches (from legacy):**
- if (_tag == null)
**Throws:** nameof(id
**Status assignments:** Status = TagStatusEnum.Disabled
**Legacy body summary (adapt to EF):**
```
 var _tag = _session.Load<ArtefactTag>(id); if (_tag == null) throw new ArgumentException(nameof(id)); _tag.Status = TagStatusEnum.Disabled; _session.Store(_tag); _session.SaveChanges(); 
```

#### `EnableTag(string id)`
**Guards / branches (from legacy):**
- if (_tag == null)
**Throws:** nameof(id
**Status assignments:** Status = TagStatusEnum.Active
**Legacy body summary (adapt to EF):**
```
 var _tag = _session.Load<ArtefactTag>(id); if (_tag == null) throw new ArgumentException(nameof(id)); _tag.Status = TagStatusEnum.Active; _session.Store(_tag); _session.SaveChanges(); 
```

#### `DisableContentTag(string id)`
**Guards / branches (from legacy):**
- if (_tag == null)
**Throws:** nameof(id
**Status assignments:** Status = TagStatusEnum.Disabled
**Legacy body summary (adapt to EF):**
```
 var _tag = _session.Load<ContentTag>(id); if (_tag == null) throw new ArgumentException(nameof(id)); _tag.Status = TagStatusEnum.Disabled; _session.Store(_tag); _session.SaveChanges(); 
```

#### `EnableContentTag(string id)`
**Guards / branches (from legacy):**
- if (_tag == null)
**Throws:** nameof(id
**Status assignments:** Status = TagStatusEnum.Active
**Legacy body summary (adapt to EF):**
```
 var _tag = _session.Load<ContentTag>(id); if (_tag == null) throw new ArgumentException(nameof(id)); _tag.Status = TagStatusEnum.Active; _session.Store(_tag); _session.SaveChanges(); 
```

#### `UpdateComment(string id, string by, string text, string html, long itemId)`
**Guards / branches (from legacy):**
- if (_artefact == null)
**Throws:** nameof(id
**Legacy body summary (adapt to EF):**
```
 var _artefact = _session.Load<Artefact>(id); if (_artefact == null) throw new ArgumentException(nameof(id)); var cm = new Comment(); cm.By = by; cm.ItemId = itemId; cm.Html = html; cm.Text = text; cm.TimeStamp = DateTime.UtcNow; _artefact.Comments.Add(cm); _session.Store(_artefact); _session.SaveChanges(); 
```

#### `UpdateVideoLink(string id, List<VLinkDto> links)`
**Guards / branches (from legacy):**
- if (_artefact == null)
**Throws:** nameof(id
**Legacy body summary (adapt to EF):**
```
 var _artefact = _session.Load<Artefact>(id); if (_artefact == null) throw new ArgumentException(nameof(id)); foreach (var link in links) { var video = new VideoLink(); video.Type = link.Type.Equals("record") ? VideoTypeEnum.Record : link.Type.Equals("drive") ? VideoTypeEnum.Drive : VideoTypeEnum.Stream; video.Url = link.Url; video.ItemId = link.ItemId; _artefact.VideoLinks.Add(video); } _session.Store(_artefact); _session.SaveChanges(); 
```

#### `ResetPublicUrl(string id)`
**Guards / branches (from legacy):**
- if (_artefact == null)
**Throws:** nameof(id
**Legacy body summary (adapt to EF):**
```
 var _artefact = _session.Load<Artefact>(id); if (_artefact == null) throw new ArgumentException(nameof(id)); _artefact.PublicUrls.Clear(); _session.Store(_artefact); _session.SaveChanges(); 
```

#### `RunEtl(string url)`
**Side effects / messaging:**
- `EnqueEtlScript(job)`
**Legacy body summary (adapt to EF):**
```
 var url_arr = url.Split('?'); var arr = url_arr[0].Split('/'); var script_path = $"{arr[3]}/{arr[4]}"; var job = new Job() { title = "etl_run_script", client_id = _uc.InstId, payload = new { s3_path = script_path } }; _rabbitMQService.EnqueEtlScript(job); 
```

#### `ExecuteEtls(string url)`
**Side effects / messaging:**
- `EnqueEtlScript(job)`
**Legacy body summary (adapt to EF):**
```
 var script_path = url.Replace("//", "").Split(new char[] { '/' }, 2)[1]; // var arr = url_arr[0].Split('/'); // var script_path = $"{arr[3]}/{arr[4]}"; var job = new Job() { title = "run_etl_with_dir", client_id = string.Empty, payload = new { s3_jsonl_dir = script_path } }; _rabbitMQService.EnqueEtlScript(job); 
```

#### `UpsertArtefact(string id, string title, string description, Dictionary<string, string> metaData, string mimeType, string fileName, double fileSize, string url, string sha1, string model, List<string> tags)`
**Guards / branches (from legacy):**
- if (string.IsNullOrEmpty(id)
- if (string.IsNullOrEmpty(artefact.Id)
**Status assignments:** Status = ArtefactStatusEnum.Active
**Legacy body summary (adapt to EF):**
```
 var artefact = new Artefact(); if (string.IsNullOrEmpty(id)) { var changeRef = new ChangeRef(); changeRef.By = _uc.UserEmail; changeRef.On = DateTime.Now; artefact = new Artefact() { Id = id, Title = title, Description = description, MetaData = metaData, MimeType = mimeType, FileName = fileName, FileSize = fileSize, Url = url, SHA1 = sha1, Model = model, Tags = tags, Status = ArtefactStatusEnum.Active, ModifiedOn = DateTime.UtcNow }; if (string.IsNullOrEmpty(artefact.Id)) artefact.SetId(); arte
```

#### `CreateEtlArtefact(string Id, string Title, string Description, Dictionary<string, string> MetaData, string MimeType, string FileName, double FileSize, string Url, string SHA1, string Model)`
**Guards / branches (from legacy):**
- if (string.IsNullOrEmpty(_artefact.Id)
**Status assignments:** Status = ArtefactStatusEnum.Etl
**Legacy body summary (adapt to EF):**
```
 var _artefact = new Artefact() { Id = Id, Title = Title, Description = Description, MetaData = MetaData, MimeType = MimeType, FileName = FileName, FileSize = FileSize, Url = Url, SHA1 = SHA1, Model = Model, Status = ArtefactStatusEnum.Etl }; if (string.IsNullOrEmpty(_artefact.Id)) _artefact.SetId(); _artefact.SetOwner(_uc.InstId); _session.Store(_artefact); _session.SaveChanges(); return _artefact; 
```

#### `CreateWithStatusUpload(string Id, string Title, string Description, Dictionary<string, string> MetaData, string MimeType, string FileName, double FileSize, string Url, string SHA1, string Model, List<string> Tags)`
**Guards / branches (from legacy):**
- if (string.IsNullOrEmpty(_artefact.Id)
**Status assignments:** Status = ArtefactStatusEnum.Uploaded
**Legacy body summary (adapt to EF):**
```
 var _artefact = new Artefact() { Id = Id, Title = Title, Description = Description, MetaData = MetaData, MimeType = MimeType, FileName = FileName, FileSize = FileSize, Url = Url, SHA1 = SHA1, Model = Model, Tags = Tags, Status = ArtefactStatusEnum.Uploaded }; if (string.IsNullOrEmpty(_artefact.Id)) _artefact.SetId(); _artefact.SetOwner(_uc.InstId); _session.Store(_artefact); _session.SaveChanges(); return _artefact; 
```

#### `PublishArtefact(string Id, string Title, string Description, Dictionary<string, string> MetaData, string MimeType, string FileName, double FileSize, string Url, string SHA1, string Model)`
**Guards / branches (from legacy):**
- if (string.IsNullOrEmpty(_artefact.Id)
**Status assignments:** Status = ArtefactStatusEnum.Published
**Legacy body summary (adapt to EF):**
```
 var _artefact = new Artefact() { Id = Id, Title = Title, Description = Description, MetaData = MetaData, MimeType = MimeType, FileName = FileName, FileSize = FileSize, Url = Url, SHA1 = SHA1, Model = Model, Status = ArtefactStatusEnum.Published }; if (string.IsNullOrEmpty(_artefact.Id)) _artefact.SetId(); _artefact.SetOwner(_uc.InstId); _session.Store(_artefact); _session.SaveChanges(); return _artefact; 
```


### Agent implementation notes
- Map Raven `IDocumentSession` → `DbContext`; `Store`/`SaveChanges` → `SaveChangesAsync`.
- Map `eventStore.Append` / stream saves → single EF entity update (current state).
- Map `throw new ...Exception` → ASP.NET 400/404 with message.
- Preserve status enum numeric meanings when writing conversions.
