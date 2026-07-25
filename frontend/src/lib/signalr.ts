import { HubConnectionBuilder, type HubConnection } from '@microsoft/signalr';

/** Konsültasyon hub bağlantısı — JWT query token ile (consultation-model.md: canlı akış SignalR). */
export function createConsultationConnection(): HubConnection {
  return new HubConnectionBuilder()
    .withUrl(`${import.meta.env.VITE_HUB_URL}/consultations`, {
      accessTokenFactory: () => localStorage.getItem('token') ?? '',
    })
    .withAutomaticReconnect()
    .build();
}
