# DockerHandler

A small HTTP API for listing, starting, stopping and restarting Docker containers on a single server, with a companion mobile app. The API only acts on containers that carry a specific label, so everything else on the host stays out of reach.

Built with .NET 10 minimal APIs, Caddy and Expo (React Native).

## Architecture

```
mobile app  →  Caddy (HTTPS)  →  API (:8080)  →  /var/run/docker.sock
```

Caddy is the only service with published ports (80 and 443) and obtains its TLS certificate automatically. The API is reachable only through Caddy on the internal compose network.

| Path | Contents |
|---|---|
| `src/DockerController.Api` | Endpoints, API key authentication, rate limiting, error handling |
| `src/DockerController.Core` | Models, access policy, result types, abstractions |
| `src/DockerController.Docker` | Docker client implementation |
| `tests/DockerController.Core.Tests` | Unit tests |
| `mobile/` | Expo app  UNDER DEVELOPMENT Under branch Stage 3 |

## Endpoints

| Method | Path | Description |
|---|---|---|
| GET | `/containers` | List containers |
| GET | `/containers/{id}` | Details for one container |
| POST | `/containers/{id}/start` | Start. Returns 409 if already running |
| POST | `/containers/{id}/stop` | Stop. Optional `timeout` query parameter in seconds. Returns 409 if already stopped |
| POST | `/containers/{id}/restart` | Restart, using the configured default stop timeout |
| GET | `/images` | List images on the host |
| GET | `/health` | Returns 503 when the Docker daemon is unreachable |
| GET | `/alive` | Liveness check. Returns 204, no key required |

Errors are returned as problem details (`application/problem+json`).

## Security model

- **API keys.** Every endpoint except `/alive` requires an `X-Api-Key` header. Keys are at least 32 characters and are supplied through environment variables, never through `appsettings.json`. The app refuses to start without at least one key.
- **Label allowlist.** Only containers labelled `managed-by=docker-controller` can be controlled. The label key and value are configurable.
- **Deny list.** Container names in `Docker__DeniedNames` are always refused. The controller's own container must be listed there.
- **Rate limiting.** 60 requests per minute per API key, 10 per minute per IP address for requests without a valid key.
- **Logging.** Logs record the `Id` of the key that made a request, never its value.
- **Non-root.** The API process runs as an unprivileged user inside its container.

### Known limitation

The API container mounts the Docker socket. Anyone who compromises that container effectively controls the Docker host. Run this only on a server where that trade-off is acceptable, and consider placing a socket proxy between the API and the socket.

## Configuration

All configuration is passed as environment variables. A double underscore denotes a nested key. See [.env.example](.env.example) for the full template.

| Variable | Description |
|---|---|
| `DOMAIN` | Public hostname Caddy serves and requests a certificate for |
| `DOCKER_GID` | Group id that owns the Docker socket on the host. On Ubuntu: `getent group docker \| cut -d: -f3` |
| `Docker__Endpoint` | Docker socket address, `unix:///var/run/docker.sock` on Linux |
| `Docker__ManagedLabelKey` | Label key a container must have to be managed |
| `Docker__ManagedLabelValue` | Label value a container must have to be managed |
| `Docker__DefaultStopTimeoutSeconds` | Seconds between SIGTERM and SIGKILL when stopping |
| `Docker__DeniedNames__0` | Container names that can never be controlled. Indexed array |
| `Auth__Keys__0__Id` | Free-form label for the key. Appears in logs |
| `Auth__Keys__0__Value` | The secret. Generate with `openssl rand -hex 32` |

To rotate a key, add `Auth__Keys__1__Id` and `Auth__Keys__1__Value`, restart, switch clients over, then remove the old entry.

## Deploying

Requires Docker with the compose plugin and a DNS record pointing at the server.

```bash
git clone https://github.com/OssyCA/DockerHandler.git
cd DockerHandler
cp .env.example .env
# fill in DOMAIN, DOCKER_GID and at least one API key
docker compose up -d --build
```

Mark the containers you want to control:

```yaml
services:
  my-service:
    labels:
      managed-by: docker-controller
```

Verify:

```bash
curl -i https://your-domain.example/alive
curl -H "X-Api-Key: <your key>" https://your-domain.example/containers
```

`.env` contains secrets and is ignored by git. Keep it readable only by the user that runs compose.

## Running locally

Requires the .NET 10 SDK and Docker Desktop. The default configuration targets the Docker Desktop named pipe on Windows.

```powershell
$env:Auth__Keys__0__Id = "local"
$env:Auth__Keys__0__Value = "<at least 32 characters>"
dotnet run --project src/DockerController.Api
```

The API listens on `http://localhost:5064`.

## Tests

```bash
dotnet test
```

## Mobile app

```bash
cd mobile
cp .env.example .env.local
# set EXPO_PUBLIC_API_URL to your server's address
npm install
npm start
```

The API key is entered in the app and kept in the device's secure storage.
