import { describe, expect, it } from "vitest";
import { formatApplicationType, formatAmount, formatNumber } from "./format";

describe("formatters", () => {
  it("traduz os tipos enviados numericamente pelo backend", () => {
    expect(formatApplicationType(0)).toBe("Web");
    expect(formatApplicationType(1)).toBe("ERP");
    expect(formatApplicationType(2)).toBe("Job");
    expect(formatApplicationType(3)).toBe("Mobile");
  });

  it("formata números para o portal em pt-BR", () => {
    expect(formatNumber(1250)).toBe("1.250");
    expect(formatAmount(12.5)).toBe("12,50");
  });
});
