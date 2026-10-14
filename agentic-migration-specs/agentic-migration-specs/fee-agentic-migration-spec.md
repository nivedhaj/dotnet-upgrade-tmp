# Detailed Migration Spec: Fee

**Legacy:** `ct.fee` → **Target:** `Ct.Fee`  
**HTTP actions to implement:** 62  
**Notes:** Raven documents Fee/FeeTx; Rabbit fee.tx.done; indexes → reports

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

### EmailController.cs
- Route prefix: `api/fee/[controller]`
| HTTP | Route | Method | Parameters |
|---|---|---|---|
| Post | `/feeReceipt` | `EmailFeeReceipt` | [FromBody] JObject jObj |

### FeeController.cs
- Route prefix: `api/[controller]`
| HTTP | Route | Method | Parameters |
|---|---|---|---|
| Get | `/` | `Get` | [FromQuery] string id = "" |
| Get | `/getById` | `FeeGetById` | [FromQuery] string id |
| Get | `/feeInstFlattened` | `FeeInst` | [FromQuery] string courseId = "" |
| Post | `/` | `Post` | [FromBody] FeeCreateDto json |
| Post | `/updateFee` | `Update` | [FromBody] UpdateDto json |
| Post | `/canRemoveStudent` | `CanRemoveStudent` | [FromBody] FeeStudentDto json |
| Post | `/disable` | `Disable` | [FromQuery] string id |
| Post | `/enable` | `Enable` | [FromQuery] string id |

### FeeTxController.cs
- Route prefix: `api/[controller]`
| HTTP | Route | Method | Parameters |
|---|---|---|---|
| Get | `/` | `Get` | [FromQuery] string id |
| Get | `/getTxHTML` | `GetTxHTML` | [FromQuery] string id |
| Post | `/transact` | `Transact` | [FromBody] FeeTx feeTx |
| Post | `/transact` | `Transact` | [FromBody] FeeTx feeTx |
| Post | `/disable` | `DisableTx` | [FromQuery] string id |
| Post | `/enable` | `EnableTx` | [FromQuery] string id |
| Get | `/studentTX` | `StudentTX` | [FromQuery] string id |
| Get | `/feesToBePaid` | `GetFeesToBePaid` | [FromQuery] string studentId |
| Post | `/updateTx` | `UpdateFeeTx` | [FromBody] UpdateFeeTxDto dto |
| Get | `/feeCollectionAccounts` | `GetFeeCollectionAccounts` |  |
| Post | `/suspenseAccountTx` | `CreateSuspenseAccountTx` | [FromBody] SuspenseAccountTxDto dto |
| Get | `/suspenseAccountTx` | `GetSuspenseAccountTx` | [FromQuery] string id |
| Post | `/allocateSuspenseAccountTx` | `AllocateSuspenseAccountTx` | [FromBody] FeeTx feeTx, [FromQuery] string suspenseAccountTxId |
| Post | `/disableSuspenseAccountTx` | `DisableSuspenseAccountTx` | [FromQuery] string id |
| Post | `/enableSuspenseAccountTx` | `EnableSuspenseAccountTx` | [FromQuery] string id |

### RefundController.cs
- Route prefix: `api/fee/[controller]`
| HTTP | Route | Method | Parameters |
|---|---|---|---|
| Post | `/` | `Post` | [FromBody] RefundCreateDto json |
| Post | `/disable` | `Disable` | [FromQuery] string id |
| Post | `/enable` | `Enable` | [FromQuery] string id |
| Get | `/` | `GetAll` |  |

