export const applicationTypeLabels = ["Web", "ERP", "Job", "Mobile"] as const;

export function formatApplicationType(type: number): string {
  return applicationTypeLabels[type] ?? "Unknown";
}

export function formatDate(value: string | null | undefined): string {
  if (!value) {
    return "Never expires";
  }

  const date = /^\d{4}-\d{2}-\d{2}$/.test(value)
    ? new Date(`${value}T00:00:00`)
    : new Date(value);

  return new Intl.DateTimeFormat("en-US", { dateStyle: "medium" }).format(date);
}

export function formatNumber(value: number): string {
  return new Intl.NumberFormat("en-US").format(value);
}

export function formatAmount(value: number, maximumFractionDigits = 2): string {
  return new Intl.NumberFormat("en-US", {
    minimumFractionDigits: 2,
    maximumFractionDigits,
  }).format(value);
}

export function formatCurrency(value: number, maximumFractionDigits = 2): string {
  return new Intl.NumberFormat("en-US", {
    style: "currency",
    currency: "USD",
    currencyDisplay: "narrowSymbol",
    minimumFractionDigits: 2,
    maximumFractionDigits,
  }).format(value);
}

export function errorMessage(error: unknown): string {
  return error instanceof Error ? error.message : "Something went wrong.";
}
