export const applicationTypeLabels = ["Web", "ERP", "Job", "Mobile"] as const;

export function formatApplicationType(type: number): string {
  return applicationTypeLabels[type] ?? "Desconhecido";
}

export function formatDate(value: string | null | undefined): string {
  if (!value) {
    return "Sem expiração";
  }

  return new Intl.DateTimeFormat("pt-BR", { dateStyle: "medium" }).format(new Date(value));
}

export function formatNumber(value: number): string {
  return new Intl.NumberFormat("pt-BR").format(value);
}

export function formatAmount(value: number, maximumFractionDigits = 2): string {
  return new Intl.NumberFormat("pt-BR", {
    minimumFractionDigits: 2,
    maximumFractionDigits,
  }).format(value);
}

export function errorMessage(error: unknown): string {
  return error instanceof Error ? error.message : "Ocorreu um erro inesperado.";
}