### ReportController.cs
- Route prefix: `api/fee/[controller]`
| HTTP | Route | Method | Parameters |
|---|---|---|---|
| Post | `/feeStructure` | `FeeStructure` | [FromBody] JObject jObj |
| Post | `/coursewiseFeeStructure` | `CoursewiseFeeStructure` | [FromBody] JObject jObj |
| Post | `/feesExpected` | `FeesExpected` | [FromBody] JObject jObj |
| Post | `/datewiseFeeCollection` | `DatewiseFeeCollection` | [FromBody] JObject jObj |
| Post | `/datewiseFeeCollectionGrouped` | `DatewiseFeeCollectionGrouped` | [FromBody] JObject jObj |
| Post | `/feesDefaultersVersion1` | `FeesDefaultersList` | [FromBody] JObject jObj |
| Post | `/feesDefaultersVersion2` | `FeesDefaultersList_v2` | [FromBody] JObject jObj |
| Get | `/defaultersAsJson` | `DefaultersAsJson` |  |
| Post | `/receiptTransaction` | `ReceiptTransaction` | [FromBody] JObject jObj |
| Post | `/receiptTransactionCSV` | `ReceiptTransactionCSV` | [FromBody] JObject jObj |
| Post | `/studentwiseTransaction` | `StudentwiseTransaction` | [FromBody] JObject jObj |
| Post | `/courseStudentwiseTransaction` | `CourseStudentwiseTransaction` | [FromBody] JObject jObj |
| Post | `/adjustmentTransactions` | `AdjustmentTransactionsReport` | [FromBody] JObject jObj |
| Post | `/feesPaid` | `FeesPaid` | [FromBody] JObject jObj |
| Post | `/feesPaidCSV` | `FeesPaidCSV` | [FromBody] JObject jObj |
| Post | `/breakUps` | `GetBreakUpsReport` | [FromBody] JObject jObj |
| Post | `/fineCollected` | `GetFineCollectedReport` | [FromBody] JObject jObj |
| Post | `/latePayment` | `GetLatePaymentReport` | [FromBody] JObject jObj |
| Post | `/timeLine` | `GetTimeLineReport` | [FromBody] JObject jObj |
| Post | `/adjustmentBalance` | `AdjustmentBalanceReport` | [FromBody] JObject jObj |
| Post | `/advanceBalance` | `AdvanceBalanceReport` | [FromBody] JObject jObj |
| Post | `/TxHistory` | `TxHistory` |  |
| Post | `/feesPaidAfterAcademicYear` | `FeesPaidAfterAcademicYear` |  |
| Post | `/feesPaidByTCStudents` | `FeesPaidByTCStudents` |  |
| Post | `/coursewiseFeeBreakupAsJson` | `CoursewiseFeesBreakupAsJson` | [FromBody] JObject jObj |
| Post | `/coursewiseFeeCollectionAsJson` | `CoursewiseFeesCollectionAsJson` | [FromBody] JObject jObj |
| Post | `/coursewiseFeeStructureAsJson` | `CoursewiseFeesStructureAsJson` |  |
| Post | `/paymentModeBreakup` | `PaymentModeBreakup` | [FromBody] JObject jObj |
| Post | `/printFeeReceipt` | `PrintFeeReceipt` | [FromBody] JObject jObj |
| Get | `/feesPayable` | `FeesPayable` | [FromBody] JObject jObj |
| Post | `/feeReceivedInOtherAccount` | `FeeReceivedInOtherAccount` | [FromBody] JObject jObj |
| Post | `/concession` | `ConcessionReport` | [FromBody] JObject jObj |
| Post | `/refund` | `RefundReport` | [FromBody] JObject jObj |
| Post | `/suspenseAccountTxn` | `SuspenseAccountTxnReport` | [FromBody] JObject jObj |

## Command layer (*Cmd) — implement all
### FeeCmd.cs
- `void Disable(string id)`
- `void Enable(string id)`
- `void EnableTx(string id)`
- `void DisableTx(string id)`

### FeeTxCmd.cs
- `FeeTx UpdateFeeTx(string id, DateTime date, string refNo, string paidBy)`
- `SuspenseAccountTx CreateSuspenseAccountTx(SuspenseAccountTxDto dto)`
- `SuspenseAccountTx AllocateSuspenseAccountTx(string suspenseAccountTxId, string feeTxId)`
- `void DisableSuspenseAccountTx(string suspenseAccountTxId)`
- `void EnableSuspenseAccountTx(string suspenseAccountTxId)`

