import { useMutation } from '@tanstack/react-query';
import { api } from '../../lib/api';
import type { LoginResult, Patient } from '../../lib/types';

export interface LoginDto {
  email: string;
  password: string;
}

export interface RegisterPatientDto {
  fullName: string;
  email: string;
  password: string;
  dateOfBirth?: string | null;
}

export const useLogin = () =>
  useMutation({ mutationFn: (dto: LoginDto) => api.post<LoginResult, LoginResult>('/auth/login', dto) });

export const useRegisterPatient = () =>
  useMutation({ mutationFn: (dto: RegisterPatientDto) => api.post<Patient, Patient>('/patients', dto) });
