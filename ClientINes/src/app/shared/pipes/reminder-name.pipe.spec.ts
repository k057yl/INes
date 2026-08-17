import { ReminderNamePipe } from './reminder-name.pipe';
import { ReminderType } from '../../features/reminder/enums/reminder-type.enum';

describe('ReminderNamePipe', () => {
  let pipe: ReminderNamePipe;

  beforeEach(() => {
    pipe = new ReminderNamePipe();
  });

  it('должен создаваться', () => {
    expect(pipe).toBeTruthy();
  });

  it('должен возвращать правильные названия по ключам', () => {
    expect(pipe.transform(0)).toBe('WARRANTY');
    expect(pipe.transform(1)).toBe('MAINTENANCE');
    expect(pipe.transform(2)).toBe('RETURN_ITEM');
    expect(pipe.transform(3)).toBe('CUSTOM');
  });

  it('должен возвращать CUSTOM по умолчанию для неизвестного статуса', () => {
    expect(pipe.transform(999)).toBe('CUSTOM');
  });
});