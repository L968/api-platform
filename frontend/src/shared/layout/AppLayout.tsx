import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { Blocks, Building2, ChartNoAxesCombined, FileText, KeyRound, LayoutDashboard, LogOut, SquareTerminal } from "lucide-react";
import { NavLink, Outlet, useNavigate } from "react-router";
import { currentUserQuery, logout } from "../../features/auth/authApi";

const navigation = [
  { to: "/", label: "Overview", icon: LayoutDashboard, end: true },
  { to: "/applications", label: "Applications", icon: KeyRound, end: false },
  { to: "/usage", label: "Usage & billing", icon: ChartNoAxesCombined, end: false },
  { to: "/invoices", label: "Invoices", icon: FileText, end: false },
  { to: "/explorer", label: "API Explorer", icon: SquareTerminal, end: false },
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
    <div className="min-h-screen bg-canvas lg:grid lg:grid-cols-[272px_1fr]">
      <aside className="border-b border-plum-800 bg-plum-950 text-white lg:fixed lg:inset-y-0 lg:w-[272px] lg:border-b-0 lg:border-r">
        <div className="flex h-16 items-center justify-between gap-3 px-5 lg:h-20">
          <div className="flex items-center gap-3">
            <span className="grid size-9 place-items-center rounded-xl bg-brand-500 text-white"><Blocks className="size-5" /></span>
            <span className="font-display text-lg font-semibold">API Platform</span>
          </div>
        </div>
        <nav className="flex gap-1 overflow-x-auto px-3 pb-3 lg:block lg:space-y-1 lg:px-4 lg:pb-0" aria-label="Main navigation">
          {navigation.map(({ to, label, icon: Icon, end }) => (
            <NavLink
              key={to}
              to={to}
              end={end}
              className={({ isActive }) => `flex shrink-0 items-center gap-3 rounded-xl px-3 py-2.5 text-sm font-medium transition ${isActive ? "bg-brand-500 text-white shadow-lg shadow-black/10" : "text-plum-300 hover:bg-white/5 hover:text-white"}`}
            >
              <Icon className="size-4" /> {label}
            </NavLink>
          ))}
        </nav>
        <div className="border-t border-plum-800 p-5 lg:absolute lg:inset-x-0 lg:bottom-0">
          <NavLink to="/organization" className="-m-2 flex items-center gap-3 rounded-xl p-2 transition hover:bg-white/5">
            <span className="grid size-9 shrink-0 place-items-center rounded-xl bg-white/10 text-plum-200"><Building2 className="size-4" /></span>
            <span className="min-w-0">
              <span className="block truncate text-sm font-medium text-white">{user?.organization.name}</span>
              <span className="mt-0.5 block truncate text-xs text-plum-400">{user?.email}</span>
            </span>
          </NavLink>
          <button className="mt-4 flex w-full items-center gap-2 rounded-lg py-2 text-sm text-plum-300 hover:text-white" onClick={() => logoutMutation.mutate()}>
            <LogOut className="size-4" /> Sign out
          </button>
        </div>
      </aside>

      <main className="min-w-0 lg:col-start-2">
        <div className="mx-auto max-w-7xl px-5 py-8 sm:px-8 lg:px-12 lg:py-12">
          <Outlet />
        </div>
      </main>
    </div>
  );
}
