import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';
import {
  Session,
  SessionDetail,
  Reservation,
  CreateReservationRequest,
  PagedResult,
} from '../models/cinema.models';

export interface SessionsQuery {
  date?: string;
  page?: number;
  pageSize?: number;
}

@Injectable({ providedIn: 'root' })
export class CinemaService {
  private http = inject(HttpClient);
  private baseUrl = environment.apiUrl;

  // ── Sessions ──────────────────────────────────
  getSessions(query?: SessionsQuery): Observable<PagedResult<Session>> {
    let params = new HttpParams();
    if (query?.date) {
      params = params.set('date', query.date);
    }
    if (query?.page) {
      params = params.set('page', query.page.toString());
    }
    if (query?.pageSize) {
      params = params.set('pageSize', query.pageSize.toString());
    }
    return this.http.get<PagedResult<Session>>(`${this.baseUrl}/sessions`, { params });
  }

  getSessionById(id: number): Observable<SessionDetail> {
    return this.http.get<SessionDetail>(`${this.baseUrl}/sessions/${id}`);
  }

  // ── Reservations ──────────────────────────────
  createReservation(request: CreateReservationRequest): Observable<Reservation> {
    return this.http.post<Reservation>(`${this.baseUrl}/reservations`, request);
  }
}
