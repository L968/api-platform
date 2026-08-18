import { describe, expect, it } from "vitest";
import { formatApplicationType, formatAmount, formatCurrency, formatDate, formatNumber } from "./format";

describe("formatters", () => {
  it("maps the numeric application types returned by the backend", () => {
    expect(formatApplicationType(0)).toBe("Web");
    expect(formatApplicationType(1)).toBe("ERP");
    expect(formatApplicationType(2)).toBe("Job");
    expect(formatApplicationType(3)).toBe("Mobile");
  });

  it("formats portal numbers using en-US", () => {
    expect(formatNumber(1250)).toBe("1,250");
    expect(formatAmount(12.5)).toBe("12.50");
    expect(formatCurrency(12.5)).toBe("$12.50");
    expect(formatCurrency(0.01, 4)).toBe("$0.01");
  });

  it("formats date-only values without shifting the calendar day", () => {
    expect(formatDate("2026-08-18")).toBe("Aug 18, 2026");
  });
});
