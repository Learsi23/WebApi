import type { ComponentProps } from "react";
import { cn } from "@/lib/cn";
import { Spinner } from "./Spinner";

type ButtonVariant = "primary" | "secondary" | "danger" | "ghost";

const variantClasses: Record<ButtonVariant, string> = {
  primary:
    "bg-brand-600 text-white shadow-xs hover:bg-brand-700 focus-visible:outline-brand-600",
  secondary:
    "bg-white text-slate-700 shadow-xs ring-1 ring-inset ring-slate-300 hover:bg-slate50",
  danger:
    "bg-red-600 text-white shadow-xs hover:bg-red-700 focus-visible:outline-red-600",
  ghost: "text-slate-600 hover:bg-slate-100 hover:text-slate-900",
};

type ButtonProps = ComponentProps<"button"> & {
  variant?: ButtonVariant;
  isLoading?: boolean;
};

export function Button({
  variant = "primary",
  isLoading = false,
  type = "button", // not "submit": a button inside a form must opt in to submitting it
  disabled,
  className,
  children,
  ...props
}: ButtonProps) {
  return (
    <button
      type={type}
      disabled={disabled || isLoading}
      className={cn(
        "inline-flex items-center justify-center gap-2 rounded-md px-3.5 py-2 text-sm font-semibold transition-colors",
        "focus-visible:outline-2 focus-visible:outline-offset-2 disabled:cursor-not-allowed disabled:opacity-50",
        variantClasses[variant],
        className,
      )}
      {...props}
    >
      {isLoading && <Spinner className="size-4" />}
      {children}
    </button>
  );
}
