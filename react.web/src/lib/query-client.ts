import { MutationCache, QueryCache, QueryClient } from "@tanstack/react-query";
import { ApiError } from "@/lib/api-client";
import { authKeys } from "@/features/auth/api";
export const queryClient = new QueryClient({
  // If ANY request answers 401 the session has expired: forget the user,
  // and RequireAuth sends them to the login page.
  queryCache: new QueryCache({ onError: handleUnauthorized }),
  mutationCache: new MutationCache({ onError: handleUnauthorized }),
  defaultOptions: {
    queries: {
      staleTime: 30_000, // data younger than 30 s is reused without asking the API again
      refetchOnWindowFocus: false,
      // Retry network errors and 5xx, but never 4xx: asking again will not change the answer.
      retry: (failureCount, error) =>
        !(error instanceof ApiError && error.status < 500) && failureCount < 2,
    },
  },
});

function handleUnauthorized(error: unknown) {
  if (error instanceof ApiError && error.status === 401) {
    queryClient.setQueryData(authKeys.me, null);
  }
}
