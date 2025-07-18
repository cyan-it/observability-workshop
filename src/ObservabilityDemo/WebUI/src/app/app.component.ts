import { Component, OnDestroy, inject } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { NgClass, NgForOf } from '@angular/common';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatExpansionModule } from '@angular/material/expansion';
import { Subject, takeUntil } from 'rxjs';

import { ApiService } from './api.service';

type Status = 'ok' | 'error' | null;

interface Endpoint {
  label: string;
  path: string;
  status: Status;
}

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [
    RouterOutlet, NgClass, NgForOf,
    MatCardModule, MatButtonModule, MatExpansionModule
  ],
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.css']
})
export class AppComponent implements OnDestroy {

  private readonly api = inject(ApiService);
  private readonly destroy$ = new Subject<void>();

  protected readonly title = 'Observability Demo';
  protected responseMessage = '';
  protected responseColor = 'green';

  readonly endpoints: Endpoint[] = [
    { label: 'Call /ok', path: '/ok', status: null },
    { label: 'Call /unauthorized', path: '/unauthorized', status: null },
    { label: 'Call /not-found', path: '/not-found', status: null },
    { label: 'Call /bad-request', path: '/bad-request', status: null },
    { label: 'Call /internal-server-error', path: '/internal-server-error', status: null }
  ];

  protected callService(ep: Endpoint): void {
    this.api.callEndpoint(ep.path)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: data => {
          this.responseMessage = data;
          this.responseColor = 'green';
          ep.status = 'ok';
        },
        error: err => {
          this.responseMessage = `Error: Status Code ${err.status ?? 'Server error'}`;
          this.responseColor = 'red';
          ep.status = 'error';
        }
      });
  }

  public ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }
}
