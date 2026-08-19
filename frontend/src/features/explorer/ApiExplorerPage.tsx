import { useState } from "react";
import { Check, Code2, Copy, Play, ShieldCheck } from "lucide-react";
import { Button, Card, Input, PageHeader, Select } from "../../shared/components/ui";

type HttpMethod = "GET" | "POST";

interface ExplorerEndpoint {
  id: string;
  api: string;
  method: HttpMethod;
  path: string;
  scope: string;
  description: string;
  body?: string;
}

interface ExplorerResponse {
  status: number;
  duration: number;
  body: string;
}

const endpoints: ExplorerEndpoint[] = [
  { id: "orders-list", api: "Orders", method: "GET", path: "/orders", scope: "orders.read", description: "List available orders." },
  { id: "orders-detail", api: "Orders", method: "GET", path: "/orders/{id}", scope: "orders.read", description: "Get one order by ID." },
  { id: "orders-create", api: "Orders", method: "POST", path: "/orders", scope: "orders.write", description: "Create a new order.", body: '{\n  "item": "Widget D",\n  "quantity": 1\n}' },
  { id: "payments-list", api: "Payments", method: "GET", path: "/payments", scope: "payments.read", description: "List available payments." },
  { id: "payments-detail", api: "Payments", method: "GET", path: "/payments/{id}", scope: "payments.read", description: "Get one payment by ID." },
  { id: "payments-create", api: "Payments", method: "POST", path: "/payments", scope: "payments.write", description: "Create a new payment.", body: '{\n  "amount": 99.90,\n  "currency": "BRL"\n}' },
];

