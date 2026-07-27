import { Component, inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { FeedbackService } from '../../../../core/services/feedback.service';
import { FeedbackType } from '../../../../core/enums/feedback-type.enum';
import { TranslateModule } from '@ngx-translate/core';
import { DashboardModalService } from '../../../../features/dashboard/dashboard.modal.service';

@Component({
  selector: 'app-feedback-modal',
  standalone: true,
  imports: [CommonModule, FormsModule, TranslateModule],
  templateUrl: './feedback-modal.component.html',
  styleUrls: ['./feedback-modal.component.scss']
})
export class FeedbackModalComponent implements OnInit {
  private feedbackService = inject(FeedbackService);
  private modalService = inject(DashboardModalService);

  step: 1 | 2 = 1;
  isLoading = false;

  feedbackType: FeedbackType = FeedbackType.Bug;
  message = '';
  feedbackTypes = FeedbackType;
  createdFeedbackId: string | null = null;

  rating = 5;
  missingFeatures = '';

  ngOnInit(): void {
    this.resetForm();
  }

  resetForm(): void {
    this.step = 1;
    this.message = '';
    this.missingFeatures = '';
    this.rating = 5;
    this.feedbackType = FeedbackType.Bug;
    this.createdFeedbackId = null;
  }

  close(): void {
    this.modalService.close();
  }

  submitStep1(): void {
    if (!this.message.trim()) return;

    this.isLoading = true;
    this.feedbackService.sendFeedback({
      type: Number(this.feedbackType),
      message: this.message
    }).subscribe({
      next: (res) => {
        this.createdFeedbackId = res.id;
        this.isLoading = false;
        this.step = 2;
      },
      error: () => this.isLoading = false
    });
  }

  setRating(stars: number): void {
    this.rating = stars;
  }

  submitStep2(): void {
    if (!this.createdFeedbackId) {
      this.close();
      return;
    }

    this.isLoading = true;
    this.feedbackService.rateFeedback(this.createdFeedbackId, {
      rating: this.rating,
      missingFeatures: this.missingFeatures
    }).subscribe({
      next: () => {
        this.isLoading = false;
        this.close();
      },
      error: () => {
        this.isLoading = false;
        this.close();
      }
    });
  }
}