### RefundCmd.cs
- `object CreateRefund(string paymentMode, string studentId, decimal amount, DateTime date, string ownerId = null)`
- `void Disable(string id)`
- `void Enable(string id)`

## Query layer (*Qry) — implement all
### FeeQry.cs
- `RangeValue GetDates(string rangeType, int month, int year)`
- `List<FeeExpectedCollectedDto> FeesExpectedCollected(string instId = "", bool includeRTE = true)`

### FeeTxQry.cs
- `List<StudentClosingBalanceDto> GetClosingBalance(string instId = "", bool includeRTE = true)`
- `List<StuFeesExpectedPaidDto> GetStudentFeesExpectedPaid2(bool includeRTE = true)`
- `FeeTx GetFeeTxByRefNo(string refNo)`
- `List<string> GetFeeCollectionAccounts()`
- `SuspenseAccountTx GetSuspenseAccountTx(string id)`
- `List<SuspenseAccountTxnWithAllocationsDto> GetSuspenseAccountTx()`

### RefundQry.cs

## Service layer — implement or fold into *Service
### CsvService.cs
- `static ByteArrayContent GetByteArrayContent(string csv)`
- `static string Escape(string s)`
- `static string Unescape(string s)`
- `void Dispose()`

### EmailService.cs
- `void EmailFeeReceipt(string receiptString, string message, string studentId)`

### EtlService.cs
- `string GetApiScript(string endPoint, object payload, string separator = "\u0009", string verb = "post")`
- `string GetApiScriptJsonl(string endPoint, object payload, string separator = "\u0009", string verb = "post")`
- `string GetTargetId(string base_id, string year)`
- `string GetS3PathFees(string shortName, string targetYear, string env = "test")`
- `string GetEtlFileName(string shortName, string targetYear, string env = "test")`

### FeeService.cs
- `List<StuFeesExpectedPaidDto> GetStudentFeesExpectedPaid(List<StudentDetailDto> stuList, List<Fee> allFees, string courseId = null, List<FeeInstDto> feeInsts = null)`

### ReportService.cs
- `string GetBreakUps(PrintOptions options = null, bool testData = false)`
- `string GetBreakUpsData(bool testData = false)`
- `string GetFineCollected(PrintOptions options = null, bool testData = false)`
- `string GetFineCollectedData(bool testData = false)`
- `string GetLatePayment(PrintOptions options = null, bool testData = false)`
- `string GetLatePaymentData(bool testData = false)`
- `string GetTimeLine(PrintOptions options = null, bool testData = false)`
- `string GetTimeLineData(bool testData = false)`
- `CoursewiseFeeWidgetDto CoursewiseFeesBreakup(List<RangeValue> dateRanges, string rangeType)`
- `CoursewiseFeeCollectedWidgetDto CoursewiseFeeCollection(List<RangeValue> dateRanges, string rangeType)`
- `CoursewiseFeeWidgetDto CoursewiseFeesStructureBreakup()`

### StudentClientService.cs
- `Task UpdateStudentFeeReceivable_old(string studentId, string instId, string feeId, string feeName, string txId, decimal amount)`
- `Task UpdateStudentFeeReceivable(string studentId, string instId, string feeId, string feeName, string txId, decimal amount)`
- `Task UpdateFeeReceivableStatus(string studentId, string txId, string instId, string feeId, string feeName, decimal amount)`
- `Task UpdateStudentFeeReceivableForSpanningFee(string feeId, string feeName, List<string> studentIds, decimal amount, string instId)`
- `Task UpdateStudentFeeReceivableStatus(string feeId, List<string> studentIds, string instId, string status)`

