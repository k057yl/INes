export interface LocationCreateDto {
  name: string;
  description?: string;
  parentId?: string | null;
  icon?: string;
  color?: string;
  sortOrder: number;
}