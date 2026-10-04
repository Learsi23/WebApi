import { cn } from "@/lib/cn";

/** Shared look for <input> and <select>: grey ring, blue when focused, red on error. */
export function fieldClasses(hasError: boolean): string {
  return cn(
    "mt-1 block w-full rounded-md border-0 bg-white px-3 py-2 text-sm shadow-xs ring-1 ring-inset",
    "focus:ring-2 focus:ring-inset focus:outline-none",
    hasError
      ? "ring-red-400 focus:ring-red-500"
      : "ring-slate-300 focus:ring-brand-600",
  );
}
