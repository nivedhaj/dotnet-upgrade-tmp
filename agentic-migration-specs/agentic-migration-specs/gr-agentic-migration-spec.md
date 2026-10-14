# Detailed Migration Spec: Gr

**Legacy:** `ct.gr` → **Target:** `Ct.Gr`  
**HTTP actions to implement:** 19  
**Notes:** Receipts / 80G / numbering

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

### GrController.cs
- Route prefix: `api/gr/[controller]`
| HTTP | Route | Method | Parameters |
|---|---|---|---|
| Get | `/` | `Get` | [FromQuery]string Id = "" |
| Post | `/` | `Post` | [FromBody]CreateReceiptDto json |
| Post | `/update` | `UpdateReceipt` | [FromQuery]string id, [FromBody]JObject jObj |
| Post | `/updatePaymentDetails` | `UpdatePaymentDetails` | [FromQuery]string id, [FromBody]JObject jObj |
| Post | `/updateOrderItemParticular` | `UpdateOrderItemParticular` | [FromQuery]string id, [FromBody]JObject jObj |
| Post | `/updateOrderItemAmount` | `UpdateOrderItemAmount` | [FromQuery]string id, [FromBody]JObject jObj |
| Post | `/updateOrderItemExtras` | `UpdateOrderItemExtras` | [FromQuery]string id, [FromBody]JObject jObj |
| Post | `/cancel` | `CancelReceipt` | [FromQuery]string id |
| Post | `/activate` | `ActivateReceipt` | [FromQuery] string id |
| Post | `/printReceipt` | `PrintReceipt` | [FromQuery]string id, [FromBody]JObject jObj |
| Get | `/getDto` | `GetDto` |  |
| Get | `/getDtoWithData` | `GetDtoWithData` | [FromQuery]string format = "json" |
| Post | `/audit` | `Audit` | [FromBody]ReceiptAuditDto json |
| Get | `/widget/receiptCollection` | `ReceiptCollection` |  |

### ReportController.cs
- Route prefix: `api/gr/[controller]`
| HTTP | Route | Method | Parameters |
|---|---|---|---|
| Post | `/getReceiptReports` | `GetReceiptReports` | [FromBody]JObject jObj |
| Post | `/getReceiptRevenueSharingReport` | `GetReceiptRevenueSharingReport` | [FromBody]JObject jObj |
| Post | `/getReceiptsSummary` | `GetReceiptsSummary` | [FromBody]JObject jObj |
| Post | `/getReceiptConsolidatedReport` | `GetReceiptConsolidatedReport` | [FromBody]JObject jObj |
| Post | `/printReceipt` | `PrintReceipt` | [FromBody]JObject jObj |

## Command layer (*Cmd) — implement all
### ReceiptCmd.cs
- `Receipt Create(DateTime date, Customer customer, ReceiptTypeEnum receiptType, List<OrderItem> orderItems, PaymentModeEnum paymentMode, FinancialInstrument fi, bool revenueSharingEnabled, int revenueShare, string receivedBy, Meta meta, string id = "", string ownerId)`
- `string UpdateReceipt(string id, ReceiptTypeEnum receiptType, DateTime date, Customer customer, bool revenueSharingEnabled, int revenueShare, Meta meta, string refNo)`
- `string UpdatePaymentDetails(string id, PaymentModeEnum paymentMode, FinancialInstrument fi)`
- `string UpdateOrderItemParticular(string id, string oldParticular, string newParticular, decimal amount)`
- `string UpdateOrderItemAmount(string id, string particular, decimal oldAmount, decimal newAmount)`
- `Receipt Audit(string id, string createdBy, DateTime createdOn)`
- `string UpdateOrderItemExtras(string id, string particular, decimal amount, string extraFieldName, string extraFieldValue)`
- `void CancelReceipt(string id)`
- `void ActivateReceipt(string id)`
- `string PrintReceipt(string id, Meta meta)`