export function ApiExplorerPage() {
  const [endpointId, setEndpointId] = useState(endpoints[0].id);
  const [apiKey, setApiKey] = useState("");
  const [resourceId, setResourceId] = useState("1");
  const [body, setBody] = useState("");
  const [response, setResponse] = useState<ExplorerResponse | null>(null);
  const [error, setError] = useState("");
  const [copied, setCopied] = useState(false);

  const endpoint = endpoints.find((item) => item.id === endpointId) ?? endpoints[0];
  const path = endpoint.path.replace("{id}", resourceId || "1");
  const requestBody = endpoint.body ?? body;

  function selectEndpoint(id: string) {
    const next = endpoints.find((item) => item.id === id) ?? endpoints[0];
    setEndpointId(id);
    setBody(next.body ?? "");
    setResponse(null);
    setError("");
  }

  async function sendRequest() {
    setError("");
    setResponse(null);
    setCopied(false);

    if (!apiKey.trim()) {
      setError("Enter an API key before sending a request.");
      return;
    }

    let parsedBody: string | undefined;
    if (endpoint.method === "POST") {
      try {
        parsedBody = JSON.stringify(JSON.parse(requestBody));
      } catch {
        setError("Request body must be valid JSON.");
        return;
      }
    }

    // Request timing is intentionally measured inside the event handler, not during render.
    // eslint-disable-next-line react-hooks/purity
    const startedAt = performance.now();
    try {
      const result = await fetch(`/api${path}`, {
        method: endpoint.method,
        headers: {
          Accept: "application/json",
          "X-Api-Key": apiKey.trim(),
          ...(parsedBody ? { "Content-Type": "application/json" } : {}),
        },
        body: parsedBody,
      });
      const rawBody = await result.text();
      setResponse({
        status: result.status,
        // eslint-disable-next-line react-hooks/purity
        duration: Math.round(performance.now() - startedAt),
        body: formatResponse(rawBody),
      });
    } catch {
      setError("The request could not reach the Gateway.");
    }
  }

  async function copyCurl() {
    await navigator.clipboard.writeText(buildCurl(endpoint, path, apiKey, requestBody));
    setCopied(true);
  }

  return (
    <div className="space-y-7">
      <PageHeader title="API Explorer" description="Try your APIs through the Gateway before integrating them into your application." />

      <Card className="border-brand-100 bg-brand-50/50 p-5 sm:p-6">
        <div className="flex items-start gap-3">
          <ShieldCheck className="mt-0.5 size-5 shrink-0 text-brand-700" />
          <div>
            <p className="font-semibold text-plum-950">Your API key stays in this page</p>
            <p className="mt-1 text-sm leading-6 text-plum-600">It is sent only to the Gateway for this request and is never saved by the Portal.</p>
          </div>
        </div>
      </Card>

      <div className="grid gap-6 xl:grid-cols-[minmax(0,0.95fr)_minmax(0,1.05fr)]">
        <Card className="p-5 sm:p-6">
          <div className="flex items-center gap-3"><span className="grid size-10 place-items-center rounded-2xl bg-brand-50 text-brand-700"><Code2 className="size-5" /></span><div><h2 className="font-display text-xl font-semibold text-plum-950">Request</h2><p className="text-sm text-plum-500">Use the same credential your integration will use.</p></div></div>
          <div className="mt-6 space-y-5">
            <label className="block text-sm font-medium text-plum-700">API key<Input className="mt-2" type="password" value={apiKey} onChange={(event) => setApiKey(event.target.value)} placeholder="app_..." autoComplete="off" /></label>
            <label className="block text-sm font-medium text-plum-700">Endpoint<Select className="mt-2" value={endpointId} onChange={(event) => selectEndpoint(event.target.value)}>{endpoints.map((item) => <option key={item.id} value={item.id}>{item.method} {item.path} · {item.api}</option>)}</Select></label>
            <div className="rounded-2xl bg-plum-950 p-4 text-sm text-white"><div className="flex flex-wrap items-center gap-2"><span className="rounded-md bg-brand-500 px-2 py-1 text-xs font-bold">{endpoint.method}</span><code>{path}</code></div><p className="mt-3 text-plum-300">{endpoint.description}</p><p className="mt-2 text-xs text-plum-400">Required grant: <code className="text-brand-300">{endpoint.scope}</code></p></div>
            {endpoint.path.includes("{id}") && <label className="block text-sm font-medium text-plum-700">Resource ID<Input className="mt-2" type="number" min="1" value={resourceId} onChange={(event) => setResourceId(event.target.value)} /></label>}
            {endpoint.method === "POST" && <label className="block text-sm font-medium text-plum-700">JSON body<textarea className="mt-2 min-h-32 w-full rounded-xl border border-plum-200 bg-paper p-3 font-mono text-sm text-plum-950 shadow-sm outline-none focus:border-brand-500" value={body || endpoint.body || ""} onChange={(event) => setBody(event.target.value)} spellCheck={false} /></label>}
            {error && <p className="text-sm text-danger-700" role="alert">{error}</p>}
            <Button className="w-full" onClick={sendRequest}><Play className="size-4" /> Send request</Button>
          </div>
        </Card>

        <Card className="overflow-hidden">
          <div className="flex items-center justify-between gap-3 border-b border-plum-100 p-5 sm:p-6"><div><h2 className="font-display text-xl font-semibold text-plum-950">Response</h2><p className="text-sm text-plum-500">Gateway response preview.</p></div>{response && <div className="flex items-center gap-3 text-xs text-plum-500"><span className={responseStatusClass(response.status)}>{response.status}</span><span>{response.duration} ms</span><button type="button" className="inline-flex items-center gap-1 rounded-lg px-2 py-1 font-semibold text-plum-600 hover:bg-plum-50" onClick={copyCurl}>{copied ? <Check className="size-3.5" /> : <Copy className="size-3.5" />}{copied ? "Copied" : "cURL"}</button></div>}</div>
          {response ? <pre className="min-h-80 overflow-auto bg-plum-950 p-5 text-sm leading-6 text-plum-100">{response.body || "(empty response)"}</pre> : <div className="grid min-h-80 place-items-center p-6 text-center"><div><Code2 className="mx-auto size-8 text-plum-300" /><p className="mt-3 font-semibold text-plum-900">No response yet</p><p className="mt-1 text-sm text-plum-500">Send a request to see the result here.</p></div></div>}
        </Card>
      </div>
    </div>
  );
}

function formatResponse(value: string): string {
  try {
    return JSON.stringify(JSON.parse(value), null, 2);
  } catch {
    return value;
  }
}

function responseStatusClass(status: number): string {
  if (status >= 200 && status < 300) {
    return "rounded-full bg-sage-100 px-2.5 py-1 font-semibold text-sage-800";
  }

  if (status >= 300 && status < 400) {
    return "rounded-full bg-brand-100 px-2.5 py-1 font-semibold text-brand-700";
  }

  if (status >= 400 && status < 500) {
    return "rounded-full bg-amber-100 px-2.5 py-1 font-semibold text-amber-800";
  }

  if (status >= 500) {
    return "rounded-full bg-danger-100 px-2.5 py-1 font-semibold text-danger-700";
  }

  return "rounded-full bg-plum-100 px-2.5 py-1 font-semibold text-plum-600";
}

function buildCurl(endpoint: ExplorerEndpoint, path: string, apiKey: string, body: string): string {
  const bodyArgument = endpoint.method === "POST" ? ` \\\n  -H 'Content-Type: application/json' \\\n  -d '${JSON.stringify(JSON.parse(body))}'` : "";
  return `curl http://localhost:5290${path} \\\n  -H 'X-Api-Key: ${apiKey}'${bodyArgument}`;
}
