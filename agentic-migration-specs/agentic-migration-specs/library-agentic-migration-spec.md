# Detailed Migration Spec: Library

**Legacy:** `ct.library` → **Target:** `Ct.Library`  
**HTTP actions to implement:** 50  
**Notes:** Material/Member/Circulation; remove any Marten

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

### BarcodeController.cs
- Route prefix: `api/library/[controller]`
| HTTP | Route | Method | Parameters |
|---|---|---|---|
| Post | `/generate` | `Generate` | [FromBody]JObject jObj |

### CirculationController.cs
- Route prefix: `api/library/[controller]`
| HTTP | Route | Method | Parameters |
|---|---|---|---|
| Get | `/` | `Get` | [FromQuery] string barcodeValue = "" |
| Post | `/issue` | `Issue` | [FromBody] MaterialIssueDto json |
| Post | `/return` | `Return` | [FromBody] MaterialReturnDto json |
| Post | `/reissue` | `Reissue` | [FromBody] MaterialReissueDto json |
| Get | `/widget/monthwiseCheckouts` | `GetMonthwiseCheckoutsWidgetData` |  |

### MaterialController.cs
- Route prefix: `api/library/[controller]`
| HTTP | Route | Method | Parameters |
|---|---|---|---|
| Get | `/` | `Get` | [FromQuery] string id |
| Get | `/verificationMaterial` | `GetStockVerificationMaterial` | [FromQuery] string barcodeValue = "" |
| Post | `/` | `Post` | [FromBody] MaterialDto json |
| Post | `/disable` | `Disable` | [FromQuery] string id |
| Post | `/maintenance` | `Maintenance` | [FromQuery] string id |
| Post | `/returnMaintenanceMaterial` | `MaintenanceMaterialReturn` | [FromQuery] string id |
| Post | `/enable` | `Enable` | [FromQuery] string id |
| Post | `/updateISBN` | `UpdateISBN` | [FromBody] UpdateISBNDto json |
| Post | `/clone` | `Clone` | [FromBody] MaterialCloneDto json |
| Post | `/relocate` | `Relocate` | [FromBody] MaterialRelocateDto json |
| Post | `/update` | `Update` | [FromBody] MaterialDto json |
| Post | `/setAttributes` | `SetAttributes` | [FromBody] UpdateAttributesDto json |
| Post | `/setTags` | `SetTags` | [FromBody] UpdateTagsDto json |
| Post | `/setValue` | `SetValue` | [FromBody] UpdateValueDto json |
| Post | `/verifyStock` | `VerifyStock` | [FromQuery] string id |
| Post | `/setPages` | `SetPages` | [FromBody] UpdatePagesDto json |
| Post | `/setOwnerId` | `SetOwnerId` | [FromBody] UpdateOwnerIdDto json |
| Get | `/getDtoWithData` | `GetDtoWithData` | [FromQuery] string format = Constants.FORMAT_CSV |
| Get | `/getDto` | `GetDtoWithoutData` |  |
| Get | `/widget/materialBreakup` | `GetMaterialBreakupWidgetData` |  |

### MemberController.cs
- Route prefix: `api/library/[controller]`
| HTTP | Route | Method | Parameters |
|---|---|---|---|
| Get | `/` | `Get` |  |
| Post | `/` | `Post` | [FromBody] MemberDto json |
| Post | `/disable` | `Disable` | [FromQuery]string id |
| Post | `/enable` | `Enable` | [FromQuery]string id |
| Post | `/setAttributes` | `SetAttributes` | [FromBody]UpdateMemberAttributesDto json |
| Post | `/setTags` | `SetTags` | [FromBody]UpdateMemberTagsDto json |
| Post | `/job` | `Job` |  |
| Post | `/upsert` | `Upsert` |  |