## Domain resources (from legacy Resource/Aggregate)
Implement as EF entities. Nested lists/dicts → jsonb unless volume requires tables.
### Fee.cs
**Enums:**
- `FeeStatusEnum`: Unknown=0, Active=1, Disabled=99
**Properties:**
- `string Name`
- `string NameLower`
- `string DisplayText`
- `decimal Amount`
- `List<string> Tags`
- `bool CollectStudentWise`
- `List<string> StudentList`
- `List<string> CourseList`
- `List<Installment> Installments`
- `List<Fine> Fines`
- `bool IsTxDone`
- `FeeStatusEnum Status`
- `bool SpansAcademicYears`
- `int Id`
- `string Description`
- `Decimal Amount`
- `DateTime? DueDate`
- `int Id`
- `string Description`
- `Decimal Amount`
- `int OverDueByDays`
- `int Id`
- `Decimal Amount`
- `string Description`
- `string DueDate`
- `string FullName`
- `List<Sections> Sections`
- `bool Status`
- `string Name`
- `string FeeId`
- `int InstallmentId`
- `string Name`
- `decimal Amount`
- `DateTime? DueDate`
- `string FeeId`
- `string Name`
- `int InstallmentId`
- `decimal Amount`
- `DateTime Date`
- `string ReceiptNo`
- `string TxId`
- `int FineId`
- `string Name`
- `string FeeId`
- `string FeeName`
- `decimal Amount`
- `string StudentId`
- `List<FeesPaid> FeesPaid`
- `List<PendingFees> PendingFees`
- `List<FinesApplicable> FinesApplicable`
- `bool AllowDiscount`
- `string Id`
- `string Name`
- `string ShortName`
- `DateTime? AcademicYearFrom`
- `DateTime? AcademicYearTo`
- `string Id`
- `string Name`
- `string InstId`

### FeeTx.cs
**Enums:**
- `FeeTxStatusEnum`: Active=1, Disabled=99
**Properties:**
- `string TxNo`
- `DateTime TxDate`
- `string StudentId`
- `List<FeeTxDto> InstallmentsPaid`
- `List<FeeFineDto> FinesPaid`
- `List<FeeDiscountDto> Discounts`
- `FeeAdjustmentDto FeeAdjustment`
- `string PaymentMode`
- `bool IsFinePaid`
- `bool IsDiscountGiven`
- `bool HasFeeAdjustment`
- `bool IsOpeningBalanceAdjusted`
- `string RefNo`
- `string ReceivedInAccount`
- `decimal Amount`
- `FeeTxStatusEnum Status`
- `string PaidBy`
- `string ChequeNo`
- `string BankName`
- `DateTime ChequeDate`
- `string OnlineTxnRefNo`
- `bool IsFromSuspenseAccount`
- `string FeeId`
- `string FeeName`
- `string FeeDisplayText`
- `int InstallmentId`
- `string Description`
- `DateTime? DueDate`
- `decimal Amount`
- `string Id`
- `string Name`
- `string Description`
- `decimal Amount`
- `int FineId`
- `string Description`
- `decimal Amount`
- `string FeeId`
- `string FeeName`
- `string Description`
- `decimal Amount`
- `string Id`
- `string OwnerId`
- `string ParentId`
- `string TxNo`
- `DateTime TxDate`
- `string StudentId`
- `string StudentName`
- `string CourseId`
- `string CourseName`
- `string Section`
- `decimal Amount`
- `List<FeeTxDto> InstallmentsPaid`
- `List<FeeFineDto> FinesPaid`
- `List<FeeDiscountDto> Discounts`
- `FeeAdjustmentDto FeeAdjustment`
- `bool IsFinePaid`
- `bool IsDiscountGiven`
- `bool IsOpeningBalanceAdjusted`
- `FeeTxStatusEnum Status`
- `string RefNo`
- `string PaidBy`
- `string ReceivedInAccount`
- `string PaymentMode`
- `string OnlineTxnRefNo`
- `string ChequeNo`
- `DateTime ChequeDate`
- `string BankName`
- `string Id`
- `DateTime TxDate`
- `string RefNo`
- `string PaidBy`
- `string ReceivedInAccount`

### Refund.cs
**Enums:**
- `RefundStatusEnum`: Active=1, Disabled=99
**Properties:**
- `string PaymentMode`
- `string StudentId`
- `decimal Amount`
- `DateTime Date`
- `RefundStatusEnum Status`

