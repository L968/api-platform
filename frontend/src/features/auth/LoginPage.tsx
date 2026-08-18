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
  const [email, setEmail] = useState("");
  const [password, setPassword] = useState("");

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
    <main className="grid min-h-screen bg-white lg:grid-cols-[1.1fr_0.9fr]">
      <section className="hidden bg-ink-950 p-12 text-white lg:flex lg:flex-col lg:justify-between">
        <div className="flex items-center gap-3 text-lg font-bold">
          <span className="grid size-10 place-items-center rounded-xl bg-brand-500"><Blocks className="size-5" /></span>
          API Platform
        </div>
        <div className="max-w-xl">
          <p className="text-sm font-semibold uppercase tracking-[0.2em] text-brand-100">Developer Portal</p>
          <h1 className="mt-5 text-5xl font-bold leading-tight">Suas integrações, chaves e consumo em um só lugar.</h1>
          <div className="mt-10 grid gap-4 sm:grid-cols-2">
            <div className="rounded-2xl border border-white/10 bg-white/5 p-5">
              <KeyRound className="size-6 text-brand-100" />
              <p className="mt-4 font-semibold">Credenciais seguras</p>
              <p className="mt-1 text-sm text-slate-400">Crie e revogue API Keys por Application.</p>
            </div>
            <div className="rounded-2xl border border-white/10 bg-white/5 p-5">
              <ShieldCheck className="size-6 text-brand-100" />
              <p className="mt-4 font-semibold">Acesso por escopo</p>
              <p className="mt-1 text-sm text-slate-400">Conceda apenas as permissões necessárias.</p>
            </div>
          </div>
        </div>
        <p className="text-sm text-slate-500">API Platform · Portal administrativo</p>
      </section>

      <section className="flex items-center justify-center bg-slate-50 px-6 py-12">
        <div className="w-full max-w-md">
          <div className="mb-10 flex items-center gap-3 text-lg font-bold text-slate-950 lg:hidden">
            <span className="grid size-10 place-items-center rounded-xl bg-brand-600 text-white"><Blocks className="size-5" /></span>
            API Platform
          </div>
          <p className="text-sm font-semibold text-brand-600">Bem-vindo de volta</p>
          <h2 className="mt-2 text-3xl font-bold tracking-tight text-slate-950">Entre no portal</h2>
          <p className="mt-2 text-sm text-slate-500">Use sua conta da Organization para continuar.</p>

          <form className="mt-8 space-y-5" onSubmit={handleSubmit}>
            <label className="block text-sm font-medium text-slate-700">
              E-mail
              <Input className="mt-2" type="email" autoComplete="email" required value={email} onChange={(event) => setEmail(event.target.value)} placeholder="voce@empresa.com" />
            </label>
            <label className="block text-sm font-medium text-slate-700">
              Senha
              <Input className="mt-2" type="password" autoComplete="current-password" required value={password} onChange={(event) => setPassword(event.target.value)} />
            </label>

            {mutation.isError && <p className="rounded-lg bg-red-50 p-3 text-sm text-red-700" role="alert">{errorMessage(mutation.error)}</p>}

            <Button className="w-full" type="submit" disabled={mutation.isPending}>
              {mutation.isPending ? "Entrando…" : "Entrar"} <ArrowRight className="size-4" />
            </Button>
          </form>
        </div>
      </section>
    </main>
  );
}