### ReportController.cs
- Route prefix: `api/library/[controller]`
| HTTP | Route | Method | Parameters |
|---|---|---|---|
| Post | `/ageAnalysis` | `AgeAnalysisReport` | [FromBody]JObject jObj |
| Post | `/booksWithoutCoverPhoto` | `BooksWithoutCoverPhoto` | [FromBody] JObject jObj |
| Post | `/booksWithoutISBN` | `BooksWithoutISBN` | [FromBody] JObject jObj |
| Post | `/booksWithoutSynopsis` | `BooksWithoutSynopsis` | [FromBody] JObject jObj |
| Post | `/lastMonitored` | `LastMonitored` | [FromBody] JObject jObj |
| Post | `/lateFees` | `LateFeesReport` | [FromBody] JObject jObj |
| Post | `/lostMaterials` | `LostMaterials` | [FromBody] JObject jObj |
| Post | `/materialsDueForMaintenance` | `MaterialsDueForMaintenance` | [FromBody] JObject jObj |
| Post | `/materialsUnderCirculation` | `MaterialsUnderCirculation` | [FromBody] JObject jObj |
| Post | `/materialsUnderMaintenance` | `MaterialsUnderMaintenance` | [FromBody] JObject jObj |
| Post | `/members` | `LibraryMembers` | [FromBody] JObject jObj |
| Post | `/overDue` | `OverDue` | [FromBody] JObject jObj |
| Post | `/referenceMaterials` | `ReferenceMaterials` | [FromBody] JObject jObj |
| Post | `/summary` | `SummaryReport` | [FromBody] JObject jObj |
| Post | `/valuation` | `ValuationReport` | [FromBody] JObject jObj |
| Post | `/unverifiedStock` | `UnverifiedStock` | [FromBody] JObject jObj |

## Command layer (*Cmd) — implement all
_None extracted._

## Query layer (*Qry) — implement all
### CirculationQry.cs
- `dynamic Get(string barcodeValue = "")`
- `dynamic GetMonthwiseCheckoutWidgetData(string instId = "")`

### MaterialQry.cs
- `MaterialView GetStockVerificationMaterial(string barcodeValue = "")`
- `dynamic GetMaterialBreakupWidgetData(string instId = "")`

### MemberQry.cs
- `IEnumerable<MemberView> Get()`

## Service layer — implement or fold into *Service
_None extracted._

## Domain resources (from legacy Resource/Aggregate)
Implement as EF entities. Nested lists/dicts → jsonb unless volume requires tables.
### Material.cs
**Enums:**
- `OwnerStatusEnum`
- `MaterialStatusEnum`: Active=1, Reserved=5, Issued=10, UnderMaintenance=20, OutOfCirculation=90, Disabled=99
**Properties:**
- `string TrackingId`
- `string ISBN`
- `string Title`
- `string Author`
- `string Publisher`
- `string OwnerId`
- `List<Owner> OwnerShip`
- `string Location`
- `Dictionary<string, object> Attributes`
- `List<string> Tags`
- `decimal Value`
- `DateTime TxDate`
- `DateTime DueOn`
- `MaterialStatusEnum Status`
- `long LastVerifiedOn`
- `string IssueTo`
- `int Pages`
- `string OwnerId`
- `DateTime? From`
- `DateTime? To`
- `OwnerStatusEnum Status`
- `string Id`
- `string TrackingId`
- `string ISBN`
- `string Title`
- `string Author`
- `string Publisher`
- `string OwnerId`
- `string Location`
- `MaterialStatusEnum Status`
- `string Id`
- `MaterialStatusEnum Status`
- `string Id`
- `MaterialStatusEnum Status`
- `string Id`
- `MaterialStatusEnum Status`
- `string Id`
- `MaterialStatusEnum Status`
- `string Id`
- `string OwnerId`
- `string TrackingId`
- `string MemberId`
- `string MembershipId`
- `string MemberType`
- `DateTime TxDate`
- `DateTime DueOn`
- `int DueIn`
- `MaterialStatusEnum Status`
- `string Id`
- `string OwnerId`
- `string TrackingId`
- `string MembershipId`
- `DateTime TxDate`
- `MaterialStatusEnum Status`
- `string Id`
- `string OwnerId`
- `string TrackingId`
- `DateTime TxDate`
- `DateTime DueOn`
- `string Id`
- `MaterialStatusEnum Status`
- `string Id`
- `Dictionary<string, object> Attributes`
- `string Id`
- `Dictionary<string, object> Attributes`
- `string Id`
- `List<string> Tags`
- `string Id`
- `List<string> Tags`
- `string Id`
- `string Location`
- `string Id`
- `string OwnerId`
- `string NewTrackingId`
- `string Title`
- `string Author`
- `string Publisher`
- `string Id`
- `decimal Value`
- `string Id`

### Member.cs
**Enums:**
- `MemberStatusEnum`: Active=1, Disabled=99
**Properties:**
- `string Name`
- `string MembershipId`
- `GenderEnum Gender`
- `Dictionary<string, object> Attributes`
- `List<string> Tags`
- `MemberStatusEnum Status`
- `string OwnerId`
- `string Id`
- `string Name`
- `string MembershipId`
- `MemberStatusEnum Status`
- `string OwnerId`
- `string Id`
- `MemberStatusEnum Status`
- `string Id`
- `MemberStatusEnum Status`

