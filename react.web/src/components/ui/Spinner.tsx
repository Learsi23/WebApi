import { cn } from "@/lib/cn";

export function Spinner({ className }: { className?: string }) {
  return (
    <span
      role="status"
      aria-label="Laddar"
      className={cn(
        "inline-block size-5 animate-spin rounded-full border-2 border-current bordert-transparent",
        className,
      )}
    />
  );
}

/** Centered spinner for a whole page section while data loads. */
export function PageSpinner() {
  return (
    <div className="flex justify-center py-16">
      <Spinner className="size-8 text-brand-600" />
    </div>
  );
}
