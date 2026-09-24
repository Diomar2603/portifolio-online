import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';

export type Resource = 'education' | 'experience' | 'certifications' | 'categories' | 'projects';

@Injectable({ providedIn: 'root' })
export class ApiService {
  private readonly http = inject(HttpClient);
  private readonly base = `${environment.apiUrl}/api/admin`;

  list<T>(resource: Resource): Observable<T[]> {
    return this.http.get<T[]>(`${this.base}/${resource}`);
  }

  get<T>(resource: Resource, id: string): Observable<T> {
    return this.http.get<T>(`${this.base}/${resource}/${id}`);
  }

  create<T>(resource: Resource, body: Partial<T>): Observable<T> {
    return this.http.post<T>(`${this.base}/${resource}`, body);
  }

  update<T>(resource: Resource, id: string, body: Partial<T>): Observable<T> {
    return this.http.put<T>(`${this.base}/${resource}/${id}`, body);
  }

  delete(resource: Resource, id: string): Observable<void> {
    return this.http.delete<void>(`${this.base}/${resource}/${id}`);
  }

  publish(): Observable<void> {
    return this.http.post<void>(`${this.base}/publish`, {});
  }
}
