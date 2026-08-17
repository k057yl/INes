import { DashboardTreeService } from './dashboard-tree.service';
import { StorageLocation } from '../../location/contracts/storage-location';

describe('DashboardTreeService', () => {
  let service: DashboardTreeService;

  const mockTree: StorageLocation[] = [
    {
      id: 'root-1',
      name: 'Гараж',
      children: [
        {
          id: 'child-1',
          name: 'Полка 1',
          children: [
            { id: 'subchild-1', name: 'Коробка' } as StorageLocation
          ]
        } as StorageLocation
      ]
    } as StorageLocation,
    { id: 'root-2', name: 'Шкаф' } as StorageLocation
  ];

  beforeEach(() => {
    service = new DashboardTreeService();
  });

  it('flattenLocations должен превращать дерево в плоский массив', () => {
    const flat = service.flattenLocations(mockTree);
    expect(flat.length).toBe(4);
    expect(flat.map(l => l.id)).toEqual(['root-1', 'child-1', 'subchild-1', 'root-2']);
  });

  it('excludeLocation должен рекурсивно вырезать локацию из дерева', () => {
    const updatedTree = service.excludeLocation(mockTree, 'child-1');
    expect(updatedTree[0].children?.length).toBe(0);
  });

  it('getSubtreeDepth должен правильно измерять максимальную глубину поддерева', () => {
    expect(service.getSubtreeDepth(mockTree[0])).toBe(3); // root-1 -> child-1 -> subchild-1
    expect(service.getSubtreeDepth(mockTree[1])).toBe(1); // root-2
  });

  it('canMoveLocation должен запрещать перемещение, если общая глубина превышает 3 уровня', () => {
    const flat = service.flattenLocations(mockTree);

    // Попытка засунуть root-1 (глубина 3) внутрь child-1 (уровень 1) -> 1 + 3 = 4 > 3
    const canMove = service.canMoveLocation(flat, 'root-1', 'child-1');
    expect(canMove).toBeFalse();
  });
});