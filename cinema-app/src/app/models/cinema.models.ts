// ── Domain Models ──────────────────────────────────

export interface Movie {
  id: number;
  title: string;
  description: string;
  genre: string;
  durationMinutes: number;
  posterUrl: string;
  isDeleted?: boolean;
}

export interface Room {
  id: number;
  name: string;
  rows: number;
  columns: number;
  isDeleted?: boolean;
}

export interface Seat {
  id: number;
  roomId: number;
  row: string;
  number: number;
  label: string; // "A1", "B3", etc.
  isOccupied?: boolean; // computed client-side per session
}

export interface Session {
  id: number;
  movieId: number;
  roomId: number;
  startTime: string; // ISO 8601
  endTime: string;
  ticketPrice: number;
  isDeleted?: boolean; // soft-delete (admin)
  movie?: Movie;
  room?: Room;
  occupiedSeats?: number[]; // array of seat IDs already reserved
}

// ── DTOs ───────────────────────────────────────────

export interface CreateReservationRequest {
  sessionId: number;
  customerName: string;
  seatIds: number[];
}

export interface Reservation {
  id: number;
  sessionId: number;
  customerName: string;
  reservedAt: string;
  tickets: Ticket[];
}

export interface Ticket {
  id: number;
  reservationId: number;
  sessionId: number;
  seatId: number;
  seat?: Seat;
}

export interface SessionDetail extends Session {
  room: Room;
  seats: Seat[];         // all seats in the room with occupancy status
  movie: Movie;
}

// ── Admin Seat (includes reservation data) ───────────

export interface AdminSeat {
  id: number;
  roomId: number;
  row: string;
  number: number;
  label: string;
  isOccupied: boolean;
  customerName?: string;   // null if not occupied
  reservedAt?: string;     // ISO 8601, null if not occupied
}

// ── Auth Models ──────────────────────────────────────

export interface LoginRequest {
  email: string;
  password: string;
}

export interface LoginResponse {
  token: string;
  refreshToken: string;
  email: string;
  name: string;
  role: string;
}

export interface RefreshTokenRequest {
  refreshToken: string;
}

// ── Admin DTOs ───────────────────────────────────────

export interface CreateMovieRequest {
  title: string;
  description: string;
  genre: string;
  durationMinutes: number;
  posterUrl: string;
}

export interface CreateRoomRequest {
  name: string;
  rows: number;
  columns: number;
}

export interface CreateSessionRequest {
  movieId: number;
  roomId: number;
  startTime: string;
  endTime: string;
  ticketPrice: number;
}

export interface ReplicateSessionsRequest {
  sourceDate: string;  // YYYY-MM-DD
  targetDate: string;  // YYYY-MM-DD
}

export interface ReplicateSessionsResult {
  createdCount: number;
  skippedCount: number;
  createdSessions: Session[];
  errors: string[];
}

// ── Error Response (backend) ──────────────────────────

export interface ApiError {
  error: string;
}

// ── Pagination ────────────────────────────────────────

export interface PagedResult<T> {
  items: T[];
  page: number;
  pageSize: number;
  totalCount: number;
  totalPages: number;
  hasPrevious: boolean;
  hasNext: boolean;
}
