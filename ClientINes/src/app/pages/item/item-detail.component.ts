import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router, RouterModule } from '@angular/router';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../../environments/environment';
import { Item } from '../../models/entities/item.entity';
import { TranslateModule } from '@ngx-translate/core';

@Component({
  selector: 'app-item-detail',
  standalone: true,
  imports: [CommonModule, RouterModule, TranslateModule],
  templateUrl: './item-detail.component.html',
  styleUrls: ['./item-detail.component.css']
})
export class ItemDetailComponent implements OnInit {
  private route = inject(ActivatedRoute);
  private router = inject(Router);
  private http = inject(HttpClient);

  item: Item | null = null;
  isLoading = true;

  ngOnInit() {
    const id = this.route.snapshot.paramMap.get('id');
    if (id) {
      this.loadItem(id);
    }
  }

  loadItem(id: string) {
    this.isLoading = true;
    this.http.get<Item>(`${environment.apiBaseUrl}/items/${id}`).subscribe({
      next: (data) => {
        this.item = data;
        this.isLoading = false;
      },
      error: (err) => {
        console.error('Ошибка загрузки предмета:', err);
        this.isLoading = false;
      }
    });
  }

  goBack() {
    this.router.navigate(['/main']);
  }
}