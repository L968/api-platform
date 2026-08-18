import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { Blocks, Building2, ChartNoAxesCombined, KeyRound, LayoutDashboard, LogOut } from "lucide-react";
import { NavLink, Outlet, useNavigate } from "react-router";
import { currentUserQuery, logout } from "../../features/auth/authApi";

const navigation = [
  { to: "/", label: "Visão geral", icon: LayoutDashboard, end: true },
  { to: "/applications", label: "Applications", icon: KeyRound, end: false },
  { to: "/usage", label: "Consumo", icon: ChartNoAxesCombined, end: false },
  { to: "/organization", label: "Organization", icon: Building2, end: false },
];

export function AppLayout() {
  const navigate = useNavigate();
  const queryClient = useQueryClient();
  const { data: user } = useQuery(currentUserQuery);
  const logoutMutation = useMutation({
    mutationFn: logout,
    onSettled: () => {
      queryClient.clear();
      navigate("/login", { replace: true });
    },
  });

  return (
    <div className="min-h-screen bg-slate-50 lg:grid lg:grid-cols-[260px_1fr]">
      <aside className="border-b border-slate-800 bg-ink-950 text-white lg:fixed lg:inset-y-0 lg:w-[260px] lg:border-b-0 lg:border-r">
        <div className="flex h-16 items-center justify-between gap-3 px-5 lg:h-20">
          <div className="flex items-center gap-3">
            <span className="grid size-9 place-items-center rounded-xl bg-brand-500"><Blocks className="size-5" /></span>
            <span className="font-bold">API Platform</span>
          </div>
          <button className="rounded-lg p-2 text-slate-400 hover:bg-white/5 hover:text-white lg:hidden" aria-label="Sair" onClick={() => logoutMutation.mutate()}>
            <LogOut className="size-5" />
          </button>
        </div>
        <nav className="flex gap-1 overflow-x-auto px-3 pb-3 lg:block lg:space-y-1 lg:px-4 lg:pb-0" aria-label="Principal">
          {navigation.map(({ to, label, icon: Icon, end }) => (
            <NavLink
              key={to}
              to={to}
              end={end}
              className={({ isActive }) => `flex shrink-0 items-center gap-3 rounded-lg px-3 py-2.5 text-sm font-medium transition ${isActive ? "bg-white/10 text-white" : "text-slate-400 hover:bg-white/5 hover:text-white"}`}
            >
              <Icon className="size-4" /> {label}
            </NavLink>
          ))}
        </nav>
        <div className="hidden border-t border-slate-800 p-4 lg:absolute lg:inset-x-0 lg:bottom-0 lg:block">
          <p className="truncate text-sm font-medium">{user?.organization.name}</p>
          <p className="mt-0.5 truncate text-xs text-slate-500">{user?.email}</p>
          <button className="mt-4 flex w-full items-center gap-2 rounded-lg py-2 text-sm text-slate-400 hover:text-white" onClick={() => logoutMutation.mutate()}>
            <LogOut className="size-4" /> Sair
          </button>
        </div>
      </aside>

      <main className="min-w-0 lg:col-start-2">
        <div className="mx-auto max-w-7xl px-5 py-8 sm:px-8 lg:px-10 lg:py-10">
          <Outlet />
        </div>
      </main>
    </div>
  );
}
