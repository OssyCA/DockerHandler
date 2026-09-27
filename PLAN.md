# Docker Remote Control

Ett .NET-API som styr Docker-containers på en VPS, med en React Native-app som
klient. CV-projekt med syftet att visa fullstack-kedjan: mobil klient, .NET-backend,
containerisering, deployment och drift.

**Varför bygga detta när Portainer finns:** för att förstå Docker Engine API,
autentisering och deployment i praktiken — och för att ha kontrollen i mobilen
utan att öppna ett webb-UI.

---

## Status

| Fas | Innehåll | Status |
|---|---|---|
| 0 | Test-API containeriserat, VPS-uppsättning verifierad | Klar |
| 1 | Docker Controller API, lokalt, utan auth | Pågår |
| 2 | Säkerhet, HTTPS, domän | Ej påbörjad |
| 3 | React Native-app | Ej påbörjad |
| 4 | Live-metrics | Ej påbörjad |
| 5 | CI/CD | Ej påbörjad |

---

## Fas 0 — Grund (klar)

- Test-API i .NET 10, containeriserat med multi-stage Dockerfile
- Image byggd och verifierad lokalt
- Hetzner VPS (CX23, Ubuntu, ~7 €/mån) uppsatt
- SSH med nyckelautentisering
- Docker installerat på servern
- API klonat, byggt och kört på servern, nåbart på publik IP
- Bitwarden för lösenord och återställningskoder

**Lärdomar att ta med:**
- Docker kringgår UFW — portar öppnas i iptables oavsett brandväggsregler
- Hetzner Console har avvikande tangentbordslayout, undvik specialtecken där
- Läs serverns egen logg (`journalctl -u ssh`) före allt annat vid SSH-problem

---

## Fas 1 — Docker Controller API

Mål: fungerande API lokalt mot Docker Desktop, utan auth.

### Arkitektur

```
DockerController.Api/      endpoints, DI, Scalar, exception handling
DockerController.Core/     interfaces, modeller, validering, egna exceptions
DockerController.Docker/   implementation mot Docker Engine API
```

Core definierar kontrakten. Docker implementerar. Api känner bara till Core.

### Endpoints

- `GET /containers` — lista hanterade containers
- `GET /containers/{id}` — detaljer
- `POST /containers/{id}/start`
- `POST /containers/{id}/stop` — valfri timeout (0–300 s)
- `POST /containers/{id}/restart`
- `GET /images`
- `GET /health` — inkl. daemon-anslutning

### Tekniska beslut

**Docker-klient:** Docker.DotNet.Enhanced (aktiv fork, Engine API v29.x).
Originalet Docker.DotNet är övergivet sedan 2023.

**Filtrering:** label `managed-by=docker-controller` som primär mekanism,
plus `DeniedNames` i appsettings som alltid blockerar. Controllerns egen
container ska ligga i denylistan.

**Streaming:** `IAsyncEnumerable` hela vägen från Docker-lagret till endpoint.
Förberedelse för fas 4.

**Fel:** Result för förväntade utfall, exceptions för oväntade. Se nedan.

**Dokumentation:** Scalar, redirect från roten.

### Felhanteringsstrategi

Skiljelinjen går mellan förväntade utfall och oväntade fel.

**Result för domänutfall.** Core returnerar `Result<T>` när utfallet är en
normal del av driften:

- Container hittas inte
- Container är inte hanterad (denied)
- Ogiltigt tillstånd (stoppa en redan stoppad)
- Valideringsfel

Endpoints matchar på resultatet och mappar till statuskod. Fördelen: felen
syns i signaturen, går att testa utan try/catch, och ingen stack unwinding
för något som inträffar rutinmässigt.

Särskilt viktigt för `denied` — filtreringslogiken är säkerhetsmodellen, och
ett explicit returvärde är lättare att granska och täcka med tester än ett
undantag som kastas någonstans i anropskedjan.

**Exceptions för infrastruktur.** Kastas bara när något faktiskt är trasigt:

