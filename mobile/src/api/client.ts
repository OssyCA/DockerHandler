import type { ContainerDetails, ContainerSummary, ErrorCode } from "./types";

export type ApiConfig = {
  baseUrl: string;
  apiKey: string;
};

export type ContainerAction = "start" | "stop" | "restart";

export class NetworkError extends Error {}

export class ApiError extends Error {
  constructor(
    readonly status: number,
    readonly code: ErrorCode | null,
    readonly retryAfterSeconds: number | null,
    message: string,
  ) {
    super(message);
  }
}

export async function listContainers(config: ApiConfig): Promise<ContainerSummary[]> {
  return readJson<ContainerSummary[]>(await send(config, "/containers", "GET"));
}

export async function getContainer(config: ApiConfig, id: string): Promise<ContainerDetails> {
  return readJson<ContainerDetails>(
    await send(config, `/containers/${encodeURIComponent(id)}`, "GET"),
  );
}

export async function runAction(
  config: ApiConfig,
  id: string,
  action: ContainerAction,
): Promise<void> {
  await send(config, `/containers/${encodeURIComponent(id)}/${action}`, "POST");
}

async function send(
  config: ApiConfig,
  path: string,
  method: "GET" | "POST",
): Promise<Response> {
  const abortController = new AbortController();
  const timeoutId = setTimeout(() => abortController.abort(), 10000);
  let response: Response;

  try {
    response = await fetch(`${config.baseUrl}${path}`, {
      method,
      headers: {
        Accept: "application/json",
        "X-Api-Key": config.apiKey,
      },
      signal: abortController.signal,
    });
  } catch (error) {
    if (error instanceof Error && error.name === "AbortError") {
      throw new NetworkError("Servern svarade inte inom tio sekunder.");
    }

    throw new NetworkError("Ingen kontakt med servern.");
  } finally {
    clearTimeout(timeoutId);
  }

  if (!response.ok) {
    throw await toApiError(response);
  }

  return response;
}

async function readJson<T>(response: Response): Promise<T> {
  try {
    return await response.json();
  } catch {
    throw new ApiError(response.status, null, null, "Servern svarade något som inte är JSON.");
  }
}

async function toApiError(response: Response): Promise<ApiError> {
  const retryAfter = Number.parseInt(response.headers.get("Retry-After") ?? "", 10);
  const problem = await readProblem(response);

  return new ApiError(
    response.status,
    problem?.code ?? null,
    Number.isNaN(retryAfter) ? null : retryAfter,
    problem?.detail ?? problem?.title ?? `Servern svarade ${response.status}.`,
  );
}

async function readProblem(
  response: Response,
): Promise<{ code?: ErrorCode; title?: string; detail?: string } | null> {
  try {
    return await response.json();
  } catch {
    return null;
  }
}
