import { ComponentFixture, TestBed } from '@angular/core/testing';
import { AdminFeedbackComponent } from './admin-feedback.component';
import { FeedbackService } from '../../feedback/services/feedback.service';
import { FeedbackType } from '../../feedback/enums/feedback-type.enum';
import { FeedbackItem, PagedFeedbackResult } from '../../feedback/contracts/feedback';
import { of, throwError } from 'rxjs';
import { FormsModule } from '@angular/forms';

describe('AdminFeedbackComponent', () => {
  let component: AdminFeedbackComponent;
  let fixture: ComponentFixture<AdminFeedbackComponent>;
  let feedbackServiceSpy: jasmine.SpyObj<FeedbackService>;

  const mockFeedbacksResponse: PagedFeedbackResult = {
    items: [
      { id: '1', message: 'Кнопка не работает', type: FeedbackType.Bug, isProcessed: false, createdAt: '2026-08-17' },
      { id: '2', message: 'Добавьте темную тему', type: FeedbackType.Idea, isProcessed: true, createdAt: '2026-08-17' }
    ] as FeedbackItem[],
    totalCount: 2,
    page: 1,
    pageSize: 10
  };

  beforeEach(async () => {
    feedbackServiceSpy = jasmine.createSpyObj('FeedbackService', ['getAdminFeedbacks', 'toggleProcessed']);
    feedbackServiceSpy.getAdminFeedbacks.and.returnValue(of(mockFeedbacksResponse));

    await TestBed.configureTestingModule({
      imports: [AdminFeedbackComponent, FormsModule],
      providers: [
        { provide: FeedbackService, useValue: feedbackServiceSpy }
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(AdminFeedbackComponent);
    component = fixture.componentInstance;
  });

  it('должен создаваться', () => {
    expect(component).toBeTruthy();
  });

  it('ngOnInit должен подгружать список фидбеков', () => {
    fixture.detectChanges();

    expect(feedbackServiceSpy.getAdminFeedbacks).toHaveBeenCalledWith(1, 10, null, null);
    expect(component.feedbacks.length).toBe(2);
    expect(component.totalCount).toBe(2);
    expect(component.isLoading).toBeFalse();
  });

  it('toggleProcessed должен оптимистично менять флаг и вызывать сервис', () => {
    feedbackServiceSpy.toggleProcessed.and.returnValue(of(void 0 as any));
    const item = { ...mockFeedbacksResponse.items[0] };

    component.toggleProcessed(item);

    expect(item.isProcessed).toBeTrue();
    expect(feedbackServiceSpy.toggleProcessed).toHaveBeenCalledWith('1');
  });

  it('toggleProcessed должен откатывать флаг isProcessed при ошибке API', () => {
    feedbackServiceSpy.toggleProcessed.and.returnValue(throwError(() => new Error('Server error')));
    const item = { ...mockFeedbacksResponse.items[0] };

    component.toggleProcessed(item);

    expect(item.isProcessed).toBeFalse();
  });

  it('getTypeLabel должен возвращать правильные текстовые метки с эмодзи', () => {
    expect(component.getTypeLabel(FeedbackType.Bug)).toBe('🐛 Баг');
    expect(component.getTypeLabel(FeedbackType.Idea)).toBe('💡 Идея');
    expect(component.getTypeLabel(FeedbackType.Other)).toBe('💬 Другое');
  });
});