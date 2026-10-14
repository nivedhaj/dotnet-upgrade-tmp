# Detailed Migration Spec: Asset

**Legacy:** `ct.asset` → **Target:** `Ct.Asset`  
**HTTP actions to implement:** 14  
**Notes:** Event-sourced legacy → current-state EF; unique TrackingId

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

### AssetController.cs
- Route prefix: `api/[controller]`
| HTTP | Route | Method | Parameters |
|---|---|---|---|
| Post | `/` | `Post` | [FromBody] AssetDto json |
| Get | `/` | `Get` | [FromQuery] string id |
| Post | `/disable` | `Disable` | [FromQuery] string id |
| Post | `/enable` | `Enable` | [FromQuery] string id |
| Post | `/setAttributes` | `SetAttributes` | [FromBody] UpdateAttributesDto json |
| Post | `/setTags` | `SetTags` | [FromBody] UpdateTagsDto json |
| Post | `/setValue` | `SetValue` | [FromBody] UpdateValueDto json |
| Post | `/relocate` | `Relocate` | [FromBody] AssetRelocateDto json |
| Post | `/updateTrackingId` | `UpdateTrackingId` | [FromBody] UpdateTrackingIdDto json |
| Post | `/maintain` | `Maintain` | [FromBody] AssetMaintainDto json |
| Post | `/setWarranty` | `SetWarranty` | [FromBody] AssetWarrantyDto json |
| Post | `/clone` | `Clone` | [FromBody] AssetCloneDto json |
| Post | `/` | `ImportArtefacts` | [FromBody]JObject jObj |

### ReportController.cs
- Route prefix: `api/asset/[controller]`
| HTTP | Route | Method | Parameters |
|---|---|---|---|
| Post | `/locationWiseAssets` | `GetLocationwiseAssetsReport` | [FromBody]JObject jObj |

## Command layer (*Cmd) — implement all
_None extracted._

## Query layer (*Qry) — implement all
### AssetQry.cs
- `AssetView GetAssetById(string id)`
- `bool IsDuplicateTrackingId(string trackingId)`

## Service layer — implement or fold into *Service
### ReportService.cs
- `string GetLocationwiseAssets(PrintOptions options = null, bool testData = false)`
- `string GetLocationwiseAssetsData(bool testData = false)`

## Domain resources (from legacy Resource/Aggregate)
Implement as EF entities. Nested lists/dicts → jsonb unless volume requires tables.
### Aggregate.cs
**Enums:**
- `AssetStatusEnum`: Active=1, Cleared=90, Disabled=99
**Properties:**
- `string Id`
- `string TrackingId`
- `string OwnerId`
- `string Location`
- `AssetStatusEnum Status`
- `string Id`
- `AssetStatusEnum Status`
- `string Id`
- `AssetStatusEnum Status`
- `string Id`
- `AssetStatusEnum Status`
- `string Id`
- `Dictionary<string, object> Attributes`
- `string Id`
- `Dictionary<string, object> Attributes`
- `string Id`
- `List<string> Tags`
- `string Id`
- `List<string> Tags`
- `string Id`
- `string OwnerId`
- `string NewTrackingId`
- `string Id`
- `string Location`
- `string Id`
- `decimal Value`
- `string Id`
- `string CloneId`
- `string TrackingId`
- `string Id`
- `Warranty Warranty`
- `string Id`
- `Maintenance Maintenance`
- `string TrackingId`
- `string OwnerId`
- `string Location`
- `Dictionary<string, object> Attributes`
- `List<string> Tags`
- `decimal Value`
- `Maintenance LastMaintenance`
- `Warranty CurrentWarranty`
- `AssetStatusEnum Status`
- `string By`
- `DateTime On`
- `string ActionTaken`
- `string CoveredBy`
- `DateTime From`
- `DateTime To`
- `string Details`

## EF mapping rules for this module
- Primary key: `Guid Id` (generate on create).
- Always set `OwnerId` from user context on insert.
- Soft status enums rather than physical delete when legacy used Disable.
- Report endpoints: return structured JSON; optional simple HTML wrapper.
- External SMS/email: stub or publish to Connect/Rabbit; do not hard-require gateways.

## Acceptance checklist
- [ ] All **14** HTTP actions present (method names match table)
- [ ] All listed Cmd/Qry methods available on services/facades
- [ ] No Raven/Marten package references
- [ ] Host registration complete
- [ ] OwnerId isolation on every query


---

## Business rules (authoritative)

Derived from legacy Command/Handler method bodies. **Implement these constraints in EF services.**


