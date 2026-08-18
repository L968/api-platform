import { afterEach, describe, expect, it, vi } from "vitest";
import { ApiError, http } from "./http";

describe("http", () => {
  afterEach(() => {
    vi.restoreAllMocks();
  });

  it("envia JSON e inclui o cookie da sessão", async () => {
    const fetchMock = vi.spyOn(globalThis, "fetch").mockResolvedValue(
      new Response(JSON.stringify({ id: "app-1" }), {
        status: 200,
        headers: { "content-type": "application/json" },
      }),
    );

    const result = await http<{ id: string }>("/applications", {
      method: "POST",
      body: JSON.stringify({ name: "ERP" }),
    });

    expect(result).toEqual({ id: "app-1" });
    expect(fetchMock).toHaveBeenCalledWith(
      "http://localhost:5019/applications",
      expect.objectContaining({
        method: "POST",
        credentials: "include",
      }),
    );

    const request = fetchMock.mock.calls[0]?.[1];
    expect(new Headers(request?.headers).get("Content-Type")).toBe("application/json");
  });

  it("aceita respostas sem conteúdo", async () => {
    vi.spyOn(globalThis, "fetch").mockResolvedValue(new Response(null, { status: 204 }));

    await expect(http<void>("/auth/logout", { method: "POST" })).resolves.toBeUndefined();
  });

  it("converte erros HTTP em ApiError", async () => {
    vi.spyOn(globalThis, "fetch").mockResolvedValue(
      new Response(JSON.stringify({ detail: "Scope inválido." }), {
        status: 400,
        headers: { "content-type": "application/json" },
      }),
    );

    await expect(http("/credentials")).rejects.toEqual(
      new ApiError(400, "Scope inválido."),
    );
  });
});
