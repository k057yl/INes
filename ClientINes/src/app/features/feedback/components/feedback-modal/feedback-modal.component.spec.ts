import { ComponentFixture, TestBed } from '@angular/core/testing';
import { FeedbackModalComponent } from './feedback-modal.component';
import { FeedbackService } from '../../services/feedback.service';
import { DashboardModalService } from '../../../dashboard/components/dashboard/dashboard.modal.service';
import { TranslateModule } from '@ngx-translate/core';
import { FormsModule } from '@angular/forms';
import { of } from 'rxjs';
import { FeedbackType } from '../../enums/feedback-type.enum';

describe('FeedbackModalComponent', () => {
  let component: FeedbackModalComponent;
  let fixture: ComponentFixture<FeedbackModalComponent>;
  let feedbackServiceSpy: jasmine.SpyObj<FeedbackService>;
  let modalServiceSpy: jasmine.SpyObj<DashboardModalService>;

  beforeEach(async () => {
    feedbackServiceSpy = jasmine.createSpyObj('FeedbackService', ['sendFeedback', 'rateFeedback']);
    modalServiceSpy = jasmine.createSpyObj('DashboardModalService', ['close']);

    await TestBed.configureTestingModule({
      imports: [FeedbackModalComponent, FormsModule, TranslateModule.forRoot()],
      providers: [
        { provide: FeedbackService, useValue: feedbackServiceSpy },
        { provide: DashboardModalService, useValue: modalServiceSpy }
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(FeedbackModalComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('должен создаваться и инициализироваться с 1 шагом', () => {
    expect(component).toBeTruthy();
    expect(component.step).toBe(1);
    expect(component.rating).toBe(5);
  });

  it('submitStep1 не должен отправлять форму с пустым сообщением', () => {
    component.message = '   ';
    component.submitStep1();

    expect(feedbackServiceSpy.sendFeedback).not.toHaveBeenCalled();
  });

  it('submitStep1 должен отправлять фидбек и переходить на шаг 2', () => {
    feedbackServiceSpy.sendFeedback.and.returnValue(of({ id: 'fb-999' }));
    component.message = 'Нашел баг в интерфейсе';
    component.feedbackType = FeedbackType.Bug;

    component.submitStep1();

    expect(feedbackServiceSpy.sendFeedback).toHaveBeenCalledWith({
      type: Number(FeedbackType.Bug),
      message: 'Нашел баг в интерфейсе'
    });
    expect(component.createdFeedbackId).toBe('fb-999');
    expect(component.step).toBe(2);
    expect(component.isLoading).toBeFalse();
  });

  it('setRating должен обновлять значение рейтинга', () => {
    component.setRating(4);
    expect(component.rating).toBe(4);
  });

  it('submitStep2 должен отправлять оценку и закрывать модалку', () => {
    feedbackServiceSpy.rateFeedback.and.returnValue(of(void 0));
    component.createdFeedbackId = 'fb-999';
    component.rating = 5;
    component.missingFeatures = 'Хочу темную тему';

    component.submitStep2();

    expect(feedbackServiceSpy.rateFeedback).toHaveBeenCalledWith('fb-999', {
      rating: 5,
      missingFeatures: 'Хочу темную тему'
    });
    expect(modalServiceSpy.close).toHaveBeenCalled();
  });
});