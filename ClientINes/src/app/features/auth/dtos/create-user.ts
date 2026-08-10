export interface AppUser {
  id: string;
  email: string;
  displayName: string;
  roles: string[];
  completedTutorials: number;
}