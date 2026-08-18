import { useEffect, useRef, type ButtonHTMLAttributes, type HTMLAttributes, type InputHTMLAttributes, type ReactNode, type SelectHTMLAttributes } from "react";
import { AlertCircle, LoaderCircle, X } from "lucide-react";

export function Button({
  className = "",
  variant = "primary",
  ...props
}: ButtonHTMLAttributes<HTMLButtonElement> & { variant?: "primary" | "secondary" | "danger" | "ghost" }) {
  const variants = {
    primary: "bg-brand-600 text-white shadow-[0_8px_20px_-10px_rgba(15,118,110,0.75)] hover:bg-brand-700",
    secondary: "border border-plum-200 bg-paper text-plum-800 hover:border-plum-300 hover:bg-canvas",
    danger: "bg-danger-600 text-white hover:bg-danger-700",
    ghost: "text-plum-600 hover:bg-plum-50 hover:text-plum-900",
  };

  return (
    <button
      className={`inline-flex min-h-10 items-center justify-center gap-2 rounded-full px-4 py-2 text-sm font-semibold transition duration-200 disabled:cursor-not-allowed disabled:opacity-50 ${variants[variant]} ${className}`}
      {...props}
    />
  );
}

export function Input({ className = "", ...props }: InputHTMLAttributes<HTMLInputElement>) {
  return (
    <input
      className={`min-h-11 w-full rounded-xl border border-plum-200 bg-paper px-3 text-sm text-plum-950 shadow-sm transition placeholder:text-plum-300 focus:border-brand-500 ${className}`}
      {...props}
    />
  );
}

export function Select({ className = "", ...props }: SelectHTMLAttributes<HTMLSelectElement>) {
  return (
    <select
      className={`min-h-11 w-full rounded-xl border border-plum-200 bg-paper px-3 text-sm text-plum-950 shadow-sm transition focus:border-brand-500 ${className}`}
      {...props}
    />
  );
}

export function Card({ className = "", ...props }: HTMLAttributes<HTMLDivElement>) {
  return <div className={`rounded-3xl border border-plum-100 bg-paper shadow-[0_18px_50px_-38px_rgba(48,29,52,0.55)] ${className}`} {...props} />;
}

export function PageHeader({ title, description, action }: { title: string; description: string; action?: ReactNode }) {
  return (
    <div className="flex flex-col justify-between gap-4 sm:flex-row sm:items-end">
      <div>
        <h1 className="font-display text-3xl font-semibold tracking-tight text-plum-950 sm:text-4xl">{title}</h1>
        <p className="mt-2 max-w-2xl text-sm leading-6 text-plum-500">{description}</p>
      </div>
      {action}
    </div>
  );
}

export function StatusBadge({ active, activeLabel = "Active", inactiveLabel = "Inactive" }: {
  active: boolean;
  activeLabel?: string;
  inactiveLabel?: string;
}) {
  return (
    <span className={`inline-flex items-center gap-1.5 rounded-full px-2.5 py-1 text-xs font-semibold ${active ? "bg-sage-100 text-sage-800" : "bg-plum-100 text-plum-600"}`}>
      <span className={`size-1.5 rounded-full ${active ? "bg-sage-600" : "bg-plum-400"}`} />
      {active ? activeLabel : inactiveLabel}
    </span>
  );
}

export function LoadingState() {
  return (
    <div className="flex min-h-48 items-center justify-center text-plum-500" role="status">
      <LoaderCircle className="mr-2 size-5 animate-spin" /> Loading…
    </div>
  );
}

export function ErrorState({ error }: { error: unknown }) {
  return (
    <div className="flex items-start gap-3 rounded-2xl border border-danger-200 bg-danger-50 p-4 text-sm text-danger-700" role="alert">
      <AlertCircle className="mt-0.5 size-5 shrink-0" />
      <span>{errorMessage(error)}</span>
    </div>
  );
}

export function EmptyState({ title, description }: { title: string; description: string }) {
  return (
    <div className="px-6 py-14 text-center">
      <p className="font-semibold text-plum-900">{title}</p>
      <p className="mt-1 text-sm text-plum-500">{description}</p>
    </div>
  );
}

function errorMessage(error: unknown): string {
  return error instanceof Error ? error.message : "We couldn't load this data.";
}

export function Dialog({
  open,
  title,
  description,
  onClose,
  children,
}: {
  open: boolean;
  title: string;
  description?: string;
  onClose: () => void;
  children: ReactNode;
}) {
  if (!open) {
    return null;
  }

  return <DialogSurface title={title} description={description} onClose={onClose}>{children}</DialogSurface>;
}

function DialogSurface({ title, description, onClose, children }: Omit<Parameters<typeof Dialog>[0], "open">) {
  const dialogRef = useRef<HTMLDialogElement>(null);

  useEffect(() => {
    const dialog = dialogRef.current;
    if (!dialog) {
      return;
    }

    if (!dialog.open) {
      dialog.showModal();
    }

    return () => {
      if (dialog.open) {
        dialog.close();
      }
    };
  }, []);

  return (
    <dialog ref={dialogRef} className="dialog-shell m-auto w-[calc(100%-2rem)] max-w-lg rounded-3xl border border-plum-100 bg-paper p-0 text-plum-950 shadow-2xl" onCancel={onClose} onClick={(event) => {
      if (event.target === event.currentTarget) {
        onClose();
      }
    }}>
      <div className="p-6 sm:p-7">
        <div className="flex items-start justify-between gap-4">
          <div>
            <h2 className="font-display text-2xl font-semibold tracking-tight">{title}</h2>
            {description && <p className="mt-2 text-sm leading-6 text-plum-500">{description}</p>}
          </div>
          <button type="button" className="grid size-9 shrink-0 place-items-center rounded-full text-plum-400 transition hover:bg-plum-50 hover:text-plum-900" aria-label="Close dialog" onClick={onClose}>
            <X className="size-4" />
          </button>
        </div>
        <div className="mt-6">{children}</div>
      </div>
    </dialog>
  );
}

export function DialogActions({ children }: { children: ReactNode }) {
  return <div className="mt-7 flex flex-col-reverse gap-2 sm:flex-row sm:justify-end">{children}</div>;
}
