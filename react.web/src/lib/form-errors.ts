import type { FieldValues, Path, UseFormSetError } from "react-hook-form";
import { ApiError } from "@/lib/api-client";
/**
 * Shows the API's 400 validation errors under the matching form fields.
 * Anything that does not belong to a field becomes a general form error (errors.root.server).
 */
export function applyServerErrors<T extends FieldValues>(
  error: unknown,
  setError: UseFormSetError<T>,
  fieldNames: readonly Path<T>[],
): void {
  if (error instanceof ApiError && error.status === 400) {
    let matchedAnyField = false;
    for (const [field, messages] of Object.entries(error.fieldErrors)) {
      if ((fieldNames as readonly string[]).includes(field)) {
        setError(field as Path<T>, { type: "server", message: messages[0] });
        matchedAnyField = true;
      }
    }
    if (matchedAnyField) return;
  }
  const message =
    error instanceof ApiError ? error.message : "Något gick fel. Försök igen.";
  setError("root.server", { type: "server", message });
}