## EF mapping rules for this module
- Primary key: `Guid Id` (generate on create).
- Always set `OwnerId` from user context on insert.
- Soft status enums rather than physical delete when legacy used Disable.
- Report endpoints: return structured JSON; optional simple HTML wrapper.
- External SMS/email: stub or publish to Connect/Rabbit; do not hard-require gateways.

## Acceptance checklist
- [ ] All **50** HTTP actions present (method names match table)
- [ ] All listed Cmd/Qry methods available on services/facades
- [ ] No Raven/Marten package references
- [ ] Host registration complete
- [ ] OwnerId isolation on every query


---

## Business rules (authoritative)

Derived from legacy Command/Handler method bodies. **Implement these constraints in EF services.**


### Cross-cutting Library rules
- **Issue:** material must be Available (not Issued/Maintenance/Disabled/Lost); member Active; count of member open issues < MaxIssues; then create Circulation Issued, set material Issued + DueOn + CurrentMemberId.
- **Return:** find open circulation for material; set ReturnedOn; if ReturnedOn.Date > DueOn.Date, LateFee = daysLate * dailyRate (legacy used policy; use 1.0 per day if unspecified); material → Available; clear CurrentMemberId.
- **Reissue:** open circulation required; extend DueOn by N days; increment ReissueCount.
- **TrackingId** unique per OwnerId among non-disabled materials.


### Per-method signals from legacy code


### Source: `CirculationHandler`

#### `Handle(IssueMaterial command)`
**Guards / branches (from legacy):**
- if (!command.TxDate.HasValue)
- if (!command.DueOn.HasValue)
**Legacy body summary (adapt to EF):**
```
 var aggregate = eventStore.GetById<Material>(command.MaterialId); //if (!command.TxDate.HasValue) command.TxDate = DateTime.Now; // 15 Calendar Days are added if the DueOn is not provided.!-- The client has to provide the actual DueOn // if (!command.DueOn.HasValue) command.DueOn = DateTime.Now.AddDays(15); aggregate.Issue(command.TrackingId, command.MembershipId, command.MemberType, command.TxDate.Value, command.DueOn.Value); eventStore.Save(aggregate); 
```

#### `Handle(ReturnMaterial command)`
**Guards / branches (from legacy):**
- if (!command.TxDate.HasValue)
**Legacy body summary (adapt to EF):**
```
 var aggregate = eventStore.GetById<Material>(command.MaterialId); if (!command.TxDate.HasValue) command.TxDate = DateTime.Now; aggregate.Return(command.TrackingId, command.TxDate.Value); eventStore.Save(aggregate); 
```

#### `Handle(ReissueMaterial command)`
**Legacy body summary (adapt to EF):**
```
 var aggregate = eventStore.GetById<Material>(command.MaterialId); aggregate.Reissue(command.TrackingId, command.TxDate.Value, command.DueOn); eventStore.Save(aggregate); 
```


### Source: `MaterialHandler`

