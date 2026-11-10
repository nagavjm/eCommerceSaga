export interface LoginRequest {
  email: string;
  passwordHash: string;
}

export interface RegisterRequest {
  email: string;
  fullName: string;
  passwordHash: string;
  role: string;
}

export interface AuthResponse {
  token: string;
  refreshToken: string;
  userId: string;
  email: string;
  fullName: string;
  role: string;
  expiresInSeconds: number;
}

export interface User {
  id: string;
  email: string;
  fullName: string;
  role: string;
  token: string;
}