### SuspenseAccountTx.cs
**Enums:**
- `SuspenseAccountTxStatusEnum`: Pending=10, PartiallyAllocated=20, Allocated=30, Active=1, Disabled=99
**Properties:**
- `DateTime TxDate`
- `decimal Amount`
- `string PaymentMode`
- `string OnlineTxnRefNo`
- `string PaymentParticulars`
- `SuspenseAccountTxStatusEnum Status`
- `List<Allocation> Allocations`
- `string FeeTxId`
- `string FeeTxNo`
- `decimal AllocatedAmount`

## EF mapping rules for this module
- Primary key: `Guid Id` (generate on create).
- Always set `OwnerId` from user context on insert.
- Soft status enums rather than physical delete when legacy used Disable.
- Report endpoints: return structured JSON; optional simple HTML wrapper.
- External SMS/email: stub or publish to Connect/Rabbit; do not hard-require gateways.

## Acceptance checklist
- [ ] All **62** HTTP actions present (method names match table)
- [ ] All listed Cmd/Qry methods available on services/facades
- [ ] No Raven/Marten package references
- [ ] Host registration complete
- [ ] OwnerId isolation on every query


---

## Business rules (authoritative)

Derived from legacy Command/Handler method bodies. **Implement these constraints in EF services.**


### Cross-cutting Fee rules
- Always scope by `OwnerId` from `X-Owner-Id` / inst header.
- **FeeTransaction uniqueness (Transact2-style):** reject duplicate payment for same student/fee/period/amount when legacy enforced uniqueness; keep transaction id stable.
- **Expected vs paid:** queries compute balances from fee definitions minus successful transactions (exclude cancelled/refunded as per status enums in domain).
- **Refunds:** only against existing successful TX; do not allow refund > paid remainder.
- **Events:** after successful fee TX, publish `fee.tx.done` (payload includes fee_tx_id) via Rabbit/optional publisher — do not require consumer in-module.
- Reports (defaulters, closing balance, sectionwise): filter Active students/fees; group by section/course as field names allow.


### Per-method signals from legacy code


### Source: `FeeCmd`

#### `Disable(string id)`
**Guards / branches (from legacy):**
- if (!_uc.BoundByInst)
- if (_fee == null)
- if (fee_tx != null)
- if (_fee.SpansAcademicYears)
**Throws:** inst_id; $"Cannot disable. Fee Tx already done for {_fee.Name}"
**Legacy body summary (adapt to EF):**
```
 var inst_id = _uc.InstId; if (!_uc.BoundByInst) throw new NotInScopeException(inst_id); var _fee = _session.Load<Fee>(id); if (_fee == null) return; var fee_tx = _session.Query<FeeTx_Index.Result, FeeTx_Index>() .Where(o => o.FeeId == _fee.Id) .OfType<Fee>() .FirstOrDefault(); if (fee_tx != null) throw new ArgumentException($"Cannot disable. Fee Tx already done for {_fee.Name}"); _fee?.Disable(); _session.Store(_fee); _session.SaveChanges(); if (_fee.SpansAcademicYears) UpdateStudentFeeReceivab
```

#### `Enable(string id)`
**Guards / branches (from legacy):**
- if (!_uc.BoundByInst)
- if (_fee == null)
- if (_fee.SpansAcademicYears)
**Throws:** inst_id
**Legacy body summary (adapt to EF):**
```
 var inst_id = _uc.InstId; if (!_uc.BoundByInst) throw new NotInScopeException(inst_id); var _fee = _session.Load<Fee>(id); if (_fee == null) return; _fee?.Enable(); _session.Store(_fee); _session.SaveChanges(); if (_fee.SpansAcademicYears) UpdateStudentFeeReceivableStatus(_fee, "Active"); 
```