#### `Handle(VerifyMaterialStock command)`
**Guards / branches (from legacy):**
- if (command == null || string.IsNullOrEmpty(command.Id)
- if (aggr != null && !string.IsNullOrEmpty(aggr.Id)
**Throws:** 
**Legacy body summary (adapt to EF):**
```
 if (command == null || string.IsNullOrEmpty(command.Id)) throw new ArgumentNullException(); var aggr = eventStore.GetById<Material>(command.Id); if (aggr != null && !string.IsNullOrEmpty(aggr.Id)) { aggr.VerifyStock(); eventStore.Save(aggr); } 
```

#### `Handle(CloneMaterial command)`
**Guards / branches (from legacy):**
- if (command == null || string.IsNullOrEmpty(command.Id)
- if (aggr != null && !string.IsNullOrEmpty(aggr.Id)
**Throws:** 
**Legacy body summary (adapt to EF):**
```
 if (command == null || string.IsNullOrEmpty(command.Id) || string.IsNullOrEmpty(command.CloneId)) throw new ArgumentNullException(); var aggr = eventStore.GetById<Material>(command.Id); if (aggr != null && !string.IsNullOrEmpty(aggr.Id)) { var clone = aggr.Clone(command.CloneId, command.TrackingId); eventStore.Save(clone); } 
```

#### `Handle(CreateMaterial command)`
**Guards / branches (from legacy):**
- if (aggr != null && !string.IsNullOrEmpty(aggr.Id)
**Legacy body summary (adapt to EF):**
```
 var aggr = eventStore.GetById<Material>(command.Id); if (aggr != null && !string.IsNullOrEmpty(aggr.Id)) return; //aggregate exists with the given id. var aggregate = new Material(command.Id, command.TrackingId, command.Title, command.OwnerId, command.Location, command.Author, command.Publisher, command.ISBN); aggregate.SetOwnerId(command.OwnerId); eventStore.Save(aggregate); 
```

#### `Handle(UpdateMaterial command)`
**Legacy body summary (adapt to EF):**
```
 var aggr = eventStore.GetById<Material>(command.Id); aggr.UpdateMaterial(command.NewTrackingId, command.Title, command.Author, command.Publisher); eventStore.Save(aggr); 
```

#### `Handle(EnableMaterial command)`
**Legacy body summary (adapt to EF):**
```
 var aggregate = eventStore.GetById<Material>(command.Id); aggregate.Enable(); eventStore.Save(aggregate); 
```

#### `Handle(DisableMaterial command)`
**Legacy body summary (adapt to EF):**
```
 var aggregate = eventStore.GetById<Material>(command.Id); aggregate.Disable(); eventStore.Save(aggregate); 
```

#### `Handle(MaterialMaintenance command)`
**Legacy body summary (adapt to EF):**
```
 var aggregate = eventStore.GetById<Material>(command.Id); aggregate.Maintenance(); eventStore.Save(aggregate); 
```

#### `Handle(MaintenanceMaterialReturn command)`
**Legacy body summary (adapt to EF):**
```
 var aggregate = eventStore.GetById<Material>(command.Id); aggregate.MaintenanceMaterialReturn(); eventStore.Save(aggregate); 
```

#### `Handle(TakeOutOfCirculation command)`
**Legacy body summary (adapt to EF):**
```
 var aggregate = eventStore.GetById<Material>(command.Id); aggregate.TakeOutOfCirculation(); eventStore.Save(aggregate); 
```

#### `Handle(SetAttributes command)`
**Legacy body summary (adapt to EF):**
```
 var aggregate = eventStore.GetById<Material>(command.Id); aggregate.SetAttributes(command.Attributes); eventStore.Save(aggregate); 
```

#### `Handle(ClearAttributes command)`
**Legacy body summary (adapt to EF):**
```
 var aggregate = eventStore.GetById<Material>(command.Id); aggregate.ClearAttributes(); eventStore.Save(aggregate); 
```

#### `Handle(SetTags command)`
**Legacy body summary (adapt to EF):**
```
 var aggregate = eventStore.GetById<Material>(command.Id); aggregate.SetTags(command.Tags); eventStore.Save(aggregate); 
```

#### `Handle(ClearTags command)`
**Legacy body summary (adapt to EF):**
```
 var aggregate = eventStore.GetById<Material>(command.Id); aggregate.ClearTags(); eventStore.Save(aggregate); 
```

#### `Handle(SetOwnerId command)`
**Legacy body summary (adapt to EF):**
```
 var aggregate = eventStore.GetById<Material>(command.Id); aggregate.SetOwnerId(command.OwnerId); eventStore.Save(aggregate); 
```

#### `Handle(MoveMaterial command)`
**Legacy body summary (adapt to EF):**
```
 var aggregate = eventStore.GetById<Material>(command.Id); aggregate.Move(command.Location); eventStore.Save(aggregate); 
```

#### `Handle(SetValue command)`
**Legacy body summary (adapt to EF):**
```
 var aggregate = eventStore.GetById<Material>(command.Id); aggregate.SetValue(command.Value); eventStore.Save(aggregate); 
```

#### `Handle(SetPages command)`
**Legacy body summary (adapt to EF):**
```
 var aggregate = eventStore.GetById<Material>(command.Id); aggregate.SetPages(command.Pages); eventStore.Save(aggregate); 
```

#### `Handle(UpdateISBN command)`
**Legacy body summary (adapt to EF):**
```
 var aggregate = eventStore.GetById<Material>(command.Id); aggregate.UpdateISBN(command.ISBN); eventStore.Save(aggregate); 
```


### Agent implementation notes
- Map Raven `IDocumentSession` → `DbContext`; `Store`/`SaveChanges` → `SaveChangesAsync`.
- Map `eventStore.Append` / stream saves → single EF entity update (current state).
- Map `throw new ...Exception` → ASP.NET 400/404 with message.
- Preserve status enum numeric meanings when writing conversions.
