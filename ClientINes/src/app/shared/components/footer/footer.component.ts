import { Component, ViewChild } from '@angular/core';
import { CommonModule } from '@angular/common';
import { TranslateModule } from '@ngx-translate/core';
import { FeedbackModalComponent } from '../modals/feedback-modal/feedback-modal.component';

@Component({
  selector: 'app-footer',
  standalone: true,
  imports: [CommonModule, TranslateModule, FeedbackModalComponent],
  templateUrl: './footer.component.html',
  styleUrls: ['./footer.component.scss']
})
export class FooterComponent {
  @ViewChild(FeedbackModalComponent) feedbackModal!: FeedbackModalComponent;

  openFeedback(): void {
    this.feedbackModal?.open();
  }
}