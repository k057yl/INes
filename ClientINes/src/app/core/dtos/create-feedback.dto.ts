import { FeedbackType } from "../enums/feedback-type.enum";

export interface CreateFeedbackDto {
  type: FeedbackType;
  message: string;
}