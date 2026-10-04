/** The error body ASP.NET Core sends (RFC 9457 ProblemDetails). */
export type ProblemDetails = {
  type?: string;
  title?: string;
  status?: number;
  detail?: string;
  traceId?: string;
  /** Only on 400 validation errors: { "email": ["'Email' is not a valid email address."] } */
  errors?: Record<string, string[]>;
};
/** Thrown for every response that is not 2xx, so callers can check error.status. */
export class ApiError extends Error {
  readonly status: number;
  readonly problem: ProblemDetails | null;
  constructor(status: number, problem: ProblemDetails | null) {
    super(
      problem?.detail ??
        problem?.title ??
        `Request failed with status ${status}`,
    );
    this.name = "ApiError";
    this.status = status;
    this.problem = problem;
  }
  get fieldErrors(): Record<string, string[]> {
    return this.problem?.errors ?? {};
  }
}

type RequestOptions = {
  method?: "GET" | "POST" | "PUT" | "DELETE";
  /** A plain object is sent as JSON; FormData is sent as multipart/form-data. */
  body?: unknown;
  signal?: AbortSignal;
};
/**
 * The ONLY place that calls fetch(). Every API call in the app goes through here,
 * so JSON handling and error handling are written once.
 */
export async function apiFetch<T>(
  path: string,
  { method = "GET", body, signal }: RequestOptions = {},
): Promise<T> {
  const isFormData = body instanceof FormData;
  const response = await fetch(`/api${path}`, {
    method,
    signal,
    headers: {
      Accept: "application/json",
      // For FormData the browser sets Content-Type itself (with the multipart boundary).
      ...(body !== undefined &&
        !isFormData && { "Content-Type": "application/json" }),
    },
    body:
      body === undefined ? undefined : isFormData ? body : JSON.stringify(body),
  });
  if (!response.ok) {
    throw new ApiError(response.status, await readProblem(response));
  }
  // 204 No Content has no body to parse.
  if (response.status === 204) {
    return undefined as T;
  }
  return (await response.json()) as T;

  async function readProblem(
    response: Response,
  ): Promise<ProblemDetails | null> {
    const contentType = response.headers.get("content-type") ?? "";
    if (!contentType.includes("json")) {
      return null;
    }
    try {
      return (await response.json()) as ProblemDetails;
    } catch {
      return null;
    }
  }
}
/** Builds "?search=x&page=2", skipping empty values. */
export function toQueryString(
  params: Record<string, string | number | null | undefined>,
): string {
  const searchParams = new URLSearchParams();
  for (const [key, value] of Object.entries(params)) {
    if (value !== null && value !== undefined && value !== "") {
      searchParams.set(key, String(value));
    }
  }
  const query = searchParams.toString();
  return query ? `?${query}` : "";
}
