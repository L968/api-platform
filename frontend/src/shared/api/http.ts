const portalApiUrl = import.meta.env.VITE_PORTAL_API_URL ?? "http://localhost:5019";

export class ApiError extends Error {
  constructor(
    public readonly status: number,
    message: string,
  ) {
    super(message);
    this.name = "ApiError";
  }
}

export async function http<T>(path: string, init: RequestInit = {}): Promise<T> {
  const headers = new Headers(init.headers);
  headers.set("Accept", "application/json");

  if (init.body && !headers.has("Content-Type")) {
    headers.set("Content-Type", "application/json");
  }

  const response = await fetch(`${portalApiUrl}${path}`, {
    ...init,
    headers,
    credentials: "include",
  });

  if (!response.ok) {
    throw new ApiError(response.status, await readError(response));
  }

  if (response.status === 204) {
    return undefined as T;
  }

  return (await response.json()) as T;
}

async function readError(response: Response): Promise<string> {
  const fallback = response.status === 401
    ? "Your session has expired. Please sign in again."
    : "We couldn't complete this request.";

  const contentType = response.headers.get("content-type");
  if (contentType?.includes("application/json")) {
    const body: unknown = await response.json();
    if (typeof body === "string") {
      return body;
    }

    if (body && typeof body === "object") {
      const problem = body as { detail?: string; title?: string };
      return problem.detail ?? problem.title ?? fallback;
    }
  }

  const message = await response.text();
  return message || fallback;
}
