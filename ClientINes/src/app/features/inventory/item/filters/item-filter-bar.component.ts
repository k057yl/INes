import { Component, Output, EventEmitter, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, ReactiveFormsModule } from '@angular/forms';
import { debounceTime, distinctUntilChanged } from 'rxjs';
import { TranslateModule } from '@ngx-translate/core';
import { CategoryService } from '../../../../shared/services/category.service';

@Component({
  selector: 'app-item-filter-bar',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, TranslateModule],
  templateUrl: './item-filter-bar.component.html',
  styleUrl: './item-filter-bar.component.scss'
})
export class ItemFilterBarComponent implements OnInit {
  private fb = inject(FormBuilder);
  private categoryService = inject(CategoryService);

  @Output() filterChanged = new EventEmitter<any>();

  showPanel = false;
  categories$ = this.categoryService.getAll();

  filterForm = this.fb.group({
    searchQuery: [''],
    categoryId: [null],
    status: [null]
  });

  ngOnInit() {
    this.filterForm.valueChanges.pipe(
      debounceTime(400),
      distinctUntilChanged()
    ).subscribe(values => {
      this.filterChanged.emit(values);
    });
  }

  toggle() {
    this.showPanel = !this.showPanel;
  }

  reset() {
    this.filterForm.reset();
  }
}