export interface GetItemFilters {
  searchQuery?: string | null;
  categoryId?: string | null;
  storageLocationId?: string | null;
  status?: number | null;
  sortBy?: number | null;
  minPrice?: number | null;
  maxPrice?: number | null;
}