### Cross-cutting Asset rules
- **TrackingId** unique per OwnerId (case-insensitive check in legacy Qry).
- **Clone:** for each requested TrackingId, create new asset copying metadata; new Id; fail per-id on duplicate tracking.
- **Clear** sets Status=Cleared; **Disable** sets Disabled; **Enable** sets Active.
- **Maintain:** append maintenance record; set LastMaintenance; typically return to Active.
- No event-sourcing replay required — persist current state only.


### Per-method signals from legacy code


### Source: `AssetHandler`

#### `Handle(CloneAsset command)`
**Guards / branches (from legacy):**
- if (command == null || string.IsNullOrEmpty(command.Id)
- if (aggr != null && !string.IsNullOrEmpty(aggr.Id)
**Throws:** 
**Legacy body summary (adapt to EF):**
```
 if (command == null || string.IsNullOrEmpty(command.Id) || string.IsNullOrEmpty(command.CloneId)) throw new ArgumentNullException(); var aggr = eventStore.GetById<Asset>(command.Id); if (aggr != null && !string.IsNullOrEmpty(aggr.Id)) { var clone = aggr.Clone(command.CloneId, command.TrackingId); eventStore.Save(clone); } 
```

#### `Handle(CreateAsset command)`
**Guards / branches (from legacy):**
- if (asset != null && !string.IsNullOrEmpty(asset.Id)
**Legacy body summary (adapt to EF):**
```
 var asset = eventStore.GetById<Asset>(command.Id); if (asset != null && !string.IsNullOrEmpty(asset.Id)) return; //aggregate exists with the given id. var aggregate = new Asset(command.Id, command.TrackingId, command.OwnerId, command.Location); eventStore.Save(aggregate); 
```

#### `Handle(UpdateTrackingIdAsset command)`
**Legacy body summary (adapt to EF):**
```
 var aggr = eventStore.GetById<Asset>(command.Id); aggr.UpdateTrackingId(command.NewTrackingId); eventStore.Save(aggr); 
```

#### `Handle(EnableAsset command)`
**Legacy body summary (adapt to EF):**
```
 var aggr = eventStore.GetById<Asset>(command.Id); aggr.Enable(); eventStore.Save(aggr); 
```

#### `Handle(DisableAsset command)`
**Legacy body summary (adapt to EF):**
```
 var aggr = eventStore.GetById<Asset>(command.Id); aggr.Disable(); eventStore.Save(aggr); 
```

#### `Handle(ClearAsset command)`
**Legacy body summary (adapt to EF):**
```
 var aggr = eventStore.GetById<Asset>(command.Id); aggr.Clear(); eventStore.Save(aggr); 
```

#### `Handle(SetAssetAttributes command)`
**Legacy body summary (adapt to EF):**
```
 var aggr = eventStore.GetById<Asset>(command.Id); aggr.SetAttributes(command.Attributes); eventStore.Save(aggr); 
```

#### `Handle(ClearAssetAttributes command)`
**Legacy body summary (adapt to EF):**
```
 var aggr = eventStore.GetById<Asset>(command.Id); aggr.ClearAttributes(); eventStore.Save(aggr); 
```

#### `Handle(ClearAssetTags command)`
**Legacy body summary (adapt to EF):**
```
 var aggr = eventStore.GetById<Asset>(command.Id); aggr.ClearTags(); eventStore.Save(aggr); 
```

#### `Handle(MoveAsset command)`
**Legacy body summary (adapt to EF):**
```
 var aggr = eventStore.GetById<Asset>(command.Id); aggr.Move(command.Location); eventStore.Save(aggr); 
```

#### `Handle(SetAssetValue command)`
**Legacy body summary (adapt to EF):**
```
 var aggr = eventStore.GetById<Asset>(command.Id); aggr.SetValue(command.Value); eventStore.Save(aggr); 
```

#### `Handle(MaintainAsset command)`
**Legacy body summary (adapt to EF):**
```
 var aggr = eventStore.GetById<Asset>(command.Id); aggr.Maintain(command.Maintenance); eventStore.Save(aggr); 
```

#### `Handle(SetWarranty command)`
**Legacy body summary (adapt to EF):**
```
 var aggr = eventStore.GetById<Asset>(command.Id); aggr.SetWarranty(command.Warranty); eventStore.Save(aggr); 
```


### Agent implementation notes
- Map Raven `IDocumentSession` → `DbContext`; `Store`/`SaveChanges` → `SaveChangesAsync`.
- Map `eventStore.Append` / stream saves → single EF entity update (current state).
- Map `throw new ...Exception` → ASP.NET 400/404 with message.
- Preserve status enum numeric meanings when writing conversions.
