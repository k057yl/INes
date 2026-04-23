import { Injectable } from '@angular/core';
import { FormGroup } from '@angular/forms';

@Injectable({ providedIn: 'root' })
export class FormErrorService {
  mapServerErrorsToForm(form: FormGroup, serverErrors: { [key: string]: string[] }): void {
    Object.keys(serverErrors).forEach(key => {
      const controlKey = key.charAt(0).toLowerCase() + key.slice(1);
      const control = form.get(controlKey) || form.get(key);

      if (control) {
        control.setErrors({ serverError: serverErrors[key][0] });
        control.markAsTouched();
      }
    });
  }
}