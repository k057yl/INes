import { ItemCreateDto } from './item-create.dto';

export interface UpdateItemDto extends Partial<ItemCreateDto> {}