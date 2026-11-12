import { ApiError, NetworkError } from "./client";

export type Problem = {
  message: string;
  remedy: "settings" | "retry" | null;
};

export function describe(err: unknown): Problem {
  if (err instanceof NetworkError) {
    return { message: err.message, remedy: "retry" };
  }

  if (!(err instanceof ApiError)) {
    return { message: "Ett okänt fel inträffade.", remedy: "retry" };
  }

  switch (err.code) {
    case "unauthorized":
      return { message: "Nyckeln godtas inte.", remedy: "settings" };
    case "rate_limited":
      return {
        message:
          err.retryAfterSeconds === null
            ? "För många anrop. Försök igen om en stund."
            : `För många anrop. Försök igen om ${err.retryAfterSeconds} sekunder.`,
        remedy: "retry",
      };
    case "docker_daemon_unavailable":
      return { message: "Servern når inte Docker.", remedy: "retry" };
    case "not_found":
      return {
        message: "Containern finns inte, eller hanteras inte av controllern.",
        remedy: null,
      };
    case null:
      if (err.status === 502 || err.status === 503 || err.status === 504) {
        return { message: "Ingen kontakt med servern.", remedy: "retry" };
      }
      return { message: err.message, remedy: "retry" };
    default:
      return { message: err.message, remedy: "retry" };
  }
}
