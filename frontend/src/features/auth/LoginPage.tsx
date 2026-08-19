import { useState, type FormEvent } from "react";
import { useMutation, useQueryClient } from "@tanstack/react-query";
import { ArrowRight, Blocks, KeyRound, ShieldCheck } from "lucide-react";
import { Navigate, useLocation, useNavigate } from "react-router";
import { Button, Input } from "../../shared/components/ui";
import { errorMessage } from "../../shared/format";
import { currentUserQuery, login } from "./authApi";

export function LoginPage() {
  const navigate = useNavigate();
  const location = useLocation();
  const queryClient = useQueryClient();
  const currentUser = queryClient.getQueryData(currentUserQuery.queryKey);
  const localLogin = localCredentials();
  const [email, setEmail] = useState(localLogin.email);
  const [password, setPassword] = useState(localLogin.password);

  const mutation = useMutation({
    mutationFn: login,
    onSuccess: async () => {
      await queryClient.invalidateQueries({ queryKey: currentUserQuery.queryKey });
      const destination = (location.state as { from?: string } | null)?.from ?? "/";
      navigate(destination, { replace: true });
    },
  });

  if (currentUser) {
    return <Navigate to="/" replace />;
  }

  function handleSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    mutation.mutate({ email, password });
  }

  return (
    <main className="grid min-h-screen bg-paper lg:grid-cols-[1.1fr_0.9fr]">
      <section className="relative hidden overflow-hidden bg-plum-950 p-12 text-white lg:flex lg:flex-col lg:justify-between">
        <span className="absolute -right-24 -top-24 size-80 rounded-full border-[56px] border-brand-500/20" />
        <span className="absolute -bottom-32 left-1/3 size-72 rounded-full bg-sage-100/10" />
        <div className="flex items-center gap-3 text-lg font-bold">
          <span className="grid size-10 place-items-center rounded-xl bg-brand-500"><Blocks className="size-5" /></span>
          <span className="font-display text-xl font-semibold">API Platform</span>
        </div>
        <div className="relative max-w-xl">
          <p className="text-sm font-semibold uppercase tracking-[0.2em] text-brand-100">Developer Portal</p>
          <h1 className="mt-5 font-display text-6xl font-semibold leading-[1.05]">Build integrations without losing control.</h1>
          <div className="mt-10 grid gap-4 sm:grid-cols-2">
            <div className="rounded-2xl border border-white/10 bg-white/5 p-5">
              <KeyRound className="size-6 text-brand-100" />
              <p className="mt-4 font-semibold">Secure credentials</p>
              <p className="mt-1 text-sm text-plum-300">Issue and revoke keys for each application.</p>
            </div>
            <div className="rounded-2xl border border-white/10 bg-white/5 p-5">
              <ShieldCheck className="size-6 text-brand-100" />
              <p className="mt-4 font-semibold">Scoped access</p>
              <p className="mt-1 text-sm text-plum-300">Grant only the permissions each system needs.</p>
            </div>
          </div>
        </div>
        <p className="relative text-sm text-plum-400">API Platform · Developer operations</p>
      </section>

      <section className="flex items-center justify-center bg-canvas px-6 py-12">
        <div className="w-full max-w-md">
          <div className="mb-10 flex items-center gap-3 text-lg font-bold text-plum-950 lg:hidden">
            <span className="grid size-10 place-items-center rounded-xl bg-brand-600 text-white"><Blocks className="size-5" /></span>
            API Platform
          </div>
          <p className="text-sm font-semibold text-brand-600">Welcome back</p>
          <h2 className="mt-2 font-display text-4xl font-semibold tracking-tight text-plum-950">Sign in to the portal</h2>
          <p className="mt-2 text-sm text-plum-500">Use your organization account to continue.</p>

          <form className="mt-8 space-y-5" onSubmit={handleSubmit}>
            <label className="block text-sm font-medium text-plum-700">
              Email
              <Input className="mt-2" type="email" autoComplete="email" required value={email} onChange={(event) => setEmail(event.target.value)} placeholder="you@company.com" />
            </label>
            <label className="block text-sm font-medium text-plum-700">
              Password
              <Input className="mt-2" type="password" autoComplete="current-password" required value={password} onChange={(event) => setPassword(event.target.value)} />
            </label>

            {mutation.isError && <p className="rounded-xl bg-danger-50 p-3 text-sm text-danger-700" role="alert">{errorMessage(mutation.error)}</p>}

            <Button className="w-full" type="submit" disabled={mutation.isPending}>
              {mutation.isPending ? "Signing in…" : "Sign in"} <ArrowRight className="size-4" />
            </Button>
          </form>
        </div>
      </section>
    </main>
  );
}

function localCredentials() {
  const isLocal = window.location.hostname === "localhost" || window.location.hostname === "127.0.0.1";

  return isLocal
    ? { email: "developer@acme.test", password: "DemoAccess123!" }
    : { email: "", password: "" };
}
