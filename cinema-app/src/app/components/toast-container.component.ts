import { Component, inject } from '@angular/core';
import { AsyncPipe } from '@angular/common';
import { ToastService } from '../services/toast.service';

@Component({
  selector: 'app-toast-container',
  standalone: true,
  imports: [AsyncPipe],
  template: `
    <div class="toast-container">
      @for (toast of toasts$ | async; track toast.id) {
        <div
          class="toast toast-{{ toast.type }}"
          (click)="toastService.dismiss(toast.id)"
        >
          @if (toast.type === 'success') { ✅ }
          @else if (toast.type === 'error') { ❌ }
          @else if (toast.type === 'warning') { ⚠️ }
          @else { ℹ️ }
          {{ toast.message }}
        </div>
      }
    </div>

    <!-- Confirm Dialog -->
    @if (confirmState$ | async; as state) {
      <div class="modal-backdrop" (click)="toastService.confirmResult(false)">
        <div class="modal-dialog" (click)="$event.stopPropagation()">
          <h3>{{ state.options.title }}</h3>
          <p>{{ state.options.message }}</p>
          <div class="modal-actions">
            <button class="btn-cancel" (click)="toastService.confirmResult(false)">
              {{ state.options.cancelLabel || 'Cancelar' }}
            </button>
            <button class="btn-confirm" (click)="toastService.confirmResult(true)">
              {{ state.options.confirmLabel || 'Confirmar' }}
            </button>
          </div>
        </div>
      </div>
    }
  `,
})
export class ToastContainerComponent {
  toastService = inject(ToastService);
  toasts$ = this.toastService.getToasts();
  confirmState$ = this.toastService.getConfirmState();
}
