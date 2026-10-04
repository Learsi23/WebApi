import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { ApiError, apiFetch } from "@/lib/api-client";
import type { CurrentUser, LoginRequest } from "@/types/api";

export const authKeys = {
  me: ["auth", "me"] as const,
};

/** null = not logged in. A 401 here is an answer, not an error. */
async function getCurrentUser(): Promise<CurrentUser | null> {
  try {
    return await apiFetch<CurrentUser>("/auth/me");
  } catch (error) {
    if (error instanceof ApiError && error.status === 401) return null;
    throw error;
  }
}

export function useCurrentUser() {
  return useQuery({
    queryKey: authKeys.me,
    queryFn: getCurrentUser,
    staleTime: Infinity, // only changes on login/logout, which update the cache themselves
  });
}

export function useIsAdmin(): boolean {
  const { data: user } = useCurrentUser();
  return user?.roles.includes("Admin") ?? false;
}

export function useLogin() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (credentials: LoginRequest) =>
      apiFetch<CurrentUser>("/auth/login", {
        method: "POST",
        body: credentials,
      }),
    onSuccess: (user) => queryClient.setQueryData(authKeys.me, user),
  });
}

export function useLogout() {

  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: () => apiFetch<void>("/auth/logout", { method: "POST" }),
    onSuccess: () => {
      // 1. "Nobody is logged in": RequireAuth now redirects to /login.
      queryClient.setQueryData(authKeys.me, null);
      // 2. Forget all other cached data: the next user must not see the previous user's data.
      //    (Not queryClient.clear(): that would also remove the "me" query that the
      //    components are watching, and they would never hear that the user is gone.)
      queryClient.removeQueries({
        predicate: (query) => query.queryKey[0] !== "auth",
      });
    },
  });
}
