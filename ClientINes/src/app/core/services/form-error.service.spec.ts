import { TestBed } from '@angular/core/testing';
import { FormErrorService } from './form-error.service';
import { FormControl, FormGroup } from '@angular/forms';

describe('FormErrorService', () => {
  let service: FormErrorService;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [FormErrorService]
    });
    service = TestBed.inject(FormErrorService);
  });

  it('mapServerErrorsToForm() должен проставлять serverError и пометить контрол как touched', () => {
    const form = new FormGroup({
      email: new FormControl('')
    });

    const serverErrors = {
      Email: ['Email уже занят']
    };

    service.mapServerErrorsToForm(form, serverErrors);

    const emailControl = form.get('email');
    expect(emailControl?.errors).toEqual({ serverError: 'Email уже занят' });
    expect(emailControl?.touched).toBeTrue();
  });
});