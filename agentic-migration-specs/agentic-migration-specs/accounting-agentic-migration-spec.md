# Detailed Migration Spec: Accounting

**Legacy:** `ct.accounting` → **Target:** `Ct.Accounting`  
**HTTP actions to implement:** 21  
**Notes:** Event-sourced ledgers/vouchers → EF; Tally XML export

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

### InventoryController.cs
- Route prefix: `api/accounting/[controller]`
| HTTP | Route | Method | Parameters |
|---|---|---|---|
| Post | `/` | `Post` | [FromBody] InventoryItemDto dto |

### JournalController.cs
- Route prefix: `api/accounting/[controller]`
| HTTP | Route | Method | Parameters |
|---|---|---|---|
| Post | `/inventory` | `InventoryJournal` | [FromBody] InventoryJournalDto dto |
| Post | `/purchase` | `PurchaseItems` | [FromBody] AccountingDto dto |
| Post | `/sales` | `SalesItems` | [FromBody] AccountingDto dto |
| Post | `/smsInventoryCredit` | `AddSMSCredits` | [FromBody] InventoryJournalDto dto |
| Get | `/smsTx` | `GetSMSTx` | [FromQuery] string instId |

### LedgerController.cs
- Route prefix: `api/accounting/[controller]`
| HTTP | Route | Method | Parameters |
|---|---|---|---|
| Post | `/assets` | `Assets` | [FromBody] LedgerAccountDto dto |
| Post | `/liabilities` | `Liabilities` | [FromBody] LedgerAccountDto dto |
| Post | `/chartOfAccounts` | `chartOfAccounts` |  |
| Get | `/voucherTargetAccounts` | `GetVoucherTargetAccounts` |  |
| Get | `/voucherSourceAccounts` | `GetVoucherSourceAccounts` |  |

### ReportController.cs
- Route prefix: `api/accounting/[controller]`
| HTTP | Route | Method | Parameters |
|---|---|---|---|
| Post | `/smsCreditsTx` | `SMSCreditsTx` | [FromBody] JObject jObj |
| Post | `/datewiseExpense` | `GetDatewiseExpenseReport` | [FromBody] JObject jObj |
| Post | `/categorywiseExpense` | `GetCategorywiseExpenseReport` | [FromBody] JObject jObj |
| Post | `/yearwiseExpense` | `GetYearwiseExpenseReport` | [FromBody] JObject jObj |
| Get | `/tallyXML` | `GetTallyXMLReport` | [FromQuery] string id |
| Get | `/tallyPrimeXML` | `GetTallyPrimeXMLReport` | [FromQuery] string id |

### VoucherController.cs
- Route prefix: `api/accounting/[controller]`
| HTTP | Route | Method | Parameters |
|---|---|---|---|
| Post | `/create` | `NewVoucher` | [FromBody] VoucherDto dto |
| Get | `/` | `Get` | [FromQuery] string id |
| Post | `/disable` | `Disable` | [FromQuery] string id |
| Post | `/enable` | `Enable` | [FromQuery] string id |

## Command layer (*Cmd) — implement all
_None extracted._

## Query layer (*Qry) — implement all
### AccountingQry.cs
- `List<SMSTx> GetSMSTx(string inst_id)`

### LedgerQry.cs
- `List<LedgerDto> GetVoucherTargetAccounts()`
- `List<LedgerDto> GetVoucherSourceAccounts()`

### VoucherQry.cs

## Service layer — implement or fold into *Service
### AccountingService.cs

### ReportService.cs

### VoucherService.cs
- `string GetNextVoucherNo()`

## Domain resources (from legacy Resource/Aggregate)
Implement as EF entities. Nested lists/dicts → jsonb unless volume requires tables.
### AccountingJournal.cs
**Enums:**
- `AccountingJournalStatusEnum`: Active=1, Disabled=99
- `JournalEntryTypeEnum`: Dr=10, Cr=20
**Properties:**
- `string LedgerAccountId`
- `decimal Amount`
- `string Details`
- `string JournalType`
- `DateTime Date`
- `string Reference`
- `List<JournalEntry> Credits`
- `List<JournalEntry> Debits`
- `string Particulars`
- `AccountingJournalStatusEnum Status`
- `List<string> Tags`
- `string InventoryJournalId`
- `string OwnerId`
- `string Id`
- `string OwnerId`
- `string JournalType`
- `string Reference`
- `string Particulars`
- `DateTime Date`
- `List<JournalEntry> Credits`
- `List<JournalEntry> Debits`
- `string InventoryJournalId`
- `string Id`
- `string OwnerId`
- `string JournalType`
- `string Reference`
- `string Particulars`
- `DateTime Date`
- `List<JournalEntry> Credits`
- `List<JournalEntry> Debits`
- `string InventoryJournalId`
- `string Id`
- `string OwnerId`
- `string JournalType`
- `string Reference`
- `string Particulars`
- `DateTime Date`
- `List<JournalEntry> Credits`
- `List<JournalEntry> Debits`
- `string InventoryJournalId`
- `string Id`
- `string OwnerId`
- `string JournalType`
- `string Reference`
- `string Particulars`
- `DateTime Date`
- `List<JournalEntry> Credits`
- `List<JournalEntry> Debits`
- `string InventoryJournalId`
- `List<string> Tags`
- `string Id`
- `AccountingJournalStatusEnum Status`
- `string Id`
- `AccountingJournalStatusEnum Status`