#### `EnableTx(string id)`
**Guards / branches (from legacy):**
- if (string.IsNullOrEmpty(id)
- if (_uc.Persist)
**Legacy body summary (adapt to EF):**
```
 if (string.IsNullOrEmpty(id)) return; var _tx = _session.Load<FeeTx>(id); _tx.Enable(); if (_uc.Persist) { _session.Store(_tx); _session.SaveChanges(); } 
```

#### `DisableTx(string id)`
**Guards / branches (from legacy):**
- if (string.IsNullOrEmpty(id)
- if (_uc.Persist)
**Legacy body summary (adapt to EF):**
```
 if (string.IsNullOrEmpty(id)) return; var _tx = _session.Load<FeeTx>(id); _tx.Disable(); if (_uc.Persist) { _session.Store(_tx); _session.SaveChanges(); } 
```


### Source: `FeeTxCmd`

#### `AllocateSuspenseAccountTx(string suspenseAccountTxId, string feeTxId)`
**Guards / branches (from legacy):**
- if (!_uc.BoundByInst)
- if (suspenseAccountTx == null)
- if (feeTx == null)
- if (suspenseAccountTx.Allocations.Count == 0)
- if (feeTx.Amount == suspenseAccountTx.Amount)
- if (feeTx.Amount < suspenseAccountTx.Amount)
- if (feeTx.Amount <= 0 || feeTx.Amount > remainingAmount)
- if (feeTx.Amount == remainingAmount)
- if (feeTx.Amount < remainingAmount)
**Throws:** inst_id; "Suspense account transaction not found"; "Fee transaction not found"; "Allocation amount exceeds available amount"; "Invalid allocation amount"
**Status assignments:** Status = SuspenseAccountTxStatusEnum.Allocated, Status = SuspenseAccountTxStatusEnum.PartiallyAllocated, Status = SuspenseAccountTxStatusEnum.Allocated, Status = SuspenseAccountTxStatusEnum.PartiallyAllocated
**Legacy body summary (adapt to EF):**
```
 var inst_id = _uc.InstId; if (!_uc.BoundByInst) throw new NotInScopeException(inst_id); var suspenseAccountTx = _session.Load<SuspenseAccountTx>(suspenseAccountTxId); if (suspenseAccountTx == null) throw new ObjectNotFoundException("Suspense account transaction not found"); var feeTx = _session.Load<FeeTx>(feeTxId); if (feeTx == null) throw new ObjectNotFoundException("Fee transaction not found"); if(suspenseAccountTx.Allocations.Count == 0) { var allocation = new Allocation() { FeeTxId = feeTx
```

#### `DisableSuspenseAccountTx(string suspenseAccountTxId)`
**Guards / branches (from legacy):**
- if (!_uc.BoundByInst)
- if (suspenseAccountTx == null)
- if (suspenseAccountTx.Allocations != null && suspenseAccountTx.Allocations.Count > 0)
**Throws:** inst_id; "Suspense account transaction not found"; "Cannot disable a suspense account transaction that has allocations"
**Status assignments:** Status = SuspenseAccountTxStatusEnum.Disabled
**Legacy body summary (adapt to EF):**
```
 var inst_id = _uc.InstId; if (!_uc.BoundByInst) throw new NotInScopeException(inst_id); var suspenseAccountTx = _session.Load<SuspenseAccountTx>(suspenseAccountTxId); if (suspenseAccountTx == null) throw new ObjectNotFoundException("Suspense account transaction not found"); if(suspenseAccountTx.Allocations != null && suspenseAccountTx.Allocations.Count > 0) { throw new InvalidOperationException("Cannot disable a suspense account transaction that has allocations"); } suspenseAccountTx.Status = S
```

#### `EnableSuspenseAccountTx(string suspenseAccountTxId)`
**Guards / branches (from legacy):**
- if (!_uc.BoundByInst)
- if (suspenseAccountTx == null)
**Throws:** inst_id; "Suspense account transaction not found"
**Status assignments:** Status = SuspenseAccountTxStatusEnum.Pending
**Legacy body summary (adapt to EF):**
```
 var inst_id = _uc.InstId; if (!_uc.BoundByInst) throw new NotInScopeException(inst_id); var suspenseAccountTx = _session.Load<SuspenseAccountTx>(suspenseAccountTxId); if (suspenseAccountTx == null) throw new ObjectNotFoundException("Suspense account transaction not found"); suspenseAccountTx.Status = SuspenseAccountTxStatusEnum.Pending; _session.Store(suspenseAccountTx); _session.SaveChanges(); 
```

#### `UpdateFeeTx(string id, DateTime date, string refNo, string paidBy)`
**Guards / branches (from legacy):**
- if (tx == null)
**Throws:** "Transaction not found"
**Legacy body summary (adapt to EF):**
```
 var tx = _session.Load<FeeTx>(id); if (tx == null) throw new ObjectNotFoundException("Transaction not found"); else { tx.TxDate = date; tx.RefNo = refNo; tx.PaidBy = paidBy; tx.ReceivedInAccount = tx.ReceivedInAccount; _session.Store(tx); _session.SaveChanges(); } return tx; 
```

#### `CreateSuspenseAccountTx(SuspenseAccountTxDto dto)`
**Guards / branches (from legacy):**
- if (!_uc.BoundByInst)
**Throws:** inst_id
**Status assignments:** Status = SuspenseAccountTxStatusEnum.Pending
**Legacy body summary (adapt to EF):**
```
 var inst_id = _uc.InstId; if (!_uc.BoundByInst) throw new NotInScopeException(inst_id); var suspenseAccountTx = new SuspenseAccountTx() { OwnerId = inst_id, TxDate = dto.TxDate, Amount = dto.Amount, PaymentMode = dto.PaymentMode, OnlineTxnRefNo = dto.OnlineTxnRefNo, PaymentParticulars = dto.PaymentParticulars, Status = SuspenseAccountTxStatusEnum.Pending }; suspenseAccountTx.SetId(); _session.Store(suspenseAccountTx); _session.SaveChanges(); return suspenseAccountTx; 
```


### Source: `RefundCmd`

#### `Disable(string id)`
**Guards / branches (from legacy):**
- if (!_uc.BoundByInst)
- if (_refund == null)
**Throws:** inst_id
**Legacy body summary (adapt to EF):**
```
 var inst_id = _uc.InstId; if (!_uc.BoundByInst) throw new NotInScopeException(inst_id); var _refund = _session.Load<Refund>(id); if (_refund == null) return; _refund?.Disable(); _session.Store(_refund); _session.SaveChanges(); 
```

#### `Enable(string id)`
**Guards / branches (from legacy):**
- if (!_uc.BoundByInst)
- if (_refund == null)
**Throws:** inst_id
**Legacy body summary (adapt to EF):**
```
 var inst_id = _uc.InstId; if (!_uc.BoundByInst) throw new NotInScopeException(inst_id); var _refund = _session.Load<Refund>(id); if (_refund == null) return; _refund?.Enable(); _session.Store(_refund); _session.SaveChanges(); 
```

#### `CreateRefund(string paymentMode, string studentId, decimal amount, DateTime date, string ownerId = null)`
**Guards / branches (from legacy):**
- if (string.IsNullOrEmpty(paymentMode)
**Throws:** "Invalid refund data."
**Status assignments:** Status = RefundStatusEnum.Active
**Legacy body summary (adapt to EF):**
```
 if (string.IsNullOrEmpty(paymentMode) || string.IsNullOrEmpty(studentId) || amount <= 0) { throw new ArgumentException("Invalid refund data."); } var refund = new Refund() { PaymentMode = paymentMode, StudentId = studentId, Amount = amount, Date = date }; refund.SetId(); refund.SetOwner(ownerId ?? _uc.InstId); refund.Status = RefundStatusEnum.Active; _session.Store(refund); _session.SaveChanges(); return refund; 
```


### Agent implementation notes
- Map Raven `IDocumentSession` → `DbContext`; `Store`/`SaveChanges` → `SaveChangesAsync`.
- Map `eventStore.Append` / stream saves → single EF entity update (current state).
- Map `throw new ...Exception` → ASP.NET 400/404 with message.
- Preserve status enum numeric meanings when writing conversions.
