# FISI Admin Suite

> Zentrale Windows-Server-Management-Anwendung für Fachinformatiker für Systemintegration, IT-Administratoren und Service-Desk-Mitarbeiter.

[![.NET](https://img.shields.io/badge/.NET-8.0-512BD4?logo=dotnet)](https://dotnet.microsoft.com/)
[![C%23](https://img.shields.io/badge/C%23-12-239120?logo=csharp)](https://learn.microsoft.com/dotnet/csharp/)
[![WPF](https://img.shields.io/badge/UI-WPF-0078D4?logo=windows)](https://learn.microsoft.com/dotnet/desktop/wpf/)
[![ASP.NET Core](https://img.shields.io/badge/API-ASP.NET%20Core-512BD4?logo=dotnet)](https://learn.microsoft.com/aspnet/core/)
[![Status](https://img.shields.io/badge/Status-MVP-orange)](#projektstatus)
[![License](https://img.shields.io/badge/Lizenz-Noch%20nicht%20festgelegt-lightgrey)](#lizenz)

## Übersicht

Die **FISI Admin Suite** ist eine modular aufgebaute Administrationsplattform für Windows- und Microsoft-Umgebungen. Wiederkehrende Aufgaben sollen über eine zentrale, moderne Benutzeroberfläche ausgeführt, kontrolliert und nachvollziehbar protokolliert werden.

Das Projekt verbindet eine WPF-Desktopanwendung mit einer ASP.NET-Core-Web-API und einem Worker Service. Administrative Aktionen werden nicht direkt in der Desktopanwendung ausgeführt, sondern als kontrollierte Aufträge an das Backend beziehungsweise an den Agenten übergeben.

## Projektstatus

Das Repository enthält einen **Entwicklungs-MVP**. Der aktuelle Stand verwendet Demonstrationsdaten und ist noch nicht für einen direkten Einsatz in produktiven Active-Directory-, Microsoft-365- oder Windows-Server-Umgebungen vorgesehen.

### Im MVP enthalten

- WPF-Anmeldefenster
- rollenbasierte Berechtigungen
- Dashboard mit Kennzahlen
- Benutzerübersicht und Benutzersuche
- Serverübersicht
- Benutzer-Onboarding-Endpunkt
- Benutzerentsperrung
- administrative Jobverwaltung
- API-Health-Check
- Worker-Service-Heartbeat
- gemeinsame Domain-Modelle

## Architektur

```text
┌──────────────────────────────────────┐
│ FisiAdmin.Desktop                    │
│ WPF, Login, Dashboard, Benutzer      │
└──────────────────┬───────────────────┘
                   │ HTTP / REST
                   ▼
┌──────────────────────────────────────┐
│ FisiAdmin.Api                        │
│ Authentifizierung, Rollen, Jobs      │
└───────────────┬──────────────────────┘
                │
        ┌───────┴────────┐
        ▼                ▼
┌───────────────┐  ┌──────────────────┐
│ Domain-Modelle│  │ FisiAdmin.Agent  │
│ Rollen, DTOs  │  │ Worker Service   │
└───────────────┘  └────────┬─────────┘
                             │
                             ▼
                  ┌────────────────────┐
                  │ Spätere Connectoren│
                  │ AD, M365, DNS, DHCP│
                  └────────────────────┘
```

## Solution-Struktur

```text
FisiAdminSuite/
├── .gitignore
├── README.md
├── FisiAdminSuite.sln
└── src/
    ├── FisiAdmin.Desktop/
    │   ├── App.xaml
    │   ├── LoginWindow.xaml
    │   ├── LoginWindow.xaml.cs
    │   ├── MainWindow.xaml
    │   └── MainWindow.xaml.cs
    ├── FisiAdmin.Api/
    │   └── Program.cs
    ├── FisiAdmin.Agent/
    │   ├── Program.cs
    │   └── appsettings.json
    └── FisiAdmin.Domain/
        └── Models.cs
```

## Projekte

### FisiAdmin.Desktop

Die WPF-Desktopanwendung stellt die grafische Benutzeroberfläche bereit.

**Aufgaben:**

- Benutzeranmeldung
- Dashboarddarstellung
- Benutzer- und Serversuche
- Rollenanzeige
- API-Kommunikation
- Abmeldung und Sitzungsverwaltung

### FisiAdmin.Api

Die ASP.NET-Core-Web-API enthält die zentrale Geschäfts- und Berechtigungslogik.

**Aufgaben:**

- Login und serverseitige Sitzungen
- Rollen- und Rechteprüfung
- Bereitstellung von Dashboarddaten
- Benutzer- und Server-Endpunkte
- Erzeugung administrativer Jobs
- Health Checks

### FisiAdmin.Agent

Der Worker Service ist die Grundlage für spätere administrative Hintergrundaufgaben.

**Geplante Aufgaben:**

- Active-Directory-Operationen
- geprüfte PowerShell-Aufträge
- Server-Monitoring
- Dateiserver-Aufgaben
- Auftragsstatus und Ergebnisrückmeldung

### FisiAdmin.Domain

Die Klassenbibliothek enthält gemeinsam verwendete Modelle und Rollendefinitionen.

**Beispiele:**

- `DirectoryUser`
- `ServerStatus`
- `DashboardSummary`
- `CreateUserRequest`
- `AdminJob`
- `AppRoles`

## Technologien

- C#
- .NET 8
- WPF
- ASP.NET Core Web API
- .NET Worker Service
- REST API
- rollenbasierte Zugriffskontrolle
- Visual Studio 2022

## Voraussetzungen

- Windows 10, Windows 11 oder Windows Server
- Visual Studio 2022
- Workload **.NET-Desktopentwicklung**
- Workload **ASP.NET und Webentwicklung**
- .NET 8 SDK
- .NET 8 Desktop Runtime

Installation prüfen:

```powershell
dotnet --version
dotnet --list-sdks
dotnet --list-runtimes
```

## Projekt lokal einrichten

### 1. Repository klonen

```powershell
git clone https://github.com/DEIN-BENUTZERNAME/FisiAdminSuite.git
cd FisiAdminSuite
```

Ersetze `DEIN-BENUTZERNAME` durch den tatsächlichen GitHub-Benutzernamen.

### 2. Abhängigkeiten wiederherstellen

```powershell
dotnet restore
```

### 3. Projekt kompilieren

```powershell
dotnet build
```

### 4. API starten

```powershell
dotnet run --project .\src\FisiAdmin.Api\FisiAdmin.Api.csproj
```

Die API verwendet im aktuellen MVP standardmäßig:

```text
http://localhost:5080
```

Health Check:

```text
http://localhost:5080/api/v1/health
```

### 5. Desktopanwendung starten

Öffne ein zweites Terminal:

```powershell
dotnet run --project .\src\FisiAdmin.Desktop\FisiAdmin.Desktop.csproj
```

### 6. Agent optional starten

Öffne ein drittes Terminal:

```powershell
dotnet run --project .\src\FisiAdmin.Agent\FisiAdmin.Agent.csproj
```

## Start in Visual Studio 2022

1. `FisiAdminSuite.sln` öffnen.
2. NuGet-Wiederherstellung abwarten.
3. Rechtsklick auf die Solution.
4. **Startprojekte konfigurieren** auswählen.
5. `FisiAdmin.Api` auf **Starten** setzen.
6. `FisiAdmin.Desktop` auf **Starten** setzen.
7. `FisiAdmin.Agent` und `FisiAdmin.Domain` auf **Ohne** setzen.
8. Projektmappe mit `Strg + Umschalt + B` erstellen.
9. Anwendung mit `F5` starten.

## Rollenmodell

### PlatformAdmin

- vollständiger Zugriff auf den MVP
- Benutzer anlegen und verwalten
- administrative Jobs anzeigen
- Systeme konfigurieren

### AdAdmin

- Benutzer anzeigen und anlegen
- Benutzer entsperren
- Gruppen und AD-Aufgaben verwalten
- relevante Jobs anzeigen

### ServiceDesk

- Benutzer suchen und anzeigen
- Benutzer entsperren
- keine privilegierte Benutzeranlage

### Auditor

- Daten und Vorgänge lesen
- Jobs und spätere Auditdaten prüfen
- keine administrativen Änderungen

### ReadOnly

- Dashboard anzeigen
- Benutzerübersicht anzeigen
- Serverstatus anzeigen
- keine Änderungen ausführen

## API-Endpunkte des MVP

```http
GET  /api/v1/health
POST /api/v1/auth/login
POST /api/v1/auth/logout
GET  /api/v1/auth/me
GET  /api/v1/dashboard
GET  /api/v1/users
GET  /api/v1/servers
GET  /api/v1/jobs
POST /api/v1/users/onboarding
POST /api/v1/users/{id}/unlock
```

## Geplante Module

- Benutzerverwaltung
- Gruppenverwaltung
- Active Directory
- DNS-Verwaltung
- DHCP-Verwaltung
- Microsoft 365
- Microsoft Entra ID
- Exchange Online
- Dateiserver
- Druckerverwaltung
- Netzwerkmanagement
- Gerätemanagement
- Ticketsystem
- Softwareverteilung
- Remote Support
- Monitoring
- Backupverwaltung
- Inventarisierung
- PowerShell Center
- Audit Logs
- KI-gestützte Administration

## Roadmap

### Phase 1: MVP-Grundlage

- [x] Solution-Struktur
- [x] WPF-Desktopclient
- [x] ASP.NET-Core-API
- [x] Worker Service
- [x] Demo-Login
- [x] Rollenprüfung
- [x] Dashboard und Beispieldaten

### Phase 2: Persistenz und Sicherheit

- [ ] SQL Server
- [ ] Entity Framework Core
- [ ] persistente Benutzer- und Rollenverwaltung
- [ ] Microsoft Entra ID und MSAL
- [ ] App Roles und MFA
- [ ] HTTPS und sichere Tokenablage
- [ ] unveränderbare Audit Logs

### Phase 3: Active Directory

- [ ] AD-Connector
- [ ] LDAPS
- [ ] Benutzer lesen und bearbeiten
- [ ] Benutzer entsperren
- [ ] Passwort zurücksetzen
- [ ] Gruppen verwalten
- [ ] OU-Browser

### Phase 4: Automatisierung

- [ ] Benutzer-Onboarding-Assistent
- [ ] Home-Verzeichnisse
- [ ] NTFS-Berechtigungen
- [ ] geprüfte PowerShell-Vorlagen
- [ ] Genehmigungsworkflow
- [ ] wiederaufnehmbare Jobschritte

### Phase 5: Cloud und Enterprise

- [ ] Microsoft Graph
- [ ] Microsoft 365
- [ ] Exchange Online
- [ ] Teams und OneDrive
- [ ] Monitoring und Benachrichtigungen
- [ ] Multi-Tenant-Unterstützung

## Sicherheit

> **Warnung:** Der aktuelle Stand ist ein Entwicklungs-MVP und darf nicht direkt mit produktiven Unternehmenssystemen verbunden werden.

Vor einem produktiven Einsatz müssen mindestens umgesetzt werden:

- Microsoft Entra ID und MSAL
- Multi-Faktor-Authentifizierung
- HTTPS
- sichere Token- und Secret-Verwaltung
- minimale Dienstkontoberechtigungen
- Agentenauthentifizierung mit Zertifikaten
- unveränderbare Audit-Protokolle
- Vier-Augen-Freigaben für kritische Vorgänge
- geprüfte und versionierte PowerShell-Vorlagen
- Eingabe- und Parameterprüfung
- Sicherheits-, Integrations- und Penetrationstests

Vertrauliche Daten dürfen nicht in Git eingecheckt werden. Dazu gehören:

- Passwörter
- Tokens
- API-Schlüssel
- Zertifikate und private Schlüssel
- produktive Verbindungszeichenfolgen
- interne Server- und Tenant-Konfigurationen

## GitHub Topics

Für dieses Repository werden folgende Topics empfohlen:

```text
csharp
dotnet-8
wpf
aspnet-core
windows-server
active-directory
powershell
rest-api
worker-service
role-based-access-control
system-administration
it-automation
server-management
fisi
```

Diese Topics werden auf GitHub im Bereich **About** über das Zahnrad eingetragen. GitHub Topics werden nicht automatisch aus der README übernommen.

## Beitragen

Beiträge sollten über einen eigenen Branch und einen Pull Request erfolgen.

Beispiel:

```powershell
git checkout -b feature/benutzersuche
git add .
git commit -m "feat: Benutzersuche erweitern"
git push -u origin feature/benutzersuche
```

Regeln für Beiträge:

1. Keine Geheimnisse oder produktiven Konfigurationen committen.
2. Rollenrechte immer im Backend prüfen.
3. Neue administrative Aktionen protokollieren.
4. Code vor einem Pull Request vollständig kompilieren.
5. Sicherheitskritische Änderungen zusätzlich prüfen lassen.

## Lizenz

Für das Projekt wurde noch keine endgültige Open-Source-Lizenz festgelegt. Bis eine Lizenzdatei hinzugefügt wird, bleiben alle Rechte beim Repository-Inhaber.

## Haftungsausschluss

Dieses Projekt ist ein Lern- und Entwicklungsprojekt. Die Verwendung erfolgt auf eigene Verantwortung. Vor der Nutzung in einer realen IT-Umgebung sind eine Sicherheitsprüfung, eine Berechtigungsprüfung und umfassende Tests in einer isolierten Testumgebung erforderlich.