- Daemon onåbar → 503
- Behörighet till socket saknas → 503
- Deserialiseringsfel → 500
- Allt oförutsett → 500, utan interna detaljer i svaret men fullt loggat

**Global handler som skyddsnät.** `IExceptionHandler` med ProblemDetails
(RFC 7807) finns alltid, men ska helst aldrig behöva göra något intressant.
Inga Docker.DotNet-exceptions får läcka ut till Api-lagret — de översätts
i Docker-projektet.

Mappning från Result till statuskod sker på ett ställe, inte utspritt i
varje endpoint.

### Klart när

- Tre testcontainrar, varav en utan label, filtreras korrekt
- Start/stop/restart fungerar mot Docker Desktop
- Scalar nåbar på `/`
- Filtreringslogiken enhetstestad utan körande Docker

---

## Fas 2 — Säkerhet

Först när fas 1 fungerar lokalt.

- API-nyckel i header, med stöd för **flera giltiga nycklar samtidigt**
  så rotation går utan nedtid
- Rate limiting per nyckel
- Egen användare på servern istället för root
- SSH: `PermitRootLogin no`, `PasswordAuthentication no`
- Domän + Caddy för automatisk HTTPS
- Container bunden till `127.0.0.1:8080` — endast Caddy exponeras
- Hetzner Cloud Firewall (ligger utanför värden, kringgås inte av Docker)
- Hemligheter via miljövariabler, `.env` i `.gitignore`, `.env.example` i repot
- Maskera nycklar i loggning

**Utan HTTPS är API-nyckeln meningslös** — headern går i klartext. Domän och
Caddy är därför inte kosmetik utan en förutsättning.

---

## Fas 3 — React Native-app

- Lista containers med status
- Start/stop/restart
- Pull-to-refresh
- API-nyckel i secure storage, inte i koden

Notera: en nyckel i en mobilapp går alltid att extrahera. Acceptabelt så länge
appen bara körs av dig. Vill du bredda användningen krävs inloggning och JWT.

---

## Fas 4 — Live-metrics

Det tekniskt intressantaste momentet, och den bästa intervjuhistorien.

- Docker stats-endpointen streamar kontinuerligt
- `IAsyncEnumerable` eller SignalR mot klienten
- Diagram i appen som uppdateras utan att lagga
- Sampling och throttling — annars äts batteri och bandbredd

Kallas medvetet metrics, inte benchmarks. Riktig benchmarking är ett eget projekt.

---

## Fas 5 — CI/CD och drift

- `compose.yaml` i repot istället för `docker run` för hand
- GitHub Actions → SSH-deploy → `docker compose up -d --build`
- Cloud config på Hetzner så en ny server bootar färdigkonfigurerad
- Kostnadslarm i Hetzner Billing
- `restart: unless-stopped` på alla tjänster
- Rutin för säkerhetsuppdateringar (`unattended-upgrades`)

---

## Säkerhetsmodell

Åtkomst till `/var/run/docker.sock` **motsvarar root på värden**. Den som når
socketen kan starta en container med värdens filsystem monterat.

Följder:

- Socketen exponeras aldrig mot nätverket
- `:ro` på volymen skyddar inte — API-anropen går igenom ändå
- Whitelist-logiken är hela säkerhetsmodellen, inte en detalj
- Controllern ska vara ett litet, granskbart projekt

---

## Driftsanteckningar

- Radera servern vid längre pauser, stäng inte bara av — avstängd server
  debiteras fullt
- Kontrollera Primary IPs efter radering, en kvarliggande IPv4 kostar
- Deploy key ≠ personlig SSH-nyckel
- Windows: `npipe://./pipe/docker_engine`. Linux: `unix:///var/run/docker.sock`

---

## Öppna frågor

- Ska `GET /containers` kunna visa ohanterade containers bakom en flagga?
- Auto-restart vid failed health check via `BackgroundService` — fas 2 eller 4?
- Stop-timeout per container via label, eller bara globalt?
