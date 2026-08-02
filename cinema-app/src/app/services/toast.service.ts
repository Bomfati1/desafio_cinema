import { Injectable } from '@angular/core';
import { BehaviorSubject, Observable } from 'rxjs';

export interface Toast {
  id: number;
  message: string;
  type: 'success' | 'error' | 'info' | 'warning';
}

export interface ConfirmOptions {
  title: string;
  message: string;
  confirmLabel?: string;
  cancelLabel?: string;
}

@Injectable({ providedIn: 'root' })
export class ToastService {
  private nextId = 0;
  private toasts$ = new BehaviorSubject<Toast[]>([]);
  private confirmResolve: ((value: boolean) => void) | null = null;
  private confirmState$ = new BehaviorSubject<{ options: ConfirmOptions } | null>(null);

  // ── Toast API ──────────────────────────────────────

  getToasts(): Observable<Toast[]> {
    return this.toasts$.asObservable();
  }

  success(message: string): void { this.show(message, 'success'); }
  error(message: string): void   { this.show(message, 'error'); }
  info(message: string): void    { this.show(message, 'info'); }
  warning(message: string): void { this.show(message, 'warning'); }

  private show(message: string, type: Toast['type']): void {
    const id = ++this.nextId;
    const toast: Toast = { id, message, type };
    this.toasts$.next([...this.toasts$.value, toast]);

    // Auto-remove após 4s (success/info) ou 6s (error/warning)
    const duration = type === 'error' || type === 'warning' ? 6000 : 4000;
    setTimeout(() => this.dismiss(id), duration);
  }

  dismiss(id: number): void {
    this.toasts$.next(this.toasts$.value.filter(t => t.id !== id));
  }

  // ── Confirm Dialog API ─────────────────────────────

  getConfirmState(): Observable<{ options: ConfirmOptions } | null> {
    return this.confirmState$.asObservable();
  }

  /** Substitui confirm() nativo — retorna Promise<boolean> */
  confirm(options: ConfirmOptions): Promise<boolean> {
    this.confirmState$.next({ options });
    return new Promise<boolean>((resolve) => {
      this.confirmResolve = resolve;
    });
  }

  confirmResult(result: boolean): void {
    this.confirmState$.next(null);
    this.confirmResolve?.(result);
    this.confirmResolve = null;
  }
}
