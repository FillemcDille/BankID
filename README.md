This is a small learning project I made to practice Blazor WebAssembly and C#.
It works like a simple bank where you can create accounts, deposit, withdraw, and transfer money between them.
All data is saved in the browser using localStorage, so it stays even after refreshing the page.

-- Tech stack--
.Net 8
Blazor WebAssembly(Client side)
C#
localStorage for saving data
Visual Studio

-- Features --
- Create account
Account name is required
Opening balance must be > 0
Curency:SEK
Account types: Salary or Savings
- Transfer
Transfer from one account to another (can’t be the same account)
Amount must be greater than 0 and not more than the source balance
- Withdraw / Deposit
Amount must be greater than 0
You can’t withdraw more money than you have
- Transaction history 
Shows all transactions for each account
Can sort by date or amount
Timestamps are saved in UTC and shown in local time

-- Pages --
Home
createaccount → Create new account
Transfer → Transfer money
history → View account history

-- Project structure --
Domain/ – logic classes like BankAccount and Transaction
Services/ – services that save and load data
Interfaces/ – interfaces that define how services should behave
Pages/ – Blazor pages (Razor components)
Layout/ – layout and navigation components

-- Validation rules --
- Create account 
Name is required
Opening balance must be ≥ 0
-- Transfer/Withdraw/Deposit
Amount must be > 0
Transfer: accounts must be different
Withdraw: cannot exceed the account balance

-- How data is stored --
All accounts and transactions are saved as JSON inside your browser’s localStorage
under the key: bankapp.accounts
If you want to reset your data, just clear the localStorage in your browser.

-- Common issues --
If navigation doesn’t work (@onclick errors), use ' around string routes in Razor.
If old data causes errors after code changes → clear localStorage.
If icons don’t show up → check that Bootstrap Icons are added in index.html.

-- Future improvements--
Real user authentication
Support for multiple languages
Unit tests for the logic
Import/export account data
Toast notifications for success/error messages