### InventoryItem.cs
**Enums:**
- `InventoryItemStatusEnum`: Active=1, Disabled=99
- `InventoryTypeEnum`: Item=1, Group=2
**Properties:**
- `string Name`
- `string GroupId`
- `InventoryTypeEnum InventoryType`
- `string UOM`
- `List<string> Tags`
- `Dictionary<string, object> Attributes`
- `string OwnerId`
- `InventoryItemStatusEnum Status`
- `string Id`
- `string Name`
- `string GroupId`
- `InventoryTypeEnum InventoryType`
- `string UOM`
- `string OwnerId`
- `List<string> Tags`
- `Dictionary<string, object> Attributes`
- `string Id`
- `string Name`
- `string UOM`
- `string GroupId`
- `InventoryTypeEnum InventoryType`
- `List<string> Tags`
- `Dictionary<string, object> Attributes`
- `string OwnerId`
- `string Id`
- `InventoryItemStatusEnum Status`
- `string Id`
- `InventoryItemStatusEnum Status`

### LedgerAccount.cs
**Enums:**
- `LedgerOwnerTypeEnum`: Org=1, Inst=2
- `LedgerTypeEnum`: Ledger=1, Group=2
- `LedgerStatusEnum`: Active=1, Disabled=99
- `NatureOfAccountsEnum`: Inherit=0, Assets=10, Liabilities=20, Income=30, Expenses=40
**Properties:**
- `string Name`
- `string GroupId`
- `string OwnerId`
- `LedgerOwnerTypeEnum OwnerType`
- `LedgerTypeEnum LedgerType`
- `NatureOfAccountsEnum NatureOfAccounts`
- `LedgerStatusEnum Status`
- `string Id`
- `string Name`
- `string GroupId`
- `string OwnerId`
- `LedgerOwnerTypeEnum OwnerType`
- `LedgerTypeEnum LedgerType`
- `NatureOfAccountsEnum NatureOfAccounts`
- `string Id`
- `string Name`
- `string Id`
- `string Id`
- `string Id`
- `string GroupId`
- `NatureOfAccountsEnum NatureOfAccounts`

### Voucher.cs
**Enums:**
- `VoucherStatusEnum`: Active=1, Disabled=99
- `VoucherTypeEnum`: Expense=1
**Properties:**
- `string LedgerId`
- `decimal Amount`
- `Content Content`
- `bool Preview`
- `string Text`
- `string HtmlText`
- `string PlainText`
- `VoucherTypeEnum Type`
- `string VoucherNo`
- `string RefNo`
- `string Description`
- `DateTime Date`
- `List<LedgerItem> By`
- `List<LedgerItem> To`
- `decimal ByTotal`
- `decimal ToTotal`
- `Section Section`
- `List<string> Tags`
- `string OwnerId`
- `VoucherStatusEnum Status`
- `string Id`
- `string VoucherNo`
- `VoucherTypeEnum Type`
- `string RefNo`
- `string Description`
- `DateTime Date`
- `List<LedgerItem> By`
- `List<LedgerItem> To`
- `decimal ByTotal`
- `decimal ToTotal`
- `Section Section`
- `List<string> Tags`
- `string OwnerId`
- `string Id`
- `VoucherStatusEnum Status`
- `string Id`
- `VoucherStatusEnum Status`

## EF mapping rules for this module
- Primary key: `Guid Id` (generate on create).
- Always set `OwnerId` from user context on insert.
- Soft status enums rather than physical delete when legacy used Disable.
- Report endpoints: return structured JSON; optional simple HTML wrapper.
- External SMS/email: stub or publish to Connect/Rabbit; do not hard-require gateways.

## Acceptance checklist
- [ ] All **21** HTTP actions present (method names match table)
- [ ] All listed Cmd/Qry methods available on services/facades
- [ ] No Raven/Marten package references
- [ ] Host registration complete
- [ ] OwnerId isolation on every query


---

## Business rules (authoritative)

Derived from legacy Command/Handler method bodies. **Implement these constraints in EF services.**


### Cross-cutting Accounting rules
- **Voucher:** sum(debit lines) must equal sum(credit lines) or reject; store ByTotal/ToTotal.
- **Ledger natures:** Assets/Liabilities/Income/Expenses drive source vs target account lists for UI.
- **Inventory In:** increase QtyOnHand; **Out:** decrease; **Adjust:** set absolute qty when legacy did.
- **Tally XML:** export active vouchers as simplified ENVELOPE/TALLYMESSAGE structure (exact Tally schema fidelity optional).


### Per-method signals from legacy code

_No method-level extract; follow cross-cutting rules and endpoint semantics._


### Agent implementation notes
- Map Raven `IDocumentSession` → `DbContext`; `Store`/`SaveChanges` → `SaveChangesAsync`.
- Map `eventStore.Append` / stream saves → single EF entity update (current state).
- Map `throw new ...Exception` → ASP.NET 400/404 with message.
- Preserve status enum numeric meanings when writing conversions.
