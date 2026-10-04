/** Joins CSS classes and skips false/null/undefined: cn('a', isActive && 'b') */

export function cn(
  ...classes: Array<string | false | null | undefined>
): string {
  return classes.filter(Boolean).join(" ");
}
