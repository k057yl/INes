import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { FeedbackService } from '../../../../core/services/feedback.service';
import { FeedbackType } from '../../../../core/enums/feedback-type.enum';
import { FeedbackItem } from '../../../../core/contracts/feedback';

@Component({
  selector: 'app-admin-feedback',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './admin-feedback.component.html',
  styleUrls: ['./admin-feedback.component.scss']
})
export class AdminFeedbackComponent implements OnInit {
  feedbacks: FeedbackItem[] = [];
  totalCount = 0;
  page = 1;
  pageSize = 10;
  
  filterProcessed: boolean | null = null;
  filterType: FeedbackType | null = null;
  feedbackTypes = FeedbackType;

  isLoading = false;

  constructor(private feedbackService: FeedbackService) {}

  ngOnInit(): void {
    this.loadFeedbacks();
  }

  loadFeedbacks(): void {
    this.isLoading = true;
    this.feedbackService.getAdminFeedbacks(this.page, this.pageSize, this.filterProcessed, this.filterType)
      .subscribe({
        next: (res) => {
          this.feedbacks = res.items;
          this.totalCount = res.totalCount;
          this.isLoading = false;
        },
        error: () => this.isLoading = false
      });
  }

  toggleProcessed(item: FeedbackItem): void {
    const oldState = item.isProcessed;
    item.isProcessed = !item.isProcessed;

    this.feedbackService.toggleProcessed(item.id).subscribe({
      error: () => item.isProcessed = oldState
    });
  }

  getTypeLabel(type: FeedbackType): string {
    switch (type) {
      case FeedbackType.Bug: return '🐛 Баг';
      case FeedbackType.Idea: return '💡 Идея';
      default: return '💬 Другое';
    }
  }
}