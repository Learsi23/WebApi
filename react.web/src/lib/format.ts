// Intl formatters are expensive to create, so we create them once and reuse them.

const currencyFormatter = new Intl.NumberFormat("sv-SE", {
  style: "currency",
  currency: "SEK",
  maximumFractionDigits: 0,
});
const dateFormatter = new Intl.DateTimeFormat("sv-SE", { dateStyle: "medium" });
/** 55000 -> "55 000 kr" */
export function formatCurrency(value: number): string {
  return currencyFormatter.format(value);
}
/** "2026-01-01T00:00:00+00:00" -> "1 jan. 2026" (in the user's time zone) */
export function formatDate(isoDate: string): string {
  return dateFormatter.format(new Date(isoDate));
}

/** "Åsa Öberg" -> "ÅÖ" */
export function getInitials(fullName: string): string {
  return fullName
    .split(" ")
    .filter(Boolean)
    .slice(0, 2)
    .map((part) => part[0].toUpperCase())
    .join("");
}