## Query layer (*Qry) — implement all
### ReceiptQry.cs
- `Receipt GetReceiptById(string id)`
- `RangeValue GetDates(string rangeType, int rangeValue)`
- `List<Receipt> GetDtoWithData()`
- `dynamic GetMonthwiseReceiptsCollectionWidgetData(string instId = "")`
- `void SetRegulatoryCompliance(string instituteCode, string registrationNumber)`

## Service layer — implement or fold into *Service
### CsvService.cs
- `static ByteArrayContent GetByteArrayContent(string csv)`
- `static string Escape(string s)`
- `static string Unescape(string s)`
- `void Dispose()`

### ReceiptService.cs
- `ReceiptDto GetDto()`
- `List<ReceiptDto> GetDtoWithData()`

### ReportService.cs
- `string GetReceiptReport(DateTime from, DateTime to, bool includeOrderItems, bool includeCustomerAddress, PrintOptions options = null)`
- `List<MonthlyReceipts> GetReceiptReportModel(ClientDto instDto, DateTime fromDate, DateTime toDate, bool includeOrderItems, bool includeCustomerAddress, CultureInfo indianCurrency)`
- `string GetReceiptRevenueSharingByClass(DateTime fromDate, DateTime toDate, PrintOptions options = null)`
- `List<RevenueClassResult> GetReceiptRevenueSharingByClassModel(ClientDto inst_dto, DateTime fromDate, DateTime toDate)`
- `string GetReceiptRevenueSharingByPerson(DateTime fromDate, DateTime toDate, PrintOptions options = null)`
- `List<RevenuePersonResult> GetReceiptRevenueSharingByPersonModel(ClientDto inst_dto, DateTime fromDate, DateTime toDate)`
- `string GetReceiptReportSummary(DateTime fromDate, DateTime toDate, PrintOptions options = null)`
- `List<ReceiptSummaryTemplate> GetReceiptReportSummaryModel(ClientDto inst_dto, DateTime fromDate, DateTime toDate, CultureInfo indianCurrency)`
- `string GetReceiptConsolidatedReport(DateTime fromDate, DateTime toDate, PrintOptions options = null)`
- `OrgReceiptResult GetReceiptConsolidatedReportModel(ClientDto inst_dto, DateTime fromDate, DateTime toDate, CultureInfo indianCurrency)`
- `string GetReceiptPrint(string receiptId, PrintOptions options = null)`

## Domain resources (from legacy Resource/Aggregate)
Implement as EF entities. Nested lists/dicts → jsonb unless volume requires tables.
### DateHelper.cs

### Receipt.cs
**Enums:**
- `ReceiptStatusEnum`: Active=1, Cancelled=99
- `PaymentModeEnum`: Cash=10, Cheque=20, DD=30, Netbanking=40, UPI=50
- `ReceiptTypeEnum`: Unknown=0, Regular=10, Donation=20
- `CustomerTypeEnum`: Unknown=0, Regular=10, Student=20
**Properties:**
- `string Number`
- `string InstId`
- `DateTime Date`
- `Customer Customer`
- `List<OrderItem> OrderItems`
- `decimal TotalAmount`
- `string ReceivedBy`
- `PaymentModeEnum PaymentMode`
- `FinancialInstrument FinancialInstrument`
- `ReceiptStatusEnum Status`
- `ReceiptTypeEnum ReceiptType`
- `bool RevenueSharingEnabled`
- `int RevenueShare`
- `Meta Meta`
- `string HTML`
- `string RefNo`
- `string Particular`
- `Dictionary<string, string> Extras`
- `decimal Amount`
- `string Name`
- `string Address`
- `string Contact`
- `string PAN`
- `string StudyingIn`
- `string Section`
- `CustomerTypeEnum CustomerType`
- `string Number`
- `string NumberAsString`
- `DateTime DrawnOn`
- `string BankName`
- `string BankBranch`
- `string Number`
- `bool Include80G`

## EF mapping rules for this module
- Primary key: `Guid Id` (generate on create).
- Always set `OwnerId` from user context on insert.
- Soft status enums rather than physical delete when legacy used Disable.
- Report endpoints: return structured JSON; optional simple HTML wrapper.
- External SMS/email: stub or publish to Connect/Rabbit; do not hard-require gateways.

