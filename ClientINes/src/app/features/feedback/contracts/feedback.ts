import { FeedbackType } from '../enums/feedback-type.enum';

export interface FeedbackItem {
  id: string;
  userName: string;
  userEmail: string;
  type: FeedbackType;
  message: string;
  rating?: number;
  missingFeatures?: string;
  createdAt: string;
  isProcessed: boolean;
}

export interface PagedFeedbackResult {
  items: FeedbackItem[];
  totalCount: number;
  page: number;
  pageSize: number;
}