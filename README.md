# FISI Admin Suite MVP Starter

Ein ausführbarer Startpunkt für die FISI Admin Suite mit .NET 8:

- **FisiAdmin.Desktop**: WPF-Client mit Dashboard, Benutzerliste und Onboarding-Dialog
- **FisiAdmin.Api**: ASP.NET Core Minimal API mit Beispiel-Daten und Job-Endpunkten
- **FisiAdmin.Agent**: Worker Service als Grundlage für sichere Hintergrundaufträge
- **FisiAdmin.Domain**: gemeinsame Domänenmodelle

> Sicherheit: Der Starter nutzt Demo-Daten. Produktive AD-, Graph- und PowerShell-Aktionen müssen hinter Rollenprüfung, Freigabe, Audit und einem least-privilege Dienstkonto implementiert werden.

## Voraussetzungen

- Windows 10/11 oder Windows Server
- Visual Studio 2022 mit .NET-Desktop- und ASP.NET-Workloads
- .NET 8 SDK

## Start

1. `FisiAdminSuite.sln` in Visual Studio öffnen.
2. `FisiAdmin.Api` starten.
3. `FisiAdmin.Desktop` starten.
4. Optional `FisiAdmin.Agent` starten.

CLI:

```powershell
dotnet restore
dotnet build
dotnet run --project src/FisiAdmin.Api
dotnet run --project src/FisiAdmin.Desktop
```

Die API läuft standardmäßig auf `http://localhost:5080`.

## Nächste produktive Schritte

1. Entra-ID/MSAL-Anmeldung ergänzen.
2. SQL Server und EF Core statt In-Memory-Repositories verwenden.
3. AD-Connector im Agent implementieren.
4. Agentenzertifikate und mTLS einführen.
5. RBAC und Audit unveränderbar implementieren.
6. PowerShell nur über geprüfte, versionierte Vorlagen ausführen.

## Demo-Login und Rollen

| Benutzer | Passwort | Rolle | Rechte |
|---|---|---|---|
| admin | Admin!123 | PlatformAdmin | Vollzugriff |
| adadmin | AdAdmin!123 | AdAdmin | Benutzer anlegen, ändern, entsperren |
| helpdesk | Helpdesk!123 | ServiceDesk | Lesen und Benutzer entsperren |
| auditor | Audit!123 | Auditor | Lesen und Jobs/Audit prüfen |
| readonly | ReadOnly!123 | ReadOnly | Nur Lesen |

Die Demo nutzt zufällige, serverseitig gehaltene Sitzungstoken. Für den Produktivbetrieb muss dies durch Microsoft Entra ID/MSAL, App Roles, HTTPS und eine persistente Benutzer-/Rollenverwaltung ersetzt werden.