## Acceptance checklist
- [ ] All **19** HTTP actions present (method names match table)
- [ ] All listed Cmd/Qry methods available on services/facades
- [ ] No Raven/Marten package references
- [ ] Host registration complete
- [ ] OwnerId isolation on every query


---

## Business rules (authoritative)

Derived from legacy Command/Handler method bodies. **Implement these constraints in EF services.**


### Cross-cutting GR (Receipt) rules
- Receipt numbers sequential per OwnerId (or per series if legacy had series field).
- Create receipt: persist lines, totals, optional 80G fields; publish receipt-created event if legacy did.
- Do not reuse numbers for voided receipts if legacy kept gaps — prefer monotonic sequence table/row.


### Per-method signals from legacy code


### Source: `CsvService.cs`

#### `Escape(string s)`
**Guards / branches (from legacy):**
- if (s.Contains(QUOTE)
- if (s.IndexOfAny(CHARACTERS_THAT_MUST_BE_QUOTED)
**Legacy body summary (adapt to EF):**
```
 if (s.Contains(QUOTE)) s = s.Replace(QUOTE, ESCAPED_QUOTE); if (s.IndexOfAny(CHARACTERS_THAT_MUST_BE_QUOTED) > -1) s = QUOTE + s + QUOTE; return s; 
```

#### `Unescape(string s)`
**Guards / branches (from legacy):**
- if (s.StartsWith(QUOTE)
- if (s.Contains(ESCAPED_QUOTE)
**Legacy body summary (adapt to EF):**
```
 if (s.StartsWith(QUOTE) && s.EndsWith(QUOTE)) { s = s.Substring(1, s.Length - 2); if (s.Contains(ESCAPED_QUOTE)) s = s.Replace(ESCAPED_QUOTE, QUOTE); } return s; 
```

#### `Dispose()`
**Guards / branches (from legacy):**
- if (null != __reader)
**Legacy body summary (adapt to EF):**
```
 if (null != __reader) __reader.Dispose(); 
```

#### `GetByteArrayContent(string csv)`
**Legacy body summary (adapt to EF):**
```
 var stream = new MemoryStream(); StreamWriter writer = new StreamWriter(stream); writer.Write(csv); writer.Flush(); stream.Position = 0; var result = new ByteArrayContent(stream.ToArray()); return result; 
```


### Source: `ReceiptCmd.cs`

#### `Create(DateTime date, Customer customer, ReceiptTypeEnum receiptType, List<OrderItem> orderItems, PaymentModeEnum paymentMode, FinancialInstrument fi, bool revenueSharingEnabled, int revenueShare, string rec)`
**Guards / branches (from legacy):**
- if (!this._uc.BoundByInst)
- if (_uc.IsETL()
- if (!ownerId.Equals(instId)
- if (customer == null || string.IsNullOrEmpty(customer.Name)
- if (orderItems == null || orderItems.Count == 0)
- if (paymentMode == PaymentModeEnum.Cheque || paymentMode == PaymentModeEnum.DD)
- if (string.IsNullOrEmpty(fi.BankName)
- if (inst_dto == null)
- if (string.IsNullOrEmpty(rcpt_number)
- if (dupRcptNum != null)
**Throws:** ; ; ; 
**Legacy body summary (adapt to EF):**
```
 var instId = _uc.InstId; // if (!this._uc.BoundByInst) return null; if (_uc.IsETL()) { if (!ownerId.Equals(instId)) return null; } if (customer == null || string.IsNullOrEmpty(customer.Name)) throw new ArgumentNullException(); if (orderItems == null || orderItems.Count == 0) throw new ArgumentNullException(); if (paymentMode == PaymentModeEnum.Cheque || paymentMode == PaymentModeEnum.DD) { if (string.IsNullOrEmpty(fi.BankName) || string.IsNullOrEmpty(fi.BankBranch)) throw new ArgumentNullExcept
```

#### `UpdateReceipt(string id, ReceiptTypeEnum receiptType, DateTime date, Customer customer, bool revenueSharingEnabled, int revenueShare, Meta meta, string refNo)`
**Guards / branches (from legacy):**
- if (!this._uc.BoundByInst)
- if (receipt == null)
- if (string.IsNullOrEmpty(customer.Name)
- if (date == null)
- if (string.IsNullOrEmpty(customer.Address)
- if (string.IsNullOrEmpty(customer.Contact)
- if (receipt.Customer.CustomerType == CustomerTypeEnum.Student && !string.IsNullOrEmpty(customer.StudyingIn)
- if (revenueSharingEnabled)
- if (revenueShare > 0 && revenueShare < 100)
**Throws:** ; ; 
**Legacy body summary (adapt to EF):**
```
 var instId = _uc.InstId; if (!this._uc.BoundByInst) return null; var receipt = _session.Load<Receipt>(id); if (receipt == null) throw new ArgumentNullException(); if (string.IsNullOrEmpty(customer.Name)) throw new ArgumentNullException(); if (date == null) throw new ArgumentNullException(); // update receipt date receipt.Date = date; receipt.ReceiptType = receiptType; //customer if (string.IsNullOrEmpty(customer.Address)) { customer.Address = null; } if (string.IsNullOrEmpty(customer.Contact)) 
```

#### `PrintReceipt(string id, Meta meta)`
**Guards / branches (from legacy):**
- if (string.IsNullOrEmpty(id)
- if (receipt == null)
- if (inst_dto == null)
- if (org == null)
- if (string.IsNullOrEmpty(inst_header_dto.Line1)
- if (e.Key.Contains("Months")
- if (e.Key.Contains("ConductedBy")
- if (receipt.ReceiptType != ReceiptTypeEnum.Donation)
- if (string.IsNullOrEmpty(receipt.Customer.Address)
- if (string.IsNullOrEmpty(receipt.Customer.Contact)
**Throws:** ; 
**Legacy body summary (adapt to EF):**
```
 var instId = _uc.InstId; if (string.IsNullOrEmpty(id)) return null; var receipt = _session.Load<Receipt>(id); if (receipt == null) return null; var inst_dto = RedisCache.GetClientDto(instId); if (inst_dto == null) throw new ArgumentNullException(); //TBD: Get org details // var org = session.Load<Org>(institute.ParentId); // if (org == null) throw new ArgumentNullException(); var include80G = meta != null ? meta.Include80G : false; var prefix = inst_dto.ShortName + "_"; string headerTemplateFil
```

#### `UpdateOrderItemParticular(string id, string oldParticular, string newParticular, decimal amount)`
**Guards / branches (from legacy):**
- if (!this._uc.BoundByInst)
- if (receipt == null)
- if (string.IsNullOrEmpty(oldParticular)
- if (orderItem == null)
**Throws:** ; ; "OrderItem not found"
**Legacy body summary (adapt to EF):**
```
 if (!this._uc.BoundByInst) return null; var receipt = _session.Load<Receipt>(id); if (receipt == null) throw new ArgumentNullException(); if (string.IsNullOrEmpty(oldParticular) || string.IsNullOrEmpty(newParticular)) throw new ArgumentNullException(); //check if order item exists var orderItem = receipt.OrderItems.Where(o => o.Particular.Equals(oldParticular) && o.Amount.Equals(amount)).SingleOrDefault(); if (orderItem == null) throw new ObjectNotFoundException("OrderItem not found"); // updat
```

#### `UpdateOrderItemAmount(string id, string particular, decimal oldAmount, decimal newAmount)`
**Guards / branches (from legacy):**
- if (!this._uc.BoundByInst)
- if (receipt == null)
- if (string.IsNullOrEmpty(particular)
- if (orderItem == null)
**Throws:** ; ; "OrderItem not found"
**Legacy body summary (adapt to EF):**
```
 if (!this._uc.BoundByInst) return null; var receipt = _session.Load<Receipt>(id); if (receipt == null) throw new ArgumentNullException(); if (string.IsNullOrEmpty(particular)) throw new ArgumentNullException(); //check if order item exists var orderItem = receipt.OrderItems.Where(o => o.Particular.Equals(particular) && o.Amount.Equals(oldAmount)).SingleOrDefault(); if (orderItem == null) throw new ObjectNotFoundException("OrderItem not found"); // update orderItem orderItem.Amount = newAmount; 
```

#### `UpdatePaymentDetails(string id, PaymentModeEnum paymentMode, FinancialInstrument fi)`
**Guards / branches (from legacy):**
- if (!this._uc.BoundByInst)
- if (paymentMode == PaymentModeEnum.Cheque || paymentMode == PaymentModeEnum.DD)
- if (string.IsNullOrEmpty(fi.BankName)
- if (receipt == null)
**Throws:** ; 
**Legacy body summary (adapt to EF):**
```
 if (!this._uc.BoundByInst) return null; if (paymentMode == PaymentModeEnum.Cheque || paymentMode == PaymentModeEnum.DD) { if (string.IsNullOrEmpty(fi.BankName) || string.IsNullOrEmpty(fi.BankBranch)) throw new ArgumentNullException(); } var receipt = _session.Load<Receipt>(id); if (receipt == null) throw new ArgumentNullException(); receipt.PaymentMode = paymentMode; receipt.FinancialInstrument = fi; receipt.HTML = this.PrintReceipt(receipt.Id, receipt.Meta); _session.SaveChanges(); return rece
```

#### `UpdateOrderItemExtras(string id, string particular, decimal amount, string extraFieldName, string extraFieldValue)`
**Guards / branches (from legacy):**
- if (!this._uc.BoundByInst)
- if (receipt == null)
- if (orderItem == null)
- if (ext.Key == null)
**Throws:** ; "OrderItem not found"
**Legacy body summary (adapt to EF):**
```
 if (!this._uc.BoundByInst) return null; var receipt = _session.Load<Receipt>(id); if (receipt == null) throw new ArgumentNullException(); //check if order item exists var orderItem = receipt.OrderItems.Where(o => o.Particular.Equals(particular) && o.Amount.Equals(amount)).SingleOrDefault(); if (orderItem == null) throw new ObjectNotFoundException("OrderItem not found"); // update orderItem var ext = orderItem.Extras.Where(e => e.Key.Equals(extraFieldName)).SingleOrDefault(); if (ext.Key == null
```

#### `Audit(string id, string createdBy, DateTime createdOn)`
**Guards / branches (from legacy):**
- if (string.IsNullOrEmpty(id)
- if (receipt == null)
**Throws:** ; 
**Legacy body summary (adapt to EF):**
```
 var instId = _uc.InstId; if (string.IsNullOrEmpty(id)) throw new ArgumentNullException(); var receipt = _session.Load<Receipt>(id); if (receipt == null) throw new ArgumentNullException(); receipt.CreatedOn = createdOn; receipt.CreatedBy = createdBy; _session.SaveChanges(); return receipt; 
```

#### `CancelReceipt(string id)`
**Guards / branches (from legacy):**
- if (this._uc.BoundByInst)
- if (receipt == null)
**Throws:** 
**Legacy body summary (adapt to EF):**
```
 if (this._uc.BoundByInst) { var receipt = _session.Load<Receipt>(id); if (receipt == null) throw new ArgumentNullException(); //set status to Cancelled receipt.CancelReceipt(); receipt.ModifiedBy = this._uc.UserId; receipt.ModifiedOn = DateTime.UtcNow; _session.SaveChanges(); } 
```

#### `ActivateReceipt(string id)`
**Guards / branches (from legacy):**
- if (this._uc.BoundByInst)
- if (receipt == null)
**Throws:** 
**Legacy body summary (adapt to EF):**
```
 if (this._uc.BoundByInst) { var receipt = _session.Load<Receipt>(id); if (receipt == null) throw new ArgumentNullException(); //set status to Active receipt.ActivateReceipt(); receipt.ModifiedBy = this._uc.UserId; receipt.ModifiedOn = DateTime.UtcNow; _session.SaveChanges(); } 
```


### Source: `ReceiptService.cs`

#### `GetDto()`
**Legacy body summary (adapt to EF):**
```
 return new ReceiptDto(); 
```

#### `GetDtoWithData()`
**Legacy body summary (adapt to EF):**
```
 var receipts = _reciptQry.GetDtoWithData(); var recpt_list = new List<ReceiptDto>(); foreach (var recpt in receipts) { var dto = new ReceiptDto(); dto.id = recpt.Id; dto.amount = recpt.TotalAmount; dto.customer_address = recpt.Customer.Address; dto.customer_contact = recpt.Customer.Contact; dto.customer_name = recpt.Customer.Name; dto.customer_pan_number = recpt.Customer.PAN; dto.customer_section = recpt.Customer.Section; dto.customer_studying_in = recpt.Customer.StudyingIn; dto.date = recpt.Da
```


### Source: `ReportService.cs`

#### `GetReceiptReport(DateTime from, DateTime to, bool includeOrderItems, bool includeCustomerAddress, PrintOptions options = null)`
**Guards / branches (from legacy):**
- if (!_uc.BoundByInst)
- if (options == null)
- if (options.RecordsPerPage == 0)
**Throws:** inst_id
**Legacy body summary (adapt to EF):**
```
 CultureInfo indianCurrency = new CultureInfo("hi-IN"); //context var inst_id = _uc.InstId; if (!_uc.BoundByInst) throw new NotInScopeException(inst_id); //report header var inst_dto = RedisCache.GetClientDto(inst_id); //report options if (options == null) options = new PrintOptions() { RecordsPerPage = 2 }; if (options.RecordsPerPage == 0) options.RecordsPerPage = 2; //model var model = GetReceiptReportModel(inst_dto, from, to, includeOrderItems, includeCustomerAddress, indianCurrency); var dat
```

#### `GetReceiptConsolidatedReportModel(ClientDto inst_dto, DateTime fromDate, DateTime toDate, CultureInfo indianCurrency)`
**Guards / branches (from legacy):**
- if (inst_ids != null && inst_ids.Count()
- if (inst != null)
- if (r == null || r.OrderItems == null || r.OrderItems.Count == 0)
- if (receiptChunkResult.Sum(x => x.ReceiptChunkCount)
- if (!instReceiptResult.Any(i => i.InstituteName.Contains(inst.Name)
**Status assignments:** Status = o, Status = r.Status
**Legacy body summary (adapt to EF):**
```
 var org = RedisCache.GetOrgDto(inst_dto.OwnerId); List<InstReceiptResult> instReceiptResult = new List<InstReceiptResult>(); var inst_ids = _session .Advanced.RawQuery<dynamic>($"from Institutes where OwnerId = '{org.Id}'") .Select(inst => (string)inst.Id) .ToList(); var tDate = toDate.AddDays(1).Date; if (inst_ids != null && inst_ids.Count() > 0) { foreach (var id in inst_ids) { var inst = RedisCache.GetClientDto(id); if (inst != null) { List<ReceiptChunkResult> receiptChunkResult = new List<R
```

#### `GetReceiptPrint(string receiptId, PrintOptions options = null)`
**Guards / branches (from legacy):**
- if (!_uc.BoundByInst)
- if (options == null)
- if (options.RecordsPerPage == 0)
**Throws:** inst_id
**Legacy body summary (adapt to EF):**
```
 //context var inst_id = _uc.InstId; if (!_uc.BoundByInst) throw new NotInScopeException(inst_id); //report header var inst_dto = RedisCache.GetClientDto(inst_id); //report options if (options == null) options = new PrintOptions() { RecordsPerPage = 1 }; //chunk size to be set if (options.RecordsPerPage == 0) options.RecordsPerPage = 1;//chunk size to be set //model var model = GetReceiptPrintModel(receiptId); //prepare report_data based on model & report options var title1 = $""; Tuple<string, 
```

#### `GetReceiptReportModel(ClientDto instDto, DateTime fromDate, DateTime toDate, bool includeOrderItems, bool includeCustomerAddress, CultureInfo indianCurrency)`
**Guards / branches (from legacy):**
- if (!_uc.BoundByInst)
- if (r == null || r.OrderItems == null || r.OrderItems.Count == 0)
**Throws:** inst_id
**Status assignments:** Status = o, Status = r.Status
**Legacy body summary (adapt to EF):**
```
 var inst_id = _uc.InstId; if (!_uc.BoundByInst) throw new NotInScopeException(inst_id); var monthlyReceipts = new List<MonthlyReceipts>(); fromDate = fromDate.Date < instDto.AcademicYearFrom.Date ? instDto.AcademicYearFrom : fromDate; toDate = toDate.Date > instDto.AcademicYearTo.Date ? instDto.AcademicYearTo.AddDays(1).Date : toDate; var receiptTemplate = new List<ReceiptTemplate>(); Expression<Func<Receipt_Details_Index.Result, bool>> filterByInst = o => o.InstId.Equals(_uc.InstId); Expressio
```

#### `GetReceiptRevenueSharingByClass(DateTime fromDate, DateTime toDate, PrintOptions options = null)`
**Guards / branches (from legacy):**
- if (inst_dto == null)
- if (options == null)
**Throws:** 
**Legacy body summary (adapt to EF):**
```
 var inst_id = _uc.InstId; var inst_dto = RedisCache.GetClientDto(inst_id); if (inst_dto == null) throw new ArgumentNullException(); if (options == null) options = new PrintOptions(); fromDate = fromDate.Date < inst_dto.AcademicYearFrom.Date ? inst_dto.AcademicYearFrom : fromDate; toDate = toDate.Date > inst_dto.AcademicYearTo.Date ? inst_dto.AcademicYearTo.AddDays(1).Date : toDate; //model var model = GetReceiptRevenueSharingByClassModel(inst_dto, fromDate, toDate); //data var data = GetReportD
```

#### `GetReceiptRevenueSharingByPerson(DateTime fromDate, DateTime toDate, PrintOptions options = null)`
**Guards / branches (from legacy):**
- if (inst_dto == null)
- if (options == null)
**Throws:** 
**Legacy body summary (adapt to EF):**
```
 var inst_id = _uc.InstId; var inst_dto = RedisCache.GetClientDto(inst_id); if (inst_dto == null) throw new ArgumentNullException(); if (options == null) options = new PrintOptions(); fromDate = fromDate.Date < inst_dto.AcademicYearFrom.Date ? inst_dto.AcademicYearFrom : fromDate; toDate = toDate.Date > inst_dto.AcademicYearTo.Date ? inst_dto.AcademicYearTo.AddDays(1).Date : toDate; //model var model = GetReceiptRevenueSharingByPersonModel(inst_dto, fromDate, toDate); //data var data = GetReport
```

#### `GetReceiptReportSummary(DateTime fromDate, DateTime toDate, PrintOptions options = null)`
**Guards / branches (from legacy):**
- if (inst_dto == null)
- if (options == null)
**Throws:** 
**Legacy body summary (adapt to EF):**
```
 CultureInfo indianCurrency = new CultureInfo("hi-IN"); var inst_id = _uc.InstId; var inst_dto = RedisCache.GetClientDto(inst_id); if (inst_dto == null) throw new ArgumentNullException(); if (options == null) options = new PrintOptions(); var org = RedisCache.GetOrgDto(inst_dto.OwnerId); fromDate = fromDate.Date < inst_dto.AcademicYearFrom.Date ? inst_dto.AcademicYearFrom : fromDate; toDate = toDate.Date > inst_dto.AcademicYearTo.Date ? inst_dto.AcademicYearTo.AddDays(1).Date : toDate; //model v
```

#### `GetReceiptRevenueSharingByClassModel(ClientDto inst_dto, DateTime fromDate, DateTime toDate)`
**Guards / branches (from legacy):**
- if (receiptQry != null && receiptQry.Count()
- if (r == null || r.OrderItems == null || r.OrderItems.Count == 0)
**Status assignments:** Status = o, Status = r.Status
**Legacy body summary (adapt to EF):**
```
 var revenueResult = new List<RevenueClassResult>(); Expression<Func<Receipt_Revenue_Share_Index.Result, bool>> filterByInst = o => o.InstId.Equals(inst_dto.Id); Expression<Func<Receipt_Revenue_Share_Index.Result, bool>> filterByReceiptStatus = o => o.Status == ReceiptStatusEnum.Active; Expression<Func<Receipt_Revenue_Share_Index.Result, bool>> filterByDateRange = o => (fromDate.Date <= o.Date) && (o.Date <= toDate.Date); var receiptQry = _session.Query<Receipt_Revenue_Share_Index.Result, Receip
```

#### `GetReceiptConsolidatedReport(DateTime fromDate, DateTime toDate, PrintOptions options = null)`
**Guards / branches (from legacy):**
- if (!_uc.BoundByOrg)
- if (options == null)
**Legacy body summary (adapt to EF):**
```
 // if (!_uc.BoundByOrg) // return null; var inst_id = _uc.InstId; var inst_dto = RedisCache.GetClientDto(inst_id); CultureInfo indianCurrency = new CultureInfo("hi-IN"); if (options == null) options = new PrintOptions(); //model var model = GetReceiptConsolidatedReportModel(inst_dto, fromDate, toDate, indianCurrency); //data var data = GetReportData_ReceiptConsolidated(inst_dto, model, fromDate, toDate, indianCurrency, options); return data; 
```

#### `GetReceiptRevenueSharingByPersonModel(ClientDto inst_dto, DateTime fromDate, DateTime toDate)`
**Guards / branches (from legacy):**
- if (r == null || r.OrderItems == null || r.OrderItems.Count == 0)
**Status assignments:** Status = o, Status = r.Status
**Legacy body summary (adapt to EF):**
```
 List<RevenuePersonResult> revenueResult = new List<RevenuePersonResult>(); Expression<Func<Receipt_Revenue_Share_Index.Result, bool>> filterByInst = o => o.InstId.Equals(inst_dto.Id); Expression<Func<Receipt_Revenue_Share_Index.Result, bool>> filterByReceiptStatus = o => o.Status == ReceiptStatusEnum.Active; Expression<Func<Receipt_Revenue_Share_Index.Result, bool>> filterByDateRange = o => (fromDate.Date <= o.Date) && (o.Date <= toDate.Date); var receiptQry = _session.Query<Receipt_Revenue_Sha
```

#### `GetReceiptReportSummaryModel(ClientDto inst_dto, DateTime fromDate, DateTime toDate, CultureInfo indianCurrency)`
**Status assignments:** Status = o
**Legacy body summary (adapt to EF):**
```
 var receiptSummary = new List<ReceiptSummaryTemplate>(); Expression<Func<Receipt_Details_Index.Result, bool>> filterByInst = o => o.InstId.Equals(inst_dto.Id); Expression<Func<Receipt_Details_Index.Result, bool>> filterByReceiptStatus = o => o.Status == ReceiptStatusEnum.Active; Expression<Func<Receipt_Details_Index.Result, bool>> filterByDateRange = o => (fromDate.Date < o.Date) && (o.Date < toDate.Date); var receiptQry = _session.Query<Receipt_Details_Index.Result, Receipt_Details_Index>() .W
```


### Agent implementation notes
- Map Raven `IDocumentSession` → `DbContext`; `Store`/`SaveChanges` → `SaveChangesAsync`.
- Map `eventStore.Append` / stream saves → single EF entity update (current state).
- Map `throw new ...Exception` → ASP.NET 400/404 with message.
- Preserve status enum numeric meanings when writing conversions.
