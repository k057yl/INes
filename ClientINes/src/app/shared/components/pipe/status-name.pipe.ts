import { Pipe, PipeTransform } from '@angular/core';
import { ItemStatus } from '../../../models/enums/item-status.enum';
import { ITEM_STATUS_LABELS } from '../../../models/constants/item-status.constants';

@Pipe({
  name: 'statusName',
  standalone: true
})
export class StatusNamePipe implements PipeTransform {
  transform(value: number | ItemStatus): string {
    const label = ITEM_STATUS_LABELS[value as ItemStatus];
    
    return label ? label.replace('STATUS.', '') : 'ACTIVE';
  }
}