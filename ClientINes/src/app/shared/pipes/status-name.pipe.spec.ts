import { StatusNamePipe } from './status-name.pipe';

describe('StatusNamePipe', () => {
  let pipe: StatusNamePipe;

  beforeEach(() => {
    pipe = new StatusNamePipe();
  });

  it('должен создаваться', () => {
    expect(pipe).toBeTruthy();
  });

  it('должен очищать префикс STATUS.', () => {
    const result = pipe.transform(0);
    expect(result).not.toContain('STATUS.');
  });

  it('должен возвращать ACTIVE для несуществующего статуса', () => {
    expect(pipe.transform(9999)).toBe('ACTIVE');
  });
});