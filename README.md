"# En-bank-app" 
📄 README – BankID2 (Delleverans 1)
🏦 Projektbeskrivning
BankID2 är en enkel men genomarbetad bankapplikation byggd i Blazor WebAssembly. Användaren kan skapa konton, se kontolista och all data sparas lokalt i webbläsaren. Applikationen är strukturerad enligt god objektorienterad design med tjänster, interface och dependency injection.

✅ Funktioner (Delleverans 1)
- Skapa ett nytt konto med:
- Kontonamn
- Kontotyp (Lönekonto/Sparkonto)
- Valuta (SEK)
- Startbelopp
- Validering av formulärdata
- Kontolista med:
- Namn
- Valuta
- Typ
- Saldo
- Senast uppdaterad
- Persistens via LocalStorage (data sparas mellan sessioner)
- Navigationsmeny med länk till kontosidan

🧱 Teknisk struktur
🔹 Domänklasser
- BankAccounts – representerar ett konto
- Transaction – förberedd för kommande transaktionslogik
- AccountType, Currency –
🔹 Tjänster
- IAccountService / AccountService – hanterar kontologik
- IStorageService / LocalStorageService – generisk lagring via JSInterop
🔹 UI-komponenter
- CreateAccount.razor – formulär och kontolista
- NavMenu.razor – navigering mellan sidor

🧪 Teknisk implementation
- Blazor WASM med Razor-komponenter
- Dependency Injection för tjänster
- LocalStorage via IJSRuntime och System.Text.Json
- Validering med DataAnnotations
- Routing via @page och NavLink


🧰 Git-flöde
- Feature branches används för nya funktioner
- Meningsfulla commits:
- feat: skapa konto med LocalStorage
- fix: serialisering med JsonConstructor
- chore: lägg till NavMenu och routing


