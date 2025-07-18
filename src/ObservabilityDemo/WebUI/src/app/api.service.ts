import { inject, Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { Config } from "./shared/config";

@Injectable({
  providedIn: 'root',
})
export class ApiService {
  private readonly apiUrl = inject(Config).apiUrl;
  private readonly http = inject(HttpClient);

  public callEndpoint(endpoint: string): Observable<any> {
    return this.http.get(`${this.apiUrl}${endpoint}`, { responseType: 'text' });
  }
}
