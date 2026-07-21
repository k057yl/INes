import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { ToastrService } from 'ngx-toastr';
import { TranslateService } from '@ngx-translate/core';

@Injectable()
export class DashboardActionExecutor {
  private toastr = inject(ToastrService);
  private translate = inject(TranslateService);

  run<T>(
    request$: Observable<T>,
    successMsgKey: string | null,
    onSuccess?: (res: T) => void,
    errorMsgKey: string = 'SYSTEM.DEFAULT_ERROR'
  ): void {
    request$.subscribe({
      next: (res) => {
        if (successMsgKey) {
          this.toastr.success(this.translate.instant(successMsgKey));
        }
        if (onSuccess) onSuccess(res);
      },
      error: (err) => {
        const messageKey = err?.error?.message || err?.error?.error || errorMsgKey;
        this.toastr.error(this.translate.instant(messageKey));
      }
    });
  }
}