import { useQueryClient } from '@tanstack/react-query';
import { useEffect, useState } from 'react';
import { useAuth } from '../../../lib/auth';
import { createConsultationConnection } from '../../../lib/signalr';
import { caseKeys, useConsultations, useMessages, useSendMessage } from '../api';

export function MessagesTab({ caseId }: { caseId: string }) {
  const consultations = useConsultations(caseId);
  const activeConsultation = consultations.data?.find((c) => c.status === 'Active') ?? consultations.data?.[0];
  const messages = useMessages(caseId, activeConsultation?.id);
  const sendMessage = useSendMessage(caseId, activeConsultation?.id);
  const { userId } = useAuth();
  const qc = useQueryClient();
  const [draft, setDraft] = useState('');

  // Canlı akış: SignalR grubu — yeni mesajda listeyi tazele.
  useEffect(() => {
    if (!activeConsultation) return;
    const connection = createConsultationConnection();
    connection.on('messageReceived', () => {
      qc.invalidateQueries({ queryKey: caseKeys.messages(caseId, activeConsultation.id) });
    });
    connection
      .start()
      .then(() => connection.invoke('JoinConsultation', caseId, activeConsultation.id))
      .catch(() => {}); // bağlantı kurulamazsa REST polling devam eder
    return () => {
      connection.stop().catch(() => {});
    };
  }, [caseId, activeConsultation, qc]);

  if (!activeConsultation) {
    return (
      <p className="rounded-xl border border-dashed border-gray-300 bg-white p-6 text-center text-sm text-gray-500">
        Henüz konsültasyon yok — "Doktorlar" sekmesinden bir doktorla konsültasyon başlatabilirsin.
      </p>
    );
  }

  const onSend = () => {
    const content = draft.trim();
    if (!content) return;
    sendMessage.mutate(content, { onSuccess: () => setDraft('') });
  };

  return (
    <div className="flex h-[28rem] flex-col rounded-xl border border-gray-200 bg-white shadow-sm">
      <div className="flex-1 space-y-3 overflow-y-auto p-4">
        {messages.data?.map((m) => {
          const mine = m.senderUserId === userId;
          return (
            <div key={m.id} className={`flex ${mine ? 'justify-end' : 'justify-start'}`}>
              <div className={`max-w-[75%] rounded-2xl px-4 py-2 text-sm ${mine ? 'bg-brand-600 text-white' : 'bg-gray-100 text-gray-800'}`}>
                <p>{m.content}</p>
                <p className={`mt-1 text-[10px] ${mine ? 'text-brand-100' : 'text-gray-400'}`}>
                  {new Date(m.sentAtUtc).toLocaleTimeString('tr-TR', { hour: '2-digit', minute: '2-digit' })}
                </p>
              </div>
            </div>
          );
        })}
        {messages.data?.length === 0 && <p className="text-center text-sm text-gray-400">İlk mesajı sen gönder 👋</p>}
      </div>

      <div className="flex gap-2 border-t border-gray-100 p-3">
        <input
          value={draft}
          onChange={(e) => setDraft(e.target.value)}
          onKeyDown={(e) => e.key === 'Enter' && onSend()}
          placeholder="Mesajını yaz…"
          className="flex-1 rounded-lg border border-gray-300 px-3 py-2 text-sm focus:border-brand-600 focus:outline-none"
        />
        <button
          onClick={onSend}
          disabled={sendMessage.isPending}
          className="rounded-lg bg-brand-600 px-4 py-2 text-sm font-medium text-white hover:bg-brand-700 disabled:opacity-50"
        >
          Gönder
        </button>
      </div>
    </div>
  );
}
