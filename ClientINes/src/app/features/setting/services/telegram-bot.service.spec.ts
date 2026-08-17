import { TestBed } from '@angular/core/testing';
import { HttpClientTestingModule, HttpTestingController } from '@angular/common/http/testing';
import { TelegramBotService } from './telegram-bot.service';
import { environment } from '../../../../environments/environment';

describe('TelegramBotService', () => {
  let service: TelegramBotService;
  let httpMock: HttpTestingController;
  const apiUrl = `${environment.apiBaseUrl}/telegram`;

  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [HttpClientTestingModule],
      providers: [TelegramBotService]
    });

    service = TestBed.inject(TelegramBotService);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpMock.verify();
  });

  it('getStatus() должен делать GET на /status', () => {
    service.getStatus().subscribe();

    const req = httpMock.expectOne(`${apiUrl}/status`);
    expect(req.request.method).toBe('GET');
    req.flush({ isLinked: false });
  });

  it('generateToken() должен делать POST на /generate-token', () => {
    service.generateToken().subscribe();

    const req = httpMock.expectOne(`${apiUrl}/generate-token`);
    expect(req.request.method).toBe('POST');
    req.flush({ isLinked: false, verificationToken: 'tok-123' });
  });

  it('unlink() должен делать POST на /unlink', () => {
    service.unlink().subscribe();

    const req = httpMock.expectOne(`${apiUrl}/unlink`);
    expect(req.request.method).toBe('POST');
    req.flush(null);
  });
});