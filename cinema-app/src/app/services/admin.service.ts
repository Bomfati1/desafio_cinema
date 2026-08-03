import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';
import {
  Movie,
  Room,
  Session,
  AdminSeat,
  CreateMovieRequest,
  CreateRoomRequest,
  CreateSessionRequest,
  ReplicateSessionsRequest,
  ReplicateSessionsResult,
  PagedResult,
} from '../models/cinema.models';

@Injectable({ providedIn: 'root' })
export class AdminService {
  private http = inject(HttpClient);
  private baseUrl = `${environment.apiUrl}/admin`;

  // ── Movies ─────────────────────────────────────
  getMovies(): Observable<Movie[]> {
    return this.http.get<Movie[]>(`${this.baseUrl}/movies`);
  }

  createMovie(request: CreateMovieRequest): Observable<Movie> {
    return this.http.post<Movie>(`${this.baseUrl}/movies`, request);
  }

  updateMovie(id: number, request: CreateMovieRequest): Observable<Movie> {
    return this.http.put<Movie>(`${this.baseUrl}/movies/${id}`, request);
  }

  deleteMovie(id: number): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/movies/${id}`);
  }

  restoreMovie(id: number): Observable<void> {
    return this.http.post<void>(`${this.baseUrl}/movies/${id}/restore`, {});
  }

  // ── Rooms ──────────────────────────────────────
  getRooms(): Observable<Room[]> {
    return this.http.get<Room[]>(`${this.baseUrl}/rooms`);
  }

  createRoom(request: CreateRoomRequest): Observable<Room> {
    return this.http.post<Room>(`${this.baseUrl}/rooms`, request);
  }

  deleteRoom(id: number): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/rooms/${id}`);
  }

  restoreRoom(id: number): Observable<void> {
    return this.http.post<void>(`${this.baseUrl}/rooms/${id}/restore`, {});
  }

  // ── Sessions ───────────────────────────────────
  getSessionsAdmin(date?: string, page?: number, pageSize?: number): Observable<PagedResult<Session>> {
    let params = new HttpParams();
    if (date) {
      params = params.set('date', date);
    }
    if (page) {
      params = params.set('page', page.toString());
    }
    if (pageSize) {
      params = params.set('pageSize', pageSize.toString());
    }
    return this.http.get<PagedResult<Session>>(`${this.baseUrl}/sessions`, { params });
  }

  createSession(request: CreateSessionRequest): Observable<Session> {
    return this.http.post<Session>(`${this.baseUrl}/sessions`, request);
  }

  softDeleteSession(id: number): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/sessions/${id}`);
  }

  restoreSession(id: number): Observable<void> {
    return this.http.post<void>(`${this.baseUrl}/sessions/${id}/restore`, {});
  }

  getSessionSeats(sessionId: number): Observable<AdminSeat[]> {
    return this.http.get<AdminSeat[]>(`${this.baseUrl}/sessions/${sessionId}/seats`);
  }

  replicateSessions(request: ReplicateSessionsRequest): Observable<ReplicateSessionsResult> {
    return this.http.post<ReplicateSessionsResult>(`${this.baseUrl}/sessions/replicate`, request);
  }
}
