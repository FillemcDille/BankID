"# En-bank-app" 
# En-bank-app (Blazor WebAssembly, .NET 8)

A learning project that simulates simple banking operations in the browser. You can create accounts, deposit/withdraw money, and transfer between accounts. State is stored in the browser (localStorage), so it persists across refreshes.

## Tech stack
- .NET 8
- Blazor WebAssembly (Client-side)
- C# 12
- Local storage persistence (key: `bankapp.accounts`)
- Visual Studio / `dotnet` CLI

## Features
- Create account
  - Required account name
  - Opening balance must be ≥ 0
  - Currency: SEK
  - Types: Deposit, Savings
- Transfer
  - From → To (must be different accounts)
  - Amount > 0 and cannot exceed the source account balance
  - Form-specific error messages (only the active form shows its own error)
  - Amount inputs are nullable; empty means “no action” until submitted
- Withdraw / Deposit
  - Amount > 0
  - Withdraw cannot exceed the account balance
  - Each operation writes a `Transaction`
- History
  - Per-account transaction table
  - Sort by date or amount
  - UTC timestamps saved; rendered as local time in UI
- Simple UI lock (optional)
  - PIN UI lock can be implemented in `MainLayout` (not real auth)

## Domain model (short)
- `BankAccount`
  - Methods: `Deposit`, `Withdraw`, `TransferTo`
  - Guards: positive amounts; sufficient funds for debit
  - Persists `Transactions` (Deposit, Withdraw, TransferIn, TransferOut)
  - Uses `[JsonConstructor]` to rehydrate transactions from storage
- `Transaction`
  - `Amount`, `TimeStamp (UTC)`, `TransactionType`, `BalanceAfter`, `FromAccountId`/`ToAccountId`

## Project structure (high level)
- `Domain/`
  - `BankAccount.cs`, `Transaction.cs`, `Type.cs` (enums: `AccountType`, `Currency`)
- `Services/`
  - `AccountService.cs` (in-memory + localStorage persistence)
  - `LokalStorageService.cs` (localStorage wrapper)
- `Interfaces/`
  - `IAccountService.cs`, `IStorageService.cs`, `IBankAccount.cs`
- `Pages/`
  - `CreateAccount.razor` – create new accounts
  - `Transfer.razor` – transfer + deposit + withdraw
  - `History.razor` – per-account transactions
  - `Home.razor` – landing page
- `Layout/`
  - `MainLayout.razor`, `NavMenu.razor`

## Validation rules (UI + domain)
- Create account
  - Name: required
  - Opening balance: `[Range(0, ∞)]`
- Transfer / Withdraw / Deposit
  - Amount must be greater than 0 (nullable until submit)
  - Transfer: accounts must differ; no overdraft
  - Withdraw: no overdraft

## Run locally

### Visual Studio
1. Open the solution.
2. Use __Build Solution__.
3. Press __Start Debugging__ (F5) or __Start Without Debugging__ (Ctrl+F5).
4. Browse to the app (default HTTPS port shown in the VS output).

### CLI
- Prerequisites: .NET 8 SDK
- Optional (publish size): `dotnet workload install wasm-tools`
- Run:
  - `dotnet restore`
  - `dotnet build`
  - `dotnet run` (from the project directory)

## How data is saved
- All accounts and transactions are serialized to JSON and saved under the localStorage key: `bankapp.accounts`.
- To reset data, clear the browser’s localStorage for that key or all site storage.

## Known gotchas
- Transaction times are saved as UTC and displayed with `.ToLocalTime()` in the UI.
- If you change the model shape, old localStorage data may not deserialize; clear storage if needed.
- Route casing in links vs `@page` directives can cause unexpected navigation; keep them consistent.

## Future improvements (ideas)
- Real authentication/authorization (ASP.NET Core Identity, external IdP)
- Globalization/formatting for currencies and dates
- Unit tests for domain logic
- Bulk import/export of accounts and transactions
- Better error toasts per form instead of inline alerts

## Disclaimer
This is a demo app for educational purposes; do not use it for real banking or financial operations.